using com.RADIO.Datinate.RMVC.Shared;

namespace com.RADIO.Datinate.RMVC
{
    internal sealed class DatEntriesCollection
    {
        public static string NormaliseKey(string? s) => (s ?? string.Empty).ToLowerInvariant();

        public int TotalEntryGroupCount => entryGroups.Count;
        public IReadOnlyList<DatEntrySet> EntryGroups => entryGroups;

        public Dictionary<DatGameVO, string> EntryDatKeyDic { get; private set; } =
            new Dictionary<DatGameVO, string>(ReferenceEqualityComparer.Instance);

        private readonly IReadOnlyDictionary<string, int> datPriorityByDatKey;
        private readonly AutoGrouperOptions options;

        private List<DatEntrySet> entryGroups = new List<DatEntrySet>();
        private AutoGrouperTraceStore? autoGroupTraceStore = null;
        public DatEntriesCollection(
            DatAdvanced datAdv,
            AutoGrouperOptions autoGroupOptions,
            IReadOnlyDictionary<string, int> datPriorityByDatKey,
            AutoGrouperTraceStore? autoGroupTraceStore)
        {
            options = autoGroupOptions;
            this.autoGroupTraceStore = autoGroupTraceStore;
            this.datPriorityByDatKey = datPriorityByDatKey;

            var list = datAdv.Dat.Entries.ToList();

            EntryDatKeyDic = new Dictionary<DatGameVO, string>(list.Count, ReferenceEqualityComparer.Instance);

            for (var i = 0; i < list.Count; i++)
                EntryDatKeyDic.Add(list[i], datAdv.Key);

            var parentNameEntriesDic = new Dictionary<string, List<DatGameVO>>(StringComparer.Ordinal);

            for (var i = 0; i < list.Count; i++)
                AddGameToParentNameEntriesDic(list[i], parentNameEntriesDic);

            SeedGroups(parentNameEntriesDic);

            MergeGroupsAndRebuild();
        }

        public int GetDatPriority(DatGameVO g)
        {
            if (!EntryDatKeyDic.TryGetValue(g, out var datKey))
                throw new KeyNotFoundException($"EntryDatKeyDic missing key for DatGameVO instance. Name='{g.Name}', Fingerprint='{g.Fingerprint}'.");

            if (!datPriorityByDatKey.TryGetValue(datKey, out var p))
                throw new KeyNotFoundException($"datPriorityByDatKey missing key for DatAdvanced '{datKey}'.");

            return p;
        }

        public void AbsorbEntries(DatEntriesCollection collection)
        {
            if (collection.options.EnableCrossDatMerging != options.EnableCrossDatMerging)
                throw new InvalidOperationException("Cannot absorb collections with different groupsAcrossDATs settings.");

            if (collection.options.CollapseFamiliesByRegionKey != options.CollapseFamiliesByRegionKey)
                throw new InvalidOperationException("Cannot absorb collections with different collapseFamiliesByRegionKey settings.");

            foreach (var item in collection.EntryDatKeyDic)
                EntryDatKeyDic[item.Key] = item.Value;

            for (var i = 0; i < collection.entryGroups.Count; i++)
            {
                var g = collection.entryGroups[i];
                entryGroups.Add(new DatEntrySet(
                    g.Entries,
                    EntryDatKeyDic,
                    datPriorityByDatKey,
                    options.EnableCrossDatMerging));
            }

            collection.RemoveAllEntries();
        }

        public void Finalise()
        {
            MergeGroupsAndRebuild();
        }

        private void SeedGroups(Dictionary<string, List<DatGameVO>> parentNameEntriesDic)
        {
            foreach (var pair in parentNameEntriesDic)
            {
                autoGroupTraceStore?.RecordInitialSeedGroup(pair.Value);

                entryGroups.Add(new DatEntrySet(
                    pair.Value,
                    EntryDatKeyDic,
                    datPriorityByDatKey,
                    options.EnableCrossDatMerging));
            }
        }

