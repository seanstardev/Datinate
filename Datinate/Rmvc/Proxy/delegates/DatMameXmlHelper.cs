using com.RADIO.Datinate.RMVC.Shared;
using System.Xml;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal static class DatMameXmlHelper
    {
        public static DatVO? GetDat(string datFullpath, string xmlContent)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(xmlContent);
            }
            catch (Exception)
            {
                return null;
            }

            var root = doc.DocumentElement;

            if (root == null || root.Name != "mame")
                throw new InvalidOperationException("Invalid MAME XML structure: " + datFullpath);

            string version = root.GetAttribute("build");

            var datHeader = new DatHeaderVO(
                datTypeEnum: DAT_FORMAT_ENUM.MameListXml,
                name: "MAME Database",
                description: "MAME XML Data",
                category: string.Empty,
                version: version,
                author: string.Empty,
                comment: string.Empty);

            var entries = RetrieveGamesDetails(doc, out bool hasPClone, out bool oneRomPerGame);

            var dat = new DatVO(datFullpath, datHeader, entries, xmlContent);

            dat.AlwaysOneRomPerGame = oneRomPerGame;
            dat.HasPCloneStructure = hasPClone;
            dat.ContainsChds = entries.Any(entry => entry.Roms.Any(rom => rom.IsDisk));

            return dat;
        }
        private static DatGameVO[] RetrieveGamesDetails(XmlDocument doc, out bool hasPCloneStructure, out bool alwaysOneRomPerGame)
        {
            List<DatGameVO> list = new List<DatGameVO>();

            hasPCloneStructure = false;
            alwaysOneRomPerGame = true;

            XmlNodeList? machines = doc.SelectNodes("/mame/machine");

            if (machines == null)
                return list.ToArray();

            foreach (XmlNode machine in machines)
            {
                string name = GetAttribute(machine, "name");
                string? parent = GetNullableAttribute(machine, "cloneof");
                string description = machine.SelectSingleNode("description")?.InnerText ?? string.Empty;
                string publisher = machine.SelectSingleNode("manufacturer")?.InnerText ?? string.Empty;
                string year = machine.SelectSingleNode("year")?.InnerText ?? string.Empty;
                string mameName = name;

                List<DatRomVO> roms = new List<DatRomVO>();

                XmlNodeList? romNodes = machine.SelectNodes("rom");

                if (romNodes != null)
                {
                    foreach (XmlNode rom in romNodes)
                        roms.Add(CreateRom(rom));
                }

                XmlNodeList? diskNodes = machine.SelectNodes("disk");

                if (diskNodes != null)
                {
                    foreach (XmlNode disk in diskNodes)
                    {
                        var diskRom = CreateDisk(disk);

                        if (diskRom != null)
                            roms.Add(diskRom);
                    }
                }

                if (roms.Count > 1)
                    alwaysOneRomPerGame = false;

                DatGameVO game = new DatGameVO(
                    name,
                    description,
                    publisher,
                    null,
                    year,
                    null,
                    mameName,
                    parent,
                    roms.ToArray(),
                    null);

                list.Add(game);

                if (!string.IsNullOrWhiteSpace(parent))
                    hasPCloneStructure = true;
            }

            return list.ToArray();
        }
        private static DatRomVO CreateRom(XmlNode rom)
        {
            string romName = GetAttribute(rom, "name");
            ulong size = GetUlongAttribute(rom, "size");
            string crc = GetAttribute(rom, "crc");
            string md5 = GetAttribute(rom, "md5");
            string sha1 = GetAttribute(rom, "sha1");

            return new DatRomVO(romName, false, size, crc, md5, sha1);
        }

        private static DatRomVO? CreateDisk(XmlNode disk)
        {
            string diskName = GetAttribute(disk, "name");

            if (string.IsNullOrWhiteSpace(diskName))
                return null;

            string sha1 = GetAttribute(disk, "sha1");

            return new DatRomVO(diskName, true, 0, string.Empty, string.Empty, sha1);
        }

        private static string GetAttribute(XmlNode node, string name)
        {
            return node.Attributes?[name]?.Value ?? string.Empty;
        }

        private static string? GetNullableAttribute(XmlNode node, string name)
        {
            var value = GetAttribute(node, name);

            return string.IsNullOrWhiteSpace(value)
                ? null
                : value;
        }

        private static ulong GetUlongAttribute(XmlNode node, string name)
        {
            return ulong.TryParse(GetAttribute(node, name), out ulong value)
                ? value
                : 0;
        }
    }
}
