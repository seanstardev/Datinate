using static Datinate.Shared.DatinateEnums;

namespace Datinate.Shared
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

    // A facts-only snapshot. Labels, wording, ordering and display limits belong
    // to the presentation layer, not to the grouping model.
    public sealed class AutomatedGroupingReport
    {
        public AutomatedGroupingIdentity Identity { get; }
        public AutomatedGroupingComparison Comparison { get; }
        public AutomatedGroupingSeed? Seed { get; }
        public AutomatedGroupingMembership? InitialGroup { get; }
        public AutomatedGroupingMembership? FinalFamily { get; }
        public bool HasCapturedTrace { get; }
        public IReadOnlyList<AutomatedGroupingMatch> SeedJoins { get; }
        public IReadOnlyList<AutomatedGroupingMatch> LaterMerges { get; }
        public IReadOnlyList<AutomatedGroupingName> FingerprintMatches { get; }

        public AutomatedGroupingReport(
            AutomatedGroupingIdentity identity,
            AutomatedGroupingComparison comparison,
            AutomatedGroupingSeed? seed,
            AutomatedGroupingMembership? initialGroup,
            AutomatedGroupingMembership? finalFamily,
            bool hasCapturedTrace,
            IEnumerable<AutomatedGroupingMatch> seedJoins,
            IEnumerable<AutomatedGroupingMatch> laterMerges,
            IEnumerable<AutomatedGroupingName> fingerprintMatches)
        {
            Identity = identity;
            Comparison = comparison;
            Seed = seed;
            InitialGroup = initialGroup;
            FinalFamily = finalFamily;
            HasCapturedTrace = hasCapturedTrace;
            SeedJoins = seedJoins.ToList().AsReadOnly();
            LaterMerges = laterMerges.ToList().AsReadOnly();
            FingerprintMatches = fingerprintMatches.ToList().AsReadOnly();
        }
    }

    public sealed class AutomatedGroupingIdentity
    {
        public string EntryName { get; }
        public string DatSourceId { get; }
        public DAT_GROUP_ENUM DatGroup { get; }
        public string? DatReference { get; }
        public string Fingerprint { get; }

        public AutomatedGroupingIdentity(string entryName, string datSourceId,
            DAT_GROUP_ENUM datGroup, string? datReference, string fingerprint)
        {
            EntryName = entryName;
            DatSourceId = datSourceId;
            DatGroup = datGroup;
            DatReference = datReference;
            Fingerprint = fingerprint;
        }
    }

    public sealed class AutomatedGroupingComparison
    {
        public string FlaglessName { get; }
        public string? NamePublisherKey { get; }
        public string? NameRegionKey { get; }
        public string? MameLaunchName { get; }
        public string? ParentName { get; }
        public string? PartOwnerName { get; }
        public string? DerivedPartOwnerKey { get; }

        public AutomatedGroupingComparison(string flaglessName, string? namePublisherKey,
            string? nameRegionKey, string? mameLaunchName, string? parentName,
            string? partOwnerName, string? derivedPartOwnerKey)
        {
            FlaglessName = flaglessName;
            NamePublisherKey = namePublisherKey;
            NameRegionKey = nameRegionKey;
            MameLaunchName = mameLaunchName;
            ParentName = parentName;
            PartOwnerName = partOwnerName;
            DerivedPartOwnerKey = derivedPartOwnerKey;
        }
    }

    public sealed class AutomatedGroupingSeed
    {
        public AUTO_GROUP_TRACE_REASON_ENUM Reason { get; }
        public string Value { get; }
        public string Key { get; }

        public AutomatedGroupingSeed(AUTO_GROUP_TRACE_REASON_ENUM reason, string value, string key)
        {
            Reason = reason;
            Value = value;
            Key = key;
        }
    }

    public sealed class AutomatedGroupingMembership
    {
        // Includes the reported entry. Names are samples of the OTHER members.
        public int MemberCount { get; }
        public IReadOnlyList<string> SampleOtherNames { get; }

        public AutomatedGroupingMembership(int memberCount, IEnumerable<string> sampleOtherNames)
        {
            MemberCount = memberCount;
            SampleOtherNames = sampleOtherNames.ToList().AsReadOnly();
        }
    }

    // Aggregated observations by reason/key, not a chronological event history.
    public sealed class AutomatedGroupingMatch
    {
        public AUTO_GROUP_TRACE_REASON_ENUM Reason { get; }
        public string Key { get; }
        public int OccurrenceCount { get; }
        public int? FirstMatchingGroupSize { get; }
        public IReadOnlyList<string> SampleMatchingNames { get; }

        public AutomatedGroupingMatch(AUTO_GROUP_TRACE_REASON_ENUM reason, string key,
            int occurrenceCount, int? firstMatchingGroupSize, IEnumerable<string> sampleMatchingNames)
        {
            Reason = reason;
            Key = key;
            OccurrenceCount = occurrenceCount;
            FirstMatchingGroupSize = firstMatchingGroupSize;
            SampleMatchingNames = sampleMatchingNames.ToList().AsReadOnly();
        }
    }

    public sealed class AutomatedGroupingName
    {
        public string Name { get; }
        public DAT_GROUP_ENUM DatGroup { get; }

        public AutomatedGroupingName(string name, DAT_GROUP_ENUM datGroup)
        {
            Name = name;
            DatGroup = datGroup;
        }
    }
}
