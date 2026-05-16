using RMVC;
using System.Security.Policy;

namespace com.RADIO.Datinate.RMVC
{
    public class LoadWebMediaHtmlCmd : RCommand
    {
        private readonly string html;

        public LoadWebMediaHtmlCmd(string html)
        {
            this.html = html;
        }

        protected override void Run()
        {
            Facade.Instance?.MediaWebMediator?.LoadPageContent(html);
        }
    }
}
