using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class CurationPartReport
    {
        public bool IsShallowReference { get; }
        public DAT_GROUPER_MEMBERSHIP_ENUM MembershipStatusEnum { get; }
        public CurationPartReport(bool isShallowReference, DAT_GROUPER_MEMBERSHIP_ENUM membershipStatusEnum)
        {
            IsShallowReference = isShallowReference;
            MembershipStatusEnum = membershipStatusEnum;
        }
    }
}
