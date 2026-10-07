namespace com.RADIO.Datinate.RMVC.Shared
{
    public class DatSummaryVO
    {
        public string DatFullpath { get; }
        public int GamesTotal { get; }
        public int RomsTotal { get; }
        public ulong SizeTotal { get; }
        public DatHeaderVO DatHeader { get; }
        public string Folder { get; }

        public DatSummaryVO(
            string datFullpath,
            int gamesTotal, 
            int romsTotal, 
            ulong sizeTotal, 
            DatHeaderVO datHeader,
            string folder)
        {
            DatFullpath = datFullpath;
            GamesTotal = gamesTotal;
            RomsTotal = romsTotal;
            SizeTotal = sizeTotal;
            DatHeader = datHeader;
            Folder = folder;
        }
    }
}
