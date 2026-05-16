using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Properties;
using Datinate.Shared;
using Datinate.Shared.Rb;

namespace datinate.app
{
    public partial class Media2View : UserControl, IMedia2View
    {
        public event Action<RbMediaItemAssignmentUpdate>? MediaAssignmentChangeEvt;
        public event Action<ILookupSet, string, bool>? ShowMediaCardEvt;
        public event Action? MediaCardDragStartEvt;
        public event Action? MediaCardDragEndEvt;

        public event Action<ILookupSet, string, bool, string>? RequestMediaContentEvt;

        public event Action? CloseMediaEvt;

        private const int SHOW_WORK_YIELD_LAYERS = 20;
        private static Image? LoadingSpinnerImage;

        private Media2AssignHoverManager? hoverManager;
        private bool disposingHandled;

        private Control dragDropOverlay;

        private IReadOnlyCollection<ILookupSet>? pendingInitialisationSets = null;
        private bool awaitingInitialisationSets = true;

        private IMediaCollection? pendingMediaCollection = null;
        private string? pendingSearchName = null;

        private bool activelyDisplayingContent = false;
        private bool readOnlyMode = false;

        private readonly Queue<Action> showWorkQueue = new();
        private int showWorkToken;
        private bool showWorkRunning;

        public Media2View()
        {
            InitializeComponent();

            Facade.RegisterActor(this);

            if (DatinatePerformanceUtil.SCROLLER_UseStock == false)
            {
#pragma warning disable CS0162 // Unreachable code detected
                ScrollbarManager2.AttachVertical(mediaContainer);
#pragma warning restore CS0162 // Unreachable code detected
            }
            DatinateHelper.SetTitleMedia(titleUI);
            dragDropOverlay = EnsureDragDropOverlay();

            Disposed += Media2View_Disposed;

            loadingSpinnerPic.SizeMode = PictureBoxSizeMode.Zoom;
            loadingSpinnerPic.BackColor = Color.Black;
            LoadingSpinnerImage ??= Image.FromStream(new MemoryStream(Resources.Loading_icon));
            loadingSpinnerPic.Image = LoadingSpinnerImage;

            closeRightBtn.Visible = false;
            TeardownView();
        }

        public void InitialiseView(IReadOnlyCollection<ILookupSet> lookupSets)
        {
            pendingInitialisationSets = lookupSets;
            awaitingInitialisationSets = false;

            if (!string.IsNullOrWhiteSpace(pendingSearchName))
            {
                ShowView(pendingSearchName, pendingMediaCollection);
            }
            pendingMediaCollection = null;
            pendingSearchName = null;
        }
        public void SetCloseBtnPosition(bool left)
        {
            if (left)
            {
                closeLeftBtn.Visible = true;
                closeRightBtn.Visible = false;
            }
            else
            {
                closeLeftBtn.Visible = false;
                closeRightBtn.Visible = true;
            }
        }
        private void ShowView(string filter, IMediaCollection? mediaCollection)
        {
            if (awaitingInitialisationSets)
            {
                pendingSearchName = filter;
                pendingMediaCollection = mediaCollection;
                return;
            }

            Ui(() =>
            {
                CancelShowWork();
                int token = showWorkToken;

                MaskUI.Visible = true;
                MaskUI.BringToFront();
                MaskUI.Update();

                dragDropOverlay.Visible = false;

                var sets = pendingInitialisationSets;
                pendingInitialisationSets = null;

                var existingUis = MediaAssignUIs;

                if (existingUis.Count > 0)
                {
                    foreach (var ui in existingUis)
                        ui.Visible = false;
                }

                EnqueueShowWork(() =>
                {
                    ScrollMediaContainerToTop();
                    MaskUI.BringToFront();
                });

                if (sets != null && sets.Count > 0)
                {
                    foreach (var lookupSet in sets)
                    {
                        EnqueueShowWork(() =>
                        {
                            var ui = new Media2AssignUI();
                            ui.Visible = false;

                            WireAssignUI(ui);

                            ui.InitialiseUI(lookupSet);

                            mediaContainer.Controls.Add(ui);

                            ui.PermitEnable();
                            ui.SetUI(filter, readOnlyMode, mediaCollection);
                            ui.SetVisibleIfViable();

                            MaskUI.BringToFront();
                        });
                    }
                }
                else
                {
                    foreach (var ui in existingUis)
                    {
                        EnqueueShowWork(() =>
                        {
                            ui.PermitEnable();
                            ui.SetUI(filter, readOnlyMode, mediaCollection);
                            ui.SetVisibleIfViable();

                            MaskUI.BringToFront();
                        });
                    }
                }

                EnqueueShowWork(() =>
                {
                    var visibleUis = VisibleMediaAssignUIs;

                    activelyDisplayingContent = visibleUis.Count > 0;

                    if (activelyDisplayingContent)
                    {
                        MaskUI.Visible = false;
                        dragDropOverlay.Visible = false;
                    }
                    else
                    {
                        if (readOnlyMode)
                        {
                            MaskUI.Visible = false;
                            dragDropOverlay.Visible = true;
                            dragDropOverlay.BringToFront();

                            MessageBox.Show(
                                this,
                                "No Media was found for this Item",
                                "Attention",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            MaskUI.Visible = true;
                            dragDropOverlay.Visible = false;
                            MaskUI.BringToFront();
                            MaskUI.Update();
                        }
                    }
                });

                EnqueueShowWorkCompletedMarker(token);
                StartShowWorkPump();
            });
        }

        private void PostWithYieldLayers(int layers, Action continuation)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            if (layers <= 0)
            {
                BeginInvoke(continuation);
                return;
            }

            BeginInvoke(new Action(() => PostWithYieldLayers(layers - 1, continuation)));
        }

