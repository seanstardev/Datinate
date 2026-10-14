using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetGameEntityDragStopCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            base.MediaMediator?.StopReceiveGameEntityDrop();
            base.MainWebMediator?.StopReceiveGameEntityDrop();

            return await Complete();
        }
    }
}
