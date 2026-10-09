using RMVC;

namespace Datinate.Rmvc.Command
{
    public class UnloadRbMediaContentCmd : RCommand
    {
        private readonly bool doNotUnloadRemoteContent;

        public UnloadRbMediaContentCmd(bool doNotUnloadRemoteContent)
        {
            this.doNotUnloadRemoteContent = doNotUnloadRemoteContent;
        }

        protected override void Run()
        {
            Facade.Instance?.MainWebMediator?.UnloadPageContent(doNotUnloadRemoteContent);
        }
    }
}
