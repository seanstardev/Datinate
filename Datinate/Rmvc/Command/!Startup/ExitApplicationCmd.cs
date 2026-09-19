using com.RADIO.Datinate;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ExitApplicationCmd : RCommand
    {
        protected override void Run()
        {
            Facade.Instance?.Shell?.ExitApplication();
        }
    }
}
