using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using datinate.app.ui;
using Datinate.Shared;
using RadioLibCore.RadioDat;
using static app.datinate.DatGrouperEditDelta;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public partial class DatGrouperView : UserControl, IDatGrouperView
    {
        public event Action? InitialisedEvt;

        public event Action<DatGrouperEditRequestDTO>? EditRequestEvt;
        public event Action<DAT_GROUPER_ACTION_ENUM, IGameEntity?>? DatGrouperActionEvt;

        public event Action<string>? SearchGameNameEvt;
        public event Action<DatGrouperEntryDTO>? ShowGameMediaEvt;

        public event Action<DatGrouperEntryDTO>? GameEntityDragStartEvt;
        public event Action? GameEntityDragStopEvt;

        public event Action? ToggleStandardLayoutEvt;


        public event Action? SaveEvt;
        public event Action? BackEvt;
        public event Action? CurateEvt;
        public event Action? ExportEvt;
        public event Action? ConfigureEvt;

        public event Action? ExitMediaEvt;

        public event Action<IGameEntity?, bool>? GameEntitySelectedEvt;

        private bool _mainControlsVisible = true;
        private float _mainControlsDividerHeightPx = -1f;
        private float _mainControlsHeightPx = -1f;

        private bool _pendingMainControlsRelayout;

        private bool _eventsHooked;

        private bool _automatedLayoutActive = true;

        private bool _delayLayoutRequests = true;
        private DatinateEnums.DAT_GROUPER_LAYOUT_ENUM? _pendingLayoutRequest = null;
        private DatinateEnums.DAT_GROUPER_LAYOUT_ENUM? _currentLayout = null;

        private Action? _pendingUiWork;
        private bool _isApplyingSplit;
        private bool _userAdjustedSplit;

        private bool _primedChildHandles;

        public DatGrouperView()
        {
            InitializeComponent();
            Facade.RegisterActor(this);
            datChipContainer.Visible = false;
            summaryLabel.Visible = true;

            CacheMainControlsRowHeights();

            UIHelper.PopButton(curateBtn);
            UIHelper.PopButton(saveBtn);

            UIHelper.PopSplitter(splitContainerLeftRight);
            UIHelper.PopSplitter(splitContainerMiddleRight);

            autoGrouperUI.SetIsAuto(true);
            curatedGrouperUI.SetIsAuto(false);

            curatedGrouperUI.SetModeCurated();
            autoGrouperUI.SetModeAutomated();

            DatGrouperUiBase.NodePreview = controlsView.DatGrouperNodePreview;

            HookupEvents();

            ResetView();
        }

        public void SetMediaCache(IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache)
        {
            autoGrouperUI.SetMediaCache(mediaCache);
            curatedGrouperUI.SetMediaCache(mediaCache);
        }

        public void SetRenderAliases(bool doRender)
        {
            autoGrouperUI.SetRenderAliases(doRender);
            curatedGrouperUI.SetRenderAliases(doRender);
        }

        public void SetExcludeFamiliesVisible(bool doExclude)
        {
            autoGrouperUI.SetExcludeFamiliesVisible(doExclude);
            curatedGrouperUI.SetExcludeFamiliesVisible(doExclude);
        }

        public void ResetView()
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() => ResetView()));
                return;
            }
            summaryLabel.Text = "-";
            datChipContainer.Controls.Clear();

            autoGrouperUI.ResetView();
            curatedGrouperUI.ResetView();

            btnLayoutPanel.Visible = true;
            progressBar.Visible = false;

            curateBtn.Visible = true;
            saveBtn.Visible = false;
            exportBtn.Visible = false;

            autoGrouperUI.SetModeAutomated();

            _delayLayoutRequests = false;

            SetScreenLayout(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.AutoGrouper);

            _delayLayoutRequests = true;
            _pendingLayoutRequest = null;
        }

        public void SetScreenLayout(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM layoutEnum)
        {
            if (object.Equals(layoutEnum, _currentLayout))
                return;

            if (_delayLayoutRequests)
            {
                _pendingLayoutRequest = layoutEnum;
                return;
            }

            _currentLayout = layoutEnum;

            RunOnUiThread(() =>
            {
                using var _ = new LayoutScope(
                    this,
                    splitContainerLeftRight,
                    splitContainerMiddleRight,
                    leftPanel,
                    middlePanel,
                    rightPanel,
                    webAndMainControlsContainer,
                    webMainPanel,
                    webMediaPanel,
                    mediaView,
                    mediaAssignmentView);

                ApplyMediaSurfaceVisibility(layoutEnum);

                // TODO: Lazy:
                if (layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Curated_Standard || layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Curated_WebInMiddle)
                    autoGrouperUI.SetModeQueued();

                switch (layoutEnum)
                {
                    case DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.AutoGrouper:
                        ControlManagementUtil.EnsureParent(autoGrouperUI, leftPanel, true);
                        ControlManagementUtil.EnsureParent(webAndMainControlsContainer, rightPanel, true);

                        webMainPanel.BringToFront();

                        ApplyAutomatedLayout(true);
                        SetMainControlsVisible(false);
                        break;

                    case DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Curated_WebInMiddle:
                        ControlManagementUtil.EnsureParent(autoGrouperUI, leftPanel, true);
                        ControlManagementUtil.EnsureParent(webAndMainControlsContainer, middlePanel, true);
                        ControlManagementUtil.EnsureParent(curatedGrouperUI, rightPanel, true);

                        controlsView.SetSwapColumnsButtonPosition(false);
                        webMainPanel.BringToFront();

                        ApplyAutomatedLayout(false);
                        SetMainControlsVisible(true);
                        break;

                    case DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Curated_Standard:
                        ControlManagementUtil.EnsureParent(autoGrouperUI, leftPanel, true);
                        ControlManagementUtil.EnsureParent(curatedGrouperUI, middlePanel, true);
                        ControlManagementUtil.EnsureParent(webAndMainControlsContainer, rightPanel, true);

                        controlsView.SetSwapColumnsButtonPosition(true);
                        webMainPanel.BringToFront();

                        ApplyAutomatedLayout(false);
                        SetMainControlsVisible(true);
                        break;

                    case DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Auto_Assign:
                    case DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Auto_ReadOnly:
                        ControlManagementUtil.EnsureParent(autoGrouperUI, leftPanel, true);
                        ControlManagementUtil.EnsureParent(mediaView, middlePanel, true);
                        ControlManagementUtil.EnsureParent(webAndMainControlsContainer, rightPanel, true);

                        mediaView.BringToFront();
                        webMediaPanel.BringToFront();
                        mediaAssignmentView.BringToFront();

                        ApplyAutomatedLayout(false);
                        SetMainControlsVisible(false);
                        mediaView.SetCloseBtnPosition(true);
                        break;

                    case DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Curated_Assign:
                    case DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Curated_ReadOnly:
                        ControlManagementUtil.EnsureParent(webAndMainControlsContainer, leftPanel, true);
                        ControlManagementUtil.EnsureParent(mediaView, middlePanel, true);
                        ControlManagementUtil.EnsureParent(curatedGrouperUI, rightPanel, true);

                        mediaView.BringToFront();
                        webMediaPanel.BringToFront();
                        mediaAssignmentView.BringToFront();

                        ApplyAutomatedLayout(false);
                        SetMainControlsVisible(false);
                        mediaView.SetCloseBtnPosition(false);
                        break;
                }

                SetMediaMode(layoutEnum);
            });
        }
        public void SetMediaMode(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM layoutEnum)
        {
            RunOnUiThread(() =>
            {
                autoGrouperUI.SetMediaMode(layoutEnum);
                curatedGrouperUI.SetMediaMode(layoutEnum);
            });
        }

        public void SetCuratedView(
            IGameFamily[] gameFamilies,
            string projectName)
        {
            curatedGrouperUI.SetView(
                gameFamilies,
                projectName);
        }

        public void SetAutoView(
            IGameFamily[] gameFamilies,
            string projectName,
            Dictionary<string, CurationPartReport> partFingerprintReportDic)
        {
            ResetView();

            autoGrouperUI.SetView(
                gameFamilies,
                projectName,
                partFingerprintReportDic);

            RunOnUiThread(() =>
            {
                ApplyAutomatedLayout(true);
                summaryLabel.Text = DatinateHelper.BuildSummaryText(gameFamilies);

                if (_delayLayoutRequests)
                {
                    _delayLayoutRequests = false;
                    if (_pendingLayoutRequest != null)
                        SetScreenLayout((DatinateEnums.DAT_GROUPER_LAYOUT_ENUM)_pendingLayoutRequest);

                    _pendingLayoutRequest = null;
                }
            });
        }
        private void SetMainControlsVisible(bool doMakeVisible)
        {
            CacheMainControlsRowHeights();

            if (_mainControlsVisible == doMakeVisible)
                return;

            _mainControlsVisible = doMakeVisible;

            webAndMainControlsContainer.SuspendLayout();
            try
            {
                controlsView.Visible = doMakeVisible;
                fillerX.Visible = doMakeVisible;

                if (webAndMainControlsContainer.RowStyles.Count >= 3)
                {
                    webAndMainControlsContainer.RowStyles[0].SizeType = SizeType.Percent;
                    webAndMainControlsContainer.RowStyles[0].Height = 100f;

                    webAndMainControlsContainer.RowStyles[1].SizeType = SizeType.Absolute;
                    webAndMainControlsContainer.RowStyles[1].Height = doMakeVisible ? _mainControlsDividerHeightPx : 0f;

                    webAndMainControlsContainer.RowStyles[2].SizeType = SizeType.Absolute;
                    webAndMainControlsContainer.RowStyles[2].Height = doMakeVisible ? _mainControlsHeightPx : 0f;
                }
            }
            finally
            {
                webAndMainControlsContainer.ResumeLayout(true);
            }

            if (!IsHandleCreated || IsDisposed)
                return;

            if (_pendingMainControlsRelayout)
                return;

            _pendingMainControlsRelayout = true;

            BeginInvoke(new Action(() =>
            {
                _pendingMainControlsRelayout = false;

                if (IsDisposed)
                    return;

                webAndMainControlsContainer.PerformLayout();
                webAndMainControlsContainer.Invalidate(true);
            }));
        }

        private void ApplyMediaSurfaceVisibility(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM layoutEnum)
        {
            bool isMedia = layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Auto_Assign
                           || layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Curated_Assign
                           || layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Auto_ReadOnly
                           || layoutEnum == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Curated_ReadOnly;

            mediaView.Visible = isMedia;
            webMediaPanel.Visible = isMedia;
            mediaAssignmentView.Visible = isMedia;

            webMainPanel.Visible = !isMedia;
        }

        private void HookupEvents()
        {
            if (_eventsHooked)
                return;

            _eventsHooked = true;

            autoGrouperUI.InitialisedEvt += () => InitialisedEvt?.Invoke();

            autoGrouperUI.ExitMediaClickEvt += () => ExitMediaEvt?.Invoke();
            curatedGrouperUI.ExitMediaClickEvt += () => ExitMediaEvt?.Invoke();

            backBtn.Click += (_, __) =>
            {
                if (_currentLayout != null && DatinateHelper.IsMediaLayout((DatinateEnums.DAT_GROUPER_LAYOUT_ENUM)_currentLayout))
                {
                    ExitMediaEvt?.Invoke();
                    return;
                }

                bool proceed = false;

                if (CurrentLayoutIsAuto)
                    proceed = true;
                else
                {
                    proceed = DatinateHelper.ShowDialogYesNo(
                        "Any unsaved changes will be lost if you return to the Projects View. Do you wish to proceed?");
                }
                if (proceed)
                {
                    // NOTE: Do not remove: stops curated UI appearing when auto grouper layout is set again later.
                    SetScreenLayout(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.AutoGrouper);
                    BackEvt?.Invoke();
                }
            };

            cfgBtn.Click += (_, __) =>
            {
                bool proceed = false;

                if (CurrentLayoutIsAuto)
                    proceed = true;
                else
                {
                    proceed = DatinateHelper.ShowDialogYesNo(
                        "Any unsaved changes will be lost if you visit the Project Settings View. Do you wish to proceed?");
                }
                if (proceed)
                {
                    SetScreenLayout(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.AutoGrouper);
                    ConfigureEvt?.Invoke();
                }
            };

            curateBtn.Click += (_, __) =>
            {
                curateBtn.Visible = false;
                saveBtn.Visible = true;
                exportBtn.Visible = true;
                CurateEvt?.Invoke();
            };

            exportBtn.Click += (_, __) =>
            {
                bool proceed = false;

                if (CurrentLayoutIsAuto)
                    return;
                else
                {
                    proceed = DatinateHelper.ShowDialogYesNo(
                        "Any unsaved changes will be lost if you visit the Export View. Do you wish to proceed?");
                }
                if (proceed)
                {
                    SetScreenLayout(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.AutoGrouper);
                    ExportEvt?.Invoke();
                }
            };

            saveBtn.Click += (_, __) => SaveEvt?.Invoke();

            autoGrouperUI.DatChipsRefreshEvt += pointerIds => OnDatChipsRefresh(pointerIds);

            autoGrouperUI.SelectedNodeChangedEvt += (nodeTag) =>
            {
                GameEntitySelectedEvt?.Invoke(nodeTag as IGameEntity, true);
            };

            autoGrouperUI.ClearDragTargetDisabledNodesEvt += () =>
                curatedGrouperUI.ClearDragTargetDisabledNodes();

            autoGrouperUI.SetDragTargetDisabledNodesEvt += set =>
                curatedGrouperUI.SetDragTargetDisabledNodes(set);

            curatedGrouperUI.SelectedNodeChangedEvt += (nodeTag) =>
            {
                GameEntitySelectedEvt?.Invoke(nodeTag as IGameEntity, false);
            };

            curatedGrouperUI.ClearDragTargetDisabledNodesEvt += () =>
                autoGrouperUI.ClearDragTargetDisabledNodes();

            curatedGrouperUI.SetDragTargetDisabledNodesEvt += set =>
                autoGrouperUI.SetDragTargetDisabledNodes(set);

            autoGrouperUI.GameEntityDragStartEvt += (dto) => GameEntityDragStartEvt?.Invoke(dto);
            autoGrouperUI.GameEntityDragStopEvt += () => GameEntityDragStopEvt?.Invoke();
            curatedGrouperUI.GameEntityDragStartEvt += (dto) => GameEntityDragStartEvt?.Invoke(dto);
            curatedGrouperUI.GameEntityDragStopEvt += () => GameEntityDragStopEvt?.Invoke();

            autoGrouperUI.ShowGameMediaEvt += dto =>
                ShowGameMediaEvt?.Invoke(dto);

            curatedGrouperUI.ShowGameMediaEvt += dto =>
                ShowGameMediaEvt?.Invoke(dto);

            autoGrouperUI.SearchGameNameEvt += searchName =>
                SearchGameNameEvt?.Invoke(searchName);

            splitContainerLeftRight.SplitterMoved += (_, __) =>
            {
                if (!_isApplyingSplit)
                    _userAdjustedSplit = true;
            };

            splitContainerMiddleRight.SplitterMoved += (_, __) =>
            {
                if (!_isApplyingSplit)
                    _userAdjustedSplit = true;
            };

            splitContainerLeftRight.SizeChanged += (_, __) => EnsureThreeWaySplitIfNeeded();
            splitContainerMiddleRight.SizeChanged += (_, __) => EnsureThreeWaySplitIfNeeded();

            controlsView.SwapGrouperColumnsEvt += () =>
                ToggleStandardLayoutEvt?.Invoke();

            autoGrouperUI.PrimaryUI.EditRequestEvt += OnEditRequest;
            autoGrouperUI.SurrogateUI.EditRequestEvt += OnEditRequest;

            curatedGrouperUI.PrimaryUI.EditRequestEvt += OnEditRequest;
            curatedGrouperUI.SurrogateUI.EditRequestEvt += OnEditRequest;

            autoGrouperUI.PrimaryUI.ToggleRenderAliasesEvt += OnToggleRenderAliases;
            autoGrouperUI.SurrogateUI.ToggleRenderAliasesEvt += OnToggleRenderAliases;
            curatedGrouperUI.PrimaryUI.ToggleRenderAliasesEvt += OnToggleRenderAliases;
            curatedGrouperUI.SurrogateUI.ToggleRenderAliasesEvt += OnToggleRenderAliases;

            autoGrouperUI.PrimaryUI.SurrogateToggleEvt += OnExcludedFamiliesShowHide;
            autoGrouperUI.SurrogateUI.SurrogateToggleEvt += OnExcludedFamiliesShowHide;
            curatedGrouperUI.PrimaryUI.SurrogateToggleEvt += OnExcludedFamiliesShowHide;
            curatedGrouperUI.SurrogateUI.SurrogateToggleEvt += OnExcludedFamiliesShowHide;

            autoGrouperUI.PrimaryUI.ActionEvt += (action, entity) => DatGrouperActionEvt?.Invoke(action, entity);
            autoGrouperUI.SurrogateUI.ActionEvt += (action, entity) => DatGrouperActionEvt?.Invoke(action, entity);
            curatedGrouperUI.PrimaryUI.ActionEvt += (action, entity) => DatGrouperActionEvt?.Invoke(action, entity);
            curatedGrouperUI.SurrogateUI.ActionEvt += (action, entity) => DatGrouperActionEvt?.Invoke(action, entity);
        }

        private void OnExcludedFamiliesShowHide(DatGrouperUiBase ui, string? familyNameToJumpTo)
        {
            autoGrouperUI.ActivateDatGrouperUI(ui.IsSurrogateUI, familyNameToJumpTo);
            curatedGrouperUI.ActivateDatGrouperUI(ui.IsSurrogateUI, familyNameToJumpTo);
        }

        private void OnToggleRenderAliases(bool doRender)
        {
            autoGrouperUI.SetRenderAliases(doRender);
            curatedGrouperUI.SetRenderAliases(doRender);
        }

        private void OnEditRequest(DatGrouperEditRequestDTO dto)
            => EditRequestEvt?.Invoke(dto);

        private void RunOnUiThread(Action action)
        {
            if (InvokeRequired)
            {
                if (IsHandleCreated)
                    BeginInvoke(action);
                else
                    _pendingUiWork = action;

                return;
            }
            action();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            CacheMainControlsRowHeights();

            BeginInvoke(new Action(() =>
            {
                var pending = _pendingUiWork;
                _pendingUiWork = null;
                pending?.Invoke();

                ApplyAutomatedLayout(_automatedLayoutActive);

                if (!_automatedLayoutActive)
                {
                    EnsureThreeWaySplit();
                }
            }));

            if (_primedChildHandles)
                return;

            _primedChildHandles = true;

            BeginInvoke(new Action(() =>
            {
                var controlsToActivate = new List<Control>();
                controlsToActivate.AddRange(autoGrouperUI.GetControlsToActive());
                controlsToActivate.AddRange(curatedGrouperUI.GetControlsToActive());

                SuspendLayout();
                try
                {
                    foreach (var ctrl in controlsToActivate)
                    {
                        if (ctrl.IsDisposed || ctrl.Parent is null)
                            continue;

                        if (ctrl.IsHandleCreated)
                            continue;

                        var wasVisible = ctrl.Visible;

                        ctrl.Visible = true;
                        var _ = ctrl.Handle;
                        ctrl.Visible = wasVisible;
                    }
                }
                finally
                {
                    ResumeLayout(false);
                }
            }));
        }

        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }

        private void OnDatChipsRefresh(ISet<string> pointerIds)
        {
            if (pointerIds.Count == 0) return;

            RunOnUiThread(() =>
            {
                if (datChipContainer.IsDisposed)
                    return;

                datChipContainer.SuspendLayout();
                try
                {
                    datChipContainer.Controls.Clear();

                    foreach (var id in pointerIds)
                    {
                        var chip = DatChipUtil.CreateChip(id);
                        
                        chip.Margin = new Padding(2);
                        chip.Enabled = false; // NOTE: Just allows click through to container.
                        
                        datChipContainer.Controls.Add(chip);
                    }
                }
                finally
                {
                    datChipContainer.ResumeLayout(true);
                    datChipContainer.Invalidate(true);
                }
            });
        }


        private void ApplyLeftRightRatio(double leftFraction)
        {
            if (leftFraction < 0.0) leftFraction = 0.0;
            if (leftFraction > 1.0) leftFraction = 1.0;

            int total = splitContainerLeftRight.ClientSize.Width - splitContainerLeftRight.SplitterWidth;
            if (total <= 0)
                return;

            int desired = (int)Math.Round(total * leftFraction);

            int min1 = splitContainerLeftRight.Panel1MinSize;
            int min2 = splitContainerLeftRight.Panel2MinSize;

            if (desired < min1) desired = min1;
            if (desired > total - min2) desired = total - min2;

            if (desired < 0) desired = 0;

            splitContainerLeftRight.SplitterDistance = desired;
        }
        private void ApplyAutomatedLayout(bool doEnable)
        {
            _automatedLayoutActive = doEnable;

            splitContainerLeftRight.SuspendLayout();
            splitContainerMiddleRight.SuspendLayout();

            try
            {
                if (doEnable)
                {
                    if (splitContainerMiddleRight.Panel1.ContainsFocus)
                    {
                        if (rightPanel.CanFocus)
                            rightPanel.Focus();
                        else if (leftPanel.CanFocus)
                            leftPanel.Focus();
                    }

                    splitContainerLeftRight.Panel2Collapsed = false;
                    splitContainerLeftRight.IsSplitterFixed = false;

                    splitContainerMiddleRight.Panel1Collapsed = true;
                    splitContainerMiddleRight.IsSplitterFixed = true;

                    Action apply = () =>
                    {
                        if (!_automatedLayoutActive)
                            return;

                        _isApplyingSplit = true;
                        try
                        {
                            ApplyLeftRightRatio(0.66);
                        }
                        finally
                        {
                            _isApplyingSplit = false;
                        }
                    };

                    if (IsHandleCreated)
                        BeginInvoke(apply);
                    else
                        _pendingUiWork += apply;
                }
                else
                {
                    splitContainerLeftRight.Panel2Collapsed = false;

                    _userAdjustedSplit = false;
                    splitContainerLeftRight.IsSplitterFixed = false;
                    splitContainerMiddleRight.IsSplitterFixed = false;

                    splitContainerMiddleRight.Panel1Collapsed = false;

                    Action apply = () =>
                    {
                        if (_automatedLayoutActive)
                            return;

                        _isApplyingSplit = true;
                        try
                        {
                            EnsureThreeWaySplit();
                        }
                        finally
                        {
                            _isApplyingSplit = false;
                        }
                    };

                    if (IsHandleCreated)
                        BeginInvoke(apply);
                    else
                        _pendingUiWork += apply;
                }
            }
            finally
            {
                splitContainerMiddleRight.ResumeLayout(true);
                splitContainerLeftRight.ResumeLayout(true);
            }
        }

        private void EnsureThreeWaySplitIfNeeded()
        {
            if (_automatedLayoutActive)
                return;

            if (_userAdjustedSplit)
                return;

            if (_isApplyingSplit)
                return;

            EnsureThreeWaySplit();
        }

        private void EnsureThreeWaySplit()
        {
            if (splitContainerLeftRight.Panel2Collapsed)
                return;

            var f = FindForm();
            if (f != null && f.WindowState == FormWindowState.Minimized)
                return;

            var leftMinTotal = splitContainerLeftRight.Panel1MinSize + splitContainerLeftRight.Panel2MinSize + splitContainerLeftRight.SplitterWidth;
            if (splitContainerLeftRight.ClientSize.Width <= leftMinTotal)
                return;

            _isApplyingSplit = true;

            try
            {
                var w = splitContainerLeftRight.ClientSize.Width;
                var leftDesired = (int)Math.Round(w / 3.0);
                splitContainerLeftRight.SplitterDistance = ClampSplitterDistance(splitContainerLeftRight, leftDesired);

                var rw = splitContainerMiddleRight.ClientSize.Width;

                var rightMinTotal = splitContainerMiddleRight.Panel1MinSize + splitContainerMiddleRight.Panel2MinSize + splitContainerMiddleRight.SplitterWidth;
                if (rw > rightMinTotal)
                {
                    var midDesired = rw / 2;
                    splitContainerMiddleRight.SplitterDistance = ClampSplitterDistance(splitContainerMiddleRight, midDesired);
                }
            }
            finally
            {
                _isApplyingSplit = false;
            }
        }
        private static int ClampSplitterDistance(SplitContainer sc, int desired)
        {
            var min = sc.Panel1MinSize;
            var max = sc.ClientSize.Width - sc.SplitterWidth - sc.Panel2MinSize;

            if (max < min)
                return sc.SplitterDistance;

            if (desired < min)
                return min;

            if (desired > max)
                return max;

            return desired;
        }
        private void CacheMainControlsRowHeights()
        {
            if (_mainControlsDividerHeightPx >= 0f && _mainControlsHeightPx >= 0f)
                return;

            if (webAndMainControlsContainer.RowStyles.Count < 3)
            {
                _mainControlsDividerHeightPx = 1f;
                _mainControlsHeightPx = 140f;
                return;
            }

            var h1 = webAndMainControlsContainer.RowStyles[1].Height;
            var h2 = webAndMainControlsContainer.RowStyles[2].Height;

            _mainControlsDividerHeightPx = h1 > 0f ? h1 : 1f;
            _mainControlsHeightPx = h2 > 0f ? h2 : 140f;
        }

        private sealed class LayoutScope : IDisposable
        {
            private readonly Control[] _controls;
            private readonly Control _root;

            public LayoutScope(Control root, params Control[] controls)
            {
                _root = root;
                _controls = controls;

                _root.SuspendLayout();
                for (int i = 0; i < _controls.Length; i++)
                    _controls[i].SuspendLayout();
            }

            public void Dispose()
            {
                for (int i = _controls.Length - 1; i >= 0; i--)
                    _controls[i].ResumeLayout(false);

                _root.ResumeLayout(false);

                for (int i = 0; i < _controls.Length; i++)
                    _controls[i].PerformLayout();

                _root.PerformLayout();

                for (int i = 0; i < _controls.Length; i++)
                    _controls[i].Invalidate(true);

                _root.Invalidate(true);

                var f = _root.FindForm();
                if (f != null)
                    ControlManagementUtil.ForceRedraw(f);
                else
                    ControlManagementUtil.ForceRedraw(_root);
            }
        }

        public void UpdateCuratedFamilies(
            DELTA_NATURE_ENUM deltaNatureEnum,
            IReadOnlyList<IGameFamily> curatedFamiliesToAdd,
            IReadOnlyList<IGameFamily> curatedFamiliesToRemove,
            int undoCount,
            int redoCount,
            IReadOnlySet<IGameEntity> affectedEntities)
        {
            curatedGrouperUI.PrimaryUI.ApplyDatGrouperDelta(
                deltaNatureEnum,
                curatedFamiliesToAdd,
                curatedFamiliesToRemove,
                new HashSet<IGamePart>(),
                undoCount,
                redoCount,
                affectedEntities);

            curatedGrouperUI.SurrogateUI.ApplyDatGrouperDelta(
                deltaNatureEnum,
                curatedFamiliesToAdd,
                curatedFamiliesToRemove,
                new HashSet<IGamePart>(),
                undoCount,
                redoCount,
                affectedEntities);
        }

        public void UpdateAutoFamilies(
            DELTA_NATURE_ENUM deltaNatureEnum,
            IReadOnlyList<IGameFamily> autoFamiliesToAdd,
            IReadOnlyList<IGameFamily> autoFamiliesToRemove,
            IReadOnlySet<IGamePart> allCuratedAutoParts,
            int undoCount,
            int redoCount,
            IReadOnlySet<IGameEntity> affectedEntities)
        {
            autoGrouperUI.PrimaryUI.ApplyDatGrouperDelta(
                deltaNatureEnum,
                autoFamiliesToAdd,
                autoFamiliesToRemove,
                allCuratedAutoParts,
                undoCount,
                redoCount,
                affectedEntities);

            autoGrouperUI.SurrogateUI.ApplyDatGrouperDelta(
                deltaNatureEnum,
                autoFamiliesToAdd,
                autoFamiliesToRemove,
                allCuratedAutoParts,
                undoCount,
                redoCount,
                affectedEntities);
        }

        public void SetLocalProgress(int parts, int total, string message)
        {
            Ui(() =>
            {
                progressBar.SetProgress(parts, total, message);
                progressBar.Visible = true;
                btnLayoutPanel.Visible = false;
            });
        }

        public void ClearLocalProgress()
        {
            Ui(() =>
            {
                progressBar.SetProgress(0, 0, null);
                progressBar.Visible = false;
                btnLayoutPanel.Visible = true;
            });
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

        private void summaryLabel_Click(object sender, EventArgs e)
        {
            datChipContainer.Visible = true;
            summaryLabel.Visible = false;
        }

        private void datChipContainer_Click(object sender, EventArgs e)
        {
            datChipContainer.Visible = false;
            summaryLabel.Visible = true;
        }

        private bool CurrentLayoutIsAuto => _currentLayout == DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.AutoGrouper;
    }
}
