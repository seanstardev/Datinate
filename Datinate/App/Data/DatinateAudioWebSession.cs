using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using System.Text.Json;

namespace datinate.app
{
    internal enum DatinateAudioWebLoadStatus
    {
        Loaded,
        NotPlayable,
        Unavailable,
        Cancelled
    }

    internal readonly record struct DatinateAudioWebLoadResult(
        DatinateAudioWebLoadStatus Status,
        Uri Source,
        string? Reason = null,
        string? Error = null)
    {
        public bool Loaded => Status == DatinateAudioWebLoadStatus.Loaded;
    }

    /// <summary>
    /// Owns one Datinate Audio Web Facade player session for one existing WebView2.
    /// Audio probing is handled by one shared hidden WebView2 so candidate checks do
    /// not disturb the target MiniWebUI / MediaWebView. The probe WebView/Core is
    /// reused across candidate checks, while local-source mappings may require the
    /// facade document to be reloaded.
    /// </summary>
    internal sealed class DatinateAudioWebSession : IDisposable
    {
        private const bool VERBOSE_DEBUG = false;
        private const string AUDIO_HOST = "datinate-audio.local";
        private static readonly TimeSpan HostReadyTimeout = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(45);
        private static readonly TimeSpan PlayerReadyTimeout = TimeSpan.FromSeconds(60);

        private static readonly SemaphoreSlim SharedProbeGate = new(1, 1);
        private static SharedProbeEnvironment? sharedProbeEnvironment;

        private readonly WebView2 webView;
        private readonly string audioWebRoot;

        private CoreWebView2? core;
        private string? mediaHostName;
        private string? currentSessionId;
        private CancellationTokenSource? operationCts;
        private TaskCompletionSource<PlayerMessage>? playerTcs;
        private bool disposed;
        private bool coreConfigured;

        internal Uri? CurrentSource { get; private set; }
        internal bool IsPlayerActive { get; private set; }

        internal DatinateAudioWebSession(WebView2 webView, string audioWebRoot)
        {
            this.webView = webView ?? throw new ArgumentNullException(nameof(webView));
            this.audioWebRoot = NormaliseAudioWebRoot(audioWebRoot);
        }

        /// <summary>
        /// Uses the one shared headless audio facade to determine whether Datinate's
        /// audio system can handle this complete source. The target WebView2 is not
        /// touched. Probes are deliberately serialised.
        /// </summary>
        internal static async Task<bool> CanPlayAsync(
            Uri source,
            string audioWebRoot,
            CancellationToken cancellationToken = default)
        {
            string root = NormaliseAudioWebRoot(audioWebRoot);

            VerboseDebug($"[AUDIO PROBE] START: {source}");
            VerboseDebug($"[AUDIO PROBE] ROOT: {root}");

            if (VERBOSE_DEBUG)
            {
                string datinateFolder = Path.Combine(root, "datinate");

                VerboseDebug($"[AUDIO PROBE] host.html: {File.Exists(Path.Combine(datinateFolder, "host.html"))}");
                VerboseDebug($"[AUDIO PROBE] player.html: {File.Exists(Path.Combine(datinateFolder, "player.html"))}");
                VerboseDebug($"[AUDIO PROBE] datinate-audio.js: {File.Exists(Path.Combine(datinateFolder, "datinate-audio.js"))}");
            }

            if (!HasRequiredFacadeFiles(root))
            {
                Debug.WriteLine($"[AUDIO PROBE] FAILED: required facade files are missing under '{root}'.");
                return false;
            }

            await SharedProbeGate.WaitAsync(cancellationToken);

            try
            {
                VerboseDebug("[AUDIO PROBE] Getting shared probe environment...");

                SharedProbeEnvironment probe = await GetSharedProbeEnvironmentAsync(root, cancellationToken);

                VerboseDebug("[AUDIO PROBE] Shared probe environment ready.");
                VerboseDebug("[AUDIO PROBE] Probing source...");

                bool result = await probe.CanPlayAsync(source, cancellationToken);

                VerboseDebug($"[AUDIO PROBE] RESULT: {result}");
                return result;
            }
            catch (OperationCanceledException)
            {
                VerboseDebug("[AUDIO PROBE] CANCELLED");
                DisposeSharedProbeEnvironment();
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[AUDIO PROBE] FAILED: {ex.GetType().FullName}\r\n" +
                    $"HRESULT: 0x{ex.HResult:X8}\r\n" +
                    $"MESSAGE: {ex.Message}\r\n" +
                    $"STACK:\r\n{ex.StackTrace}");

                DisposeSharedProbeEnvironment();
                return false;
            }
            finally
            {
                SharedProbeGate.Release();
            }
        }

