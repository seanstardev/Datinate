using Datinate.Shared.Dat;
using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Shared.Rmvc
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
