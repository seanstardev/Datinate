using RadioLibCore.RadioDat;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class RbMediaItemAssignmentUpdate
    {
        public RbMediaItemAssignmentUpdate(
            IGameFamily family,
            string sourceId,
            MEDIA_ASSIGNMENT_ENUM assignmentEnum,
            string? entryName = null,
            string? lookupName = null)
        {
            Family = family;
            SourceId = sourceId;
            AssignmentEnum = assignmentEnum;
            EntryName = entryName;
            LookupName = lookupName;
        }

        public IGameFamily Family { get; }
        public string SourceId { get; }
        public MEDIA_ASSIGNMENT_ENUM AssignmentEnum { get; }
        public string? EntryName { get; }
        public string? LookupName { get; }
    }
}
