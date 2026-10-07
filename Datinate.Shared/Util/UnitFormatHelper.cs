namespace com.RADIO.Datinate.RMVC.Shared
{
    public class UnitFormatHelper {

        public enum Unit {B, KB, MB, GB, TB};

        private const Decimal OneKiloByte = 1024M;
        private const Decimal OneMegaByte = OneKiloByte * 1024M;
        private const Decimal OneGigaByte = OneMegaByte * 1024M;
        private const Decimal OneTeraByte = OneGigaByte * 1024M;

        public static string getColumnText(Unit unit) {
            return "Size (" + unit.ToString() + ")";
        }

        public static string Convert(UInt64 size, Unit unit, bool showUnitInCell)
        {
            decimal startSize = size;

            decimal value = unit switch
            {
                Unit.B => startSize,
                Unit.KB => startSize / OneKiloByte,
                Unit.MB => startSize / OneMegaByte,
                Unit.GB => startSize / OneGigaByte,
                Unit.TB => startSize / OneTeraByte,
                _ => startSize
            };

            int decimals = unit switch
            {
                Unit.B => 0,
                Unit.KB => 1,
                _ => 2
            };

            string text = DatinateHelper.GetReadableNumber(value, decimals);

            if (showUnitInCell)
                text += " " + unit;

            return text;
        }
    }
}