        private void MergeGroupsAndRebuild()
        {
            if (entryGroups.Count <= 1)
            {
                for (var i = 0; i < entryGroups.Count; i++)
                {
                    entryGroups[i].RebuildFingerprintSet();
                    autoGroupTraceStore?.RecordFinalFamily(entryGroups[i].Entries);
                }
                return;
            }

            entryGroups = MergeGroupsCore(
                entryGroups,
                options.MergeByNameAndPublisherKey,
                options.MergeByNameAndRegionKey,
                options.MergeByNormalisedName,
                autoGroupTraceStore);

            if (options.CollapseFamiliesByRegionKey)
                entryGroups = CollapseFamiliesByRegionKeyCore(entryGroups);

            for (var i = 0; i < entryGroups.Count; i++)
            {
                entryGroups[i].RebuildFingerprintSet();
                autoGroupTraceStore?.RecordFinalFamily(entryGroups[i].Entries);
            }
        }

        private List<DatEntrySet> CollapseFamiliesByRegionKeyCore(IReadOnlyList<DatEntrySet> groups)
        {
            var merged = new List<DatEntrySet>(groups.Count);
            var index = new Dictionary<string, int>(StringComparer.Ordinal);

            for (var i = 0; i < groups.Count; i++)
            {
                var g = groups[i];

                var hasPublisherKeys = g.EntryNamePublishers.Any();
                if (hasPublisherKeys)
                {
                    merged.Add(g);
                    continue;
                }

                var hasRegionKeys = g.EntryNameRegions.Any();
                if (!hasRegionKeys)
                {
                    merged.Add(g);
                    continue;
                }

                var key = BuildRegionCollapseFamilyKey(g);

                if (!index.TryGetValue(key, out var existing))
                {
                    merged.Add(g);
                    index.Add(key, merged.Count - 1);
                }
                else
                {
                    autoGroupTraceStore?.RecordMerge(
                        merged[existing],
                        g,
                        AUTO_GROUP_TRACE_REASON_ENUM.CollapseFamilyByRegionNormalisedTitle,
                        key);

                    merged[existing].AppendEntries(g);
                }
            }

            return merged;
        }
        private string BuildRegionCollapseFamilyKey(DatEntrySet g)
        {
            var seed = g.Entries.FirstOrDefault(e => !e.HasParent) ?? g.Entries[0];

            var baseName = NormaliseKey(seed.FlaglessName);
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = NormaliseKey(seed.Name);

            var prefix = options.EnableCrossDatMerging ? string.Empty : (GetDatKey(seed) + "|");
            return prefix + baseName;
        }

        private static List<DatEntrySet> MergeGroupsCore(
            IReadOnlyList<DatEntrySet> groups,
            bool groupOnNamePublisherKeys,
            bool groupOnNameRegionKeys,
            bool groupOnNameWithFlags,
            AutoGrouperTraceStore? autoGroupTraceStore)
        {
            var n = groups.Count;
            if (n <= 1)
                return groups.ToList();

            var parent = new int[n];
            for (var i = 0; i < n; i++)
                parent[i] = i;

            int Find(int x)
            {
                while (parent[x] != x)
                {
                    parent[x] = parent[parent[x]];
                    x = parent[x];
                }
                return x;
            }

            void Union(int a, int b)
            {
                var ra = Find(a);
                var rb = Find(b);
                if (ra == rb)
                    return;
                parent[rb] = ra;
            }

            var fpIndex = new Dictionary<string, int>(StringComparer.Ordinal);
            var pubIndex = groupOnNamePublisherKeys ? new Dictionary<string, int>(StringComparer.Ordinal) : null;
            var regionIndex = groupOnNameRegionKeys ? new Dictionary<string, int>(StringComparer.Ordinal) : null;
            var nameIndex = groupOnNameWithFlags ? new Dictionary<string, int>(StringComparer.Ordinal) : null;

            for (var i = 0; i < n; i++)
            {
                var g = groups[i];

                foreach (var fp in g.EntryFingerprints)
                {
                    if (fpIndex.TryGetValue(fp, out var other))
                    {
                        autoGroupTraceStore?.RecordMerge(
                            groups[i],
                            groups[other],
                            AUTO_GROUP_TRACE_REASON_ENUM.MergeByFingerprint,
                            fp);

                        Union(i, other);
                    }
                    else
                    {
                        fpIndex[fp] = i;
                    }
                }

                if (pubIndex != null)
                {
                    foreach (var pk in g.EntryNamePublishers)
                    {
                        if (pubIndex.TryGetValue(pk, out var other))
                        {
                            autoGroupTraceStore?.RecordMerge(
                                groups[i],
                                groups[other],
                                AUTO_GROUP_TRACE_REASON_ENUM.MergeByNamePublisher,
                                pk);

                            Union(i, other);
                        }
                        else
                        {
                            pubIndex[pk] = i;
                        }
                    }
                }

                if (regionIndex != null)
                {
                    foreach (var rk in g.EntryNameRegions)
                    {
                        if (regionIndex.TryGetValue(rk, out var other))
                        {
                            autoGroupTraceStore?.RecordMerge(
                                groups[i],
                                groups[other],
                                AUTO_GROUP_TRACE_REASON_ENUM.MergeByNameRegion,
                                rk);

                            Union(i, other);
                        }
                        else
                        {
                            regionIndex[rk] = i;
                        }
                    }
                }

                if (nameIndex != null)
                {
                    foreach (var name in g.EntryNames)
                    {
                        if (nameIndex.TryGetValue(name, out var other))
                        {
                            autoGroupTraceStore?.RecordMerge(
                                groups[i],
                                groups[other],
                                AUTO_GROUP_TRACE_REASON_ENUM.MergeByNormalisedName,
                                name);

                            Union(i, other);
                        }
                        else
                        {
                            nameIndex[name] = i;
                        }
                    }
                }
            }

            var merged = new List<DatEntrySet>();
            var rootToIndex = new Dictionary<int, int>();

            for (var i = 0; i < n; i++)
            {
                var root = Find(i);

                if (!rootToIndex.TryGetValue(root, out var idx))
                {
                    merged.Add(groups[i]);
                    rootToIndex[root] = merged.Count - 1;
                }
                else
                {
                    merged[idx].AppendEntries(groups[i]);
                }
            }

            return merged;
        }

