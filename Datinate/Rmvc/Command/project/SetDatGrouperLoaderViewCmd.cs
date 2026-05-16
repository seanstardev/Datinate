using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SetDatGrouperLoaderViewCmd : RCommand 
    {
        private readonly string? projectToLoad;
        private readonly bool skipProjectLoaderPageCheck;

        public SetDatGrouperLoaderViewCmd(string? projectToLoad = null, bool skipProjectLoaderPageCheck = false) 
        {
            this.projectToLoad = projectToLoad;
            this.skipProjectLoaderPageCheck = skipProjectLoaderPageCheck;
        }

        protected override void Run() 
        {
            if (skipProjectLoaderPageCheck == false &&
                Facade.Instance?.Shell is { } shell &&
                shell.CurrentProjectsPageIsProjectLoaderPage == false)
            {
                shell.ShowMessageBox(
                    "Attention",
                    "Cannot proceed. Please Exit the active DAT Grouper Project and try again.");

                base.ExecuteCommand(new SetDatGrouperFormVisibleCmd());
                return;
            }

            base.ExecuteCommand(new ClearDatGrouperViewCmd());
            
            DatGrouperProjectDTO[] projectVOs = Facade.Instance?.ProjectProxy?.LoadAllProjectVOs() ?? new DatGrouperProjectDTO[] { };

            Facade.Instance?.ProjectLoaderMediator?.SetView(projectVOs, projectToLoad);
            Facade.Instance?.LandingMediator?.SetDatGrouperProjects(projectVOs);
        }
    }
}
