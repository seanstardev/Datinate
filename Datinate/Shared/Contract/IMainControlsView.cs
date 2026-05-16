using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.UnitFormatHelper;

namespace Datinate.Shared
{
    public interface IMainControlsView : IRContract
    {
        event Action<Unit, bool>? UnitViewChangeEvt;
        event Action? ShowCustomiseViewEvt;
        event Action? ShowCompareViewEvt;
        event Action? ShowProjectsEvt;
        event Action? ToggleMainViewEvt;
        void ActivateView();
        void SetActiveMainView(DatinateEnums.DAT_SCREEN_ENUM currentView);
        void SetMainControlEnabled(DatinateEnums.MAIN_CONTROL_ENUM mainCtrlEnum, bool doSetEnabled);
    }
}
