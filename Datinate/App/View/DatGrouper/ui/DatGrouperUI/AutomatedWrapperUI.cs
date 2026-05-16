using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using System.Diagnostics;

namespace datinate.app
{
    public partial class AutomatedWrapperUI : UserControl
    {
        public event Action? InitialisedEvt;

        public event Action<ISet<string>>? DatChipsRefreshEvt;
        public event Action? ExitMediaClickEvt;
        public event Action<object?>? SelectedNodeChangedEvt;

        public event Action<string>? SearchGameNameEvt;
        public event Action<DatGrouperEntryDTO>? ShowGameMediaEvt;
        public event Action<IGameEntity>? ShowGroupingReportEvt;

        public event Action<DatGrouperEntryDTO>? GameEntityDragStartEvt;
        public event Action? GameEntityDragStopEvt;

        public Action? ClearDragTargetDisabledNodesEvt;
        public Action<IReadOnlySet<Type>>? SetDragTargetDisabledNodesEvt;

        public DatGrouperUiBase PrimaryUI => primaryUI;
        public DatGrouperUiBase SurrogateUI => surrogateUI;

        private bool isAuto = true;

        private DatGrouperUiBase ActiveDatGrouperUI =>
            primaryUI.Visible ? primaryUI : surrogateUI;

        public void SetExcludeFamiliesVisible(bool doExclude)
        {
            string? targetGameFamily = null;
            if (doExclude)
            {
                targetGameFamily = primaryUI.VisibleGameFamilyName;
                ActivateDatGrouperUI(false, targetGameFamily);
            }
            else
            {
                targetGameFamily = surrogateUI.VisibleGameFamilyName;
                ActivateDatGrouperUI(true, targetGameFamily);
            }
        }

        public IReadOnlyCollection<Control> GetControlsToActive()
        {
            return [primaryUI, surrogateUI];
        }
            
        public AutomatedWrapperUI()
        {
            InitializeComponent();

            ActivateDatGrouperUI(true, null, true);

            primaryUI.BringToFront();

            primaryUI.InitialisedEvt += OnInitialised;
        }

        public void SetView(
            IGameFamily[] gameFamilies,
            string projectName)
        {
            SetView(gameFamilies,
                projectName,
                new Dictionary<string, CurationPartReport>());
        }
        public void SetView(
            IGameFamily[] gameFamilies,
            string projectName,
            Dictionary<string, CurationPartReport> partFingerprintReportDic)
        {
            try
            {
                primaryUI.SetView(
                    gameFamilies,
                    projectName,
                    partFingerprintReportDic,
                    false,
                    isAuto);

                surrogateUI.SetView(
                    gameFamilies,
                    projectName,
                    partFingerprintReportDic,
                    true,
                    isAuto);
            }
            catch (Exception e) { Debug.WriteLine(e); }
        }

        public void SetMediaCache(IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache)
        {
            primaryUI.SetMediaCache(mediaCache);
            surrogateUI.SetMediaCache(mediaCache);
        }

        public void SetRenderAliases(bool doRender)
        {
            primaryUI.SetRenderAliases(doRender);
            surrogateUI.SetRenderAliases(doRender);
        }
        public void SetDragTargetDisabledNodes(IReadOnlySet<Type> set) =>
            ActiveDatGrouperUI.SetDisabledNodes(set);
        public void ClearDragTargetDisabledNodes() =>
            ActiveDatGrouperUI.ClearDisabledNodes();
        public void SetIsAuto(bool isAuto) =>
            this.isAuto = isAuto;

        public void SetMediaMode(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM layoutEnum)
        {
            
            if (isAuto)
            {
                switch (layoutEnum)
                {
                    case DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Auto_Assign:
                        titleContainer.BackColor = Color.Black;
                        ActiveDatGrouperUI.EnterMediaMode(false);
                        break;
                    
                    case DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Auto_ReadOnly:
                        titleContainer.BackColor = Color.Black;
                        ActiveDatGrouperUI.EnterMediaMode(true);
                        break;

                    default:
                        titleContainer.BackColor = SystemColors.Control;
                        ActiveDatGrouperUI.ExitMediaMode();
                        break;
                }
            }
            else
            {
                switch (layoutEnum)
                {
                    case DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Curated_Assign:
                        titleContainer.BackColor = Color.Black;
                        ActiveDatGrouperUI.EnterMediaMode(false);
                        break;

                    case DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.Media_Curated_ReadOnly:
                        titleContainer.BackColor = Color.Black;
                        ActiveDatGrouperUI.EnterMediaMode(true);
                        break;

                    default:
                        titleContainer.BackColor = SystemColors.Control;
                        ActiveDatGrouperUI.ExitMediaMode();
                        break;
                }
            }
        }

