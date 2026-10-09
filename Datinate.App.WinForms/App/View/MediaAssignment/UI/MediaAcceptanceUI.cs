using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace datinate.app
{
    public sealed class MediaAcceptanceUI : Control
    {
        private int acceptedCount;
        private string noMatchText = "0";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int AcceptedCount
        {
            get => acceptedCount;
            set
            {
                int v = value < 0 ? 0 : value;
                if (acceptedCount == v)
                    return;

                acceptedCount = v;
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string NoMatchText
        {
            get => noMatchText;
            set
            {
                string v = string.IsNullOrWhiteSpace(value) ? "0" : value.Trim();
                if (noMatchText == v)
                    return;

                noMatchText = v;
                Invalidate();
            }
        }

        public MediaAcceptanceUI()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            Size = new Size(50, 50);
            MinimumSize = new Size(12, 12);
            TabStop = false;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (Parent != null && BackColor == Color.Transparent)
            {
                var s = pevent.Graphics.Save();
                pevent.Graphics.TranslateTransform(-Left, -Top);
                var r = new Rectangle(Left, Top, Width, Height);
                using var e = new PaintEventArgs(pevent.Graphics, r);
                InvokePaintBackground(Parent, e);
                InvokePaint(Parent, e);
                pevent.Graphics.Restore(s);
                return;
            }

            base.OnPaintBackground(pevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            Rectangle r = ClientRectangle;
            if (r.Width <= 1 || r.Height <= 1)
                return;

            int minDim = Math.Min(r.Width, r.Height);
            int pad = Math.Max(1, (int)Math.Round(minDim * 0.04f));
            r = Rectangle.Inflate(r, -pad, -pad);
            if (r.Width <= 2 || r.Height <= 2)
                return;

            bool hasMatch = acceptedCount > 0;
            string text = hasMatch ? "+" + acceptedCount.ToString() : noMatchText;

            Color baseTop;
            Color baseBottom;
            Color sheenTop;
            Color sheenBottom;
            Color softShadowTop;
            Color softShadowBottom;

            if (hasMatch)
            {
                baseTop = Color.FromArgb(88, 214, 72);
                baseBottom = Color.FromArgb(60, 191, 49);
                sheenTop = Color.FromArgb(70, 255, 255, 255);
                sheenBottom = Color.FromArgb(0, 255, 255, 255);
                softShadowTop = Color.FromArgb(0, 0, 0, 0);
                softShadowBottom = Color.FromArgb(35, 0, 0, 0);
            }
            else
            {
                baseTop = Color.FromArgb(230, 73, 73);
                baseBottom = Color.FromArgb(205, 41, 41);
                sheenTop = Color.FromArgb(58, 255, 255, 255);
                sheenBottom = Color.FromArgb(0, 255, 255, 255);
                softShadowTop = Color.FromArgb(0, 0, 0, 0);
                softShadowBottom = Color.FromArgb(42, 0, 0, 0);
            }

            int radius = Math.Max(4, (int)Math.Round(Math.Min(r.Width, r.Height) * 0.18f));

            using (var path = CreateRoundedRectPath(r, radius))
            {
                using (var b = new LinearGradientBrush(r, baseTop, baseBottom, LinearGradientMode.Vertical))
                    g.FillPath(b, path);

                int sheenH = Math.Max(2, (int)Math.Round(r.Height * 0.34f));
                var sheenRect = new Rectangle(r.X, r.Y, r.Width, sheenH);
                var gs1 = g.Save();
                g.SetClip(path);
                using (var bSheen = new LinearGradientBrush(sheenRect, sheenTop, sheenBottom, LinearGradientMode.Vertical))
                    g.FillRectangle(bSheen, sheenRect);
                g.Restore(gs1);

                int shadeY = r.Y + (int)Math.Round(r.Height * 0.56f);
                if (shadeY < r.Bottom)
                {
                    var shadeRect = new Rectangle(r.X, shadeY, r.Width, r.Bottom - shadeY);
                    var gs2 = g.Save();
                    g.SetClip(path);
                    using (var bShade = new LinearGradientBrush(shadeRect, softShadowTop, softShadowBottom, LinearGradientMode.Vertical))
                        g.FillRectangle(bShade, shadeRect);
                    g.Restore(gs2);
                }
            }

            DrawScaledText(g, r, text);
        }

        private void DrawScaledText(Graphics g, Rectangle r, string text)
        {
            var flags =
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine |
                TextFormatFlags.NoPadding |
                TextFormatFlags.PreserveGraphicsClipping;

            int minDim = Math.Min(r.Width, r.Height);
            int innerPadX = Math.Max(2, (int)Math.Round(minDim * 0.10f));
            int innerPadY = Math.Max(1, (int)Math.Round(minDim * 0.06f));
            Rectangle textRect = Rectangle.Inflate(r, -innerPadX, -innerPadY);

            float maxPt = Math.Max(7f, Math.Min(r.Width, r.Height) * (text.Length >= 3 ? 0.54f : 0.60f));
            float minPt = 6f;

            using var font = CreateFittedFont(g, text, textRect, maxPt, minPt, FontStyle.Bold, flags);

            int shadowDy = Math.Max(1, (int)Math.Round(minDim * 0.03f));
            var shadowRect = new Rectangle(textRect.X, textRect.Y + shadowDy, textRect.Width, textRect.Height);

            TextRenderer.DrawText(g, text, font, shadowRect, Color.FromArgb(70, 0, 0, 0), flags);
            TextRenderer.DrawText(g, text, font, textRect, Color.White, flags);
        }

        private Font CreateFittedFont(Graphics g, string text, Rectangle bounds, float maxPt, float minPt, FontStyle style, TextFormatFlags flags)
        {
            var family = Font?.FontFamily ?? SystemFonts.MessageBoxFont.FontFamily;

            for (float pt = maxPt; pt >= minPt; pt -= 0.5f)
            {
                var f = new Font(family, pt, style, GraphicsUnit.Point);
                Size measured = TextRenderer.MeasureText(g, text, f, new Size(int.MaxValue, int.MaxValue), flags);
                if (measured.Width <= bounds.Width && measured.Height <= bounds.Height)
                    return f;

                f.Dispose();
            }

            return new Font(family, minPt, style, GraphicsUnit.Point);
        }

        private static GraphicsPath CreateRoundedRectPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();

            if (d <= 0)
            {
                p.AddRectangle(r);
                p.CloseFigure();
                return p;
            }

            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;

            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();

            return p;
        }
    }
}