using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace Datinate.Shared
{
    public interface IDatSummaryView : IRContract
    {
        event Action<DatSummaryVO>? DatSelectedEvt;
        event Action? HomeClickEvt;

        void HighlightDats(DatGrouperProjectEntry[] includeDatHeadlineVOs, DatGrouperProjectEntry[] ignoreDatHeadlineVOs);
        void UpdateUnits(UnitFormatHelper.Unit unit, bool showUnitInCells, bool autoResize);
        void PopulateTable(DatSummaryVO[] datSummaries, UnitFormatHelper.Unit unit, bool showUnitInCell);
        void ClearView();
    }
}
