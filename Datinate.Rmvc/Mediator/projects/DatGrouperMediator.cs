using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Radio;
using Datinate.Shared.Rmvc;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Mediator
{
    public class DatGrouperMediator : RMediator<IDatGrouperView>
    {
        public void SetAutoView(
            IGameFamily[] families, 
            string projectName,
            Dictionary<string, CurationPartReport> partFingerprintReportDic)
            => actor?.SetAutoView(
                families, 
                projectName,
                partFingerprintReportDic);
        
        public void SetCuratedView(
            IGameFamily[] families,
            string projectName)
            => actor?.SetCuratedView(families, projectName);
        
        public void SetLocalProgress(int parts, int total, string message)
            => actor?.SetLocalProgress(parts, total, message);  
        
        public void ClearLocalProgress()
            => actor?.ClearLocalProgress();
        
        public void SetMediaCache(IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache)
            => actor?.SetMediaCache(mediaCache);

        public void SetRenderAliases(bool doRender)
            => actor?.SetRenderAliases(doRender);
        
        public void SetExcludeFamiliesVisible(bool doExclude)
            => actor?.SetExcludeFamiliesVisible(doExclude);

        public void UpdateCuratedFamilies(
            DELTA_NATURE_ENUM deltaNatureEnum,
            IReadOnlyList<IGameFamily> curatedFamiliesToAdd,
            IReadOnlyList<IGameFamily> curatedFamiliesToRemove,
            int undoCount,
            int redoCount,
            IReadOnlySet20<IGameEntity> affectedEntities)
            => actor?.UpdateCuratedFamilies(
                deltaNatureEnum,
                curatedFamiliesToAdd, 
                curatedFamiliesToRemove, 
                undoCount, 
                redoCount,
                affectedEntities);
        

        public void UpdateAutoFamilies(
            DELTA_NATURE_ENUM deltaNatureEnum,
            IReadOnlyList<IGameFamily> autoFamiliesToAdd,
            IReadOnlyList<IGameFamily> autoFamiliesToRemove,
            IReadOnlySet20<IGamePart> allCuratedAutoParts,
            int undoCount,
            int redoCount,
            IReadOnlySet20<IGameEntity> affectedEntities)
            => actor?.UpdateAutoFamilies(
                deltaNatureEnum,
                autoFamiliesToAdd, 
                autoFamiliesToRemove, 
                allCuratedAutoParts, 
                undoCount, 
                redoCount,
                affectedEntities);
        
        public void SetScreenLayout(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM layoutEnum) 
            => actor?.SetScreenLayout(layoutEnum);

        public void SetMediaMode(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM layoutEnum)
            => actor?.SetMediaMode(layoutEnum);
        
        public void ResetView() 
            => actor?.ResetView();
        
        private void OnBack() 
            => base.Execute(new LoadDatGrouperProjectsCmd(null, false, true));

        private void OnDatGrouperAction(DatinateEnums.DAT_GROUPER_ACTION_ENUM actionEnum, IGameEntity? entity)
        {
            switch(actionEnum)
            {
                case DatinateEnums.DAT_GROUPER_ACTION_ENUM.StateUndo:
                    base.Execute(new ApplyDatGrouperUndoRedoCmd(true));
                    break;
                case DatinateEnums.DAT_GROUPER_ACTION_ENUM.StateRedo:
                    base.Execute(new ApplyDatGrouperUndoRedoCmd(false));
                    break;
            }
        }

        private void OnSave()
            => base.Execute(new SaveCuratedCmd());
        
        private void OnExport()
            => base.Execute(new SetDatGrouperExportViewCmd());

        private void OnInitialised()
        {
            base.Execute(new ClearProgressCmd());
            base.Execute(new LazyLoadDatGrouperFilesCmd());
        }

        private void OnExitMedia()
            => base.Execute(new ExitMediaModeCmd());
        
        private void OnToggleStandardLayout()
            => base.Execute(new ToggleDatGrouperLayoutCmd());
        
        private void OnSearchGameName(string searchName)
            => base.Execute(new SetWebSearchTermsCmd(searchName, null, true));
        
        private void OnGameEntityDragStart(DatGrouperEntryDTO dto)
            => base.Execute(new SetGameEntityDragStartCmd(dto));
        
        private void OnGameEntityDragStop()
            => base.Execute(new SetGameEntityDragStopCmd());
        
        private void OnShowGameMedia(DatGrouperEntryDTO dto) 
            => base.Execute(new ShowMediaCollectionCmd(dto));
        
        private void OnGameEntitySelected(IGameEntity? entity, bool isFromAutoUI)
        {
            base.Execute(new SetWebSearchTermsCmd(
                DatinateFamilyHelper.GetGameEntityName(entity), null));

            base.Execute(new SetDatGrouperControlsCmd(entity, isFromAutoUI));
        }

        private void OnEditRequest(DatGrouperEditRequestDTO dto)
            => base.Execute(new RequestDatGrouperEditCmd(dto));

        private void OnCurate()
            => base.Execute(new LoadDatGrouperContentPathsCmd(true));

        protected override void Initialised()
        {
            if (actor == null) return;

            actor.InitialisedEvt += OnInitialised;

            actor.SaveEvt += OnSave;
            actor.BackEvt += OnBack;
            actor.CurateEvt += OnCurate;
            actor.ExportEvt += OnExport;

            actor.GameEntitySelectedEvt += OnGameEntitySelected;

            actor.GameEntityDragStartEvt += OnGameEntityDragStart;
            actor.GameEntityDragStopEvt += OnGameEntityDragStop;

            actor.ShowGameMediaEvt += OnShowGameMedia;
            actor.SearchGameNameEvt += OnSearchGameName;

            actor.ToggleStandardLayoutEvt += OnToggleStandardLayout;

            actor.ExitMediaEvt += OnExitMedia;

            actor.EditRequestEvt += OnEditRequest;
            actor.DatGrouperActionEvt += OnDatGrouperAction;
        }

        protected override void Disposing()
        {
            if (actor == null) return;

            actor.InitialisedEvt -= OnInitialised;

            actor.SaveEvt -= OnSave;
            actor.BackEvt -= OnBack;
            actor.CurateEvt -= OnCurate;
            actor.ExportEvt -= OnExport;

            actor.GameEntitySelectedEvt -= OnGameEntitySelected;

            actor.GameEntityDragStartEvt -= OnGameEntityDragStart;
            actor.GameEntityDragStopEvt -= OnGameEntityDragStop;

            actor.SearchGameNameEvt -= OnSearchGameName;
            actor.ShowGameMediaEvt -= OnShowGameMedia;

            actor.ToggleStandardLayoutEvt -= OnToggleStandardLayout;

            actor.ExitMediaEvt -= OnExitMedia;

            actor.EditRequestEvt -= OnEditRequest;

            actor.DatGrouperActionEvt -= OnDatGrouperAction;
        }

    }
}
