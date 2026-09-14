using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SetMediaItemDragStartCmd : RCommand
    {
        protected override void Run()
        {
            // NOTE: Order is critical:
            Facade.Instance?.MediaAssignmentMediator?.SetMediaCardDragStart();
            Facade.Instance?.MediaWebMediator?.StartReceiveMediaDrop();
        }
    }
}
