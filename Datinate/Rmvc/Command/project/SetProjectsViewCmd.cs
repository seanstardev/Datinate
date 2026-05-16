using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SetProjectsViewCmd : RCommandAsync
    {
        protected async override Task RunAsync()
        {
            await base.ExecuteCommandAsync(new ShowProgressCmd("Reloading Projects View.", 1, 2));

            Facade.Instance?.Shell?.ShowRbProjectsView();
            Facade.Instance?.Shell?.SetProjectsFormTitle("DAT Grouper");
            Facade.Instance?.ProjectLoaderMediator?.ReloadCurrentProject();

            base.ExecuteCommand(new ClearProgressCmd());
        }
    }
}
