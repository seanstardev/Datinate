using Datinate.Shared.Dat;
using RMVC;

namespace Datinate.Shared.Rmvc
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
