using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
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
            Facade.Instance?.Shell?.SetDatPathsUpdateFormVisible(!DatGrouperProjectDTO.GetAllDatPathFilesExist(projectVO));
        }
    }
}
