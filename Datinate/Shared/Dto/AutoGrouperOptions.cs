namespace com.RADIO.Datinate.RMVC.Shared
{
    public class AutoGrouperOptions
    {
        public bool MergeByNameAndPublisherKey { get; }
        public bool MergeByNameAndRegionKey { get; }
        public bool MergeByNormalisedName { get; }
        public bool CollapseFamiliesByRegionKey { get; }
        public bool UsePathSafeNormalisedNames { get; }
        public bool EnableCrossDatMerging { get; }
        public bool OrderGamesByInclusionState { get; }

        public AutoGrouperOptions(
            bool mergeByNameAndPublisherKey = true,
            bool mergeByNameAndRegionKey = true,
            bool mergeByNormalisedName = true,
            bool collapseFamiliesByRegionKey = true,
            bool usePathSafeNormalisedNames = true,
            bool enableCrossDatMerging = true,
            bool orderGamesByInclusionState = true)
        {
            MergeByNameAndPublisherKey = mergeByNameAndPublisherKey;
            MergeByNameAndRegionKey = mergeByNameAndRegionKey;
            MergeByNormalisedName = mergeByNormalisedName;
            CollapseFamiliesByRegionKey = collapseFamiliesByRegionKey;
            UsePathSafeNormalisedNames = usePathSafeNormalisedNames;
            EnableCrossDatMerging = enableCrossDatMerging;
            OrderGamesByInclusionState = orderGamesByInclusionState;
        }
    }
}
