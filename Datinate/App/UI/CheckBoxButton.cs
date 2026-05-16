using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    [DesignerCategory("Code")]
    [ToolboxItem(true)]
    [DefaultEvent(nameof(CheckedChanged))]
    [DefaultProperty(nameof(Checked))]
    public sealed class CheckBoxButton : Control
    {
        private bool _checked;

        private bool _hover;
        private bool _mouseDown;
        private bool _keyDown;
        private const bool _blackOutlineEnabled = true;
        private const float _blackOutlineThickness = 1f;
        private static readonly Color _blackOutlineColor = Color.FromArgb(180, 0, 0, 0);
        private Color _checkedBackColor = Color.DodgerBlue;
        private Color _uncheckedBackColor = Color.White;

        private Color _checkedForeColor = Color.White;
        private Color _uncheckedForeColor = Color.DimGray;

        private Color _borderColor = Color.Gainsboro;
        private float _borderThickness = 1f;

        private float _cornerRadiusRatio = 0.5f;

        private bool _shadowEnabled = true;
        private int _shadowOpacity = 70;
        private int _shadowOffsetY = 2;

        private bool _innerBevelEnabled = true;
        private int _innerBevelOpacity = 70;

        private bool _useParentBackColor = true;

        public event EventHandler? CheckedChanged;

        public CheckBoxButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable,
                true);

            TabStop = true;
            Cursor = Cursors.Hand;

            Size = new Size(180, 56);
            Padding = new Padding(14, 6, 14, 6);
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool ToggleOnClick { get; set; } = true;

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "DodgerBlue")]
        public Color CheckedBackColor
        {
            get => _checkedBackColor;
            set
            {
                if (_checkedBackColor == value) return;
                _checkedBackColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "White")]
        public Color UncheckedBackColor
        {
            get => _uncheckedBackColor;
            set
            {
                if (_uncheckedBackColor == value) return;
                _uncheckedBackColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "White")]
        public Color CheckedForeColor
        {
            get => _checkedForeColor;
            set
            {
                if (_checkedForeColor == value) return;
                _checkedForeColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "DimGray")]
        public Color UncheckedForeColor
        {
            get => _uncheckedForeColor;
            set
            {
                if (_uncheckedForeColor == value) return;
                _uncheckedForeColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Gainsboro")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor == value) return;
                _borderColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(1f)]
        public float BorderThickness
        {
            get => _borderThickness;
            set
            {
                var v = value;
                if (v < 0f) v = 0f;
                if (Math.Abs(_borderThickness - v) < 0.0001f) return;
                _borderThickness = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(0.5f)]
        public float CornerRadiusRatio
        {
            get => _cornerRadiusRatio;
            set
            {
                var v = value;
                if (v < 0f) v = 0f;
                if (v > 0.5f) v = 0.5f;
                if (Math.Abs(_cornerRadiusRatio - v) < 0.0001f) return;
                _cornerRadiusRatio = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool ShadowEnabled
        {
            get => _shadowEnabled;
            set
            {
                if (_shadowEnabled == value) return;
                _shadowEnabled = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(70)]
        public int ShadowOpacity
        {
            get => _shadowOpacity;
            set
            {
                var v = ClampByte(value);
                if (_shadowOpacity == v) return;
                _shadowOpacity = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(2)]
        public int ShadowOffsetY
        {
            get => _shadowOffsetY;
            set
            {
                var v = value;
                if (v < 0) v = 0;
                if (_shadowOffsetY == v) return;
                _shadowOffsetY = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool InnerBevelEnabled
        {
            get => _innerBevelEnabled;
            set
            {
                if (_innerBevelEnabled == value) return;
                _innerBevelEnabled = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(70)]
        public int InnerBevelOpacity
        {
            get => _innerBevelOpacity;
            set
            {
                var v = ClampByte(value);
                if (_innerBevelOpacity == v) return;
                _innerBevelOpacity = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool UseParentBackColor
        {
            get => _useParentBackColor;
            set
            {
                if (_useParentBackColor == value) return;
                _useParentBackColor = value;
                Invalidate();
            }
        }

        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = false; Invalidate(); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left || !Enabled)
                return;

            Focus();
            Capture = true;
            _mouseDown = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Left)
                return;

            var wasDown = _mouseDown;
            _mouseDown = false;
            Capture = false;

            if (wasDown && Enabled && ClientRectangle.Contains(e.Location))
            {
                if (ToggleOnClick)
                    Checked = !Checked;

                OnClick(EventArgs.Empty);
            }

            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!Enabled)
                return;

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                if (!_keyDown)
                {
                    _keyDown = true;
                    Invalidate();
                }

                e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (!Enabled)
                return;

            if (_keyDown && (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter))
            {
                _keyDown = false;

                if (ToggleOnClick)
                    Checked = !Checked;

                OnClick(EventArgs.Empty);
                Invalidate();

                e.Handled = true;
            }
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _keyDown = false;
            _mouseDown = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var parentBack = _useParentBackColor && Parent is not null ? Parent.BackColor : BackColor;

            using (var bg = new SolidBrush(parentBack))
                g.FillRectangle(bg, ClientRectangle);

            var r = ClientRectangle;
            if (r.Width <= 1 || r.Height <= 1)
                return;

            var pressedVisual = (_checked || _mouseDown || _keyDown);

            var back = _checked ? _checkedBackColor : _uncheckedBackColor;
            var fore = _checked ? _checkedForeColor : _uncheckedForeColor;

            if (!Enabled)
            {
                back = Blend(back, parentBack, 0.55f);
                fore = Blend(fore, parentBack, 0.55f);
            }
            else if (_hover && !pressedVisual)
            {
                back = Blend(back, Color.White, 0.10f);
            }

            var pillRect = Rectangle.Inflate(r, -1, -1);

            var effShadowY = 0;
            if (_shadowEnabled && _shadowOffsetY > 0)
            {
                var cap = Math.Max(1, pillRect.Height / 12);
                effShadowY = Math.Min(_shadowOffsetY, cap);
            }

            var shadowRect = pillRect;
            shadowRect.Offset(0, effShadowY);

            var radius = ComputeRadius(pillRect, _cornerRadiusRatio);

            if (_shadowEnabled && effShadowY > 0 && Enabled && !pressedVisual && _shadowOpacity > 0)
            {
                using var shadowPath = CreateRoundRectPath(shadowRect, radius);
                using var shadowBrush = new SolidBrush(Color.FromArgb(_shadowOpacity, Color.Black));
                g.FillPath(shadowBrush, shadowPath);
            }

            using var path = CreateRoundRectPath(pillRect, radius);

            var gradTop = pressedVisual ? Blend(back, Color.Black, 0.10f) : Blend(back, Color.White, 0.16f);
            var gradBot = pressedVisual ? Blend(back, Color.White, 0.10f) : Blend(back, Color.Black, 0.08f);

            using (var fill = new LinearGradientBrush(pillRect, gradTop, gradBot, LinearGradientMode.Vertical))
                g.FillPath(fill, path);

            if (_borderThickness > 0f)
            {
                var bTop = pressedVisual ? Blend(_borderColor, Color.Black, 0.12f) : Blend(_borderColor, Color.White, 0.20f);
                var bBot = pressedVisual ? Blend(_borderColor, Color.White, 0.12f) : Blend(_borderColor, Color.Black, 0.10f);

                using var borderBrush = new LinearGradientBrush(pillRect, bTop, bBot, LinearGradientMode.Vertical);
                using var pen = new Pen(borderBrush, _borderThickness) { Alignment = PenAlignment.Inset };
                g.DrawPath(pen, path);
            }

            if (_innerBevelEnabled && _innerBevelOpacity > 0)
            {
                var insetPx = pillRect.Height < 28 ? 1 : 2;
                var inset = Rectangle.Inflate(pillRect, -insetPx, -insetPx);

                if (inset.Width > 4 && inset.Height > 4)
                {
                    var insetRadius = ComputeRadius(inset, _cornerRadiusRatio);
                    using var innerPath = CreateRoundRectPath(inset, insetRadius);

                    var a = _innerBevelOpacity;
                    var innerTop = pressedVisual ? Color.FromArgb(a, 0, 0, 0) : Color.FromArgb(a, 255, 255, 255);
                    var innerBot = pressedVisual ? Color.FromArgb(a, 255, 255, 255) : Color.FromArgb(a, 0, 0, 0);

                    using var innerBrush = new LinearGradientBrush(inset, innerTop, innerBot, LinearGradientMode.Vertical);
                    using var innerPen = new Pen(innerBrush, 1f) { Alignment = PenAlignment.Inset };
                    g.DrawPath(innerPen, innerPath);
                }
            }

            if (_blackOutlineEnabled && _blackOutlineThickness > 0f)
            {
                using var outlinePen = new Pen(_blackOutlineColor, _blackOutlineThickness) { Alignment = PenAlignment.Inset };
                g.DrawPath(outlinePen, path);
            }

            var pad = GetEffectivePadding(pillRect, Padding);

            var textRect = new Rectangle(
                pillRect.X + pad.Left,
                pillRect.Y + pad.Top,
                Math.Max(1, pillRect.Width - pad.Left - pad.Right),
                Math.Max(1, pillRect.Height - pad.Top - pad.Bottom));

            if (pressedVisual && Enabled && pillRect.Height >= 26)
                textRect.Offset(0, 1);

            TextRenderer.DrawText(
                g,
                Text ?? string.Empty,
                Font,
                textRect,
                fore,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix);

            if (Focused && ShowFocusCues)
            {
                var focusInset = pillRect.Height < 28 ? 2 : 4;
                var focusRect = Rectangle.Inflate(pillRect, -focusInset, -focusInset);

                if (focusRect.Width > 4 && focusRect.Height > 4)
                    ControlPaint.DrawFocusRectangle(g, focusRect);
            }
        }
        private static Padding GetEffectivePadding(Rectangle pillRect, Padding desired)
        {
            var wAvail = Math.Max(1, pillRect.Width - 6);
            var hAvail = Math.Max(1, pillRect.Height - 6);

            var padL = Math.Max(0, Math.Min(desired.Left, wAvail / 2));
            var padR = Math.Max(0, Math.Min(desired.Right, wAvail / 2));
            var padT = Math.Max(0, Math.Min(desired.Top, hAvail / 2));
            var padB = Math.Max(0, Math.Min(desired.Bottom, hAvail / 2));

            var sumX = padL + padR;
            if (sumX > wAvail && sumX > 0)
            {
                var s = wAvail / (float)sumX;
                padL = (int)Math.Floor(padL * s);
                padR = wAvail - padL;
            }

            var sumY = padT + padB;
            if (sumY > hAvail && sumY > 0)
            {
                var s = hAvail / (float)sumY;
                padT = (int)Math.Floor(padT * s);
                padB = hAvail - padT;
            }

            var minX = pillRect.Height < 28 ? 6 : 8;
            var minY = pillRect.Height < 28 ? 2 : 4;

            padL = Math.Min(padL, wAvail - 1);
            padR = Math.Min(padR, wAvail - 1);
            padT = Math.Min(padT, hAvail - 1);
            padB = Math.Min(padB, hAvail - 1);

            if (pillRect.Height < 28)
            {
                padL = Math.Min(padL, minX);
                padR = Math.Min(padR, minX);
                padT = Math.Min(padT, minY);
                padB = Math.Min(padB, minY);
            }

            return new Padding(padL, padT, padR, padB);
        }

        private static int ComputeRadius(Rectangle rect, float ratio)
        {
            var min = Math.Min(rect.Width, rect.Height);
            if (min <= 0) return 0;

            if (ratio < 0f) ratio = 0f;
            if (ratio > 0.5f) ratio = 0.5f;

            var r = (int)Math.Round(min * ratio);
            var max = Math.Max(1, (min / 2));

            if (r < 1) r = 1;
            if (r > max) r = max;

            return r;
        }

        private static GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 1)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            int d = radius * 2;
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

        private static Color Blend(Color a, Color b, float t)
        {
            if (t <= 0f) return a;
            if (t >= 1f) return b;

            int ar = a.R, ag = a.G, ab = a.B, aa = a.A;
            int br = b.R, bg = b.G, bb = b.B, ba = b.A;

            int r = ar + (int)((br - ar) * t);
            int g = ag + (int)((bg - ag) * t);
            int bl = ab + (int)((bb - ab) * t);
            int al = aa + (int)((ba - aa) * t);

            if (r < 0) r = 0; else if (r > 255) r = 255;
            if (g < 0) g = 0; else if (g > 255) g = 255;
            if (bl < 0) bl = 0; else if (bl > 255) bl = 255;
            if (al < 0) al = 0; else if (al > 255) al = 255;

            return Color.FromArgb(al, r, g, bl);
        }

        private static int ClampByte(int v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }
    }
}
