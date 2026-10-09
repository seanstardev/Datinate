using RMVC;

namespace Datinate.Rmvc.Command
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
