using datinate.app;

namespace Datinate.App.WinForms.View
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
         

        //public bool CurrentProjectsPageIsProjectLoaderPage
        //{
        //    get
        //    {
        //        if (tabControl.InvokeRequired)
        //            return (bool)tabControl.Invoke(
        //                new Func<bool>(() => tabControl.SelectedIndex == 0));

        //        return tabControl.SelectedIndex == 0;
        //    }
        //}

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
    }
}
