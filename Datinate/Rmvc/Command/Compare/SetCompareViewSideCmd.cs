using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal class SetCompareViewSideCmd:RCommand 
    {
        private readonly bool left;

        public SetCompareViewSideCmd(bool left) 
        {
            this.left = left;
        }
        protected override void Run() 
        {
         
            Facade? instance = Facade.Instance;

            base.ExecuteCommand(new SetMainControlsBtnEnabledCmd(MAIN_CONTROL_ENUM.DatCompare, true));

            ActiveDatsModel? activeDatsModel = instance?.ActiveDatsModel;
            CompareMediator? compareMediator = instance?.CompareMediator;

            if (activeDatsModel != null)
            {
                DatVO? dat = activeDatsModel.DetailedDat;

                if (left && dat != null)
                {
                    activeDatsModel.CompareLeftDat = dat;
                    compareMediator?.SetLeft(dat);
                    Facade.Instance?.Shell?.SetCompareFormVisible(true);
                }
                else if (dat != null)
                {
                    activeDatsModel.CompareRightDat = dat;
                    compareMediator?.SetRight(dat);
                    Facade.Instance?.Shell?.SetCompareFormVisible(true);
                }
            }
        }
    }
}
