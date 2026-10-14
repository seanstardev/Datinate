using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    public class MediaWebMediator : RMediator<IWebMediaView>
    {
        public void LoadUrl(string url)
            => actor?.LoadUrl(url);
        
        public void LoadPageContent(string html)
            => actor?.LoadPageContent(html);
        
        public void ClearView()
            => actor?.ClearView();

        public void StartReceiveMediaDrop()
            => actor?.StartReceiveMediaDrop();

        public void StopReceiveMediaDrop()
            => actor?.StopReceiveMediaDrop();

        public void LoadUriInBrowser()
            => actor?.LoadUriInBrowser();

        public void SetAudioEnvironmentPath(string? audioEnvironmentPath) 
            => actor?.SetAudioEnvironmentPath(audioEnvironmentPath);

        protected override void Initialised()
        {

        }

        protected override void Disposing()
        {

        }
    }
}