        /// <summary>
        /// Navigates this session's existing WebView2 directly to player.html.
        /// Call CanPlayAsync first when routing an unknown source.
        /// </summary>
        internal async Task<DatinateAudioWebLoadResult> LoadPlayerAsync(Uri source, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (webView.InvokeRequired)
                return await InvokeOnUiAsync(() => LoadPlayerAsync(source, cancellationToken));

            if (disposed || webView.IsDisposed || !webView.IsHandleCreated)
                return new DatinateAudioWebLoadResult(DatinateAudioWebLoadStatus.Unavailable, source, "webview-unavailable");

            CancellationToken token = BeginOperation(cancellationToken);
            TaskCompletionSource<PlayerMessage>? localPlayerTcs = null;

            try
            {
                if (!HasRequiredFacadeFiles(audioWebRoot))
                    return new DatinateAudioWebLoadResult(DatinateAudioWebLoadStatus.Unavailable, source, "audio-facade-missing");

                CoreWebView2? currentCore = await EnsureCoreAsync();
                if (currentCore == null)
                    return new DatinateAudioWebLoadResult(DatinateAudioWebLoadStatus.Unavailable, source, "webview-core-unavailable");

                await ResetFacadeDocumentAsync(currentCore);
                ClearMediaMapping(currentCore, ref mediaHostName);
                token.ThrowIfCancellationRequested();

                string sessionId = Guid.NewGuid().ToString("N");
                currentSessionId = sessionId;

                string? sourceUrl = PrepareBrowserSource(currentCore, source, sessionId, ref mediaHostName);
                if (string.IsNullOrWhiteSpace(sourceUrl))
                    return new DatinateAudioWebLoadResult(DatinateAudioWebLoadStatus.Unavailable, source, "source-cannot-be-exposed");

                CurrentSource = source;
                IsPlayerActive = false;

                localPlayerTcs = NewTcs<PlayerMessage>();
                playerTcs = localPlayerTcs;

                currentCore.Navigate(BuildPageUrl("player.html", sessionId, sourceUrl));
                PlayerMessage player = await localPlayerTcs.Task.WaitAsync(PlayerReadyTimeout, token);

                if (!player.Ready)
                {
                    await ResetFacadeDocumentAsync(currentCore);
                    ClearMediaMapping(currentCore, ref mediaHostName);
                    CurrentSource = null;
                    return new DatinateAudioWebLoadResult(DatinateAudioWebLoadStatus.NotPlayable, source, player.Reason, player.Error);
                }

                IsPlayerActive = true;
                return new DatinateAudioWebLoadResult(DatinateAudioWebLoadStatus.Loaded, source);
            }
            catch (OperationCanceledException)
            {
                return new DatinateAudioWebLoadResult(DatinateAudioWebLoadStatus.Cancelled, source, "cancelled");
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine(ex);
                await SafeResetAndReleaseAsync();
                return new DatinateAudioWebLoadResult(DatinateAudioWebLoadStatus.Unavailable, source, "audio-player-timeout", ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await SafeResetAndReleaseAsync();
                return new DatinateAudioWebLoadResult(DatinateAudioWebLoadStatus.Unavailable, source, "audio-player-error", ex.Message);
            }
            finally
            {
                if (ReferenceEquals(playerTcs, localPlayerTcs))
                    playerTcs = null;
            }
        }

        /// <summary>
        /// Convenience path for callers that do not need a separate routing probe.
        /// </summary>
        internal async Task<DatinateAudioWebLoadResult> TryLoadPlayerAsync(Uri source, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await CanPlayAsync(source, audioWebRoot, cancellationToken))
                    return new DatinateAudioWebLoadResult(DatinateAudioWebLoadStatus.NotPlayable, source, "not-playable");

                return await LoadPlayerAsync(source, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return new DatinateAudioWebLoadResult(DatinateAudioWebLoadStatus.Cancelled, source, "cancelled");
            }
        }

