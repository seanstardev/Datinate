using RMVC;

namespace Datinate.Rmvc.Command
{
    public class StartupCmd : RCommand
    {
        protected override void Run()
        {   

            var facade = Facade.Instance;
            var projectRootPath = facade?.GlobalSettingsProxy?.ProjectRoot;

            if (facade == null || projectRootPath == null)
                return;


            // Bundled seed resources are embedded in Datinate.Rmvc.
            var assembly = typeof(Facade).Assembly;


            facade.DatDbProxy?.SetProjectRootPath(projectRootPath);

            facade.ModelDataProxy?.SetProjectRootPath(
                projectRootPath,
                assembly);

            facade.ExpressionsProxy?.SetProjectRootPath(projectRootPath);
            facade.ProjectProxy?.SetProjectRootPath(projectRootPath);
            facade.CuratedDatProxy?.SetProjectRootPath(projectRootPath);
            facade.ExportDatGrouperProjectProxy?.SetProjectRootPath(projectRootPath);

            facade.AudioProxy?.SetProjectRootPath(
                projectRootPath,
                assembly);

            facade.GlobalSettingsProxy?.Startup();
        }
    }
}