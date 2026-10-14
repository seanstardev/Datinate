using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class StartupCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {   
            var projectRootPath = base.GlobalSettingsProxy?.ProjectRoot;

            if (projectRootPath == null)
                return await Complete();

            // Bundled seed resources are embedded in Datinate.Rmvc.
            var assembly = typeof(Facade).Assembly;

            base.DatDbProxy?.SetProjectRootPath(projectRootPath);

            base.ModelDataProxy?.SetProjectRootPath(
                projectRootPath,
                assembly);

            base.ExpressionsProxy?.SetProjectRootPath(projectRootPath);
            base.ProjectProxy?.SetProjectRootPath(projectRootPath);
            base.CuratedDatProxy?.SetProjectRootPath(projectRootPath);
            base.ExportDatGrouperProjectProxy?.SetProjectRootPath(projectRootPath);

            base.AudioProxy?.SetProjectRootPath(
                projectRootPath,
                assembly);

            base.GlobalSettingsProxy?.Startup();

            return await Complete();
        }
    }
}