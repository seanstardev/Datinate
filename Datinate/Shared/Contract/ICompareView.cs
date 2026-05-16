using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace Datinate.Shared
{
    public interface ICompareView : IRContract
    {
        event Action<DatVO>? CustomiseClickEvt;
        event Action<DatVO>? CreateDatClickEvt;
        event Action? CompareClickEvt;
        event Action? FormClosingEvt;
        event Action? ClearClickEvt;

        void SetLeft(DatVO datVO);
        void SetRight(DatVO datVO);
        void SetMiddle(DatVO datVO);
        void UpdateUnits(UnitFormatHelper.Unit unit, bool showUnitInCells);
        void ClearView();
    }
}
