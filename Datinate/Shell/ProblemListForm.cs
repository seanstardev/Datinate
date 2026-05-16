using System.ComponentModel;

namespace datinate.app
{
    public partial class ProblemListForm : Form 
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AppClosing { get; internal set; }

        public ProblemListForm() 
        {
            InitializeComponent();
            
            // TODO: Why needed?
            ProblemListView.Visible = true;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (AppClosing == false)
            {
                e.Cancel = true;
                this.Hide();
            }
        }


        public void ShowAndBringToFront() 
        {
            BringToFront();
            Show();
        }
    }
}
