using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    /// <summary>
    /// Show the difference DAT of the left and right Dats:
    /// Don't update model as we haven't committed to anything. 
    /// </summary>
    internal class SetCompareViewResultCmd : CommandBase<RVoid> 
    {
        private readonly DatComparisonVO vo;

        public SetCompareViewResultCmd(DatComparisonVO vo) 
        {
            this.vo = vo;
        }

        protected override async Task<RVoid> Run() 
        {
            base.CompareMediator?.SetComparisonResult(vo);

            return await Complete();
        }
    }
}
