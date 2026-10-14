using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetAudioConfigurationCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            var audioEnvironmentPath = base.AudioProxy?.AudioEnvironmentPath;

            base.MediaWebMediator?.SetAudioEnvironmentPath(audioEnvironmentPath);
            base.MediaMediator?.SetAudioEnvironmentPath(audioEnvironmentPath);

            return await Complete();
        }
    }
}
