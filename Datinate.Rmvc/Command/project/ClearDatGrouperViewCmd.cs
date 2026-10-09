using RMVC;

namespace Datinate.Rmvc.Command
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
