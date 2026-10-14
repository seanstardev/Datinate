using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ClearDatDetailsViewCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run() 
        {
            base.DatDetailsMediator?.ResetView();

            return await Complete();
        }
    }
}
