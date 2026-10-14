using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ClearDatGrouperViewCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run() 
        {
            base.DatGrouperMediator?.ResetView();

            return await Complete();
        }
    }
}
