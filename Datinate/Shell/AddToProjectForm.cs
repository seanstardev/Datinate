using System.ComponentModel;

namespace datinate.app 
{
    public partial class AddToProjectForm : Form 
    {
        public AddToProjectForm() 
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AppClosing { get; internal set; }

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
