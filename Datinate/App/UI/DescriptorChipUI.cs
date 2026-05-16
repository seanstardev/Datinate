using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;

namespace datinate.app
{
    public sealed class DescriptorChipUI : Control
    {
        public static class Defaults
        {
            public const int TagFixedWidth = 40;

            public const float TagBevelAngleDegrees = 22f;
            public const int TagBevelMinPx = 6;
            public const int TagBevelMaxPx = 28;

            public const float TagLeftBevelScale = 1.00f;
            public const float TagRightBevelScale = 1.00f;

            public const int OuterPadX = 3;
            public const int OuterPadY = 3;

            public const int GapAfterCheckbox = 2;
            public const int GapAfterTag = 3;

            public const int TagPadX = 8;
            public const int TagPadY = 4;

            public const int ShortCodeMaxLen = 2;
            public const float ShortCodeFontScale = 1.18f;

            public const int HighlightCornerRadius = 6;
            public const int HighlightInset = 1;

            public const float CheckedFillBlend = 0.14f;
            public const float CheckedBorderBlend = 0.28f;

            public const float HoverFillBlend = 0.08f;
            public const float HoverBorderBlend = 0.18f;

            public const TextFormatFlags CodeTextFlags =
                TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;

            public const TextFormatFlags DescTextFlags =
                TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;

            public const bool SheenDefault = true;

            public const int SheenTopAlpha = 70;
            public const int SheenBottomAlpha = 0;
            public const int SheenHeightPercent = 55;

            public const int InnerShadowTopAlpha = 0;
            public const int InnerShadowBottomAlpha = 55;
            public const int InnerShadowHeightPercent = 28;

            public const int SheenTopStrokeAlpha = 45;
        }

        private string code = "";
        private string description = "";
        private Color tagColor = Color.White;
        private bool isChecked;

        private bool isHotHover;

        private float tagBevelAngleDegrees = Defaults.TagBevelAngleDegrees;
        private int tagBevelMinPx = Defaults.TagBevelMinPx;
        private int tagBevelMaxPx = Defaults.TagBevelMaxPx;

        private float tagLeftBevelScale = Defaults.TagLeftBevelScale;
        private float tagRightBevelScale = Defaults.TagRightBevelScale;

        private int tagPadY = Defaults.TagPadY;

        private int tagFixedWidth = Defaults.TagFixedWidth;

        private int shortCodeMaxLen = Defaults.ShortCodeMaxLen;
        private float shortCodeFontScale = Defaults.ShortCodeFontScale;

        private int highlightCornerRadius = Defaults.HighlightCornerRadius;
        private int highlightInset = Defaults.HighlightInset;

        private int outerPadX = Defaults.OuterPadX;
        private int outerPadY = Defaults.OuterPadY;

        private int gapAfterCheckbox = Defaults.GapAfterCheckbox;
        private int gapAfterTag = Defaults.GapAfterTag;

        private float checkedFillBlend = Defaults.CheckedFillBlend;
        private float checkedBorderBlend = Defaults.CheckedBorderBlend;

        private float hoverFillBlend = Defaults.HoverFillBlend;
        private float hoverBorderBlend = Defaults.HoverBorderBlend;

        private bool useParentBackColor = true;
        private Control? lastParent;

        private bool sheen = Defaults.SheenDefault;

        private bool hideCheckbox;

        public event EventHandler? CheckedChanged;

        public DescriptorChipUI()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable, true);

            TabStop = true;

            ForeColor = SystemColors.ControlText;
            BackColor = SystemColors.Control;

            AutoSize = false;
            Size = GetPreferredSize(Size.Empty);

