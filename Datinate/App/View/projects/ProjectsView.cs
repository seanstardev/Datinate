using datinate.app;
namespace com.RADIO.Datinate.App.View.projects 
{
    public partial class ProjectsView : UserControl 
    {
        public ProjectsView() 
        {
            InitializeComponent();
            FormsHelper.HideTabs(tabControl);
        }

        public bool CurrentProjectsPageIsProjectLoaderPage 
            => tabControl.SelectedIndex == 0;

        public void ShowProjectsView()
        {
            if (InvokeRequired)
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(() => ShowProjectsView()));
                }
                return;
            }
            tabControl.SelectedIndex = 0;
        }
        public void ShowExportView()
        {
            if (InvokeRequired)
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(() => ShowExportView()));
                }
                return;
            }
            tabControl.SelectedIndex = 3;
        }
        public void ShowCurationView()
        {
            if (InvokeRequired)
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(() => ShowCurationView()));
                }
                return;
            }
            tabControl.SelectedIndex = 1;
        }
        public void ShowRbContentPathsView()
        {
            if (InvokeRequired)
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(() => ShowRbContentPathsView()));
                }
                return;
            }
            tabControl.SelectedIndex = 2;
        }
    }
}
