using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ClearProblemListViewCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run() 
        {
            base.ProblemListMediator?.ClearView();
            
            return await Complete();
        }
    }
}
