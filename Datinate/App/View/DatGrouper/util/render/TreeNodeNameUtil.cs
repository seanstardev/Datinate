using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using System.Text;

namespace datinate.app
{
    public static class TreeNodeNameUtil
    {
        private static readonly Dictionary<string, int> datGroupTabCountCache =
            new(StringComparer.Ordinal);
        private static readonly object datGroupTabCountLock = new();
        private const int DatGroupTargetColumnPx = 120;

        public static string CreateTooltip(
            IGameEntity entity,
            IMediaCollection? mediaCollection = null)
        {
            if (entity is IGameEntityProxy)
                return string.Empty;

            if (entity is IGameFamily family)
            {
                return CreateFamilyTooltip(
                    family,
                    mediaCollection);
            }

            if (entity is IGame game)
            {
                return GetGameTooltipRender(
                    game.GetGameParts(false),
                    game.GetNameWithoutExt());
            }

            if (entity is IGamePart part)
            {
                var aliases = part.GetSoftwareAliases();

                var sb = new StringBuilder();

                AppendTooltipPartRow(sb, part);

                if (aliases.Any())
                {
                    sb.AppendLine().AppendLine();
                    sb.Append("\tAliases:");
                    sb.AppendLine();
                }
                for (int i = 0; i < aliases.Length; i++)
                {
                    AppendTooltipPartRow(sb, aliases[i], true);

                    if (i != aliases.Length - 1)
                        sb.AppendLine();
                }

                return sb.ToString();
            }

            return string.Empty;
        }
        public static string GetGameNameRender(IReadOnlyList<IGamePart> partsArr, string? gameName)
        {
            var sb = new StringBuilder();

            int roms = 0;
            int aliases = 0;

            for (int i = 0; i < partsArr.Count; i++)
            {
                var p = partsArr[i];
                roms += p.GetChecksums().Length;
                aliases += p.GetSoftwareAliases().Length;
            }

            int parts = partsArr.Count;

            sb.Append("ROMs: " + roms);
            if (parts > 1) sb.Append(", Parts: " + parts);
            if (aliases > 0) sb.Append(", Aliases: " + aliases);

            var info = sb.ToString();

            var result = !string.IsNullOrWhiteSpace(gameName)
                ? gameName + " │ " + info
                : info;

            return result;
        }
        public static string GetGamePartNameRender(IGamePart part)
        {
            string entryName = part.GetName();

            // TODO: Why are we reverse engineering the string id?
            var datGroupEnum = DatinatePointerHelper.GetDatGroup(part.GetDirectoryId() ?? string.Empty);

            if (DatinateHelper.IsMameOrMameSoftlist(datGroupEnum))
            {
                var append = string.Empty;
                bool pipe = false;

                if (!string.IsNullOrWhiteSpace(part.LaunchName))
                {
                    append += part.LaunchName;
                    pipe = true;
                }

                if (!string.IsNullOrWhiteSpace(part.Tag))
                {
                    append += pipe ? " | " + part.Tag : part.Tag;
                    pipe = true;
                }

                if (!string.IsNullOrWhiteSpace(append))
                    entryName += " <" + append + ">";
            }
            else if (!string.IsNullOrWhiteSpace(part.Tag))
                entryName += " <" + part.Tag + ">";

            return entryName;
        }
        private static string GetGameTooltipRender(
            IReadOnlyList<IGamePart> partsArr,
            string? gameName)
        {
            var sb = new StringBuilder();

            int roms = 0;
            int aliases = 0;

            for (int i = 0; i < partsArr.Count; i++)
            {
                var part = partsArr[i];

                roms += part.GetChecksums().Length;
                aliases += part.GetSoftwareAliases().Length;
            }

            int parts = partsArr.Count;

            sb.Append(gameName);
            sb.AppendLine().AppendLine();
            

            sb.Append("\tROMs:\t");
            sb.Append(roms);

            if (parts > 1)
            {
                sb.AppendLine();
                sb.Append("\tParts:\t");
                sb.Append(parts);
            }

            if (aliases > 0)
            {
                sb.AppendLine();
                sb.Append("\tAliases:\t");
                sb.Append(aliases);
            }

            return sb.ToString();
        }
        private static string CreateFamilyTooltip(
            IGameFamily family,
            IMediaCollection? mediaCollection)
        {
            var sb = new StringBuilder();

            var games = family.GetAllGames();
            var sources = GetFamilySources(family);

            sb.Append(family.GetFamilyDisplayName());
            sb.AppendLine().AppendLine();

            sb.Append("\tGames:\t\t\t ");
            sb.Append(games.Length);
            sb.AppendLine();

            sb.Append("\tSources:\t\t\t ");

            if (sources.Count == 0)
                sb.Append("none");
            else
                sb.Append(string.Join(", ", sources));

            if (mediaCollection == null ||
                (mediaCollection.CheckedDescriptorCodes.Any() == false &&
                string.IsNullOrWhiteSpace(mediaCollection.FamilyNotes) &&
                mediaCollection.SourceIdAssignmentDictionary.Count == 0)
            ) 
            {
                return sb.ToString();
            }

            sb.AppendLine().AppendLine();

            if (mediaCollection.CheckedDescriptorCodes.Count > 0)
            {
                sb.Append("\tDescriptors:\t\t ");
                sb.Append(
                    string.Join(
                        ", ",
                        mediaCollection.CheckedDescriptorCodes
                            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)));

                sb.AppendLine();
            }

