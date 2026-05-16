namespace com.RADIO.Datinate.view.datPaths.ui
{
    public partial class DatPathUI : UserControl
    {
        public event EventHandler? removePathEvent;

        public DatPathUI()
        {
            InitializeComponent();

            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 1;
            button1.FlatAppearance.BorderColor = Color.FromArgb(188, 188, 188);
            button1.FlatAppearance.MouseOverBackColor = Color.FromArgb(246, 246, 246);
            button1.FlatAppearance.MouseDownBackColor = Color.FromArgb(238, 238, 238);
            button1.BackColor = Color.FromArgb(250, 250, 250);
            button1.ForeColor = Color.FromArgb(64, 64, 64);

            referenceTextBox.BackColor = Color.White;
            referenceTextBox.ForeColor = Color.FromArgb(33, 37, 41);

            pathLabel.ForeColor = Color.FromArgb(28, 28, 28);
        }

        public void SetUI(string path, string referenceName)
        {
            pathLabel.Text = path.Trim();
            referenceTextBox.Text = referenceName.Trim();
            Refresh();
        }

        public string GetReferenceName()
            => referenceTextBox.Text;

        public string GetPath()
            => pathLabel.Text;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using Pen borderPen = new Pen(Color.FromArgb(220, 223, 226));
            using Pen topHighlightPen = new Pen(Color.FromArgb(248, 248, 248));

            e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
            e.Graphics.DrawLine(topHighlightPen, 1, 1, Width - 2, 1);
        }

        void onRemoveClick(object sender, EventArgs e)
        {
            if (removePathEvent != null)
                removePathEvent(this, new EventArgs());
        }
    }
}