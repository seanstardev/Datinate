using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using Datinate.Properties;
using Datinate.Shared;
using RadioLibCore.RadioDat;
using System.Diagnostics;

namespace datinate.app
{
    public partial class MainWebView : UserControl, IMainWebView
    {
        public event Action<IGamePart>? ShowGamePartReportsEvt;
        public event Action<DatGrouperEntryDTO>? PreviewMediaEvt;
        public event Action<DatGrouperEntryDTO>? AssignMediaEvt;
        public event Action<string>? SearchEntityNameEvt;

        private bool blankRequested;

        private bool dragSessionActive;
        private bool browserSuppressedForOverlay;

        private bool curationModeIsActive = false;
        private bool mediaIsAvailable = false;

        private DatGrouperEntryDTO? pendingPayload = null;
        private bool rawHtmlContentActive;

        private string autoSource = "Automated";
        public MainWebView()
        {
            InitializeComponent();
            Facade.RegisterActor(this);

            DatinateWebView2Manager.Register(browser);

            AttachDragDropPrompt();

            ClearView(true);

            BrowserUi(async () =>
            {
                if (browser.CoreWebView2 == null)
                    await browser.EnsureCoreWebView2Async();

                if (browser.CoreWebView2 != null)
                {
                    browser.CoreWebView2.IsMuted = true;
                    browser.AllowExternalDrop = false;
                }
            });
        }

        public void StartReceiveGameEntityDrop(DatGrouperEntryDTO datGrouperEntryDTO)
        {
            pendingPayload = datGrouperEntryDTO;

            Ui(() =>
            {
                dragSessionActive = true;
                ApplyPromptOverlayCardVisibility();
                ApplyOverlayState();
            });
        }
        public void StopReceiveGameEntityDrop()
        {
            pendingPayload = null;

            Ui(() =>
            {
                dragSessionActive = false;
                ApplyPromptOverlayCardVisibility();
                ApplyOverlayState();
            });
        }

        public void SetCurationModeIsActive()
        {
            Ui(() =>
            {
                curationModeIsActive = true;
                autoSource = "Queued or Curated";
                ApplyAutoSourceTextToPromptCards();
                ApplyPromptOverlayCardVisibility();
            });
        }
        private void ApplyOverlayState()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            Uri? src = TryGetBrowserSource();

            bool blankBrowserSurface =
                IsAboutBlank(src) &&
                !rawHtmlContentActive;

            bool overlayWanted =
                blankRequested ||
                dragSessionActive ||
                blankBrowserSurface;

