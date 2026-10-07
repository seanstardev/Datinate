namespace com.RADIO.Datinate.RMVC.Shared
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
