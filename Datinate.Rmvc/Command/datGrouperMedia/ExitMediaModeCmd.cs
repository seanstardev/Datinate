using RMVC;

namespace Datinate.Rmvc.Command
{
    public class ExitMediaModeCmd : RCommand
    {
        protected override void Run()
        {
            Facade.Instance?.MediaMediator?.EmptyView();

            if (Facade.Instance?.DatGrouperSessionModel != null)
            {
                var layout = Facade.Instance.DatGrouperSessionModel.ExitMediaMode();
                Facade.Instance?.DatGrouperMediator?.SetScreenLayout(layout);
                Facade.Instance?.DatGrouperControlsMediator?.SetDatGrouperScreenLayout(layout);

                Facade.Instance?.MediaAssignmentMediator?.ClearView();
                Facade.Instance?.MediaWebMediator?.ClearView();
                Facade.Instance?.MediaMediator?.EmptyView();
            }
        }
    }
}