            if (overlayWanted)
            {
                if (!dragDropOverlay.Visible)
                    dragDropOverlay.Visible = true;

                dragDropOverlay.BringToFront();
                promptOverlayControl?.BringToFront();

                if (browser.Visible)
                {
                    browserSuppressedForOverlay = true;
                    browser.Visible = false;
                }
            }
            else
            {
                if (dragDropOverlay.Visible)
                    dragDropOverlay.Visible = false;

                if (browserSuppressedForOverlay)
                {
                    browserSuppressedForOverlay = false;
                    browser.Visible = true;
                }
            }
        }
        public void LoadPageContent(string html)
        {
            rawHtmlContentActive = true;
            blankRequested = false;
            Ui(ApplyOverlayState);

            BrowserUi(async () =>
            {
                if (browser.CoreWebView2 == null)
                    await browser.EnsureCoreWebView2Async();

                try
                {
                    browser.CoreWebView2?.NavigateToString(html);
                }
                catch (Exception) { }
            });
        }
        public void LoadUrl(string url)
        {
            rawHtmlContentActive = false;
            blankRequested = false;
            Ui(ApplyOverlayState);

            BrowserUi(async () =>
            {
                if (browser.CoreWebView2 == null)
                    await browser.EnsureCoreWebView2Async();

                try
                {
                    browser.CoreWebView2?.Navigate(url);
                }
                catch (Exception) { }
            });
        }
        public void UnloadPageContent(bool doNotUnloadRemoteContent)
        {
            Ui(() =>
            {
                BrowserUi(async () =>
                {
                    if (browser.IsDisposed || !browser.IsHandleCreated)
                        return;

                    try
                    {
                        if (browser.CoreWebView2 == null)
                            await browser.EnsureCoreWebView2Async();
                    }
                    catch (Exception) { return; }

                    if (browser.CoreWebView2 == null)
                        return;

                    var source = TryGetBrowserSource();

                    if (!ShouldUnloadSource(source, doNotUnloadRemoteContent))
                        return;

                    try { browser.CoreWebView2.Stop(); }
                    catch (Exception) { }

                    blankRequested = true;
                    rawHtmlContentActive = false;
                    ApplyOverlayState();

                    try { browser.CoreWebView2.NavigateToString(DatinateHelper.LoadingHtmlBlack); }
                    catch (Exception) { }
                });
            });
        }
        public string? RenderCuratedImportErrorsReport(Dictionary<string, IGamePart?> importErrorReport)
        {
            return HtmlReportsUtil.CreateCuratedImportErrorsReport(importErrorReport);
        }
        public string? RenderGamePartGroupingReports(
            IGamePart part,
            ManagedListItemReport? managedReport,
            string customiseReport)
        {
            return HtmlReportsUtil.CreatePartReports(part, customiseReport, managedReport, this);
        }
        private void ApplyAutoSourceTextToPromptCards()
        {
            var stack = PromptStack;
            if (stack == null)
                return;

            string hint = $"Drop from {autoSource} → Here";

            if (stack.TryGetCard(PromptCardIx_Search, out var searchCard))
                stack.TrySetCard(PromptCardIx_Search, searchCard with { Hint = hint });

            if (stack.TryGetCard(PromptCardIx_PreviewMedia, out var previewCard))
                stack.TrySetCard(PromptCardIx_PreviewMedia, previewCard with { Hint = hint });

            if (stack.TryGetCard(PromptCardIx_AssignMedia, out var assignCard))
                stack.TrySetCard(PromptCardIx_AssignMedia, assignCard with { Hint = hint });

            if (stack.TryGetCard(PromptCardIx_Reports, out var reportsCard))
                stack.TrySetCard(PromptCardIx_Reports, reportsCard with { Hint = hint });

            stack.RefreshLayout();
        }
        private static bool ShouldUnloadSource(Uri? source, bool doNotUnloadRemoteContent)
        {
            if (source == null)
                return false;

            if (!source.IsAbsoluteUri)
                return true;

            var scheme = source.Scheme;

            if (scheme.Equals(Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase) ||
                scheme.Equals("about", StringComparison.OrdinalIgnoreCase) ||
                scheme.Equals("data", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return source.IsLoopback || !doNotUnloadRemoteContent;
            }

            return true;
        }

        public void SetMediaIsAvailable()
        {
            SetMediaAvailability(true);
        }
        public void ClearView(bool performFullReset)
        {
            Ui(() =>
            {
                curationModeIsActive = false;
                pendingPayload = null;
                dragSessionActive = false;
                rawHtmlContentActive = false;

                if (performFullReset)
                {
                    mediaIsAvailable = false;
                    autoSource = "Automated";
                    ApplyAutoSourceTextToPromptCards();
                }

                ApplyPromptOverlayCardVisibility();

                urlText.Text = string.Empty;

                blankRequested = true;
                ApplyOverlayState();

                BrowserUi(() =>
                {
                    if (browser.CoreWebView2 == null)
                        return;

                    browser.CoreWebView2.NavigateToString(DatinateHelper.LoadingHtmlBlack);
                });
            });
        }
        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
            DatinateWebView2Manager.Unregister(browser);
        }

        private void backBtn_Click(object sender, EventArgs e)
        {
            BrowserUi(() =>
            {
                if (browser.CanGoBack)
                {
                    try
                    {
                        browser.GoBack();
                    }
                    catch
                    {
                    }
                }
                else
                {
                    Ui(() =>
                    {
                        rawHtmlContentActive = false;
                        blankRequested = true;
                        ApplyOverlayState();
                    });
                }
            });
        }
        private void goBtn_Click(object sender, EventArgs e)
        {
            LoadUrl(urlText.Text);
        }


        private void closeBtn_Click(object sender, EventArgs e)
        {
            Ui(() =>
            {
                rawHtmlContentActive = false;
                blankRequested = true;
                ApplyPromptOverlayCardVisibility();
                ApplyOverlayState();

                BrowserUi(() =>
                {
                    try { browser.CoreWebView2?.Stop(); } catch (Exception) { }
                    try { browser.CoreWebView2?.Navigate("about:blank"); } catch (Exception) { }
                });
            });
        }
        private void browserBtn_Click(object sender, EventArgs e)
        {
            if (!TryGetExternalBrowserTarget(out string target))
            {
                MessageBox.Show("This content cannot be viewed in a Browser.", "Attention");
                return;
            }

            BrowserUtil.LoadInBrowser(target);
        }

