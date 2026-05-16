using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SetMediaItemDragStopCmd : RCommand
    {
        protected override void Run()
        {
            Facade.Instance?.MediaWebMediator?.StopReceiveMediaDrop();

            Facade.Instance?.MediaAssignmentMediator?.SetMediaCardDragStop();
        }
    }
}