        public void SetAssignedEntriesCache(IReadOnlyDictionary<string, HashSet<string>> assignedEntriesCache)
        {
            var uis = MediaAssignUIs;
            foreach (var ui in uis)
            {
                if (!string.IsNullOrWhiteSpace(ui.SourceId)
                    && assignedEntriesCache.TryGetValue(ui.SourceId, out var entries))
                {
                    ui.SetAssignedEntriesCache(entries);
                }
            }
        }

        public void ShowViewAssign(string searchName, IMediaCollection mediaCollection)
        {
            readOnlyMode = false;
            ShowView(searchName, mediaCollection);
        }
        public void ShowViewPreview(string searchName)
        {
            this.readOnlyMode = true;
            ShowView(searchName, null);
        }

        public void EmptyView()
        {
            Ui(() =>
            {
                activelyDisplayingContent = false;

                var uis = MediaAssignUIs;

                foreach (var ui in uis)
                    ui.Disable();
            });
        }
        
        public void StartReceiveMediaDrop()
        {
            Ui(() =>
            {
                var uis = MediaAssignUIs;

                dragDropOverlay.Visible =
                    uis.Count > 0;

                foreach (var ui in uis)
                    ui.Disable();
            });
        }

        public void StopReceiveMediaDrop()
        {
            Ui(() =>
            {
                var uis = MediaAssignUIs;
                var visibleUis = VisibleMediaAssignUIs;

                activelyDisplayingContent = visibleUis.Count > 0;

                foreach (var ui in uis)
                    ui.Disable();

                if (activelyDisplayingContent)
                {
                    foreach (var ui in visibleUis)
                        ui.PermitEnable();

                    dragDropOverlay.Visible = false;
                    MaskUI.Visible = false;
                    return;
                }

                if (readOnlyMode)
                {
                    dragDropOverlay.Visible = true;
                    dragDropOverlay.BringToFront();

                    MaskUI.Visible = false;
                    return;
                }

                if (uis.Count > 0)
                {
                    dragDropOverlay.Visible = false;
                    MaskUI.Visible = true;
                    MaskUI.BringToFront();
                    MaskUI.Update();
                }
                else
                {
                    dragDropOverlay.Visible = true;
                    dragDropOverlay.BringToFront();

                    MaskUI.Visible = false;
                }
            });
        }

