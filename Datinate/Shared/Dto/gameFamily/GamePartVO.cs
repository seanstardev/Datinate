using RadioLibCore.RadioDat;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;
using static datinate.shared.DatFilterHelper;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class GamePartVO : IGamePart 
    {    
        public string Name { get; }
        public string DatSourceID { get; }
        public DatGameVO DatGameSource { get; }
        public DAT_GROUPER_MEMBERSHIP_ENUM MembershipStatusEnum { get; set; } = DAT_GROUPER_MEMBERSHIP_ENUM.NOT_SET;
        public EXPRESSION_ACTION_ENUM ExpressionActionEnum { get; private set; }
        public string RomsFingerprint { get; }
        public string Fingerprint => RomsFingerprint;
        public PartOwnerInfo PartOwnerInfo { get; }

        public DAT_GROUP_ENUM DatGroupEnum => PartOwnerInfo.DatGroupEnum;
        public string? DatReference => PartOwnerInfo.DatFriendlyReference;
        public string? PartOwnerName => PartOwnerInfo.PartOwnerName;
        public string? MameName => PartOwnerInfo.MameName;

        public bool IsShallowPartReference { get; private set; } = false;
        public string? MameSlPartName { get; }
        public bool Exclude {
            get => MembershipStatusEnum == DAT_GROUPER_MEMBERSHIP_ENUM.EXCLUDE_RELATIVE || MembershipStatusEnum == DAT_GROUPER_MEMBERSHIP_ENUM.EXCLUDE_EXPLICIT;
            set {
                // Do nothing.
            } 
        }
        public string? LaunchName {
            get 
            {
                if (PartOwnerInfo.IsMameOrMameSl)
                    return MameName;
                else
                    return null; 
            } 
            set { }
        }

        public string? Tag
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(MameSlPartName)) 
                    return MameSlPartName;
                return category;
            }
        }
        private List<GamePartVO> aliases = [];

        private readonly string? category;
        private readonly IReadOnlyCollection<string> romChecksums;

        public GamePartVO(
            string name
            , string datSourceID
            , DatGameVO datGameSource
            , EXPRESSION_ACTION_ENUM expressionActionEnum
            , string romsFingerprint
            , DAT_GROUP_ENUM datGroupEnum
            , string? datReference
            , string? partOwnerName
            , string? mameName
            , bool isShallowPartReference
            , string? mameSlPartName
            , string? category) 
        {
            Name = name;
            DatSourceID = datSourceID;
            DatGameSource = datGameSource;
            ExpressionActionEnum = expressionActionEnum;
            RomsFingerprint = romsFingerprint;
            PartOwnerInfo = new PartOwnerInfo(datGroupEnum, datReference, partOwnerName, mameName);
            IsShallowPartReference = isShallowPartReference;
            MameSlPartName = mameSlPartName;
            this.category = category;

            if (DatinateHelper.IsMameOrMameSoftlist(datGroupEnum))
            {
                LaunchName = mameName;
            }


            if (string.IsNullOrWhiteSpace(RomsFingerprint))
            {
                romChecksums = Array.Empty<string>();
            }
            else
            {
                romChecksums = RomsFingerprint
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();
            }
        }

        public bool IsPartOwnerMatch(GamePartVO gamePart, bool includeAliases)
        {
            if (PartOwnerInfo.IsMatch(gamePart.PartOwnerInfo))
                return true;

            if (!includeAliases)
                return false;

            foreach (var thisAlias in aliases)
                if (thisAlias.PartOwnerInfo.IsMatch(gamePart.PartOwnerInfo))
                    return true;

            foreach (var otherAlias in gamePart.aliases)
                if (PartOwnerInfo.IsMatch(otherAlias.PartOwnerInfo))
                    return true;

            foreach (var thisAlias in aliases)
                foreach (var otherAlias in gamePart.aliases)
                    if (thisAlias.PartOwnerInfo.IsMatch(otherAlias.PartOwnerInfo))
                        return true;

            return false;
        }

        public void MarkExpressionAsEscalated()
        {
            ExpressionActionEnum = EXPRESSION_ACTION_ENUM.EXCLUDE;
        }

        public string[] GetChecksums() 
        {
            return romChecksums.ToArray();
        }
        public bool AddAliases(IEnumerable<GamePartVO> aliases)
        {
            bool allAdded = true;

            foreach (var alias in aliases)
            {
                if (!AddAlias(alias))
                    allAdded = false;
            }
            return allAdded;
        }

        public bool AddAlias(GamePartVO part) 
        {
            foreach (var alias in aliases)
            {
                if (part.Name == alias.Name &&
                    part.DatReference == alias.DatReference &&
                    part.DatSourceID == alias.DatSourceID &&
                    part.DatGroupEnum == alias.DatGroupEnum) 
                    return false;
            }
            aliases.Add(part);
            
            return true;
        }

        public GamePartVO[] GetAliases()
        {
            return aliases.ToArray();
        }

        public string GetName() 
        {
            return Name;
        }

        public string GetFullpath() 
        {
            return string.Empty;
        }

        public string GetPath() 
        {
            return string.Empty;
        }
        public void SetPath(string path) 
        {
            
        }
   
        public IGamePart[] GetSoftwareAliases() 
        {
            List<IGamePart> list = new List<IGamePart>();

            foreach (var alias in aliases) 
            {
                list.Add(alias);
            }
            return list.ToArray();
        }
        
        public bool HasDirectoryId() 
        {
            return true;
        }

        public string GetDirectoryId() 
        {
            return DatSourceID;
        }
        public void SetDirectoryId(string directoryId) 
        {
            //
        }

        public string GetDisplayName() 
        {
            if (PartOwnerName != null && !PartOwnerInfo.IsMameOrMameSl)
                return PartOwnerName;
            else
                return Name;
        }

        public bool HasDisplayName() 
        {
            return false;
        }

        // INTERFACE
        public void SetSoftwareAliases(IGamePart[] gameParts)
        {
            //
        }
    }
}
