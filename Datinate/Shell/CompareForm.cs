using System.ComponentModel;

namespace datinate.app 
{
    public partial class CompareForm : Form 
    {
        public Action formClosingEvt;


        public CompareForm() 
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AppClosing { get; internal set; }

        public void ShowAndBringToFront() 
        {
            Show();
            BringToFront();
        }

        private void onFormClosing(object sender, FormClosingEventArgs e) 
        {
            if (AppClosing == false)
            {
                e.Cancel = true;
                this.Hide();

                if (formClosingEvt != null)
                    formClosingEvt.Invoke();
            }
        }
    }
}
