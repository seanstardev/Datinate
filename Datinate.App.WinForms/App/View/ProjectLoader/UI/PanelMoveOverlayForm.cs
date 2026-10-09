namespace datinate.app
{

    public sealed class PanelMoveOverlayForm : Form
    {
        private const int WM_NCHITTEST = 0x0084;
        private const int HTTRANSPARENT = -1;

        private readonly Bitmap bmpBase;
        private readonly Bitmap[] bmps;
        private readonly Rectangle[] rects;

        public PanelMoveOverlayForm(Rectangle screenBounds, Bitmap bmpBase, Bitmap[] bmps, Rectangle[] rects)
        {
            this.bmpBase = bmpBase;
            this.bmps = bmps;
            this.rects = rects;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Bounds = screenBounds;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            UpdateStyles();
        }

        public void ShowNoActivate() => Show();

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TOOLWINDOW = 0x00000080;
                const int WS_EX_NOACTIVATE = 0x08000000;

                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTTRANSPARENT;
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(bmpBase, 0, 0);

            for (int i = 0; i < bmps.Length; i++)
                e.Graphics.DrawImage(bmps[i], rects[i]);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { bmpBase.Dispose(); } catch { }

                for (int i = 0; i < bmps.Length; i++)
                {
                    try { bmps[i].Dispose(); } catch { }
                }
            }
            base.Dispose(disposing);
        }
    }
}
