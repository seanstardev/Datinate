using datinate.app;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class LoadDatGrouperContentPathsCmd : RCommandAsync
    {
        public bool AutoLoadSuccessful { get; private set; } = false;
        private readonly bool jumpToViewAfterLoad;

        public LoadDatGrouperContentPathsCmd(
            bool jumpToViewAfterLoad)
        {
            this.jumpToViewAfterLoad = jumpToViewAfterLoad;
        }

        protected override async Task RunAsync()
        {
            var facade = Facade.Instance;

            RadioDatModel? radioDatModel = facade?.RadioDatModel;
            ProjectProxy? projectProxy = facade?.ProjectProxy;
            DatGrouperSessionModel? sessionModel = facade?.DatGrouperSessionModel;

            if (sessionModel == null)
                return;

            var projectName = radioDatModel?.ProjectName ?? string.Empty;

            if (!sessionModel.ContentPathsResolved)
            {
                if (radioDatModel?.ProjectName == null ||
                    projectProxy == null)
                {
                    return;
                }

                var project = projectProxy.LoadProject(projectName);
                if (project == null) 
                    return;

                base.ExecuteCommand(new ShowProgressCmd("Checking Content Paths", 1, 4));

                AutoLoadSuccessful = project.GetAllDatContentPathsAreValidOrEmpty();

                base.ExecuteCommand(new ClearProgressCmd());
            }
            else
            {
                AutoLoadSuccessful = true;

                base.ExecuteCommand(new ClearProgressCmd());

                if (jumpToViewAfterLoad)
                    await base.ExecuteCommandAsync(new LoadCurationEnvironmentCmd(projectName));
            }

        }
    }
}
