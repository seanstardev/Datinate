using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class UpdateUnitDisplayCmd : CommandBase<RVoid> 
    {
        private readonly UnitFormatHelper.Unit unit;
        private readonly bool showUnitInCells;

        public UpdateUnitDisplayCmd(UnitFormatHelper.Unit unit, bool showUnitInCells) 
        {
            this.unit = unit;
            this.showUnitInCells = showUnitInCells;
        }

        protected override async Task<RVoid> Run() 
        {
            base.UnitDisplayModel?.SetUnit(unit);
            base.UnitDisplayModel?.SetShowUnitInCells(showUnitInCells);

            base.DatSummaryMediator?.UpdateUnitsView(unit, showUnitInCells);
            base.DatDetailsMediator?.UpdateUnitsView(unit, showUnitInCells);
            base.CompareMediator?.UpdateUnitsView(unit, showUnitInCells);

            return await Complete();
        }
    }
}
