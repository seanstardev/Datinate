using Datinate.Shared;
using RadioLibCore.RadioDat;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class DatGrouperScoringItem
    {
        public DatGrouperScoringItem(
        MEDIA_TYPE_ENUM mediaTypeEnum,
        IReadOnlyList<string> sourceIds,
        MEDIA_TYPE_ENUM icon1Type,
        MEDIA_TYPE_ENUM? icon2Type)
        {
            MediaTypeEnum = mediaTypeEnum;
            SourceIds = sourceIds;
            Icon1Type = icon1Type;
            Icon2Type = icon2Type;
        }

        public bool HasScore => SourceIds.Count > 0;
        public MEDIA_TYPE_ENUM MediaTypeEnum { get; }
        public IReadOnlyList<string> SourceIds { get; }
        public MEDIA_TYPE_ENUM Icon1Type { get; }
        public MEDIA_TYPE_ENUM? Icon2Type { get; }
    }

    public class DatGrouperScoring
    {
        public DatGrouperScoring(
            bool isScoringExempt,
            IReadOnlyDictionary<MEDIA_TYPE_ENUM, DatGrouperScoringItem> resourceDictionary,
            IReadOnlyDictionary<MEDIA_TYPE_ENUM, DatGrouperScoringItem> mediaDictionary)
        {
            IsScoringExempt = isScoringExempt;
            ResourceDictionary = resourceDictionary;
            MediaDictionary = mediaDictionary;
        }

        public bool IsScoringExempt { get; }
        public IReadOnlyDictionary<MEDIA_TYPE_ENUM, DatGrouperScoringItem> ResourceDictionary { get; }
        public IReadOnlyDictionary<MEDIA_TYPE_ENUM, DatGrouperScoringItem> MediaDictionary { get; }
    }
    public interface IMediaCollection
    {
        bool IsEmptyForExport { get; }
        bool IsEmpty { get; }
        IReadOnlyDictionary<string, RbMediaItemAssignment> SourceIdAssignmentDictionary { get; }
        IReadOnlySet20<string> CheckedDescriptorCodes { get; }
        string FamilyNotes { get; }
        IGameFamily Family { get; }
        bool IsScoringExempt { get; }
    }
    public interface IAssignedMediaItem
    {
        string SourceId { get; }
        string EntryName { get; }
        string LookupName { get; }
    }
    public interface IMediaCollectionImportExport
    {
        Dictionary<string, IAssignedMediaItem> SourceIdAssignedItemDictionary { get; }
        bool IsEmptyForExport { get; }
        IReadOnlySet20<string> CheckedDescriptorCodes { get; }
        string FamilyNotes { get; }
    }


    public class RbMediaCollection : IMediaCollection, IMediaCollectionImportExport
    {
        private readonly IReadOnlySet20<string> nonScoringDescriptors;

        private HashSet<string> _checkedDescriptorCodes = new HashSet<string>();
        private IReadOnlySet20<string> _checkedDescriptorCodesView;

        public RbMediaCollection(
            IGameFamily family,
            IReadOnlySet20<string> nonScoringDescriptors)
        {
            Family = family;
            this.nonScoringDescriptors = nonScoringDescriptors;

            _checkedDescriptorCodesView =
                new ReadOnlySet20<string>(_checkedDescriptorCodes);
        }

        public bool IsEmpty => IsEmptyCore(includeNotFoundAsEmpty: false);

        public bool IsEmptyForExport => IsEmptyCore(includeNotFoundAsEmpty: true);

        private bool IsEmptyCore(bool includeNotFoundAsEmpty)
        {
            if (!string.IsNullOrWhiteSpace(FamilyNotes))
                return false;

            if (CheckedDescriptorCodes.Count > 0)
                return false;

            return SourceIdAssignmentDictionary.Count == 0
                || SourceIdAssignmentDictionary.Values.All(a =>
                    a.AssignmentEnum == MEDIA_ASSIGNMENT_ENUM.None
                    || (includeNotFoundAsEmpty
                        && a.AssignmentEnum == MEDIA_ASSIGNMENT_ENUM.NotFound));
        }

        public bool IsScoringExempt
        {
            get
            {
                if (nonScoringDescriptors.Count == 0
                    || CheckedDescriptorCodes.Count == 0)
                {
                    return false;
                }

                foreach (var code in CheckedDescriptorCodes)
                {
                    if (nonScoringDescriptors.Contains(code))
                        return true;
                }

                return false;
            }
        }

        public Dictionary<string, RbMediaItemAssignment>
            SourceIdAssignmentDictionary
        { get; } =
                new Dictionary<string, RbMediaItemAssignment>();

        public HashSet<string> CheckedDescriptorCodes
        {
            get => _checkedDescriptorCodes;
            set
            {
                _checkedDescriptorCodes = value;
                _checkedDescriptorCodesView = new ReadOnlySet20<string>(value);
            }
        }

        IReadOnlySet20<string> IMediaCollection.CheckedDescriptorCodes =>
            _checkedDescriptorCodesView;

        IReadOnlySet20<string> IMediaCollectionImportExport.CheckedDescriptorCodes =>
            _checkedDescriptorCodesView;

        IReadOnlyDictionary<string, RbMediaItemAssignment>
            IMediaCollection.SourceIdAssignmentDictionary =>
                SourceIdAssignmentDictionary;

        public string FamilyNotes { get; set; } = "";

        public IGameFamily Family { get; set; }

        public Dictionary<string, IAssignedMediaItem> SourceIdAssignedItemDictionary
        {
            get
            {
                var dic = new Dictionary<string, IAssignedMediaItem>();

                foreach (var kvp in SourceIdAssignmentDictionary)
                {
                    var item = kvp.Value;

                    if (item.AssignmentEnum == MEDIA_ASSIGNMENT_ENUM.Assigned
                        && !string.IsNullOrWhiteSpace(item.EntryName)
                        && !string.IsNullOrWhiteSpace(item.LookupName))
                    {
                        dic.Add(
                            kvp.Key,
                            new RbAssignedMediaItem(
                                kvp.Key, item.EntryName!, item.LookupName!));
                    }
                }

                return dic;
            }
        }
    }

    public class RbAssignedMediaItem : IAssignedMediaItem
        {
            public RbAssignedMediaItem(string sourceId, string entryName, string lookupName)
            {
                SourceId = sourceId;
                EntryName = entryName;
                LookupName = lookupName;
            }

            public string SourceId { get; }
            public string EntryName { get; }
            public string LookupName { get; }
        }

        //public class RbMediaItemAssignment
        //{
        //    public RbMediaItemAssignment(string sourceId, MEDIA_ASSIGNMENT_ENUM assignmentEnum, string? entryName, string? lookupName, bool contentPathExists)
        //    {
        //        SourceId = sourceId;
        //        AssignmentEnum = assignmentEnum;
        //        EntryName = entryName;
        //        LookupName = lookupName;
        //        ContentPathExists = contentPathExists;
        //    }
        //    public bool ContentPathExists { get; set; } = false;
        //    public string SourceId { get; }
        //    public MEDIA_ASSIGNMENT_ENUM AssignmentEnum { get; }
        //    public string? EntryName { get; }
        //    public string? LookupName { get; }
        //}

        //public class RbMediaItemAssignmentUpdate
        //{
        //    public RbMediaItemAssignmentUpdate(
        //        IGameFamily family,
        //        string sourceId,
        //        MEDIA_ASSIGNMENT_ENUM assignmentEnum,
        //        string? entryName = null,
        //        string? lookupName = null)
        //    {
        //        Family = family;
        //        SourceId = sourceId;
        //        AssignmentEnum = assignmentEnum;
        //        EntryName = entryName;
        //        LookupName = lookupName;
        //    }

        //    public IGameFamily Family { get; }
        //    public string SourceId { get; }
        //    public MEDIA_ASSIGNMENT_ENUM AssignmentEnum { get; }
        //    public string? EntryName { get; }
        //    public string? LookupName { get; }
        //}
    }
