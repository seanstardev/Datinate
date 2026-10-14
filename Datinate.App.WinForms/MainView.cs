using Datinate.App.WinForms.View.Util;
using Datinate.Rmvc;
using Datinate.Shared;
using Datinate.Shared.Rmvc;

namespace Datinate.App.WinForms.View
{
    public partial class MainView : UserControl, IMainView 
    {
        private readonly Datinate.App.WinForms.View.Util.UiDispatcher uiDispatcher;

        public event Action? AppStartupCompleteEvt;
        public event Action? ExitProjectsFormEvt;

        public MainControlsView MainControlsView => mainControlsView;

        public DatSummaryView DatSummaryView => datSummaryView;

        public DatDetailsView DatDetailsView => datDetailsView;
        public LandingView DatPathsView => landingView;

        public MainView() 
        {
            uiDispatcher = new Datinate.App.WinForms.View.Util.UiDispatcher(this);
            InitializeComponent();
            UIHelper.PopSplitter(splitContainer);
            FormsHelper.HideTabs(tabControl);
            ToggleDatListView(DatinateEnums.DAT_SCREEN_ENUM.Landing);

            Facade.RegisterActor<IMainView>(this);
        }

        public DatinateEnums.DAT_SCREEN_ENUM GetCurrentView()
        {
            if (!uiDispatcher.CheckAccess)
                return uiDispatcher.Read(GetCurrentView);

            if (tabControl.SelectedTab == datManagerPage)
                return DatinateEnums.DAT_SCREEN_ENUM.DatManager;
            else
                return DatinateEnums.DAT_SCREEN_ENUM.Landing;
        }
        public void ToggleDatListView(DatinateEnums.DAT_SCREEN_ENUM datScreenEnum) 
        {
            if (!uiDispatcher.CheckAccess)
            {
                uiDispatcher.Run(() => ToggleDatListView(datScreenEnum));
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

        private bool delayedStartupEventInvoked = false;
        private bool rmvcStartupComplete = false;
        private bool rmvcRegisterActorComplete  = false;

        public void SetRmvcStartupComplete()
        {
            if (!uiDispatcher.CheckAccess)
            {
                uiDispatcher.Run(() => SetRmvcStartupComplete());
                return;
            }

            if (delayedStartupEventInvoked || rmvcStartupComplete) 
                return;

            rmvcStartupComplete = true;

            if (rmvcRegisterActorComplete && rmvcStartupComplete)
            {
                delayedStartupEventInvoked = true;
                AppStartupCompleteEvt?.Invoke();
            }
        }

        public void SetRmvcRegisterActorComplete()
        {
            if (!uiDispatcher.CheckAccess)
            {
                uiDispatcher.Run(() => SetRmvcRegisterActorComplete());
                return;
            }

            if (delayedStartupEventInvoked || rmvcRegisterActorComplete) 
                return;

            rmvcRegisterActorComplete = true;

            if (rmvcRegisterActorComplete && rmvcStartupComplete)
            {
                delayedStartupEventInvoked = true;
                AppStartupCompleteEvt?.Invoke();
            }
        }

        public void HandleProjectsFormHidden()
            => ExitProjectsFormEvt?.Invoke();

        protected void HandleDisposing()
        {
            Facade.UnregisterActor<IMainView>(this);
        }
    }
}

