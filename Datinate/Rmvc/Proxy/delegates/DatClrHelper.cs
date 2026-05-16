using com.RADIO.Datinate.RMVC.Shared;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal class DatClrHelper 
    {
        public static DatVO? GetDat(string datFullpath, string datRawText)
        {
            try
            {
                var header = GetDatHeader(datRawText);
                if (header == null)
                    return null;

                var entries = RetrieveGamesDetails(datRawText, out bool hasPClone, out bool oneRomPerGame);

                var dat = new DatVO(
                    datFullpath,
                    header, 
                    entries,
                    datRawText);

                dat.HasPCloneStructure = hasPClone;
                dat.AlwaysOneRomPerGame = oneRomPerGame;

                return dat;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            return null;
        }

        //public static ulong GetTotalRomsSize(string raw) {

        //    // is not necessarily preceded by a space. So not valid: (>" size <). Valid:
        //    string pattern = " size \\d+ "; // If changed check 'String adapted' substring as well (below)!
        //    Regex rgx = new Regex(pattern);

        //    ulong totalSize = 0;

        //    foreach (Match match in rgx.Matches(raw)) {

        //        string result = match.Value;
        //        string adapted = result.Substring(6);
        //        adapted = adapted.TrimEnd(' ');

        //        try {
        //            totalSize += Convert.ToUInt64(adapted);
        //        }
        //        catch (System.FormatException) {
        //            Debug.Print("DatClrHelper: ERROR: result: >" + result + "<, adapted: >" + adapted + "<");
        //        }
        //    }
        //    return totalSize;
        //}

        /**
         * Get high level info on the DAT
         */
        private static DatHeaderVO? GetDatHeader(string raw) {
            string? part = GetRawHeaderText(raw);

            if (part == null)
                return null;
            
            DatHeaderVO datHeaderVO = new DatHeaderVO(
                DAT_FORMAT_ENUM.ClrMamePro
                , DatClrHelper.GetItem(part, "name", 0, false) ?? string.Empty
                , DatClrHelper.GetItem(part, "description", 0, false) ?? string.Empty
                , DatClrHelper.GetItem(part, "category", 0, false) ?? string.Empty
                , DatClrHelper.GetItem(part, "version", 0, true) ?? string.Empty
                , DatClrHelper.GetItem(part, "author", 0, false) ?? string.Empty
                , DatClrHelper.GetItem(part, "comment", 0, false) ?? string.Empty
            );
            return datHeaderVO;
        }

        /**
         * Identify the area of the file (expected to be at the top) that
         * contains the header information
         */
        private static string? GetRawHeaderText(String raw) {

            string startPattern = "clrmamepro";
            string startStr = startPattern + " (\r";


            string endStr = "\r)";

            int start = raw.IndexOf(startStr);

            if (start < 0) 
                return null;

            int newStart = start + startStr.Length;

            int end = raw.IndexOf(endStr, newStart);

            if (end < 0) 
                return null;

            end += endStr.Length;

            string part = raw.Substring(start, end - start);
            return part;
        }

        /**
         * DAT standards seem to require value names to be preceded by a tab (\t) followed by value.
         * Quotes may or may not surround the value
         */
        private static string? GetItem(string raw, string itemName, int startIndex, bool ignoreQuotes) {

            string quoteStr = "\"";
            if (ignoreQuotes)
                quoteStr = "";

            string startStr = "\t" + itemName + " " + quoteStr;

            string endStr = quoteStr + "\r";

            int start = raw.IndexOf(startStr, startIndex);

            if (start < 0) 
                return null;
            

            int newStart = start + startStr.Length;

            int end = raw.IndexOf(endStr, newStart);

            if (end < 0) 
                return null;

            string substr = raw.Substring(newStart, end - newStart);

            return substr;
        }

        /**
         * With thanks:
         * http://www.dotnetperls.com/string-occurrence
         */
        //public static int CountStringOccurrences(string text, string pattern) {
        //    // Loop through all instances of the string 'text'.
        //    int count = 0;
        //    int i = 0;
        //    while ((i = text.IndexOf(pattern, i)) != -1) {
        //        i += pattern.Length;
        //        count++;
        //    }
        //    return count;
        //}

        private static DatGameVO [] RetrieveGamesDetails(string raw, out bool hasPCloneStructure, out bool alwaysOneRomPerGame) {

            hasPCloneStructure = false;
            alwaysOneRomPerGame = true;

            List<DatGameVO> games = new List<DatGameVO>();

            // NOTE: [string standardisation change] -> String pattern = "\ngame \\((.*?)\n\\)\r";
            string pattern = "\rgame \\((.*?)\r\\)\r";
            Regex rgx = new Regex(pattern, RegexOptions.Singleline);

            foreach (Match match in rgx.Matches(raw)) {
                var part = match.Value;

                // NOTE: the name value should probably always have quotes around it, 
                // but it seems this is not always the case. e.g. PC Games (DRM Free) (20140621).dat
                string? name = DatClrHelper.GetItem(part, "name", 0, false);

                if (name == null || name  == "")
                    name = DatClrHelper.GetItem(part, "name", 0, true);

                var roms = RetrieveRoms(part);

                if (roms.Length > 1)
                    alwaysOneRomPerGame = false;

                var game = new DatGameVO(
                    name ?? string.Empty
                    , DatClrHelper.GetItem(part, "description", 0, false)
                    , null
                    , null
                    , null
                    , null
                    , null
                    , null
                    , roms
                    , null
                );
                games.Add(game);

            }
            return games.ToArray();
        }

        /**
         * Parse a game encapsulated area of text for component ROM data
         */
        private static DatRomVO [] RetrieveRoms(string raw) {

            string pattern = "\trom \\( (.*?) \\)\r";
            Regex rgx = new Regex(pattern);

            string part;
            DatRomVO rom;

            List<DatRomVO> roms = new List<DatRomVO>();

            string name;
            string part2;

            foreach (Match match in rgx.Matches(raw)) {

                part = match.Value;
                name = GetRomAttribute(part, "name", false);

                // NOTE: make sure we only remove the first instance of the string match (we're careful
                // as description may be the same value, for example).
                int index = part.IndexOf(name);
                part2 = (index < 0)
                    ? part
                    : part.Remove(index, name.Length);

                // NOTE: Size can be null!! (Commodore 64 (merged) WOZ DAT)
                // rom ( name "Castles of Doctor Creep [m2i] (45).prg" md5 d41d8cd98f00b204e9800998ecf8427e sha1 da39a3ee5e6b4b0d3255bfef95601890afd80709 )
                string size = GetRomAttribute(part2, "size", true);

                try {
                    rom = new DatRomVO(
                        name
                        , false
                        , (size == null) ? 0 : UInt64.Parse(GetRomAttribute(part2, "size", true))
                        , GetRomAttribute(part2, "crc", true)
                        , GetRomAttribute(part2, "md5", true)
                        , GetRomAttribute(part2, "sha1", true)
                    );
                    roms.Add(rom);
                }
                catch(Exception e) {

                    Debug.WriteLine(e.ToString());
                    Debug.WriteLine(part);

                    Debug.WriteLine("a-|----- " + GetRomAttribute(part, "name", false));
                    Debug.WriteLine("b-|----- " + GetRomAttribute(part, "size", true));
                    Debug.WriteLine("c-|----- " + UInt64.Parse(GetRomAttribute(part, "size", true)));
                    Debug.WriteLine("d-|----- " + GetRomAttribute(part, "crc", true));
                    Debug.WriteLine("e-|----- " + GetRomAttribute(part, "md5", true));
                    Debug.WriteLine("f-|----- " + GetRomAttribute(part, "sha1", true));
                }
            }
            return roms.ToArray();
        }

        /**
         * Note that ignoreQuotes is only a recommendation - if the quoted attname isn't
         * found we try again without the quotes:
         */
        private static string GetRomAttribute(string raw, string romAttribute, bool ignoreQuotes) {
            string quoteStr = "\"";
            if (ignoreQuotes)
                quoteStr = "";

            // NOTE: space at start as well as end:
            string startStr = " " + romAttribute + " " + quoteStr;

            string endStr = quoteStr + " ";

            int start = raw.IndexOf(startStr);

            // NOTE: Quotes are not consistently added amongst TOSEC and other sets, so this fallback exists:
            if (start < 0 && !ignoreQuotes)
                return GetRomAttribute(raw, romAttribute, true);


            if (start < 0) {
                return string.Empty;
            }
            int newStart = start + startStr.Length;

            int end = raw.IndexOf(endStr, newStart);

            if (end < 0) {
                return string.Empty;
            }

            string substr = raw.Substring(newStart, end - newStart);
            return substr;
        }

        /**
         * Depending on the DAT creation tool that has been used, \n and \r can be used interchangeably.
         * Here we ensure that we only need to look for: \r.
         * This saves us making multiple checks throughout analyses later on. Code readability and speed benefit.
         */
        private static string StandardiseCarriageReturns(string text) {
            
            // NOTE: Do not remove this new line as it may help encapsulate items later:
            text += "\r"; 

            return text.Replace('\n', '\r');
        }
    }
}
