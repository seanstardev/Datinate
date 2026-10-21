using Datinate.Shared;
using System;
using System.Collections.Generic;

namespace Datinate.App.Shared.Media
{
    // Mutable presentation state for one entry list, used on its owning UI thread.
    // Backing indices always refer to the original dataset; view indices change
    // with filtering and sorting. Neither native controls nor selection live here.
    public sealed class MediaEntryListSession
    {
        private string[] entriesFlaglessUpper = Array.Empty<string>();
        private string[] lastReferenceFlaglessUppers = Array.Empty<string>();
        private readonly Dictionary<string, int> backingIndexByFull = new(StringComparer.Ordinal);
        private string[] entriesFull = Array.Empty<string>();
        private string[] entriesFlagless = Array.Empty<string>();
        private readonly Dictionary<string, string> flaglessByFull = new(StringComparer.Ordinal);
        private bool[] assigned = Array.Empty<bool>();
        private int[] viewOrder = Array.Empty<int>();
        private int[] scorePct = Array.Empty<int>();
        private int[] scratch = Array.Empty<int>();
        private bool alphaSort;
        private string lastReferenceFlagless = string.Empty;
        private int scoreGeneration = 1;
        private int[] scoreGen = Array.Empty<int>();
        private string lastFilterFull = string.Empty;
        private string lastFilterFlagless = string.Empty;
        private MediaEntryInfo? singleHighConfidenceMatch;
        private MediaEntryInfo[] highConfidenceMatches = Array.Empty<MediaEntryInfo>();

        public const int HighConfidenceThresholdPct = 100;

        public int EntryCount => entriesFull.Length;
        public int VisibleCount => viewOrder.Length;
        public MediaEntryInfo? SingleHighConfidenceMatch => singleHighConfidenceMatch;
        public IReadOnlyList<MediaEntryInfo> HighConfidenceMatches => highConfidenceMatches;

        // Raised before replacing a filtered mapping, while old indices still identify old
        // entries. A native virtual list can reconcile its size and selection here.
        public event Action<int>? ViewOrderChanging;

        public void SetEntries(string[] newEntries, HashSet<string>? alreadyAssigned = null)
        {
            entriesFull = newEntries ?? Array.Empty<string>();
            alphaSort = false;

            entriesFlagless = new string[entriesFull.Length];
            entriesFlaglessUpper = new string[entriesFull.Length];

            flaglessByFull.Clear();
            backingIndexByFull.Clear();

            for (int i = 0; i < entriesFull.Length; i++)
            {
                var full = entriesFull[i] ?? string.Empty;
                var flagless = DatinateHelper.GetFlaglessName(full) ?? string.Empty;

                entriesFlagless[i] = flagless;
                entriesFlaglessUpper[i] = flagless.ToUpperInvariant();

                flaglessByFull[full] = flagless;

                if (!backingIndexByFull.ContainsKey(full))
                    backingIndexByFull.Add(full, i);
            }

            assigned = new bool[entriesFull.Length];
            if (alreadyAssigned != null && alreadyAssigned.Count > 0)
            {
                for (int i = 0; i < entriesFull.Length; i++)
                    assigned[i] = alreadyAssigned.Contains(entriesFull[i]);
            }

            viewOrder = new int[entriesFull.Length];
            scorePct = new int[entriesFull.Length];
            scoreGen = new int[entriesFull.Length];

            scoreGeneration = 1;

            for (int i = 0; i < entriesFull.Length; i++)
            {
                viewOrder[i] = i;
                scorePct[i] = -1;
                scoreGen[i] = 0;
            }

            EnsureScratchCapacity(entriesFull.Length);

            lastReferenceFlagless = string.Empty;
            lastReferenceFlaglessUppers = Array.Empty<string>();

            lastFilterFull = string.Empty;
            lastFilterFlagless = string.Empty;

            singleHighConfidenceMatch = null;
            highConfidenceMatches = Array.Empty<MediaEntryInfo>();
        }

        public void ToggleSort() => SetSortModeShowAll(!alphaSort);

