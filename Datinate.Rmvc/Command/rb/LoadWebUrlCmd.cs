using RMVC;

namespace Datinate.Rmvc.Command
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
