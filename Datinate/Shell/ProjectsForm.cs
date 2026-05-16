using System.ComponentModel;

namespace datinate.app
{
    public partial class ProjectsForm : Form 
    {
        public Action? FormHiddenEvt;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AppClosing { get; internal set; }

        public ProjectsForm() 
        {
            InitializeComponent();
            CenterToScreen();
        }
        public void ShowProjectsFormAndBringToFront()
        {
            if (!Visible)
                Show();

            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;

            BringToFront();
            Activate();
        }

        public void SetProjectTitle(string title)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetProjectTitle(title)));
                return;
            }
            base.Text = title;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (AppClosing == false)
            {
                e.Cancel = true;
                Hide();
                if (FormHiddenEvt != null)
                    FormHiddenEvt.Invoke();
            }
            else
                sizeBar.Dispose();
        }
        private void HandleDisposing()
        {

        }
    }
}
