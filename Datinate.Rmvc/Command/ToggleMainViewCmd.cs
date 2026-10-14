using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class ToggleMainViewCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            if (base.MainMediator is { })
            {
                var currentView = base.MainMediator.GetCurrentView();

                if (currentView == DAT_SCREEN_ENUM.Landing)
                    base.MainMediator.ToggleDatListView(DAT_SCREEN_ENUM.DatManager);
                else
                    base.MainMediator.ToggleDatListView(DAT_SCREEN_ENUM.Landing);

                if (base.MainControlsMediator is { })
                {
                    base.MainControlsMediator.SetActiveMainView(currentView);
                }
            }

            return await Complete();
        }
    }
}