            Cursor = Cursors.Default;
        }

        private bool grayscale;

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool Grayscale
        {
            get => grayscale;
            set
            {
                if (grayscale == value)
                    return;

                grayscale = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(false)]
        public bool HideCheckbox
        {
            get => hideCheckbox;
            set
            {
                if (hideCheckbox == value)
                    return;

                hideCheckbox = value;

                if (hideCheckbox)
                {
                    if (isHotHover)
                        isHotHover = false;

                    if (!ReferenceEquals(Cursor, Cursors.Default))
                        Cursor = Cursors.Default;

                    TabStop = false;
                    SetStyle(ControlStyles.Selectable, false);
                }
                else
                {
                    TabStop = true;
                    SetStyle(ControlStyles.Selectable, true);
                }

                Invalidate();

                if (AutoSize)
                    PerformLayout();
                else
                    Size = GetPreferredSize(Size.Empty);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Size MinimumSize { get => _minimumSize; set => _minimumSize = value; }
        private Size _minimumSize = new Size(160, 20);

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(true)]
        public bool UseParentBackColor
        {
            get => useParentBackColor;
            set
            {
                if (useParentBackColor == value)
                    return;
                useParentBackColor = value;
                ApplyParentBackColor();
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(Defaults.SheenDefault)]
        public bool Sheen
        {
            get => sheen;
            set
            {
                if (sheen == value)
                    return;
                sheen = value;
                Invalidate();
            }
        }

        public static Bitmap RenderChevronAndCodeBitmap(string code, Color tagColor)
        {
            code ??= "";

            using var dpiProbeG = Graphics.FromHwnd(IntPtr.Zero);
            float dpiX = dpiProbeG.DpiX;
            float dpiY = dpiProbeG.DpiY;

            int cbSideFallback = (int)Math.Round(13f * (dpiX / 96f), MidpointRounding.AwayFromZero);
            if (cbSideFallback < 1) cbSideFallback = 1;

            Size cb = new Size(cbSideFallback, cbSideFallback);

            if (Application.RenderWithVisualStyles)
            {
                using var probeBmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                probeBmp.SetResolution(dpiX, dpiY);

                using var probeG = Graphics.FromImage(probeBmp);
                try
                {
                    cb = CheckBoxRenderer.GetGlyphSize(probeG, CheckBoxState.UncheckedNormal);
                }
                catch
                {
                    cb = new Size(cbSideFallback, cbSideFallback);
                }
            }

            var baseFont = Control.DefaultFont;

            int tagH = Math.Max(cb.Height, baseFont.Height) + (Defaults.TagPadY * 2);
            int tagW = Math.Max(0, Defaults.TagFixedWidth);

            if (tagW <= 0 || tagH <= 0)
            {
                var tiny = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                tiny.SetResolution(dpiX, dpiY);
                return tiny;
            }

            double radians = Defaults.TagBevelAngleDegrees * (Math.PI / 180.0);
            int bevelBase = (int)Math.Round(Math.Tan(radians) * tagH, MidpointRounding.AwayFromZero);
            bevelBase = Math.Clamp(bevelBase, Defaults.TagBevelMinPx, Defaults.TagBevelMaxPx);

            int bevelL = (int)Math.Round(bevelBase * Defaults.TagLeftBevelScale, MidpointRounding.AwayFromZero);
            bevelL = Math.Clamp(bevelL, 0, Defaults.TagBevelMaxPx);

            int bevelR = (int)Math.Round(bevelBase * Defaults.TagRightBevelScale, MidpointRounding.AwayFromZero);
            bevelR = Math.Clamp(bevelR, 0, Defaults.TagBevelMaxPx);

            var bmpOut = new Bitmap(tagW, tagH, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            bmpOut.SetResolution(dpiX, dpiY);

            using var g = Graphics.FromImage(bmpOut);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            var tagRect = new Rectangle(0, 0, tagW, tagH);

            using (var path = TagPath(tagRect, bevelL, bevelR))
            {
                using (var b = new SolidBrush(tagColor))
                    g.FillPath(b, path);

                if (Defaults.SheenDefault)
                    DrawTagSheenAndDepth(g, path, tagRect);
            }

            bool shortCode = code.Length > 0 && code.Length <= Defaults.ShortCodeMaxLen;
            float scale = shortCode ? Defaults.ShortCodeFontScale : 1.0f;
            float sizePt = baseFont.SizeInPoints * scale;

            using var codeFont = new Font(baseFont.FontFamily, sizePt, FontStyle.Bold, GraphicsUnit.Point, baseFont.GdiCharSet);

            var codeRect = new Rectangle(
                tagRect.X + bevelL,
                tagRect.Y,
                Math.Max(0, tagRect.Width - bevelL - bevelR),
                tagRect.Height);

            TextRenderer.DrawText(g, code, codeFont, codeRect, Color.White, Defaults.CodeTextFlags);

            return bmpOut;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (AutoSize)
            {
                Size sz = GetPreferredSize(Size.Empty);
                width = sz.Width;
                height = sz.Height;
            }

            base.SetBoundsCore(x, y, width, height, specified);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        private static Color ToGray(Color c)
        {
            int y = (int)Math.Round((0.299 * c.R) + (0.587 * c.G) + (0.114 * c.B));
            y = Math.Clamp(y, 0, 255);
            return Color.FromArgb(c.A, y, y, y);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (lastParent != null)
                lastParent.BackColorChanged -= Parent_BackColorChanged;

            lastParent = Parent;

            if (lastParent != null)
                lastParent.BackColorChanged += Parent_BackColorChanged;

            ApplyParentBackColor();
        }

        private void Parent_BackColorChanged(object? sender, EventArgs e)
        {
            ApplyParentBackColor();
            Invalidate();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            if (BackColor.A != 255)
                BackColor = Color.FromArgb(255, BackColor.R, BackColor.G, BackColor.B);

            base.OnBackColorChanged(e);
        }

        private void ApplyParentBackColor()
        {
            if (!useParentBackColor)
                return;

            if (Parent != null)
                BackColor = Parent.BackColor;
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        public string Code
        {
            get => code;
            set
            {
                value ??= "";
                if (string.Equals(code, value, StringComparison.Ordinal))
                    return;
                code = value;
                Invalidate();
                if (AutoSize) PerformLayout();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue("")]

        // This is the right-hand description text (designer-friendly).
        public override string Text
        {
            get => description;
            set
            {
                value ??= "";
                if (string.Equals(description, value, StringComparison.Ordinal))
                    return;

                description = value;
                base.Text = value;

                Invalidate();
                if (AutoSize) PerformLayout();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        public string Description
        {
            get => Text;
            set => Text = value;
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "White")]
        public Color TagColor
        {
            get => tagColor;
            set
            {
                if (tagColor.ToArgb() == value.ToArgb())
                    return;
                tagColor = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool Checked
        {
            get => isChecked;
            set
            {
                if (isChecked == value)
                    return;
                isChecked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Browsable(true)]
        [Category("Layout")]
        [DefaultValue(Defaults.TagFixedWidth)]
        public int TagFixedWidth
        {
            get => tagFixedWidth;
            set
            {
                tagFixedWidth = Math.Max(0, value);
                Invalidate();
                if (AutoSize) PerformLayout();
            }
        }

        [Browsable(true)]
        [Category("Highlight")]
        [DefaultValue(Defaults.CheckedFillBlend)]
        public float CheckedFillBlend
        {
            get => checkedFillBlend;
            set
            {
                checkedFillBlend = Math.Clamp(value, 0f, 1f);
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Highlight")]
        [DefaultValue(Defaults.CheckedBorderBlend)]
        public float CheckedBorderBlend
        {
            get => checkedBorderBlend;
            set
            {
                checkedBorderBlend = Math.Clamp(value, 0f, 1f);
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Highlight")]
        [DefaultValue(Defaults.HoverFillBlend)]
        public float HoverFillBlend
        {
            get => hoverFillBlend;
            set
            {
                hoverFillBlend = Math.Clamp(value, 0f, 1f);
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Highlight")]
        [DefaultValue(Defaults.HoverBorderBlend)]
        public float HoverBorderBlend
        {
            get => hoverBorderBlend;
            set
            {
                hoverBorderBlend = Math.Clamp(value, 0f, 1f);
                Invalidate();
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size cb = Size.Empty;

            if (!hideCheckbox)
            {
                int cbSide = ScalePx(13);
                cb = new Size(cbSide, cbSide);
            }

            string desc = string.IsNullOrEmpty(Text) ? " " : Text;
            Size descSz = TextRenderer.MeasureText(desc, Font, Size.Empty, Defaults.DescTextFlags);

            int tagH = Math.Max(cb.Height, Font.Height) + (tagPadY * 2);
            int tagW = Math.Max(0, tagFixedWidth);

            int height = Math.Max(cb.Height, tagH) + (outerPadY * 2);

            int width;
            if (hideCheckbox)
                width = outerPadX + tagW + gapAfterTag + descSz.Width + outerPadX;
            else
                width = outerPadX + cb.Width + gapAfterCheckbox + tagW + gapAfterTag + descSz.Width + outerPadX;

            if (MinimumSize.Width > 0) width = Math.Max(width, MinimumSize.Width);
            if (MinimumSize.Height > 0) height = Math.Max(height, MinimumSize.Height);

            return new Size(width, height);
        }

        private int ScalePx(int px)
            => (int)Math.Round(px * (DeviceDpi / 96f));

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            e.Graphics.Clear(BackColor);

            Size cb = Size.Empty;
            Rectangle cbRect = Rectangle.Empty;

            if (!hideCheckbox)
            {
                var checkState0 = isChecked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                cb = CheckBoxRenderer.GetGlyphSize(e.Graphics, checkState0);

                int cy0 = ClientRectangle.Y + (ClientRectangle.Height / 2);

                int cbX0 = outerPadX;
                int cbY0 = cy0 - (cb.Height / 2);
                cbRect = new Rectangle(cbX0, cbY0, cb.Width, cb.Height);
            }

            int cy = ClientRectangle.Y + (ClientRectangle.Height / 2);

            int tagH = Math.Max(cb.Height, Font.Height) + (tagPadY * 2);
            int tagY = cy - (tagH / 2);

            int bevelBase = ComputeBevelPx(tagH);
            int bevelL = ScaleBevel(bevelBase, tagLeftBevelScale);
            int bevelR = ScaleBevel(bevelBase, tagRightBevelScale);

            int tagX = hideCheckbox ? outerPadX : (cbRect.Right + gapAfterCheckbox);
            int tagW = Math.Max(0, tagFixedWidth);
            Rectangle tagRect = new Rectangle(tagX, tagY, tagW, tagH);

            Rectangle hotRect = hideCheckbox ? Rectangle.Empty : Rectangle.Union(cbRect, tagRect);

            bool gray = grayscale || !Enabled;
            Color effectiveTagColor = gray ? ToGray(tagColor) : tagColor;

            DrawStateBackground(e.Graphics, ClientRectangle, hotRect, effectiveTagColor);

            if (!hideCheckbox)
            {
                var checkState = isChecked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;

                if (Application.RenderWithVisualStyles)
                    CheckBoxRenderer.DrawCheckBox(e.Graphics, cbRect.Location, checkState);
                else
                    ControlPaint.DrawCheckBox(e.Graphics, cbRect, isChecked ? ButtonState.Checked : ButtonState.Normal);
            }

            using (var path = TagPath(tagRect, bevelL, bevelR))
            {
                using (var b = new SolidBrush(effectiveTagColor))
                    e.Graphics.FillPath(b, path);

                if (sheen)
                    DrawTagSheenAndDepth(e.Graphics, path, tagRect);
            }

            bool shortCode = code.Length > 0 && code.Length <= shortCodeMaxLen;
            using var codeFont = CreateCodeFont(shortCode);

            Rectangle codeRect = new Rectangle(tagRect.X + bevelL, tagRect.Y, Math.Max(0, tagRect.Width - bevelL - bevelR), tagRect.Height);
            TextRenderer.DrawText(e.Graphics, code, codeFont, codeRect, Color.White, Defaults.CodeTextFlags);

            int descX = tagRect.Right + gapAfterTag;
            Rectangle descRect = new Rectangle(descX, tagRect.Y, ClientRectangle.Right - outerPadX - descX, tagRect.Height);
            var effectiveFore = Enabled ? ForeColor : SystemColors.GrayText;
            TextRenderer.DrawText(e.Graphics, Text, Font, descRect, effectiveFore, Defaults.DescTextFlags);

            if (!hideCheckbox && Focused)
                ControlPaint.DrawFocusRectangle(e.Graphics, Inflate(ClientRectangle, -2, -2));
        }

        private static void DrawTagSheenAndDepth(Graphics g, GraphicsPath tagPath, Rectangle tagRect)
        {
            var state = g.Save();
            try
            {
                g.SetClip(tagPath, CombineMode.Replace);

                int sheenH = Math.Max(1, (tagRect.Height * Defaults.SheenHeightPercent) / 100);
                var sheenRect = new Rectangle(tagRect.X, tagRect.Y, tagRect.Width, sheenH);

                using (var lg = new LinearGradientBrush(
                           sheenRect,
                           Color.FromArgb(Defaults.SheenTopAlpha, 255, 255, 255),
                           Color.FromArgb(Defaults.SheenBottomAlpha, 255, 255, 255),
                           LinearGradientMode.Vertical))
                {
                    g.FillRectangle(lg, sheenRect);
                }

                int shadowH = Math.Max(1, (tagRect.Height * Defaults.InnerShadowHeightPercent) / 100);
                var shadowRect = new Rectangle(tagRect.X, tagRect.Bottom - shadowH, tagRect.Width, shadowH);

                using (var lg2 = new LinearGradientBrush(
                           shadowRect,
                           Color.FromArgb(Defaults.InnerShadowTopAlpha, 0, 0, 0),
                           Color.FromArgb(Defaults.InnerShadowBottomAlpha, 0, 0, 0),
                           LinearGradientMode.Vertical))
                {
                    g.FillRectangle(lg2, shadowRect);
                }
            }
            finally
            {
                g.Restore(state);
            }

            using (var topStroke = new Pen(Color.FromArgb(Defaults.SheenTopStrokeAlpha, 255, 255, 255), 1f))
            {
                var s = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPath(topStroke, tagPath);
                g.SmoothingMode = s;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (hideCheckbox)
            {
                if (isHotHover)
                {
                    isHotHover = false;
                    Invalidate();
                }

                if (!ReferenceEquals(Cursor, Cursors.Default))
                    Cursor = Cursors.Default;

                return;
            }

            Rectangle hotRect = GetHotRect();
            bool hot = hotRect.Contains(e.Location);

            if (hot != isHotHover)
            {
                isHotHover = hot;
                Invalidate();
            }

            var desired = hot ? Cursors.Hand : Cursors.Default;
            if (!ReferenceEquals(Cursor, desired))
                Cursor = desired;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hideCheckbox)
            {
                if (isHotHover)
                {
                    isHotHover = false;
                    Invalidate();
                }

                if (!ReferenceEquals(Cursor, Cursors.Default))
                    Cursor = Cursors.Default;

                return;
            }

            if (isHotHover)
            {
                isHotHover = false;
                Invalidate();
            }

            if (!ReferenceEquals(Cursor, Cursors.Default))
                Cursor = Cursors.Default;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (hideCheckbox)
                return;

            if (e.Button != MouseButtons.Left)
                return;

            if (!GetHotRect().Contains(e.Location))
                return;

            Focus();
            Checked = !Checked;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (hideCheckbox)
                return;

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                Checked = !Checked;
                e.Handled = true;
            }
        }

        private Rectangle GetHotRect()
        {
            if (hideCheckbox)
                return Rectangle.Empty;

            using var g = CreateGraphics();

            var checkState = isChecked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
            Size cb = CheckBoxRenderer.GetGlyphSize(g, checkState);

            int cy = ClientRectangle.Y + (ClientRectangle.Height / 2);

            int cbX = outerPadX;
            int cbY = cy - (cb.Height / 2);
            Rectangle cbRect = new Rectangle(cbX, cbY, cb.Width, cb.Height);

            int tagH = Math.Max(cb.Height, Font.Height) + (tagPadY * 2);
            int tagY = cy - (tagH / 2);

            int tagX = cbRect.Right + gapAfterCheckbox;
            int tagW = Math.Max(0, tagFixedWidth);
            Rectangle tagRect = new Rectangle(tagX, tagY, tagW, tagH);

            return Rectangle.Union(cbRect, tagRect);
        }

        private void DrawStateBackground(Graphics g, Rectangle fullRect, Rectangle hotRect, Color effectiveTagColor)
        {
            float fillBlend = 0f;
            float borderBlend = 0f;

            bool isHoverState = false;

            if (isChecked)
            {
                fillBlend = checkedFillBlend;
                borderBlend = checkedBorderBlend;
            }
            else if (!hideCheckbox && isHotHover)
            {
                isHoverState = true;
                fillBlend = hoverFillBlend;
                borderBlend = hoverBorderBlend;
            }

            if (fillBlend <= 0f && borderBlend <= 0f)
                return;

            Color fill = Blend(BackColor, effectiveTagColor, fillBlend);
            Color border = Blend(BackColor, effectiveTagColor, borderBlend);

            Rectangle target = isHoverState ? hotRect : fullRect;

            var hi = Inflate(target, -highlightInset, -highlightInset);
            using var path = RoundedRect(hi, highlightCornerRadius);

            if (fillBlend > 0f)
            {
                using var b = new SolidBrush(fill);
                g.FillPath(b, path);
            }

            if (borderBlend > 0f)
            {
                using var p = new Pen(border, 1f);
                g.DrawPath(p, path);
            }
        }

        private Font CreateCodeFont(bool shortCode)
        {
            float scale = shortCode ? shortCodeFontScale : 1.0f;
            float size = Font.SizeInPoints * scale;
            return new Font(Font.FontFamily, size, FontStyle.Bold, GraphicsUnit.Point, Font.GdiCharSet);
        }

        private int ComputeBevelPx(int tagHeight)
        {
            double radians = tagBevelAngleDegrees * (Math.PI / 180.0);
            int px = (int)Math.Round(Math.Tan(radians) * tagHeight);
            return Math.Clamp(px, tagBevelMinPx, tagBevelMaxPx);
        }

        private int ScaleBevel(int bevelBase, float scale)
        {
            int px = (int)Math.Round(bevelBase * scale);
            return Math.Clamp(px, 0, tagBevelMaxPx);
        }

        private static Color Blend(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            int r = (int)Math.Round(a.R + (b.R - a.R) * t);
            int g = (int)Math.Round(a.G + (b.G - a.G) * t);
            int bl = (int)Math.Round(a.B + (b.B - a.B) * t);
            return Color.FromArgb(255, r, g, bl);
        }

        private static Rectangle Inflate(Rectangle r, int dx, int dy)
            => new Rectangle(r.X + dx, r.Y + dy, r.Width - (dx * 2), r.Height - (dy * 2));

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(r);
                path.CloseFigure();
                return path;
            }

            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath TagPath(Rectangle r, int bevelLeftPx, int bevelRightPx)
        {
            var p = new GraphicsPath();
            Point a = new Point(r.Left + bevelLeftPx, r.Top);
            Point b = new Point(r.Right, r.Top);
            Point c = new Point(r.Right - bevelRightPx, r.Bottom);
            Point d = new Point(r.Left, r.Bottom);
            p.AddPolygon(new[] { a, b, c, d });
            p.CloseFigure();
            return p;
        }
    }
}
