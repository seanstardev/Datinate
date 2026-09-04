using com.RADIO.Datinate.RMVC.Shared;
using System.Xml;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Rmvc.Proxy.delegates
{
    public static class NoIntroHelper
    {
        private enum PCloneFlavor
        {
            None,
            IdCloneOfId,
            CloneOf
        }

        public static DatVO? GetDat(string datFullpath, string datRawText)
        {
            var doc = new XmlDocument();

            try
            {
                if (!string.IsNullOrWhiteSpace(datRawText))
                    doc.LoadXml(datRawText);
                else
                    doc.Load(datFullpath);
            }
            catch
            {
                return null;
            }

            var root = doc.DocumentElement;
            if (root == null || !root.Name.Equals("datafile", StringComparison.Ordinal))
                return null;

            var headerEl = root.SelectSingleNode("header") as XmlElement;
            if (headerEl == null)
                return null;

            var datName = (headerEl.SelectSingleNode("name") as XmlElement)?.InnerText ?? string.Empty;
            var datDescription = (headerEl.SelectSingleNode("description") as XmlElement)?.InnerText ?? string.Empty;

            var gameNodes = new List<(XmlElement GameEl, string DirPath)>();
            CollectGameNodes(root, string.Empty, gameNodes);

            if (gameNodes.Count == 0)
                return null;

            var flavor = DetectFlavor(gameNodes);

            var games = new List<DatGameVO>(gameNodes.Count);
            var hasPCloneStructure = false;

            foreach (var gameNode in gameNodes)
            {
                var gameEl = gameNode.GameEl;

                var romNodes = gameEl.SelectNodes("rom|disk");
                if (romNodes == null || romNodes.Count == 0)
                    continue;

                var roms = new List<DatRomVO>(romNodes.Count);

                foreach (XmlElement romEl in romNodes)
                {
                    if (!romEl.HasAttribute("name"))
                        continue;

                    roms.Add(new DatRomVO(
                        romEl.GetAttribute("name"),
                        romEl.Name == "disk",
                        GetSize(romEl.GetAttribute("size")),
                        romEl.GetAttribute("crc"),
                        romEl.GetAttribute("md5"),
                        romEl.GetAttribute("sha1")));
                }

                if (roms.Count == 0)
                    continue;

                var displayName =
                    (gameEl.SelectSingleNode("description") as XmlElement)?.InnerText
                    ?? gameEl.GetAttribute("name")
                    ?? string.Empty;

                var finalName = BuildGameName(gameNode.DirPath, displayName);

                var category = (gameEl.SelectSingleNode("category") as XmlElement)?.InnerText ?? null;

                string mameName;
                string? parentName;

                if (flavor == PCloneFlavor.IdCloneOfId)
                {
                    mameName = gameEl.GetAttribute("id");
                    var cloneOfId = gameEl.GetAttribute("cloneofid");
                    parentName = string.IsNullOrWhiteSpace(cloneOfId) ? null : cloneOfId;
                }
                else if (flavor == PCloneFlavor.CloneOf)
                {
                    mameName = gameEl.GetAttribute("name");
                    if (string.IsNullOrWhiteSpace(mameName))
                        mameName = displayName;

                    if (string.IsNullOrWhiteSpace(mameName))
                        mameName = roms[0].Name;

                    var cloneOf = gameEl.GetAttribute("cloneof");
                    parentName = string.IsNullOrWhiteSpace(cloneOf) ? null : cloneOf;
                }
                else
                {
                    mameName = string.Empty;
                    parentName = null;
                }

                if (parentName != null)
                    hasPCloneStructure = true;

                games.Add(new DatGameVO(
                    finalName,
                    null,
                    null,
                    null,
                    null,
                    category,
                    mameName,
                    parentName,
                    roms.ToArray(),
                    string.Empty));
            }

            int gamesTotal = games.Count;
            int romsTotal = games.Sum(g => g.Roms.Length);
            ulong sizeTotal = games.SelectMany(g => g.Roms).Aggregate(0UL, (sum, rom) => sum + rom.Size);

            var header = new DatHeaderVO(
                DAT_FORMAT_ENUM.LogiqxXml,
                datName,
                datDescription,
                string.Empty, string.Empty, string.Empty, string.Empty);

            return new DatVO(datFullpath, header, games, datRawText)
            {
                AlwaysOneRomPerGame = gamesTotal == romsTotal && romsTotal != 0,
                HasPCloneStructure = hasPCloneStructure,
                ContainsChds = false
            };
        }

        private static void CollectGameNodes(
            XmlElement parentEl,
            string currentDirPath,
            List<(XmlElement GameEl, string DirPath)> gameNodes)
        {
            var hasDirectGameNodes = parentEl.SelectSingleNode("game") != null;

            foreach (XmlNode childNode in parentEl.ChildNodes)
            {
                if (childNode is not XmlElement childEl)
                    continue;

                if (childEl.Name.Equals("game", StringComparison.Ordinal))
                {
                    gameNodes.Add((childEl, currentDirPath));
                    continue;
                }

                if (!hasDirectGameNodes && childEl.Name.Equals("machine", StringComparison.Ordinal))
                {
                    gameNodes.Add((childEl, currentDirPath));
                    continue;
                }

                if (childEl.Name.Equals("dir", StringComparison.Ordinal))
                {
                    var dirName = childEl.GetAttribute("name");
                    var nextDirPath = BuildDirPath(currentDirPath, dirName);

                    CollectGameNodes(childEl, nextDirPath, gameNodes);
                }
            }
        }

        private static string BuildDirPath(string currentDirPath, string dirName)
        {
            if (string.IsNullOrWhiteSpace(dirName))
                return currentDirPath;

            if (string.IsNullOrWhiteSpace(currentDirPath))
                return dirName;

            return currentDirPath + "/" + dirName;
        }

        private static string BuildGameName(string dirPath, string gameName)
        {
            if (string.IsNullOrWhiteSpace(dirPath))
                return gameName;

            if (string.IsNullOrWhiteSpace(gameName))
                return dirPath;

            return dirPath + "/" + gameName;
        }

        private static PCloneFlavor DetectFlavor(List<(XmlElement GameEl, string DirPath)> gameNodes)
        {
            foreach (var gameNode in gameNodes)
            {
                var gameEl = gameNode.GameEl;

                if (gameEl.HasAttribute("cloneofid") || gameEl.HasAttribute("id"))
                    return PCloneFlavor.IdCloneOfId;

                if (gameEl.HasAttribute("cloneof"))
                    return PCloneFlavor.CloneOf;
            }

            return PCloneFlavor.None;
        }

        private static ulong GetSize(string sizeAttValue)
        {
            if (string.IsNullOrWhiteSpace(sizeAttValue))
                return 0;

            return ulong.TryParse(sizeAttValue, out var v) ? v : 0;
        }
    }
}