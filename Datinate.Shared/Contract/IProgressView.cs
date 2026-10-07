using RMVC;

namespace Datinate.Shared
{
    public interface IProgressView : IRContract
    {
        void ClearProgress();

        public void UpdateProgress(string message, int part, int total);
    }
}
