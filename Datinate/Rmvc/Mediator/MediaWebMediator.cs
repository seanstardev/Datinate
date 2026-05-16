using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class MediaWebMediator : RMediator
    {
        private IWebMediaView? view => (IWebMediaView?)base.viewBase;
        public MediaWebMediator(Type view) : base(view)
        {
        }
        public void LoadUrl(string url)
        {
            view?.LoadUrl(url);
        }
        public void LoadPageContent(string html)
        {
            view?.LoadPageContent(html);
        }
        public void ClearView()
        {
            view?.ClearView();
        }

        protected override void Disposing()
        {

        }

        protected override void Initialsed()
        {

        }

        public void StartReceiveMediaDrop()
        {
            view?.StartReceiveMediaDrop();
        }

        public void StopReceiveMediaDrop()
        {
            view?.StopReceiveMediaDrop();
        }

        public void LoadUriInBrowser()
            => view?.LoadUriInBrowser();
    }
}
