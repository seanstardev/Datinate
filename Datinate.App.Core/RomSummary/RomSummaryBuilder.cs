using System.Collections.Generic;
using System.Linq;
using Datinate.Shared;
using Datinate.Shared.Dat;

namespace Datinate.App.Core.RomSummary
{
    public sealed class RomSummaryBuilder
    {
        private readonly List<RomExtensionSummary> summaries = new List<RomExtensionSummary>();

        /**
         * Slightly trickier than it looks: softlist -> megacd is an example that contains problems.
         * 
         * For example, it a chd named:
         *      -> world cup usa 94 (1994)(u.s. gold)(pal)(m8)[!]
         *  It should be logged as having no ext, but if we're not careful we can end up with this ext: 
         *      -> ' . gold)(pal)(m8)[!]'
         *      
         *  The added alphanumeric check below should avoid this:
         */
        public void AddRom(DatRomVO rom, string gameName)
        {
            string[] nameParts = rom.Name.Split('.');

            string extension = nameParts.Length == 1
                ? Constants.NO_VALUE
                : nameParts[nameParts.Length - 1];

            if (!extension.All(char.IsLetterOrDigit))
                extension = Constants.NO_VALUE;

            foreach (var summary in summaries)
            {
                if (summary.Extension == extension)
                {
                    summary.AddRom(gameName, rom.Size);
                    
                    return;
                }
            }

            summaries.Add(new RomExtensionSummary(
                extension, gameName, rom.Size));
        }

        public void AddGame(DatGameVO game)
        {
            foreach (var rom in game.Roms)
                AddRom(rom, game.Name);
        }

        public RomExtensionSummary[] GetSummaries() => summaries.ToArray();
    }
}
