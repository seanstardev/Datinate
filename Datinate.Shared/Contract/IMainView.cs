using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Shared.Rmvc
{
    public interface IMainView : IRContract
    {
        event Action? ExitProjectsFormEvt;
        DatinateEnums.DAT_SCREEN_ENUM GetCurrentView();
        void ToggleDatListView(DAT_SCREEN_ENUM datScreenEnum);
    }
}
