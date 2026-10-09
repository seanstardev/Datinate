using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class EnterMediaModeCmd : RCommand
    {
        private readonly bool enterPreviewMode;
        private readonly bool sourceIsAuto;

        public EnterMediaModeCmd(bool enterPreviewMode, bool sourceIsAuto)
        {
            this.enterPreviewMode = enterPreviewMode;
            this.sourceIsAuto = sourceIsAuto;
        }

        protected override void Run()
        {
            if (Facade.Instance?.DatGrouperSessionModel != null)
            {
                Facade.Instance.MediaAssignmentMediator?.ClearView();
                Facade.Instance.MediaWebMediator?.ClearView();
                Facade.Instance.MediaMediator?.EmptyView();

                var layout = Facade.Instance.DatGrouperSessionModel.EnterMediaMode(sourceIsAuto, enterPreviewMode);
             
                Facade.Instance.MediaAssignmentMediator?.SetReadOnlyModeActive(enterPreviewMode);
                Facade.Instance.DatGrouperMediator?.SetScreenLayout(layout);
                Facade.Instance.DatGrouperControlsMediator?.SetDatGrouperScreenLayout(layout);
            }
        }
    }
}
