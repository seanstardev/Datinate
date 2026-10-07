using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using Datinate.Shared.Rmvc;
using Microsoft.Web.WebView2.Core;

namespace datinate.app
{
    public partial class MediaWebView : UserControl, IWebMediaView
    {
        private Control? dragDropOverlay;
        private bool blankRequested;
        private bool overlayWired;
        private bool dragSessionActive;
        private bool browserSuppressedForOverlay;
        private string? lastTempHtmlPath = null;
        private string? audioEnvironmentPath = null;
        private DatinateAudioWebSession? audioSession;
        private int loadToken;
        private Form? hostForm;

        public MediaWebView()
        {
            InitializeComponent();

            Facade.RegisterActor(this);

            DatinateWebView2Manager.Register(browser);

            try
            {
                browser.SourceChanged += browser_SourceChanged;
            }
            catch (Exception) { }

            AttachDragDropPrompt();
            AttachDragDropReceiptOnly();

            ClearView();

            BrowserUi(async () =>
            {
                _ = await WebHelper.EnsureCoreAsync(browser);

                ConfigureCore();
            });
        }
        
        public void SetAudioEnvironmentPath(string? audioEnvironmentPath)
        {
            if (string.Equals(this.audioEnvironmentPath, audioEnvironmentPath, StringComparison.OrdinalIgnoreCase))
                return;

            audioSession?.Dispose();
            audioSession = null;
            this.audioEnvironmentPath = audioEnvironmentPath;
        }


        public void StartReceiveMediaDrop()
        {
            Ui(() =>
            {
                dragSessionActive = true;
                ApplyOverlayState();
            });
        }

        public void StopReceiveMediaDrop()
        {
            Ui(() =>
            {
                dragSessionActive = false;
                ApplyOverlayState();
            });
        }
        public void LoadPageContent(string html)
        {
            int token = unchecked(++loadToken);

            blankRequested = false;
            Ui(ApplyOverlayState);

            BrowserUi(async () =>
            {
                await ResetAudioSessionAsync();

                if (token != loadToken)
                    return;

                var core = await WebHelper.EnsureCoreAsync(browser);

                ConfigureCore();

                try
                {
                    Uri tempUri =
                        await WebHelper.CreateTempHtmlAsync(html);

                    if (token != loadToken)
                    {
                        WebHelper.DeleteFile(tempUri.LocalPath);
                        return;
                    }

                    string? oldPath = lastTempHtmlPath;
                    lastTempHtmlPath = tempUri.LocalPath;

                    core.Navigate(tempUri.AbsoluteUri);

                    WebHelper.DeleteFile(oldPath);
                }
                catch (Exception) { }
            });
        }

        public void LoadUrl(string url)
        {
            if (!WebHelper.TryGetSupportedUri(url, out var uri))
            {
                ShowUnavailableContent(
                    "Content Cannot be Previewed",
                    "This media source cannot currently be previewed.");

                return;
            }

            int token = unchecked(++loadToken);
            string extension = WebHelper.GetExtensionNoQuery(uri);

            if (WebHelper.IsImageExtension(extension) ||
                WebHelper.IsVideoExtension(extension) ||
                WebHelper.LooksLikeHtmlOrDocument(extension))
            {
                LoadWebUri(uri, token);
                return;
            }

            LoadAudioOrWebUri(uri, token);
        }


