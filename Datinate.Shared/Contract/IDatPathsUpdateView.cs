using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace Datinate.Shared
{
    public interface IDatPathsUpdateView : IRContract
    {
        event Action<DatGrouperProjectDTO>? SaveClickEvt;
        void SetView(DatGrouperProjectDTO projectVO); 
        void Hide();
    }
}
