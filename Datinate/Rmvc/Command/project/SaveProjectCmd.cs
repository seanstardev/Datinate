using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class SaveProjectCmd : RCommand 
    {
        private readonly DatGrouperProjectDTO projectVO;
        private readonly bool showResultsDialog;

        public SaveProjectCmd(DatGrouperProjectDTO projectVO, bool showResultsDialog) 
        {
            this.projectVO = projectVO;
            this.showResultsDialog = showResultsDialog;
        }

        protected override void Run() 
        {
            Facade? context = Facade.Instance;
            ProjectProxy? projectProxy = context?.ProjectProxy;

            if (context == null || projectProxy == null)
                return;

            projectProxy.SaveProject(projectVO);

            if (showResultsDialog)
                Facade.Instance?.Shell?.ShowMessageBox("OK", "The Project Settings have been Saved.", false);
        }
    }
}
