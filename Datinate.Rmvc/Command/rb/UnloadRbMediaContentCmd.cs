using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class UnloadRbMediaContentCmd : CommandBase<RVoid>
    {
        private readonly bool doNotUnloadRemoteContent;

        public UnloadRbMediaContentCmd(bool doNotUnloadRemoteContent)
        {
            this.doNotUnloadRemoteContent = doNotUnloadRemoteContent;
        }

        protected override async Task<RVoid> Run()
        {
            base.MainWebMediator?.UnloadPageContent(doNotUnloadRemoteContent);

            return await Complete();
        }
    }
}
