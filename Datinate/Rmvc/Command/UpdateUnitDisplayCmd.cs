using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class UpdateUnitDisplayCmd : RCommand 
    {
        private readonly UnitFormatHelper.Unit unit;
        private readonly bool showUnitInCells;

        public UpdateUnitDisplayCmd(UnitFormatHelper.Unit unit, bool showUnitInCells) 
        {
            this.unit = unit;
            this.showUnitInCells = showUnitInCells;
        }

        protected override void Run() 
        { 
            Facade? instance = Facade.Instance;

            UnitDisplayModel? displayModel = instance?.UnitDisplayModel;
            displayModel?.SetUnit(unit);
            displayModel?.SetShowUnitInCells(showUnitInCells);

            Facade.Instance?.DatSummaryMediator?.UpdateUnitsView(unit, showUnitInCells);
            Facade.Instance?.DatDetailsMediator?.UpdateUnitsView(unit, showUnitInCells);
            Facade.Instance?.CompareMediator?.UpdateUnitsView(unit, showUnitInCells);
        }
    }
}
