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
        public static string CreateTooltip(IGameEntity entity)
        {
            if (entity is IGameEntityProxy) 
                return string.Empty;
            
            if (entity is IGameFamily family)
            {
                return family.GetFamilyDisplayName();
            }

            if (entity is IGame game)
            {
                return GetGameNameRender(game.GetGameParts(false), game.GetNameWithoutExt());
                //return game.GetNameWithoutExt();
            }

            if (entity is IGamePart part)
            {
                var aliases = part.GetSoftwareAliases();

                var sb = new StringBuilder();

                AppendTooltipPartRow(sb, part);

                if (aliases.Any())
                    sb.AppendLine().AppendLine();

                for (int i = 0; i < aliases.Length; i++)
                {
                    AppendTooltipPartRow(sb, aliases[i]);

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
            IGamePart part)
        {
            string datGroup =
                DatinatePointerHelper.GetDatGroupAndFriendlyName(part.GetDirectoryId());

            sb.Append(datGroup);

            int tabCount = GetDatGroupTabCount(datGroup);

            for (int i = 0; i < tabCount; i++)
                sb.Append('\t');

            sb.Append(GetGamePartNameRender(part));
        }
    }
}