        /// <summary>
        /// Stops/cleans the current player session but leaves subsequent navigation
        /// to the owning MiniWebUI / MediaWebView.
        /// </summary>
        internal async Task ResetAsync()
        {
            if (webView.InvokeRequired)
            {
                await InvokeOnUiAsync(ResetAsync);
                return;
            }

            CancelOperation();
            CoreWebView2? currentCore = webView.CoreWebView2;
            if (currentCore != null)
            {
                await ResetFacadeDocumentAsync(currentCore);
                ClearMediaMapping(currentCore, ref mediaHostName);
            }

            CurrentSource = null;
            IsPlayerActive = false;
            currentSessionId = null;
        }

        internal void Cancel()
        {
            if (webView.InvokeRequired)
            {
                if (!webView.IsDisposed && webView.IsHandleCreated)
                    webView.BeginInvoke(new Action(Cancel));
                return;
            }

            CancelOperation();
        }

        /// <summary>
        /// Optional application-shutdown cleanup for the shared hidden probe WebView.
        /// </summary>
        internal static void ShutdownSharedProbe()
        {
            if (SharedProbeGate.Wait(0))
            {
                try { DisposeSharedProbeEnvironment(); }
                finally { SharedProbeGate.Release(); }
            }
        }

        private static async Task<SharedProbeEnvironment> GetSharedProbeEnvironmentAsync(
            string audioWebRoot,
            CancellationToken cancellationToken)
        {
            if (sharedProbeEnvironment != null &&
                sharedProbeEnvironment.AudioWebRoot == audioWebRoot)
            {
                return sharedProbeEnvironment;
            }

            DisposeSharedProbeEnvironment();

            if (!Application.MessageLoop)
                throw new InvalidOperationException(
                    "The shared Datinate audio probe must first be created on the WinForms UI thread.");

            var probe = new SharedProbeEnvironment(audioWebRoot);

            try
            {
                await probe.InitialiseAsync(cancellationToken);
                sharedProbeEnvironment = probe;
                return probe;
            }
            catch
            {
                probe.Dispose();
                throw;
            }
        }

        private static void DisposeSharedProbeEnvironment()
        {
            SharedProbeEnvironment? probe = sharedProbeEnvironment;
            sharedProbeEnvironment = null;
            probe?.Dispose();
        }

        private CancellationToken BeginOperation(CancellationToken externalToken)
        {
            CancelOperation();
            operationCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            return operationCts.Token;
        }

        private void CancelOperation()
        {
            try { operationCts?.Cancel(); } catch { }
            operationCts?.Dispose();
            operationCts = null;
            playerTcs?.TrySetCanceled();
        }

        private async Task<CoreWebView2?> EnsureCoreAsync()
        {
            CoreWebView2? ensured = await WebHelper.EnsureCoreAsync(webView);
            if (ensured == null)
                return null;

            if (!ReferenceEquals(core, ensured))
            {
                if (core != null)
                {
                    try { core.WebMessageReceived -= Core_WebMessageReceived; } catch { }
                }

                core = ensured;
                coreConfigured = false;
            }

            if (!coreConfigured)
            {
                core.WebMessageReceived += Core_WebMessageReceived;
                core.SetVirtualHostNameToFolderMapping(AUDIO_HOST, audioWebRoot, CoreWebView2HostResourceAccessKind.DenyCors);
                coreConfigured = true;
            }

            return core;
        }

        private async Task SafeResetAndReleaseAsync()
        {
            try
            {
                CoreWebView2? currentCore = webView.CoreWebView2;
                if (currentCore != null)
                {
                    await ResetFacadeDocumentAsync(currentCore);
                    ClearMediaMapping(currentCore, ref mediaHostName);
                }
            }
            catch { }

            CurrentSource = null;
            IsPlayerActive = false;
        }

