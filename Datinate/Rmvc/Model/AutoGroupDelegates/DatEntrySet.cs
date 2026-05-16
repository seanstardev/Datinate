using com.RADIO.Datinate.RMVC.Shared;

namespace com.RADIO.Datinate.RMVC
{
    public sealed class DatEntrySet
    {
        public IEnumerable<string> EntryFingerprints => fingerprintHash;
        public IEnumerable<string> EntryNamePublishers => namePublisherKeyHashSet;
        public IEnumerable<string> EntryNameRegions => nameRegionKeyHashSet;
        public IEnumerable<string> EntryNames => entryNormalisedNameKeyHashSet;

        public IReadOnlyList<DatGameVO> Entries => entries;

        public Dictionary<string, List<DatGameVO>> FingerprintDatGamesDic { get; private set; } =
            new Dictionary<string, List<DatGameVO>>(StringComparer.Ordinal);

        private readonly IReadOnlyDictionary<DatGameVO, string> entryDatKeyDic;
        private readonly IReadOnlyDictionary<string, int> datPriorityByDatKey;
        private readonly bool groupsAcrossDATs;

        private readonly HashSet<string> namePublisherKeyHashSet = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> nameRegionKeyHashSet = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> fingerprintHash = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> entryNormalisedNameKeyHashSet = new HashSet<string>(StringComparer.Ordinal);

        private readonly List<DatGameVO> entries = new List<DatGameVO>();

        public DatEntrySet(
            IEnumerable<DatGameVO> datGames,
            IReadOnlyDictionary<DatGameVO, string> entryDatKeyDic,
            IReadOnlyDictionary<string, int> datPriorityByDatKey,
            bool groupsAcrossDATs)
        {
            this.entryDatKeyDic = entryDatKeyDic;
            this.datPriorityByDatKey = datPriorityByDatKey;
            this.groupsAcrossDATs = groupsAcrossDATs;

            AddEntries(datGames);
            RebuildFingerprintSet();
        }

        public void AddEntries(DatEntrySet entryGroup)
        {
            AppendEntries(entryGroup);
            RebuildFingerprintSet();
        }

        public void AppendEntries(DatEntrySet other)
        {
            for (var i = 0; i < other.entries.Count; i++)
                AddEntry(other.entries[i]);
        }

        public void Empty()
        {
            entries.Clear();
            namePublisherKeyHashSet.Clear();
            nameRegionKeyHashSet.Clear();
            fingerprintHash.Clear();
            entryNormalisedNameKeyHashSet.Clear();
            FingerprintDatGamesDic.Clear();
        }

        public void RebuildFingerprintSet()
        {
            FingerprintDatGamesDic = new Dictionary<string, List<DatGameVO>>(StringComparer.Ordinal);

            for (var i = 0; i < entries.Count; i++)
            {
                var datGame = entries[i];
                var fp = datGame.Fingerprint.ToLowerInvariant();

                if (!FingerprintDatGamesDic.TryGetValue(fp, out var list))
                {
                    list = new List<DatGameVO>();
                    FingerprintDatGamesDic.Add(fp, list);
                }

                list.Add(datGame);
            }

            foreach (var kvp in FingerprintDatGamesDic)
            {
                var list = kvp.Value;

                list.Sort((a, b) =>
                {
                    var pa = GetDatPriority(a);
                    var pb = GetDatPriority(b);
                    if (pa != pb) return pa.CompareTo(pb);

                    if (a.HasParent != b.HasParent) return a.HasParent ? 1 : -1;

                    return StringComparer.Ordinal.Compare(a.Name, b.Name);
                });
            }
        }

        private int GetDatPriority(DatGameVO g)
        {
            if (!entryDatKeyDic.TryGetValue(g, out var datKey))
                throw new KeyNotFoundException($"EntryDatKeyDic missing key for DatGameVO instance. Name='{g.Name}', Fingerprint='{g.Fingerprint}'.");

            if (!datPriorityByDatKey.TryGetValue(datKey, out var p))
                throw new KeyNotFoundException($"datPriorityByDatKey missing key for DatAdvanced '{datKey}'.");

            return p;
        }

        private void AddEntries(IEnumerable<DatGameVO> datGames)
        {
            foreach (var datGame in datGames)
                AddEntry(datGame);
        }

        private void AddEntry(DatGameVO datGame)
        {
            entries.Add(datGame);

            if (!entryDatKeyDic.TryGetValue(datGame, out var datKey))
                throw new KeyNotFoundException($"EntryDatKeyDic missing key for DatGameVO instance. Name='{datGame.Name}', Fingerprint='{datGame.Fingerprint}'.");

            var prefix = groupsAcrossDATs ? string.Empty : (datKey + "|");

            if (!string.IsNullOrWhiteSpace(datGame.NamePublisherKey))
            {
                var pk = DatEntriesCollection.NormaliseKey(datGame.NamePublisherKey);
                namePublisherKeyHashSet.Add(prefix + pk);
            }

            if (!string.IsNullOrWhiteSpace(datGame.NameRegionKey))
            {
                var rk = DatEntriesCollection.NormaliseKey(datGame.NameRegionKey);
                nameRegionKeyHashSet.Add(prefix + rk);
            }

            fingerprintHash.Add(datGame.Fingerprint);

            var nameKey = DatEntriesCollection.NormaliseKey(datGame.NormalisedName);
            entryNormalisedNameKeyHashSet.Add(prefix + nameKey);
        }
    }
}
