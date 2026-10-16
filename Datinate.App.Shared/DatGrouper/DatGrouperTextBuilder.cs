using Datinate.Shared;
using Datinate.Shared.Radio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datinate.App.Shared.DatGrouper
{
    public static class DatGrouperTextBuilder
    {
        public static string CreateTooltip(
            IGameEntity entity,
            Func<string, int> getSourceTabCount,
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
                return CreateGameTooltip(
                    game.GetGameParts(false),
                    game.GetNameWithoutExt());
            }

            if (entity is IGamePart part)
            {
                var aliases = part.GetSoftwareAliases();

                var sb = new StringBuilder();

                AppendTooltipPartRow(sb, part, getSourceTabCount);

                if (aliases.Any())
                {
                    sb.AppendLine().AppendLine();
                    sb.Append("\tAliases:");
                    sb.AppendLine();
                }
                for (int i = 0; i < aliases.Length; i++)
                {
                    AppendTooltipPartRow(sb, aliases[i], getSourceTabCount, true);

                    if (i != aliases.Length - 1)
                        sb.AppendLine();
                }

                return sb.ToString();
            }

            return string.Empty;
        }

        public static string CreateGameLabel(IReadOnlyList<IGamePart> partsArr, string? gameName)
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

        public static string CreatePartLabel(IGamePart part)
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

        private static string CreateGameTooltip(
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

            sb.AppendLine();

            sb.Append(family.GetFamilyDisplayName());
            sb.AppendLine().AppendLine();

            sb.Append("\tGames:\t\t\t ");
            sb.Append(games.Length);
            sb.AppendLine();

            sb.Append("\tSources:\t\t\t ");

            if (sources.Count == 0)
                sb.Append("none");

            else
                sb.Append(string.Join(" | ", sources));

            bool appendMediaInfo = true;

            if (mediaCollection == null ||
                (mediaCollection.CheckedDescriptorCodes.Any() == false &&
                string.IsNullOrWhiteSpace(mediaCollection.FamilyNotes) &&
                mediaCollection.SourceIdAssignmentDictionary.Count == 0)
            )
            {
                appendMediaInfo = false;
            }

            if (mediaCollection != null && appendMediaInfo == true)
            {
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

                if (!string.IsNullOrWhiteSpace(mediaCollection.FamilyNotes))
                {
                    sb.AppendLine().AppendLine();
                    sb.Append("\tComment:");
                    sb.AppendLine();
                    sb.Append("\t\t" + GetTooltipComment(mediaCollection.FamilyNotes));
                }
            }

            var allNames = DatinateFamilyHelper.GetAllNames(family);
            HashSet<string> distinctNames = new HashSet<string>();

            if (allNames.Count > 1)
                foreach (var str in allNames)
                    _ = distinctNames.Add(DatinateHelper.UnBracket(str));

            if (distinctNames.Count > 1) 
            {
                sb.AppendLine().AppendLine();
                sb.Append("\tName Variations:");

                foreach (var name in distinctNames)
                {
                    sb.AppendLine();
                    sb.Append("\t\t" + name);
                }
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

            // NOTE: netstandard2.0 has no range-based string slicing.
            return comment.Substring(0, maxLength).TrimEnd() + "...";
        }

        private static IReadOnlyCollection<string> GetFamilySources(
            IGameFamily family)
        {
            var sources = new List<string>();
            var seenSources =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                string source =
                    DatinatePointerHelper.GetDatGroupAndFriendlyName(
                        part.GetDirectoryId());

                if (string.IsNullOrWhiteSpace(source))
                    return;

                if (seenSources.Add(source))
                    sources.Add(source);
            }
        }

        private static void AppendTooltipPartRow(
            StringBuilder sb,
            IGamePart part,
            Func<string, int> getSourceTabCount,
            bool indent = false)
        {
            string datGroup =
                DatinatePointerHelper.GetDatGroupAndFriendlyName(part.GetDirectoryId());

            if (indent) sb.Append("\t");

            sb.Append(datGroup);

            int tabCount = getSourceTabCount(datGroup);

            for (int i = 0; i < tabCount; i++)
                sb.Append('\t');

            sb.Append(CreatePartLabel(part));
        }
    }
}
