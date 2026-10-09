using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ProjectDatFullpathsChangedCmd : RCommand 
    {
        private readonly DatGrouperProjectDTO projectVO;

        public ProjectDatFullpathsChangedCmd(DatGrouperProjectDTO projectVO) 
        {
            this.projectVO = projectVO;
        }

        protected override void Run() 
        {
            Facade.Instance?.DatPathsUpdateMediator?.SetView(projectVO);
            Facade.Instance?.Shell?.SetDatPathsUpdateFormVisible(!DatGrouperProjectDTO.GetAllDatFilesExist(projectVO));
        }
    }
}
