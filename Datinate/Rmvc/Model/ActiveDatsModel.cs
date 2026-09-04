using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class ActiveDatsModel : RModel
    {
        public DatSummaryVO[]? AllStoredDats { get; set; } = null;
        public DatGrouperProjectEntry[]? HierarchyDats { get; set; } = null;
        public DatVO? DetailedDat { get; set; } = null;
        public DatVO? CustomiseDat { get; set; } = null;
        public DatVO? CompareLeftDat { get; set; } = null;
        public DatVO? CompareRightDat { get; set; } = null;

        public DatSummaryVO? GetDatSummary(string fullpath) 
        {
            if (AllStoredDats == null)
                return null;

            foreach (var dat in AllStoredDats)
            {
                if (dat.DatFullpath.ToLower() == fullpath.ToLower())
                    return dat;
            }
            
            return null;
        }
    }
}
