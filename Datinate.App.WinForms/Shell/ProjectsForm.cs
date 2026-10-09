using System.ComponentModel;

namespace Datinate.App.WinForms
{
    public partial class ProjectsForm : Form 
    {
        public Action? FormCloseRequest;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AppClosing { get; internal set; }

        public ProjectsForm() 
        {
            InitializeComponent();
            CenterToScreen();
        }
        public void ShowProjectsFormAndBringToFront()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(ShowProjectsFormAndBringToFront));
                return;
            }

            if (!Visible)
                Show();

            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;

            BringToFront();
            Activate();
            
            // NOTE: workaround for a race condition. Ensures this form is front
            // ... when add dat to project command completes.
            BeginInvoke(() =>
            {
                BringToFront();
                Activate();
            });
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
                
                FormCloseRequest?.Invoke();
            }
            else
                sizeBar.Dispose();
        }
        private void HandleDisposing()
        {

        }
    }
}
