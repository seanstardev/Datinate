using Datinate.Rmvc.Model;
using RMVC;

namespace Datinate.Rmvc.Command
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
