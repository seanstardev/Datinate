using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Rb;
using System.ComponentModel;
using System.Text;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;
using static datinate.app.FastEntryListUI;

namespace datinate.app
{
    public partial class Media2AssignUI : UserControl
    {

        public event Action<Media2AssignUI>? DragStartEvt;
        public event Action<Media2AssignUI>? DragEndEvt;

        public event Action<RbMediaItemAssignmentUpdate>? MediaAssignmentChangeEvt;

        public event Action<ILookupSet, string>? LoadMediaCardEvt;
        public event Action<Media2AssignUI, ILookupSet, FastEntryListUI.EntryInfo, bool>? EntrySelectedEvt;
        public event Action<ILookupSet, FastEntryListUI.EntryInfo>? EntryDoubleClicked;


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? ActivelyLoadedMediaItem { get; private set; } = null;
        public ILookupSet? LookupSet => lookupSet;
        public string? SourceId => lookupSet?.Id ?? null;

        private string[] entryNames = Array.Empty<string>();
        private bool eventsHooked;

        private readonly IReadOnlyCollection<Control> DragFriendlyControls;

        private ILookupSet? lookupSet;

        private bool hoverActive;
        private Size pageCurlDefaultSize;

        private bool dragEventsHooked;
        private bool dragArmed;
        private Point dragStartPoint;

        private Control HidingOverlay;

        private bool externallySuppressed;

        private bool readOnlyMode = false;
        private bool oneOrMoreHighConfidenceMatchesFound = false;
        private bool contentPathExists = false;

        private IMediaCollection? mediaCollection = null;

        private string? lastTempHtmlPath = null;

        public Media2AssignUI()
        {
            InitializeComponent();
            DatinateHelper.HideTabs(tabControl);

            pageCurlDefaultSize = pageCurl.Size;
            pageCurl.Visible = false;

            tableLayoutPanel.Location = Point.Empty;
            tableLayoutPanel.Dock = DockStyle.Fill;

            pageCurl.BringToFront();
            
            bannerBgLeft.Visible = false;
            bannerBgRight.Visible = false;
            entryListUI.BorderStyle = BorderStyle.None;

            GeneratePageCurl();

            DragFriendlyControls = [pageCurl, mediaNameTextBox, mediaIconUI, headerPanel, datChipUI];

            HidingOverlay = new Panel()
            {
                Size = Size,
                BackColor = Color.Black,
                Visible = false
            };
            Controls.Add(HidingOverlay);
            HidingOverlay.BringToFront();

            HookDragSources();
            UpdateUiState(false);
        }

        public void SetVisibleIfViable()
        {
            if (readOnlyMode && !oneOrMoreHighConfidenceMatchesFound)
                return;
            else if (readOnlyMode && oneOrMoreHighConfidenceMatchesFound && contentPathExists)
                Visible = true;
            else
                Visible = true;
        }

