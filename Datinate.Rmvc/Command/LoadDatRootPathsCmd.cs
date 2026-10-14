using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class LoadDatRootPathsCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            if (base.GlobalSettingsProxy is not { })
                return await Complete();

            var paths = base.GlobalSettingsProxy.LoadCfg();

            if (paths != null)
            {
                base.LandingMediator?.LoadDatPaths(
                    paths,
                    base.GlobalSettingsProxy.MameSlHashPath);
            }

            return await Complete();
        }
    }
}