        public void TeardownView()
        {
            Ui(() =>
            {
                TeardownViewInternal();
            });
        }

        public void SetMediaCardContentNotAvailable(
            ILookupSet lookupSet, 
            string entryName,
            bool entryWasSelectedByUser)
        {
            Ui(() =>
            {
                var uis = MediaAssignUIs;
                foreach (var ui in uis)
                {
                    if (ui.LookupSet is { } && ui.LookupSet.Id == lookupSet.Id)
                    {
                        ui.LoadUnpreviewableItem(entryName, entryWasSelectedByUser);
                        
                        return;
                    }
                }
            });
        }
        public void SetMediaCardContent(
            ILookupSet lookupSet, 
            string urlOrHtml, 
            bool isHtmlRawText, 
            bool entryWasSelectedByUser,
            string entryName)
        {
            Ui(() =>
            {
                var uis = MediaAssignUIs;
                foreach (var ui in uis)
                {
                    if (ui.LookupSet is { } &&  ui.LookupSet.Id == lookupSet.Id)
                    {
                        if (isHtmlRawText)
                            ui.LoadMediaItemRawHtml(urlOrHtml, entryName, entryWasSelectedByUser);
                        else
                            ui.LoadMediaItemUri(urlOrHtml, entryName, entryWasSelectedByUser);
                        
                        return;
                    }
                }
            });
        }
        private void AssignUI_EntrySingleClicked(
            Media2AssignUI sourceUI,
            ILookupSet lookupSet,
            FastEntryListUI.EntryInfo info,
            bool entryWasSelectedByUser)
        {
            var lookup = lookupSet.GetLookup(info.Name);

            if (string.IsNullOrWhiteSpace(lookup))
                return;

            RequestMediaContentEvt?.Invoke(lookupSet, lookup, entryWasSelectedByUser, info.Name);
        }
        private IReadOnlyCollection<Media2AssignUI> MediaAssignUIs
        {
            get
            {
                List<Media2AssignUI> list = [];

                for (int i = 0; i < mediaContainer.Controls.Count; i++)
                    if (mediaContainer.Controls[i] is Media2AssignUI ui)
                        list.Add(ui);

                return list;
            }
        }
        private IReadOnlyCollection<Media2AssignUI> VisibleMediaAssignUIs
        {
            get
            {
                List<Media2AssignUI> list = [];

                for (int i = 0; i < mediaContainer.Controls.Count; i++)
                    if (mediaContainer.Controls[i] is Media2AssignUI ui && ui.Visible)
                        list.Add(ui);

                return list;
            }
        }

        private void Media2View_Disposed(object? sender, EventArgs e)
        {
            HandleDisposing();
        }

        protected void HandleDisposing()
        {
            if (disposingHandled)
                return;

            Facade.UnregisterActor(this);

            disposingHandled = true;

            hoverManager?.Dispose();
            hoverManager = null;

            TeardownViewInternal();
        }

        private void ResetHoverManager()
        {
            hoverManager?.Dispose();
            hoverManager = new Media2AssignHoverManager(mediaContainer)
            {
                PollIntervalMs = 16,
                EnterDelayMs = 45,
                LeaveDelayMs = 220
            };
        }

        private void TeardownViewInternal()
        {
            CancelShowWork();

            activelyDisplayingContent = false;

            if (mediaContainer.IsDisposed)
                return;

            ResetHoverManager();

            var uis = MediaAssignUIs;

            foreach (var ui in uis)
                ui.Visible = false;

            mediaContainer.SuspendLayout();

            for (int i = mediaContainer.Controls.Count - 1; i >= 0; i--)
            {
                var c = mediaContainer.Controls[i];

                if (c is Media2AssignUI ui)
                    UnwireAssignUI(ui);

                c.Dispose();
            }

            mediaContainer.Controls.Clear();

            mediaContainer.ResumeLayout(performLayout: true);

            MaskUI.Visible = true;
            MaskUI.BringToFront();
            MaskUI.Update();

            dragDropOverlay.Visible = false;

            activelyDisplayingContent = false;

            awaitingInitialisationSets = true;
            pendingInitialisationSets = null;
        }

