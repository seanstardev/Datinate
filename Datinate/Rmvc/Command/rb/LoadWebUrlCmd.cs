using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class LoadWebUrlCmd : RCommand
    {
        private readonly string url;

        public LoadWebUrlCmd(string url)
        {
            this.url = url;
        }

        protected override void Run()
        {
            Facade.Instance?.MainWebMediator?.LoadUrl(url);
        }
    }
}
