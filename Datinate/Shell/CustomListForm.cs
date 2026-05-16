using System.ComponentModel;

namespace datinate.app
{
    public partial class CustomListForm : Form 
    {
        public EventHandler? formClosingEvent;

        public CustomListForm() 
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AppClosing { get; internal set; }

        private void OnFormClosing(object? sender, FormClosingEventArgs e) 
        {
            if (AppClosing == false)
            {
                e.Cancel = true;

                if (formClosingEvent != null)
                {
                    formClosingEvent(this, new EventArgs());
                }
            }
        }
    }
}