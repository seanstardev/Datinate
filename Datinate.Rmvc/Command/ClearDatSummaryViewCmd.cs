using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ClearDatSummaryViewCmd : RCommand 
    {
        protected override void Run() 
        {
            Facade.Instance?.DatSummaryMediator?.ClearView();
        }
    }
}
