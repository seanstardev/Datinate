using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ProjectDatFullpathsChangedCmd : CommandBase<RVoid>
    {
        private readonly DatGrouperProjectDTO projectVO;

        public ProjectDatFullpathsChangedCmd(DatGrouperProjectDTO projectVO) 
        {
            this.projectVO = projectVO;
        }

        protected override async Task<RVoid> Run() 
        {
            base.DatPathsUpdateMediator?.SetView(projectVO);
            base.Shell?.SetDatPathsUpdateFormVisible(!DatGrouperProjectDTO.GetAllDatFilesExist(projectVO));

            return await Complete();
        }
    }
}
