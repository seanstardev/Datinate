using RadioLibCore.RadioDat;

namespace Datinate.Shared.Util
{
    public static class DatinateFamilyHelper
    {
        public static bool GetAllGamesContainExactlyOnePart(IEnumerable<IGameFamily> families)
        {
            foreach (var family in families)
            {
                foreach (var game in family.GetAllGames())
                {
                    if (game.GetGameParts(false).Length != 1)
                        return false;
                }
            }
            return true;
        }
        public static bool GetAllPartsIgnored(IGameFamily family)
        {
            foreach (var game in family.GetAllGames())
            {
                if (!GetAllPartsIgnored(game))
                    return false;
            }
            return true;
        }
        public static bool GetAllPartsIgnored(IGame game)
        {
            foreach (var part in game.GetGameParts(false))
            {
                if (!part.Exclude)
                    return false;
            }
            return true;
        }
    }
}