        private void WireAssignUI(Media2AssignUI ui)
        {
            ui.EntrySelectedEvt += AssignUI_EntrySingleClicked;
            ui.EntryDoubleClicked += AssignUI_EntryDoubleClicked;
            ui.LoadMediaCardEvt += AssignUI_LoadInMediaWebView;
            ui.DragStartEvt += AssignUI_DragStart;
            ui.DragEndEvt += AssignUI_DragEnd;
            ui.MediaAssignmentChangeEvt += OnMediaAssignmentChange;
        }

        private void OnMediaAssignmentChange(RbMediaItemAssignmentUpdate assignment) =>
            MediaAssignmentChangeEvt?.Invoke(assignment);

        private void UnwireAssignUI(Media2AssignUI ui)
        {
            ui.EntrySelectedEvt -= AssignUI_EntrySingleClicked;
            ui.EntryDoubleClicked -= AssignUI_EntryDoubleClicked;
            ui.LoadMediaCardEvt -= AssignUI_LoadInMediaWebView;
            ui.DragStartEvt -= AssignUI_DragStart;
            ui.DragEndEvt -= AssignUI_DragEnd;
            ui.MediaAssignmentChangeEvt -= OnMediaAssignmentChange;
        }
        private void AssignUI_DragStart(Media2AssignUI ui)
        {
            MediaCardDragStartEvt?.Invoke();
        }
        private void AssignUI_DragEnd(Media2AssignUI ui)
        {
            MediaCardDragEndEvt?.Invoke();
        }

        private void AssignUI_LoadInMediaWebView(ILookupSet set, string entryName) =>
            ShowMediaCardEvt?.Invoke(set, entryName, false);

        private void AssignUI_EntryDoubleClicked(ILookupSet lookupSet, FastEntryListUI.EntryInfo info)
        {

        }

        private void ScrollMediaContainerToTop()
        {
            if (mediaContainer == null)
                return;

            if (DatinatePerformanceUtil.SCROLLER_UseStock == false)
            {
#pragma warning disable CS0162 // Unreachable code detected
                ScrollbarManager2.ScrollToTop(mediaContainer);
                return;
#pragma warning restore CS0162 // Unreachable code detected
            }
            else
            {
#pragma warning disable CS0162 // Unreachable code detected
                mediaContainer.SuspendLayout();

                mediaContainer.AutoScrollPosition = new Point(0, 0);

                if (mediaContainer.VerticalScroll != null)
                    mediaContainer.VerticalScroll.Value = mediaContainer.VerticalScroll.Minimum;

                mediaContainer.PerformLayout();
                mediaContainer.ResumeLayout(performLayout: true);
#pragma warning restore CS0162 // Unreachable code detected
            }
        }

        private Control EnsureDragDropOverlay()
        {
            if (dragDropOverlay != null && !dragDropOverlay.IsDisposed)
                return dragDropOverlay;

            dragDropOverlay = DragPromptOverlayRenderer.CreateOverlayControlDark(
                source: dragDropContainer,
                title: "Drag a Game Family here",
                hint: "Drop to view Media Cards");

            dragDropOverlay.AllowDrop = true;

            dragDropContainer.Controls.Add(dragDropOverlay);

            dragDropOverlay.BringToFront();
            MaskUI.BringToFront();

            AttachDragDropReceiptOnly();

            return dragDropOverlay;
        }

        private void AttachDragDropReceiptOnly()
        {
            AttachReceiptHandlers(dragDropOverlay);
            dragDropOverlay.AllowDrop = true;
        }

        private void AttachReceiptHandlers(Control c)
        {
            c.AllowDrop = true;

            c.DragEnter -= Receipt_DragEnter;
            c.DragOver -= Receipt_DragOver;
            c.DragDrop -= Receipt_DragDrop;

            c.DragEnter += Receipt_DragEnter;
            c.DragOver += Receipt_DragOver;
            c.DragDrop += Receipt_DragDrop;
        }
        private void Receipt_DragEnter(object? sender, DragEventArgs e) => UpdateReceiptDragState(e);
        private void Receipt_DragOver(object? sender, DragEventArgs e) => UpdateReceiptDragState(e);

