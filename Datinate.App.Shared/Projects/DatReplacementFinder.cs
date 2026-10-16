using Datinate.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Datinate.App.Shared.Projects
{
    public static class DatReplacementFinder
    {
        public static string? FindReplacement(string originalFullpath)
        {
            string? directory = Path.GetDirectoryName(originalFullpath);

            if (IsNullOrWhiteSpace20(directory) ||
                !Directory.Exists(directory))
            {
                return null;
            }

            string targetFilename = Path.GetFileName(originalFullpath);
            string[] fullpaths = Directory.GetFiles(directory);

            var candidates =
                new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (string fullpath in fullpaths)
            {
                if (fullpath.EndsWith(
                    ".json",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                candidates.Add(Path.GetFileName(fullpath), fullpath);
            }

            string? bestMatch = LevenshteinDistanceUtil.GetBestMatch(
                targetFilename,
                candidates.Keys.ToArray());

            // TODO: document this RomVault phenomenon
            if (IsNullOrWhiteSpace20(bestMatch) ||
                bestMatch.EndsWith(
                    ".dat.old",
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return candidates[bestMatch];
        }
    }
}
