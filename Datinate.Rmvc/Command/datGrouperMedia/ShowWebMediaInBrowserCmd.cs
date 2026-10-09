using RMVC;

namespace Datinate.Rmvc.Command
{
    public class ShowWebMediaInBrowserCmd : RCommand
    {
        protected override void Run()
        {
            Facade.Instance?.MediaWebMediator?.LoadUriInBrowser();
        }
    }
}
