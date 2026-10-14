using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ClearDatSummaryViewCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run() 
        {
            base.DatSummaryMediator?.ClearView();

            return await Complete();
        }
    }
}
