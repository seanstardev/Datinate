using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.UnitFormatHelper;

namespace com.RADIO.Datinate.RMVC
{
    internal class MainControlsMediator : RMediator
    {
        private IMainControlsView? view => (IMainControlsView?)base.viewBase;


        public MainControlsMediator(Type actor) : base(actor)
        {
        }

        public void ActivateView()
        {
            view?.ActivateView();
        }

        public void SetMainControlEnabled(DatinateEnums.MAIN_CONTROL_ENUM mainCtrlEnum, bool doSetEnabled)
        {
            view?.SetMainControlEnabled(mainCtrlEnum, doSetEnabled);
        }
        public void SetActiveMainView(DatinateEnums.DAT_SCREEN_ENUM currentView)
            => view?.SetActiveMainView(currentView);

        private void OnShowProjectsView() =>
            base.ExecuteCommand(new SetDatGrouperFormVisibleCmd());

        private void OnShowCustomiseView() =>
            base.ExecuteCommand(new SetCustomiseViewVisibleCmd(true));

        private void OnShowCompareView() =>
            base.ExecuteCommand(new SetCompareViewVisibleCmd(true));

        private void OnViewChange(Unit unit, bool shoInCells) 
        {
            base.ExecuteCommand(new UpdateUnitDisplayCmd(unit, shoInCells));
        }
        private void OnToggleMainView()
            => base.ExecuteCommand(new ToggleMainViewCmd());

        protected override void Initialsed()
        {
            if (view != null)
            {
                view.UnitViewChangeEvt += OnViewChange;
                view.ShowCustomiseViewEvt += OnShowCustomiseView;
                view.ShowCompareViewEvt += OnShowCompareView;
                view.ShowProjectsEvt += OnShowProjectsView;
                view.ToggleMainViewEvt += OnToggleMainView;
            }
        }

        protected override void Disposing()
        {
            if (view != null)
            {
                view.UnitViewChangeEvt -= OnViewChange;
                view.ShowCustomiseViewEvt -= OnShowCustomiseView;
                view.ShowCompareViewEvt -= OnShowCompareView;
                view.ShowProjectsEvt -= OnShowProjectsView;
                view.ToggleMainViewEvt -= OnToggleMainView;
            }
        }
    }
}
