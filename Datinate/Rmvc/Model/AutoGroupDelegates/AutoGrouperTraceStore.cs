using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using RadioLibCore.RadioDat;
using System.Runtime.CompilerServices;
using System.Text;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public enum AUTO_GROUP_TRACE_REASON_ENUM
    {
        SeedJoinParentClone,
        SeedJoinMameLaunchCategory,
        SeedJoinNormalisedName,
        MergeByNamePublisher,
        MergeByNameRegion,
        MergeByNormalisedName,
        MergeByFingerprint,
        CollapseFamilyByRegionNormalisedTitle
    }

    public class AutoGrouperTraceStore
    {
        private const int MaxTraceKeysPerType = 8;
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
        private ConditionalWeakTable<GamePartVO, string>? partReports;
        private Dictionary<string, string>? partFingerprintReports;

        public AutoGrouperTraceStore(IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> _)
        {
            entryTraces = new ConditionalWeakTable<DatGameVO, EntryTrace>();
            partReports = new ConditionalWeakTable<GamePartVO, string>();
            partFingerprintReports = new Dictionary<string, string>(StringComparer.Ordinal);
            fingerprintTitleSamples = new Dictionary<string, List<FingerprintTitleSample>>(StringComparer.Ordinal);
        }

        public void RecordSeed(DatGameVO entry, string seedRaw, string seedKey)
        {
            if (entryTraces == null)
                return;

            var t = entryTraces.GetOrCreateValue(entry);
            t.SeedRaw = seedRaw;
            t.SeedKey = seedKey;
        }

        public void RecordInitialSeedGroup(IReadOnlyList<DatGameVO> entries)
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

        public void RecordFinalFamily(IReadOnlyList<DatGameVO> entries)
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

        public void RecordSeedJoin(
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

        public void RecordMerge(
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

        public void AttachPartReport(
            GamePartVO part,
            DatGameVO entry,
            DAT_GROUP_ENUM datGroupEnum,
            string datSourceId,
            string datReference,
            string? derivedPartOwner)
        {
            if (partReports == null || partFingerprintReports == null)
                return;

            EntryTrace? trace = null;
            if (entryTraces != null)
                entryTraces.TryGetValue(entry, out trace);

            var raw = entry.Name ?? string.Empty;
            var fingerprint = string.IsNullOrWhiteSpace(part.Fingerprint) ? entry.Fingerprint : part.Fingerprint;
            var fingerprintTitleMatches = RegisterAndGetFingerprintTitleSamples(fingerprint, raw, datGroupEnum);

            var sb = new StringBuilder(3072);

            AppendHeader(sb, "Automated Grouping Report");
            AppendText(sb, raw);
            AppendFingerprintTitleMatches(sb, raw, datGroupEnum, fingerprintTitleMatches);

            AppendHeader(sb, "Identity");

            if (string.IsNullOrWhiteSpace(datReference) == false)
                AppendKV(sb, "DAT:", $"{datGroupEnum} | {datReference}");
            else
                AppendKV(sb, "DAT:", $"{datGroupEnum}");

            AppendKV(sb, "Fingerprint:", fingerprint);

            AppendHeader(sb, "Comparison data");
            AppendKV(sb, "Title after removing flags:", entry.FlaglessName);
            AppendKV(sb, "Comparison key (title + publisher):", entry.NamePublisherKey);
            AppendKV(sb, "Comparison key (title + region):", entry.NameRegionKey);
            AppendKV(sb, "MAME launch/category name:", entry.MameLaunchName);
            AppendKV(sb, "Clone of:", entry.ParentName);
            AppendKV(sb, "Part owner name:", entry.PartOwnerName);
            AppendKV(sb, "Derived part-owner key:", derivedPartOwner);

            AppendHeader(sb, "Grouping timeline");
            AppendGroupingTimeline(sb, entry, trace);

            AppendHeader(sb, "Initial grouping");
            AppendKV(sb, "This item was first grouped using:", GetFriendlyTraceReason(GetSeedTraceReason(entry, trace)));
            AppendKV(sb, "Seed value:", trace?.SeedRaw ?? "<not captured>");
            AppendKV(sb, "Normalised grouping key:", trace?.SeedKey ?? "<not captured>");

            AppendHeader(sb, "Grouping result");
            AppendInitialGroupingResult(sb, trace);

            AppendHeader(sb, "Final family result");
            AppendFinalFamilyResult(sb, trace);

            AppendHeader(sb, "Seed-group joins");
            AppendTraceGroups(
                sb,
                trace?.SeedJoinsByType,
                countLabelSingular: "seed-group join",
                countLabelPlural: "seed-group joins",
                sizeLabel: "Size of matching group at that step:",
                emptyMessage: "No additional seed-group joins were captured after the initial grouping step.");

            AppendHeader(sb, "Later merges");
            AppendTraceGroups(
                sb,
                trace?.MergesByType,
                countLabelSingular: "merge event",
                countLabelPlural: "merge events",
                sizeLabel: "Size of matching group at that step:",
                emptyMessage: "No later merges were captured.");

            var str = NormaliseReportText(sb.ToString());

            partReports.Remove(part);
            partReports.Add(part, str);

            partFingerprintReports[fingerprint] = str;
        }

        public bool TryGetPartReport(IGamePart part, out string report)
        {
            report = string.Empty;

            if (partFingerprintReports != null &&
                partFingerprintReports.TryGetValue(part.Fingerprint, out report))
            {
                return true;
            }

            if (part is GamePartVO gamePart &&
                partReports != null &&
                partReports.TryGetValue(gamePart, out report))
            {
                return true;
            }

            report = string.Empty;
            return false;
        }

        public void AddManagedListReport(string id, Dictionary<string, ManagedListItemReport> dictionary)
        {
            managedListDic[id] = new ManagedListWrapper(dictionary);
        }

        public ManagedListItemReport? GetManagedReport(string entryName, string id)
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

        private static void AppendHeader(StringBuilder sb, string text)
        {
            AppendBlankLine(sb);
            sb.Append("## ");
            sb.AppendLine(text);
        }

        private static void AppendText(StringBuilder sb, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            sb.AppendLine(value.Trim());
        }

        private static void AppendBlankLine(StringBuilder sb)
        {
            if (sb.Length == 0)
                return;

            var n = sb.Length;
            if (n >= 2 && sb[n - 1] == '\n' && sb[n - 2] == '\n')
                return;

            sb.AppendLine();
        }

        private static void AppendKV(StringBuilder sb, string keyWithColon, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            sb.AppendLine($"\t{keyWithColon}\t{value}");
        }

        private static void AppendGroupingTimeline(StringBuilder sb, DatGameVO entry, EntryTrace? trace)
        {
            sb.AppendLine($"\t- Seeded by {GetFriendlyTraceReason(GetSeedTraceReason(entry, trace)).ToLowerInvariant()}.");

            if (trace != null && trace.InitialSeedGroupSize > 0)
                sb.AppendLine($"\t- Initial group size: {FormatCount(trace.InitialSeedGroupSize, "item")}.");

            if (trace?.SeedJoinsByType != null && trace.SeedJoinsByType.Count > 0)
            {
                var joinSummary = BuildReasonSummary(trace.SeedJoinsByType, "seed-group join", "seed-group joins");
                if (!string.IsNullOrWhiteSpace(joinSummary))
                    sb.AppendLine($"\t- Additional seed-group joins: {joinSummary}.");
            }

            if (trace?.MergesByType != null && trace.MergesByType.Count > 0)
            {
                var mergeSummary = BuildReasonSummary(trace.MergesByType, "merge event", "merge events");
                if (!string.IsNullOrWhiteSpace(mergeSummary))
                    sb.AppendLine($"\t- Later merges: {mergeSummary}.");
            }
            else
            {
                sb.AppendLine("\t- No later merges were needed.");
            }

            if (trace != null && trace.FinalFamilySize > 0)
                sb.AppendLine($"\t- Final family size: {FormatCount(trace.FinalFamilySize, "item")}.");
        }

        private static string BuildReasonSummary(
            Dictionary<AUTO_GROUP_TRACE_REASON_ENUM, Dictionary<string, MergeKeyTrace>> groupsByType,
            string singular,
            string plural)
        {
            var parts = new List<string>();

            foreach (var pair in groupsByType)
            {
                var total = 0;
                foreach (var keyPair in pair.Value)
                    total += keyPair.Value.MergeCount;

                if (total <= 0)
                    continue;

                parts.Add($"{GetFriendlyTraceReason(pair.Key)} ({total} {(total == 1 ? singular : plural)})");
            }

            return string.Join(", ", parts);
        }

        private static void AppendInitialGroupingResult(StringBuilder sb, EntryTrace? trace)
        {
            if (trace == null || trace.InitialSeedGroupSize <= 0)
            {
                AppendText(sb, "The initial grouping outcome was not captured.");
                return;
            }

            AppendKV(sb, "This item started in an initial group of:", FormatCount(trace.InitialSeedGroupSize, "item"));

            if (trace.InitialSeedGroupSize <= 1)
            {
                AppendText(sb, "No other items matched this item's initial grouping seed.");
                return;
            }

            if (trace.InitialSeedSiblingSamples.Count == 0)
            {
                AppendText(sb, "This item grouped with other matching items, but no example item names were captured.");
                return;
            }

            AppendBlankLine(sb);
            AppendText(sb, "Other matching items in this initial group:");

            for (var i = 0; i < trace.InitialSeedSiblingSamples.Count; i++)
                sb.AppendLine($"\t\t\t- {trace.InitialSeedSiblingSamples[i]}");

            var remaining = (trace.InitialSeedGroupSize - 1) - trace.InitialSeedSiblingSamples.Count;
            if (remaining > 0)
                sb.AppendLine($"\t\t\t... and {remaining} other {Pluralise(remaining, "item", "items")}.");
        }

        private static void AppendFinalFamilyResult(StringBuilder sb, EntryTrace? trace)
        {
            if (trace == null || trace.FinalFamilySize <= 0)
            {
                AppendText(sb, "The final family outcome was not captured.");
                return;
            }

            AppendKV(sb, "This item ended in a final family of:", FormatCount(trace.FinalFamilySize, "item"));

            if (trace.FinalFamilySize <= 1)
            {
                AppendText(sb, "No other items ended up grouped with this item.");
                return;
            }

            if (trace.FinalFamilySiblingSamples.Count == 0)
            {
                AppendText(sb, "Other final family members existed, but no example item names were captured.");
                return;
            }

            AppendBlankLine(sb);
            AppendText(sb, "Sample final family members:");

            for (var i = 0; i < trace.FinalFamilySiblingSamples.Count; i++)
                sb.AppendLine($"\t\t\t- {trace.FinalFamilySiblingSamples[i]}");

            var remaining = (trace.FinalFamilySize - 1) - trace.FinalFamilySiblingSamples.Count;
            if (remaining > 0)
                sb.AppendLine($"\t\t\t... and {remaining} other {Pluralise(remaining, "item", "items")}.");
        }

        private static void AppendTraceGroups(
            StringBuilder sb,
            Dictionary<AUTO_GROUP_TRACE_REASON_ENUM, Dictionary<string, MergeKeyTrace>>? groupsByType,
            string countLabelSingular,
            string countLabelPlural,
            string sizeLabel,
            string emptyMessage)
        {
            if (groupsByType == null || groupsByType.Count == 0)
            {
                AppendText(sb, emptyMessage);
                return;
            }

            var wroteAny = false;

            foreach (var typePair in groupsByType)
            {
                if (typePair.Value.Count == 0)
                    continue;

                if (wroteAny)
                    AppendBlankLine(sb);

                wroteAny = true;
                sb.AppendLine($"\t{GetFriendlyTraceReason(typePair.Key)}:");

                var shown = 0;
                foreach (var keyPair in typePair.Value)
                {
                    var mk = keyPair.Value;

                    sb.AppendLine($"\t\tKey:\t{FormatTraceKey(keyPair.Key)}");
                    sb.AppendLine($"\t\tSeen:\t{mk.MergeCount} {(mk.MergeCount == 1 ? countLabelSingular : countLabelPlural)}");

                    if (mk.OtherNameSamples.Count > 0)
                    {
                        sb.AppendLine("\t\tSample items from matching group:");
                        for (var i = 0; i < mk.OtherNameSamples.Count; i++)
                            sb.AppendLine($"\t\t\t- {mk.OtherNameSamples[i]}");
                    }

                    if (mk.OtherSetSizeSample > 0)
                        sb.AppendLine($"\t\t{sizeLabel}\t{FormatCount(mk.OtherSetSizeSample, "item")}");

                    shown++;

                    if (shown >= MaxTraceKeysPerType)
                    {
                        var omitted = typePair.Value.Count - shown;
                        if (omitted > 0)
                        {
                            AppendBlankLine(sb);
                            sb.AppendLine($"\t\t... and {omitted} other matching {Pluralise(omitted, "key", "keys")}.");
                        }

                        break;
                    }

                    if (shown < typePair.Value.Count)
                        AppendBlankLine(sb);
                }
            }

            if (!wroteAny)
                AppendText(sb, emptyMessage);
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

        private static string GetFriendlyTraceReason(AUTO_GROUP_TRACE_REASON_ENUM traceReason)
        {
            return traceReason switch
            {
                AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinParentClone => "Parent/clone relationship",
                AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinMameLaunchCategory => "MAME launch/category name",
                AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinNormalisedName => "Normalised name",
                AUTO_GROUP_TRACE_REASON_ENUM.MergeByNamePublisher => "Normalised title + publisher",
                AUTO_GROUP_TRACE_REASON_ENUM.MergeByNameRegion => "Normalised title + region",
                AUTO_GROUP_TRACE_REASON_ENUM.MergeByNormalisedName => "Normalised name",
                AUTO_GROUP_TRACE_REASON_ENUM.MergeByFingerprint => "Exact fingerprint",
                AUTO_GROUP_TRACE_REASON_ENUM.CollapseFamilyByRegionNormalisedTitle => "Collapsed family by region-normalised title",
                _ => traceReason.ToString()
            };
        }

        private static string FormatTraceKey(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return "<empty>";

            return key;
        }

        private static string FormatCount(int count, string singular)
        {
            return $"{count} {Pluralise(count, singular, singular + "s")}";
        }

        private static string Pluralise(int count, string singular, string plural)
        {
            return count == 1 ? singular : plural;
        }

        private static string NormaliseReportText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            var sb = new StringBuilder(text.Length);
            var previousBlank = false;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].TrimEnd();
                var isBlank = line.Length == 0;

                if (isBlank)
                {
                    if (previousBlank)
                        continue;

                    previousBlank = true;
                    sb.AppendLine();
                    continue;
                }

                previousBlank = false;
                sb.AppendLine(line);
            }

            return sb.ToString().TrimEnd();
        }

        private IReadOnlyList<FingerprintTitleSample> RegisterAndGetFingerprintTitleSamples(
            string fingerprint,
            string raw,
            DAT_GROUP_ENUM datGroupEnum)
        {
            if (fingerprintTitleSamples == null || string.IsNullOrWhiteSpace(fingerprint))
                return Array.Empty<FingerprintTitleSample>();

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

            return samples;
        }

        private static void AppendFingerprintTitleMatches(
            StringBuilder sb,
            string raw,
            DAT_GROUP_ENUM datGroupEnum,
            IReadOnlyList<FingerprintTitleSample> matches)
        {
            if (matches.Count <= 1)
                return;

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                if (string.IsNullOrWhiteSpace(match.Name))
                    continue;

                if (string.Equals(match.Name, raw, StringComparison.Ordinal) &&
                    match.DatGroupEnum == datGroupEnum)
                {
                    continue;
                }

                sb.AppendLine($"{match.Name} [{match.DatGroupEnum}]");
            }
        }

        private sealed class ManagedListWrapper
        {
            private readonly Dictionary<string, ManagedListItemReport> dictionary;

            public ManagedListWrapper(Dictionary<string, ManagedListItemReport> dictionary)
            {
                this.dictionary = dictionary;
            }

            public ManagedListItemReport? GetReport(string entryName)
            {
                if (dictionary.ContainsKey(entryName))
                    return dictionary[entryName];
                else
                    return null;
            }
        }
    }
}
