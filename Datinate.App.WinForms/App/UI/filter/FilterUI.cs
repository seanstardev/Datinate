using com.RADIO.Datinate.RMVC.Shared;

namespace datinate.app
{
    public partial class FilterUI : UserControl 
    {
        
        public event EventHandler? filterEvt;
        public event EventHandler? clearEvt;

        private Color defaultBackColour;

        public FilterUI() 
        {
            InitializeComponent();
            defaultBackColour = filterTextbox.BackColor;
        }

        public void Empty() 
        {
            filterTextbox.Text = "";
            filterTextbox.BackColor = defaultBackColour;
        }

        /**
         * Will return NULL if empty.
         */
        public string? GetFilterValue() 
        {
            if (filterTextbox.Text == "")
                return null;
            else 
                return filterTextbox.Text;
        } 

        private void onGoClick(object sender, EventArgs e) 
        {
            dispatchFilter();
        }

        protected void dispatchFilter() 
        {
            filterTextbox.BackColor = UIHelper.POP_COLOUR;

            if (filterEvt != null)
                filterEvt(this, new EventArgs());
        }

        private void onClearClick(object sender, EventArgs e) 
        {
            dispatchClear();
        }

        protected void dispatchClear() 
        {
            Empty();
            if (clearEvt != null)
                clearEvt(this, new EventArgs());
        } 

        private void onKeyDown(object sender, KeyEventArgs e) 
        {
            if (e.KeyCode == Keys.Enter) {
                if (filterTextbox.Text != "")
                    dispatchFilter();
                else 
                    dispatchClear();
            }
        }
    }
}
