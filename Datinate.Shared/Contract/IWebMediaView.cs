using RMVC;

namespace Datinate.Shared.Rmvc
{
    public interface IWebMediaView : IRContract
    {
        void LoadUrl(string url);
        void LoadPageContent(string html);
        void ClearView();

        void StartReceiveMediaDrop();

        void StopReceiveMediaDrop();
        void LoadUriInBrowser();
        void SetAudioEnvironmentPath(string? audioEnvironmentPath);
    }
}
