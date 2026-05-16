using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal class DatSummaryMediator : RMediator 
    {
        private IDatSummaryView? view => (IDatSummaryView?)base.viewBase;

        public DatSummaryMediator(Type actor) : base(actor)
        {
        }

        public void HighlightDats(DatGrouperProjectEntry[] includeDatHeadlineVOs, DatGrouperProjectEntry[] ignoreDatHeadlineVOs) 
        {
            view?.HighlightDats(
                includeDatHeadlineVOs
                , ignoreDatHeadlineVOs
             );
        }

        public void UpdateUnitsView(UnitFormatHelper.Unit unit, bool showUnitInCells) 
        {
            view?.UpdateUnits(unit, showUnitInCells, false);
        }

        public void PopulateTable(DatSummaryVO [] datSummaries, UnitFormatHelper.Unit unit, bool showUnitInCell) 
        {
            view?.PopulateTable(datSummaries, unit, showUnitInCell);
        }

        public void ClearView() {
            view?.ClearView();
        }

        private void OnDatSummarySelected(DatSummaryVO summary) 
        {
            base.ExecuteCommand(new LoadDatDetailsCmd(summary.DatFullpath));
        }

        private void OnHomeEvt()
        {
            base.ExecuteCommand(new SwitchDatViewCmd(DAT_SCREEN_ENUM.Landing));
        }

        protected override void Initialsed()
        {
            if (view != null)
            {
                view.DatSelectedEvt += OnDatSummarySelected;
                view.HomeClickEvt += OnHomeEvt;
            }
        }

        protected override void Disposing()
        {
            if (view != null)
            {
                view.DatSelectedEvt -= OnDatSummarySelected;
                view.HomeClickEvt -= OnHomeEvt;
            }
        }
    }
}
