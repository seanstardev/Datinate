using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using RMVC;

namespace Datinate.Shared
{
    public interface ICustomListView : IRContract
    {
        event Action<DatVO?>? CreateDatClickEvt;
        event Action? FormClosingEvt;
        event Action? LoadExpressionsEvt;
        event Action<DatFilter[]>? SaveExpressionsEvt;
        void SetView(Flag[] flags, DatVO datVO, Flag[] categories);
        void ApplyExpressions(DatFilter[] expressions);
        void ClearAll();
        void Hide();
    }
}
