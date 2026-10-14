using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetMediaItemDragStopCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            base.MediaWebMediator?.StopReceiveMediaDrop();
            base.MediaAssignmentMediator?.SetMediaCardDragStop();

            return await Complete();
        }
    }
}
