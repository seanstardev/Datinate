using RMVC;

namespace com.RADIO.Datinate.RMVC
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
