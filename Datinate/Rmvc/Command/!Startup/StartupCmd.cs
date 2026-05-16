using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class StartupCmd : RCommand 
    {
        protected override void Run() 
        {
            var projectRootPath = Facade.Instance?.GlobalSettingsProxy?.ProjectRoot ?? null;

            if (projectRootPath != null)
            {
                Facade.Instance?.DatDbProxy?.SetProjectRootPath(projectRootPath);

                Facade.Instance?.AppDataProxy?.SetProjectRootPath(projectRootPath);

                Facade.Instance?.ExpressionsProxy?.SetProjectRootPath(projectRootPath);
                Facade.Instance?.ProjectProxy?.SetProjectRootPath(projectRootPath);
                Facade.Instance?.CuratedDatProxy?.SetProjectRootPath(projectRootPath);
                Facade.Instance?.ExportDatGrouperProjectProxy?.SetProjectRootPath(projectRootPath);

                Facade.Instance?.GlobalSettingsProxy?.Startup();
            }
        }
    }
}