            sb.Append("\tScoring Exempt:\t\t ");

            sb.Append(
                mediaCollection.IsScoringExempt
                    ? "yes"
                    : "no");

            sb.AppendLine();

            sb.Append("\tMedia & Resources:\t ");
            sb.Append(mediaCollection.SourceIdAssignmentDictionary.Count);
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(mediaCollection.FamilyNotes))
            {
                sb.Append("\tComment:\t\t ");
                sb.Append(GetTooltipComment(mediaCollection.FamilyNotes));
            }


            return sb.ToString();
        }
        private static string GetTooltipComment(string comment)
        {
            const int maxLength = 100;

            comment = comment
                .Replace("\r\n", " ")
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Trim();

            if (comment.Length <= maxLength)
                return comment;

            return comment[..maxLength].TrimEnd() + "...";
        }
        private static IReadOnlyCollection<string> GetFamilySources(
            IGameFamily family)
        {
            var sources =
                new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var game in family.GetAllGames())
            {
                foreach (var part in game.GetGameParts(false))
                {
                    AddSource(part);

                    foreach (var alias in part.GetSoftwareAliases())
                        AddSource(alias);
                }
            }

            return sources;

            void AddSource(IGamePart part)
            {
                string? directoryId = part.GetDirectoryId();

                if (string.IsNullOrWhiteSpace(directoryId))
                    return;

                var datGroup =
                    DatinatePointerHelper.GetDatGroup(directoryId);

                string name = datGroup
                    .ToString()
                    .Replace('_', ' ');

                if (!string.IsNullOrWhiteSpace(name))
                    sources.Add(name);
            }
        }

        private static int GetDatGroupTabCount(string datGroup)
        {
            lock (datGroupTabCountLock)
            {
                if (datGroupTabCountCache.TryGetValue(datGroup, out int cached))
                    return cached;

                var font = SystemFonts.StatusFont;

                const int maxTabs = 10;

                for (int tabCount = 1; tabCount <= maxTabs; tabCount++)
                {
                    string test =
                        datGroup + new string('\t', tabCount);

                    int width = TextRenderer.MeasureText(
                        test,
                        font,
                        Size.Empty,
                        TextFormatFlags.NoPadding |
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.ExpandTabs).Width;

                    if (width >= DatGroupTargetColumnPx)
                    {
                        datGroupTabCountCache[datGroup] = tabCount;
                        return tabCount;
                    }
                }

                datGroupTabCountCache[datGroup] = maxTabs;
                return maxTabs;
            }
        }

        private static void AppendTooltipPartRow(
            StringBuilder sb,
            IGamePart part,
            bool indent = false)
        {
            string datGroup =
                DatinatePointerHelper.GetDatGroupAndFriendlyName(part.GetDirectoryId());

            if (indent) sb.Append("\t");

            sb.Append(datGroup);

            int tabCount = GetDatGroupTabCount(datGroup);

            for (int i = 0; i < tabCount; i++)
                sb.Append('\t');

            sb.Append(GetGamePartNameRender(part));
        }
    }
}
