namespace Datinate.Shared
{
    public sealed class MediaScoringSummaryDTO
    {
        private const int CompletePercent = 100;

        public int Hits { get; }
        public int Total { get; }
        public int Percent { get; }

        public bool IsPerfect => Total > 0 && Hits == Total;
        public bool IsScoringExempt { get; }

        public MediaScoringSummaryDTO(DatGrouperScoring scoring)
        {
            Total =
                scoring.ResourceDictionary.Count +
                scoring.MediaDictionary.Count;

            int hits = 0;

            foreach (var item in scoring.ResourceDictionary.Values)
            {
                if (item.HasScore)
                    hits++;
            }

            foreach (var item in scoring.MediaDictionary.Values)
            {
                if (item.HasScore)
                    hits++;
            }

            Hits = hits;
            IsScoringExempt = scoring.IsScoringExempt;

            int percent = Total == 0
                ? 0
                : (int)Math.Round(
                    Hits * (double)CompletePercent / Total,
                    MidpointRounding.AwayFromZero);

            percent = Math.Max(0, Math.Min(CompletePercent, percent));

            // Do not visually claim 100% unless scoring is genuinely complete.
            Percent = !IsPerfect && percent == CompletePercent
                ? CompletePercent - 1
                : percent;
        }
    }
}
