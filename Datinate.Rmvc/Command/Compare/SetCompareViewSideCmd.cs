using Datinate.Shared.Dat;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class SetCompareViewSideCmd : CommandBase<RVoid> 
    {
        private readonly bool left;

        public SetCompareViewSideCmd(bool left) 
        {
            this.left = left;
        }
        protected override async Task<RVoid> Run() 
        {
            await base.Execute(new SetMainControlsBtnEnabledCmd(
                MAIN_CONTROL_ENUM.DatCompare, true));

            if (base.ActiveDatsModel != null)
            {
                DatVO? dat = base.ActiveDatsModel.DetailedDat;

                if (left && dat != null)
                {
                    base.ActiveDatsModel.CompareLeftDat = dat;
                    base.CompareMediator?.SetLeft(dat);
                    base.Shell?.SetCompareFormVisible(true);
                }
                else if (dat != null)
                {
                    base.ActiveDatsModel.CompareRightDat = dat;
                    base.CompareMediator?.SetRight(dat);
                    base.Shell?.SetCompareFormVisible(true);
                }
            }

            return await Complete();
        }
    }
}
