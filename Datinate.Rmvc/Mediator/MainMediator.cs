using Datinate.Rmvc.Command;
using Datinate.Shared.Rmvc;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Mediator
{
    public class MainMediator : RMediator<IMainView>
    {
        private static bool appStartupCompleteHandled = false;

        public DAT_SCREEN_ENUM GetCurrentView()
        {
            if (base.actor is { })
                return base.actor.GetCurrentView();
            else 
                return DAT_SCREEN_ENUM.NOT_SET;
        }

        public void ToggleDatListView(DAT_SCREEN_ENUM datScreenEnum)
            => actor?.ToggleDatListView(datScreenEnum);

        private void OnExitProjectsForm()
            => base.Execute(new ExitDatGrouperProjectsCmd());

        protected override void Disposing()
        {
            if (actor != null)
            {
                actor.ExitProjectsFormEvt -= OnExitProjectsForm;
                actor.AppStartupCompleteEvt -= OnAppStartupComplete;
            }
        }

        protected override void Initialised()
        {
            if (actor != null) 
            {
                actor.AppStartupCompleteEvt += OnAppStartupComplete;
                actor.ExitProjectsFormEvt += OnExitProjectsForm;
                actor?.SetRmvcRegisterActorComplete();
            }
        }

        private void OnAppStartupComplete()
        {
            if (actor != null)
                actor.AppStartupCompleteEvt -= OnAppStartupComplete;
            
            if (appStartupCompleteHandled)
                return;

            appStartupCompleteHandled = true;

            base.Execute(new DelayedStartupCmd());
        }
    }
}