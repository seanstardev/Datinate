using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class DatGrouperProjectEntry 
    {
        public static DatGrouperProjectEntry Empty => new DatGrouperProjectEntry(
            COLLECTION_SET_ENUM.NOT_SET, string.Empty, string.Empty, string.Empty, DAT_GROUP_ENUM.NOT_SET, string.Empty, null, null, null, null, false);

        public string DatFullpath { get; set; }
        public DAT_GROUP_ENUM DatGroupEnum { get; }
        public string ExpressionsXmlFullpath { get; }
        public string FriendlyName { get; private set; }
        public string ID { get; private set; }
        public string? Comment { get; private set; }
        public string? ContentPath { get; }
        public bool HideInUi { get; }

        public bool HasExpressionFilterDat => !string.IsNullOrWhiteSpace(ExpressionsXmlFullpath);
        public DatSubsetFilter? DatSubsetFilter { get; }
        public string? InternalDescriptor { get; }
        public COLLECTION_SET_ENUM CollectionSetEnum { get; }
        public string NameWithoutExt =>
            Path.GetFileNameWithoutExtension(DatFullpath);


        public DatGrouperProjectEntry(
            COLLECTION_SET_ENUM collectionSetEnum,
            string fullpath,
            string id,
            string expressionsXmlFullpath,
            DAT_GROUP_ENUM datGroupEnum,
            string friendlyName,
            DatSubsetFilter? datSubsetFilter,
            string? internalDescriptor,
            string? comment,
            string? contentPath,
            bool hideInUi) 
        {
            CollectionSetEnum = collectionSetEnum;
            DatFullpath = fullpath;
            ID = id;
            DatGroupEnum = datGroupEnum;
            ExpressionsXmlFullpath = expressionsXmlFullpath;
            FriendlyName = friendlyName;
            DatSubsetFilter = datSubsetFilter;
            InternalDescriptor = internalDescriptor;
            Comment = comment;
            ContentPath = contentPath;
            HideInUi = hideInUi;
        }
    }
}
