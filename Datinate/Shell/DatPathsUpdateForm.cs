using System.ComponentModel;

namespace datinate.app {
    public partial class DatPathsUpdateForm : Form 
    {
        public DatPathsUpdateForm() 
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AppClosing { get; internal set; }

        public void ShowAndBringToFront() 
        {
            if (!Visible)
                Show();

            BringToFront();
            TopMost = true;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (AppClosing == false)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
