using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    public class ProgressMediator : RMediator<IProgressView>
    {
        public void ClearProgress()
            => actor?.ClearProgress();
        
        public void UpdateProgress(string message, int part, int total)
            => actor?.UpdateProgress(message, part, total);
        
        protected override void Initialised()
        {

        }

        protected override void Disposing()
        {

        }
    }
}
