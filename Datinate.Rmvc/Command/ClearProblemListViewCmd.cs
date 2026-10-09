using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class ClearProblemListViewCmd : RCommand 
    {
        protected override void Run() 
        {
            Facade.Instance?.ProblemListMediator?.ClearView();
        }
    }
}