        public void InitialiseUI(ILookupSet lookupSet)
        {
            Ui(() =>
            {
                UnhookListEvents();

                this.lookupSet = lookupSet;

                var radioSource = lookupSet.RadioSource;

                datChipUI.DatKey = radioSource.DatGroupEnum.ToString();
                mediaIconUI.ImageKey = radioSource.Source;

                mediaNameTextBox.Text = !string.IsNullOrWhiteSpace(radioSource.Source)
                    ? radioSource.Source.Replace("_", ": ")
                    : string.Empty;

                var friendly = DatinateHelper.GetDatFriendlyName(radioSource.Id);

                if (!string.IsNullOrWhiteSpace(friendly))
                    mediaNameTextBox.Text += " - " + friendly;

                if (string.IsNullOrWhiteSpace(lookupSet.RadioSource.ContentPath))
                {
                    BackColor = innerContainer.BackColor = Color.LightGray;
                }
                else
                {
                    BackColor = innerContainer.BackColor = radioSource.IsRadioResource
                        ? Color.LightBlue
                        : Color.FromArgb(221, 181, 178);
                }

                assignControls.SetBackColour(BackColor);

                entryNames = CopyToArray(lookupSet.EntryNames);
                entryListUI.SetEntries(entryNames);

                HookListEvents();
                SetAssignmentBannersVisible(false, false);

                SetSelectedTab(listPage);
            });
        }
        public void LoadUnpreviewableItem(string entryName, bool showMediaAssignmentControls)
        {
            ActivelyLoadedMediaItem = entryName;

            Ui(() =>
            {
                assignControls.SetEntryName(entryName);

                SetSelectedTab(mediaPage);

                miniWebUI.ShowUnavailableContent();

                if (showMediaAssignmentControls && !readOnlyMode)
                    assignControls.ShowMediaAssignmentControlsPage();
                else
                    assignControls.ShowNamePage();
            });
        }
        private void SetAssignmentBannersVisible(bool okPicVisble, bool notFoundPicVisible)
        {
            okPic.Visible = okPicVisble;
            notFoundPic.Visible = notFoundPicVisible;
            
            const int Pad = 10; 


            if (okPicVisble || notFoundPicVisible)
            {
                outerContainer.Padding = new Padding(Pad, Pad, Pad, Pad);
                bannerBgLeft.Visible = true;
                bannerBgRight.Visible = true;
            }
            else
            {
                outerContainer.Padding = new Padding(0, 0, 0, 0);
                bannerBgLeft.Visible = false;
                bannerBgRight.Visible = false;
            }
        }
        public void SetUI(
            string filterText, 
            bool readOnlyMode, 
            IMediaCollection? mediaCollection)
        {
            ActivelyLoadedMediaItem = null;

            this.readOnlyMode = readOnlyMode;
            this.mediaCollection = mediaCollection;

            Ui(() =>
            {
                UpdateUiState(false);
                GeneratePageCurl();

                assignControls.ResetUI();

                assignControls.SetAssignment(MEDIA_ASSIGNMENT_ENUM.None);
                SetAssignmentBannersVisible(false, false);

                var bestScorePercentage = entryListUI.SetEntryToScoreAgainst(filterText);
                assignControls.SetBestScore(bestScorePercentage);

                if (!readOnlyMode)
                {
                    assignControls.ShowBestScorePage();
                    SetSelectedTab(listPage);
                }
                else
                {
                    assignControls.ShowNamePage();
                    SetSelectedTab(mediaPage);
                }

                var info = entryListUI.GetSingleHighConfidenceMatch();
                oneOrMoreHighConfidenceMatchesFound = false;

                if (lookupSet != null)
                {
                    bool performBestGuess = true;

                    if (mediaCollection != null
                        && !mediaCollection.IsEmpty
                        && mediaCollection.SourceIdAssignmentDictionary.TryGetValue(lookupSet.Id, out var assignment)
                        && !readOnlyMode)
                    {
                        bool showOK = assignment.AssignmentEnum == MEDIA_ASSIGNMENT_ENUM.Assigned;
                        bool showNotFound = assignment.AssignmentEnum == MEDIA_ASSIGNMENT_ENUM.NotFound;

                        this.contentPathExists = assignment.ContentPathExists;

                        SetAssignmentBannersVisible(showOK, showNotFound);

                        if (showNotFound)
                            assignControls.SetAssignment(MEDIA_ASSIGNMENT_ENUM.NotFound);

                        if (assignment.AssignmentEnum == MEDIA_ASSIGNMENT_ENUM.Assigned && assignment.EntryName != null)
                        {
                            bool selected = entryListUI.TrySelectEntry(assignment.EntryName);

                            if (selected)
                            {
                                assignControls.SetAssignment(MEDIA_ASSIGNMENT_ENUM.Assigned);
                                var entryInfo = new EntryInfo(0, assignment.EntryName, 0, false);
                                EntrySelectedEvt?.Invoke(this, lookupSet, entryInfo, false);
                                performBestGuess = false;
                            }
                        }
                    }

                    if (info != null && performBestGuess)
                    {
                        oneOrMoreHighConfidenceMatchesFound = true;
                        EntrySelectedEvt?.Invoke(this, lookupSet, (EntryInfo)info, false);
                    }
                    else if (readOnlyMode && entryListUI.GetAllHighConfidenceMatches().Any() && performBestGuess)
                    {
                        oneOrMoreHighConfidenceMatchesFound = true;
                        EntrySelectedEvt?.Invoke(
                            this,
                            lookupSet,
                            entryListUI.GetAllHighConfidenceMatches().First(),
                            false);
                    }
                    // NOTE: Remove item visual from flowcontainer so user can't see it.
                    if (readOnlyMode && !oneOrMoreHighConfidenceMatchesFound)
                    {
                        Visible = false;
                    }
                }
            });
        }

