using Datinate.Rmvc.Command;
using Datinate.Shared.Radio;
using Datinate.Shared.Rmvc;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Mediator
{
    public class DatGrouperControlsMediator : RMediator<IDatGrouperControlsView>
    {
        public void ClearView()
            => actor?.ClearView();
        
        public void SetDatGrouperScreenLayout(DAT_GROUPER_LAYOUT_ENUM layout)
            => actor?.SetDatGrouperScreenLayout(layout);
        
        public void SetViewForEntity(IGameEntity? entity, bool isFromAuto)
            => actor?.SetView(entity, isFromAuto);
        
        public void SetCompletionStats(long curatedParts, long totalParts)
            => actor?.SetCompletionStats(curatedParts, totalParts);
        
        private void OnShowExcludedFamilies(bool doExclude)
            => base.Execute(new SetExcludeFamiliesInViewCmd(doExclude));
        
        private void OnHideAliases(bool doRender)
            => base.Execute(new SetRenderAliasesInViewCmd(doRender));
        
        private void OnSearch(bool isFromAuto, string name)
            => base.Execute(new SetWebSearchTermsCmd(name, null, true));
        
        private void OnAssignMedia(bool isFromAuto, IGameEntity entity)
        {
            //
        }

        private void OnEnterMediaMode(bool isFromAuto)
        {
            //
        }
        private void OnExitMediaMode()
            => base.Execute(new ExitMediaModeCmd());
        
        private void OnShowPartGroupingReports(bool isFromAuto, IGamePart part)
            => base.Execute(new ShowRbPartGroupingReportsCmd(part));
        
        protected override void Initialised()
        {
            if (actor == null) return;

            actor.AssignMediaEvt += OnAssignMedia;
            actor.EnterMediaModeEvt += OnEnterMediaMode;
            actor.ShowPartGroupingReportsEvt += OnShowPartGroupingReports;
            actor.SearchEvt += OnSearch;
            actor.ExitMediaModeEvt += OnExitMediaMode;
            actor.HideAliasesEvt += OnHideAliases;
            actor.ShowExcludedFamiliesEvt += OnShowExcludedFamilies;
        }

        protected override void Disposing()
        {
            if (actor == null) return;

            actor.AssignMediaEvt -= OnAssignMedia;
            actor.EnterMediaModeEvt -= OnEnterMediaMode;
            actor.ShowPartGroupingReportsEvt -= OnShowPartGroupingReports;
            actor.SearchEvt -= OnSearch;
            actor.ExitMediaModeEvt -= OnExitMediaMode;
            actor.HideAliasesEvt -= OnHideAliases;
            actor.ShowExcludedFamiliesEvt -= OnShowExcludedFamilies;
        }
    }
}
