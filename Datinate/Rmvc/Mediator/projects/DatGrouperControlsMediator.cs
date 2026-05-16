using Datinate.Shared;
using RadioLibCore.RadioDat;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class DatGrouperControlsMediator : RMediator
    {
        private IDatGrouperControlsView? view => (IDatGrouperControlsView?)base.viewBase;
        public DatGrouperControlsMediator(Type view) : base(view)
        {
        }
        public void ClearView()
        {
            view?.ClearView();
        }
        public void SetDatGrouperScreenLayout(DAT_GROUPER_LAYOUT_ENUM layout)
        {
            view?.SetDatGrouperScreenLayout(layout);
        }
        public void SetViewForEntity(IGameEntity? entity, bool isFromAuto)
        {
            view?.SetView(entity, isFromAuto);
        }
        public void SetCompletionStats(long curatedParts, long totalParts)
        {
            view?.SetCompletionStats(curatedParts, totalParts);
        }
        protected override void Initialsed()
        {
            if (view == null) return;

            view.AssignMediaEvt += OnAssignMedia;
            view.EnterMediaModeEvt += OnEnterMediaMode;
            view.ShowPartGroupingReportsEvt += OnShowPartGroupingReports;
            view.SearchEvt += OnSearch;
            view.ExitMediaModeEvt += OnExitMediaMode;
            view.HideAliasesEvt += OnHideAliases;
            view.ShowExcludedFamiliesEvt += OnShowExcludedFamilies;
        }

        private void OnShowExcludedFamilies(bool doExclude)
        {
            base.ExecuteCommand(new SetExcludeFamiliesInViewCmd(doExclude));
        }

        private void OnHideAliases(bool doRender)
        {
            base.ExecuteCommand(new SetRenderAliasesInViewCmd(doRender));
        }

        private void OnSearch(bool isFromAuto, string name)
        {
            base.ExecuteCommand(new SetWebSearchTermsCmd(name, null, true));
        }

        private void OnAssignMedia(bool isFromAuto, IGameEntity entity)
        {
            //base.ExecuteCommand(new ShowMediaViewCmd(isFromAuto, false, null));
        }

        private void OnEnterMediaMode(bool isFromAuto)
        {
            //base.ExecuteCommand(new ShowMediaViewCmd(isFromAuto, true));
        }
        private void OnExitMediaMode()
        {
            base.ExecuteCommand(new ExitMediaModeCmd());
        }
        private void OnShowPartGroupingReports(bool isFromAuto, IGamePart part)
        {
            base.ExecuteCommand(new ShowRbPartGroupingReportsCmd(part));
        }
        protected override void Disposing()
        {
            if (view == null) return;

            view.AssignMediaEvt -= OnAssignMedia;
            view.EnterMediaModeEvt -= OnEnterMediaMode;
            view.ShowPartGroupingReportsEvt -= OnShowPartGroupingReports;
            view.SearchEvt -= OnSearch;
            view.ExitMediaModeEvt -= OnExitMediaMode;
            view.HideAliasesEvt -= OnHideAliases;
            view.ShowExcludedFamiliesEvt -= OnShowExcludedFamilies;
        }
    }
}
