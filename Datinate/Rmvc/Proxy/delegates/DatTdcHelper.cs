using com.RADIO.Datinate.RMVC.Shared;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal static class DatTdcHelper
    {
        public static DatVO? GetDat(string datFullpath, string text)
        {
            try
            {
                return GetDatInternal(datFullpath, text);
            }
            catch {
                return null;
            }
        }
        private static DatVO? GetDatInternal(string datFullpath, string text)
        {
            bool hasPCloneStructure = false; // No parent-clone structure in DOSCenter format
            bool alwaysOneRomPerGame = true; // Default assumption

            List<DatGameVO> gamesList = new List<DatGameVO>();

            // Split text into lines and trim each line
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(line => line.Trim())
                            .ToArray();

            // Header parsing
            var headerValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool isHeader = false;
            foreach (var line in lines)
            {
                if (line == "DOSCenter (")
                {
                    isHeader = true;
                    continue;
                }
                if (isHeader && line == ")")
                {
                    break;
                }
                if (isHeader)
                {
                    var split = line.Split(new[] { ':' }, 2);
                    if (split.Length == 2)
                    {
                        var key = split[0].Trim();
                        var value = split[1].Trim();
                        headerValues[key] = value; // Overwrite if duplicates
                    }
                }
            }
            string name = headerValues.TryGetValue("Name", out var nameValue) ? nameValue : string.Empty;
            string description = headerValues.TryGetValue("Description", out var descValue) ? descValue : string.Empty;
            string version = headerValues.TryGetValue("Version", out var versionValue) ? versionValue : string.Empty;
            string author = headerValues.TryGetValue("Author", out var authorValue) ? authorValue : string.Empty;
            string comment = headerValues.TryGetValue("Comment", out var commentValue) ? commentValue : string.Empty;

            // Create DatHeaderVO
            var datHeader = new DatHeaderVO(
                datTypeEnum: DAT_FORMAT_ENUM.DosCenter,
                name: name,
                description: description,
                category: string.Empty, // DOSCenter does not have a category
                version: version,
                author: author,
                comment: comment
            );

            // Game parsing
            bool inGameBlock = false;
            List<DatRomVO> roms = new List<DatRomVO>();
            string gameName = string.Empty;
            int totalRomCount = 0;
            ulong totalSize = 0;

            foreach (var line in lines.Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)))
            {
                // Detect start of a game block
                if (line == "game (")
                {
                    inGameBlock = true;
                    roms = new List<DatRomVO>(); // Initialize rom list for the new game
                    continue;
                }

                // Detect end of a game block
                if (inGameBlock && line == ")")
                {
                    // Finalize the current game
                    var game = new DatGameVO(
                        gameName,
                        null,
                        null, 
                        null, 
                        null,
                        null,
                        null,
                        null,
                        roms.ToArray(), 
                        null);
                    
                    gamesList.Add(game);

                    if (roms.Count > 1)
                        alwaysOneRomPerGame = false;

                    // Reset for next game
                    inGameBlock = false;
                    gameName = string.Empty;
                    continue;
                }

                if (inGameBlock)
                {
                    // Parse game name
                    if (line.StartsWith("name "))
                    {
                        gameName = line.Substring(5).Trim('"');
                    }
                    // Parse file entries
                    else if (line.StartsWith("file ("))
                    {
                        string trimmedLine = line.Substring(6, line.Length - 8);

                        // Extract key-value pairs from end to start
                        string sha1 = ExtractValue(ref trimmedLine, " sha1 ");
                        string crc = ExtractValue(ref trimmedLine, " crc ");
                        string date = ExtractValue(ref trimmedLine, " date ");
                        string sizeStr = ExtractValue(ref trimmedLine, " size ");
                        string fileName = ExtractValue(ref trimmedLine, " name ");

                        ulong size = ulong.TryParse(sizeStr, out var parsedSize) ? parsedSize : 0;

                        // Add ROM to the list
                        roms.Add(new DatRomVO(fileName, false, size, crc, string.Empty, sha1));

                        // Update totals
                        totalRomCount++;
                        totalSize += size;
                    }
                }
            }

            DatVO datVO = new DatVO(datFullpath, datHeader, gamesList, text);

            datVO.AlwaysOneRomPerGame = alwaysOneRomPerGame;
            datVO.HasPCloneStructure = hasPCloneStructure;
            return datVO;
        }
        static string ExtractValue(ref string line, string key)
        {
            int keyIndex = line.LastIndexOf(key);
            string value = line.Substring(keyIndex + key.Length).Trim();
            line = line.Substring(0, keyIndex);
            return value;
        }
    }
}