        public void FilterByText(string searchText)
        {
            alphaSort = true;

            string filterFull = searchText.Trim();
            string filterFlagless = DatinateHelper.GetFlaglessName(filterFull) ?? string.Empty;

            lastFilterFull = filterFull;
            lastFilterFlagless = filterFlagless;

            EnsureScratchCapacity(entriesFull.Length);

            int count = 0;

            if (filterFull.Length == 0 && filterFlagless.Length == 0)
            {
                for (int i = 0; i < entriesFull.Length; i++)
                    scratch[count++] = i;
            }
            else
            {
                for (int i = 0; i < entriesFull.Length; i++)
                {
                    if (filterFull.Length > 0 &&
                        entriesFull[i].IndexOf(filterFull, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        scratch[count++] = i;
                        continue;
                    }

                    if (filterFlagless.Length > 0 &&
                        entriesFlagless[i].IndexOf(filterFlagless, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        scratch[count++] = i;
                    }
                }
            }

            if (count == 0)
            {
                ReplaceViewOrder(Array.Empty<int>());
                return;
            }

            var newViewOrder = new int[count];
            Array.Copy(scratch, newViewOrder, count);

            Array.Sort(newViewOrder, CompareAlpha);

            ReplaceViewOrder(newViewOrder);
        }

        public void SetAlreadyAssigned(HashSet<string> alreadyAssigned)
        {
            Array.Clear(assigned, 0, assigned.Length);

            if (alreadyAssigned.Count > 0)
            {
                for (int i = 0; i < entriesFull.Length; i++)
                    assigned[i] = alreadyAssigned.Contains(entriesFull[i]);
            }
        }

        public int SetScoringReferences(
            IReadOnlySet20<string> textsToMatch,
            bool excludeAlreadyAssigned)
        {
            alphaSort = false;

            var references = new List<string>(textsToMatch.Count);
            var seenReferences = new HashSet<string>(StringComparer.Ordinal);

            foreach (string textToMatch in textsToMatch)
            {
                if (string.IsNullOrWhiteSpace(textToMatch))
                    continue;

                string refFull = textToMatch.Trim();
                string refFlagless = DatinateHelper.GetFlaglessName(refFull) ?? string.Empty;

                if (string.IsNullOrWhiteSpace(refFlagless))
                    refFlagless = refFull;

                if (string.IsNullOrWhiteSpace(refFlagless))
                    continue;

                string refFlaglessUpper = refFlagless.ToUpperInvariant();

                // NOTE: Different full names can reduce to the same flagless name.
                if (seenReferences.Add(refFlaglessUpper))
                    references.Add(refFlaglessUpper);
            }

            if (references.Count == 0)
            {
                SetReferenceFlagless(Array.Empty<string>(), forceReset: true);
                singleHighConfidenceMatch = null; // NOTE: clear stale.
                highConfidenceMatches = Array.Empty<MediaEntryInfo>();
                return 0;
            }

            // NOTE: Keep comparison deterministic so an unchanged reference set
            // does not invalidate the score cache merely because enumeration order changed.
            references.Sort(StringComparer.Ordinal);

            SetReferenceFlagless(references.ToArray(), forceReset: false);

            int bestScore = 0;

            for (int i = 0; i < entriesFull.Length; i++)
            {
                int s = GetScorePct(i);
                if (s > bestScore)
                    bestScore = s;
            }

            SetSortModeShowAll(alpha: false);

            IdentifyHighConfidenceMatches(excludeAlreadyAssigned);

            return bestScore;
        }

        private void SetSortModeShowAll(bool alpha)
        {
            alphaSort = alpha;

            lastFilterFull = string.Empty;
            lastFilterFlagless = string.Empty;

            int n = entriesFull.Length;

            if (viewOrder.Length != n)
                viewOrder = new int[n];

            for (int i = 0; i < n; i++)
                viewOrder[i] = i;

            if (alphaSort || string.IsNullOrWhiteSpace(lastReferenceFlagless))
            {
                Array.Sort(viewOrder, CompareAlpha);
            }
            else
            {
                EnsureScoresForViewOrder();
                Array.Sort(viewOrder, CompareScoreDescThenAlpha);
            }
        }

        private int CompareAlpha(int a, int b)
        {
            var sa = entriesFull[a];
            var sb = entriesFull[b];

            int c = StringComparer.OrdinalIgnoreCase.Compare(sa, sb);
            if (c != 0) return c;

            return StringComparer.Ordinal.Compare(sa, sb);
        }

        private int CompareScoreDescThenAlpha(int a, int b)
        {
            int pa = scorePct[a];
            int pb = scorePct[b];

            int c = pb.CompareTo(pa);
            if (c != 0) return c;

            return CompareAlpha(a, b);
        }

        private void IdentifyHighConfidenceMatches(bool excludeAlreadyAssigned)
        {
            singleHighConfidenceMatch = null;
            highConfidenceMatches = Array.Empty<MediaEntryInfo>();

            if (viewOrder.Length == 0)
            {
                return;
            }

            int countHigh = 0;

            for (int i = 0; i < viewOrder.Length; i++)
            {
                int backingIndex = viewOrder[i];
                int score = GetScorePct(backingIndex);

                if (score < HighConfidenceThresholdPct)
                    continue;

                if (excludeAlreadyAssigned && assigned[backingIndex])
                    continue;

                countHigh++;
            }

            if (countHigh == 0)
            {
                return;
            }

            var matches = new MediaEntryInfo[countHigh];
            int w = 0;

            for (int i = 0; i < viewOrder.Length; i++)
            {
                int backingIndex = viewOrder[i];
                int score = GetScorePct(backingIndex);

                if (score < HighConfidenceThresholdPct)
                    continue;

                if (excludeAlreadyAssigned && assigned[backingIndex])
                    continue;

                matches[w++] = new MediaEntryInfo(
                    backingIndex,
                    entriesFull[backingIndex],
                    score,
                    assigned[backingIndex]);
            }

            highConfidenceMatches = matches;

            if (countHigh != 1)
            {
                return;
            }

            singleHighConfidenceMatch = matches[0];
        }

        private void EnsureScratchCapacity(int required)
        {
            if (scratch.Length >= required)
                return;

            scratch = new int[required];
        }

        private static int ComputePercentUpper(string sUpper, string tUpper)
        {
            int maxLen = sUpper.Length >= tUpper.Length ? sUpper.Length : tUpper.Length;
            if (maxLen == 0)
                return 100;

            int dist = ComputeDistanceUpper(sUpper, tUpper);
            int pct = (int)Math.Round(100.0 * (1.0 - (double)dist / maxLen));

            if (pct < 0) return 0;
            if (pct > 100) return 100;
            return pct;
        }

        private static int ComputeDistanceUpper(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;

            if (n == 0) return m;
            if (m == 0) return n;

            if (m > n)
            {
                var tmp = s; s = t; t = tmp;
                n = s.Length;
                m = t.Length;
            }

            int mPlus1 = m + 1;

            // NOTE: System.Memory supplies Span<T> for netstandard2.0; retain stack-allocated matching buffers.
            Span<int> prev = mPlus1 <= 512 ? stackalloc int[mPlus1] : new int[mPlus1];
            Span<int> curr = mPlus1 <= 512 ? stackalloc int[mPlus1] : new int[mPlus1];

            for (int j = 0; j <= m; j++)
                prev[j] = j;

            for (int i = 1; i <= n; i++)
            {
                curr[0] = i;

                char sc = s[i - 1];

                for (int j = 1; j <= m; j++)
                {
                    char tc = t[j - 1];
                    int cost = sc == tc ? 0 : 1;

                    int del = prev[j] + 1;
                    int ins = curr[j - 1] + 1;
                    int sub = prev[j - 1] + cost;

                    int v = del < ins ? del : ins;
                    if (sub < v) v = sub;

                    curr[j] = v;
                }

                var swap = prev;
                prev = curr;
                curr = swap;
            }

            return prev[m];
        }

        private void SetReferenceFlagless(string[] newReferenceFlaglessUppers, bool forceReset)
        {
            newReferenceFlaglessUppers ??= Array.Empty<string>();

            if (!forceReset &&
                lastReferenceFlaglessUppers.Length == newReferenceFlaglessUppers.Length)
            {
                bool same = true;

                for (int i = 0; i < newReferenceFlaglessUppers.Length; i++)
                {
                    if (!string.Equals(
                        lastReferenceFlaglessUppers[i],
                        newReferenceFlaglessUppers[i],
                        StringComparison.Ordinal))
                    {
                        same = false;
                        break;
                    }
                }

                if (same)
                    return;
            }

            lastReferenceFlaglessUppers = newReferenceFlaglessUppers;

            // NOTE: This existing field is used as a simple "has scoring reference"
            // indicator elsewhere in the session.
            lastReferenceFlagless =
                newReferenceFlaglessUppers.Length == 0
                    ? string.Empty
                    : newReferenceFlaglessUppers[0];

            scoreGeneration++;
            if (scoreGeneration == int.MaxValue)
            {
                Array.Clear(scoreGen, 0, scoreGen.Length);
                scoreGeneration = 1;
            }
        }

        private int GetScorePct(int backingIndex)
        {
            if (lastReferenceFlaglessUppers.Length == 0)
                return -1;

            if (scoreGen[backingIndex] != scoreGeneration)
            {
                string entryUpper = entriesFlaglessUpper[backingIndex];

                int bestScore = 0;

                for (int i = 0; i < lastReferenceFlaglessUppers.Length; i++)
                {
                    string referenceUpper = lastReferenceFlaglessUppers[i];

                    int score;

                    // NOTE: Avoid Levenshtein entirely for an exact match.
                    if (string.Equals(referenceUpper, entryUpper, StringComparison.Ordinal))
                    {
                        score = 100;
                    }
                    else
                    {
                        score = ComputePercentUpper(referenceUpper, entryUpper);
                    }

                    if (score > bestScore)
                        bestScore = score;

                    // NOTE: Cannot improve upon a perfect match.
                    if (bestScore == 100)
                        break;
                }

                scorePct[backingIndex] = bestScore;
                scoreGen[backingIndex] = scoreGeneration;
            }

            return scorePct[backingIndex];
        }

        private void EnsureScoresForViewOrder()
        {
            if (lastReferenceFlaglessUppers.Length == 0)
                return;

            for (int i = 0; i < viewOrder.Length; i++)
                _ = GetScorePct(viewOrder[i]);
        }

        public int FindBackingIndex(string value)
        {
            // NOTE: Fast exact match.
            if (backingIndexByFull.TryGetValue(value, out int idx))
                return idx;

            // NOTE: Trim fallback (common when values come from persisted text)
            var trimmed = value.Trim();
            if (trimmed.Length != value.Length && backingIndexByFull.TryGetValue(trimmed, out idx))
                return idx;

            // NOTE: Ignore-case fallback
            for (int i = 0; i < entriesFull.Length; i++)
            {
                if (string.Equals(entriesFull[i], value, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            // NOTE: Flagless fallback (only if persisted value might be flagless)
            var wantFlaglessUpper = DatinateHelper.GetFlaglessName(value) ?? value;
            wantFlaglessUpper = wantFlaglessUpper.ToUpperInvariant();

            for (int i = 0; i < entriesFlaglessUpper.Length; i++)
            {
                if (entriesFlaglessUpper[i] == wantFlaglessUpper)
                    return i;
            }

            return -1;
        }

        public int FindViewIndex(int backingIndex)
        {
            for (int i = 0; i < viewOrder.Length; i++)
            {
                if (viewOrder[i] == backingIndex)
                    return i;
            }

            return -1;
        }

        public string? GetNameAtViewIndex(int viewIndex)
            => (uint)viewIndex < (uint)viewOrder.Length ? entriesFull[viewOrder[viewIndex]] : null;

        // Click events consume the existing score snapshot; they do not start scoring.
        public MediaEntryInfo? GetCachedEntryAtViewIndex(int viewIndex)
        {
            if ((uint)viewIndex >= (uint)viewOrder.Length)
                return null;

            int backingIndex = viewOrder[viewIndex];
            return new MediaEntryInfo(
                backingIndex,
                entriesFull[backingIndex],
                scorePct[backingIndex],
                assigned[backingIndex]);
        }

        // Row rendering calculates a score only when its generation is stale.
        public bool TryGetEntryAtViewIndex(int viewIndex, out MediaEntryInfo entry)
        {
            entry = default;

            if ((uint)viewIndex >= (uint)viewOrder.Length)
                return false;

            int backingIndex = viewOrder[viewIndex];

            if ((uint)backingIndex >= (uint)entriesFull.Length ||
                (uint)backingIndex >= (uint)assigned.Length ||
                (uint)backingIndex >= (uint)scorePct.Length ||
                (uint)backingIndex >= (uint)scoreGen.Length ||
                (uint)backingIndex >= (uint)entriesFlaglessUpper.Length)
            {
                return false;
            }

            entry = new MediaEntryInfo(
                backingIndex,
                entriesFull[backingIndex],
                GetScorePct(backingIndex),
                assigned[backingIndex]);
            return true;
        }

        private void ReplaceViewOrder(int[] newViewOrder)
        {
            ViewOrderChanging?.Invoke(newViewOrder.Length);
            viewOrder = newViewOrder;
        }
    }
}
