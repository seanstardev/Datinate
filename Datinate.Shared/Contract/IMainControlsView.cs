using RMVC;
using static Datinate.Shared.UnitFormatHelper;

namespace Datinate.Shared.Rmvc
{
    public interface IMainControlsView : IRContract
    {
        event Action<Unit, bool>? UnitViewChangeEvt;
        event Action? ShowCustomiseViewEvt;
        event Action? ShowCompareViewEvt;
        event Action? ShowDatGrouperProjectsEvt;
        event Action? ToggleMainViewEvt;
        event Action? InstallVgmEvt;
        void ActivateView();
        void SetActiveMainView(DatinateEnums.DAT_SCREEN_ENUM currentView);
        void SetMainControlEnabled(DatinateEnums.MAIN_CONTROL_ENUM mainCtrlEnum, bool doSetEnabled);
    }
}
