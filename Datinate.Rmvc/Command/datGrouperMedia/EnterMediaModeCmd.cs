using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class EnterMediaModeCmd : CommandBase<RVoid>
    {
        private readonly bool enterPreviewMode;
        private readonly bool sourceIsAuto;

        public EnterMediaModeCmd(bool enterPreviewMode, bool sourceIsAuto)
        {
            this.enterPreviewMode = enterPreviewMode;
            this.sourceIsAuto = sourceIsAuto;
        }

        protected override async Task<RVoid> Run()
        {
            if (base.DatGrouperSessionModel != null)
            {
                base.MediaAssignmentMediator?.ClearView();
                base.MediaWebMediator?.ClearView();
                base.MediaMediator?.EmptyView();

                var layout = base.DatGrouperSessionModel.EnterMediaMode(sourceIsAuto, enterPreviewMode);
             
                base.MediaAssignmentMediator?.SetReadOnlyModeActive(enterPreviewMode);
                base.DatGrouperMediator?.SetScreenLayout(layout);
                base.DatGrouperControlsMediator?.SetDatGrouperScreenLayout(layout);
            }

            return await Complete();
        }
    }
}
