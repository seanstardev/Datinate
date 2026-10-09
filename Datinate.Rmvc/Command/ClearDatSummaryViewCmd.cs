using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class ClearDatSummaryViewCmd : RCommand 
    {
        protected override void Run() 
        {
            Facade.Instance?.DatSummaryMediator?.ClearView();
        }
    }
}
