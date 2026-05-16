using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SetGameEntityDragStopCmd : RCommand
    {
        protected override void Run()
        {
            Facade.Instance?.MediaMediator?.StopReceiveGameEntityDrop();
            Facade.Instance?.MainWebMediator?.StopReceiveGameEntityDrop();
        }
    }
}
