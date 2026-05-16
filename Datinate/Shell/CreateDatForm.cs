using System.ComponentModel;

namespace datinate.app
{
    public partial class CreateDatForm : Form 
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AppClosing { get; internal set; }

        public CreateDatForm() 
        {
            InitializeComponent();
        }

        private void onFormClosing(object sender, FormClosingEventArgs e) 
        {
            if (AppClosing == false)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
        public void ShowAndBringToFront()
        {
            if (!Visible)
                Show();

            BringToFront();
            TopMost = true;
        }
    }
}
