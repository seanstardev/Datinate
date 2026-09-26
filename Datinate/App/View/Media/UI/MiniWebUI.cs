using Datinate.Properties;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace datinate.app
{
    public partial class MiniWebUI : UserControl
    {
        private const bool CLEARUI_DISPOSE_WEBVIEW = false;
        private const double AUDIO_ZOOM = 0.1;

        private Task? ensureCoreTask;

        private readonly Panel unavailableOverlay;
        private readonly Label unavailableTitleLabel;
        private readonly Label unavailableBodyLabel;

        private readonly Panel blankOverlay;
        private readonly PictureBox spinnerOverlay;
        private bool prevSpinnerVisible;

        private static readonly MemoryStream SpinnerStream = new(Resources.Loading_icon);
        private static readonly Image SpinnerImage = Image.FromStream(SpinnerStream);

        private readonly PictureBox imageView;
     
        private static readonly HttpClient Http = new();

        private string? pendingUri;
        private bool coreConfigured;

        private int navToken;

        private WebView2? webView;

        private bool interactionSuppressed;
        private bool prevBlankVisible;
        private bool prevImageVisible;
        private bool prevWebVisible;

        private string? audioEnvironmentPath;
        private DatinateAudioWebSession? audioSession;
        
        private int loaderPercentageScale = 40;

        public MiniWebUI()
        {
            InitializeComponent();

            BorderStyle = BorderStyle.None;

            imageView = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Visible = false,
                Enabled = false,
                TabStop = false,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            blankOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Visible = false,
                TabStop = false
            };

            spinnerOverlay = new PictureBox
            {
                Dock = DockStyle.None,
                Anchor = AnchorStyles.None,
                BackColor = Color.Black,
                Visible = false,
                TabStop = false,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = SpinnerImage
            };

            unavailableOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Visible = false,
                TabStop = false
            };

            unavailableTitleLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.BottomCenter,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font(Font.FontFamily, 11f, FontStyle.Bold),
                Text = "Content Cannot be Previewed",
                Padding = new Padding(12, 0, 12, 0)
            };

            unavailableBodyLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                ForeColor = Color.Gainsboro,
                BackColor = Color.Transparent,
                Font = new Font(Font.FontFamily, 9f, FontStyle.Regular),
                Text = "This media source can still be assigned, but there is no previewable content path available.",
                Padding = new Padding(16, 8, 16, 0)
            };

            unavailableOverlay.Controls.Add(unavailableBodyLabel);
            unavailableOverlay.Controls.Add(unavailableTitleLabel);

            blankOverlay.Controls.Add(spinnerOverlay);
            spinnerOverlay.BringToFront();

            hostPanel.Controls.Add(imageView);
            hostPanel.Controls.Add(blankOverlay);
            hostPanel.Controls.Add(unavailableOverlay);

            VisibleChanged += (_, __) => TryLoadPendingUri();
            hostPanel.SizeChanged += (_, __) => TryLoadPendingUri();

            blankOverlay.SizeChanged += (_, __) => LayoutSpinnerOverlay();
            LayoutSpinnerOverlay();

            ShowSpinnerOverlay();
            blankOverlay.BringToFront();
        }

        internal void SetAudioEnvironmentPath(string? audioEnvironmentPath)
        {
            if (string.Equals(this.audioEnvironmentPath, audioEnvironmentPath, StringComparison.OrdinalIgnoreCase))
                return;

            audioSession?.Dispose();
            audioSession = null;
            this.audioEnvironmentPath = audioEnvironmentPath;
        }

        internal void SetInteractionSuppressed(bool suppress)
        {
            Ui(() =>
            {
                if (interactionSuppressed == suppress)
                    return;

                if (suppress)
                {
                    prevBlankVisible = blankOverlay.Visible;
                    prevImageVisible = imageView.Visible;
                    prevWebVisible = webView?.Visible ?? false;
                    prevSpinnerVisible = spinnerOverlay.Visible;

                    blankOverlay.Visible = true;
                    blankOverlay.BringToFront();

                    spinnerOverlay.Visible = prevSpinnerVisible;
                    if (spinnerOverlay.Visible)
                        spinnerOverlay.BringToFront();

                    imageView.Visible = false;

                    if (webView != null)
                        webView.Visible = false;
                }
                else
                {
                    blankOverlay.Visible = prevBlankVisible;
                    imageView.Visible = prevImageVisible;

                    if (webView != null)
                        webView.Visible = prevWebVisible;

                    spinnerOverlay.Visible = prevBlankVisible && prevSpinnerVisible;

                    if (blankOverlay.Visible)
                    {
                        blankOverlay.BringToFront();
                        if (spinnerOverlay.Visible)
                            spinnerOverlay.BringToFront();
                    }
                }

                interactionSuppressed = suppress;
            });
        }
        internal void ShowUnavailableContent(
            string title = "Content Cannot be Previewed",
            string body = "This media source can still be assigned, but there is no previewable content path available.")
        {
            pendingUri = null;
            unchecked { ++navToken; }

            Ui(() =>
            {
                audioSession?.Dispose();
                audioSession = null;

                try
                {
                    HideSpinnerOverlay();
                    ClearImage();

                    if (webView?.CoreWebView2 != null)
                    {
                        WebHelper.Stop(webView.CoreWebView2);
                        WebHelper.NavigateToAboutBlank(webView.CoreWebView2);
                    }

                    if (webView != null)
                        webView.Visible = false;

                    imageView.Visible = false;
                    blankOverlay.Visible = false;

                    unavailableTitleLabel.Text = title;
                    unavailableBodyLabel.Text = body;

                    unavailableOverlay.Visible = true;
                    unavailableOverlay.BringToFront();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            });
        }

        internal void HideUnavailableContent()
        {
            Ui(() =>
            {
                unavailableOverlay.Visible = false;
            });
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            TryLoadPendingUri();
        }

        private void LayoutSpinnerOverlay()
        {
            var cs = blankOverlay.ClientSize;
            if (cs.Width < 2 || cs.Height < 2)
                return;

            int pct = Math.Clamp(loaderPercentageScale, 1, 100);

            int maxW = (int)Math.Round(cs.Width * (pct / 100.0));
            int maxH = (int)Math.Round(cs.Height * (pct / 100.0));

            maxW = Math.Max(1, Math.Min(maxW, cs.Width));
            maxH = Math.Max(1, Math.Min(maxH, cs.Height));

            int w = maxW;
            int h = maxH;

            var img = spinnerOverlay.Image;
            if (img != null && img.Width > 0 && img.Height > 0)
            {
                double r = (double)img.Width / img.Height;

                w = maxW;
                h = (int)Math.Round(w / r);

                if (h > maxH)
                {
                    h = maxH;
                    w = (int)Math.Round(h * r);
                }

                w = Math.Max(1, Math.Min(w, maxW));
                h = Math.Max(1, Math.Min(h, maxH));
            }

            spinnerOverlay.Size = new Size(w, h);
            spinnerOverlay.Location = new Point((cs.Width - w) / 2, (cs.Height - h) / 2);
        }

        private async Task LoadAudioOrWebInternalAsync(Uri uri, int token)
        {
            if (string.IsNullOrWhiteSpace(audioEnvironmentPath))
            {
                await LoadWebInternalAsync(uri, token);
                return;
            }

            bool canPlay;

            try
            {
                canPlay = await DatinateAudioWebSession.CanPlayAsync(uri, audioEnvironmentPath);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (token != navToken) return;

            if (!canPlay)
            {
                await LoadWebInternalAsync(uri, token);
                return;
            }

            if (InvokeRequired)
            {
                Ui(() => _ = LoadAudioOrWebInternalAsync(uri, token));
                return;
            }

            if (interactionSuppressed)
            {
                pendingUri = uri.AbsoluteUri;
                return;
            }

            EnsureWebView();

            ShowSpinnerOverlay();
            imageView.Visible = false;

            webView!.Visible = true;
            webView.SendToBack();

            try
            {
                var t = ensureCoreTask;

                if (t == null)
                {
                    t = WebHelper.EnsureCoreAsync(webView);
                    ensureCoreTask = t;
                }

                await t;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return;
            }

            if (token != navToken) return;

            var core = webView.CoreWebView2;
            
            if (core == null) return;

            if (!coreConfigured)
            {
                ConfigureCore(core);
                coreConfigured = true;
            }

            core.IsMuted = true;
            webView.ZoomFactor = AUDIO_ZOOM;

            await ResetAudioSessionAsync();

            if (token != navToken) return;

            audioSession ??= new DatinateAudioWebSession(webView, audioEnvironmentPath);

            DatinateAudioWebLoadResult result = await audioSession.LoadPlayerAsync(uri);

            if (token != navToken) return;

            if (!result.Loaded)
            {
                await LoadWebInternalAsync(uri, token);
                return;
            }

            HideSpinnerOverlay();

            if (!interactionSuppressed)
                blankOverlay.Visible = false;
        }

        private async Task LoadImageInternalAsync(Uri uri, int token)
        {
            await ResetAudioSessionAsync();

            if (token != navToken) return;

            Ui(() =>
            {
                ShowSpinnerOverlay();
                imageView.Visible = false;
                if (webView != null) webView.Visible = false;
            });

            try
            {
                byte[] bytes;

                if (uri.IsFile)
                {
                    var path = uri.LocalPath;

                    if (!File.Exists(path)) throw new FileNotFoundException(path);

                    bytes = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
                }
                else
                {
                    bytes = await Http.GetByteArrayAsync(uri).ConfigureAwait(false);
                }

                if (token != navToken)
                    return;

                Image img;

                using (var ms = new MemoryStream(bytes))
                using (var tmp = Image.FromStream(
                    ms, useEmbeddedColorManagement: true, validateImageData: true))
                {
                    img = new Bitmap(tmp);
                }

                Ui(() =>
                {
                    if (token != navToken)
                    {
                        img.Dispose();
                        return;
                    }

                    SetImage(img);

                    HideSpinnerOverlay();

                    if (!interactionSuppressed)
                        blankOverlay.Visible = false;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                Ui(() =>
                {
                    if (token != navToken)
                        return;

                    _ = LoadWebInternalAsync(uri, token);
                });
            }
        }

        private void EnsureWebView()
        {
            if (webView != null) return;

            var newWv = new BorderlessWebView2
            {
                AllowExternalDrop = false,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Name = "webView",
                TabIndex = 0,
                DefaultBackgroundColor = Color.Black
            };

            DatinateWebView2Manager.Register(newWv);

            WebHelper.ApplyCreationProperties(newWv);

            webView = newWv;

            hostPanel.Controls.Add(webView);
            webView.SendToBack();
        }
        private void ConfigureCore(CoreWebView2 core)
        {
            core.DownloadStarting += Core_DownloadStarting;
            core.NewWindowRequested += Core_NewWindowRequested;

            core.Settings.IsStatusBarEnabled = false;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = false;
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


        private void Core_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            try
            {
                e.Handled = true;

                if (webView?.CoreWebView2 != null && !string.IsNullOrWhiteSpace(e.Uri))
                    webView.CoreWebView2.Navigate(e.Uri);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void ClearImage()
        {
            try
            {
                var old = imageView.Image;
                imageView.Image = null;
                old?.Dispose();
            }
            catch { }
        }

        private void SetImage(Image img)
        {
            ClearImage();
            imageView.Image = img;
            imageView.Visible = true;
            imageView.BringToFront();
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

        private void DisposeMiniResources()
        {
            audioSession?.Dispose();
            audioSession = null;

            try { ClearImage(); } catch { }

            WebView2? wv = webView;

            if (wv == null) return;

            webView = null;

            try { DatinateWebView2Manager.Unregister(wv); } catch { }
            try { hostPanel.Controls.Remove(wv); } catch { }
            try { wv.Dispose(); } catch { }
        }
        
        private void ShowSpinnerOverlay()
        {
            LayoutSpinnerOverlay();

            spinnerOverlay.Visible = true;
            blankOverlay.Visible = true;
            blankOverlay.BringToFront();
            spinnerOverlay.BringToFront();
        }

        private void HideSpinnerOverlay()
        {
            spinnerOverlay.Visible = false;
        }

        private void ShowBlankOnly()
        {
            blankOverlay.Visible = true;
            blankOverlay.BringToFront();

            spinnerOverlay.Visible = false;

            imageView.Visible = false;

            if (webView != null)
                webView.Visible = false;
        }

        private void HandleDisposing()
        {
            DisposeMiniResources();
        }

        internal void LoadURI(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri)) return;

            if (!WebHelper.TryGetSupportedUri(uri, out var parsed)) return;

            var abs = parsed.AbsoluteUri;

            if (InvokeRequired)
            {
                if (!IsDisposed && IsHandleCreated)
                    BeginInvoke(new Action(() => LoadURI(abs)));
                else
                    pendingUri = abs;

                return;
            }

            unavailableOverlay.Visible = false;

            if (interactionSuppressed)
            {
                pendingUri = abs;
                return;
            }

            if (!IsHandleCreated ||
                !Visible ||
                hostPanel.ClientSize.Width < 2 ||
                hostPanel.ClientSize.Height < 2)
            {
                pendingUri = abs;
                return;
            }

            pendingUri = null;

            int token = unchecked(++navToken);
            string ext = WebHelper.GetExtensionNoQuery(parsed);

            if (WebHelper.IsImageExtension(ext))
            {
                _ = LoadImageInternalAsync(parsed, token);
            }
            else if (WebHelper.IsVideoExtension(ext) || WebHelper.LooksLikeHtmlOrDocument(ext))
            {
                _ = LoadWebInternalAsync(parsed, token);
            }
            else
            {
                _ = LoadAudioOrWebInternalAsync(parsed, token);
            }
        }

        private void TryLoadPendingUri()
        {
            var u = pendingUri;

            if (string.IsNullOrWhiteSpace(u)) return;
            if (IsDisposed || !IsHandleCreated) return;
            if (!Visible) return;
            if (interactionSuppressed) return;
            if (hostPanel.ClientSize.Width < 2 || hostPanel.ClientSize.Height < 2) return;

            pendingUri = null;
            LoadURI(u);
        }

        internal void ClearUI()
        {
            pendingUri = null;
            unchecked { ++navToken; }

            var t = ensureCoreTask;
            if (t != null && (t.IsCanceled || t.IsFaulted))
                ensureCoreTask = null;

            Ui(() =>
            {
                try
                {
                    unavailableOverlay.Visible = false;

                    audioSession?.Dispose();
                    audioSession = null;

                    ShowBlankOnly();
                    ClearImage();

                    if (CLEARUI_DISPOSE_WEBVIEW)
                    {
                        ensureCoreTask = null;
                        coreConfigured = false;
                        DisposeMiniResources();
                        return;
                    }

                    if (webView?.CoreWebView2 != null)
                    {
                        WebHelper.Stop(webView.CoreWebView2);
                        WebHelper.NavigateToAboutBlank(webView.CoreWebView2);
                    }

                    if (webView != null)
                        webView.Visible = false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            });
        }
        
        private Task ResetAudioSessionAsync() =>
            audioSession?.ResetAsync() ?? Task.CompletedTask;
        
        private async Task LoadWebInternalAsync(Uri uri, int token)
        {
            try
            {
                if (InvokeRequired)
                {
                    Ui(() => _ = LoadWebInternalAsync(uri, token));
                    return;
                }

                if (interactionSuppressed)
                {
                    pendingUri = uri.AbsoluteUri;
                    return;
                }

                await ResetAudioSessionAsync();

                if (token != navToken) return;

                EnsureWebView();

                Ui(() =>
                {
                    ShowSpinnerOverlay();
                    imageView.Visible = false;

                    if (webView != null)
                    {
                        webView.Visible = true;
                        webView.SendToBack();
                    }
                });

                try
                {
                    if (webView == null) return;

                    var t = ensureCoreTask;

                    if (t == null)
                    {
                        t = WebHelper.EnsureCoreAsync(webView);
                        ensureCoreTask = t;
                    }

                    await t;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    return;
                }

                if (token != navToken) return;

                if (interactionSuppressed)
                {
                    pendingUri = uri.AbsoluteUri;
                    return;
                }

                var core = webView?.CoreWebView2;

                if (core == null) return;

                if (!coreConfigured)
                {
                    ConfigureCore(core);
                    coreConfigured = true;
                }

                core.IsMuted = true;

                string ext = WebHelper.GetExtensionNoQuery(uri);

                bool isVideo = WebHelper.IsVideoExtension(ext);

                bool looksLikeHtml = WebHelper.LooksLikeHtmlOrDocument(ext);

                double targetZoom = looksLikeHtml ? 0.1 : 1.0;

                ulong targetNavId = 0;

                EventHandler<CoreWebView2NavigationStartingEventArgs>? onStarting = null;
                EventHandler<CoreWebView2NavigationCompletedEventArgs>? onCompleted = null;

                onStarting = (_, e) =>
                {
                    if (token != navToken)
                    {
                        try { core.NavigationStarting -= onStarting; } catch { }
                        return;
                    }

                    if (string.Equals(e.Uri, uri.AbsoluteUri, StringComparison.Ordinal))
                    {
                        targetNavId = e.NavigationId;
                        try { webView!.ZoomFactor = targetZoom; } catch { }
                    }
                };

                onCompleted = async (_, e) =>
                {
                    if (token != navToken)
                    {
                        try { core.NavigationCompleted -= onCompleted; } catch { }
                        try { core.NavigationStarting -= onStarting; } catch { }
                        return;
                    }

                    if (targetNavId == 0 || e.NavigationId != targetNavId)
                        return;

                    try { core.NavigationCompleted -= onCompleted; } catch { }
                    try { core.NavigationStarting -= onStarting; } catch { }

                    Ui(() =>
                    {
                        if (token != navToken) return;

                        HideSpinnerOverlay();

                        if (!interactionSuppressed)
                            blankOverlay.Visible = false;
                    });

                    if (isVideo)
                    {
                        try
                        {
                            await WebHelper.EnableVideoLoopAsync(core);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex);
                        }
                    }
                };

                core.NavigationStarting += onStarting;
                core.NavigationCompleted += onCompleted;

                WebHelper.Stop(core);

                try
                {
                    webView!.ZoomFactor = 1.0;
                }
                catch { }

                WebHelper.NavigateToBlankPage(core);

                core.Navigate(uri.AbsoluteUri);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private sealed class BorderlessWebView2 : WebView2
        {
            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;

                    const int WS_BORDER = unchecked((int)0x00800000);
                    const int WS_EX_CLIENTEDGE = 0x00000200;
                    const int WS_EX_STATICEDGE = 0x00020000;
                    const int WS_EX_WINDOWEDGE = 0x00000100;

                    cp.Style &= ~WS_BORDER;
                    cp.ExStyle &= ~(WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_WINDOWEDGE);

                    return cp;
                }
            }
        }
    }
}