        private void LoadAudioOrWebUri(Uri uri, int token)
        {
            blankRequested = false;
            Ui(ApplyOverlayState);

            BrowserUi(async () =>
            {
                if (token != loadToken)
                    return;

                if (string.IsNullOrWhiteSpace(audioEnvironmentPath))
                {
                    LoadWebUri(uri, token);
                    return;
                }

                bool canPlay;

                try
                {
                    canPlay =
                        await DatinateAudioWebSession.CanPlayAsync(
                            uri,
                            audioEnvironmentPath);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (token != loadToken)
                    return;

                if (!canPlay)
                {
                    LoadWebUri(uri, token);
                    return;
                }
                if (token != loadToken)
                    return;

                var core = await WebHelper.EnsureCoreAsync(browser);

                ConfigureCore();

                // Override ConfigureCore() for audio player:
                core.Settings.AreDefaultContextMenusEnabled = false;

                await ResetAudioSessionAsync();

                if (token != loadToken)
                    return;

                audioSession ??=
                    new DatinateAudioWebSession(
                        browser,
                        audioEnvironmentPath);

                DatinateAudioWebLoadResult result =
                    await audioSession.LoadPlayerAsync(uri);

                if (token != loadToken)
                    return;

                if (!result.Loaded)
                {
                    LoadWebUri(uri, token);
                    return;
                }

                blankRequested = false;
                ApplyOverlayState();
            });
        }

        private void LoadWebUri(Uri uri, int token)
        {
            blankRequested = false;
            Ui(ApplyOverlayState);

            BrowserUi(async () =>
            {
                await ResetAudioSessionAsync();

                if (token != loadToken)
                    return;

                var core = await WebHelper.EnsureCoreAsync(browser);

                ConfigureCore();

                try
                {
                    core.Navigate(uri.AbsoluteUri);
                }
                catch (Exception)
                {
                    if (token != loadToken)
                        return;

                    ShowUnavailableContent(
                        "Content Cannot be Previewed",
                        "This media source cannot currently be previewed.");
                }
            });
        }

        private Task ResetAudioSessionAsync() =>
            audioSession?.ResetAsync() ?? Task.CompletedTask;

        public void LoadUriInBrowser()
        {
            Uri? source =
                audioSession?.IsPlayerActive == true
                    ? audioSession.CurrentSource
                    : WebHelper.TryGetSource(browser);

            if (source == null)
                return;

            BrowserUtil.LoadInBrowser(source.ToString());
        }

        public void ClearView()
        {
            int token = unchecked(++loadToken);

            Ui(() =>
            {
                blankRequested = true;
                ApplyOverlayState();

                BrowserUi(async () =>
                {
                    await ResetAudioSessionAsync();

                    if (token != loadToken)
                        return;

                    var core = await WebHelper.EnsureCoreAsync(browser);

                    LoadBlankHtml();
                });
            });
        }

        private void LoadBlankHtml()
        {
            Ui(() =>
            {
                WebHelper.NavigateToBlankPage(
                    browser.CoreWebView2);
            });
        }
        private void ShowUnavailableContent(string title, string body)
        {
            var html = WebHelper.CreateContentUnavailableHtml(title, body);
            LoadPageContent(html);
        }
        protected void HandleDisposing()
        {
            unchecked { ++loadToken; }

            if (hostForm != null)
            {
                hostForm.Resize -= HostForm_Resize;
                hostForm = null;
            }

            audioSession?.Dispose();
            audioSession = null;

            Facade.UnregisterActor(this);

            try
            {
                browser.SourceChanged -= browser_SourceChanged;
            }
            catch (Exception) { }

            try
            {
                var core = browser.CoreWebView2;

                if (core != null)
                {
                    core.DownloadStarting -= Core_DownloadStarting;
                    core.NavigationCompleted -= Core_NavigationCompleted;
                }
            }
            catch (Exception) { }

            if (dragDropOverlay != null)
            {
                try
                {
                    dragDropOverlay.Dispose();
                }
                catch (Exception) { }

                dragDropOverlay = null;
            }

            WebHelper.DeleteFile(lastTempHtmlPath);
            lastTempHtmlPath = null;
        }
        
        private void browser_SourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
        {
            Ui(() =>
            {
                Uri? src = WebHelper.TryGetSource(browser);
                UpdateBlankOverlayForSource(src);
            });
        }

        private void UpdateBlankOverlayForSource(Uri? src)
        {
            if (!blankRequested)
            {
                ApplyOverlayState();
                return;
            }

            if (WebHelper.IsAboutBlank(src))
            {
                ApplyOverlayState();
                return;
            }

            if (src != null && src.IsAbsoluteUri)
            {
                var scheme = src.Scheme;

                if (scheme.Equals(
                        Uri.UriSchemeHttp,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    scheme.Equals(
                        Uri.UriSchemeHttps,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    scheme.Equals(
                        Uri.UriSchemeFile,
                        StringComparison.OrdinalIgnoreCase))
                {
                    blankRequested = false;
                    ApplyOverlayState();
                    return;
                }
            }

            ApplyOverlayState();
        }

        private void ApplyOverlayState()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            var overlayWanted = blankRequested || dragSessionActive;

            if (overlayWanted)
            {
                var overlay = EnsureDragDropOverlay();
                WireDragDropOverlay(overlay);

                overlay.Enabled = true;
                overlay.BringToFront();
                overlay.Visible = true;

                if (browser.Visible)
                {
                    browserSuppressedForOverlay = true;
                    browser.Visible = false;
                }

                return;
            }

            if (dragDropOverlay != null && !dragDropOverlay.IsDisposed)
            {
                dragDropOverlay.Visible = false;
                dragDropOverlay.Enabled = false;
            }

            if (browserSuppressedForOverlay)
            {
                browserSuppressedForOverlay = false;
                browser.Visible = true;
            }
        }

        private Control EnsureDragDropOverlay()
        {
            if (dragDropOverlay != null && !dragDropOverlay.IsDisposed)
                return dragDropOverlay;

            var host = browser.Parent ?? this;

            dragDropOverlay = DragPromptOverlayRenderer.CreateOverlayControlDark(
                source: host,
                title: "Drag a Media Card here",
                hint: "Drop to view Media",
                offsetX: 0,
                offsetY: 0);

            dragDropOverlay.AllowDrop = true;

            host.Controls.Add(dragDropOverlay);
            dragDropOverlay.BringToFront();

            overlayWired = false;

            return dragDropOverlay;
        }

        private void AttachDragDropPrompt()
        {
            EnsureDragDropOverlay();
        }

        private void AttachDragDropReceiptOnly()
        {
            if (dragDropOverlay != null && !dragDropOverlay.IsDisposed)
                WireDragDropOverlay(dragDropOverlay);
        }

        private void WireDragDropOverlay(Control overlay)
        {
            if (overlayWired)
                return;

            overlay.AllowDrop = true;

            overlay.DragEnter -= DragDropOverlay_DragEnter;
            overlay.DragOver -= DragDropOverlay_DragOver;
            overlay.DragDrop -= DragDropOverlay_DragDrop;

            overlay.DragEnter += DragDropOverlay_DragEnter;
            overlay.DragOver += DragDropOverlay_DragOver;
            overlay.DragDrop += DragDropOverlay_DragDrop;

            overlayWired = true;
        }

        private void ConfigureCore()
        {
            var core = browser.CoreWebView2;
            if (core == null)
                return;

            try
            {
                browser.AllowExternalDrop = false;
            }
            catch (Exception) { }

            try
            {
                core.Settings.AreDefaultContextMenusEnabled = true;
            }
            catch (Exception) { }

            try
            {
                core.ContextMenuRequested -= Core_ContextMenuRequested;
                core.ContextMenuRequested += Core_ContextMenuRequested;
            }
            catch (Exception) { }

            try
            {
                core.DownloadStarting -= Core_DownloadStarting;
                core.DownloadStarting += Core_DownloadStarting;
            }
            catch (Exception) { }

            try
            {
                core.NavigationCompleted -= Core_NavigationCompleted;
                core.NavigationCompleted += Core_NavigationCompleted;
            }
            catch (Exception) { }
        }

        private void Core_ContextMenuRequested(
            object? sender,
            CoreWebView2ContextMenuRequestedEventArgs e)
        {
            for (int i = e.MenuItems.Count - 1; i >= 0; i--)
            {
                string name = e.MenuItems[i].Name;

                if (string.Equals(name, "back", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, "forward", StringComparison.OrdinalIgnoreCase))
                {
                    e.MenuItems.RemoveAt(i);
                }
            }
        }

        private void Core_DownloadStarting(
            object? sender,
            CoreWebView2DownloadStartingEventArgs e)
        {
            WebHelper.CancelDownload(e);

            ShowUnavailableContent(
                "Content Cannot be Previewed",
                "This media type cannot currently be previewed.");
        }

        private async void Core_NavigationCompleted(
          object? sender,
          CoreWebView2NavigationCompletedEventArgs e)
        {
            if (sender is not CoreWebView2 core)
                return;

            Uri? source = WebHelper.TryGetSource(browser);
            if (source == null)
                return;

            string extension = WebHelper.GetExtensionNoQuery(source);

            if (WebHelper.IsVideoExtension(extension) == false)
                return;

            try
            {
                await WebHelper.EnableVideoLoopAsync(core);

                // Override ConfigureCore() for audio player:
                core.Settings.AreDefaultContextMenusEnabled = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void DragDropOverlay_DragEnter(object? sender, DragEventArgs e) =>
            SetReceiptEffectOnly(e);
        
        private void DragDropOverlay_DragOver(object? sender, DragEventArgs e) =>
            SetReceiptEffectOnly(e);
        
        private void DragDropOverlay_DragDrop(object? sender, DragEventArgs e)
        {
            int token = unchecked(++loadToken);

            BrowserUi(async () =>
            {
                await ResetAudioSessionAsync();

                if (token == loadToken)
                    LoadBlankHtml();
            });

            SetReceiptEffectOnly(e);

            try
            {
                if (e.Data is DataObject dobj)
                    dobj.SetData(DatinateHelper.WEB_BROWSER_MEDIA_ShowMediaCard, true);
            }
            catch (Exception)
            {
            }
        }

        private void SetReceiptEffectOnly(DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) != 0)
                e.Effect = DragDropEffects.Copy;
            else if ((e.AllowedEffect & DragDropEffects.Move) != 0)
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Ui(Action action)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(action);
                return;
            }

            action();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            var form = FindForm();

            if (ReferenceEquals(form, hostForm))
                return;

            if (hostForm != null)
                hostForm.Resize -= HostForm_Resize;

            hostForm = form;

            if (hostForm != null)
            {
                hostForm.Resize += HostForm_Resize;
                HostForm_Resize(hostForm, EventArgs.Empty);
            }
        }

        private void HostForm_Resize(object? sender, EventArgs e)
        {
            if (browser.CoreWebView2 != null && hostForm != null)
            {
                browser.CoreWebView2.IsMuted =
                    hostForm.WindowState == FormWindowState.Minimized;
            }
        }
        private void BrowserUi(Func<Task> action)
        {
            if (!browser.IsHandleCreated)
                return;

            browser.BeginInvoke(new Action(async () =>
            {
                try { await action(); }
                catch (Exception) { }
            }));
        }
    }
}
