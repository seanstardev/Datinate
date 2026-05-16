namespace com.RADIO.Datinate.RMVC.Shared
{
    public static class LevenshteinDistanceUtil
    {
        public readonly record struct MatchResult(string? Match, int Distance, int Percent, int Index);

        public static int ComputeDistance(string s, string t, bool ignoreCase, int maxDistance = int.MaxValue)
        {
            int n = s.Length;
            int m = t.Length;

            if (n == 0)
                return m;

            if (m == 0)
                return n;

            int lenDiff = n - m;
            if (lenDiff < 0) lenDiff = -lenDiff;
            if (lenDiff > maxDistance)
                return maxDistance + 1;

            if (m > n)
            {
                var tmp = s; s = t; t = tmp;
                n = s.Length;
                m = t.Length;
            }

            int mPlus1 = m + 1;

            Span<int> prev = mPlus1 <= 512 ? stackalloc int[mPlus1] : new int[mPlus1];
            Span<int> curr = mPlus1 <= 512 ? stackalloc int[mPlus1] : new int[mPlus1];

            for (int j = 0; j <= m; j++)
                prev[j] = j;

            for (int i = 1; i <= n; i++)
            {
                curr[0] = i;

                int bestInRow = curr[0];

                char sc = s[i - 1];
                if (ignoreCase) sc = char.ToUpperInvariant(sc);

                for (int j = 1; j <= m; j++)
                {
                    char tc = t[j - 1];
                    if (ignoreCase) tc = char.ToUpperInvariant(tc);

                    int cost = sc == tc ? 0 : 1;

                    int del = prev[j] + 1;
                    int ins = curr[j - 1] + 1;
                    int sub = prev[j - 1] + cost;

                    int v = del < ins ? del : ins;
                    if (sub < v) v = sub;

                    curr[j] = v;

                    if (v < bestInRow)
                        bestInRow = v;
                }

                if (bestInRow > maxDistance)
                    return maxDistance + 1;

                var swap = prev;
                prev = curr;
                curr = swap;
            }

            return prev[m];
        }
        public static string? GetBestMatch(string targetString, string[] strings)
        {
            ArgumentNullException.ThrowIfNull(targetString);
            ArgumentNullException.ThrowIfNull(strings);

            int bestScore = int.MaxValue;
            string? bestMatch = null;

            for (int i = 0; i < strings.Length; i++)
            {
                string candidate = strings[i];
                int score = ComputeDistance(targetString, candidate, true);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestMatch = candidate;
                }
            }

            return bestMatch;
        }
        public static int ComputePercent(string s, string t, bool ignoreCase, int maxDistance = int.MaxValue)
        {
            int maxLen = s.Length >= t.Length ? s.Length : t.Length;
            if (maxLen == 0)
                return 100;

            int dist = ComputeDistance(s, t, ignoreCase, maxDistance);
            if (dist > maxDistance)
                return 0;

            int pct = (int)Math.Round(100.0 * (1.0 - (double)dist / maxLen));
            if (pct < 0) return 0;
            if (pct > 100) return 100;
            return pct;
        }

        public static void ComputePercents(string targetString, string[] strings, bool ignoreCase, int[] outputPercents)
        {
            if (outputPercents.Length != strings.Length)
                throw new ArgumentException("outputPercents length must match strings length.", nameof(outputPercents));

            int targetLen = targetString.Length;

            for (int i = 0; i < strings.Length; i++)
            {
                string cand = strings[i];
                int maxLen = targetLen >= cand.Length ? targetLen : cand.Length;

                if (maxLen == 0)
                {
                    outputPercents[i] = 100;
                    continue;
                }

                int dist = ComputeDistance(targetString, cand, ignoreCase);
                int pct = (int)Math.Round(100.0 * (1.0 - (double)dist / maxLen));

                if (pct < 0) pct = 0;
                else if (pct > 100) pct = 100;

                outputPercents[i] = pct;
            }
        }

        public static MatchResult GetBestMatchWithScore(string targetString, string[] strings, bool ignoreCase, int minPercent = 0)
        {
            if (strings.Length == 0)
                return new MatchResult(null, int.MaxValue, 0, -1);

            int bestDist = int.MaxValue;
            int bestIndex = -1;

            int targetLen = targetString.Length;

            int maxLenForThreshold = targetLen;
            for (int i = 0; i < strings.Length; i++)
            {
                int l = strings[i].Length;
                if (l > maxLenForThreshold) maxLenForThreshold = l;
            }

            int allowedDist = int.MaxValue;
            if (minPercent > 0)
            {
                if (minPercent > 100) minPercent = 100;
                allowedDist = (int)Math.Floor((1.0 - minPercent / 100.0) * maxLenForThreshold);
                if (allowedDist < 0) allowedDist = 0;
            }

            for (int i = 0; i < strings.Length; i++)
            {
                string cand = strings[i];

                int maxLen = targetLen >= cand.Length ? targetLen : cand.Length;
                if (maxLen == 0)
                    return new MatchResult(cand, 0, 100, i);

                int localAllowed = allowedDist;
                if (bestDist < localAllowed) localAllowed = bestDist;

                int dist = ComputeDistance(targetString, cand, ignoreCase, localAllowed);
                if (dist > localAllowed)
                    continue;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = i;

                    if (bestDist == 0)
                        break;
                }
            }

            if (bestIndex < 0)
                return new MatchResult(null, int.MaxValue, 0, -1);

            int bestMaxLen = targetLen >= strings[bestIndex].Length ? targetLen : strings[bestIndex].Length;
            int pct = bestMaxLen == 0 ? 100 : (int)Math.Round(100.0 * (1.0 - (double)bestDist / bestMaxLen));
            if (pct < 0) pct = 0;
            else if (pct > 100) pct = 100;

            return new MatchResult(strings[bestIndex], bestDist, pct, bestIndex);
        }
    }
}
