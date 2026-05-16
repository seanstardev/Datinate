using com.RADIO.Datinate.RMVC.Shared;
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
                MessageBox.Show("The settings have been saved.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("The settings could not be saved.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
