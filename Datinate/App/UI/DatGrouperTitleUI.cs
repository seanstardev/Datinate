using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    [DefaultProperty(nameof(Text))]
    public sealed class DatGrouperTitleUI : Control
    {
        private Color baseColor = Color.FromArgb(222, 160, 217);
        private string rightText = "Set";
        private int cornerRadius = 2;
        private Padding leftTextPadding = new Padding(10, 2, 10, 2);
        private Padding rightTextPadding = new Padding(10, 2, 10, 2);
        private int borderThickness = 1;

        public DatGrouperTitleUI()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            TabStop = false;
            Cursor = Cursors.Default;
            BackColor = Color.Transparent;

            Size = new Size(120, 26);
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BaseColor
        {
            get => baseColor;
            set { baseColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue("Set")]
        public string RightText
        {
            get => rightText;
            set { rightText = value ?? string.Empty; InvalidateAutoSize(); Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(10)]
        public int CornerRadius
        {
            get => cornerRadius;
            set { cornerRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Layout")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding LeftTextPadding
        {
            get => leftTextPadding;
            set { leftTextPadding = value; InvalidateAutoSize(); Invalidate(); }
        }

        [Category("Layout")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Padding RightTextPadding
        {
            get => rightTextPadding;
            set { rightTextPadding = value; InvalidateAutoSize(); Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(1)]
        public int BorderThickness
        {
            get => borderThickness;
            set { borderThickness = Math.Max(0, value); Invalidate(); }
        }

        protected override bool ShowFocusCues => false;

        protected override void OnGotFocus(EventArgs e)
        {
            if (Parent != null)
                Parent.Select();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (BackColor != Color.Transparent)
            {
                using var b = new SolidBrush(BackColor);
                e.Graphics.FillRectangle(b, ClientRectangle);
                return;
            }

            PaintTransparentBackground(e.Graphics, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            if (rect.Width <= 1 || rect.Height <= 1)
                return;

            var radius = Math.Min(cornerRadius, Math.Max(0, (rect.Height - 1) / 2));

            var rightW = GetRightSegmentWidth(g);
            rightW = Math.Min(rightW, Math.Max(0, Width));

            using var path = CreateRoundedRectPath(rect, radius);

            var metalBase = ToMetalTint(baseColor);
            var metalRight = Darken(metalBase, 0.06f);

            var borderColor = Darken(metalBase, 0.45f);

            FillMetalSegment(g, path, rect, metalBase);

            var rightRect = new Rectangle(Width - rightW, 0, rightW, Height);
            if (rightW > 0)
            {
                var st = g.Save();
                try
                {
                    g.SetClip(path);
                    g.SetClip(rightRect, CombineMode.Intersect);

                    FillMetalRect(g, rightRect, metalRight);
                }
                finally
                {
                    g.Restore(st);
                }
            }

            ApplyBrushedGrain(g, path, rect, metalBase.ToArgb());

            ApplySpecularBands(g, path, rect);

            if (borderThickness > 0)
            {
                using var borderPen = new Pen(borderColor, borderThickness) { Alignment = PenAlignment.Inset };
                g.DrawPath(borderPen, path);

                if (rect.Width > 4 && rect.Height > 4)
                {
                    var innerRect = Rectangle.Inflate(rect, -1, -1);
                    var innerRadius = Math.Max(0, radius - 1);

                    using var innerPath = CreateRoundedRectPath(innerRect, innerRadius);

                    var topSt = g.Save();
                    try
                    {
                        g.SetClip(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 2));
                        using var topPen = new Pen(Color.FromArgb(125, Color.White), 1f) { Alignment = PenAlignment.Inset };
                        g.DrawPath(topPen, innerPath);
                    }
                    finally
                    {
                        g.Restore(topSt);
                    }

                    var bottomSt = g.Save();
                    try
                    {
                        g.SetClip(new Rectangle(rect.X, rect.Y + rect.Height / 2, rect.Width, rect.Height));
                        using var bottomPen = new Pen(Color.FromArgb(90, Color.Black), 1f) { Alignment = PenAlignment.Inset };
                        g.DrawPath(bottomPen, innerPath);
                    }
                    finally
                    {
                        g.Restore(bottomSt);
                    }
                }
            }

            if (rightW > 0 && rightW < Width)
            {
                var sepX = Width - rightW;
                var sepTop = Math.Max(1, radius / 2);
                var sepBottom = Height - Math.Max(2, radius / 2);

                using var sepDark = new Pen(Color.FromArgb(110, Color.Black), 1f);
                using var sepLight = new Pen(Color.FromArgb(90, Color.White), 1f);

                g.DrawLine(sepDark, sepX, sepTop, sepX, sepBottom);
                g.DrawLine(sepLight, sepX + 1, sepTop, sepX + 1, sepBottom);
            }

            var leftTextRect = new Rectangle(0, 0, Math.Max(0, Width - rightW), Height);
            leftTextRect = ApplyPadding(leftTextRect, leftTextPadding);

            var rightTextRect = new Rectangle(Width - rightW, 0, rightW, Height);
            rightTextRect = ApplyPadding(rightTextRect, rightTextPadding);

            var flags = TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.EndEllipsis;

            var mainText = Color.FromArgb(18, 18, 18);
            var hiText = Color.FromArgb(70, Color.White);

            var leftHiRect = OffsetRect(leftTextRect, 0, -1);
            TextRenderer.DrawText(g, Text ?? string.Empty, Font, leftHiRect, hiText, flags);
            TextRenderer.DrawText(g, Text ?? string.Empty, Font, leftTextRect, mainText, flags);

            var rightHiRect = OffsetRect(rightTextRect, 0, -1);
            TextRenderer.DrawText(g, rightText ?? string.Empty, Font, rightHiRect, hiText, flags);
            TextRenderer.DrawText(g, rightText ?? string.Empty, Font, rightTextRect, mainText, flags);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            using var g = CreateGraphics();

            var leftTextSize = TextRenderer.MeasureText(g, Text ?? string.Empty, Font, Size.Empty, TextFormatFlags.SingleLine);
            var rightTextSize = TextRenderer.MeasureText(g, rightText ?? string.Empty, Font, Size.Empty, TextFormatFlags.SingleLine);

            var h = Math.Max(leftTextSize.Height, rightTextSize.Height) + leftTextPadding.Vertical;
            h = Math.Max(h, 22);

            var rightW = rightTextSize.Width + rightTextPadding.Horizontal;
            var leftW = leftTextSize.Width + leftTextPadding.Horizontal;

            var w = leftW + rightW + (cornerRadius / 2);
            return new Size(w, h);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            InvalidateAutoSize();
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            InvalidateAutoSize();
            Invalidate();
        }

        private void InvalidateAutoSize()
        {
            if (AutoSize)
                Size = GetPreferredSize(Size.Empty);
        }

        private int GetRightSegmentWidth(Graphics g)
        {
            var size = TextRenderer.MeasureText(g, rightText ?? string.Empty, Font, Size.Empty, TextFormatFlags.SingleLine);
            return size.Width + rightTextPadding.Horizontal;
        }

        private static Rectangle ApplyPadding(Rectangle r, Padding p)
        {
            var x = r.X + p.Left;
            var y = r.Y + p.Top;
            var w = Math.Max(0, r.Width - p.Horizontal);
            var h = Math.Max(0, r.Height - p.Vertical);
            return new Rectangle(x, y, w, h);
        }

        private static Rectangle OffsetRect(Rectangle r, int dx, int dy) =>
            new Rectangle(r.X + dx, r.Y + dy, r.Width, r.Height);

        private static void FillMetalSegment(Graphics g, GraphicsPath clipPath, Rectangle rect, Color tint)
        {
            var st = g.Save();
            try
            {
                g.SetClip(clipPath);
                FillMetalRect(g, rect, tint);
            }
            finally
            {
                g.Restore(st);
            }
        }

        private static void FillMetalRect(Graphics g, Rectangle rect, Color tint)
        {
            var top = Lighten(tint, 0.075f);
            var bottom = Darken(tint, 0.075f);

            using var baseBrush = new LinearGradientBrush(rect, top, bottom, LinearGradientMode.Vertical);
            g.FillRectangle(baseBrush, rect);

            var midBand = new Rectangle(rect.X, rect.Y + (rect.Height / 3), rect.Width, Math.Max(1, rect.Height / 3));
            using var midBrush = new LinearGradientBrush(
                midBand,
                Color.FromArgb(45, Color.White),
                Color.FromArgb(0, Color.White),
                LinearGradientMode.Vertical);
            g.FillRectangle(midBrush, midBand);
        }

        private static void ApplySpecularBands(Graphics g, GraphicsPath clipPath, Rectangle rect)
        {
            var st = g.Save();
            try
            {
                g.SetClip(clipPath);

                using var bandBrush = new LinearGradientBrush(rect, Color.Transparent, Color.Transparent, LinearGradientMode.Horizontal);

                var cb = new ColorBlend
                {
                    Positions = new[]
                    {
                        0.00f, 0.18f, 0.30f, 0.38f, 0.55f, 0.70f, 0.78f, 1.00f
                    },
                    Colors = new[]
                    {
                        Color.FromArgb(0, Color.White),
                        Color.FromArgb(20, Color.White),
                        Color.FromArgb(95, Color.White),
                        Color.FromArgb(18, Color.White),
                        Color.FromArgb(28, Color.Black),
                        Color.FromArgb(110, Color.White),
                        Color.FromArgb(16, Color.White),
                        Color.FromArgb(0, Color.White)
                    }
                };

                bandBrush.InterpolationColors = cb;

                g.FillRectangle(bandBrush, rect);

                using var edgeVignette = new LinearGradientBrush(rect, Color.Transparent, Color.Transparent, LinearGradientMode.Horizontal);
                var cb2 = new ColorBlend
                {
                    Positions = new[] { 0.00f, 0.08f, 0.50f, 0.92f, 1.00f },
                    Colors = new[]
                    {
                        Color.FromArgb(42, Color.Black),
                        Color.FromArgb(0, Color.Black),
                        Color.FromArgb(0, Color.Black),
                        Color.FromArgb(0, Color.Black),
                        Color.FromArgb(42, Color.Black)
                    }
                };
                edgeVignette.InterpolationColors = cb2;
                g.FillRectangle(edgeVignette, rect);
            }
            finally
            {
                g.Restore(st);
            }
        }

        private static void ApplyBrushedGrain(Graphics g, GraphicsPath clipPath, Rectangle rect, int seed)
        {
            var st = g.Save();
            try
            {
                g.SetClip(clipPath);

                for (int y = rect.Top; y <= rect.Bottom; y++)
                {
                    uint h = (uint)(seed ^ (y * 374761393));
                    h ^= h >> 13;
                    h *= 1274126177;
                    h ^= h >> 16;

                    int v = (int)(h & 0xFF) - 128;
                    int av = Math.Abs(v);

                    bool light = v >= 0;

                    int alpha = light
                        ? (10 + (av * 18 / 128))
                        : (8 + (av * 10 / 128));

                    var c = light
                        ? Color.FromArgb(alpha, 255, 255, 255)
                        : Color.FromArgb(alpha, 0, 0, 0);

                    using var b = new SolidBrush(c);
                    g.FillRectangle(b, rect.Left, y, rect.Width + 1, 1);
                }
            }
            finally
            {
                g.Restore(st);
            }
        }

        private static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            var d = radius * 2;
            var arc = new Rectangle(rect.X, rect.Y, d, d);

            path.AddArc(arc, 180, 90);

            arc.X = rect.Right - d;
            path.AddArc(arc, 270, 90);

            arc.Y = rect.Bottom - d;
            path.AddArc(arc, 0, 90);

            arc.X = rect.X;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private static Color Darken(Color c, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);

            int r = (int)Math.Round(c.R * (1f - amount));
            int g = (int)Math.Round(c.G * (1f - amount));
            int b = (int)Math.Round(c.B * (1f - amount));

            return Color.FromArgb(c.A, r, g, b);
        }

        private static Color Lighten(Color c, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);

            int r = (int)Math.Round(c.R + (255 - c.R) * amount);
            int g = (int)Math.Round(c.G + (255 - c.G) * amount);
            int b = (int)Math.Round(c.B + (255 - c.B) * amount);

            return Color.FromArgb(c.A, r, g, b);
        }

        private static Color ToMetalTint(Color c)
        {
            var neutral = Color.FromArgb(214, 214, 214);
            var mixed = Mix(c, neutral, 0.68f);

            double l = (0.299 * mixed.R) + (0.587 * mixed.G) + (0.114 * mixed.B);
            if (l < 150)
            {
                float t = (float)((150 - l) / 255.0);
                mixed = Lighten(mixed, Math.Clamp(t * 0.9f, 0f, 0.22f));
            }

            return Color.FromArgb(255, mixed.R, mixed.G, mixed.B);
        }

        private static Color Mix(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            float it = 1f - t;

            int r = (int)Math.Round((a.R * it) + (b.R * t));
            int g = (int)Math.Round((a.G * it) + (b.G * t));
            int bl = (int)Math.Round((a.B * it) + (b.B * t));

            return Color.FromArgb(255, r, g, bl);
        }

        private void PaintTransparentBackground(Graphics g, Rectangle clipRect)
        {
            if (Parent == null)
            {
                g.Clear(SystemColors.Control);
                return;
            }

            var state = g.Save();
            try
            {
                g.TranslateTransform(-Left, -Top);
                var pe = new PaintEventArgs(g, new Rectangle(Left, Top, Width, Height));
                InvokePaintBackground(Parent, pe);
                InvokePaint(Parent, pe);
            }
            finally
            {
                g.Restore(state);
            }
        }
    }
}