        public void LoadMediaItemUri(string uri, string entryName, bool showMediaAssignmentControls)
        {
            ActivelyLoadedMediaItem = entryName;

            Ui(() =>
            {
                assignControls.SetEntryName(entryName);

                SetSelectedTab(mediaPage);

                miniWebUI.HideUnavailableContent();
                BeginInvoke(new Action(() => miniWebUI.LoadURI(uri)));

                if (showMediaAssignmentControls && !readOnlyMode)
                    assignControls.ShowMediaAssignmentControlsPage();
                else
                    assignControls.ShowNamePage();
            });
        }
        public void LoadMediaItemRawHtml(string rawHtml, string entryName, bool showMediaAssignmentControls)
        {
            ActivelyLoadedMediaItem = entryName;

            Ui(() =>
            {
                assignControls.SetEntryName(entryName);

                SetSelectedTab(mediaPage);

                miniWebUI.HideUnavailableContent();

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

                    System.IO.File.WriteAllText(
                        tempPath,
                        rawHtml,
                        new UTF8Encoding(false));

                    string? oldPath = lastTempHtmlPath;
                    lastTempHtmlPath = tempPath;

                    BeginInvoke(new Action(() => miniWebUI.LoadURI(new Uri(tempPath).AbsoluteUri)));

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

                if (showMediaAssignmentControls && !readOnlyMode)
                    assignControls.ShowMediaAssignmentControlsPage();
                else
                    assignControls.ShowNamePage();
            });
        }

        public void Disable()
        {
            Ui(() =>
            {
                externallySuppressed = true;
                ApplyExternalSuppression();
            });
        }

        public void PermitEnable()
        {
            Ui(() =>
            {
                externallySuppressed = false;

                ApplyExternalSuppression();
            });
        }

        internal void SetHoverState(bool isOver)
        {
            if (hoverActive == isOver)
                return;

            UpdateUiState(isOver);
        }

        internal void UpdateUiState(bool isInFocus)
        {
            hoverActive = isInFocus;

            if (hoverActive)
            {
                entryListUI.SetListInFocus(true);

                if (tabControl.SelectedTab == mediaPage)
                {
                    pageCurl.Visible = true;

                    if (!readOnlyMode)
                        assignControls.ShowMediaAssignmentControlsPage();
                }
                else
                {
                    if (!readOnlyMode)
                        assignControls.ShowListFilterPage();
                }
            }
            else
            {
                entryListUI.SetListInFocus(false);

                pageCurl.Visible = false;

                if (readOnlyMode)
                {
                    assignControls.ShowNamePage();
                }
                else
                {
                    if (tabControl.SelectedTab == mediaPage)
                        assignControls.ShowNamePage();
                    else
                        assignControls.ShowBestScorePage();
                }
            }
        }

        private void ApplyExternalSuppression()
        {
            try
            {
                miniWebUI.SetInteractionSuppressed(externallySuppressed);
            }
            catch (Exception)
            {
            }

            if (externallySuppressed)
            {
                HidingOverlay.BringToFront();
                HidingOverlay.Visible = true;
            }
            else
            {
                HidingOverlay.Visible = false;
            }
        }
        private void SetSelectedTab(TabPage page)
        {
            if (tabControl.SelectedTab == page)
                return;

            var isMediaPage = page == mediaPage;

            pageCurl.Visible = isMediaPage && hoverActive;

            var cursor = isMediaPage ? Cursors.NoMove2D : Cursors.Default;  
            
            foreach (var  c in DragFriendlyControls)
            {
                c.Cursor = cursor;
            }
            SetSelectedPage(page);
        }
        private void SetSelectedPage(TabPage page)
        {
            if (readOnlyMode)
                page = mediaPage;

            if (tabControl.SelectedTab != page)
                tabControl.SelectedTab = page;
        }