        public void ResetView()
        {
            primaryUI.ResetView();
            surrogateUI.ResetView();
            titleContainer.BackColor = SystemColors.Control;
            SetMediaMode(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM.NOT_SET);
        }

        public void SetModeAutomated()
        {
            SetTitle("Automated");
            primaryUI.AllowInteraction = true;
            surrogateUI.AllowInteraction = true;
        }
        public void SetModeQueued()
        {
            SetTitle("Queued");
            primaryUI.AllowInteraction = true;
            surrogateUI.AllowInteraction = true;
        }
        public void SetModeCurated()
        {
            SetTitle("Curated", true);
            primaryUI.AllowInteraction = true;
            surrogateUI.AllowInteraction = true;
        }

        private void OnInitialised()
        {
            InitialisedEvt?.Invoke();
        }
        private void SetTitle(string title, bool isCurated = false)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() => SetTitle(title, isCurated)));
                return;
            }

            if (isCurated)
                DatinateHelper.SetTitleCurated(titleUI);
            else if (title.ToLower() == "queued")
                DatinateHelper.SetTitleQueued(titleUI);
            else
                DatinateHelper.SetTitleAutomated(titleUI);
        }

        public void ActivateDatGrouperUI(
            bool activatePrimary, 
            string? targetGameFamilyName = null,
            bool forceActivation = false)
        {
            if (!forceActivation)
            {
                if (activatePrimary && ActiveDatGrouperUI == PrimaryUI)
                    return;
                else if (!activatePrimary && ActiveDatGrouperUI == SurrogateUI)
                    return;
            }

            ClearEvents(primaryUI);
            ClearEvents(surrogateUI);

            primaryUI.Visible = false;
            surrogateUI.Visible = false;

            primaryUI.PrepForToggle(null);
            surrogateUI.PrepForToggle(null);

            var active = activatePrimary ? primaryUI : surrogateUI;
            active.PrepForToggle(targetGameFamilyName);

            active.DatChipsRefreshEvt += OnDatChipsRefresh;
            active.SelectedNodeChangedEvt += OnSelectedNodeChanged;

            active.SearchGameNameEvt += OnSearchGameName;
            active.ShowGameMediaEvt += OnShowGameMedia;
            active.ShowGroupingReportEvt += OnShowGroupingReport;


            active.SetDragTargetDisabledNodesEvt += OnSetDragTargetDisabledNodes;
            active.ClearDragTargetDisabledNodesEvt += OnClearDragTargetDisabledNodes;
            active.GameEntityDragStartEvt += OnGameEntityDragStart;
            active.GameEntityDragStopEvt += OnGameEntityDragStop;

            active.Visible = true;
        }

        private void OnGameEntityDragStop() =>
            GameEntityDragStopEvt?.Invoke();

        private void OnGameEntityDragStart(DatGrouperEntryDTO dto) =>
            GameEntityDragStartEvt?.Invoke(dto);

        private void ClearEvents(DatGrouperUiBase datGrouperUI)
        {
            datGrouperUI.SearchGameNameEvt -= OnSearchGameName;
            datGrouperUI.ShowGameMediaEvt -= OnShowGameMedia;
            datGrouperUI.ShowGroupingReportEvt -= OnShowGroupingReport;

            datGrouperUI.DatChipsRefreshEvt -= OnDatChipsRefresh;
            datGrouperUI.SelectedNodeChangedEvt -= OnSelectedNodeChanged;

            datGrouperUI.SetDragTargetDisabledNodesEvt -= OnSetDragTargetDisabledNodes;
            datGrouperUI.ClearDragTargetDisabledNodesEvt -= OnClearDragTargetDisabledNodes;
            datGrouperUI.GameEntityDragStartEvt -= OnGameEntityDragStart;
            datGrouperUI.GameEntityDragStopEvt -= OnGameEntityDragStop;
        }

        private void OnShowGroupingReport(IGameEntity entity) =>
            ShowGroupingReportEvt?.Invoke(entity);

        private void OnSearchGameName(string name) =>
            SearchGameNameEvt?.Invoke(name);

        private void OnShowGameMedia(DatGrouperEntryDTO dto) =>
            ShowGameMediaEvt?.Invoke(dto);

        private void OnClearDragTargetDisabledNodes() =>
            ClearDragTargetDisabledNodesEvt?.Invoke();

        private void OnSetDragTargetDisabledNodes(IReadOnlySet<Type> set) =>
            SetDragTargetDisabledNodesEvt?.Invoke(set);

        private void OnSelectedNodeChanged(object? obj) =>
            SelectedNodeChangedEvt?.Invoke(obj);

        private void OnDatChipsRefresh(ISet<string> pointerIds) =>
            DatChipsRefreshEvt?.Invoke(pointerIds);
    }
}
