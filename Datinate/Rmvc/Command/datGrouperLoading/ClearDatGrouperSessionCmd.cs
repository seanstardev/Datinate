using com.RADIO.Datinate;
using RMVC;

namespace Datinate.Rmvc.Command
{
    public class ClearDatGrouperSessionCmd : RCommand
    {
        protected override void Run()
        {
            Facade.Instance?.DatGrouperMediator?.ResetView();
            Facade.Instance?.DatGrouperControlsMediator?.ClearView();

            Facade.Instance?.MediaWebMediator?.ClearView();
            Facade.Instance?.MainWebMediator?.ClearView(true);

            Facade.Instance?.MediaAssignmentMediator?.ClearView();
            Facade.Instance?.MediaMediator?.TeardownView();

            Facade.Instance?.RbWebSearchMediator?.ClearView();

            Facade.Instance?.RadioDatModel?.Reset();

            Facade.Instance?.DatGrouperModel?.Teardown();

            Facade.Instance?.ProjectLoaderMediator?.ClearView();
            Facade.Instance?.ContentPathsMediator?.ClearView();


            var appSession = Facade.Instance?.DatGrouperSessionModel;

            if (appSession != null)
            {
                var layout = appSession.ClearSession();
                Facade.Instance?.DatGrouperControlsMediator?.SetDatGrouperScreenLayout(layout);
            }
        }
    }
}
