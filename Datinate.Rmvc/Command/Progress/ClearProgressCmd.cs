using RMVC;

namespace Datinate.Rmvc.Command
{
    public class ClearProgressCmd : RCommand 
    {
        protected override void Run() 
        {
            Facade.Instance?.Shell?.SetProgressFormVisible(false);
            Facade.Instance?.ProgressMediator?.ClearProgress();
        }
    }
}
