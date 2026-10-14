using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ExitApplicationCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            base.Shell?.ExitApplication();

            return await Complete();
        }
    }
}
