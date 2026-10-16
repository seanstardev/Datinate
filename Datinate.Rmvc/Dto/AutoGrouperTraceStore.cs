using Datinate.Shared;
using Datinate.Shared.Dat;
using Datinate.Shared.Radio;
using System.Runtime.CompilerServices;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Dto
{
    internal class AutoGrouperTraceStore
    {
        private const int MaxInitialSiblingSamples = 3;
        private const int MaxFinalFamilySiblingSamples = 3;
        private const int MaxMatchingGroupSamples = 3;

        private sealed class EntryTrace
        {
            public string? SeedRaw;
            public string? SeedKey;
            public int InitialSeedGroupSize;
            public int FinalFamilySize;

            public readonly List<string> InitialSeedSiblingSamples = new List<string>();
            public readonly List<string> FinalFamilySiblingSamples = new List<string>();

            public readonly Dictionary<AUTO_GROUP_TRACE_REASON_ENUM, Dictionary<string, MergeKeyTrace>> SeedJoinsByType =
                new();

            public readonly Dictionary<AUTO_GROUP_TRACE_REASON_ENUM, Dictionary<string, MergeKeyTrace>> MergesByType =
                new();
        }

        private sealed class MergeKeyTrace
        {
            public int MergeCount;
            public int OtherSetSizeSample;
            public readonly List<string> OtherNameSamples = new List<string>();
        }
        private readonly record struct FingerprintTitleSample(string Name, DAT_GROUP_ENUM DatGroupEnum);

        private Dictionary<string, List<FingerprintTitleSample>>? fingerprintTitleSamples;

        private Dictionary<string, ManagedListWrapper> managedListDic = new Dictionary<string, ManagedListWrapper>();

        private ConditionalWeakTable<DatGameVO, EntryTrace>? entryTraces;
        private ConditionalWeakTable<IGamePart, AutomatedGroupingReport>? partReports;
        private ConditionalWeakTable<DatGameVO, AutomatedGroupingReport>? sourceReports;

        internal AutoGrouperTraceStore(IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> _)
        {
            entryTraces = new ConditionalWeakTable<DatGameVO, EntryTrace>();
            partReports = new ConditionalWeakTable<IGamePart, AutomatedGroupingReport>();
            sourceReports = new ConditionalWeakTable<DatGameVO, AutomatedGroupingReport>();
            fingerprintTitleSamples = new Dictionary<string, List<FingerprintTitleSample>>(StringComparer.Ordinal);
        }

        internal void RecordSeed(DatGameVO entry, string seedRaw, string seedKey)
        {
            if (entryTraces == null)
                return;

            var t = entryTraces.GetOrCreateValue(entry);
            t.SeedRaw = seedRaw;
            t.SeedKey = seedKey;
        }

        internal void RecordInitialSeedGroup(IReadOnlyList<DatGameVO> entries)
        {
            if (entryTraces == null || entries.Count == 0)
                return;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var t = entryTraces.GetOrCreateValue(entry);

                t.InitialSeedGroupSize = entries.Count;
                t.InitialSeedSiblingSamples.Clear();
                AddSiblingSamples(t.InitialSeedSiblingSamples, entries, i, MaxInitialSiblingSamples);
            }
        }

        internal void RecordFinalFamily(IReadOnlyList<DatGameVO> entries)
        {
            if (entryTraces == null || entries.Count == 0)
                return;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var t = entryTraces.GetOrCreateValue(entry);

                t.FinalFamilySize = entries.Count;
                t.FinalFamilySiblingSamples.Clear();
                AddSiblingSamples(t.FinalFamilySiblingSamples, entries, i, MaxFinalFamilySiblingSamples);
            }
        }

        internal void RecordSeedJoin(
            DatGameVO entry,
            AUTO_GROUP_TRACE_REASON_ENUM joinReason,
            string joinKey,
            int otherBucketSize,
            IReadOnlyList<string>? otherNameSamples)
        {
            if (entryTraces == null)
                return;

            var t = entryTraces.GetOrCreateValue(entry);
            RecordKeyEventInTrace(t.SeedJoinsByType, joinReason, joinKey, otherBucketSize, otherNameSamples);
        }

        internal void RecordMerge(
            DatEntrySet a,
            DatEntrySet b,
            AUTO_GROUP_TRACE_REASON_ENUM mergeReason,
            string mergeKey)
        {
            if (entryTraces == null)
                return;

            var aEntries = a.Entries;
            var bEntries = b.Entries;

            var aSamples = FirstNames(aEntries, MaxMatchingGroupSamples);
            var bSamples = FirstNames(bEntries, MaxMatchingGroupSamples);

            foreach (var e in aEntries)
                RecordMergeForEntry(e, mergeReason, mergeKey, bEntries.Count, bSamples);

            foreach (var e in bEntries)
                RecordMergeForEntry(e, mergeReason, mergeKey, aEntries.Count, aSamples);
        }

        internal void AttachPartReport(
            GamePartVO part,
            DatGameVO entry,
            DAT_GROUP_ENUM datGroupEnum,
            string datSourceId,
            string datReference,
            string? derivedPartOwner)
        {
            if (partReports == null || sourceReports == null)
                return;

            EntryTrace? trace = null;
            if (entryTraces != null)
                entryTraces.TryGetValue(entry, out trace);

            var raw = entry.Name ?? string.Empty;
            var fingerprint = string.IsNullOrWhiteSpace(part.Fingerprint) ? entry.Fingerprint : part.Fingerprint;
            RegisterFingerprintTitle(fingerprint, raw, datGroupEnum);

            var report = new AutomatedGroupingReport(
                new AutomatedGroupingIdentity(raw, datSourceId, datGroupEnum, datReference, fingerprint),
                new AutomatedGroupingComparison(entry.FlaglessName, entry.NamePublisherKey,
                    entry.NameRegionKey, entry.MameLaunchName, entry.ParentName,
                    entry.PartOwnerName, derivedPartOwner),
                trace?.SeedRaw != null && trace.SeedKey != null
                    ? new AutomatedGroupingSeed(GetSeedTraceReason(entry, trace), trace.SeedRaw, trace.SeedKey)
                    : null,
                trace != null && trace.InitialSeedGroupSize > 0
                    ? new AutomatedGroupingMembership(trace.InitialSeedGroupSize, trace.InitialSeedSiblingSamples)
                    : null,
                trace != null && trace.FinalFamilySize > 0
                    ? new AutomatedGroupingMembership(trace.FinalFamilySize, trace.FinalFamilySiblingSamples)
                    : null,
                trace != null,
                SnapshotMatches(trace?.SeedJoinsByType),
                SnapshotMatches(trace?.MergesByType),
                Array.Empty<AutomatedGroupingName>());

            partReports.Remove(part);
            partReports.Add(part, report);

            sourceReports.Remove(entry);
            sourceReports.Add(entry, report);
        }

        internal bool TryGetPartReport(IGamePart part, out AutomatedGroupingReport? report)
        {
            AutomatedGroupingReport? captured = null;

            if (partReports != null)
                partReports.TryGetValue(part, out captured);

            // Shallow parts are new instances retaining the original source entry.
            if (captured == null && part is GamePartVO gamePart &&
                gamePart.IsShallowPartReference && sourceReports != null)
            {
                sourceReports.TryGetValue(gamePart.DatGameSource, out captured);
            }

            if (captured == null)
            {
                report = null;
                return false;
            }

            // Resolve names at interrogation time, after grouping has populated the
            // registry. Earlier attachments must also see later fingerprint matches.
            var names = new List<AutomatedGroupingName>();

            if (fingerprintTitleSamples != null &&
                fingerprintTitleSamples.TryGetValue(captured.Identity.Fingerprint, out var matches))
            {
                foreach (var match in matches)
                {
                    if (match.Name != captured.Identity.EntryName || match.DatGroupEnum != captured.Identity.DatGroup)
                        names.Add(new AutomatedGroupingName(match.Name, match.DatGroupEnum));
                }
            }

            report = new AutomatedGroupingReport(captured.Identity, captured.Comparison,
                captured.Seed, captured.InitialGroup, captured.FinalFamily, captured.HasCapturedTrace,
                captured.SeedJoins, captured.LaterMerges, names);

            return true;
        }

        // The UI receives BaseGamePart instances from DatinateFamilyConverter.
        // Transfer each exact association while its original part is still known;
        // fingerprint equality is not sufficient to identify a report's source.
        internal void AssociateConvertedPartReport(GamePartVO source, BaseGamePart converted)
        {
            if (partReports != null && TryGetPartReport(source, out var report) && report != null)
            {
                partReports.Remove(converted);
                partReports.Add(converted, report);
            }
        }

        private static IEnumerable<AutomatedGroupingMatch> SnapshotMatches(
            Dictionary<AUTO_GROUP_TRACE_REASON_ENUM, Dictionary<string, MergeKeyTrace>>? groups)
        {
            if (groups == null)
                yield break;

            foreach (var group in groups)
            {
                foreach (var pair in group.Value)
                {
                    MergeKeyTrace trace = pair.Value;

                    yield return new AutomatedGroupingMatch(group.Key, pair.Key, trace.MergeCount,
                        trace.OtherSetSizeSample > 0 ? trace.OtherSetSizeSample : null, trace.OtherNameSamples);
                }
            }
        }

        internal void AddManagedListReport(string id, Dictionary<string, DatCustomisationDecisionReport> dictionary)
            => managedListDic[id] = new ManagedListWrapper(dictionary);
        

        internal DatCustomisationDecisionReport? GetManagedReport(string entryName, string id)
        {
            if (managedListDic.ContainsKey(id))
                return managedListDic[id].GetReport(entryName);
            else
                return null;
        }

        private void RecordMergeForEntry(
            DatGameVO entry,
            AUTO_GROUP_TRACE_REASON_ENUM mergeReason,
            string mergeKey,
            int otherSetSize,
            IReadOnlyList<string>? otherNameSamples)
        {
            if (entryTraces == null)
                return;

            var t = entryTraces.GetOrCreateValue(entry);
            RecordKeyEventInTrace(t.MergesByType, mergeReason, mergeKey, otherSetSize, otherNameSamples);
        }

        private void RecordKeyEventInTrace(
            Dictionary<AUTO_GROUP_TRACE_REASON_ENUM, Dictionary<string, MergeKeyTrace>> groupsByType,
            AUTO_GROUP_TRACE_REASON_ENUM traceReason,
            string mergeKey,
            int otherSetSize,
            IReadOnlyList<string>? otherNameSamples)
        {
            if (!groupsByType.TryGetValue(traceReason, out var byKey))
            {
                byKey = new Dictionary<string, MergeKeyTrace>(StringComparer.Ordinal);
                groupsByType[traceReason] = byKey;
            }

            if (!byKey.TryGetValue(mergeKey, out var mk))
            {
                mk = new MergeKeyTrace();
                byKey[mergeKey] = mk;
            }

            mk.MergeCount++;

            if (mk.OtherSetSizeSample == 0)
                mk.OtherSetSizeSample = otherSetSize;

            AddDistinctNames(mk.OtherNameSamples, otherNameSamples, MaxMatchingGroupSamples);
        }

        private static void AddSiblingSamples(List<string> target, IReadOnlyList<DatGameVO> entries, int selfIndex, int maxCount)
        {
            if (entries.Count <= 1)
                return;

            var seen = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < entries.Count; i++)
            {
                if (i == selfIndex)
                    continue;

                var name = entries[i].Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (!seen.Add(name))
                    continue;

                target.Add(name);

                if (target.Count >= maxCount)
                    break;
            }
        }

        private static List<string> FirstNames(IReadOnlyCollection<DatGameVO> entries, int maxCount)
        {
            var result = new List<string>(maxCount);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var entry in entries)
            {
                var name = entry.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (!seen.Add(name))
                    continue;

                result.Add(name);

                if (result.Count >= maxCount)
                    break;
            }

            return result;
        }

        private static void AddDistinctNames(List<string> target, IReadOnlyList<string>? source, int maxCount)
        {
            if (source == null || source.Count == 0 || target.Count >= maxCount)
                return;

            var seen = new HashSet<string>(target, StringComparer.Ordinal);

            for (var i = 0; i < source.Count; i++)
            {
                var name = source[i];
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (!seen.Add(name))
                    continue;

                target.Add(name);

                if (target.Count >= maxCount)
                    break;
            }
        }

        private static AUTO_GROUP_TRACE_REASON_ENUM GetSeedTraceReason(DatGameVO entry, EntryTrace? trace)
        {
            if (!string.IsNullOrWhiteSpace(entry.ParentName))
                return AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinParentClone;

            if (!string.IsNullOrWhiteSpace(entry.MameLaunchName)
                && string.Equals(trace?.SeedRaw, entry.MameLaunchName, StringComparison.Ordinal))
                return AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinMameLaunchCategory;

            return AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinNormalisedName;
        }

        private void RegisterFingerprintTitle(
            string fingerprint,
            string raw,
            DAT_GROUP_ENUM datGroupEnum)
        {
            if (fingerprintTitleSamples == null || string.IsNullOrWhiteSpace(fingerprint))
                return;

            if (!fingerprintTitleSamples.TryGetValue(fingerprint, out var samples))
            {
                samples = new List<FingerprintTitleSample>();
                fingerprintTitleSamples[fingerprint] = samples;
            }

            if (string.IsNullOrWhiteSpace(raw) == false)
            {
                var exists = false;

                for (var i = 0; i < samples.Count; i++)
                {
                    if (string.Equals(samples[i].Name, raw, StringComparison.Ordinal) &&
                        samples[i].DatGroupEnum == datGroupEnum)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                    samples.Add(new FingerprintTitleSample(raw, datGroupEnum));
            }

        }

        private sealed class ManagedListWrapper
        {
            private readonly Dictionary<string, DatCustomisationDecisionReport> dictionary;

            public ManagedListWrapper(Dictionary<string, DatCustomisationDecisionReport> dictionary)
            {
                this.dictionary = dictionary;
            }

            public DatCustomisationDecisionReport? GetReport(string entryName)
            {
                if (dictionary.ContainsKey(entryName))
                    return dictionary[entryName];
                else
                    return null;
            }
        }
    }
}
