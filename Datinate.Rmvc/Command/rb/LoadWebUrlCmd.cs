using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class LoadWebUrlCmd : CommandBase<RVoid>
    {
        private readonly string url;

        public LoadWebUrlCmd(string url)
        {
            this.url = url;
        }

        protected override async Task<RVoid> Run()
        {
            base.MainWebMediator?.LoadUrl(url);

            return await Complete();
        }
    }
}
