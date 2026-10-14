using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class LoadWebMediaUrlCmd : CommandBase<RVoid>
    {
        private readonly string url;

        public LoadWebMediaUrlCmd(string url)
        {
            this.url = url;
        }

        protected override async Task<RVoid> Run()
        {
            base.MediaWebMediator?.LoadUrl(url);

            return await Complete();
        }
    }
}
