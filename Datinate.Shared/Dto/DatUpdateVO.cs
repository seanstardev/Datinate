using Datinate.Shared.DatGrouper;

namespace Datinate.Shared
{
    public class DatUpdateVO 
    {
        public string NewFullpath { get; }
        public DatGrouperProjectEntry DatHeadlineVO { get; }
        public DatUpdateVO(
            string newFullpath
            , DatGrouperProjectEntry datHeadlineVO) 
        {
            NewFullpath = newFullpath;
            DatHeadlineVO = datHeadlineVO;
        }
    }
}
