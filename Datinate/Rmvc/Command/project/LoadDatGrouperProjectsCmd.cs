using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.DatGrouper;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class LoadDatGrouperProjectsCmd : RCommand 
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

        protected override void Run() 
        {
            var shell = Facade.Instance?.Shell;

            if (shell != null &&
                abortIfDatGrouperSessionActive &&
                shell.CurrentProjectsPageIsProjectLoaderPage == false)
            {
                _ = shell.ShowMessageBox(
                    "Attention",
                    "Cannot proceed. Please Exit the active DAT Grouper Project and try again.");

                return;
            }

            if (showProjectsView)
                base.ExecuteCommand(new SetDatGrouperFormVisibleCmd());

            base.ExecuteCommand(new ShowProgressCmd("loading Projects View.", 1, 2));

            Facade.Instance?.Shell?.ShowProjectsView();
            Facade.Instance?.Shell?.SetProjectsFormTitle("DAT Grouper");

            base.ExecuteCommand(new ClearDatGrouperViewCmd());
            
            DatGrouperProjectDTO[] projectVOs = 
                Facade.Instance?.ProjectProxy?.LoadAllProjectVOs() ?? Array.Empty<DatGrouperProjectDTO>();

            Facade.Instance?.ProjectLoaderMediator?.SetView(projectVOs, projectToLoad);
            
            Facade.Instance?.LandingMediator?.SetDatGrouperProjects(projectVOs);

            base.ExecuteCommand(new ClearProgressCmd());
        }
    }
}
