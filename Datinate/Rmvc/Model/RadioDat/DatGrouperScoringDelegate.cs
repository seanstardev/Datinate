using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Rb;
using RadioLibCore.RadioResource;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public static class DatGrouperScoringDelegate
    {
        public static ResourceDetailsDTO? CreateResourceDetails(
            string lookupName, 
            string sourceId, 
            DatVO dat, 
            InfoVO? info)
        {
            var entry = dat.Entries.FirstOrDefault(e => e.Name == lookupName);
            if (entry == null) return null;

            Dictionary<string, MEDIA_TYPE_ENUM> dictionary = new Dictionary<string, MEDIA_TYPE_ENUM>();

            foreach (var rom in entry.Roms)
            {
                var type_ = R2DatWebFlag.Media.GetTypeEnum(rom.Name);
                _ = dictionary.TryAdd(rom.Name, type_);
            }

            return new ResourceDetailsDTO(sourceId, lookupName, info, dictionary);
        }
        public static DatGrouperScoring? CreateScoring(
            RbMediaCollection collection, 
            DatGrouperProjectDTO project,
            Dictionary<string, ResourceDetailsDTO> sourceIdResourceDictionary,
            Dictionary<string, RadioSourceDTO> sourceIdContentDictionary)
        {
            var excludeCodes = project.ExcludedDescriptorCodes;

            Dictionary<MEDIA_TYPE_ENUM, DatGrouperScoringItem> resources = new Dictionary<MEDIA_TYPE_ENUM, DatGrouperScoringItem>();
            Dictionary<MEDIA_TYPE_ENUM, DatGrouperScoringItem> media = new Dictionary<MEDIA_TYPE_ENUM, DatGrouperScoringItem>();

            var assignedItems = collection.SourceIdAssignmentDictionary.Values.Where(a =>
                    a.AssignmentEnum == MEDIA_ASSIGNMENT_ENUM.Assigned);

            foreach (var scoringMedia in project.ScoringMediaTypes)
            {
                var mediaEnum = DatinateHelper.GetEnumFromString<MEDIA_TYPE_ENUM>(scoringMedia, MEDIA_TYPE_ENUM.NOT_SET);
                if (mediaEnum == MEDIA_TYPE_ENUM.NOT_SET)
                    continue;

                // Resources - Info
                else if (mediaEnum == MEDIA_TYPE_ENUM.Info)
                {
                    DatGrouperScoringItem? resourceItem;

                    // About
                    resourceItem = BuildResourceItem(
                        project, 
                        GetResourceAboutSources(sourceIdResourceDictionary), 
                        MEDIA_TYPE_ENUM.Info_About);
                    
                    if (resourceItem != null)
                        resources.Add(MEDIA_TYPE_ENUM.Info_About, resourceItem);

                    // Releases
                    resourceItem = BuildResourceItem(
                        project, 
                        GetResourceReleasesSources(sourceIdResourceDictionary), 
                        MEDIA_TYPE_ENUM.Info_Releases);
                    
                    if (resourceItem != null)
                        resources.Add(MEDIA_TYPE_ENUM.Info_Releases, resourceItem);

                    // Credits
                    resourceItem = BuildResourceItem(
                        project, 
                        GetResourceCreditsSources(sourceIdResourceDictionary),
                        MEDIA_TYPE_ENUM.Info_Credits);

                    if (resourceItem != null)
                        resources.Add(MEDIA_TYPE_ENUM.Info_Credits, resourceItem);
                }

                // Media and Resources
                else
                {
                    DatGrouperScoringItem? mediaItem = BuildMediaItem(
                        project,
                        GetMediaSources(
                            mediaEnum, 
                            assignedItems, 
                            sourceIdContentDictionary, 
                            sourceIdResourceDictionary),
                            mediaEnum);

                    if (mediaItem != null)
                        media.Add(mediaEnum, mediaItem);
                }
            }


            bool isScoringExempt =
                collection.CheckedDescriptorCodes.Any(checkedCode =>
                    excludeCodes.Contains(checkedCode, StringComparer.Ordinal));

            Dictionary<MEDIA_TYPE_ENUM, DatGrouperScoringItem> orderedMedia = new();
            HashSet<MEDIA_TYPE_ENUM> addedMediaEnums = new();

            foreach (var scoringMedia in project.ScoringMediaTypes)
            {
                var mediaEnum = DatinateHelper.GetEnumFromString<MEDIA_TYPE_ENUM>(scoringMedia, MEDIA_TYPE_ENUM.NOT_SET);

                if (mediaEnum == MEDIA_TYPE_ENUM.NOT_SET || mediaEnum == MEDIA_TYPE_ENUM.Info)
                    continue;

                if (!addedMediaEnums.Add(mediaEnum))
                    continue;

                if (media.TryGetValue(mediaEnum, out var mediaItem))
                    orderedMedia.Add(mediaEnum, mediaItem);
            }

            var scoring = new DatGrouperScoring(
                isScoringExempt,
                resources,
                orderedMedia);

            return scoring;
        }

        private static IReadOnlyList<string> GetMediaSources(
            MEDIA_TYPE_ENUM mediaEnum, 
            IEnumerable<RbMediaItemAssignment> assignedItems,
            Dictionary<string, RadioSourceDTO> sourceIdContentDictionary,
            Dictionary<string, ResourceDetailsDTO> sourceIdResourceDictionary)
        {
            var list = new List<string>();
            foreach (var assignment in assignedItems)
            {
                if (sourceIdContentDictionary.TryGetValue(assignment.SourceId, out var dto) && dto.Source == mediaEnum.ToString())
                    list.Add(assignment.SourceId);
            }

            foreach (var kvp in sourceIdResourceDictionary)
            {
                var resourceSet = kvp.Value;
                var resourceMatches = resourceSet.GetRomNamesOfType(mediaEnum);

                // TODO: Feels like a hack...
                foreach (var match in resourceMatches)
                    list.Add(kvp.Key);
            }

            return list;
        }

        private static IReadOnlyList<string> GetResourceCreditsSources(Dictionary<string, ResourceDetailsDTO> sourceIdResourceDictionary)
        {
            var list = new List<string>();
            foreach (var kvp in sourceIdResourceDictionary)
            {
                var info = kvp.Value.Info;

                if (info != null && info.CreditVOs != null && info.CreditVOs.Length > 0)
                    list.Add(kvp.Key);
            }
            return list;
        }

        private static IReadOnlyList<string> GetResourceReleasesSources(Dictionary<string, ResourceDetailsDTO> sourceIdResourceDictionary)
        {
            var list = new List<string>();
            foreach (var kvp in sourceIdResourceDictionary)
            {
                var info = kvp.Value.Info;

                if (info != null && info.ReleaseVOs != null && info.ReleaseVOs.Length > 0)
                    list.Add(kvp.Key);
            }
            return list;
        }

        private static IReadOnlyList<string> GetResourceAboutSources(Dictionary<string, ResourceDetailsDTO> sourceIdResourceDictionary)
        {
            var list = new List<string>();
            foreach (var kvp in sourceIdResourceDictionary)
            {
                var info = kvp.Value.Info;

                if (info != null && info.DescriptionVO != null 
                    && !string.IsNullOrWhiteSpace(info.DescriptionVO.Description)
                    && !info.DescriptionVO.IsBoilerplate)
                    list.Add(kvp.Key);
            }
            return list;
        }
        private static DatGrouperScoringItem? BuildMediaItem(DatGrouperProjectDTO project, IReadOnlyList<string> sourceIds, MEDIA_TYPE_ENUM mediaEnum)
        {
            var icon1 = DatinateHelper.GetMediaIconBmp(mediaEnum);

            if (icon1 == null) return null;

            return new DatGrouperScoringItem(mediaEnum, sourceIds, icon1, null);
        }

        private static DatGrouperScoringItem? BuildResourceItem(DatGrouperProjectDTO project, IReadOnlyList<string> sourceIds, MEDIA_TYPE_ENUM mediaEnum)
        {
            var icon1 = DatinateHelper.GetMediaIconBmp(MEDIA_TYPE_ENUM.Info);
            var icon2 = DatinateHelper.GetMediaIconBmp(mediaEnum);
            
            if (icon1 == null || icon2 == null) return null;

            return new DatGrouperScoringItem(mediaEnum, sourceIds, icon1, icon2);
        }
    }
}