        private void OnAssignmentChange(MEDIA_ASSIGNMENT_ENUM assignmentEnum)
        {
            RbMediaItemAssignmentUpdate? assignmentItem = null;
            switch (assignmentEnum)
            {
                case MEDIA_ASSIGNMENT_ENUM.Assigned:

                    if (mediaCollection != null
                        && lookupSet != null && assignControls.EntryName is { } entryName
                        && !string.IsNullOrWhiteSpace(entryName)
                        && lookupSet.GetLookup(entryName) is { } lookup
                        && !string.IsNullOrWhiteSpace(lookup))
                    {    
                        if (okPic.Visible)
                        {
                            assignControls.SetAssignment(MEDIA_ASSIGNMENT_ENUM.None);
                            OnAssignmentChange(MEDIA_ASSIGNMENT_ENUM.None);
                        }
                        else
                        {
                            SetAssignmentBannersVisible(true, false);

                            assignmentItem = new RbMediaItemAssignmentUpdate(
                                mediaCollection.Family,
                                lookupSet.Id,
                                MEDIA_ASSIGNMENT_ENUM.Assigned,
                                entryName,
                                lookup);
                        }
                    }
                    break;

                case MEDIA_ASSIGNMENT_ENUM.None:
                    SetAssignmentBannersVisible(false, false);
                    
                    if (!readOnlyMode)
                    {
                        assignControls.ShowListFilterPage();
                        SetSelectedTab(listPage);
                    }

                    if (mediaCollection != null && lookupSet != null)
                    {
                        assignmentItem = new RbMediaItemAssignmentUpdate(
                            mediaCollection.Family,
                            lookupSet.Id,
                            MEDIA_ASSIGNMENT_ENUM.None);
                    }

                    break;

                case MEDIA_ASSIGNMENT_ENUM.NotFound:
                    
                    if (mediaCollection != null && lookupSet != null)
                    {
                        if (notFoundPic.Visible)
                        {
                            assignControls.SetAssignment(MEDIA_ASSIGNMENT_ENUM.None);
                            OnAssignmentChange(MEDIA_ASSIGNMENT_ENUM.None);
                        }
                        else
                        {
                            SetAssignmentBannersVisible(false, true);

                            assignmentItem = new RbMediaItemAssignmentUpdate(
                                mediaCollection.Family,
                                lookupSet.Id,
                                MEDIA_ASSIGNMENT_ENUM.NotFound);
                        }
                    }
                    break;
            }

            if (!readOnlyMode && assignmentItem != null)
                MediaAssignmentChangeEvt?.Invoke(assignmentItem);
        }

        private void GeneratePageCurl()
        {
            var right = pageCurl.Right;
            double factor = 0.5 + (Random.Shared.NextDouble() * 0.4);
            var w = (int)Math.Round(pageCurlDefaultSize.Width * factor);
            var h = (int)Math.Round(pageCurlDefaultSize.Height * factor);
            pageCurl.Size = new Size(w, h);
            pageCurl.Left = right - pageCurl.Width;
        }

        protected void OnDispose(bool disposing)
        {
            if (disposing)
            {
                UnhookListEvents();
                UnhookDragSources();

                lookupSet = null;
                entryNames = Array.Empty<string>();
            }
        }

        private void HookListEvents()
        {
            if (eventsHooked)
                return;

            assignControls.PerformSearchEvt += OnPerformSearch;
            assignControls.ToggleSortEvt += OnToggleSort;
            assignControls.BrowserBtnClickEvt += OnBrowserBtnClick;
            assignControls.NotFoundBtnClickEvt += OnNotFoundBtnClick;
            assignControls.AssignmentChangeEvt += OnAssignmentChange;
            entryListUI.EntrySingleClicked += EntryListUI_EntrySingleClicked;
            entryListUI.EntryDoubleClicked += EntryListUI_EntryDoubleClicked;

            eventsHooked = true;
        }

        private void UnhookListEvents()
        {
            if (!eventsHooked)
                return;

            assignControls.PerformSearchEvt -= OnPerformSearch;
            assignControls.ToggleSortEvt -= OnToggleSort;
            assignControls.BrowserBtnClickEvt -= OnBrowserBtnClick;
            assignControls.NotFoundBtnClickEvt -= OnNotFoundBtnClick;
            assignControls.AssignmentChangeEvt -= OnAssignmentChange;
            entryListUI.EntrySingleClicked -= EntryListUI_EntrySingleClicked;
            entryListUI.EntryDoubleClicked -= EntryListUI_EntryDoubleClicked;

            eventsHooked = false;
        }

        private void HookDragSources()
        {
            if (dragEventsHooked)
                return;

            foreach (var c in DragFriendlyControls)
            {
                c.MouseDown += DragSource_MouseDown;
                c.MouseMove += DragSource_MouseMove;
                c.MouseUp += DragSource_MouseUp;
            }

            dragEventsHooked = true;
        }

