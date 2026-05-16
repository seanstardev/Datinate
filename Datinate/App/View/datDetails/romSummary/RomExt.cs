namespace datinate.shared
{
    public class RomExt
    {

        string? lastGameName = null;
        string extension;

        UInt64 totalSize = 0;

        uint romTally = 1;
        uint gameTally = 1;

        public RomExt(string extension, string lastGameName, UInt64 size) 
        {
            this.lastGameName = lastGameName;
            this.extension = extension;
            totalSize = size;
        }
        public void updateRomCount(string gameName, UInt64 size) 
        {
            romTally++;
            totalSize += size;

            if (gameName != lastGameName) 
            {
                gameTally++;
                lastGameName = gameName;
            }
        }

        public UInt64 getTotalSize() =>
            totalSize;
        
        public uint getRomOccurances() =>
            romTally;
        
        public uint getGameOccurances() =>
            gameTally;
        
        public string getExtension() =>
            extension;
        
        public Decimal getPercentage(UInt64 totalDatSize) 
        {
            if (totalDatSize == 0 || totalSize == 0)
                return 0M;
            else 
                return (((Decimal)totalSize / (Decimal)totalDatSize) * 100M); ;
        }
    }
}
