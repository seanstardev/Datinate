using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SetMediaItemDragStartCmd : RCommand
    {
        protected override void Run()
        {
            Facade.Instance?.MediaWebMediator?.StartReceiveMediaDrop();
            Facade.Instance?.MediaAssignmentMediator?.SetMediaCardDragStart();
        }
    }
}