        private void UnhookDragSources()
        {
            if (!dragEventsHooked)
                return;

            foreach (var c in DragFriendlyControls)
            {
                c.MouseDown -= DragSource_MouseDown;
                c.MouseMove -= DragSource_MouseMove;
                c.MouseUp -= DragSource_MouseUp;
            }

            dragEventsHooked = false;
        }

        private void DragSource_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            dragArmed = true;
            dragStartPoint = PointToScreen(new Point(e.X, e.Y));
        }

        private void DragSource_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            dragArmed = false;
        }

        private void DragSource_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!dragArmed)
                return;

            if ((e.Button & MouseButtons.Left) == 0)
            {
                dragArmed = false;
                return;
            }

            var cur = PointToScreen(new Point(e.X, e.Y));
            var dx = Math.Abs(cur.X - dragStartPoint.X);
            var dy = Math.Abs(cur.Y - dragStartPoint.Y);

            if (dx < SystemInformation.DragSize.Width / 2 && dy < SystemInformation.DragSize.Height / 2)
                return;

            dragArmed = false;
            BeginDragDrop();
        }

        private void BeginDragDrop()
        {
            if (lookupSet == null)
                return;

            if (tabControl.SelectedTab != mediaPage)
                return;

            var entryName = assignControls.EntryName;
            if (string.IsNullOrWhiteSpace(entryName))
                entryName = string.Empty;

            var data = new DataObject();
            data.SetData(typeof(Media2AssignUI), this);

            DragStartEvt?.Invoke(this);

            DragDropEffects effect = DragDropEffects.None;

            assignControls.ShowNamePage();
            pageCurl.Visible = false;

            // NOTE: Must momentarily enable component visibility to create drag preview.
            HidingOverlay.Visible = false;
            DragDropPreviewForm.Initialise(this);

            HidingOverlay.BringToFront();
            HidingOverlay.Visible = true;

            try
            {
                effect = DoDragDrop(data, DragDropEffects.Copy | DragDropEffects.Move);
            }
            finally
            {
                HidingOverlay.Visible = false;
                DragDropPreviewForm.Teardown();

                DragEndEvt?.Invoke(this);

                if (effect != DragDropEffects.None)
                {
                    if (!string.IsNullOrWhiteSpace(entryName))
                        LoadMediaCardEvt?.Invoke(lookupSet, entryName);
                }
            }
        }

        private void OnToggleSort()
        {
            entryListUI.ToggleSort();
        }

        private void OnPerformSearch(string searchText)
        {
            assignControls.ShowListFilterPage();
            SetSelectedTab(listPage);

            searchText ??= string.Empty;
            entryListUI.FilterByText(searchText);
        }

        private void OnNotFoundBtnClick()
        {
            assignControls.SetAssignment(MEDIA_ASSIGNMENT_ENUM.NotFound);
            OnAssignmentChange(MEDIA_ASSIGNMENT_ENUM.NotFound);
        }

        private void EntryListUI_EntrySingleClicked(FastEntryListUI.EntryInfo info)
        {
            if (lookupSet == null)
                return;

            EntrySelectedEvt?.Invoke(this, lookupSet, info, true);
        }

        private void EntryListUI_EntryDoubleClicked(FastEntryListUI.EntryInfo info)
        {
            var ls = lookupSet;
            if (ls == null)
                return;

            EntryDoubleClicked?.Invoke(ls, info);
        }

        private static string[] CopyToArray(IReadOnlyCollection<string> values)
        {
            if (values.Count == 0)
                return Array.Empty<string>();

            var arr = new string[values.Count];
            int i = 0;

            foreach (var s in values)
                arr[i++] = s;

            return arr;
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

        private void Media2AssignUI_MouseEnter(object sender, EventArgs e)
        {
            SetHoverState(true);
        }

        private void Media2AssignUI_MouseLeave(object sender, EventArgs e)
        {
            if (ClientRectangle.Contains(PointToClient(Control.MousePosition)))
                return;

            SetHoverState(false);
        }

        private void OnBrowserBtnClick()
        {
            if (!string.IsNullOrWhiteSpace(assignControls.EntryName) && lookupSet != null)
                LoadMediaCardEvt?.Invoke(lookupSet, assignControls.EntryName);
        }

        internal void HideMask()
        {
            Ui(() =>
            {
                HidingOverlay.Visible = false;
            });
        }

        public void SetAssignedEntriesCache(HashSet<string> entries)
        {
            entryListUI.SetAlreadyAssigned(entries);
        }
    }
}
