using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ClearProgressCmd : CommandBase<RVoid> 
    {
        protected override async Task<RVoid> Run() 
        {
            base.Shell?.SetProgressFormVisible(false);
            base.ProgressMediator?.ClearProgress();

            return await Complete();
        }
    }
}
