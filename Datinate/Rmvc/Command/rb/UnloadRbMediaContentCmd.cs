using RMVC;

namespace com.RADIO.Datinate.RMVC
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
