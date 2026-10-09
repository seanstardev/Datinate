using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class LoadDatRootPathsCmd : RCommand
    {
        protected override void Run()
        {
            if (Facade.Instance?.GlobalSettingsProxy is not { } proxy)
                return;

            var paths = proxy.LoadCfg();
            if (paths != null)
            {
                Facade.Instance?.LandingMediator?.LoadDatPaths(
                    paths,
                    proxy.MameSlHashPath);
            }
        }
    }
}
