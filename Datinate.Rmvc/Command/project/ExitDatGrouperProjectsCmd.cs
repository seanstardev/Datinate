using Datinate.Rmvc.Command;
using RMVC;

namespace Datinate.Rmvc.Command
{
    public class ExitDatGrouperProjectsCmd : RCommandAsync
    {
        protected async override Task RunAsync()
        {
            base.ExecuteCommand(new ClearDatGrouperSessionCmd());
            base.ExecuteCommand(new LoadDatGrouperProjectsCmd(
                null,
                false,
                true));
            
            Facade.Instance?.Shell?.SetProjectsFormVisible(false);
        }
    }
}
