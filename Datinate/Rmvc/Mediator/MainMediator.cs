using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Rmvc.Command;
using Datinate.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class MainMediator : RMediator 
    {
        private IMainView? view => (IMainView?)base.viewBase;

        public MainMediator(Type actor) : base(actor)
        {
        }
        public DatinateEnums.DAT_SCREEN_ENUM GetCurrentView()
        {
            if (view is { })
                return view.GetCurrentView();
            else 
                return DAT_SCREEN_ENUM.NOT_SET;
        }
        public void ToggleDatListView(DAT_SCREEN_ENUM datScreenEnum)
        {
            view?.ToggleDatListView(datScreenEnum);
        }

        protected override void Disposing()
        {

        }

        protected override void Initialsed()
        {
            base.ExecuteCommand(new DelayedStartupCmd());
        }

        private void OnProjectsFormHidden(object sender, EventArgs e) {
            HighlightProjectDatsCmd.Execute();
        }
    }
}
