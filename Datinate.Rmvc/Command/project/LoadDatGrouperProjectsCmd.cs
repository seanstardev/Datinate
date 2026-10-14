using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class LoadDatGrouperProjectsCmd : CommandBase<RVoid>
    {
        private readonly string? projectToLoad;
        private readonly bool abortIfDatGrouperSessionActive;
        private readonly bool showProjectsView;

        public LoadDatGrouperProjectsCmd(
            string? projectToLoad, 
            bool abortIfDatGrouperSessionActive,
            bool showProjectsView) 
        {
            this.projectToLoad = projectToLoad;
            this.abortIfDatGrouperSessionActive = abortIfDatGrouperSessionActive;
            this.showProjectsView = showProjectsView;
        }

        protected override async Task<RVoid> Run() 
        {
            if (base.Shell != null &&
                abortIfDatGrouperSessionActive &&
                base.Shell.CurrentProjectsPageIsProjectLoaderPage == false)
            {
                _ = base.Shell.ShowMessageBox(
                    "Attention",
                    "Cannot proceed. Please Exit the active DAT Grouper Project and try again.");

                return await Complete();
            }

            if (showProjectsView)
                await base.Execute(new SetDatGrouperFormVisibleCmd());

            await base.Execute(new ShowProgressCmd("loading Projects View.", 1, 2));

            base.Shell?.ShowProjectsView();
            base.Shell?.SetProjectsFormTitle("DAT Grouper");

            await base.Execute(new ClearDatGrouperViewCmd());
            
            DatGrouperProjectDTO[] projectVOs =
                base.ProjectProxy?.LoadAllProjectVOs() ?? Array.Empty<DatGrouperProjectDTO>();

            base.ProjectLoaderMediator?.SetView(projectVOs, projectToLoad);

            base.LandingMediator?.SetDatGrouperProjects(projectVOs);

            await base.Execute(new ClearProgressCmd());

            return await Complete();
        }
    }
}
