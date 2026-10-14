using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SaveDatRootPathsCmd : CommandBase<RVoid>
    {
        private readonly DatRootDTO[] datRootVOs;
        private readonly string mameHashPath;

        public SaveDatRootPathsCmd(DatRootDTO[] datRootVOs, string mameHashPath)
        {
            this.datRootVOs = datRootVOs;
            this.mameHashPath = mameHashPath;
        }
        protected override async Task<RVoid> Run()
        {
            if (base.GlobalSettingsProxy == null)
                return await Complete();

            var success = base.GlobalSettingsProxy.SaveDatRootPathsCfg(datRootVOs, mameHashPath);

            if (success)
            {
                base.Shell?.ShowMessageBox(
                    "OK", "The settings have been saved.");
            }
            else
            {
                base.Shell?.ShowMessageBox(
                    "There was a problem", "The settings could not be saved.");
            }

            return await Complete();
        }
    }
}
