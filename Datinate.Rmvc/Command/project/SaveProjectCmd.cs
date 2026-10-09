using Datinate.Rmvc.Proxy;
using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SaveProjectCmd : RCommandAsync
    {
        private readonly DatGrouperProjectDTO projectVO;

        public SaveProjectCmd(DatGrouperProjectDTO projectVO) 
        {
            this.projectVO = projectVO;
        }

        protected override async Task RunAsync()
        {
            Facade? context = Facade.Instance;
            ProjectProxy? projectProxy = context?.ProjectProxy;

            if (context == null || projectProxy == null)
                return;

            var success =
                projectProxy.SaveProject(projectVO);


            //            MessageBox.Show(
            //    "The Project '" + projectVO.ProjectName + "' has been Saved."
            //    , "OK"
            //    , MessageBoxButtons.OK
            //    , MessageBoxIcon.Information
            //);

            if (Facade.Instance?.Shell == null)
                return;

            if (success)
            {
                await Facade.Instance.Shell.ShowMessageBox(
                    "OK",
                    "The Project '" + projectVO.ProjectName + "' has been Saved.");

                base.ExecuteCommand(new LoadDatGrouperProjectsCmd(
                    projectVO.ProjectName, false, true));
            }
            else
            {
                await Facade.Instance.Shell.ShowMessageBox(
                    "There was a problem",
                    "The Project could not be saved.");

            }
        }
    }
}