        private void Core_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (!TryReadAudioMessage(e, currentSessionId, out JsonDocument? document, out JsonElement root, out string? type))
                return;

            using (document)
            {
                if (type == "console")
                {
                    WriteWebConsoleMessage("[AUDIO JS]", root);
                    return;
                }

                switch (type)
                {
                    case "player-ready":
                        playerTcs?.TrySetResult(new PlayerMessage(true, null, null));
                        break;

                    case "player-unavailable":
                        playerTcs?.TrySetResult(
                            new PlayerMessage(false, GetOptionalString(root, "reason"), GetOptionalString(root, "error")));
                        break;
                }
            }
        }

        private static string NormaliseAudioWebRoot(string audioWebRoot)
        {
            if (string.IsNullOrWhiteSpace(audioWebRoot))
                throw new ArgumentException("An Audio Web root is required.", nameof(audioWebRoot));

            return Path.GetFullPath(audioWebRoot);
        }

        private static bool HasRequiredFacadeFiles(string audioWebRoot)
        {
            if (!Directory.Exists(audioWebRoot))
                return false;

            string datinateFolder = Path.Combine(audioWebRoot, "datinate");
            return File.Exists(Path.Combine(datinateFolder, "host.html")) &&
                File.Exists(Path.Combine(datinateFolder, "player.html")) &&
                File.Exists(Path.Combine(datinateFolder, "datinate-audio.js"));
        }

        private static string? PrepareBrowserSource(CoreWebView2 core, Uri source, string sessionId, ref string? mediaHostName)
        {
            if (source.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                source.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return source.AbsoluteUri;
            }

            if (!source.IsFile)
                return null;

            string path = source.LocalPath;
            string? folder = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                return null;

            string fileName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            string hostName = "media-" + sessionId + ".datinate.local";
            core.SetVirtualHostNameToFolderMapping(hostName, folder, CoreWebView2HostResourceAccessKind.Allow);
            mediaHostName = hostName;
            return "https://" + hostName + "/" + Uri.EscapeDataString(fileName);
        }

        private static void ClearMediaMapping(CoreWebView2 core, ref string? mediaHostName)
        {
            string? hostName = mediaHostName;
            mediaHostName = null;
            if (string.IsNullOrWhiteSpace(hostName))
                return;

            try { core.ClearVirtualHostNameToFolderMapping(hostName); }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        private static string BuildPageUrl(string page, string sessionId, string? sourceUrl = null)
        {
            string url = "https://" + AUDIO_HOST + "/datinate/" + page + "?session=" + Uri.EscapeDataString(sessionId);
            if (!string.IsNullOrWhiteSpace(sourceUrl))
                url += "&source=" + Uri.EscapeDataString(sourceUrl);
            return url;
        }

        private static async Task ResetFacadeDocumentAsync(CoreWebView2 core)
        {
            try { await core.ExecuteScriptAsync("void window.datinateAudio?.reset?.();"); }
            catch { }
        }

        private static bool TryReadAudioMessage(CoreWebView2WebMessageReceivedEventArgs e, string? expectedSessionId,
            out JsonDocument? document, out JsonElement root, out string? type)
        {
            document = null;
            root = default;
            type = null;

            string json;
            try { json = e.TryGetWebMessageAsString(); }
            catch { json = e.WebMessageAsJson; }

            try
            {
                document = JsonDocument.Parse(json);
                root = document.RootElement;

                if (!TryGetString(root, "channel", out string? channel) || channel != "datinate-audio" ||
                    !TryGetString(root, "sessionId", out string? sessionId) || sessionId != expectedSessionId ||
                    !TryGetString(root, "type", out type))
                {
                    document.Dispose();
                    document = null;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                document?.Dispose();
                document = null;
                Debug.WriteLine(ex);
                return false;
            }
        }

        private static TaskCompletionSource<T> NewTcs<T>() => new(TaskCreationOptions.RunContinuationsAsynchronously);

        private static bool TryGetString(JsonElement root, string propertyName, out string? value)
        {
            value = null;
            if (!root.TryGetProperty(propertyName, out JsonElement element) || element.ValueKind != JsonValueKind.String)
                return false;
            value = element.GetString();
            return true;
        }

        private static string? GetOptionalString(JsonElement root, string propertyName) =>
            TryGetString(root, propertyName, out string? value) ? value : null;

        private static void VerboseDebug(string message)
        {
            if (VERBOSE_DEBUG)
#pragma warning disable CS0162 // Unreachable code detected
                Debug.WriteLine(message);
#pragma warning restore CS0162 // Unreachable code detected
        }

        private static void WriteWebConsoleMessage(string prefix, JsonElement root)
        {
            if (!TryGetString(root, "message", out string? message))
                return;

            string level = GetOptionalString(root, "level") ?? "log";
            bool important = level.Equals("warn", StringComparison.OrdinalIgnoreCase) ||
                level.Equals("error", StringComparison.OrdinalIgnoreCase);

            if (important || VERBOSE_DEBUG)
                Debug.WriteLine($"{prefix} [{level.ToUpperInvariant()}] {message}");
        }

        private Task<T> InvokeOnUiAsync<T>(Func<Task<T>> action)
        {
            var tcs = NewTcs<T>();
            if (webView.IsDisposed || !webView.IsHandleCreated)
            {
                tcs.TrySetException(new InvalidOperationException("WebView2 handle is unavailable."));
                return tcs.Task;
            }

            webView.BeginInvoke(new Action(async () =>
            {
                try { tcs.TrySetResult(await action()); }
                catch (Exception ex) { tcs.TrySetException(ex); }
            }));

            return tcs.Task;
        }

        private Task InvokeOnUiAsync(Func<Task> action)
        {
            var tcs = NewTcs<bool>();
            if (webView.IsDisposed || !webView.IsHandleCreated)
            {
                tcs.TrySetException(new InvalidOperationException("WebView2 handle is unavailable."));
                return tcs.Task;
            }

            webView.BeginInvoke(new Action(async () =>
            {
                try { await action(); tcs.TrySetResult(true); }
                catch (Exception ex) { tcs.TrySetException(ex); }
            }));

            return tcs.Task;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            CancelOperation();

            CoreWebView2? currentCore = core ?? webView.CoreWebView2;
            if (currentCore != null)
            {
                try { currentCore.WebMessageReceived -= Core_WebMessageReceived; } catch { }
                ClearMediaMapping(currentCore, ref mediaHostName);

                if (coreConfigured)
                {
                    try { currentCore.ClearVirtualHostNameToFolderMapping(AUDIO_HOST); } catch { }
                }
            }

            core = null;
            coreConfigured = false;
            currentSessionId = null;
            CurrentSource = null;
            IsPlayerActive = false;
        }

        private readonly record struct ProbeMessage(bool CanPlay, string? Reason, string? Error);
        private readonly record struct PlayerMessage(bool Ready, string? Reason, string? Error);

        /// <summary>
        /// One hidden WebView2, kept alive for Datinate's lifetime and reused for all
        /// audio capability probes. It is created lazily on the WinForms UI thread.
        /// </summary>
        private sealed class SharedProbeEnvironment : IDisposable
        {
            private readonly ProbeHostForm hostForm;
            private readonly WebView2 webView;
            private CoreWebView2? core;
            private string? mediaHostName;
            private readonly string sessionId = Guid.NewGuid().ToString("N");
            private TaskCompletionSource<bool>? facadeReadyTcs;
            private TaskCompletionSource<ProbeMessage>? probeTcs;
            private bool disposed;

            internal string AudioWebRoot { get; }

            internal SharedProbeEnvironment(string audioWebRoot)
            {
                AudioWebRoot = audioWebRoot;

                hostForm = new ProbeHostForm
                {
                    FormBorderStyle = FormBorderStyle.None,
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.Manual,
                    Location = new Point(-32000, -32000),
                    ClientSize = new Size(2, 2),
                    Opacity = 0.01
                };

                webView = new WebView2
                {
                    Dock = DockStyle.Fill,
                    Margin = Padding.Empty,
                    AllowExternalDrop = false,
                    DefaultBackgroundColor = Color.Black
                };

                WebHelper.ApplyCreationProperties(webView);
                hostForm.Controls.Add(webView);
                hostForm.Show();
            }

            internal async Task InitialiseAsync(CancellationToken cancellationToken)
            {
                if (hostForm.InvokeRequired)
                {
                    VerboseDebug("[AUDIO PROBE] InitialiseAsync marshaling to probe UI...");
                    await InvokeOnUiAsync(() => InitialiseAsync(cancellationToken));
                    return;
                }

                VerboseDebug(
                    $"[AUDIO PROBE] InitialiseAsync UI thread={Environment.CurrentManagedThreadId}, " +
                    $"Handle={hostForm.IsHandleCreated}, WebViewHandle={webView.IsHandleCreated}");

                VerboseDebug("[AUDIO PROBE] Ensuring hidden WebView2 core...");

                CoreWebView2 currentCore;

                try
                {
                    currentCore = await WebHelper.EnsureCoreAsync(webView);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"[AUDIO PROBE] EnsureCore FAILED: {ex.GetType().FullName}, " +
                        $"HRESULT=0x{ex.HResult:X8}, {ex.Message}");

                    throw;
                }

                VerboseDebug("[AUDIO PROBE] Hidden WebView2 core ready.");

                core = currentCore;
                core.Settings.AreDevToolsEnabled = false;
                core.Settings.AreDefaultContextMenusEnabled = false;
                core.WebMessageReceived += Core_WebMessageReceived;

                VerboseDebug($"[AUDIO PROBE] Mapping facade root: {AudioWebRoot}");

                try
                {
                    core.SetVirtualHostNameToFolderMapping(
                        AUDIO_HOST,
                        AudioWebRoot,
                        CoreWebView2HostResourceAccessKind.DenyCors);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"[AUDIO PROBE] AUDIO_HOST mapping FAILED: {ex.GetType().FullName}, " +
                        $"HRESULT=0x{ex.HResult:X8}, {ex.Message}");

                    throw;
                }

                VerboseDebug("[AUDIO PROBE] Facade root mapped.");
                VerboseDebug("[AUDIO PROBE] Loading host.html...");

                await ReloadFacadeAsync(core, cancellationToken);

                VerboseDebug("[AUDIO PROBE] host.html ready.");
            }

            internal async Task<bool> CanPlayAsync(Uri source, CancellationToken cancellationToken)
            {
                if (hostForm.InvokeRequired)
                    return await InvokeOnUiAsync(() => CanPlayAsync(source, cancellationToken));

                if (disposed || core == null)
                    return false;

                CoreWebView2 currentCore = core;
                TaskCompletionSource<ProbeMessage>? localProbeTcs = null;

                try
                {
                    await ResetFacadeDocumentAsync(currentCore);
                    ClearMediaMapping(currentCore, ref mediaHostName);
                    cancellationToken.ThrowIfCancellationRequested();

                    VerboseDebug($"[AUDIO PROBE] Mapping source: {source}");

                    string mappingId = Guid.NewGuid().ToString("N");
                    string? sourceUrl = PrepareBrowserSource(currentCore, source, mappingId, ref mediaHostName);

                    if (string.IsNullOrWhiteSpace(sourceUrl))
                        return false;

                    VerboseDebug($"[AUDIO PROBE] Browser source: {sourceUrl}");

                    if (source.IsFile)
                        await ReloadFacadeAsync(currentCore, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    localProbeTcs = NewTcs<ProbeMessage>();
                    probeTcs = localProbeTcs;

                    string jsSource = JsonSerializer.Serialize(sourceUrl);
                    await currentCore.ExecuteScriptAsync($"void window.datinateAudio.probeAndPost({jsSource});");

                    ProbeMessage result = await localProbeTcs.Task.WaitAsync(ProbeTimeout, cancellationToken);

                    if (!result.CanPlay &&
                        (!string.IsNullOrWhiteSpace(result.Reason) || !string.IsNullOrWhiteSpace(result.Error)))
                    {
                        Debug.WriteLine(
                            $"[AUDIO PROBE] FAILED: {source}; reason={result.Reason}; error={result.Error}");
                    }
                    else
                    {
                        VerboseDebug($"[AUDIO PROBE] {source} => {result.CanPlay}");
                    }

                    return result.CanPlay;
                }
                finally
                {
                    if (ReferenceEquals(probeTcs, localProbeTcs))
                        probeTcs = null;

                    await ResetFacadeDocumentAsync(currentCore);
                    ClearMediaMapping(currentCore, ref mediaHostName);
                }
            }

            private void Core_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
            {
                if (!TryReadAudioMessage(e, sessionId, out JsonDocument? document, out JsonElement root, out string? type))
                    return;

                using (document)
                {
                    if (type == "console")
                    {
                        WriteWebConsoleMessage("[AUDIO PROBE JS]", root);
                        return;
                    }

                    switch (type)
                    {
                        case "facade-ready":
                            facadeReadyTcs?.TrySetResult(true);
                            break;

                        case "probe-result":
                            probeTcs?.TrySetResult(new ProbeMessage(
                                root.TryGetProperty("canPlay", out JsonElement canPlay) &&
                                    canPlay.ValueKind == JsonValueKind.True,
                                GetOptionalString(root, "reason"),
                                GetOptionalString(root, "error")));
                            break;
                    }
                }
            }

            private Task<T> InvokeOnUiAsync<T>(Func<Task<T>> action)
            {
                var tcs = NewTcs<T>();
                if (hostForm.IsDisposed || !hostForm.IsHandleCreated)
                {
                    tcs.TrySetException(new InvalidOperationException("Shared audio probe UI is unavailable."));
                    return tcs.Task;
                }

                hostForm.BeginInvoke(new Action(async () =>
                {
                    try { tcs.TrySetResult(await action()); }
                    catch (Exception ex) { tcs.TrySetException(ex); }
                }));

                return tcs.Task;
            }

            private Task InvokeOnUiAsync(Func<Task> action)
            {
                var tcs = NewTcs<bool>();
                if (hostForm.IsDisposed || !hostForm.IsHandleCreated)
                {
                    tcs.TrySetException(new InvalidOperationException("Shared audio probe UI is unavailable."));
                    return tcs.Task;
                }

                hostForm.BeginInvoke(new Action(async () =>
                {
                    try { await action(); tcs.TrySetResult(true); }
                    catch (Exception ex) { tcs.TrySetException(ex); }
                }));

                return tcs.Task;
            }

            private async Task ReloadFacadeAsync(CoreWebView2 currentCore, CancellationToken cancellationToken)
            {
                var localTcs = NewTcs<bool>();
                facadeReadyTcs = localTcs;

                try
                {
                    currentCore.Navigate(BuildPageUrl("host.html", sessionId));
                    await localTcs.Task.WaitAsync(HostReadyTimeout, cancellationToken);
                }
                finally
                {
                    if (ReferenceEquals(facadeReadyTcs, localTcs))
                        facadeReadyTcs = null;
                }
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                facadeReadyTcs?.TrySetCanceled();
                probeTcs?.TrySetCanceled();

                void DisposeOnUi()
                {
                    if (core != null)
                    {
                        try { core.WebMessageReceived -= Core_WebMessageReceived; } catch { }
                        ClearMediaMapping(core, ref mediaHostName);
                        try { core.ClearVirtualHostNameToFolderMapping(AUDIO_HOST); } catch { }
                    }

                    core = null;
                    try { webView.Dispose(); } catch { }
                    try { hostForm.Close(); } catch { }
                    try { hostForm.Dispose(); } catch { }
                }

                if (hostForm.IsDisposed)
                    return;

                if (hostForm.InvokeRequired && hostForm.IsHandleCreated)
                {
                    try { hostForm.BeginInvoke(new Action(DisposeOnUi)); } catch { }
                    return;
                }

                DisposeOnUi();
            }
        }

        private sealed class ProbeHostForm : Form
        {
            protected override bool ShowWithoutActivation => true;
        }
    }
}
