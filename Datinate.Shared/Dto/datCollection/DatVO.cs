namespace com.RADIO.Datinate.RMVC.Shared
{
    public class DatVO 
    {
        public string DatFullpath { get; }          // The full path of the DAT, including its extension
        public string DatFolder { get; }            // this is just the name of the folder in the DAT fullpath

        public IReadOnlyCollection<DatGameVO> Entries { get; }

        public int GamesTotal { get; private set; }
        public int RomsTotal { get; private set; }
        public ulong SizeTotal { get; private set; }

        public DatHeaderVO DatHeaderVO { get; }
        public string? RawDatText { get; }
        
        public bool HasPCloneStructure { get; set; } = false;
        public bool AlwaysOneRomPerGame { get; set; } = false;
        public bool ContainsChds { get; set; } = false;
        
        public DatVO(
            string datFullpath
            , DatHeaderVO datHeaderVO
            , IReadOnlyCollection<DatGameVO> entries
            , string? rawDatText = null
        ) {
            DatFullpath = datFullpath;
            DatHeaderVO = datHeaderVO;
            RawDatText = rawDatText;

            Entries = entries;
            
            GamesTotal = Entries.Count;
            RomsTotal = Entries.Sum(g => g.Roms.Length);
            SizeTotal = Entries.SelectMany(g => g.Roms).Aggregate(0UL, (sum, rom) => sum + rom.Size);

            try
            {
                if (string.IsNullOrWhiteSpace(datFullpath))
                    DatFolder = string.Empty;
                else
                    DatFolder = Path.GetFileName(Path.GetDirectoryName(datFullpath)) ?? string.Empty;
            }
            catch (PathTooLongException)
            { DatFolder = string.Empty; }
            catch (Exception)
            { DatFolder = string.Empty; }
        }

        public string GetDatNameWithoutExt() 
        {
            return Path.GetFileNameWithoutExtension(DatFullpath);
        }

        public string[] GetEntryNames()
        {
            return Entries.Select(e => e.Name).ToArray();
        }

        public DatSummaryVO CreateSummary()
        {
            return new DatSummaryVO(DatFullpath, GamesTotal, RomsTotal, SizeTotal, DatHeaderVO, DatFolder);
        }

        public static DatVO CreateDatFrom(DatVO dat, IReadOnlyCollection<DatGameVO> replacementEntries)
        {
            var newDat = new DatVO(
                dat.DatFullpath, dat.DatHeaderVO, replacementEntries, dat.RawDatText)
            {
                AlwaysOneRomPerGame = dat.AlwaysOneRomPerGame,
                ContainsChds = dat.ContainsChds,
                HasPCloneStructure = dat.HasPCloneStructure,
            };

            return newDat;
        }
    }
}
