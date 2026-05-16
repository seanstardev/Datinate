using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Shared
{
    public interface IMainView : IRContract
    {
        DatinateEnums.DAT_SCREEN_ENUM GetCurrentView();
        void ToggleDatListView(DAT_SCREEN_ENUM datScreenEnum);
    }
}
