namespace Datinate.App.Shared.RomSummary
{
    public sealed class RomExtensionSummary
    {
        private string lastGameName;

        public string Extension { get; }
        public ulong TotalSize { get; private set; }
        public uint RomCount { get; private set; } = 1;
        public uint GameCount { get; private set; } = 1;

        internal RomExtensionSummary(string extension, string gameName, ulong size)
        {
            Extension = extension;
            lastGameName = gameName;
            TotalSize = size;
        }

        internal void AddRom(string gameName, ulong size)
        {
            RomCount++;
            TotalSize += size;

            // TODO: Review whether counting game-name transitions rather than distinct games is desired.
            // Preserve existing counting: consecutive ROMs of this extension from one
            // game count once. Callers add each game's ROMs together through AddGame().
            if (gameName != lastGameName)
            {
                GameCount++;
                lastGameName = gameName;
            }
        }

        public decimal GetSizePercentage(ulong totalDatSize)
            => totalDatSize == 0 || TotalSize == 0
                ? 0m
                : ((decimal)TotalSize / totalDatSize) * 100m;
    }
}
