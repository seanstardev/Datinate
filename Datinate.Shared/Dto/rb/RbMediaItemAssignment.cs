using static Datinate.Shared.DatinateEnums;

namespace Datinate.Shared
{
    public class RbMediaItemAssignment
    {
        public RbMediaItemAssignment(string sourceId, MEDIA_ASSIGNMENT_ENUM assignmentEnum, string? entryName, string? lookupName, bool contentPathExists)
        {
            SourceId = sourceId;
            AssignmentEnum = assignmentEnum;
            EntryName = entryName;
            LookupName = lookupName;
            ContentPathExists = contentPathExists;
        }
        public bool ContentPathExists { get; set; } = false;
        public string SourceId { get; }
        public MEDIA_ASSIGNMENT_ENUM AssignmentEnum { get; }
        public string? EntryName { get; }
        public string? LookupName { get; }
    }
}
