using System.ComponentModel;
using System.Runtime.InteropServices;

namespace datinate.app
{
    public sealed class BorderlessTabControl : TabControl
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private Color topEdgeCoverColor = Color.Empty;
        private int topEdgeCoverHeight = 10;

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "")]
        public Color TopEdgeCoverColor
        {
            get => topEdgeCoverColor;
            set
            {
                if (topEdgeCoverColor == value)
                    return;

                topEdgeCoverColor = value;
                if (IsHandleCreated) Invalidate();
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int TCM_ADJUSTRECT = 0x1328;
            const int WM_PAINT = 0x000F;
            const int WM_NCPAINT = 0x0085;

            if (m.Msg == TCM_ADJUSTRECT && m.LParam != IntPtr.Zero)
            {
                var rc = Marshal.PtrToStructure<RECT>(m.LParam);

                rc.Left -= 4;
                rc.Top -= 4;
                rc.Right += 4;
                rc.Bottom += 4;

                Marshal.StructureToPtr(rc, m.LParam, true);
            }

            base.WndProc(ref m);

            if (m.Msg == WM_PAINT || m.Msg == WM_NCPAINT)
                PaintTopEdgeCover();
        }

        private void PaintTopEdgeCover()
        {
            if (!IsHandleCreated)
                return;

            if (topEdgeCoverHeight <= 0)
                return;

            if (topEdgeCoverColor.IsEmpty)
                return;

            int h = topEdgeCoverHeight;
            if (h > Height) h = Height;

            using var g = Graphics.FromHwnd(Handle);
            using var b = new SolidBrush(topEdgeCoverColor);
            g.FillRectangle(b, 0, 0, Width, h);
        }
    }
}
