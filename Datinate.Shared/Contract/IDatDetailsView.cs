using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace Datinate.Shared
{
    public interface IDatDetailsView : IRContract
    {
        event Action<DatVO>? CustomiseClickEvt;
        event Action? CompareLeftEvt;
        event Action? CompareRightEvt;
        event Action? ReadDatInDefaultAppFailEvt;
        event Action<DatVO> ShowAddToProjectEvt;

        void SetView(DatVO datVO, UnitFormatHelper.Unit unit, bool showUnitInCells);
        void UpdateUnits(UnitFormatHelper.Unit unit, bool showUnitInCells);
        void ClearView();
    }
}
