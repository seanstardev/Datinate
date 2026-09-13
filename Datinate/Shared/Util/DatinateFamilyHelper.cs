using com.RADIO.Datinate.RMVC.Shared;
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
        public static string BuildSummaryText(IGameFamily[] gameFamilies)
        {
            long families = gameFamilies.Length;
            long games = 0;
            long parts = 0;
            long roms = 0;

            for (int i = 0; i < gameFamilies.Length; i++)
            {
                var f = gameFamilies[i];

                var g = f.GetAllGames();
                games += g.Length;

                var ps = f.GetAllGameParts(false);
                parts += ps.Length;

                for (int p = 0; p < ps.Length; p++)
                    roms += ps[p].GetChecksums().Length;
            }

            const string s = "    ";
            return $"■ Families: {families.ToString("N0") + s} ■ Entries: {games.ToString("N0") + s} ■ Parts: {parts.ToString("N0") + s} ■ ROMs: {roms.ToString("N0")}";
        }

        public static long GetTotalParts(IEnumerable<IGameFamily> gameFamilies)
        {
            long parts = 0L;
            foreach (var family in gameFamilies)
            {
                parts += family.GetAllGameParts(false).Length;
            }
            return parts;
        }

        public static string? GetGameEntityName(IGameEntity? entity)
        {
            if (entity is IGameFamily family)
                return family.GetFamilyDisplayName();

            if (entity is IGame game)
                return game.GetNameWithoutExt();

            if (entity is IGamePart part)
                return part.GetName();

            return null;
        }

        public static IReadOnlyList<string> CreateSearchPrompts(
            IGamePart part,
            bool limitToTen = true)
        {
            return CreateSearchPrompts(
                new[] { part },
                limitToTen);
        }

        public static IReadOnlyList<string> CreateSearchPrompts(
            IGame game,
            bool limitToTen = true)
        {
            return CreateSearchPrompts(
                game.GetGameParts(true),
                limitToTen);
        }

        public static IReadOnlyList<string> CreateSearchPrompts(
            IGameFamily family,
            bool limitToTen = true)
        {
            return CreateSearchPrompts(
                family.GetAllGameParts(true),
                limitToTen);
        }
        public static IReadOnlyList<string> CreateSearchPrompts(IGameEntity entity)
        {
            if (entity is IGameFamily family) return CreateSearchPrompts(family);
            if (entity is IGame game) return CreateSearchPrompts(game);
            if (entity is IGamePart part) return CreateSearchPrompts(part);
            else return new List<string>();
        }
        private static IReadOnlyList<string> CreateSearchPrompts(
            IEnumerable<IGamePart> parts,
            bool limitToTen)
        {
            List<string> prompts = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var part in parts)
            {
                var unbracketed =
                    DatinateHelper.GetFlaglessName(part.GetName());

                string[] nameParts = unbracketed.Split(
                    new[] { ' ', '-' },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var namePart in nameParts)
                {
                    var section = namePart.Trim();

                    if (section.Length == 0 || !seen.Add(section))
                        continue;

                    prompts.Add(section);

                    if (limitToTen && prompts.Count >= 10)
                        return prompts;
                }
            }

            return prompts;
        }
    }
}
