using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetMediaItemDragStartCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            // NOTE: Order is critical:
            base.MediaAssignmentMediator?.SetMediaCardDragStart();
            base.MediaWebMediator?.StartReceiveMediaDrop();

            return await Complete();
        }
    }
}
