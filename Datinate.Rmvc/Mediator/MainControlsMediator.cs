using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.Rmvc;
using RMVC;
using static Datinate.Shared.UnitFormatHelper;

namespace Datinate.Rmvc.Mediator
{
    internal class MainControlsMediator : RMediator<IMainControlsView>
    {
        public void ActivateView()
            => actor?.ActivateView();

        public void SetMainControlEnabled(DatinateEnums.MAIN_CONTROL_ENUM mainCtrlEnum, bool doSetEnabled)
            => actor?.SetMainControlEnabled(mainCtrlEnum, doSetEnabled);
        
        public void SetActiveMainView(DatinateEnums.DAT_SCREEN_ENUM currentView)
            => actor?.SetActiveMainView(currentView);

        private void OnShowDatGrouperProjectsView() 
            => base.Execute(new SetDatGrouperFormVisibleCmd());

        private void OnShowCustomiseView() 
            => base.Execute(new SetCustomiseViewVisibleCmd(true));

        private void OnShowCompareView() 
            => base.Execute(new SetCompareViewVisibleCmd(true));

        private void OnViewChange(Unit unit, bool shoInCells)  
            => base.Execute(new UpdateUnitDisplayCmd(unit, shoInCells));
        
        private void OnToggleMainView()
            => base.Execute(new ToggleMainViewCmd());

        private void OnInstallVgm()
            => base.Execute(new InstallOrUpdateVgmEngineCmd());

        protected override void Initialised()
        {
            if (actor != null)
            {
                actor.UnitViewChangeEvt += OnViewChange;
                actor.ShowCustomiseViewEvt += OnShowCustomiseView;
                actor.ShowCompareViewEvt += OnShowCompareView;
                actor.ShowDatGrouperProjectsEvt += OnShowDatGrouperProjectsView;
                actor.ToggleMainViewEvt += OnToggleMainView;
                actor.InstallVgmEvt += OnInstallVgm;
            }
        }

        protected override void Disposing()
        {
            if (actor != null)
            {
                actor.UnitViewChangeEvt -= OnViewChange;
                actor.ShowCustomiseViewEvt -= OnShowCustomiseView;
                actor.ShowCompareViewEvt -= OnShowCompareView;
                actor.ShowDatGrouperProjectsEvt -= OnShowDatGrouperProjectsView;
                actor.ToggleMainViewEvt -= OnToggleMainView;
                actor.InstallVgmEvt -= OnInstallVgm;
            }
        }
    }
}