        private bool TryGetExternalBrowserTarget(out string target)
        {
            target = string.Empty;

            if (blankRequested)
                return false;

            if (rawHtmlContentActive)
                return false;

            Uri? src = TryGetBrowserSource();
            if (src == null)
                return false;

            string raw = src.ToString();
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            if (!src.IsAbsoluteUri)
                return false;

            string scheme = src.Scheme;

            if (scheme.Equals("about", StringComparison.OrdinalIgnoreCase))
                return false;

            if (scheme.Equals("data", StringComparison.OrdinalIgnoreCase))
                return false;

            if (scheme.Equals("javascript", StringComparison.OrdinalIgnoreCase))
                return false;

            if (scheme.Equals(Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(src.LocalPath))
                    return false;

                if (!File.Exists(src.LocalPath) && !Directory.Exists(src.LocalPath))
                    return false;
            }

            target = raw;
            return true;
        }

        private void browser_SourceChanged(object sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs e)
        {
            Ui(() =>
            {
                Uri? src = TryGetBrowserSource();
                urlText.Text = src?.ToString() ?? string.Empty;
                UpdateBlankOverlayForSource(src);
            });
        }

        private void urlTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                LoadUrl(urlText.Text);
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

        private void BrowserUi(Action action)
        {
            if (!browser.IsHandleCreated)
                return;

            browser.BeginInvoke(action);
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

        private Uri? TryGetBrowserSource()
        {
            try { return browser.Source; }
            catch (Exception) { return null; }
        }

        private void UpdateBlankOverlayForSource(Uri? src)
        {
            if (blankRequested && src != null && src.IsAbsoluteUri && !IsAboutBlank(src))
                blankRequested = false;

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

        private Control? promptOverlayControl;
        private const string DragFmt_GameEntity = "datinate.app.drag.GameEntity";

        private void AttachDragDropPrompt()
        {
            var cards = new[]
            {
                new DragPromptOverlayRenderer.OverlayCardSpec(
                    "Drag an Entity here to Search",
                    "Drop from " + autoSource + " → Here",
                    [Resources.search_game_icon],
                    Drag: new DragPromptOverlayRenderer.OverlayDragHandlers(
                        DragEnter: (s, e) =>
                        {
                            e.Effect = dragDataValid ? DragDropEffects.Move : DragDropEffects.None;
                        },
                        DragOver: (s, e) =>
                        {
                            e.Effect = dragDataValid ? DragDropEffects.Move : DragDropEffects.None;
                        },
                        DragLeave: (s, e) => { },
                        DragDrop: (s, e) =>
                        {
                            if (dragDataValid && pendingPayload?.Entity is not null )
                                SearchEntityNameEvt?.Invoke(DatinateHelper.GetGameEntityName(pendingPayload.Entity) ?? string.Empty);
                        }
                    ), Color.LightBlue
                ),
                new DragPromptOverlayRenderer.OverlayCardSpec(
                    "Drag an Entity here to Preview Media",
                    "Drop from " + autoSource+ " → Here",
                    [Resources.media_icons_Family, Resources.media_icons_Game, Resources.media_icons_GamePart],
                    Drag: new DragPromptOverlayRenderer.OverlayDragHandlers(
                        DragEnter: (s, e) =>
                        {
                            e.Effect = dragDataValidForMediaPreview ? DragDropEffects.Move : DragDropEffects.None;
                        },
                        DragOver: (s, e) => {
                            e.Effect = dragDataValidForMediaPreview ? DragDropEffects.Move : DragDropEffects.None;
                        },
                        DragLeave: (s, e) => { },
                        DragDrop: (s, e) =>
                        {
                            if (dragDataValidForMediaPreview && pendingPayload is not null)
                                PreviewMediaEvt?.Invoke(pendingPayload);
                        }
                    ), Color.Black
                ),
                new DragPromptOverlayRenderer.OverlayCardSpec(
                    "Drag here to Assign Media",
                    "Drop from " + autoSource+ " → Here",
                    [Resources.media_icons_Family],
                    Drag: new DragPromptOverlayRenderer.OverlayDragHandlers(
                        DragEnter: (s, e) => {
                            e.Effect = dragDataValidForMediaAssign ? DragDropEffects.Move : DragDropEffects.None;
                        },
                        DragOver: (s, e) => {
                            e.Effect = dragDataValidForMediaAssign ? DragDropEffects.Move : DragDropEffects.None;
                        },
                        DragLeave: (s, e) => { },
                        DragDrop: (s, e) =>
                        {
                            if (dragDataValidForMediaAssign && pendingPayload != null)
                            {
                                if (e.Data is DataObject dobj)
                                {
                                    Debug.WriteLine("___ web main view: WEB_BROWSER_MEDIA_ShowMedia");
                                    dobj.SetData(DatinateHelper.WEB_BROWSER_MEDIA_ShowMedia_2, true);
                                }
                                AssignMediaEvt?.Invoke(pendingPayload);
                            }
                            else Debug.WriteLine($"!!! assign: null? -> {pendingPayload == null}. family? -> {pendingPayload?.Entity is IGameFamily}");
                        }
                    ), Color.OrangeRed
                ),
                new DragPromptOverlayRenderer.OverlayCardSpec(
                    "Drag a Game Part here to View Reports",
                    "Drop from " + autoSource+ " → Here",
                    [Resources.media_icons_GamePart],
                    Drag: new DragPromptOverlayRenderer.OverlayDragHandlers(
                        DragEnter: (s, e) => {
                            e.Effect = dragDataValidForReports ? DragDropEffects.Move : DragDropEffects.None;
                        },
                        DragOver: (s, e) => {
                            e.Effect = dragDataValidForReports ? DragDropEffects.Move : DragDropEffects.None;
                        },
                        DragLeave: (s, e) => { },
                        DragDrop: (s, e) =>
                        {
                            if (dragDataValidForReports && pendingPayload?.Entity is IGamePart part)
                            {
                                Debug.WriteLine("1. do show part reports");
                                ShowGamePartReportsEvt?.Invoke(part);
                            }
                            else
                            {
                                Debug.WriteLine("1. cannot show part reports. "+dragDataValidForReports+" - "+pendingPayload);
                            }
                        }
                    ), Color.LightGreen
                )
            };

            promptOverlayControl = DragPromptOverlayRenderer.CreateOverlayStackControl(
                source: dragDropOverlay,
                cards: cards);

            dragDropOverlay.Controls.Add(promptOverlayControl);
            promptOverlayControl.BringToFront();

            ApplyPromptOverlayCardVisibility();
        }

        private bool dragDataValid => pendingPayload != null;
        private bool dragDataValidForReports => dragDataValid && pendingPayload?.Entity is IGamePart;
        private bool dragDataValidForMediaPreview => dragDataValid;
        private bool dragDataValidForMediaAssign => curationModeIsActive && dragDataValid && pendingPayload?.Entity is IGameFamily;
        private const int PromptCardIx_Search = 0;
        private const int PromptCardIx_PreviewMedia = 1;
        private const int PromptCardIx_AssignMedia = 2;
        private const int PromptCardIx_Reports = 3;
        private void SetMediaAvailability(bool value)
        {
            Ui(() =>
            {
                if (mediaIsAvailable == value)
                    return;

                mediaIsAvailable = value;
                ApplyPromptOverlayCardVisibility();
            });
        }
        private DragPromptOverlayRenderer.IOverlayCardStack? PromptStack =>
            promptOverlayControl as DragPromptOverlayRenderer.IOverlayCardStack;

        private void ApplyPromptOverlayCardVisibility()
        {

            var stack = PromptStack;
            if (stack == null)
                return;

            bool showSearch = true;
            bool showReports = !dragSessionActive || dragDataValidForReports;

            bool showPreview =
                mediaIsAvailable &&
                curationModeIsActive == false &&
                (!dragSessionActive || dragDataValidForMediaPreview);

            bool showAssign =
                mediaIsAvailable &&
                curationModeIsActive &&
                (!dragSessionActive || dragDataValidForMediaAssign);

            stack.SetCardVisible(PromptCardIx_Search, showSearch);
            stack.SetCardVisible(PromptCardIx_PreviewMedia, showPreview);
            stack.SetCardVisible(PromptCardIx_AssignMedia, showAssign);
            stack.SetCardVisible(PromptCardIx_Reports, showReports);
        }
    }
}
