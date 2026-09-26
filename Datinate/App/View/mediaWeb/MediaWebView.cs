using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
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
                if (browser.CoreWebView2 == null)
                    await browser.EnsureCoreWebView2Async();

                ConfigureCore();
            });
        }
        
        public void SetAudioEnvironmentPath(string? audioEnvironmentPath) =>
            this.audioEnvironmentPath = null;

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
            blankRequested = false;
            Ui(ApplyOverlayState);

            BrowserUi(async () =>
            {
                if (browser.CoreWebView2 == null)
                    await browser.EnsureCoreWebView2Async();

                var core = browser.CoreWebView2;
                if (core == null)
                    return;

                ConfigureCore();

                try
                {
                    Uri tempUri =
                        await WebHelper.CreateTempHtmlAsync(html);

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

            string extension =
                WebHelper.GetExtensionNoQuery(uri);
            
            if (WebHelper.IsMusicVgmExtension(extension))
            {
                ShowUnavailableContent(
                    "Game Music Preview Unavailable",
                    "Playback support for this video game music format is not yet available.");

                return;
            }

            if (WebHelper.IsMusicStandardExtension(extension))
            {
                LoadWebUri(uri);
                return;
            }

            LoadWebUri(uri);
        }
        private void LoadWebUri(Uri uri)
        {
            blankRequested = false;
            Ui(ApplyOverlayState);

            BrowserUi(async () =>
            {
                if (browser.CoreWebView2 == null)
                    await browser.EnsureCoreWebView2Async();

                var core = browser.CoreWebView2;
                if (core == null)
                    return;

                // Ensure handlers exist before navigation.
                ConfigureCore();

                try
                {
                    core.Navigate(uri.AbsoluteUri);
                }
                catch (Exception)
                {
                    ShowUnavailableContent(
                        "Content Cannot be Previewed",
                        "This media source cannot currently be previewed.");
                }
            });
        }
        public void LoadUriInBrowser()
        {
            Uri? source = WebHelper.TryGetSource(browser);

            if (source == null) return;

            BrowserUtil.LoadInBrowser(source.ToString());
        }

        public void ClearView()
        {
            Ui(() =>
            {
                blankRequested = true;
                ApplyOverlayState();

                BrowserUi(async () =>
                {
                    if (browser.CoreWebView2 == null)
                        await browser.EnsureCoreWebView2Async();

                    if (browser.CoreWebView2 == null)
                        return;

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
                // Embedded media surface - do not expose browser-level
                // Save As / download-style context menu operations.
                core.Settings.AreDefaultContextMenusEnabled = false;
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

            if (!WebHelper.IsVideoExtension(extension))
                return;

            try
            {
                await WebHelper.EnableVideoLoopAsync(core);
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
            LoadBlankHtml();
            SetReceiptEffectOnly(e);

            try
            {
                if (e.Data is DataObject dobj)
                    dobj.SetData(DatinateHelper.WEB_BROWSER_MEDIA_ShowMediaCard, true);
            }
            catch (Exception) { }
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
