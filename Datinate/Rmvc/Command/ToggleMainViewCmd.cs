using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class ToggleMainViewCmd : RCommand
    {
        protected override void Run()
        {
            if (Facade.Instance?.MainMediator is { } mainMediator)
            {
                var currentView = mainMediator.GetCurrentView();

                if (currentView == DatinateEnums.DAT_SCREEN_ENUM.Landing)
                    mainMediator.ToggleDatListView(DatinateEnums.DAT_SCREEN_ENUM.DatManager);
                else
                    mainMediator.ToggleDatListView(DatinateEnums.DAT_SCREEN_ENUM.Landing);

                if (Facade.Instance?.MainControlsMediator is { } controlsMediator)
                {
                    controlsMediator.SetActiveMainView(currentView);
                }
            }
        }
    }
}
