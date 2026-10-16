using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Datinate.App.Shared.Web.Audio
{
    /// <summary>
    /// Shared player and capability-probe behaviour for a host-provided browser.
    /// The host supplies native UI dispatch; shared entry points use it consistently.
    /// The visible player borrows its browser; the dedicated hidden probe owns its
    /// browser. The host creates the probe; DatinateAudioWebProbe owns its reuse,
    /// serialisation and recovery policy.
    /// </summary>
    public abstract class DatinateAudioWebSessionBase : IDisposable
    {
        private const bool VERBOSE_DEBUG = false;

        private const int HostReadyTimeoutSeconds = 15;
        private const int ProbeTimeoutSeconds = 45;
        private const int PlayerReadyTimeoutSeconds = 60;
        private const int ResetTimeoutSeconds = 15;

        protected const string AudioHostName = "datinate-audio.local";
        private const string MediaHostPrefix = "media-";
        private const string MediaHostSuffix = ".datinate.local";

        protected const string AudioFolderName = "datinate";
        private const string HostPageFileName = "host.html";
        private const string PlayerPageFileName = "player.html";
        private const string AudioScriptFileName = "datinate-audio.js";

        private const string AudioMessageChannel = "datinate-audio";
        private const string ConsoleMessageType = "console";
        private const string FacadeReadyMessageType = "facade-ready";
        private const string ProbeResultMessageType = "probe-result";
        private const string PlayerReadyMessageType = "player-ready";
        private const string PlayerUnavailableMessageType = "player-unavailable";
        private const string ResetCompleteMessageType = "reset-complete";
        private const string ResetFailedMessageType = "reset-failed";

        private const string ChannelPropertyName = "channel";
        private const string SessionIdPropertyName = "sessionId";
        private const string RequestIdPropertyName = "requestId";
        private const string TypePropertyName = "type";
        private const string CanPlayPropertyName = "canPlay";
        private const string ReasonPropertyName = "reason";
        private const string ErrorPropertyName = "error";
        private const string MessagePropertyName = "message";
        private const string LevelPropertyName = "level";

        private const string SessionQueryPrefix = "?session=";
        private const string SourceQueryPrefix = "&source=";
        private const string PreviewQuery = "&preview=1";

        private const string ProbeJavascriptFunction = "window.datinateAudio.probeAndPost";
        private const string ResetJavascriptFunction = "window.datinateAudio.resetAndPost";

        // Shared URL conventions are defaults; each browser supplies its actual accessible URLs.
        protected static readonly Uri AudioDocumentRoot =
            new($"https://{AudioHostName}/{AudioFolderName}/");

        private readonly string audioWebRoot;

        private Uri? documentRoot;
        private string? currentSessionId;
        private CancellationTokenSource? operationCts;
        private TaskCompletionSource<bool>? facadeReadyTcs;
        private TaskCompletionSource<ProbeMessage>? probeTcs;
        private TaskCompletionSource<PlayerMessage>? playerTcs;
        private TaskCompletionSource<bool>? resetTcs;
        private string? resetRequestId;
        private Task<bool>? resetTask;
        private bool disposed;
        private bool isProbeSession;

        internal string AudioWebRoot => audioWebRoot;

        public Uri? CurrentSource { get; private set; }
        public bool IsPlayerActive { get; private set; }

        protected DatinateAudioWebSessionBase(string audioWebRoot)
            => this.audioWebRoot = NormaliseAudioWebRoot(audioWebRoot);

        protected static string CreateMediaHostName(string mappingId)
            => MediaHostPrefix + mappingId + MediaHostSuffix;

        protected static Uri CreateMediaSourceUri(string hostName, string fileName)
            => new($"https://{hostName}/{Uri.EscapeDataString(fileName)}");

        protected abstract bool IsBrowserAvailable { get; }

        protected abstract bool IsOnBrowserThread { get; }

        // Post without blocking. Throw if the native dispatcher cannot accept the action.
        protected abstract void PostToBrowserThread(Action action);

        protected abstract DatinateAudioWebProbe AudioProbe { get; }

        /// <summary>
        /// Initialises the browser, connects message forwarding and exposes the audio
        /// environment. Returns the absolute URL of its datinate/ folder, ending in '/'.
        /// Repeated calls must be safe. Return null if the browser cannot initialise.
        /// </summary>
        protected abstract Task<Uri?> InitialiseBrowserAsync(string audioWebRoot);

        /// <summary>
        /// Exposes a local file using a mapping specific to mappingId, and returns its
        /// browser-accessible URL. Return null if it cannot be exposed.
        /// </summary>
        protected abstract Uri? ExposeLocalFile(string filePath, string mappingId);

        // Must be safe before the first mapping and after a previous release.
        protected abstract void ReleaseLocalFile();

        protected abstract void Navigate(Uri uri);

        // Returns the script's JSON-encoded result. Promise completion is acknowledged
        // separately through browser messages, rather than assumed from this return value.
        protected abstract Task<string> ExecuteScriptAsync(string javascript);

        /// <summary>
        /// Disconnects messages and removes the audio-environment mapping. Dispose
        /// the browser/window only if this session owns them, as the hidden probe does.
        /// </summary>
        protected abstract void ReleaseBrowserResources();

        public virtual Task<DatinateAudioWebLoadResult> LoadPlayerAsync(
            Uri source,
            CancellationToken cancellationToken = default)
        {
            return RunOnBrowserThreadAsync(
                () => LoadPlayerPageAsync(source, preview: false, cancellationToken));
        }

        public virtual Task<DatinateAudioWebLoadResult> LoadPlayerPreviewAsync(
            Uri source,
            CancellationToken cancellationToken = default)
        {
            return RunOnBrowserThreadAsync(
                () => LoadPlayerPageAsync(source, preview: true, cancellationToken));
        }

        /// <summary>
        /// Convenience path for callers that do not need a separate routing probe.
        /// </summary>
        public Task<DatinateAudioWebLoadResult> TryLoadPlayerAsync(
            Uri source,
            CancellationToken cancellationToken = default)
        {
            return RunOnBrowserThreadAsync(async () =>
            {
                try
                {
                    if (!await AudioProbe.CanPlayAsync(source, audioWebRoot, cancellationToken))
                        return new DatinateAudioWebLoadResult(
                            DatinateAudioWebLoadStatus.NotPlayable,
                            source,
                            "not-playable");

                    return await LoadPlayerPageAsync(source, preview: false, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return new DatinateAudioWebLoadResult(
                        DatinateAudioWebLoadStatus.Cancelled,
                        source,
                        "cancelled");
                }
            });
        }

        private async Task<DatinateAudioWebLoadResult> LoadPlayerPageAsync(
            Uri source,
            bool preview,
            CancellationToken cancellationToken)
        {
            if (disposed || !IsBrowserAvailable)
                return new DatinateAudioWebLoadResult(
                    DatinateAudioWebLoadStatus.Unavailable,
                    source,
                    "webview-unavailable");

            CancellationToken token = BeginOperation(cancellationToken);
            TaskCompletionSource<PlayerMessage>? localPlayerTcs = null;

            try
            {
                if (!HasRequiredFacadeFiles(audioWebRoot))
                    return new DatinateAudioWebLoadResult(
                        DatinateAudioWebLoadStatus.Unavailable,
                        source,
                        "audio-facade-missing");

                documentRoot = await InitialiseBrowserAsync(audioWebRoot);

                if (documentRoot == null)
                    return new DatinateAudioWebLoadResult(
                        DatinateAudioWebLoadStatus.Unavailable,
                        source,
                        "webview-core-unavailable");

                bool resetComplete = await ResetFacadeDocumentAsync();

                token.ThrowIfCancellationRequested();

                if (!resetComplete)
                    return new DatinateAudioWebLoadResult(
                        DatinateAudioWebLoadStatus.Unavailable,
                        source,
                        "audio-reset-failed");

                ClearMediaMapping();

                currentSessionId = Guid.NewGuid().ToString("N");
                Uri? sourceUrl = PrepareBrowserSource(source, currentSessionId);

                if (sourceUrl == null)
                    return new DatinateAudioWebLoadResult(
                        DatinateAudioWebLoadStatus.Unavailable,
                        source,
                        "source-cannot-be-exposed");

                CurrentSource = source;
                IsPlayerActive = false;

                localPlayerTcs = NewTcs<PlayerMessage>();
                playerTcs = localPlayerTcs;

                Navigate(BuildPageUri(PlayerPageFileName, sourceUrl, preview));

                PlayerMessage player = await WaitAsync(
                    localPlayerTcs.Task,
                    TimeSpan.FromSeconds(PlayerReadyTimeoutSeconds),
                    token);

                if (!player.Ready)
                {
                    await ResetFacadeDocumentAsync();

                    token.ThrowIfCancellationRequested();

                    ClearMediaMapping();
                    CurrentSource = null;

                    return new DatinateAudioWebLoadResult(
                        DatinateAudioWebLoadStatus.NotPlayable,
                        source,
                        player.Reason,
                        player.Error);
                }

                IsPlayerActive = true;

                return new DatinateAudioWebLoadResult(
                    DatinateAudioWebLoadStatus.Loaded,
                    source);
            }
            catch (OperationCanceledException)
            {
                return new DatinateAudioWebLoadResult(
                    DatinateAudioWebLoadStatus.Cancelled,
                    source,
                    "cancelled");
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine(ex);
                await SafeResetAndReleaseAsync(token);

                return new DatinateAudioWebLoadResult(
                    DatinateAudioWebLoadStatus.Unavailable,
                    source,
                    "audio-player-timeout",
                    ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await SafeResetAndReleaseAsync(token);

                return new DatinateAudioWebLoadResult(
                    DatinateAudioWebLoadStatus.Unavailable,
                    source,
                    "audio-player-error",
                    ex.Message);
            }
            finally
            {
                if (ReferenceEquals(playerTcs, localPlayerTcs))
                    playerTcs = null;
            }
        }

        /// <summary>
        /// Initialises a dedicated hidden probe before the host publishes it for reuse.
        /// The host must serialise probes so candidate checks do not disturb a visible
        /// MiniWebUI / MediaWebView. Local-source mappings may require reloading the
        /// facade document, while the underlying probe browser remains alive.
        /// </summary>
        protected internal Task InitialiseProbeAsync(CancellationToken cancellationToken = default)
        {
            return RunOnBrowserThreadAsync(
                () => InitialiseProbeOnBrowserThreadAsync(cancellationToken));
        }

        private async Task InitialiseProbeOnBrowserThreadAsync(CancellationToken cancellationToken)
        {
            if (disposed || !IsBrowserAvailable)
                throw new InvalidOperationException("Shared audio probe browser is unavailable.");

            if (!HasRequiredFacadeFiles(audioWebRoot))
                throw new InvalidOperationException(
                    $"Required audio facade files are missing under '{audioWebRoot}'.");

            isProbeSession = true;
            documentRoot = await InitialiseBrowserAsync(audioWebRoot);

            if (documentRoot == null)
                throw new InvalidOperationException("Shared audio probe browser could not initialise.");

            currentSessionId = Guid.NewGuid().ToString("N");

            await ReloadFacadeAsync(cancellationToken);
        }

        /// <summary>
        /// Probes through an already initialised, dedicated hidden browser. The host's
        /// shared coordinator owns serialisation and recreates the probe after an
        /// exception or cancellation, preserving the current recovery policy.
        /// </summary>
        protected internal Task<bool> ProbeAsync(Uri source, CancellationToken cancellationToken = default)
        {
            return RunOnBrowserThreadAsync(
                () => ProbeOnBrowserThreadAsync(source, cancellationToken));
        }

        private async Task<bool> ProbeOnBrowserThreadAsync(Uri source, CancellationToken cancellationToken)
        {
            if (disposed || !IsBrowserAvailable || documentRoot == null)
                return false;

            TaskCompletionSource<ProbeMessage>? localProbeTcs = null;
            bool resetComplete = false;

            try
            {
                resetComplete = await ResetFacadeDocumentAsync();

                cancellationToken.ThrowIfCancellationRequested();

                if (!resetComplete)
                    throw new InvalidOperationException("The audio probe reset did not complete.");

                ClearMediaMapping();

                string mappingId = Guid.NewGuid().ToString("N");
                Uri? sourceUrl = PrepareBrowserSource(source, mappingId);

                if (sourceUrl == null)
                    return false;

                // Local-source mappings may require the facade document to reload.
                if (source.IsFile)
                    await ReloadFacadeAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                localProbeTcs = NewTcs<ProbeMessage>();
                probeTcs = localProbeTcs;

                string jsSource = JsonSerializer.Serialize(sourceUrl.AbsoluteUri);

                await ExecuteScriptAsync(
                    $"void {ProbeJavascriptFunction}({jsSource});");

                ProbeMessage result = await WaitAsync(
                    localProbeTcs.Task,
                    TimeSpan.FromSeconds(ProbeTimeoutSeconds),
                    cancellationToken);

                if (!result.CanPlay &&
                    (!string.IsNullOrWhiteSpace(result.Reason) || !string.IsNullOrWhiteSpace(result.Error)))
                {
                    Debug.WriteLine(
                        $"[AUDIO PROBE] FAILED: {source}; reason={result.Reason}; error={result.Error}");
                }

                return result.CanPlay;
            }
            finally
            {
                if (ReferenceEquals(probeTcs, localProbeTcs))
                    probeTcs = null;

                if (resetComplete)
                    await ResetFacadeDocumentAsync();

                ClearMediaMapping();
            }
        }

        private async Task ReloadFacadeAsync(CancellationToken cancellationToken)
        {
            var localTcs = NewTcs<bool>();
            facadeReadyTcs = localTcs;

            try
            {
                Navigate(BuildPageUri(HostPageFileName));

                await WaitAsync(localTcs.Task, TimeSpan.FromSeconds(HostReadyTimeoutSeconds), cancellationToken);
            }
            finally
            {
                if (ReferenceEquals(facadeReadyTcs, localTcs))
                    facadeReadyTcs = null;
            }
        }

        /// <summary>
        /// Forwards the browser's JSON message text, without any native event arguments.
        /// Messages from another channel or an older session are ignored.
        /// </summary>
        protected void ReceiveBrowserMessage(string json)
        {
            if (disposed)
                return;

            try
            {
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    JsonElement root = document.RootElement;

                    if (root.ValueKind != JsonValueKind.Object ||
                        GetOptionalString(root, ChannelPropertyName) != AudioMessageChannel)
                    {
                        return;
                    }

                    string? type = GetOptionalString(root, TypePropertyName);

                    // Reset can target an older document before this C# session has
                    // loaded a page. Correlate its acknowledgement by request, not session.
                    if (type == ResetCompleteMessageType || type == ResetFailedMessageType)
                    {
                        if (resetRequestId != null &&
                            GetOptionalString(root, RequestIdPropertyName) == resetRequestId)
                        {
                            if (type == ResetFailedMessageType)
                            {
                                Debug.WriteLine(
                                    $"[AUDIO RESET] FAILED: {GetOptionalString(root, ErrorPropertyName)}");
                            }

                            resetTcs?.TrySetResult(type == ResetCompleteMessageType);
                        }

                        return;
                    }

                    if (currentSessionId == null ||
                        GetOptionalString(root, SessionIdPropertyName) != currentSessionId)
                    {
                        return;
                    }

                    switch (type)
                    {
                        case ConsoleMessageType:
                            WriteWebConsoleMessage(root);
                            break;

                        case FacadeReadyMessageType:
                            facadeReadyTcs?.TrySetResult(true);
                            break;

                        case ProbeResultMessageType:
                            probeTcs?.TrySetResult(new ProbeMessage(
                                root.TryGetProperty(CanPlayPropertyName, out JsonElement canPlay) &&
                                    canPlay.ValueKind == JsonValueKind.True,
                                GetOptionalString(root, ReasonPropertyName),
                                GetOptionalString(root, ErrorPropertyName)));
                            break;

                        case PlayerReadyMessageType:
                            playerTcs?.TrySetResult(new PlayerMessage(true, null, null));
                            break;

                        case PlayerUnavailableMessageType:
                            playerTcs?.TrySetResult(new PlayerMessage(
                                false,
                                GetOptionalString(root, ReasonPropertyName),
                                GetOptionalString(root, ErrorPropertyName)));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Stops/cleans the current player session but leaves subsequent navigation
        /// to the owning MiniWebUI / MediaWebView.
        /// </summary>
        public virtual Task ResetAsync()
        {
            return RunOnBrowserThreadAsync(ResetOnBrowserThreadAsync);
        }

        private async Task ResetOnBrowserThreadAsync()
        {
            CancelOperation();
            string? sessionId = currentSessionId;

            if (IsBrowserAvailable && documentRoot != null)
            {
                await ResetFacadeDocumentAsync();

                // A newer load may have resumed while this reset was awaiting its acknowledgement.
                if (currentSessionId != sessionId)
                    return;

                ClearMediaMapping();
            }

            CurrentSource = null;
            IsPlayerActive = false;
            currentSessionId = null;
        }

        // Cancels player loading. Dedicated probes use their supplied cancellation token.
        public virtual void Cancel()
        {
            RunOnBrowserThread(CancelOperation);
        }

        private void CancelOperation()
        {
            try { operationCts?.Cancel(); } catch { }

            operationCts?.Dispose();
            operationCts = null;
            playerTcs?.TrySetCanceled();
        }

        private CancellationToken BeginOperation(CancellationToken externalToken)
        {
            CancelOperation();

            operationCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

            return operationCts.Token;
        }

        private async Task SafeResetAndReleaseAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            try
            {
                if (IsBrowserAvailable && documentRoot != null)
                {
                    await ResetFacadeDocumentAsync();

                    if (token.IsCancellationRequested)
                        return;

                    ClearMediaMapping();
                }
            }
            catch { }

            CurrentSource = null;
            IsPlayerActive = false;
        }

        private Task<bool> ResetFacadeDocumentAsync()
        {
            // Share an in-flight reset instead of starting competing JavaScript resets.
            if (resetTask == null || resetTask.IsCompleted)
                resetTask = ResetFacadeDocumentCoreAsync();

            return resetTask;
        }

        private async Task<bool> ResetFacadeDocumentCoreAsync()
        {
            if (disposed)
                return false;

            string requestId = Guid.NewGuid().ToString("N");
            var localTcs = NewTcs<bool>();

            resetRequestId = requestId;
            resetTcs = localTcs;

            try
            {
                return await WaitAsync(
                    ExecuteResetAndWaitAsync(requestId, localTcs.Task),
                    TimeSpan.FromSeconds(ResetTimeoutSeconds),
                    CancellationToken.None);
            }
            catch (OperationCanceledException) when (disposed)
            {
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUDIO RESET] FAILED: request={requestId}; {ex}");

                return false;
            }
            finally
            {
                // Release the inner wait after timeout too; late acknowledgements are ignored.
                localTcs.TrySetCanceled();

                if (ReferenceEquals(resetTcs, localTcs))
                {
                    resetTcs = null;
                    resetRequestId = null;
                }
            }
        }

        private async Task<bool> ExecuteResetAndWaitAsync(string requestId, Task<bool> completion)
        {
            Task<string> invocation = ExecuteScriptAsync(CreateResetJavascript(requestId));

            // Acknowledgement or disposal may arrive before the native script call returns.
            if (await Task.WhenAny(invocation, completion) == completion)
                return await completion;

            string result = await invocation;

            // Blank pages and ordinary websites have no Datinate audio state to reset.
            if (result == "false")
                return true;

            if (result != "true")
                throw new InvalidOperationException("The audio reset script did not start correctly.");

            return await completion;
        }

        private static string CreateResetJavascript(string requestId)
            => $@"(function() {{
    if (typeof window.datinateAudio?.reset !== 'function')
        return false;

    void {ResetJavascriptFunction}({JsonSerializer.Serialize(requestId)});
    return true;
}})();";

        private Uri? PrepareBrowserSource(Uri source, string mappingId)
        {
            if (source.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                source.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return source;
            }

            if (!source.IsFile)
                return null;

            string path = source.LocalPath;
            string? folder = Path.GetDirectoryName(path);

            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                return null;

            if (string.IsNullOrWhiteSpace(Path.GetFileName(path)))
                return null;

            return ExposeLocalFile(path, mappingId);
        }

        private void ClearMediaMapping()
        {
            try { ReleaseLocalFile(); }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        private Uri BuildPageUri(string page, Uri? sourceUrl = null, bool preview = false)
        {
            if (documentRoot == null || currentSessionId == null)
                throw new InvalidOperationException("The audio browser session has not initialised.");

            string url = new Uri(documentRoot, page).AbsoluteUri +
                SessionQueryPrefix + Uri.EscapeDataString(currentSessionId);

            if (sourceUrl != null)
                url += SourceQueryPrefix + Uri.EscapeDataString(sourceUrl.AbsoluteUri);

            if (preview)
                url += PreviewQuery;

            return new Uri(url);
        }

        internal static string NormaliseAudioWebRoot(string audioWebRoot)
        {
            if (string.IsNullOrWhiteSpace(audioWebRoot))
                throw new ArgumentException("An Audio Web root is required.", nameof(audioWebRoot));

            return Path.GetFullPath(audioWebRoot);
        }

        internal static bool HasRequiredFacadeFiles(string audioWebRoot)
        {
            if (!Directory.Exists(audioWebRoot))
                return false;

            string folder = Path.Combine(audioWebRoot, AudioFolderName);

            return File.Exists(Path.Combine(folder, HostPageFileName)) &&
                File.Exists(Path.Combine(folder, PlayerPageFileName)) &&
                File.Exists(Path.Combine(folder, AudioScriptFileName));
        }

        private static string? GetOptionalString(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out JsonElement element) ||
                element.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            return element.GetString();
        }

        private void WriteWebConsoleMessage(JsonElement root)
        {
            string? message = GetOptionalString(root, MessagePropertyName);

            if (message == null)
                return;

            string level = GetOptionalString(root, LevelPropertyName) ?? "log";
            bool important = level.Equals("warn", StringComparison.OrdinalIgnoreCase) ||
                level.Equals("error", StringComparison.OrdinalIgnoreCase);

            if (important || VERBOSE_DEBUG)
            {
                string prefix = isProbeSession
                    ? "[AUDIO PROBE JS]"
                    : "[AUDIO JS]";

                Debug.WriteLine($"{prefix} [{level.ToUpperInvariant()}] {message}");
            }
        }

        private Task<T> RunOnBrowserThreadAsync<T>(Func<Task<T>> action)
        {
            if (IsOnBrowserThread)
                return action();

            var tcs = NewTcs<T>();

            try
            {
                PostToBrowserThread(async () =>
                {
                    try { tcs.TrySetResult(await action()); }
                    catch (Exception ex) { tcs.TrySetException(ex); }
                });
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        private Task RunOnBrowserThreadAsync(Func<Task> action)
        {
            if (IsOnBrowserThread)
                return action();

            return RunOnBrowserThreadAsync(async () =>
            {
                await action();

                return true;
            });
        }

        private void RunOnBrowserThread(Action action)
        {
            if (IsOnBrowserThread)
            {
                action();
                return;
            }

            try { PostToBrowserThread(action); }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        private static TaskCompletionSource<T> NewTcs<T>()
        {
            return new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        // NOTE: .NET Standard 2.0 lacks Task.WaitAsync(), so race the task against a cancellable delay.
        private static async Task<T> WaitAsync<T>(
            Task<T> task,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            if (task.IsCompleted)
                return await task;

            using (var delayCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                try
                {
                    Task delay = Task.Delay(timeout, delayCts.Token);

                    if (await Task.WhenAny(task, delay) != task)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        throw new TimeoutException("The operation has timed out.");
                    }

                    return await task;
                }
                finally
                {
                    delayCts.Cancel();
                }
            }
        }

        public virtual void Dispose()
        {
            RunOnBrowserThread(DisposeOnBrowserThread);
        }

        private void DisposeOnBrowserThread()
        {
            if (disposed)
                return;

            disposed = true;
            CancelOperation();

            facadeReadyTcs?.TrySetCanceled();
            probeTcs?.TrySetCanceled();
            resetTcs?.TrySetCanceled();

            try
            {
                ClearMediaMapping();
                ReleaseBrowserResources();
            }
            finally
            {
                documentRoot = null;
                currentSessionId = null;
                CurrentSource = null;
                IsPlayerActive = false;
            }
        }

        // NOTE: Plain immutable structs avoid the record/init support types missing from .NET Standard 2.0.
        private readonly struct ProbeMessage
        {
            public bool CanPlay { get; }
            public string? Reason { get; }
            public string? Error { get; }

            public ProbeMessage(bool canPlay, string? reason, string? error)
            {
                CanPlay = canPlay;
                Reason = reason;
                Error = error;
            }
        }

        private readonly struct PlayerMessage
        {
            public bool Ready { get; }
            public string? Reason { get; }
            public string? Error { get; }

            public PlayerMessage(bool ready, string? reason, string? error)
            {
                Ready = ready;
                Reason = reason;
                Error = error;
            }
        }
    }
}
