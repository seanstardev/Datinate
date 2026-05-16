using com.RADIO.Datinate.RMVC.Shared;
using System.Xml;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Rmvc.Proxy.delegates
{
    public static class DatSoftHelper
    {
        public static DatVO? GetDat(string datFullpath, string datRawText)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(datFullpath);
            }
            catch (Exception e)
            {
                return null;
            }

            var rootNode = doc.DocumentElement;
            if (rootNode == null ||
                rootNode.Name != "softwarelist" ||
                !rootNode.HasAttribute("name"))
            {
                return null;
            }

            var softwareNodes = rootNode.SelectNodes("software");
            if (softwareNodes == null) return null;

            bool hasPCloneStructure = false;
            bool hasCHDs = false;

            int skippedParts = 0;
            int skippedRoms = 0;
            int skippedChds = 0;

            var games = new List<DatGameVO>();

            foreach (XmlElement gameEl in softwareNodes)
            {
                var mameName = gameEl.GetAttribute("name");
                var parent = gameEl.GetAttribute("cloneof");
                var description = (gameEl.SelectSingleNode("description") as XmlElement)?.InnerText ?? string.Empty;
                var publisher = (gameEl.SelectSingleNode("publisher") as XmlElement)?.InnerText ?? string.Empty;
                var year = (gameEl.SelectSingleNode("year") as XmlElement)?.InnerText ?? string.Empty;

                var partNodes = gameEl.SelectNodes("part");
                if (partNodes == null) continue;

                foreach (XmlElement partEl in partNodes)
                {
                    var roms = new List<DatRomVO>();

                    var partName = partEl.GetAttribute("name");

                    var areaNodes = partEl.SelectNodes("./dataarea | ./diskarea");
                    if (areaNodes == null)
                        continue;

                    foreach (XmlElement areaEl in areaNodes)
                    {
                        // Roms
                        if (areaEl.Name == "dataarea")
                        {
                            var romNodes = areaEl.SelectNodes("rom");
                            if (romNodes == null) continue;

                            foreach (XmlElement romEl in romNodes)
                            {
                                if (!romEl.HasAttribute("name") ||
                                    romEl.HasAttribute("loadflag") ||
                                    romEl.GetAttribute("status").Equals("nodump", StringComparison.OrdinalIgnoreCase))
                                {
                                    skippedRoms++;
                                    continue;
                                }
                                else
                                {
                                    roms.Add(new DatRomVO(
                                        romEl.GetAttribute("name"),
                                        false,
                                        GetSize(romEl.GetAttribute("size")),
                                        romEl.GetAttribute("crc"),
                                        string.Empty,
                                        romEl.GetAttribute("sha1")));
                                }
                            }

                        }
                        // Chds
                        else if (areaEl.Name == "diskarea")
                        {
                            var romNodes = areaEl.SelectNodes("disk");
                            if (romNodes == null) continue;

                            foreach (XmlElement diskEl in romNodes)
                            {
                                if (diskEl.GetAttribute("status").Equals("nodump", StringComparison.OrdinalIgnoreCase))
                                {
                                    skippedChds++;
                                    continue;
                                }

                                roms.Add(new DatRomVO(
                                    diskEl.GetAttribute("name"),
                                    true,
                                    0, string.Empty, string.Empty, diskEl.GetAttribute("sha1")));

                                hasCHDs = true;
                            }
                        }
                    } // end rom & chd list build.

                    if (roms.Count == 0)
                    {
                        skippedParts++;
                        continue;
                    }
                    else 
                    {
                        games.Add(new DatGameVO(
                            description, null, publisher.Trim(), null, year, null, mameName, parent, roms.ToArray(),
                            partName));

                        if (!string.IsNullOrEmpty(parent))
                            hasPCloneStructure = true;
                    }
                } // end game / part list build.
            }

            var header = new DatHeaderVO(
                DAT_FORMAT_ENUM.MameSoftwareListXml,
                rootNode.GetAttribute("name"),
                rootNode.GetAttribute("description"),
                string.Empty, string.Empty, string.Empty, string.Empty);

            int gamesTotal = games.Count;
            int romsTotal = games.Sum(g => g.Roms.Length);
            ulong sizeTotal = games.SelectMany(g => g.Roms).Aggregate(0UL, (sum, rom) => sum + rom.Size);
            

            return new DatVO(
                datFullpath, 
                header,
                games.OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase).ToList(),
                datRawText)
            {
                AlwaysOneRomPerGame = gamesTotal == romsTotal && romsTotal != 0,
                HasPCloneStructure = hasPCloneStructure,
                ContainsChds = hasCHDs
            };

            // For prosperity: skippedParts, skippedRoms, skippedChds.
        }

        private static ulong GetSize(string sizeAttValue)
        {
            if (string.IsNullOrWhiteSpace(sizeAttValue))
                return 0;

            try
            {
                // Check if the value is hexadecimal (starts with 0x or contains hex-only characters)
                if (sizeAttValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    // Remove the 0x prefix and parse as hex
                    return Convert.ToUInt64(sizeAttValue.Substring(2), 16);
                }

                // If the string contains any valid hexadecimal letters, parse as hex
                if (sizeAttValue.IndexOfAny("abcdefABCDEF".ToCharArray()) >= 0)
                {
                    // If the string contains only valid hexadecimal characters, parse as hex
                    return Convert.ToUInt64(sizeAttValue, 16);
                }

                // Otherwise, treat as a decimal value
                return Convert.ToUInt64(sizeAttValue);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}

/**

vb: 40,894,464
0x100000 in hex = 1,048,576 bytes (1 MiB).

nes: 2,249,853,236
     2,249,853,236
*/

/**
 * 
 * 
 * nes loadflag
 * 
 * 	<software name="brdtalej" cloneof="brdtale">
<description>The Bard's Tale - Tales of the Unknown (Japan)</description>
<year>1990</year>
<publisher>Pony Canyon</publisher>
<info name="serial" value="PNF-ET (R68V5935)"/>
<info name="release" value="19901221"/>
<info name="alt_title" value="バーズテイル"/>
<part name="cart" interface="nes_cart">
    <feature name="slot" value="sxrom" />
    <feature name="pcb" value="HVC-SNROM" />
    <feature name="mmc1_type" value="MMC1B2" />
    <dataarea name="prg" size="262144">
        <rom name="pnf-et-0 prg" size="262144" crc="7ee02ca2" sha1="e22ae723541184adf7a81d824ad003cfd59ab485" offset="00000" />
    </dataarea>
    <!-- 8k VRAM on cartridge -->
    <dataarea name="vram" size="8192" />
    <!-- 8k WRAM on cartridge, battery backed up -->
    <dataarea name="bwram" size="8192">
        <rom value="0x00" size="8192" offset="0" loadflag="fill" />
    </dataarea>
</part>
</software>
 */

/**
 * diskarea
 * 
 * 	<software name="draculax" supported="partial">
<description>Akumajou Dracula X - Chi no Rondo (Japan)</description>
<year>1993</year>
<publisher>Konami</publisher>
<notes><![CDATA[
[redbook] skips first two seconds, offsets on repeats, Stage 5 outro Dracula lip sync is off
[ADPCM] streaming cuts off when saving Maria
Richter sprite becomes all blue in some specific sections (cfr. stage 1 mermaid area, stage 2' final section, stage 4' start), generally when water is present, timing?
Stage 4 to 4' boss fight has ugly green background (btanb)
]]></notes>
<info name="serial" value="KMCD3005"/>
<info name="release" value="19931029"/>
<info name="alt_title" value="悪魔城ドラキュラX 血の輪廻"/>
<info name="usage" value="CD-ROM² Super System Card required" />
<sharedfeat name="requirement" value="scdsys"/>
<part name="cdrom" interface="cdrom">
    <diskarea name="cdrom">
        <disk name="akumajou dracula x - chi no rondo (scd)(jpn)" sha1="bff01c949ae66b51797ec891befd91009591fbc1"/>
    </diskarea>
</part>
</software>
 */


/**
 
psx nodump example

    <!-- boot OK, 2nd disc is missing from the set -->
<software name="metamorv" supported="no">
    <!--
    Unknown source
    <rom name="Himitsu Sentai Metamor V Deluxe (Japan) (Disc 1) [SLPS-01626].bin" size="539010192" crc="b1f5731d" sha1="4ce244fa224d9664f244a387517494639e99a062"/>
    <rom name="Himitsu Sentai Metamor V Deluxe (Japan) (Disc 1) [SLPS-01626].cue" size="125" crc="081f5055" sha1="7d7557d8457d706ef7543ba15a17ed6938a05264"/>
    -->
    <description>Himitsu Sentai Metamor V Deluxe (Japan, disc 1 only)</description>
    <year>1998</year>
    <publisher>Mycom</publisher>
    <info name="serial" value="SLPS-01626" />
    <info name="release" value="19981015" />
    <info name="alt_title" value="ひみつ戦隊メタモルVデラックス"/>
    <sharedfeat name="compatibility" value="NTSC-J"/>
    <part name="cdrom1" interface="cdrom">
        <diskarea name="cdrom">
            <disk name="himitsu sentai metamor v deluxe (japan) (disc 1) [slps-01626]" sha1="5621c1d350dc9eb7be2ebfea2f4b459493982f6a" status="baddump"/>
        </diskarea>
    </part>
    <part name="cdrom2" interface="cdrom">
        <diskarea name="cdrom">
            <disk name="himitsu sentai metamor v deluxe (japan) (disc 2) [slps-01627]" status="nodump"/>
        </diskarea>
    </part>
</software>
 */
