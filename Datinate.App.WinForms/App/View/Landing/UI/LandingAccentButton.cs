using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    public sealed class LandingAccentButton : Button
    {
        private bool isHovering;
        private bool isPressed;
        private Color borderColor = Color.Empty;
        private Color disabledBackColor = Color.Empty;
        private Color disabledForeColor = Color.Empty;
        private Color disabledBorderColor = Color.Empty;
        private int cornerRadius = 3;

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color DisabledBackColor
        {
            get => disabledBackColor;
            set
            {
                disabledBackColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color DisabledForeColor
        {
            get => disabledForeColor;
            set
            {
                disabledForeColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color DisabledBorderColor
        {
            get => disabledBorderColor;
            set
            {
                disabledBorderColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(3)]
        public int CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        public LandingAccentButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.Selectable,
                true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovering = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovering = false;
            isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);

            if (mevent.Button == MouseButtons.Left)
            {
                isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            isPressed = false;
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            isHovering = false;
            isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pevent.Graphics.Clear(Parent?.BackColor ?? SystemColors.Control);

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            Color baseFill = ResolveBaseFill();
            Color topFill = AdjustBrightness(baseFill, Enabled ? 0.10f : 0.04f);
            Color bottomFill = AdjustBrightness(baseFill, Enabled ? -0.08f : -0.03f);
            Color currentBorderColor = ResolveBorderColor(baseFill);
            Color textColor = ResolveTextColor();

            using (GraphicsPath path = CreateRoundRectPath(rect, CornerRadius))
            using (LinearGradientBrush brush = new LinearGradientBrush(rect, topFill, bottomFill, LinearGradientMode.Vertical))
            using (Pen borderPen = new Pen(currentBorderColor))
            {
                pevent.Graphics.FillPath(brush, path);
                pevent.Graphics.DrawPath(borderPen, path);

                if (Enabled)
                {
                    using Pen highlightPen = new Pen(Color.FromArgb(46, Color.White));
                    pevent.Graphics.DrawLine(highlightPen, 2, 1, Width - 3, 1);
                }

                Rectangle textRect = Rectangle.Inflate(rect, -6, -3);
                TextRenderer.DrawText(
                    pevent.Graphics,
                    Text,
                    Font,
                    textRect,
                    textColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.SingleLine);
            }
        }

        private Color ResolveBaseFill()
        {
            if (!Enabled)
            {
                if (DisabledBackColor != Color.Empty)
                    return DisabledBackColor;

                return BlendColors(BackColor, Color.FromArgb(232, 232, 232), 0.45f);
            }

            if (isPressed)
                return AdjustBrightness(BackColor, -0.12f);

            if (isHovering)
                return AdjustBrightness(BackColor, 0.06f);

            return BackColor;
        }

        private Color ResolveTextColor()
        {
            if (!Enabled)
            {
                if (DisabledForeColor != Color.Empty)
                    return DisabledForeColor;

                return BlendColors(ForeColor, Color.FromArgb(150, 150, 150), 0.30f);
            }

            return ForeColor;
        }

        private Color ResolveBorderColor(Color fillColor)
        {
            if (!Enabled)
            {
                if (DisabledBorderColor != Color.Empty)
                    return DisabledBorderColor;

                return AdjustBrightness(fillColor, -0.18f);
            }

            if (BorderColor != Color.Empty)
                return BorderColor;

            return AdjustBrightness(fillColor, -0.24f);
        }

        private static Color AdjustBrightness(Color color, float amount)
        {
            if (amount >= 0f)
                return BlendColors(color, Color.White, amount);

            return BlendColors(color, Color.Black, -amount);
        }

        private static Color BlendColors(Color a, Color b, float amount)
        {
            amount = Math.Clamp(amount, 0f, 1f);

            int r = (int)(a.R + ((b.R - a.R) * amount));
            int g = (int)(a.G + ((b.G - a.G) * amount));
            int bValue = (int)(a.B + ((b.B - a.B) * amount));

            return Color.FromArgb(255, r, g, bValue);
        }

        private static GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            int diameter = radius * 2;

            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);

            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
