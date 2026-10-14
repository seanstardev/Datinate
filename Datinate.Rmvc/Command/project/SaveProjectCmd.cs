using Datinate.Rmvc.Proxy;
using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SaveProjectCmd : CommandBase<RVoid>
    {
        private readonly DatGrouperProjectDTO projectVO;

        public SaveProjectCmd(DatGrouperProjectDTO projectVO) 
        {
            this.projectVO = projectVO;
        }

        protected override async Task<RVoid> Run()
        {
            if (base.ProjectProxy == null)
                return await Complete();

            var success =
                base.ProjectProxy.SaveProject(projectVO);

            if (base.Shell == null)
                return await Complete();

            if (success)
            {
                _ = await base.Shell.ShowMessageBox(
                        "OK",
                        "The Project '" + projectVO.ProjectName + "' has been Saved.");

                await base.Execute(new LoadDatGrouperProjectsCmd(
                    projectVO.ProjectName, false, true));
            }
            else
            {
                _ = await base.Shell.ShowMessageBox(
                        "There was a problem",
                        "The Project could not be saved.");

            }

            return await Complete();
        }
    }
}
