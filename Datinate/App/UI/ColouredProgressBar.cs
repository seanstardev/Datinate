using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace datinate.app
{
    [DesignerCategory("Code")]
    [ToolboxItem(true)]
    public sealed class ColouredProgressBar : Control
    {
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;

        private Color _barBackColor = Color.White;
        private Color _fillStartColor = Color.DeepSkyBlue;
        private Color _fillEndColor = Color.MediumPurple;

        private Color _borderColor = Color.FromArgb(210, 226, 235);
        private int _borderThickness = 1;

        private int _cornerRadius = 10;
        private int _innerPadding = 2;

        private float _gradientAngle = 0f;

        private bool _enableSheen = true;
        private int _sheenStrength = 55;
        private float _sheenHeightRatio = 0.62f;

        private bool _enableGrain = true;
        private int _grainStrength = 18;
        private int _grainSpacing = 3;
        private float _grainAngle = -25f;
        private bool _grainAffectsTrack = true;
        private int _grainTileSize = 64;

        private Bitmap? _grainTile;
        private int _grainTileCacheSize;
        private int _grainTileCacheStrength;
        private int _grainTileCacheSpacing;

        public event EventHandler? ValueChanged;

        public ColouredProgressBar()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(200, 16);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _grainTile?.Dispose();
                _grainTile = null;
            }

            base.Dispose(disposing);
        }

        [Category("Behavior")]
        [DefaultValue(0)]
        public int Minimum
        {
            get => _minimum;
            set
            {
                if (_minimum == value) return;
                _minimum = value;
                if (_maximum < _minimum) _maximum = _minimum;
                Value = _value;
                RepaintNow();
            }
        }

        [Category("Behavior")]
        [DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set
            {
                if (_maximum == value) return;
                _maximum = value;
                if (_maximum < _minimum) _minimum = _maximum;
                Value = _value;
                RepaintNow();
            }
        }

        [Category("Behavior")]
        [DefaultValue(0)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Value
        {
            get => _value;
            set
            {
                var clamped = Math.Max(_minimum, Math.Min(_maximum, value));
                if (_value == clamped) return;
                _value = clamped;
                ValueChanged?.Invoke(this, EventArgs.Empty);
                RepaintNow();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "White")]
        public Color BarBackColor
        {
            get => _barBackColor;
            set { if (_barBackColor == value) return; _barBackColor = value; RepaintNow(); }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "DeepSkyBlue")]
        public Color FillStartColor
        {
            get => _fillStartColor;
            set { if (_fillStartColor == value) return; _fillStartColor = value; RepaintNow(); }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "MediumPurple")]
        public Color FillEndColor
        {
            get => _fillEndColor;
            set { if (_fillEndColor == value) return; _fillEndColor = value; RepaintNow(); }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "210, 226, 235")]
        public Color BorderColor
        {
            get => _borderColor;
            set { if (_borderColor == value) return; _borderColor = value; RepaintNow(); }
        }

        [Category("Appearance")]
        [DefaultValue(1)]
        public int BorderThickness
        {
            get => _borderThickness;
            set { var v = Math.Max(0, value); if (_borderThickness == v) return; _borderThickness = v; RepaintNow(); }
        }

        [Category("Appearance")]
        [DefaultValue(10)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set { var v = Math.Max(0, value); if (_cornerRadius == v) return; _cornerRadius = v; RepaintNow(); }
        }

        [Category("Layout")]
        [DefaultValue(2)]
        public int InnerPadding
        {
            get => _innerPadding;
            set { var v = Math.Max(0, value); if (_innerPadding == v) return; _innerPadding = v; RepaintNow(); }
        }

        [Category("Appearance")]
        [DefaultValue(0f)]
        public float GradientAngle
        {
            get => _gradientAngle;
            set { if (Math.Abs(_gradientAngle - value) < 0.001f) return; _gradientAngle = value; RepaintNow(); }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool EnableSheen
        {
            get => _enableSheen;
            set { if (_enableSheen == value) return; _enableSheen = value; RepaintNow(); }
        }

        [Category("Appearance")]
        [DefaultValue(55)]
        public int SheenStrength
        {
            get => _sheenStrength;
            set
            {
                var v = Math.Max(0, Math.Min(100, value));
                if (_sheenStrength == v) return;
                _sheenStrength = v;
                RepaintNow();
            }
        }

        [Category("Appearance")]
        [DefaultValue(0.62f)]
        public float SheenHeightRatio
        {
            get => _sheenHeightRatio;
            set
            {
                var v = Math.Max(0.2f, Math.Min(1.0f, value));
                if (Math.Abs(_sheenHeightRatio - v) < 0.0001f) return;
                _sheenHeightRatio = v;
                RepaintNow();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool EnableGrain
        {
            get => _enableGrain;
            set { if (_enableGrain == value) return; _enableGrain = value; RepaintNow(); }
        }

        [Category("Appearance")]
        [DefaultValue(18)]
        public int GrainStrength
        {
            get => _grainStrength;
            set
            {
                var v = Math.Max(0, Math.Min(100, value));
                if (_grainStrength == v) return;
                _grainStrength = v;
                InvalidateGrainTile();
                RepaintNow();
            }
        }

        [Category("Appearance")]
        [DefaultValue(3)]
        public int GrainSpacing
        {
            get => _grainSpacing;
            set
            {
                var v = Math.Max(1, Math.Min(12, value));
                if (_grainSpacing == v) return;
                _grainSpacing = v;
                InvalidateGrainTile();
                RepaintNow();
            }
        }

        [Category("Appearance")]
        [DefaultValue(-25f)]
        public float GrainAngle
        {
            get => _grainAngle;
            set { if (Math.Abs(_grainAngle - value) < 0.001f) return; _grainAngle = value; RepaintNow(); }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool GrainAffectsTrack
        {
            get => _grainAffectsTrack;
            set { if (_grainAffectsTrack == value) return; _grainAffectsTrack = value; RepaintNow(); }
        }

        [Category("Appearance")]
        [DefaultValue(64)]
        public int GrainTileSize
        {
            get => _grainTileSize;
            set
            {
                var v = Math.Max(16, Math.Min(256, value));
                if (_grainTileSize == v) return;
                _grainTileSize = v;
                InvalidateGrainTile();
                RepaintNow();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            var outer = ClientRectangle;
            if (outer.Width <= 1 || outer.Height <= 1) return;

            PaintParentBackground(g);

            var borderRect = Rectangle.Inflate(outer, -_borderThickness / 2, -_borderThickness / 2);
            var outerRadius = ClampRadius(_cornerRadius, borderRect);

            using (var outerPath = RoundedRect(borderRect, outerRadius))
            {
                using (var backBrush = new SolidBrush(_barBackColor))
                    g.FillPath(backBrush, outerPath);

                if (_enableGrain && _grainStrength > 0 && _grainAffectsTrack)
                {
                    EnsureGrainTile();
                    if (_grainTile != null)
                    {
                        var alphaScale = 0.55f;
                        using var tb = CreateGrainBrush(borderRect, alphaScale);
                        var s = g.Save();
                        try
                        {
                            g.SetClip(outerPath);
                            g.FillRectangle(tb, borderRect);
                        }
                        finally { g.Restore(s); }
                    }
                }

                if (_enableSheen && _sheenStrength > 0)
                {
                    var aTop = (int)Math.Round(18 + (_sheenStrength * 0.55));
                    var aBot = (int)Math.Round(14 + (_sheenStrength * 0.35));

                    using (var trackSheen = new LinearGradientBrush(
                               borderRect,
                               Color.FromArgb(aTop, 255, 255, 255),
                               Color.FromArgb(aBot, 0, 0, 0),
                               90f))
                    {
                        g.FillPath(trackSheen, outerPath);
                    }

                    var innerForStroke = Rectangle.Inflate(borderRect, -1, -1);
                    if (innerForStroke.Width > 2 && innerForStroke.Height > 2)
                    {
                        using var innerPath = RoundedRect(innerForStroke, ClampRadius(outerRadius - 1, innerForStroke));
                        using var topPen = new Pen(Color.FromArgb(55, 255, 255, 255), 1);
                        using var botPen = new Pen(Color.FromArgb(35, 0, 0, 0), 1);

                        var clipState = g.Save();
                        try
                        {
                            g.SetClip(innerPath);
                            g.DrawLine(topPen, innerForStroke.Left + 1, innerForStroke.Top + 1, innerForStroke.Right - 2, innerForStroke.Top + 1);
                            g.DrawLine(botPen, innerForStroke.Left + 1, innerForStroke.Bottom - 2, innerForStroke.Right - 2, innerForStroke.Bottom - 2);
                        }
                        finally { g.Restore(clipState); }
                    }
                }

                var range = Math.Max(1, _maximum - _minimum);
                var pct = (float)(_value - _minimum) / range;
                pct = Math.Max(0f, Math.Min(1f, pct));

                var inner = Rectangle.Inflate(borderRect, -_innerPadding, -_innerPadding);
                if (inner.Width > 0 && inner.Height > 0 && pct > 0f)
                {
                    var fillWidth = (int)Math.Round(inner.Width * pct);
                    fillWidth = Math.Max(1, Math.Min(inner.Width, fillWidth));

                    var fillRect = new Rectangle(inner.X, inner.Y, fillWidth, inner.Height);

                    var leftR = Math.Max(0, ClampRadius(_cornerRadius - _innerPadding, inner));
                    var rightR = (pct >= 0.999f) ? leftR : 0;

                    using (var fillPath = RoundedRectSides(fillRect, leftR, rightR))
                    {
                        using (var fillBrush = new LinearGradientBrush(fillRect, _fillStartColor, _fillEndColor, _gradientAngle))
                            g.FillPath(fillBrush, fillPath);

                        if (_enableGrain && _grainStrength > 0)
                        {
                            EnsureGrainTile();
                            if (_grainTile != null)
                            {
                                using var tb = CreateGrainBrush(fillRect, 1.0f);
                                var s = g.Save();
                                try
                                {
                                    g.SetClip(fillPath);
                                    g.FillRectangle(tb, fillRect);
                                }
                                finally { g.Restore(s); }
                            }
                        }

                        if (_enableSheen && _sheenStrength > 0)
                        {
                            var sheenHeight = Math.Max(1, (int)Math.Round(fillRect.Height * _sheenHeightRatio));
                            var sheenRect = new Rectangle(fillRect.X, fillRect.Y, fillRect.Width, sheenHeight);

                            var a1 = (int)Math.Round(30 + (_sheenStrength * 1.10));
                            var a2 = (int)Math.Round(18 + (_sheenStrength * 0.55));
                            var a3 = (int)Math.Round(10 + (_sheenStrength * 0.35));

                            using var sheenBrush = CreateMetalSheenBrush(sheenRect, a1, a2, a3);
                            var state = g.Save();
                            try
                            {
                                g.SetClip(fillPath);
                                g.FillRectangle(sheenBrush, sheenRect);
                            }
                            finally { g.Restore(state); }

                            var edgeA = (int)Math.Round(16 + (_sheenStrength * 0.55));
                            using var edgePen = new Pen(Color.FromArgb(edgeA, 255, 255, 255), 1);
                            var state2 = g.Save();
                            try
                            {
                                g.SetClip(fillPath);
                                g.DrawLine(edgePen, fillRect.Left + 1, fillRect.Top + 1, fillRect.Right - 2, fillRect.Top + 1);
                            }
                            finally { g.Restore(state2); }
                        }
                    }
                }

                if (_borderThickness > 0)
                {
                    using var pen = new Pen(_borderColor, _borderThickness);
                    g.DrawPath(pen, outerPath);
                }
            }
        }

        private TextureBrush CreateGrainBrush(Rectangle target, float alphaScale)
        {
            var tile = _grainTile!;
            var tb = new TextureBrush(tile, WrapMode.Tile);

            var cx = target.Left + (target.Width * 0.5f);
            var cy = target.Top + (target.Height * 0.5f);

            var m = new Matrix();
            m.Translate(cx, cy);
            m.Rotate(_grainAngle);
            m.Translate(-cx, -cy);

            tb.Transform = m;

            if (Math.Abs(alphaScale - 1.0f) > 0.001f)
            {
                var cm = new ColorMatrix
                {
                    Matrix00 = 1f,
                    Matrix11 = 1f,
                    Matrix22 = 1f,
                    Matrix33 = alphaScale,
                    Matrix44 = 1f
                };

                var ia = new ImageAttributes();
                ia.SetColorMatrix(cm);

                var b = new Bitmap(tile.Width, tile.Height, PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(b))
                {
                    g.DrawImage(tile,
                        new Rectangle(0, 0, b.Width, b.Height),
                        0, 0, tile.Width, tile.Height,
                        GraphicsUnit.Pixel, ia);
                }

                tb.Dispose();
                tb = new TextureBrush(b, WrapMode.Tile);
                tb.Transform = m;
                b.Dispose();
            }

            return tb;
        }

        private void EnsureGrainTile()
        {
            var size = _grainTileSize;
            var strength = _grainStrength;
            var spacing = _grainSpacing;

            if (_grainTile != null &&
                _grainTileCacheSize == size &&
                _grainTileCacheStrength == strength &&
                _grainTileCacheSpacing == spacing)
                return;

            _grainTile?.Dispose();
            _grainTile = CreateGrainTile(size, strength, spacing);

            _grainTileCacheSize = size;
            _grainTileCacheStrength = strength;
            _grainTileCacheSpacing = spacing;
        }

        private void InvalidateGrainTile()
        {
            _grainTile?.Dispose();
            _grainTile = null;
            _grainTileCacheSize = 0;
            _grainTileCacheStrength = 0;
            _grainTileCacheSpacing = 0;
        }

        private static Bitmap CreateGrainTile(int size, int strength, int spacing)
        {
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppPArgb);

            uint rng = 0xA3C59AC3u;

            int LightAlpha(int baseA)
            {
                rng ^= rng << 13;
                rng ^= rng >> 17;
                rng ^= rng << 5;
                var n = (int)(rng & 0xFF);
                var jitter = (n - 128) / 18;
                return Math.Max(0, Math.Min(255, baseA + jitter));
            }

            var aLight = (int)Math.Round(4 + (strength * 0.85));
            var aDark = (int)Math.Round(3 + (strength * 0.70));

            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.None;
                g.CompositingMode = CompositingMode.SourceOver;

                using var clear = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
                g.FillRectangle(clear, new Rectangle(0, 0, size, size));

                for (int x = 0; x < size; x += spacing)
                {
                    var la = LightAlpha(aLight);
                    var da = LightAlpha(aDark);

                    using var p1 = new Pen(Color.FromArgb(la, 255, 255, 255), 1);
                    using var p2 = new Pen(Color.FromArgb(da, 0, 0, 0), 1);

                    g.DrawLine(p1, x, 0, x, size);
                    g.DrawLine(p2, x + 1, 0, x + 1, size);
                }

                var speckA = (int)Math.Round(2 + (strength * 0.35));
                if (speckA > 0)
                {
                    for (int i = 0; i < size * 6; i++)
                    {
                        rng ^= rng << 13;
                        rng ^= rng >> 17;
                        rng ^= rng << 5;

                        var x = (int)(rng & 0x3F) % size;
                        var y = (int)((rng >> 8) & 0x3F) % size;
                        var isLight = ((rng >> 16) & 1) == 0;

                        var a = LightAlpha(speckA);
                        var c = isLight ? Color.FromArgb(a, 255, 255, 255) : Color.FromArgb(a, 0, 0, 0);

                        using var p = new Pen(c, 1);
                        g.DrawRectangle(p, x, y, 1, 1);
                    }
                }
            }

            return bmp;
        }

        private static LinearGradientBrush CreateMetalSheenBrush(Rectangle rect, int aTop, int aMid, int aLow)
        {
            var brush = new LinearGradientBrush(rect, Color.Transparent, Color.Transparent, 90f);

            var cb = new ColorBlend
            {
                Positions = new[] { 0.0f, 0.22f, 0.52f, 1.0f },
                Colors = new[]
                {
                    Color.FromArgb(aTop, 255, 255, 255),
                    Color.FromArgb(aMid, 255, 255, 255),
                    Color.FromArgb(aLow, 0, 0, 0),
                    Color.FromArgb(0, 0, 0, 0)
                }
            };

            brush.InterpolationColors = cb;
            return brush;
        }

        private void RepaintNow()
        {
            Invalidate();
            if (IsDesigning())
                Update();
        }

        private bool IsDesigning()
            => LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site?.DesignMode ?? false);

        private void PaintParentBackground(Graphics g)
        {
            if (BackColor.A == 255)
            {
                using var b = new SolidBrush(BackColor);
                g.FillRectangle(b, ClientRectangle);
                return;
            }

            if (Parent == null)
            {
                using var b = new SolidBrush(Color.FromArgb(255, BackColor));
                g.FillRectangle(b, ClientRectangle);
                return;
            }

            var state = g.Save();
            try
            {
                g.TranslateTransform(-Left, -Top);
                var pe = new PaintEventArgs(g, Parent.ClientRectangle);
                InvokePaintBackground(Parent, pe);
                InvokePaint(Parent, pe);
            }
            finally
            {
                g.Restore(state);
            }
        }

        private static int ClampRadius(int radius, Rectangle rect)
        {
            var max = Math.Max(0, Math.Min(rect.Width, rect.Height) / 2);
            return Math.Max(0, Math.Min(radius, max));
        }

        private static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            var r = ClampRadius(radius, rect);
            var d = r * 2;

            var path = new GraphicsPath();

            if (r == 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            var tl = new Rectangle(rect.Left, rect.Top, d, d);
            var tr = new Rectangle(rect.Right - d, rect.Top, d, d);
            var br = new Rectangle(rect.Right - d, rect.Bottom - d, d, d);
            var bl = new Rectangle(rect.Left, rect.Bottom - d, d, d);

            path.AddArc(tl, 180, 90);
            path.AddArc(tr, 270, 90);
            path.AddArc(br, 0, 90);
            path.AddArc(bl, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath RoundedRectSides(Rectangle rect, int leftRadius, int rightRadius)
        {
            var lr = ClampRadius(leftRadius, rect);
            var rr = ClampRadius(rightRadius, rect);

            var dL = lr * 2;
            var dR = rr * 2;

            var path = new GraphicsPath();

            if (lr == 0 && rr == 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            var tl = new Rectangle(rect.Left, rect.Top, dL, dL);
            var bl = new Rectangle(rect.Left, rect.Bottom - dL, dL, dL);

            var tr = new Rectangle(rect.Right - dR, rect.Top, dR, dR);
            var br = new Rectangle(rect.Right - dR, rect.Bottom - dR, dR, dR);

            if (lr > 0) path.AddArc(tl, 180, 90);
            else path.AddLine(rect.Left, rect.Top, rect.Left, rect.Top);

            if (rr > 0) path.AddArc(tr, 270, 90);
            else path.AddLine(rect.Right, rect.Top, rect.Right, rect.Top);

            if (rr > 0) path.AddArc(br, 0, 90);
            else path.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom);

            if (lr > 0) path.AddArc(bl, 90, 90);
            else path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Bottom);

            path.CloseFigure();
            return path;
        }
    }
}
