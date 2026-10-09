using Datinate.Shared;
using Datinate.Shared.Dat;
using RMVC;

namespace Datinate.Rmvc.Command
{
    public class ShowAddToProjectViewCmd : RCommand 
    {
        private readonly DatVO datVO;
        
        public ShowAddToProjectViewCmd(DatVO datVO) 
        {
            this.datVO = datVO;
        }

        protected override void Run() 
        {
        
            if (datVO == null) return;

            if (Facade.Instance?.Shell is { } shell)
            {
                if (shell.CurrentProjectsPageIsProjectLoaderPage == false)
                {
                    _ = shell.ShowMessageBox(
                        "Attention",
                        "Cannot proceed. Please Exit the active DAT Grouper Project and try again.");

                    base.ExecuteCommand(new SetDatGrouperFormVisibleCmd());
                    return;
                }

                if (Facade.Instance?.ProjectLoaderMediator is { } projects && projects.IsProjectLoaded == false)
                {
                    _ = shell.ShowMessageBox(
                        "Attention",
                        "Cannot proceed. Please Load or Create a DAT Grouper Project and try again.");

                    base.ExecuteCommand(new SetDatGrouperFormVisibleCmd());
                    return;
                }
            }

            var resources = Facade.Instance?.ModelDataProxy?.R2DatResources;
            Facade.Instance?.AddToProjectMediator?.SetR2DatResources(resources ?? new List<R2DatResourceDTO>());
            Facade.Instance?.AddToProjectMediator?.SetView(datVO);
            
            Facade.Instance?.Shell?.SetAddToProjectFormVisible(true);
        }
    }
}
