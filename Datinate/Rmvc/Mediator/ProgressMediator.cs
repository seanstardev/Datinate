using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class ProgressMediator : RMediator
    {
        private IProgressView? view => (IProgressView?)base.viewBase;
        public ProgressMediator(Type view) : base(view)
        {
        }

        public void ClearProgress()
        {
            view?.ClearProgress();
        }
        public void UpdateProgress(string message, int part, int total)
        {
            view?.UpdateProgress(message, part, total);
        }

        protected override void Disposing()
        {

        }

        protected override void Initialsed()
        {

        }
    }
}
