using RadioLibCore.RadioDat;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;
using static datinate.shared.DatFilterHelper;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class GameFamilyVO : IGameFamily 
    {

        public string DisplayName { get; private set; }
        public GameVO[] Games { get; private set; } = Array.Empty<GameVO>();

        // TODO - see auto grouper tech:
        public List<IGamePart> OrphanedGameRoms { get; private set; } = new List<IGamePart>();

        public GameFamilyVO(string displayName) 
        {
            DisplayName = displayName;
        }
        public int GetTotalRomsCount()
        {
            return Games.Sum(gs => gs.Parts.Sum(ps => ps.GetChecksums().Length));
        }
        public int GetTotalIncludedParts()
        {
            return Games.Sum(gs => gs.Parts.Length);
        }

        public void ReplaceGames(IEnumerable<GameVO> list)
        {
            Games = list.ToArray();
        }

        public void AddGame(GameVO vo) {

            List<GameVO> list = Games.ToList();
            list.Add(vo);
            Games = list.ToArray();
        }

        public void FormaliseMembershipStatuses()
        {
            var ps = GetAllGameParts(false);
            var list = new List<GamePartVO>();

            foreach (var part in ps)
                list.Add((GamePartVO)part);

            bool hasAtLeastOneGoodPart = false;
            foreach (var part in list)
            {
                if (part.ExpressionActionEnum == EXPRESSION_ACTION_ENUM.INCLUDE_IMPLICIT ||
                    part.ExpressionActionEnum == EXPRESSION_ACTION_ENUM.INVALID ||
                    part.ExpressionActionEnum == EXPRESSION_ACTION_ENUM.INCLUDE ||
                    part.ExpressionActionEnum == EXPRESSION_ACTION_ENUM.NO_FILTER)
                {
                    hasAtLeastOneGoodPart = true;
                    break;
                }
            }


            foreach (var part in list)
            {
                // Assuming this is most likely membership scenario:
                part.MembershipStatusEnum = DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_IMPLICIT;

                if (part.ExpressionActionEnum == EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL && hasAtLeastOneGoodPart)
                {
                    part.MarkExpressionAsEscalated();
                    part.MembershipStatusEnum = DAT_GROUPER_MEMBERSHIP_ENUM.EXCLUDE_RELATIVE;
                }
                else if (part.ExpressionActionEnum == EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL)
                {
                    part.MembershipStatusEnum = DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_RELATIVE;
                }
                else if (part.ExpressionActionEnum == EXPRESSION_ACTION_ENUM.INCLUDE)
                {
                    part.MembershipStatusEnum = DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_EXPLICIT;
                }
                else if (part.ExpressionActionEnum == EXPRESSION_ACTION_ENUM.EXCLUDE)
                {
                    part.MembershipStatusEnum = DAT_GROUPER_MEMBERSHIP_ENUM.EXCLUDE_EXPLICIT;
                }
            }
        }

        // INTERFACE
        // TODO: Can DEFINITELY add descriptors like 'No Good Dump', 'Incomplete', 'Educational' going forwards.
        public IDescriptor[] GetDescriptors()
        {
            return [];
        }
        // INTERFACE
        public string GetFamilyDisplayName() 
        {
            return DisplayName;
        }
        // INTERFACE
        public void SetFamilyDisplayName(string displayName) 
        {
            DisplayName = displayName;
        }
        // INTERFACE
        public IGame GetParentGame() 
        {
            return Games[0];
        }
        // INTERFACE
        public IGame[] GetChildGamesOnly()
        {
            return Games.Skip(1).Cast<IGame>().ToArray();
        }
        // INTERFACE
        public IGame[] GetAllGames() {
            return Games;
        }
        // INTERFACE
        public IResourceCollection GetResourceCollection() {
            return BaseResourceCollection.Empty;
        }
        // INTERFACE
        public string GetComment() {
            return string.Empty;
        }
        // INTERFACE
        public bool GetIgnored() {
            return false;
        }
        // INTERFACE
        public IGamePart[] GetAllGameParts(bool includeAliasPartsInFlatList)
        {
            return Games
                .SelectMany(gs => gs.GetGameParts(includeAliasPartsInFlatList))
                .ToArray();
        }
    }
}
