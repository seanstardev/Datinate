using Datinate.Rmvc.Model;
using Datinate.Rmvc.Proxy;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class LoadDatGrouperContentPathsCmd : CommandBase<bool>
    {
        private readonly bool jumpToViewAfterLoad;

        public LoadDatGrouperContentPathsCmd(
            bool jumpToViewAfterLoad)
        {
            this.jumpToViewAfterLoad = jumpToViewAfterLoad;
        }

        protected override async Task<bool> Run()
        {
            if (base.DatGrouperSessionModel == null)
                return await Complete(false);

            var projectName = base.RadioDatModel?.ProjectName ?? string.Empty;

            bool autoLoadSuccessful = false;

            if (base.DatGrouperSessionModel.ContentPathsResolved == false)
            {
                if (base.RadioDatModel?.ProjectName == null ||
                    base.ProjectProxy == null)
                {
                    return await Complete(false);
                }

                var project = base.ProjectProxy.LoadProject(projectName);
                if (project == null)
                    return await Complete(false);

                await base.Execute(new ShowProgressCmd("Checking Content Paths", 1, 4));

                autoLoadSuccessful = project.GetAllDatContentPathsAreValidOrEmpty();

                await base.Execute(new ClearProgressCmd());
            }
            else
            {
                autoLoadSuccessful = true;

                await base.Execute(new ClearProgressCmd());

                if (jumpToViewAfterLoad)
                    await base.Execute(new LoadCurationEnvironmentCmd(projectName));
            }

            return await Complete(autoLoadSuccessful);
        }
    }
}
