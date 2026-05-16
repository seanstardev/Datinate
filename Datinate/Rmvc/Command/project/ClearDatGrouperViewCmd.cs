using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class ClearDatGrouperViewCmd : RCommand 
    {
        public ClearDatGrouperViewCmd() 
        {
        }
    
        protected override void Run() 
        {
            Facade.Instance?.DatGrouperMediator?.ResetView();
        }
    }
}
