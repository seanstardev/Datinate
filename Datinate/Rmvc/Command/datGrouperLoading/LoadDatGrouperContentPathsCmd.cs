using datinate.app;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class LoadDatGrouperContentPathsCmd : RCommandAsync
    {
        public bool AutoLoadSuccessful { get; private set; } = false;
        private readonly bool jumpToViewAfterLoad;
        private readonly bool forceJumpToCfgView;
        private readonly bool clearProgressWhenDone;

        public LoadDatGrouperContentPathsCmd(
            bool jumpToViewAfterLoad, 
            bool forceJumpToCfgView,
            bool clearProgressWhenDone = true)
        {
            this.jumpToViewAfterLoad = jumpToViewAfterLoad;
            this.forceJumpToCfgView = forceJumpToCfgView;
            this.clearProgressWhenDone = clearProgressWhenDone;
        }

        protected override async Task RunAsync()
        {
            var facade = Facade.Instance;

            RadioDatModel? radioDatModel = facade?.RadioDatModel;
            ProjectProxy? projectProxy = facade?.ProjectProxy;
            DatGrouperSettingsMediator? contentPathsMediator = facade?.ContentPathsMediator;
            DatGrouperSessionModel? sessionModel = facade?.DatGrouperSessionModel;

            if (sessionModel == null)
                return;

            var projectName = radioDatModel?.ProjectName ?? string.Empty;

            if (forceJumpToCfgView || !sessionModel.ContentPathsResolved)
            {
                if (radioDatModel?.ProjectName == null ||
                    projectProxy == null ||
                    contentPathsMediator == null)
                {
                    return;
                }

                var project = projectProxy.LoadProject(projectName);
                if (project == null) 
                    return;

                await base.ExecuteCommandAsync(new ShowProgressCmd("Checking Content Paths", 1, 4));

                AutoLoadSuccessful = project.GetAllPathsAreValidOrEmpty();

                if (jumpToViewAfterLoad && (!AutoLoadSuccessful || forceJumpToCfgView))
                {
                    contentPathsMediator.SetView(project, DescriptorChipUtil.DescriptorDefinitions);
                    facade?.Shell?.ShowContentPathsView();
                }

                if (clearProgressWhenDone)
                    base.ExecuteCommand(new ClearProgressCmd());
            }
            else
            {
                AutoLoadSuccessful = true;

                if (clearProgressWhenDone)
                    base.ExecuteCommand(new ClearProgressCmd());

                if (jumpToViewAfterLoad)
                  await base.ExecuteCommandAsync(new LoadCurationEnvironmentCmd(projectName));
            }
            return;
        }
    }
}
