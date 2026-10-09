using RMVC;

namespace Datinate.Rmvc.Command
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
