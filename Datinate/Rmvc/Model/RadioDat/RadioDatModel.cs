using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using Datinate.Properties;
using Datinate.Shared.Rb;
using RadioLibCore.RadioDat;
using RadioLibCore.RadioResource;
using RMVC;
using System.Collections;
using System.Diagnostics;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class RadioDatModel : RModel
    {
        public DatGrouperProjectDTO? ActiveProject { get; private set; } = null;

        public string? ProjectName => ActiveProject?.ProjectName;
        public RadioDatMeta? DatMeta { get; private set; }

        // NOTE: Probably slow, but only used sparingly.
        // TODO: Make this a function.
        public IReadOnlyDictionary<string, ISourceDefinition> SourceIdContentDictionary =>
            sourceIdContentDictionary.ToDictionary(kvp => kvp.Key, kvp => (ISourceDefinition)kvp.Value);

        public AutoGrouperTraceStore? AutoGroupTraceStore { get; private set; } = null;


        public IReadOnlySet<string> DescriptorDefinitions => descriptorDefinitions.Select(d => d.Code).ToHashSet<string>();

        private readonly Dictionary<string, DatVO> sourceIdDatDictionary = [];
        private readonly Dictionary<string, ILookupSet> sourceIdLookupSetDictionary = [];

        private readonly Dictionary<string, RadioSourceDTO> sourceIdContentDictionary = new Dictionary<string, RadioSourceDTO>();
        
        private readonly Dictionary<string, ResourceDetailsDTO> sourceIdResourceDictionary = new Dictionary<string, ResourceDetailsDTO>();

        private readonly Dictionary<IGameFamily, RbMediaCollection> mediaCollectionsDictionary =
            new Dictionary<IGameFamily, RbMediaCollection>(ReferenceEqualityComparer.Instance);

        private IReadOnlyDictionary<IGameFamily, IMediaCollection>? mediaCollectionsDictionaryView;

        private readonly Dictionary<string, HashSet<string>> assignedMediaEntries = new Dictionary<string, HashSet<string>>();

        private readonly Dictionary<string, bool> existingContentPaths = new Dictionary<string, bool>();
        private IReadOnlySet<DescriptorDefinitionDTO> descriptorDefinitions = new HashSet<DescriptorDefinitionDTO>();
        public void SetDescriptorDefinitions(IReadOnlySet<DescriptorDefinitionDTO> descriptorDefinitions)
        {
            this.descriptorDefinitions = descriptorDefinitions;
        }

        
        private IEnumerable<GameFamilyVO>? autoGrouperSourceCollection = null;

        private static readonly object gate = new();
        public RadioDatModel()
        {
        }
        public IReadOnlyDictionary<string, DatVO> CreateSourceDatExportSnapshot()
        {
            lock (gate)
            {
                return sourceIdDatDictionary.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value,
                    StringComparer.OrdinalIgnoreCase);
            }
        }
        public IReadOnlyDictionary<IGameFamily, IReadOnlyDictionary<string, ResourceDetailsDTO>> GetResourceDetailsDictionary(
            IReadOnlyList<IGameFamily> curatedFamilies)
        {
            List<(IGameFamily Family, string SourceId, string LookupName)> pending = [];

            lock (gate)
            {
                foreach (var family in curatedFamilies)
                {
                    if (!mediaCollectionsDictionary.TryGetValue(family, out var collection))
                        continue;

                    foreach (var kvp in collection.SourceIdAssignedItemDictionary)
                    {
                        var item = kvp.Value;

                        if (string.IsNullOrWhiteSpace(item.SourceId))
                            continue;

                        if (string.IsNullOrWhiteSpace(item.LookupName))
                            continue;

                        pending.Add((family, item.SourceId, item.LookupName));
                    }
                }
            }

            var mutableResult =
                new Dictionary<IGameFamily, Dictionary<string, ResourceDetailsDTO>>(ReferenceEqualityComparer.Instance);

            foreach (var item in pending)
            {
                var details = GetResourceDetails(item.SourceId, item.LookupName);

                if (details == null)
                    continue;

                if (!mutableResult.TryGetValue(item.Family, out var sourceDictionary))
                {
                    sourceDictionary = new Dictionary<string, ResourceDetailsDTO>(StringComparer.OrdinalIgnoreCase);
                    mutableResult.Add(item.Family, sourceDictionary);
                }

                _ = sourceDictionary.TryAdd(item.SourceId, details);
            }

            var result =
                new Dictionary<IGameFamily, IReadOnlyDictionary<string, ResourceDetailsDTO>>(ReferenceEqualityComparer.Instance);

            foreach (var kvp in mutableResult)
                result.Add(kvp.Key, kvp.Value);

            return result;
        }
        public IReadOnlyDictionary<MEDIA_TYPE_ENUM, IReadOnlyList<MediaExportPriorityItemDTO>> CreateMediaExportPriorityDictionary(
            IReadOnlyList<IGameFamily> curatedFamilies,
            IReadOnlyCollection<MEDIA_TYPE_ENUM> mediaTypes)
        {
            lock (gate)
            {
                return DatGrouperMediaExportDelegate.CreateMediaExportPriorityDictionary(
                    curatedFamilies,
                    mediaTypes,
                    ActiveProject,
                    mediaCollectionsDictionary,
                    sourceIdLookupSetDictionary,
                    sourceIdDatDictionary,
                    GetResourceDetails);
            }
        }
        public void Reset()
        {
            lock (gate)
            {
                ActiveProject = null;
                DatMeta = null;
                AutoGroupTraceStore = null;

                sourceIdResourceDictionary.Clear();
                sourceIdDatDictionary.Clear();
                sourceIdContentDictionary.Clear();
                sourceIdLookupSetDictionary.Clear();
                mediaCollectionsDictionary.Clear();
                assignedMediaEntries.Clear();
                existingContentPaths.Clear();
                descriptorDefinitions = new HashSet<DescriptorDefinitionDTO>();

                autoGrouperSourceCollection = null;
            }
        }
        public void AddDats(Dictionary<string, DatVO> dats)
        {
            lock(gate)
                foreach (var kvp in dats)
                    _ = sourceIdDatDictionary.TryAdd(kvp.Key, kvp.Value);
        }
        
        public void SetRadioDat(
            DatGrouperProjectDTO projectVO, 
            AutoGrouperTraceStore? autoGroupTraceStore,
            IEnumerable<GameFamilyVO> autoGrouperSourceCollection) 
        {
            this.ActiveProject = projectVO;
            this.AutoGroupTraceStore = autoGroupTraceStore;
            this.autoGrouperSourceCollection = autoGrouperSourceCollection;

            DatMeta = new RadioDatMeta([], [], projectVO.Comment);

            lock (gate)
            {
                sourceIdContentDictionary.Clear();
                mediaCollectionsDictionary.Clear();
            }
        }

        public void SetLookupSets(List<ILookupSet> lookupSets)
        {
            lock (gate)
                foreach (var lookupSet in lookupSets)
                _ = sourceIdLookupSetDictionary.TryAdd(lookupSet.Id, lookupSet);
        }

        public IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> CreateExportCollection(
            IReadOnlyList<IGameFamily> curatedFamilies)
        {
            var dic = new Dictionary<IGameFamily, IMediaCollectionImportExport?>();

            foreach (var family in curatedFamilies)
            {
                _ = mediaCollectionsDictionary.TryGetValue(family, out var mc);

                var payload = (mc == null || mc.IsEmptyForExport) ? null : mc;
                dic.Add(family, payload);
            }

            return dic;
        }
        public IReadOnlyDictionary<IGameFamily, IMediaCollection> ImportMediaUpdates(
            IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> importedCollection,
            IReadOnlyDictionary<IGameFamily, IGameFamily> deltaRemaps)
        {
            var mediaUpdates = new List<RbMediaItemAssignmentUpdate>();
            var metaUpdates = new List<PendingMetaUpdate>();

            lock (gate)
            {
                foreach (var kv in importedCollection)
                {
                    var importedFamily = kv.Key;

                    if (!TryResolveDeltaRemap(importedFamily, deltaRemaps, out var targetFamily) || targetFamily is null)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "ImportMediaUpdates: no delta remap found for an imported family; skipping media payload.");
                        continue;
                    }


                    var media = kv.Value;

                    if (media == null) continue;

                    foreach (var kvp_ in media.SourceIdAssignedItemDictionary)
                    {
                        var sourceId = kvp_.Key;
                        var mediaItem = kvp_.Value;

                        if (!sourceIdLookupSetDictionary.TryGetValue(sourceId, out var lookupSet))
                            continue;

                        string? entryName = lookupSet.GetEntry(mediaItem.LookupName);
                        if (string.IsNullOrWhiteSpace(entryName))
                            continue;

                        mediaUpdates.Add(new RbMediaItemAssignmentUpdate(
                            targetFamily,
                            sourceId,
                            DatinateEnums.MEDIA_ASSIGNMENT_ENUM.Assigned,
                            entryName,
                            mediaItem.LookupName));
                    }

                    if (media.CheckedDescriptorCodes.Count > 0 || !string.IsNullOrWhiteSpace(media.FamilyNotes))
                    {
                        metaUpdates.Add(new PendingMetaUpdate(
                            targetFamily,
                            media.CheckedDescriptorCodes.ToHashSet(),
                            media.FamilyNotes));
                    }
                }

                _ = UpdateMediaCollectionItems(mediaUpdates.ToArray());
                _ = UpdateMediaCollectionMetas(metaUpdates.ToArray());

                return MediaCollectionsView;
            }
        }

        public IReadOnlyDictionary<IGameFamily, IMediaCollection> UpdateMediaReferences(
            IReadOnlyDictionary<IGameFamily, IGameFamily> familyReferenceUpdates)
        {
            lock (gate)
            {
                foreach (var update in familyReferenceUpdates)
                {
                    var oldFamilyRef = update.Key;
                    var newFamilyRef = update.Value;

                    if (oldFamilyRef == newFamilyRef)
                        continue;

                    if (!mediaCollectionsDictionary.TryGetValue(oldFamilyRef, out var moving))
                        continue;

                    _ = mediaCollectionsDictionary.Remove(oldFamilyRef);

                    if (mediaCollectionsDictionary.TryGetValue(newFamilyRef, out var existing))
                    {
                        Debug.WriteLine("[WARN] Media Collection Collision. Untested scenario.");
                        // NOTE: Optional: merge moving -> existing.
                        // Current policy: keep existing, drop moving.
                        continue;
                    }

                    moving.Family = newFamilyRef;
                    mediaCollectionsDictionary[newFamilyRef] = moving;
                }
            }

            return MediaCollectionsView;
        }

        public IReadOnlyDictionary<IGameFamily, IMediaCollection> UpdateMediaCollectionItem(RbMediaItemAssignmentUpdate assignment)
        {
            return UpdateMediaCollectionItems([assignment]);
        }

        public IReadOnlyDictionary<IGameFamily, IMediaCollection> UpdateMediaCollectionMeta(
            IGameFamily family,
            HashSet<string> checkedDescriptors,
            string familyNotesText)
        {
            UpdateMediaCollectionMetas(
                [new PendingMetaUpdate(family, checkedDescriptors, familyNotesText)]);

            return MediaCollectionsView;
        }
        private IReadOnlyDictionary<IGameFamily, IMediaCollection> UpdateMediaCollectionMetas(PendingMetaUpdate[] metaUpdates)
        {
            lock (gate)
            {
                foreach (var meta in metaUpdates)
                {
                    if (!mediaCollectionsDictionary.TryGetValue(meta.Family, out var collection))
                        mediaCollectionsDictionary[meta.Family] = collection = 
                            new RbMediaCollection(
                                meta.Family, 
                                ActiveProject?.ExcludedDescriptorCodes ?? new HashSet<string>());

                    collection.CheckedDescriptorCodes = new HashSet<string>(meta.CheckedDescriptors);
                    collection.FamilyNotes = meta.FamilyNotesText;
                }
            }
            return MediaCollectionsView;
        }

        private IReadOnlyDictionary<IGameFamily, IMediaCollection> UpdateMediaCollectionItems(
            RbMediaItemAssignmentUpdate[] assignments)
        {
            lock (gate)
            {
                foreach (var assignment in assignments)
                {
                    if (!mediaCollectionsDictionary.TryGetValue(assignment.Family, out var collection))
                        mediaCollectionsDictionary[assignment.Family] = collection = 
                            new RbMediaCollection(assignment.Family,
                                ActiveProject?.ExcludedDescriptorCodes ?? new HashSet<string>());

                    var contentPathExists = false;
                    if (sourceIdContentDictionary.TryGetValue(assignment.SourceId, out var col))
                        if (existingContentPaths.TryGetValue(col.ContentPath, out var exists))
                            contentPathExists = exists;

                    collection.SourceIdAssignmentDictionary[assignment.SourceId] = new RbMediaItemAssignment(
                        assignment.SourceId,
                        assignment.AssignmentEnum,
                        assignment.EntryName,
                        assignment.LookupName,
                        contentPathExists);

                    if (!assignedMediaEntries.ContainsKey(assignment.SourceId))
                        assignedMediaEntries.Add(assignment.SourceId, new HashSet<string>());

                    var item = collection.SourceIdAssignmentDictionary[assignment.SourceId];
                    if (item.AssignmentEnum == MEDIA_ASSIGNMENT_ENUM.Assigned
                        && !string.IsNullOrWhiteSpace(item.EntryName))
                    {
                        _ = assignedMediaEntries[assignment.SourceId].Add(item.EntryName);
                    }
                    else if (!string.IsNullOrWhiteSpace(item.EntryName))
                        _ = assignedMediaEntries[assignment.SourceId].Remove(item.EntryName);
                }

                base.ExecuteCommand(new UpdateAssignedMediaCacheCmd(assignedMediaEntries));
                return MediaCollectionsView;
            }
        }

        public IReadOnlyList<GamePartVO> GetSourceGameParts()
        {
            List<GamePartVO> sourceParts = new List<GamePartVO>();

            if (autoGrouperSourceCollection == null)
                return sourceParts;

            foreach (var family in autoGrouperSourceCollection)
            {
                var games = family.Games;
                foreach (var game in games)
                {
                    var parts = game.Parts;
                    foreach (var part in parts)
                    {
                        sourceParts.Add(part);

                        // NOTE: Do not include aliases as shallow copies as it will break export related tech.
                    }
                }
            }
            return sourceParts;
        }
        private sealed class PendingMediaImportUpdate
        {
            public RbMediaItemAssignmentUpdate[] PendingAssignments { get; }
            public PendingMetaUpdate[] PendingMetaUpdates { get; }
            public PendingMediaImportUpdate(
                RbMediaItemAssignmentUpdate[] pendingAssignments,
                PendingMetaUpdate[] pendingMetaUpdates)
            {
                PendingAssignments = pendingAssignments;
                PendingMetaUpdates = pendingMetaUpdates;
            }
        }

        public ResourceDetailsDTO? GetResourceDetails(string sourceId, string lookupName)
        {
            lock (gate)
            {
                if (!sourceIdContentDictionary.TryGetValue(sourceId, out var dto))
                    return null;

                string path = Path.Combine(dto.ContentPath, lookupName);

                if (sourceIdResourceDictionary.TryGetValue(path, out var resourceDetails))
                    return resourceDetails;

                if (!sourceIdDatDictionary.TryGetValue(sourceId, out var dat))
                    return null;

                string fullPath = Path.Combine(path, "Info.xml");
                InfoVO? info = null;

                try
                {
                    if (File.Exists(fullPath))
                        info = InfoHelper.LoadInfoVO(fullPath);
                }
                catch
                {
                }

                var created = DatGrouperScoringDelegate.CreateResourceDetails(
                    lookupName,
                    sourceId,
                    dat,
                    info);

                if (created != null)
                {
                    _ = sourceIdResourceDictionary.TryAdd(path, created);
                    return created;
                }

                return null;
            }
        }
        public DatGrouperScoring? GetScoring(IGameFamily family)
        {
            if (ActiveProject == null) return null;

            lock (gate)
            {
                var collection = GetMediaCollection(family);

                if (collection == null) return null;

                Dictionary<string, ResourceDetailsDTO> localSourceIdResourceDictionary = new Dictionary<string, ResourceDetailsDTO>();

                foreach (var assignment in collection.SourceIdAssignmentDictionary.Values
                    .Where(a => a.AssignmentEnum == MEDIA_ASSIGNMENT_ENUM.Assigned 
                        && !string.IsNullOrWhiteSpace(a.LookupName)))
                {
                    var sourceId = assignment.SourceId;

                    var resourceDetails = GetResourceDetails(sourceId, assignment.LookupName!);
                    if (resourceDetails != null)
                        _ = localSourceIdResourceDictionary.TryAdd(sourceId, resourceDetails);
                }

                return DatGrouperScoringDelegate.CreateScoring(
                    collection, 
                    ActiveProject,
                    localSourceIdResourceDictionary,
                    sourceIdContentDictionary);
            }
        }
        public RbMediaCollection? GetMediaCollection(IGameFamily family)
        {
            lock (gate)
            {
                _ = mediaCollectionsDictionary.TryGetValue(family, out var collection);

                if (collection == null)
                    mediaCollectionsDictionary[family] = collection = 
                        new RbMediaCollection(family,
                                ActiveProject?.ExcludedDescriptorCodes ?? new HashSet<string>());

                return collection;
            }
        }

        public RadioSourceDTO? GetSource(string sourceId)
        {
            lock (gate)
            {
                return sourceIdContentDictionary[sourceId];
            }
        }

        public IReadOnlyCollection<RadioSourceDTO> CreateSourceSet(
            IReadOnlyCollection<DatGrouperProjectEntry> paths,
            COLLECTION_SET_ENUM collectionSetEnum)
        {
            List<RadioSourceDTO> list = new List<RadioSourceDTO>();

            var pathById =
                (paths)
                .GroupBy(p => p.ID, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var path in paths)
            {
                if (!string.IsNullOrWhiteSpace(path.ContentPath) && !existingContentPaths.ContainsKey(path.ContentPath))
                    existingContentPaths.Add(
                        path.ContentPath
                        , !string.IsNullOrWhiteSpace(path.ContentPath) && Directory.Exists(path.ContentPath));

                var dto = new RadioSourceDTO(
                    path.ID,
                    path.DatFullpath,
                    path.ContentPath ?? string.Empty,
                    path.DatGroupEnum,
                    collectionSetEnum,
                    null,
                    path.DatSubsetFilter?.Clone(),
                    path.InternalDescriptor,
                    path.HideInUi);

                list.Add(dto);

                lock (gate)
                    sourceIdContentDictionary[path.ID] = dto;
            }

            return list;
        }

        private IReadOnlyDictionary<IGameFamily, IMediaCollection> MediaCollectionsView
        {
            get
            {
                lock (gate)
                    return mediaCollectionsDictionaryView ??= new MediaCollectionsReadOnlyView(mediaCollectionsDictionary);
            }
        }

        private sealed class PendingMetaUpdate
        {
            public PendingMetaUpdate(IGameFamily family, HashSet<string> checkedDescriptors, string familyNotesText)
            {
                Family = family;
                CheckedDescriptors = checkedDescriptors;
                FamilyNotesText = familyNotesText;
            }

            public IGameFamily Family { get; }
            public HashSet<string> CheckedDescriptors { get; }
            public string FamilyNotesText { get; }
        }

        private sealed class MediaCollectionsReadOnlyView : IReadOnlyDictionary<IGameFamily, IMediaCollection>
        {
            private readonly Dictionary<IGameFamily, RbMediaCollection> _inner;

            public MediaCollectionsReadOnlyView(Dictionary<IGameFamily, RbMediaCollection> inner) => _inner = inner;

            public int Count { get { lock (gate) return _inner.Count; } }

            public IEnumerable<IGameFamily> Keys
            {
                get { lock (gate) return _inner.Keys.ToArray(); } // snapshot
            }

            public IEnumerable<IMediaCollection> Values
            {
                get { lock (gate) return _inner.Values.ToArray(); } // snapshot
            }

            public IMediaCollection this[IGameFamily key]
            {
                get { lock (gate) return _inner[key]; }
            }

            public bool ContainsKey(IGameFamily key)
            {
                lock (gate) return _inner.ContainsKey(key);
            }

            public bool TryGetValue(IGameFamily key, out IMediaCollection value)
            {
                lock (gate)
                {
                    if (_inner.TryGetValue(key, out var v))
                    {
                        value = v;
                        return true;
                    }

                    value = default!;
                    return false;
                }
            }

            public IEnumerator<KeyValuePair<IGameFamily, IMediaCollection>> GetEnumerator()
            {
                KeyValuePair<IGameFamily, IMediaCollection>[] snapshot;

                lock (gate)
                {
                    snapshot = new KeyValuePair<IGameFamily, IMediaCollection>[_inner.Count];
                    int i = 0;
                    foreach (var kvp in _inner)
                        snapshot[i++] = new KeyValuePair<IGameFamily, IMediaCollection>(kvp.Key, kvp.Value);
                }

                return ((IEnumerable<KeyValuePair<IGameFamily, IMediaCollection>>)snapshot).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private static string BuildImportFamilyKey(IGameFamily family)
        {
            var keys = new List<string>(256);

            var games = family.GetAllGames();
            for (int gi = 0; gi < games.Length; gi++)
            {
                var parts = games[gi].GetGameParts(false);
                for (int pi = 0; pi < parts.Length; pi++)
                {
                    var p = parts[pi];
                    var name = p.GetName();
                    var fp = FingerprintHelper.GetFingerprint(p);
                    keys.Add(name + "\u001F" + fp);
                }
            }

            keys.Sort(StringComparer.Ordinal);
            return string.Join("\u001E", keys);
        }

        private static bool TryResolveDeltaRemap(
            IGameFamily importedFamily,
            IReadOnlyDictionary<IGameFamily, IGameFamily> deltaRemaps,
            out IGameFamily? targetFamily)
        {
            if (deltaRemaps.TryGetValue(importedFamily, out var direct) && direct is not null)
            {
                targetFamily = direct;
                return true;
            }

            var key = BuildImportFamilyKey(importedFamily);

            foreach (var kv in deltaRemaps)
            {
                var k = kv.Key;
                var v = kv.Value;

                if (v is null)
                    continue;

                if (BuildImportFamilyKey(k) == key)
                {
                    targetFamily = v;
                    return true;
                }
            }

            targetFamily = null;
            return false;
        }
    }
}
