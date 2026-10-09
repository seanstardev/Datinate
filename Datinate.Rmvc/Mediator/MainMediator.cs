using Datinate.Rmvc.Command;
using Datinate.Shared.Rmvc;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Mediator
{
    public class MainMediator : RMediator 
    {
        private IMainView? view => (IMainView?)base.viewBase;

        public MainMediator(Type actor) : base(actor)
        {
        }
        public DAT_SCREEN_ENUM GetCurrentView()
        {
            if (view is { })
                return view.GetCurrentView();
            else 
                return DAT_SCREEN_ENUM.NOT_SET;
        }
        public void ToggleDatListView(DAT_SCREEN_ENUM datScreenEnum)
            => view?.ToggleDatListView(datScreenEnum);

        private void OnExitProjectsForm()
        {
            base.ExecuteCommand(new ExitDatGrouperProjectsCmd());
        }

        protected override void Disposing()
        {
            if (view != null)
            {
                view.ExitProjectsFormEvt -= OnExitProjectsForm;
            }
        }

        protected override void Initialsed()
        {
            if (view != null) 
            {
                view.ExitProjectsFormEvt += OnExitProjectsForm;
            }

            base.ExecuteCommand(new DelayedStartupCmd());
        }
    }
}
