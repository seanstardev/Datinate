using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class PartOwnerInfo
    {
        public DAT_GROUP_ENUM DatGroupEnum { get; }
        public string? DatFriendlyReference { get; }
        public string? PartOwnerName { get; }
        public string? MameName { get; }
        public bool IsMameOrMameSl { get; }

        private readonly string? owner;
        private readonly bool canMatch;

        public PartOwnerInfo(
            DAT_GROUP_ENUM datGroupEnum, 
            string? datFriendlyReference, 
            string? partOwnerName,
            string? mameName)
        {
            DatGroupEnum = datGroupEnum;
            DatFriendlyReference = datFriendlyReference;
            PartOwnerName = partOwnerName;
            MameName = mameName;

            IsMameOrMameSl = DatinateHelper.IsMameOrMameSoftlist(DatGroupEnum);
            
            owner = IsMameOrMameSl ? MameName : PartOwnerName;
            canMatch = !string.IsNullOrWhiteSpace(owner);
        }

        public bool IsMatch(PartOwnerInfo toMatch)
        {
            if (!canMatch || object.ReferenceEquals(this, toMatch)) // ReferenceEquals should never happen
                return false;

            if (DatGroupEnum != toMatch.DatGroupEnum || DatFriendlyReference != toMatch.DatFriendlyReference)
                return false;

            if (string.IsNullOrWhiteSpace(owner))
                return false;

            return owner == toMatch.owner;
        }
    }
}
