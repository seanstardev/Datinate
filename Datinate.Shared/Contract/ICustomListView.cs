using Datinate.Shared.Dat;
using RMVC;

namespace Datinate.Shared.Rmvc
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
