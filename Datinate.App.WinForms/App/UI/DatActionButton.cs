using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace datinate.app
{
    [DefaultEvent(nameof(Click))]
    [DefaultProperty(nameof(Text))]
    [ToolboxItem(true)]
    public class DatActionButton : Control
    {
        private bool isHovering;
        private bool isPressed;

        private Image? buttonImage;
        private bool useIcon = true;
        private int cornerRadiusTopLeft = 12;
        private int cornerRadiusTopRight = 12;
        private int cornerRadiusBottomRight = 12;
        private int cornerRadiusBottomLeft = 12;
        private int minimumFontSize = 8;
        private int maximumFontSize = 30;
        private bool showFocusCue = true;
        private bool bannerMode;

        public DatActionButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.Selectable,
                true);

            Size = new Size(260, 64);
            MinimumSize = new Size(30, 28);
            Font = new Font("Segoe UI Semibold", 18f, FontStyle.Regular, GraphicsUnit.Point);
            ForeColor = Color.White;
            BackColor = Color.FromArgb(35, 152, 220);
            Cursor = Cursors.Hand;
            TabStop = true;
            Text = "DAT Action";
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                if (base.BackColor == value)
                    return;

                base.BackColor = value;
                Invalidate();
            }
        }

        protected override Size DefaultSize => new Size(260, 64);

        [Category("Appearance")]
        [DefaultValue(null)]
        public Image? ButtonImage
        {
            get => buttonImage;
            set
            {
                if (ReferenceEquals(buttonImage, value))
                    return;

                buttonImage = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool UseIcon
        {
            get => useIcon;
            set
            {
                if (useIcon == value)
                    return;

                useIcon = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(12)]
        public int CornerRadius
        {
            get => cornerRadiusTopLeft;
            set
            {
                value = Math.Max(0, value);

                if (cornerRadiusTopLeft == value &&
                    cornerRadiusTopRight == value &&
                    cornerRadiusBottomRight == value &&
                    cornerRadiusBottomLeft == value)
                    return;

                cornerRadiusTopLeft = value;
                cornerRadiusTopRight = value;
                cornerRadiusBottomRight = value;
                cornerRadiusBottomLeft = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(12)]
        public int CornerRadiusTopLeft
        {
            get => cornerRadiusTopLeft;
            set
            {
                value = Math.Max(0, value);
                if (cornerRadiusTopLeft == value)
                    return;

                cornerRadiusTopLeft = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(12)]
        public int CornerRadiusTopRight
        {
            get => cornerRadiusTopRight;
            set
            {
                value = Math.Max(0, value);
                if (cornerRadiusTopRight == value)
                    return;

                cornerRadiusTopRight = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(12)]
        public int CornerRadiusBottomRight
        {
            get => cornerRadiusBottomRight;
            set
            {
                value = Math.Max(0, value);
                if (cornerRadiusBottomRight == value)
                    return;

                cornerRadiusBottomRight = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(12)]
        public int CornerRadiusBottomLeft
        {
            get => cornerRadiusBottomLeft;
            set
            {
                value = Math.Max(0, value);
                if (cornerRadiusBottomLeft == value)
                    return;

                cornerRadiusBottomLeft = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(8)]
        public int MinimumFontSize
        {
            get => minimumFontSize;
            set
            {
                value = Math.Max(1, value);
                if (minimumFontSize == value)
                    return;

                minimumFontSize = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(30)]
        public int MaximumFontSize
        {
            get => maximumFontSize;
            set
            {
                value = Math.Max(1, value);
                if (maximumFontSize == value)
                    return;

                maximumFontSize = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool ShowFocusCue
        {
            get => showFocusCue;
            set
            {
                if (showFocusCue == value)
                    return;

                showFocusCue = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool BannerMode
        {
            get => bannerMode;
            set
            {
                if (bannerMode == value)
                    return;

                bannerMode = value;
                isHovering = false;
                isPressed = false;
                Cursor = bannerMode ? Cursors.Default : Cursors.Hand;
                TabStop = !bannerMode;
                Invalidate();
            }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        [DefaultValue("DAT Action")]
        public override string Text
        {
            get => base.Text;
            set
            {
                if (base.Text == value)
                    return;

                base.Text = value;
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Parent != null)
            {
                GraphicsState state = e.Graphics.Save();
                try
                {
                    e.Graphics.TranslateTransform(-Left, -Top);
                    using PaintEventArgs parentArgs = new PaintEventArgs(e.Graphics, Parent.ClientRectangle);
                    InvokePaintBackground(Parent, parentArgs);
                    InvokePaint(Parent, parentArgs);
                }
                finally
                {
                    e.Graphics.Restore(state);
                }

                return;
            }

            using SolidBrush backBrush = new SolidBrush(SystemColors.Control);
            e.Graphics.FillRectangle(backBrush, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Rectangle rect = ClientRectangle;
            if (rect.Width <= 1 || rect.Height <= 1)
                return;

            const int shadowDepth = 3;

            Rectangle buttonRect = new Rectangle(
                rect.X,
                rect.Y,
                Math.Max(1, rect.Width - 1),
                Math.Max(1, rect.Height - shadowDepth - 1));

            if (isPressed)
                buttonRect.Offset(0, 1);

            Rectangle paintRect = Rectangle.Inflate(buttonRect, -1, -1);
            if (paintRect.Width <= 1 || paintRect.Height <= 1)
                return;

            int topLeft = cornerRadiusTopLeft;
            int topRight = cornerRadiusTopRight;
            int bottomRight = cornerRadiusBottomRight;
            int bottomLeft = cornerRadiusBottomLeft;

            Color visualBase = GetVisualBaseColor();
            Color top = Lighten(visualBase, 32);
            Color upperMid = Lighten(visualBase, 10);
            Color lowerMid = Darken(visualBase, 9);
            Color bottom = Darken(visualBase, 28);
            Color border = Darken(visualBase, 48);
            Color innerBorder = Color.FromArgb(145, Lighten(visualBase, 85));
            Color focusColor = Color.FromArgb(180, 255, 255, 255);

            DrawShadow(e.Graphics, paintRect, topLeft, topRight, bottomRight, bottomLeft, shadowDepth);

            using GraphicsPath buttonPath = CreateRoundRectPath(paintRect, topLeft, topRight, bottomRight, bottomLeft);
            using LinearGradientBrush fillBrush = new LinearGradientBrush(paintRect, top, bottom, LinearGradientMode.Vertical);
            fillBrush.InterpolationColors = new ColorBlend
            {
                Colors = new[] { top, upperMid, lowerMid, bottom },
                Positions = new[] { 0f, 0.44f, 0.60f, 1f }
            };
            e.Graphics.FillPath(fillBrush, buttonPath);

            Rectangle glossRect = new Rectangle(
                paintRect.X,
                paintRect.Y,
                paintRect.Width,
                Math.Max(1, (int)Math.Round(paintRect.Height * 0.46)));

            using (GraphicsPath glossPath = CreateTopRoundRectPath(glossRect, topLeft, topRight))
            {
                GraphicsState glossState = e.Graphics.Save();
                e.Graphics.SetClip(buttonPath);
                using LinearGradientBrush glossBrush = new LinearGradientBrush(
                    glossRect,
                    Color.FromArgb(72, 255, 255, 255),
                    Color.FromArgb(6, 255, 255, 255),
                    LinearGradientMode.Vertical);
                e.Graphics.FillPath(glossBrush, glossPath);
                e.Graphics.Restore(glossState);
            }

            using (Pen borderPen = new Pen(border, 1f))
                e.Graphics.DrawPath(borderPen, buttonPath);

            Rectangle innerRect = Rectangle.Inflate(paintRect, -2, -2);
            if (innerRect.Width > 2 && innerRect.Height > 2)
            {
                using GraphicsPath innerPath = CreateRoundRectPath(
                    innerRect,
                    Math.Max(0, topLeft - 2),
                    Math.Max(0, topRight - 2),
                    Math.Max(0, bottomRight - 2),
                    Math.Max(0, bottomLeft - 2));

                using Pen innerPen = new Pen(innerBorder, 1f);
                e.Graphics.DrawPath(innerPen, innerPath);
            }

            DrawContent(e.Graphics, paintRect);

            if (!Enabled)
            {
                using SolidBrush disabledBrush = new SolidBrush(Color.FromArgb(105, 235, 235, 235));
                e.Graphics.FillPath(disabledBrush, buttonPath);
            }

            if (!bannerMode && Focused && showFocusCue)
                DrawFocusCue(e.Graphics, paintRect, topLeft, topRight, bottomRight, bottomLeft, focusColor);
        }

        private void DrawContent(Graphics graphics, Rectangle paintRect)
        {
            const int horizontalPadding = 8;
            const int verticalPadding = 4;
            const int iconTextGap = 8;

            Rectangle contentRect = new Rectangle(
                paintRect.X + horizontalPadding,
                paintRect.Y + verticalPadding,
                Math.Max(1, paintRect.Width - (horizontalPadding * 2)),
                Math.Max(1, paintRect.Height - (verticalPadding * 2)));

            bool hasImage = useIcon && buttonImage != null;
            bool hasVisibleText = !string.IsNullOrWhiteSpace(Text);

            if (!hasImage)
            {
                if (!hasVisibleText)
                    return;

                using Font drawFont_ = CreateBestFitFont(graphics, Text, contentRect.Size);
                DrawButtonText(graphics, drawFont_, contentRect, Text, true);
                return;
            }

            Size iconSize = GetIconSize(buttonImage!, contentRect);

            if (!hasVisibleText)
            {
                Rectangle iconOnlyRect = new Rectangle(
                    contentRect.X + ((contentRect.Width - iconSize.Width) / 2),
                    contentRect.Y + ((contentRect.Height - iconSize.Height) / 2),
                    iconSize.Width,
                    iconSize.Height);

                DrawIcon(graphics, buttonImage!, iconOnlyRect, Enabled ? 1f : 0.55f);
                return;
            }

            Size textArea = new Size(
                Math.Max(1, contentRect.Width - iconSize.Width - iconTextGap),
                contentRect.Height);

            using Font drawFont = CreateBestFitFont(graphics, Text, textArea);
            Size textSize = MeasureText(drawFont, Text);

            int groupWidth = iconSize.Width + iconTextGap + textSize.Width;
            int startX = contentRect.X + Math.Max(0, (contentRect.Width - groupWidth) / 2);

            Rectangle iconRect = new Rectangle(
                startX,
                contentRect.Y + ((contentRect.Height - iconSize.Height) / 2),
                iconSize.Width,
                iconSize.Height);

            Rectangle textRect = new Rectangle(
                iconRect.Right + iconTextGap,
                contentRect.Y,
                Math.Max(1, textSize.Width),
                contentRect.Height);

            DrawIcon(graphics, buttonImage!, iconRect, Enabled ? 1f : 0.55f);
            DrawButtonText(graphics, drawFont, textRect, Text, false);
        }

        private void DrawIcon(Graphics graphics, Image image, Rectangle bounds, float opacity)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            if (Math.Abs(opacity - 1f) < 0.0001f)
            {
                graphics.DrawImage(image, bounds);
                return;
            }

            using ImageAttributes attributes = new ImageAttributes();
            ColorMatrix matrix = new ColorMatrix
            {
                Matrix33 = opacity
            };
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            graphics.DrawImage(
                image,
                bounds,
                0,
                0,
                image.Width,
                image.Height,
                GraphicsUnit.Pixel,
                attributes);
        }

        private void DrawButtonText(Graphics graphics, Font font, Rectangle textRect, string text, bool centerHorizontally)
        {
            TextFormatFlags flags =
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                (centerHorizontally ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Left);

            Rectangle shadowRect = textRect;
            shadowRect.Offset(0, 1);
            TextRenderer.DrawText(graphics, text, font, shadowRect, Color.FromArgb(72, 0, 0, 0), flags);

            Color finalTextColor = Enabled
                ? GetResolvedTextColor(ForeColor)
                : Color.FromArgb(218, 218, 218);

            TextRenderer.DrawText(graphics, text, font, textRect, finalTextColor, flags);
        }

        private static Color GetResolvedTextColor(Color color)
        {
            if (color.R >= 248 && color.G >= 248 && color.B >= 248)
                return Color.FromArgb(color.A, 245, 247, 250);

            return color;
        }

        private void DrawFocusCue(Graphics graphics, Rectangle rect, int topLeft, int topRight, int bottomRight, int bottomLeft, Color focusColor)
        {
            Rectangle focusRect = Rectangle.Inflate(rect, -6, -6);
            if (focusRect.Width <= 4 || focusRect.Height <= 4)
                return;

            using GraphicsPath focusPath = CreateRoundRectPath(
                focusRect,
                Math.Max(0, topLeft - 5),
                Math.Max(0, topRight - 5),
                Math.Max(0, bottomRight - 5),
                Math.Max(0, bottomLeft - 5));

            using Pen pen = new Pen(focusColor) { DashStyle = DashStyle.Dash };
            graphics.DrawPath(pen, focusPath);
        }

        private void DrawShadow(Graphics graphics, Rectangle rect, int topLeft, int topRight, int bottomRight, int bottomLeft, int depth)
        {
            Rectangle shadowRect = rect;
            shadowRect.Offset(0, depth);

            using GraphicsPath shadowPath = CreateRoundRectPath(shadowRect, topLeft, topRight, bottomRight, bottomLeft);
            using PathGradientBrush shadowBrush = new PathGradientBrush(shadowPath)
            {
                CenterColor = Color.FromArgb(56, 0, 0, 0),
                SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) }
            };

            GraphicsState state = graphics.Save();
            graphics.SetClip(shadowRect);
            graphics.FillPath(shadowBrush, shadowPath);
            graphics.Restore(state);
        }

        private Size GetIconSize(Image image, Rectangle contentRect)
        {
            int maxHeight = Math.Max(1, contentRect.Height);
            int maxWidth = Math.Max(maxHeight, Math.Min(contentRect.Width / 3, contentRect.Height * 2));

            double ratioX = (double)maxWidth / image.Width;
            double ratioY = (double)maxHeight / image.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int width = Math.Max(1, (int)Math.Round(image.Width * ratio));
            int height = Math.Max(1, (int)Math.Round(image.Height * ratio));
            return new Size(width, height);
        }

        private Font CreateBestFitFont(Graphics graphics, string text, Size availableSize)
        {
            int min = Math.Max(1, minimumFontSize);
            int max = Math.Max(min, maximumFontSize);

            float low = min;
            float high = max;
            float best = min;

            for (int i = 0; i < 12; i++)
            {
                float mid = (low + high) / 2f;
                using Font testFont = new Font(Font.FontFamily, mid, Font.Style, GraphicsUnit.Point);
                Size measured = MeasureText(testFont, text);

                if (measured.Width <= availableSize.Width && measured.Height <= availableSize.Height)
                {
                    best = mid;
                    low = mid;
                }
                else
                {
                    high = mid;
                }
            }

            return new Font(Font.FontFamily, Math.Max(min, Math.Min(max, best)), Font.Style, GraphicsUnit.Point);
        }

        private static Size MeasureText(Font font, string text)
        {
            if (string.IsNullOrEmpty(text))
                return Size.Empty;

            return TextRenderer.MeasureText(
                text,
                font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        }

        private Color GetVisualBaseColor()
        {
            Color color = BackColor;

            if (!bannerMode && Enabled && isHovering)
                color = Lighten(color, 10);

            if (!bannerMode && Enabled && isPressed)
                color = Darken(color, 14);

            return color;
        }

        private static Color Lighten(Color color, int amount)
        {
            int r = color.R + ((255 - color.R) * amount / 100);
            int g = color.G + ((255 - color.G) * amount / 100);
            int b = color.B + ((255 - color.B) * amount / 100);
            return Color.FromArgb(color.A, Clamp255(r), Clamp255(g), Clamp255(b));
        }

        private static Color Darken(Color color, int amount)
        {
            int r = color.R - (color.R * amount / 100);
            int g = color.G - (color.G * amount / 100);
            int b = color.B - (color.B * amount / 100);
            return Color.FromArgb(color.A, Clamp255(r), Clamp255(g), Clamp255(b));
        }

        private static int Clamp255(int value)
        {
            if (value < 0)
                return 0;

            if (value > 255)
                return 255;

            return value;
        }

        private static GraphicsPath CreateRoundRectPath(Rectangle rect, int topLeft, int topRight, int bottomRight, int bottomLeft)
        {
            GraphicsPath path = new GraphicsPath();

            if (rect.Width <= 0 || rect.Height <= 0)
                return path;

            NormalizeCornerRadii(rect, ref topLeft, ref topRight, ref bottomRight, ref bottomLeft);

            if (topLeft == 0 && topRight == 0 && bottomRight == 0 && bottomLeft == 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            int left = rect.Left;
            int top = rect.Top;
            int right = rect.Right;
            int bottom = rect.Bottom;

            path.StartFigure();

            path.AddLine(left + topLeft, top, right - topRight, top);

            if (topRight > 0)
                path.AddArc(right - (topRight * 2), top, topRight * 2, topRight * 2, 270, 90);

            path.AddLine(right, top + topRight, right, bottom - bottomRight);

            if (bottomRight > 0)
                path.AddArc(right - (bottomRight * 2), bottom - (bottomRight * 2), bottomRight * 2, bottomRight * 2, 0, 90);

            path.AddLine(right - bottomRight, bottom, left + bottomLeft, bottom);

            if (bottomLeft > 0)
                path.AddArc(left, bottom - (bottomLeft * 2), bottomLeft * 2, bottomLeft * 2, 90, 90);

            path.AddLine(left, bottom - bottomLeft, left, top + topLeft);

            if (topLeft > 0)
                path.AddArc(left, top, topLeft * 2, topLeft * 2, 180, 90);

            path.CloseFigure();
            return path;
        }

        private static void NormalizeCornerRadii(Rectangle rect, ref int topLeft, ref int topRight, ref int bottomRight, ref int bottomLeft)
        {
            int maxRadius = Math.Min(rect.Width, rect.Height) / 2;

            topLeft = Math.Max(0, Math.Min(topLeft, maxRadius));
            topRight = Math.Max(0, Math.Min(topRight, maxRadius));
            bottomRight = Math.Max(0, Math.Min(bottomRight, maxRadius));
            bottomLeft = Math.Max(0, Math.Min(bottomLeft, maxRadius));

            double scale = 1.0;
            scale = Math.Min(scale, GetCornerScale(rect.Width, topLeft, topRight));
            scale = Math.Min(scale, GetCornerScale(rect.Width, bottomLeft, bottomRight));
            scale = Math.Min(scale, GetCornerScale(rect.Height, topLeft, bottomLeft));
            scale = Math.Min(scale, GetCornerScale(rect.Height, topRight, bottomRight));

            if (scale >= 1.0)
                return;

            topLeft = (int)Math.Round(topLeft * scale);
            topRight = (int)Math.Round(topRight * scale);
            bottomRight = (int)Math.Round(bottomRight * scale);
            bottomLeft = (int)Math.Round(bottomLeft * scale);
        }

        private static double GetCornerScale(int limit, int first, int second)
        {
            int sum = first + second;
            if (sum <= 0 || sum <= limit)
                return 1.0;

            return (double)limit / sum;
        }

        private static GraphicsPath CreateTopRoundRectPath(Rectangle rect, int topLeft, int topRight)
        {
            return CreateRoundRectPath(rect, topLeft, topRight, 0, 0);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (bannerMode)
                return;

            base.OnMouseEnter(e);
            isHovering = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (bannerMode)
                return;

            base.OnMouseLeave(e);
            isHovering = false;
            isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (bannerMode)
                return;

            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                Focus();
                isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (bannerMode)
                return;

            base.OnMouseUp(e);

            if (isPressed)
            {
                isPressed = false;
                Invalidate();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (bannerMode)
                return;

            base.OnClick(e);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            if (bannerMode)
                return;

            base.OnDoubleClick(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            if (bannerMode)
                return;

            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            isPressed = false;
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (bannerMode)
                return false;

            Keys key = keyData & Keys.KeyCode;
            if (key == Keys.Space || key == Keys.Enter)
                return true;

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (bannerMode)
            {
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                isPressed = true;
                Invalidate();
                e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (bannerMode)
            {
                e.Handled = true;
                return;
            }

            base.OnKeyUp(e);

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                isPressed = false;
                Invalidate();
                OnClick(EventArgs.Empty);
                e.Handled = true;
            }
        }
    }
}
