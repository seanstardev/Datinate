using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class DatGrouperMediaExportEntryDTO
    {
        public MEDIA_TYPE_ENUM MediaTypeEnum { get; }
        public IReadOnlyList<DatGrouperMediaExportSourceDTO> Sources { get; }

        public DatGrouperMediaExportEntryDTO(
            MEDIA_TYPE_ENUM mediaTypeEnum,
            IReadOnlyList<DatGrouperMediaExportSourceDTO> entries)
        {
            MediaTypeEnum = mediaTypeEnum;
            Sources = entries;
        }
    }

    public class DatGrouperMediaExportSourceDTO
    {
        public string SourceId { get; }
        public bool Include { get; }

        public DatGrouperMediaExportSourceDTO(string sourceId, bool include)
        {
            SourceId = sourceId;
            Include = include;
        }
    }
}