        private void Receipt_DragDrop(object? sender, DragEventArgs e)
        {
            if (!TryGetDropPayload(e.Data, out _))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if ((e.AllowedEffect & DragDropEffects.Move) != 0)
                e.Effect = DragDropEffects.Move;
            else if ((e.AllowedEffect & DragDropEffects.Copy) != 0)
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;

            try
            {
                if (e.Data is DataObject dobj)
                    dobj.SetData(DatinateHelper.WEB_BROWSER_MEDIA_ShowMedia, true);
            }
            catch (Exception)
            {
            }
        }

        private void UpdateReceiptDragState(DragEventArgs e)
        {
            if (!TryGetDropPayload(e.Data, out _))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if ((e.AllowedEffect & DragDropEffects.Move) != 0)
                e.Effect = DragDropEffects.Move;
            else if ((e.AllowedEffect & DragDropEffects.Copy) != 0)
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private static bool TryGetDropPayload(IDataObject? data, out object payload)
        {
            payload = null!;

            if (TryGetPayload<IGameEntityDataPacket>(data, out var packet))
            {
                payload = packet;
                return true;
            }

            return false;
        }

        private static bool TryGetPayload<T>(IDataObject? data, out T payload) where T : class
        {
            payload = null!;

            if (data == null)
                return false;

            try
            {
                if (data.GetDataPresent(typeof(T)) && data.GetData(typeof(T)) is T direct)
                {
                    payload = direct;
                    return true;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                foreach (var fmt in data.GetFormats())
                {
                    object? obj = null;
                    try { obj = data.GetData(fmt); }
                    catch (Exception) { }

                    if (obj is T match)
                    {
                        payload = match;
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
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

        private void closeBtn_Click(object sender, EventArgs e)
        {
            CloseMediaEvt?.Invoke();
        }

        private void CancelShowWork()
        {
            unchecked { showWorkToken++; }
            showWorkQueue.Clear();
            showWorkRunning = false;
        }

        private void EnqueueShowWork(Action action)
        {
            showWorkQueue.Enqueue(action);
        }

        private void StartShowWorkPump()
        {
            if (showWorkRunning)
                return;

            if (IsDisposed || !IsHandleCreated)
                return;

            showWorkRunning = true;

            int token = showWorkToken;
            BeginInvoke(new Action(() => PumpShowWork(token)));
        }

        private void PumpShowWork(int token)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                showWorkQueue.Clear();
                showWorkRunning = false;
                return;
            }

            if (token != showWorkToken)
                return;

            if (showWorkQueue.Count == 0)
            {
                showWorkRunning = false;
                return;
            }

            var action = showWorkQueue.Dequeue();

            try
            {
                action();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            if (IsDisposed || !IsHandleCreated)
            {
                showWorkQueue.Clear();
                showWorkRunning = false;
                return;
            }

            if (token != showWorkToken)
                return;

            if (showWorkQueue.Count == 0)
            {
                showWorkRunning = false;
                return;
            }

            PostWithYieldLayers(SHOW_WORK_YIELD_LAYERS, () => PumpShowWork(token));
        }

        private void EnqueueShowWorkCompletedMarker(int token)
        {
            EnqueueShowWork(() => ShowWorkCompleted(token));
        }

        private void ShowWorkCompleted(int token)
        {
            if (token != showWorkToken)
                return;

            if (IsDisposed || !IsHandleCreated)
                return;

            BeginInvoke(new Action(() =>
            {
                if (token != showWorkToken)
                    return;

                var visibleUIs = VisibleMediaAssignUIs;
                foreach (var ui  in visibleUIs)
                {
                    if (!string.IsNullOrWhiteSpace(ui.ActivelyLoadedMediaItem) && ui.LookupSet != null) 
                    {
                        ShowMediaCardEvt?.Invoke(ui.LookupSet, ui.ActivelyLoadedMediaItem, true);
                        break;
                    }
                }
            }));
        }
    }
}
