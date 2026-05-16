using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class LoadWebMediaUrlCmd : RCommand
    {
        private readonly string url;
        public LoadWebMediaUrlCmd(string url)
        {
            this.url = url;
        }

        protected override void Run()
        {
            Facade.Instance?.MediaWebMediator?.LoadUrl(url);
        }
    }
}
