using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ExitMediaModeCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            base.MediaMediator?.EmptyView();

            if (base.DatGrouperSessionModel != null)
            {
                var layout = base.DatGrouperSessionModel.ExitMediaMode();

                base.DatGrouperMediator?.SetScreenLayout(layout);
                base.DatGrouperControlsMediator?.SetDatGrouperScreenLayout(layout);
                
                base.MediaAssignmentMediator?.ClearView();
                base.MediaWebMediator?.ClearView();
                base.MediaMediator?.EmptyView();
            }

            return await Complete();
        }
    }
}
