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

            var sessionModel = Facade.Instance?.DatGrouperSessionModel;
            
            // NOTE: When in DatGrouper-only mode we won't have a project to reload.
            if (sessionModel is { } && sessionModel.DatGrouperStartupProject is { } startupProject)
            {
                // TODO: Load sessionModel.DatGrouperStartupProject in Projects view.
                sessionModel.DatGrouperStartupProject = null;
            }
            else
            {
                Facade.Instance?.ProjectLoaderMediator?.ReloadCurrentProject();
            }
            base.ExecuteCommand(new ClearProgressCmd());
        }
    }
}
