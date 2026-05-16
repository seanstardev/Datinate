using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    /// <summary>
    /// Show the difference DAT of the left and right Dats:
    /// Don't update model as we haven't committed to anything. 
    /// </summary>
    internal class SetCompareViewResultCmd : RCommand 
    {
        private readonly DatComparisonVO vo;

        public SetCompareViewResultCmd(DatComparisonVO vo) 
        {
            this.vo = vo;
        }

        protected override void Run() 
        {
            Facade.Instance?.CompareMediator?.SetComparisonResult(vo);
        }
    }
}
