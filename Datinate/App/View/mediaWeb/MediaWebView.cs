using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using Microsoft.Web.WebView2.Core;
using System.Text;

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

                try
                {
                    browser.AllowExternalDrop = false;
                }
                catch (Exception) { }

                try
                {
                    if (browser.CoreWebView2 != null)
                        browser.CoreWebView2.IsMuted = true;
                }
                catch (Exception) { }
            });
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
            blankRequested = false;
            Ui(ApplyOverlayState);

            BrowserUi(async () =>
            {
                if (browser.CoreWebView2 == null)
                    await browser.EnsureCoreWebView2Async();

                try
                {
                    if (browser.CoreWebView2 != null)
                        browser.CoreWebView2.IsMuted = true;
                }
                catch (Exception) { }

                try
                {
                    string tempFolder = System.IO.Path.Combine(
                        System.IO.Path.GetTempPath(),
                        "Datinate",
                        "MediaWebView");

                    System.IO.Directory.CreateDirectory(tempFolder);

                    string tempPath = System.IO.Path.Combine(
                        tempFolder,
                        Guid.NewGuid().ToString("N") + ".html");

                    await System.IO.File.WriteAllTextAsync(
                        tempPath,
                        html,
                        new UTF8Encoding(false));

                    string? oldPath = lastTempHtmlPath;
                    lastTempHtmlPath = tempPath;

                    browser.CoreWebView2?.Navigate(new Uri(tempPath).AbsoluteUri);

                    if (!string.IsNullOrWhiteSpace(oldPath))
                    {
                        try
                        {
                            if (System.IO.File.Exists(oldPath))
                                System.IO.File.Delete(oldPath);
                        }
                        catch (Exception) { }
                    }
                }
                catch (Exception) { }
            });
        }
        
        public void LoadUrl(string url)
        {
            blankRequested = false;
            Ui(ApplyOverlayState);

            BrowserUi(async () =>
            {
                if (browser.CoreWebView2 == null)
                    await browser.EnsureCoreWebView2Async();

                try
                {
                    if (browser.CoreWebView2 != null)
                        browser.CoreWebView2.IsMuted = true;
                }
                catch (Exception) { }

                try
                {
                    browser.CoreWebView2?.Navigate(url);
                }
                catch (Exception) { }
            });
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

                    LoadBlanHtml();
                    
                });
            });
        }

        private void LoadBlanHtml()
        {
            Ui(() =>
            {
                if (browser.CoreWebView2 == null)
                    return;
                
                try { browser.CoreWebView2.NavigateToString(DatinateHelper.LoadingHtmlBlack); }
                catch (Exception) { }
                
            });
        }

        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);

            if (dragDropOverlay != null)
            {
                try { dragDropOverlay.Dispose(); }
                catch (Exception) { }

                dragDropOverlay = null;
            }
        }

        private void browser_SourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
        {
            Ui(() =>
            {
                Uri? src = TryGetBrowserSource();
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

            if (IsAboutBlank(src))
            {
                ApplyOverlayState();
                return;
            }

            if (src != null && src.IsAbsoluteUri)
            {
                var scheme = src.Scheme;

                if (scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                    scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                    scheme.Equals(Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
                {
                    blankRequested = false;
                    ApplyOverlayState();
                    return;
                }
            }

            ApplyOverlayState();
        }

        private static bool IsAboutBlank(Uri? src)
        {
            if (src == null)
                return false;

            if (!src.IsAbsoluteUri)
                return false;

            if (!src.Scheme.Equals("about", StringComparison.OrdinalIgnoreCase))
                return false;

            return string.Equals(src.AbsoluteUri, "about:blank", StringComparison.OrdinalIgnoreCase);
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

        private void DragDropOverlay_DragEnter(object? sender, DragEventArgs e)
        {
            SetReceiptEffectOnly(e);
        }

        private void DragDropOverlay_DragOver(object? sender, DragEventArgs e)
        {
            SetReceiptEffectOnly(e);
        }

        private void DragDropOverlay_DragDrop(object? sender, DragEventArgs e)
        {           
            LoadBlanHtml();
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

        private Uri? TryGetBrowserSource()
        {
            try { return browser.Source; }
            catch (Exception) { return null; }
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

        public void LoadUriInBrowser()
        {
            BrowserUtil.LoadInBrowser(browser.Source.ToString());
        }
    }
}
