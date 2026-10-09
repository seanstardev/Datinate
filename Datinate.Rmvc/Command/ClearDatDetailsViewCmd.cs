using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class ClearDatDetailsViewCmd : RCommand 
    {
        protected override void Run() 
        {
            Facade.Instance?.DatDetailsMediator?.ResetView();
        }
    }
}
