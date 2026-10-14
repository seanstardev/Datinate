
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ClearCompareDatsModelAndViewCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run() 
        {
            if (base.ActiveDatsModel != null)
            {
                base.ActiveDatsModel.CompareLeftDat = null;
                base.ActiveDatsModel.CompareRightDat = null;

                base.CompareMediator?.ClearView();
            }

            return await Complete();
        }
    }
}
