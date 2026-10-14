using Datinate.App.WinForms.View.Util;

namespace Datinate.App.WinForms.View
{
    public partial class ProjectsView : UserControl
    {
        private readonly UiDispatcher uiDispatcher;

        public ProjectsView()
        {
            uiDispatcher = new UiDispatcher(this);
            InitializeComponent();
            FormsHelper.HideTabs(tabControl);
        }

        public bool CurrentProjectsPageIsProjectLoaderPage
            => uiDispatcher.Read(() => tabControl.SelectedIndex == 0);

        public void ShowProjectsView()
            => uiDispatcher.Run(() => tabControl.SelectedIndex = 0);

        public void ShowExportView()
            => uiDispatcher.Run(() => tabControl.SelectedIndex = 3);

        public void ShowCurationView()
            => uiDispatcher.Run(() => tabControl.SelectedIndex = 1);
    }
}
