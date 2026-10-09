using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class ClearCompareDatsModelAndViewCmd : RCommand 
    {
        protected override void Run() 
        {
            if (Facade.Instance?.ActiveDatsModel != null)
            {
                ActiveDatsModel activeDatsModel = Facade.Instance.ActiveDatsModel;

                activeDatsModel.CompareLeftDat = null;
                activeDatsModel.CompareRightDat = null;

                Facade.Instance.CompareMediator?.ClearView();
            }
        }
    }
}
