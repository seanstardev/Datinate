using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class ToggleMainViewCmd : RCommand
    {
        protected override void Run()
        {
            if (Facade.Instance?.MainMediator is { } mainMediator)
            {
                var currentView = mainMediator.GetCurrentView();

                if (currentView == DAT_SCREEN_ENUM.Landing)
                    mainMediator.ToggleDatListView(DAT_SCREEN_ENUM.DatManager);
                else
                    mainMediator.ToggleDatListView(DAT_SCREEN_ENUM.Landing);

                if (Facade.Instance?.MainControlsMediator is { } controlsMediator)
                {
                    controlsMediator.SetActiveMainView(currentView);
                }
            }
        }
    }
}
