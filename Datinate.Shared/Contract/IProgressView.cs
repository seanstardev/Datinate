using RMVC;

namespace Datinate.Shared.Rmvc
{
    public interface IProgressView : IRContract
    {
        void ClearProgress();

        public void UpdateProgress(string message, int part, int total);
    }
}
