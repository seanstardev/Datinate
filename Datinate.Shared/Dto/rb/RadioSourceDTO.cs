using com.RADIO.Datinate.RMVC.Shared;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Shared.Rb
{
    public interface ISourceDefinition
    {
        string Id { get; }
        COLLECTION_SET_ENUM CollectionSetEnum { get; }
        DatSubsetFilter? DatSubset { get; }
        string ContentPath { get; }
        DAT_GROUP_ENUM DatGroupEnum { get; }
        string DatFullpath { get; }
        string? Source { get; }
    }
    public class RadioSourceDTO : ISourceDefinition
    {
        public string Id { get; }
        public string DatFullpath { get; }
        public string ContentPath { get; }
        public DAT_GROUP_ENUM DatGroupEnum { get; }
        public COLLECTION_SET_ENUM CollectionSetEnum { get; }
        public bool IsRadioResource { get; }
        public string? Source { get; }
        public string? MameHashPath { get; }
        public DatSubsetFilter? DatSubset { get; }
        public bool Hide { get; }

        public RadioSourceDTO(
            string id,
            string datFullpath,
            string contentPath,
            DAT_GROUP_ENUM datGroup,
            COLLECTION_SET_ENUM collectionSetEnum,
            string? mameHashPath,
            DatSubsetFilter? datSubset,
            string? source,
            bool hide = false)
        {
            Id = id;
            DatFullpath = datFullpath;
            ContentPath = contentPath;
            DatGroupEnum = datGroup;
            CollectionSetEnum = collectionSetEnum;
            IsRadioResource = CollectionSetEnum == COLLECTION_SET_ENUM.Resource;
            MameHashPath = mameHashPath;
            DatSubset = datSubset;
            Hide = hide;
            Source = source;
        }
    }
}
