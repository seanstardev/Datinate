using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.Dat;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Rmvc;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Mediator
{
    internal class DatSummaryMediator : RMediator<IDatSummaryView>
    {
        public void HighlightDats(
            DatGrouperProjectEntry[] includeDatHeadlineVOs, 
            DatGrouperProjectEntry[] ignoreDatHeadlineVOs) 
            => actor?.HighlightDats(
                includeDatHeadlineVOs
                , ignoreDatHeadlineVOs);
        
        public void UpdateUnitsView(UnitFormatHelper.Unit unit, bool showUnitInCells) 
            => actor?.UpdateUnits(unit, showUnitInCells, false);
        
        public void PopulateTable(DatSummaryVO [] datSummaries, UnitFormatHelper.Unit unit, bool showUnitInCell) 
            => actor?.PopulateTable(datSummaries, unit, showUnitInCell);
        
        public void ClearView()
            => actor?.ClearView();

        private void OnDatSummarySelected(DatSummaryVO summary) 
            => base.Execute(new LoadDatDetailsCmd(summary.DatFullpath));
        
        private void OnHomeEvt()
            => base.Execute(new SwitchDatViewCmd(DAT_SCREEN_ENUM.Landing));
        
        protected override void Initialised()
        {
            if (actor != null)
            {
                actor.DatSelectedEvt += OnDatSummarySelected;
                actor.HomeClickEvt += OnHomeEvt;
            }
        }

        protected override void Disposing()
        {
            if (actor != null)
            {
                actor.DatSelectedEvt -= OnDatSummarySelected;
                actor.HomeClickEvt -= OnHomeEvt;
            }
        }
    }
}
