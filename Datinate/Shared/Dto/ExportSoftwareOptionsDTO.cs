
namespace com.RADIO.Datinate.RMVC.Shared
{
    public class ExportSoftwareOptionsDTO
    {
        public bool ExportAs1G1R { get; }
        public bool SkipScoringExemptFamilies { get; }
        public bool SkipExcludedGames { get; }
        public bool ExportM3Us { get; }
        public ExportSoftwareOptionsDTO(
            bool exportAs1G1R,
            bool skipScoringExemptFamilies,
            bool skipExcludedGames,
            bool exportM3Us)
        {
            ExportAs1G1R = exportAs1G1R;
            SkipScoringExemptFamilies = skipScoringExemptFamilies;
            SkipExcludedGames = skipExcludedGames;
            ExportM3Us = exportM3Us;
        }

        public static ExportSoftwareOptionsDTO CreateDefault()
        {
            return new ExportSoftwareOptionsDTO(false, false, false, true);
        }
    }
}