        private void AddGameToParentNameEntriesDic(
            DatGameVO datGame,
            Dictionary<string, List<DatGameVO>> parentNameEntriesDic)
        {
            string raw;

            if (!datGame.HasParent)
            {
                if (!string.IsNullOrWhiteSpace(datGame.MameLaunchName))
                    raw = datGame.MameLaunchName;
                else
                    raw = datGame.NormalisedName;
            }
            else
            {
                raw = datGame.ParentName!;
            }

            var nameKey = NormaliseKey(raw);
            var seedKey = options.EnableCrossDatMerging ? nameKey : (GetDatKey(datGame) + "|" + nameKey);

            autoGroupTraceStore?.RecordSeed(datGame, raw, seedKey);

            if (!parentNameEntriesDic.TryGetValue(seedKey, out var list))
            {
                parentNameEntriesDic.Add(seedKey, new List<DatGameVO> { datGame });
                return;
            }

            var joinReason =
                datGame.HasParent ? AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinParentClone :
                !string.IsNullOrWhiteSpace(datGame.MameLaunchName) ? AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinMameLaunchCategory :
                AUTO_GROUP_TRACE_REASON_ENUM.SeedJoinNormalisedName;

            var samples = BuildExistingSeedBucketSamples(list);
            autoGroupTraceStore?.RecordSeedJoin(datGame, joinReason, seedKey, list.Count, samples);

            if (datGame.HasParent)
            {
                list.Add(datGame);
                return;
            }

            var firstChildIndex = -1;

            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].HasParent)
                {
                    firstChildIndex = i;
                    break;
                }
            }

            if (firstChildIndex < 0)
                list.Add(datGame);
            else
                list.Insert(firstChildIndex, datGame);
        }

        private static List<string> BuildExistingSeedBucketSamples(IReadOnlyList<DatGameVO> list)
        {
            var samples = new List<string>(3);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < list.Count; i++)
            {
                var name = list[i].Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (!seen.Add(name))
                    continue;

                samples.Add(name);

                if (samples.Count >= 3)
                    break;
            }

            return samples;
        }

        private string GetDatKey(DatGameVO g)
        {
            if (!EntryDatKeyDic.TryGetValue(g, out var key))
                throw new KeyNotFoundException($"EntryDatKeyDic missing key for DatGameVO instance. Name='{g.Name}', Fingerprint='{g.Fingerprint}'.");

            return key;
        }

        private void RemoveAllEntries()
        {
            entryGroups.Clear();
        }
    }
}
