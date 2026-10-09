using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class SaveDatRootPathsCmd : RCommand
    {
        private readonly DatRootDTO[] datRootVOs;
        private readonly string mameHashPath;

        public SaveDatRootPathsCmd(DatRootDTO[] datRootVOs, string mameHashPath)
        {
            this.datRootVOs = datRootVOs;
            this.mameHashPath = mameHashPath;
        }
        protected override void Run()
        {
            if (Facade.Instance?.GlobalSettingsProxy == null) return;

            var success = Facade.Instance.GlobalSettingsProxy.SaveDatRootPathsCfg(datRootVOs, mameHashPath);

            if (success)
            {
                Facade.Instance?.Shell?.ShowMessageBox(
                    "OK", "The settings have been saved.");
            }
            else
            {
                Facade.Instance?.Shell?.ShowMessageBox(
                    "There was a problem", "The settings could not be saved.");
            }
        }
    }
}
