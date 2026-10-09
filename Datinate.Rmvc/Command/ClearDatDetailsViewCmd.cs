using RMVC;

namespace Datinate.Rmvc.Command
{
    public class ClearDatDetailsViewCmd : RCommand 
    {
        protected override void Run() 
        {
            Facade.Instance?.DatDetailsMediator?.ResetView();
        }
    }
}
