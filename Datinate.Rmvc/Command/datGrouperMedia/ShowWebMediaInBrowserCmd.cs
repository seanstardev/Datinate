using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class ShowWebMediaInBrowserCmd : RCommand
    {
        protected override void Run()
        {
            Facade.Instance?.MediaWebMediator?.LoadUriInBrowser();
        }
    }
}
