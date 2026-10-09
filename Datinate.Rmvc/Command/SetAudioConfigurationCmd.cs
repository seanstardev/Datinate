using RMVC;

namespace Datinate.Rmvc.Command
{
    public class SetAudioConfigurationCmd : RCommand
    {
        protected override void Run()
        {
            var audioEnvironmentPath = Facade.Instance?.AudioProxy?.AudioEnvironmentPath;

            Facade.Instance?.MediaWebMediator?.SetAudioEnvironmentPath(audioEnvironmentPath);
            Facade.Instance?.MediaMediator?.SetAudioEnvironmentPath(audioEnvironmentPath);
        }
    }
}
