using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    [ToolboxItem(true)]
    [DesignerCategory("Code")]
    [DefaultProperty(nameof(PercentFull))]
    public sealed class MatchBarsUI : Control
    {
        private const int STAR_SIZE_PERCENT = 49;

        private int _percentFull;
        private int _percentMinWarning = 25;

        private Color _barEnabledColour = Color.Black;
        private Color _barWarningColour = Color.Black;

        public MatchBarsUI()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            Size = new Size(28, 22);
        }

        [Category("Appearance")]
        [Description("0..100. 50 means two bars are lit.")]
        [DefaultValue(0)]
        public int PercentFull
        {
            get => _percentFull;
            set
            {
                int v = Clamp(value, 0, 100);
                if (_percentFull == v) return;
                _percentFull = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Minimum percentage before warning colour is preferred.")]
        [DefaultValue(25)]
        public int PercentMinWarning
        {
            get => _percentMinWarning;
            set
            {
                int v = Clamp(value, 0, 100);
                if (_percentMinWarning == v) return;
                _percentMinWarning = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Colour used for active bars when PercentFull is >= PercentMinWarning.")]
        [DefaultValue(typeof(Color), "DimGray")]
        public Color BarEnabledColour
        {
            get => _barEnabledColour;
            set
            {
                if (_barEnabledColour == value) return;
                _barEnabledColour = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Colour used for active bars when PercentFull is < PercentMinWarning (very low match).")]
        [DefaultValue(typeof(Color), "OrangeRed")]
        public Color BarWarningColour
        {
            get => _barWarningColour;
            set
            {
                if (_barWarningColour == value) return;
                _barWarningColour = value;
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (BackColor.A == 255)
            {
                base.OnPaintBackground(pevent);
                return;
            }

            var back = Parent?.BackColor ?? SystemColors.Control;
            pevent.Graphics.Clear(back);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            var client = ClientRectangle;
            if (client.Width <= 1 || client.Height <= 1)
                return;

            var back = ResolveBackColor();
            var baseColor = PercentFull < PercentMinWarning ? BarWarningColour : BarEnabledColour;
            var inactiveColor = DimTowardsBackground(baseColor, back, 0.72f);

            int activeBars = PercentFull == 100 ? 4 : (PercentFull / 25);

            float pad = MathF.Max(0f, MathF.Min(client.Width, client.Height) * 0.08f);
            float availW = client.Width - pad * 2f;
            float availH = client.Height - pad * 2f;

            if (availW <= 1f || availH <= 1f)
                return;

            float gap = MathF.Max(1f, availW * 0.06f);
            float barW = (availW - (gap * 3f)) / 4f;
            if (barW < 1f) barW = 1f;

            float radius = MathF.Max(1f, barW * 0.5f);

            float[] heightFrac = { 0.25f, 0.50f, 0.75f, 1.00f };

            float x = client.Left + pad;

            for (int i = 0; i < 4; i++)
            {
                float bh = MathF.Max(1f, availH * heightFrac[i]);
                float y = client.Top + pad + (availH - bh);

                var r = new RectangleF(x, y, barW, bh);
                var c = i < activeBars ? baseColor : inactiveColor;

                using var brush = new SolidBrush(c);
                using var path = CreateRoundedRectPath(r, radius);
                g.FillPath(brush, path);

                x += barW + gap;
            }

            if (PercentFull == 100)
            {
                float inset = MathF.Max(1f, pad * 0.5f);

                float minWH = MathF.Min(availW, availH);
                float starSize = minWH * (STAR_SIZE_PERCENT / 100f);
                if (starSize < 2f) starSize = 2f;

                var starRect = new RectangleF(
                    client.Left + inset,
                    client.Top + inset,
                    starSize,
                    starSize);

                using var starBrush = new SolidBrush(baseColor);
                using var starPath = CreateStarPath(starRect, points: 8, innerRatio: 0.45f);
                g.FillPath(starBrush, starPath);
            }

            if (PercentFull < 25)
            {
                var strikeColor = BarWarningColour;
                float penW = MathF.Max(1f, MathF.Min(availW, availH) * 0.05f);

                using var pen = new Pen(strikeColor, penW)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                };

                float yMid = client.Top + (client.Height / 1.8f);

                float x1 = client.Left + pad * 0.5f;
                float x2 = client.Right - pad * 0.5f;

                g.DrawLine(pen, x1, yMid, x2, yMid);
            }
        }

        private Color ResolveBackColor()
        {
            if (BackColor.A == 255)
                return BackColor;

            if (Parent != null)
                return Parent.BackColor;

            return SystemColors.Control;
        }

        private static Color DimTowardsBackground(Color fg, Color bg, float t)
        {
            if (t < 0f) t = 0f;
            if (t > 1f) t = 1f;

            int r = (int)Math.Round((fg.R * (1f - t)) + (bg.R * t), MidpointRounding.AwayFromZero);
            int g = (int)Math.Round((fg.G * (1f - t)) + (bg.G * t), MidpointRounding.AwayFromZero);
            int b = (int)Math.Round((fg.B * (1f - t)) + (bg.B * t), MidpointRounding.AwayFromZero);

            if (r < 0) r = 0; if (r > 255) r = 255;
            if (g < 0) g = 0; if (g > 255) g = 255;
            if (b < 0) b = 0; if (b > 255) b = 255;

            return Color.FromArgb(255, r, g, b);
        }

        private static GraphicsPath CreateRoundedRectPath(RectangleF r, float radius)
        {
            float d = radius * 2f;
            if (d < 1f) d = 1f;
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;

            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180f, 90f);
            path.AddArc(r.Right - d, r.Y, d, d, 270f, 90f);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0f, 90f);
            path.AddArc(r.X, r.Bottom - d, d, d, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath CreateStarPath(RectangleF bounds, int points, float innerRatio)
        {
            if (points < 3) points = 3;

            if (innerRatio < 0.05f) innerRatio = 0.05f;
            if (innerRatio > 0.95f) innerRatio = 0.95f;

            float cx = bounds.Left + (bounds.Width / 2f);
            float cy = bounds.Top + (bounds.Height / 2f);

            float outerR = MathF.Min(bounds.Width, bounds.Height) / 2f;
            float innerR = outerR * innerRatio;

            int n = points * 2;
            var pts = new PointF[n];

            float a = -MathF.PI / 2f;
            float step = MathF.PI / points;

            for (int i = 0; i < n; i++)
            {
                float r = (i & 1) == 0 ? outerR : innerR;
                pts[i] = new PointF(cx + MathF.Cos(a) * r, cy + MathF.Sin(a) * r);
                a += step;
            }

            var path = new GraphicsPath();
            path.AddPolygon(pts);
            return path;
        }

        private static int Clamp(int v, int min, int max)
            => v < min ? min : (v > max ? max : v);
    }
}
