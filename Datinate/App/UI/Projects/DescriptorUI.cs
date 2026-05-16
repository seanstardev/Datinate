namespace datinate.app 
{
    public partial class DescriptorUI : UserControl 
    {

        public string DescriptorName { get { return descriptorCheck.Text; } }
        public bool Checked { get { return descriptorCheck.Checked; } }
        public bool DoesExclude { get { return excludeCheck.Checked; } }

        private Color defaultBackgroundColour;

        public DescriptorUI(string descriptorName, bool selected, bool doesExclude) 
        {
            InitializeComponent();

            defaultBackgroundColour = base.BackColor;

            descriptorCheck.Text = descriptorName;
            descriptorCheck.Checked = selected;
            excludeCheck.Checked = doesExclude;
            UpdateUI();
        }

        private void descriptorCheck_CheckedChanged(object sender, EventArgs e) 
        {
            UpdateUI();
        }

        private void excludeCheck_CheckedChanged(object sender, EventArgs e) 
        {
            UpdateUI();
        }

        private void UpdateUI() 
        {
            excludeCheck.Enabled = Checked;
            base.BackColor = !Checked ? Color.LightGray : defaultBackgroundColour;
        }
    }
}
