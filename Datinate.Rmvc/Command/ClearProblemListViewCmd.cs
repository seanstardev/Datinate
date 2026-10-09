using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ClearProblemListViewCmd : RCommand 
    {
        protected override void Run() 
        {
            Facade.Instance?.ProblemListMediator?.ClearView();
        }
    }
}
