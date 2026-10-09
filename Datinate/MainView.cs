using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using Datinate.Rmvc;
using Datinate.Shared;
using Datinate.Shared.Rmvc;

namespace com.RADIO.Datinate
{
    public partial class MainView : UserControl, IMainView 
    {
        public event Action? ExitProjectsFormEvt;
        public MainControlsView MainControlsView => mainControlsView;

        public DatSummaryView DatSummaryView => datSummaryView;

        public DatDetailsView DatDetailsView => datDetailsView;
        public LandingView DatPathsView => landingView;

        public MainView() 
        {
            InitializeComponent();
            UIHelper.PopSplitter(splitContainer);
            FormsHelper.HideTabs(tabControl);
            ToggleDatListView(DatinateEnums.DAT_SCREEN_ENUM.Landing);
            Facade.RegisterActor(this);
        }

        public DatinateEnums.DAT_SCREEN_ENUM GetCurrentView()
        {
            if (tabControl.SelectedTab == datManagerPage)
                return DatinateEnums.DAT_SCREEN_ENUM.DatManager;
            else
                return DatinateEnums.DAT_SCREEN_ENUM.Landing;
        }
        public void ToggleDatListView(DatinateEnums.DAT_SCREEN_ENUM datScreenEnum) 
        {

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => ToggleDatListView(datScreenEnum)));
                return;
            }
            splitContainer.Panel2Collapsed = false;
            switch (datScreenEnum) {

                case DatinateEnums.DAT_SCREEN_ENUM.DatManager:
                    tabControl.SelectedTab = datManagerPage;
                    break;

                case DatinateEnums.DAT_SCREEN_ENUM.Landing:
                    tabControl.SelectedTab = landingPage;
                    splitContainer.Panel2Collapsed = true;
                    break;
            }
        }
        public void HandleProjectsFormHidden()
            => ExitProjectsFormEvt?.Invoke();

        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }
    }
}

