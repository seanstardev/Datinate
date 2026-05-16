using RadioLibCore.RadioResource;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Shared.Rb
{
    public class ResourceDetailsDTO
    {
        public string SourceId { get; }
        public string LookupName { get; }
        public InfoVO? Info { get; }
        public IReadOnlyDictionary<string, MEDIA_TYPE_ENUM> FilenameMediaTypeDictionary { get; }

        public ResourceDetailsDTO(
            string sourceId, 
            string lookupName,
            InfoVO? info,
            IReadOnlyDictionary<string, MEDIA_TYPE_ENUM> filenameMediaTypeDictionary)
        {
            SourceId = sourceId;
            LookupName = lookupName;
            Info = info;
            FilenameMediaTypeDictionary = filenameMediaTypeDictionary;
        }

        public IReadOnlyList<string> GetRomNamesOfType(MEDIA_TYPE_ENUM mediaEnum)
        {
            if (FilenameMediaTypeDictionary.Count == 0)
                return Array.Empty<string>();

            var list = new List<string>();

            foreach (var kvp in FilenameMediaTypeDictionary)
                if (kvp.Value == mediaEnum)
                    list.Add(kvp.Key);

            return list;
        }
    }
}
