using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ShowWebMediaInBrowserCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            base.MediaWebMediator?.LoadUriInBrowser();
            
            return await Complete();
        }
    }
}
