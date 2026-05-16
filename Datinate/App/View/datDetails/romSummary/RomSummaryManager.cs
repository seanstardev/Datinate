using com.RADIO.Datinate.RMVC.Shared;

namespace datinate.shared
{
    public class RomSummaryManager 
    {

        List<RomExt> romExtVOs = new List<RomExt>();


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
        public void manageRom(DatRomVO vo, string gameName) 
        {
            string [] arr = vo.Name.Split('.');
            string ext = arr.Length == 1 ? Constants.NO_VALUE : arr[arr.Length-1];

            if (!ext.All(char.IsLetterOrDigit))
                ext = Constants.NO_VALUE;

            foreach (RomExt romExt in romExtVOs) 
            {
                if (romExt.getExtension() == ext) 
                {
                    romExt.updateRomCount(gameName, vo.Size);
                    return;
                }
            } 

            romExtVOs.Add(new RomExt(ext, gameName, vo.Size));
        }

        public void manageRoms(DatGameVO game) 
        {    
            foreach (DatRomVO vo in game.Roms) 
                manageRom(vo, game.Name);
        }

        public RomExt[] getSummaries() 
        {
            return romExtVOs.ToArray();
        }
    }
}
