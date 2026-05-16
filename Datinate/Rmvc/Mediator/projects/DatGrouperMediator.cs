using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using Datinate.Shared;
using RadioLibCore.RadioDat;
using RMVC;
using static app.datinate.DatGrouperEditDelta;

namespace com.RADIO.Datinate.RMVC
{
    public class DatGrouperMediator : RMediator 
    {
        private IDatGrouperView? view => (IDatGrouperView?)base.viewBase;

        public DatGrouperMediator(Type actor) : base(actor)
        {
        }

        public void SetAutoView(
            IGameFamily[] families, 
            string projectName,
            Dictionary<string, CurationPartReport> partFingerprintReportDic) 
        {
            view?.SetAutoView(
                families, 
                projectName,
                partFingerprintReportDic);
        }
        public void SetCuratedView(
            IGameFamily[] families,
            string projectName)
        {
            view?.SetCuratedView(families, projectName);
        }
        public void SetLocalProgress(int parts, int total, string message)
        {
            view?.SetLocalProgress(parts, total, message);  
        }
        public void ClearLocalProgress()
        {
            view?.ClearLocalProgress();
        }

        public void SetMediaCache(IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache)
        {
            view?.SetMediaCache(mediaCache);
        }

        public void SetRenderAliases(bool doRender)
        {
            view?.SetRenderAliases(doRender);
        }

        public void SetExcludeFamiliesVisible(bool doExclude)
        {
            view?.SetExcludeFamiliesVisible(doExclude);
        }

        public void UpdateCuratedFamilies(
            DELTA_NATURE_ENUM deltaNatureEnum,
            IReadOnlyList<IGameFamily> curatedFamiliesToAdd,
            IReadOnlyList<IGameFamily> curatedFamiliesToRemove,
            int undoCount,
            int redoCount,
            IReadOnlySet<IGameEntity> affectedEntities)
        {
            view?.UpdateCuratedFamilies(
                deltaNatureEnum,
                curatedFamiliesToAdd, 
                curatedFamiliesToRemove, 
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
            view?.UpdateAutoFamilies(
                deltaNatureEnum,
                autoFamiliesToAdd, 
                autoFamiliesToRemove, 
                allCuratedAutoParts, 
                undoCount, 
                redoCount,
                affectedEntities);
        }
        public void SetScreenLayout(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM layoutEnum) =>
            view?.SetScreenLayout(layoutEnum);

        public void SetMediaMode(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM layoutEnum)
        {
            view?.SetMediaMode(layoutEnum);
        }
        public void ResetView() 
        {
            view?.ResetView();
        }

        private void OnBack() 
        {
            base.ExecuteCommand(new SetProjectsViewCmd());
        }

        protected override void Initialsed()
        {
            if (view == null) return;

            view.InitialisedEvt += OnInitialised;

            view.SaveEvt += OnSave;
            view.BackEvt += OnBack;
            view.CurateEvt += OnCurate;
            view.ExportEvt += OnExport;
            view.ConfigureEvt += OnConfigure;

            view.GameEntitySelectedEvt += OnGameEntitySelected;

            view.GameEntityDragStartEvt += OnGameEntityDragStart;
            view.GameEntityDragStopEvt += OnGameEntityDragStop;

            view.ShowGameMediaEvt += OnShowGameMedia;
            view.SearchGameNameEvt += OnSearchGameName;

            view.ToggleStandardLayoutEvt += OnToggleStandardLayout;

            view.ExitMediaEvt += OnExitMedia;

            view.EditRequestEvt += OnEditRequest;
            view.DatGrouperActionEvt += OnDatGrouperAction;
        }

        private void OnDatGrouperAction(DatinateEnums.DAT_GROUPER_ACTION_ENUM actionEnum, IGameEntity? entity)
        {
            switch(actionEnum)
            {
                case DatinateEnums.DAT_GROUPER_ACTION_ENUM.StateUndo:
                    base.ExecuteCommand(new ApplyDatGrouperUndoRedoCmd(true));
                    break;
                case DatinateEnums.DAT_GROUPER_ACTION_ENUM.StateRedo:
                    base.ExecuteCommand(new ApplyDatGrouperUndoRedoCmd(false));
                    break;
            }
        }

        private void OnSave()
        {
            base.ExecuteCommand(new SaveCuratedCmd());
        }
        private void OnExport()
        {
            base.ExecuteCommand(new SetDatGrouperExportViewCmd());
        }
        private void OnInitialised()
        {
            base.ExecuteCommand(new ClearProgressCmd());
            base.ExecuteCommand(new LazyLoadDatGrouperFilesCmd());
        }

        private void OnExitMedia()
        {
            base.ExecuteCommand(new ExitMediaModeCmd());
        }

        private void OnToggleStandardLayout()
        {
            base.ExecuteCommand(new ToggleDatGrouperLayoutCmd());
        }

        private void OnSearchGameName(string searchName)
        {
            base.ExecuteCommand(new SetWebSearchTermsCmd(searchName, null, true));
        }

        private void OnGameEntityDragStart(DatGrouperEntryDTO dto)
        {
            base.ExecuteCommand(new SetGameEntityDragStartCmd(dto));
        }

        private void OnGameEntityDragStop()
        {
            base.ExecuteCommand(new SetGameEntityDragStopCmd());
        }

        private void OnShowGameMedia(DatGrouperEntryDTO dto) 
        {
            base.ExecuteCommand(new ShowMediaCollectionCmd(dto));
        }
        private void OnGameEntitySelected(IGameEntity? entity, bool isFromAutoUI)
        {
            base.ExecuteCommand(new SetWebSearchTermsCmd(
                DatinateHelper.GetGameEntityName(entity), null));

            base.ExecuteCommand(new SetDatGrouperControlsCmd(entity, isFromAutoUI));
        }

        private void OnCurate()
        {
            base.ExecuteCommand(new LoadDatGrouperContentPathsCmd(true, false));
        }

        protected override void Disposing()
        {
            if (view == null) return;

            view.InitialisedEvt -= OnInitialised;

            view.SaveEvt -= OnSave;
            view.BackEvt -= OnBack;
            view.CurateEvt -= OnCurate;
            view.ExportEvt -= OnExport;
            view.ConfigureEvt -= OnConfigure;

            view.GameEntitySelectedEvt -= OnGameEntitySelected;

            view.GameEntityDragStartEvt -= OnGameEntityDragStart;
            view.GameEntityDragStopEvt -= OnGameEntityDragStop;

            view.SearchGameNameEvt -= OnSearchGameName;
            view.ShowGameMediaEvt -= OnShowGameMedia;

            view.ToggleStandardLayoutEvt -= OnToggleStandardLayout;

            view.ExitMediaEvt -= OnExitMedia;

            view.EditRequestEvt -= OnEditRequest;

            view.DatGrouperActionEvt -= OnDatGrouperAction;
        }

        private void OnEditRequest(DatGrouperEditRequestDTO dto)
        {
            base.ExecuteCommand(new RequestDatGrouperEditCmd(dto));
        }

        private void OnConfigure()
        {
            base.ExecuteCommand(new LoadDatGrouperContentPathsCmd(true, true));
        }
    }
}
