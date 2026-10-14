using Datinate.Shared;
using Datinate.Shared.Dat;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ShowAddToProjectViewCmd : CommandBase<RVoid> 
    {
        private readonly DatVO datVO;
        
        public ShowAddToProjectViewCmd(DatVO datVO) 
        {
            this.datVO = datVO;
        }

        protected override async Task<RVoid> Run() 
        {
        
            if (datVO == null) return await Complete();

            if (base.Shell is { })
            {
                if (base.Shell.CurrentProjectsPageIsProjectLoaderPage == false)
                {
                    _ = base.Shell.ShowMessageBox(
                        "Attention",
                        "Cannot proceed. Please Exit the active DAT Grouper Project and try again.");

                    await base.Execute(new SetDatGrouperFormVisibleCmd());

                    return await Complete();
                }

                if (base.ProjectLoaderMediator is { } && base.ProjectLoaderMediator.IsProjectLoaded == false)
                {
                    _ = await base.Shell.ShowMessageBox(
                        "Attention",
                        "Cannot proceed. Please Load or Create a DAT Grouper Project and try again.");

                    await base.Execute(new SetDatGrouperFormVisibleCmd());

                    return await Complete();
                }
            }

            var resources = base.ModelDataProxy?.R2DatResources;
            base.AddToProjectMediator?.SetR2DatResources(resources ?? new List<R2DatResourceDTO>());
            base.AddToProjectMediator?.SetView(datVO);
 
            base.Shell?.SetAddToProjectFormVisible(true);

            return await Complete();
        }
    }
}
