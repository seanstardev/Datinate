using datinate.app;
using System.ComponentModel;

namespace Datinate.App
{
    public partial class ProgressForm : Form
    {
        public ProgressView ProgressView => progressView;

        protected override bool ShowWithoutActivation => true;
        
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int HTCAPTION = 0x0002;
        private const int SC_MOVE = 0xF010;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AppClosing { get; internal set; }

        public ProgressForm()
        {
            InitializeComponent();

            ShowInTaskbar = false;
            ControlBox = false;
            MaximumSize = MinimumSize = Size;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
                BeginInvoke(new Action(CenterOnTargetBounds));
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            BeginInvoke(new Action(CenterOnTargetBounds));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CenterOnTargetBounds();
        }

        private void CenterOnTargetBounds()
        {
            Rectangle area = GetTargetBounds();

            Location = new Point(
                area.Left + ((area.Width - Width) / 2),
                area.Top + ((area.Height - Height) / 2));
        }

        private Rectangle GetTargetBounds()
        {
            if (Owner != null && !Owner.IsDisposed && Owner.Visible)
                return Owner.Bounds;

            Form? activeForm = Form.ActiveForm;
            if (activeForm != null && !activeForm.IsDisposed && activeForm.Visible)
                return activeForm.Bounds;

            return Screen.FromPoint(Cursor.Position).WorkingArea;
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCLBUTTONDOWN && (int)m.WParam == HTCAPTION)
                return;

            if (m.Msg == WM_SYSCOMMAND && (((int)m.WParam) & 0xFFF0) == SC_MOVE)
                return;

            base.WndProc(ref m);
        }

        private void CenterOnTargetScreen()
        {
            Screen screen = GetTargetScreen();
            Rectangle area = screen.WorkingArea;

            Location = new Point(
                area.Left + ((area.Width - Width) / 2),
                area.Top + ((area.Height - Height) / 2));
        }

        private Screen GetTargetScreen()
        {
            if (Owner != null && !Owner.IsDisposed)
                return Screen.FromControl(Owner);

            Form? activeForm = Form.ActiveForm;
            if (activeForm != null && !activeForm.IsDisposed)
                return Screen.FromControl(activeForm);

            return Screen.FromPoint(Cursor.Position);
        }
    }
}