using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class UnitDisplayModel : RModel 
    {

        protected UnitFormatHelper.Unit unit = UnitFormatHelper.Unit.B;

        protected bool showUnitInCells = false;

        public UnitDisplayModel() {

        }

        public bool GetShowUnitInCells() {
            return showUnitInCells;
        }

        public void SetShowUnitInCells(bool val) {
            showUnitInCells = val;
        }

        public void SetUnit(UnitFormatHelper.Unit unit) {
            this.unit = unit;
        }
        public UnitFormatHelper.Unit GetUnit() {
            return unit;
        }
    }
}
