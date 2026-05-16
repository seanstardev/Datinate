using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class MediaExportPriorityItemDTO
    {
        public string SourceId { get; }
        public DAT_GROUP_ENUM DatGroupEnum { get; }
        public string? ResourceSourceName { get; }
        public bool IsScoringItem { get; }
        public int EntryCount { get; }
        public bool Include { get; }

        public MediaExportPriorityItemDTO(
            string sourceId,
            DAT_GROUP_ENUM datGroupEnum,
            string? resourceSourceName,
            bool isScoringItem,
            int entryCount,
            bool include = true)
        {
            SourceId = sourceId;
            DatGroupEnum = datGroupEnum;
            ResourceSourceName = resourceSourceName;
            IsScoringItem = isScoringItem;
            EntryCount = entryCount;
            Include = include;
        }

        public MediaExportPriorityItemDTO WithInclude(bool include)
        {
            return new MediaExportPriorityItemDTO(
                SourceId,
                DatGroupEnum,
                ResourceSourceName,
                IsScoringItem,
                EntryCount,
                include);
        }
    }
}