using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Shared.Rmvc
{
    public interface IDatPathsUpdateView : IRContract
    {
        event Action<DatGrouperProjectDTO>? SaveClickEvt;
        void SetView(DatGrouperProjectDTO projectVO); 
        void Hide();
    }
}
