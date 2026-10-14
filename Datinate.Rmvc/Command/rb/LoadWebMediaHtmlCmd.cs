using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class LoadWebMediaHtmlCmd : CommandBase<RVoid>
    {
        private readonly string html;

        public LoadWebMediaHtmlCmd(string html)
        {
            this.html = html;
        }

        protected override async Task<RVoid> Run()
        {
            base.MediaWebMediator?.LoadPageContent(html);

            return await Complete();
        }
    }
}
