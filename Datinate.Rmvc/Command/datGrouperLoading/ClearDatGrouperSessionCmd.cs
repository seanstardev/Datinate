using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ClearDatGrouperSessionCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            base.DatGrouperMediator?.ResetView();
            base.DatGrouperControlsMediator?.ClearView();

            base.MediaWebMediator?.ClearView();
            base.MainWebMediator?.ClearView(true);
            base.MediaAssignmentMediator?.ClearView();
            base.MediaMediator?.TeardownView();
            base.RbWebSearchMediator?.ClearView();
            base.RadioDatModel?.Reset();
            base.DatGrouperModel?.Teardown();
            base.ProjectLoaderMediator?.ClearView();
            base.ExportMediator?.ClearView();

            if (base.DatGrouperSessionModel != null)
            {
                var layout = base.DatGrouperSessionModel.ClearSession();
                base.DatGrouperControlsMediator?.SetDatGrouperScreenLayout(layout);
            }

            return await Complete();
        }
    }
}
