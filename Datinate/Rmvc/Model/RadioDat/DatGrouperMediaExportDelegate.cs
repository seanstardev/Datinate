using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Rb;
using RadioLibCore.RadioDat;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public static class DatGrouperMediaExportDelegate
    {
        private const bool CountOnlyAssignedEntries = true;

        public static IReadOnlyDictionary<MEDIA_TYPE_ENUM, IReadOnlyList<MediaExportPriorityItemDTO>> CreateMediaExportPriorityDictionary(
            IReadOnlyList<IGameFamily> curatedFamilies,
            IReadOnlyCollection<MEDIA_TYPE_ENUM> mediaTypes,
            DatGrouperProjectDTO? activeProject,
            IReadOnlyDictionary<IGameFamily, RbMediaCollection> mediaCollectionsDictionary,
            IReadOnlyDictionary<string, ILookupSet> sourceIdLookupSetDictionary,
            IReadOnlyDictionary<string, DatVO> sourceIdDatDictionary,
            Func<string, string, ResourceDetailsDTO?> getResourceDetails)
        {
            var expandedMediaTypes = ExpandMediaExportTypes(mediaTypes);
            var scoringMediaTypes = CreateExpandedScoringMediaTypeSet(activeProject);
            var assignedLookupsByKey = new Dictionary<MediaExportPriorityKey, HashSet<string>>();

            foreach (var family in curatedFamilies)
            {
                if (!mediaCollectionsDictionary.TryGetValue(family, out var collection))
                    continue;

                foreach (var assignment in collection.SourceIdAssignmentDictionary.Values)
                {
                    if (assignment.AssignmentEnum != MEDIA_ASSIGNMENT_ENUM.Assigned)
                        continue;

                    if (!sourceIdLookupSetDictionary.TryGetValue(assignment.SourceId, out var lookupSet))
                        continue;

                    if (lookupSet.IsMedia)
                    {
                        foreach (var mediaTypeEnum in expandedMediaTypes)
                        {
                            if (IsResourceOnlyMediaType(mediaTypeEnum))
                                continue;

                            if (lookupSet.RadioSource.Source == mediaTypeEnum.ToString())
                            {
                                var assignedLookupName = assignment.LookupName ?? assignment.EntryName;

                                if (string.IsNullOrWhiteSpace(assignedLookupName))
                                    continue;

                                var key = new MediaExportPriorityKey(
                                    mediaTypeEnum,
                                    lookupSet.RadioSource.DatGroupEnum,
                                    assignment.SourceId,
                                    DatinateHelper.GetDatFriendlyName(lookupSet.RadioSource.Id));

                                AddAssignedLookup(
                                    assignedLookupsByKey,
                                    key,
                                    assignedLookupName);
                            }
                        }

                        continue;
                    }

                    if (!lookupSet.IsRadioResource)
                        continue;

                    if (string.IsNullOrWhiteSpace(assignment.LookupName))
                        continue;

                    var resourceDetails = getResourceDetails(assignment.SourceId, assignment.LookupName);

                    if (resourceDetails == null)
                        continue;

                    foreach (var mediaTypeEnum in expandedMediaTypes)
                    {
                        if (ResourceHasMediaType(resourceDetails, mediaTypeEnum))
                        {
                            var key = new MediaExportPriorityKey(
                                mediaTypeEnum,
                                lookupSet.RadioSource.DatGroupEnum,
                                assignment.SourceId,
                                lookupSet.RadioSource.Source);

                            AddAssignedLookup(
                                assignedLookupsByKey,
                                key,
                                assignment.LookupName);
                        }
                    }
                }
            }

            return expandedMediaTypes
                .ToDictionary(
                    mediaTypeEnum => mediaTypeEnum,
                    mediaTypeEnum => (IReadOnlyList<MediaExportPriorityItemDTO>)assignedLookupsByKey
                        .Where(kvp => kvp.Key.MediaTypeEnum == mediaTypeEnum)
                        .Select(kvp => new
                        {
                            Key = kvp.Key,
                            EntryCount = CountOnlyAssignedEntries
                                ? kvp.Value.Count
                                : CountTotalEntriesForSource(
                                    kvp.Key.SourceId,
                                    kvp.Key.MediaTypeEnum,
                                    kvp.Key.ResourceName != null,
                                    sourceIdDatDictionary,
                                    getResourceDetails)
                        })
                        .OrderByDescending(item => item.EntryCount)
                        .ThenBy(item => item.Key.DatGroupEnum)
                        .ThenBy(item => item.Key.SourceId, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(item => item.Key.ResourceName, StringComparer.OrdinalIgnoreCase)
                        .Select(item => new MediaExportPriorityItemDTO(
                            item.Key.SourceId,
                            item.Key.DatGroupEnum,
                            item.Key.ResourceName,
                            scoringMediaTypes.Contains(item.Key.MediaTypeEnum),
                            item.EntryCount))
                        .ToArray());
        }
        private static void AddAssignedLookup(
            Dictionary<MediaExportPriorityKey, HashSet<string>> assignedLookupsByKey,
            MediaExportPriorityKey key,
            string lookupName)
        {
            if (!assignedLookupsByKey.TryGetValue(key, out var lookups))
            {
                lookups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                assignedLookupsByKey[key] = lookups;
            }

            _ = lookups.Add(lookupName);
        }
        private static bool IsResourceOnlyMediaType(MEDIA_TYPE_ENUM mediaTypeEnum)
        {
            return mediaTypeEnum is
                MEDIA_TYPE_ENUM.Info_About or
                MEDIA_TYPE_ENUM.Info_Releases or
                MEDIA_TYPE_ENUM.Info_Credits or
                MEDIA_TYPE_ENUM.Thumb;
        }
        private readonly record struct MediaExportPriorityKey(
            MEDIA_TYPE_ENUM MediaTypeEnum,
            DAT_GROUP_ENUM DatGroupEnum,
            string SourceId,
            string? ResourceName);
        private static int CountTotalEntriesForSource(
            string sourceId,
            MEDIA_TYPE_ENUM mediaTypeEnum,
            bool isResourceSource,
            IReadOnlyDictionary<string, DatVO> sourceIdDatDictionary,
            Func<string, string, ResourceDetailsDTO?> getResourceDetails)
        {
            if (!sourceIdDatDictionary.TryGetValue(sourceId, out var dat))
                return 0;

            if (!isResourceSource)
                return dat.Entries.Count;

            if (!RequiresInfoFile(mediaTypeEnum))
            {
                return dat.Entries.Count(entry =>
                    entry.Roms.Any(rom =>
                        R2DatWebFlag.Media.GetTypeEnum(rom.Name) == mediaTypeEnum));
            }

            var count = 0;

            foreach (var entry in dat.Entries)
            {
                var resourceDetails = getResourceDetails(sourceId, entry.Name);

                if (resourceDetails == null)
                    continue;

                if (ResourceHasMediaType(resourceDetails, mediaTypeEnum))
                    count++;
            }

            return count;
        }
        private static bool RequiresInfoFile(MEDIA_TYPE_ENUM mediaTypeEnum)
        {
            return mediaTypeEnum is
                MEDIA_TYPE_ENUM.Info_About or
                MEDIA_TYPE_ENUM.Info_Releases or
                MEDIA_TYPE_ENUM.Info_Credits;
        }
        private static IReadOnlyCollection<MEDIA_TYPE_ENUM> ExpandMediaExportTypes(
            IReadOnlyCollection<MEDIA_TYPE_ENUM> mediaTypes)
        {
            var set = new HashSet<MEDIA_TYPE_ENUM>
            {
                MEDIA_TYPE_ENUM.Info_About,
                MEDIA_TYPE_ENUM.Info_Releases,
                MEDIA_TYPE_ENUM.Info_Credits,
                MEDIA_TYPE_ENUM.Thumb
            };

            foreach (var mediaType in mediaTypes)
            {
                if (mediaType == MEDIA_TYPE_ENUM.Info)
                {
                    _ = set.Add(MEDIA_TYPE_ENUM.Info_About);
                    _ = set.Add(MEDIA_TYPE_ENUM.Info_Releases);
                    _ = set.Add(MEDIA_TYPE_ENUM.Info_Credits);
                    continue;
                }

                if (mediaType != MEDIA_TYPE_ENUM.NOT_SET && mediaType != MEDIA_TYPE_ENUM.Unspecified)
                    _ = set.Add(mediaType);
            }

            return set;
        }
        private static HashSet<MEDIA_TYPE_ENUM> CreateExpandedScoringMediaTypeSet(
            DatGrouperProjectDTO? activeProject)
        {
            var set = new HashSet<MEDIA_TYPE_ENUM>();

            if (activeProject == null)
                return set;

            foreach (var scoringMedia in activeProject.ScoringMediaTypes)
            {
                var mediaTypeEnum = DatinateHelper.GetEnumFromString<MEDIA_TYPE_ENUM>(
                    scoringMedia,
                    MEDIA_TYPE_ENUM.NOT_SET);

                if (mediaTypeEnum == MEDIA_TYPE_ENUM.NOT_SET)
                    continue;

                if (mediaTypeEnum == MEDIA_TYPE_ENUM.Info)
                {
                    _ = set.Add(MEDIA_TYPE_ENUM.Info_About);
                    _ = set.Add(MEDIA_TYPE_ENUM.Info_Releases);
                    _ = set.Add(MEDIA_TYPE_ENUM.Info_Credits);
                    continue;
                }

                _ = set.Add(mediaTypeEnum);
            }

            return set;
        }

        private static bool ResourceHasMediaType(ResourceDetailsDTO resourceDetails, MEDIA_TYPE_ENUM mediaTypeEnum)
        {
            var info = resourceDetails.Info;

            return mediaTypeEnum switch
            {
                MEDIA_TYPE_ENUM.Info_About =>
                    info != null
                    && info.DescriptionVO != null
                    && !string.IsNullOrWhiteSpace(info.DescriptionVO.Description)
                    && !info.DescriptionVO.IsBoilerplate,

                MEDIA_TYPE_ENUM.Info_Releases =>
                    info != null
                    && info.ReleaseVOs != null
                    && info.ReleaseVOs.Length > 0,

                MEDIA_TYPE_ENUM.Info_Credits =>
                    info != null
                    && info.CreditVOs != null
                    && info.CreditVOs.Length > 0,

                _ => resourceDetails.GetRomNamesOfType(mediaTypeEnum).Count > 0
            };
        }
    }
}