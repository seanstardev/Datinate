using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    public static partial class DragPromptOverlayRenderer
    {
        private const int EmptyOverlayCardMinWidth = 360;
        private const int EmptyOverlayCardMaxWidth = 560;

        private const int EmptyOverlayCardRadius = 16;

        private const int EmptyOverlayInnerPad = 16;

        private const int EmptyOverlayDashInset = 12;

        private const int EmptyOverlayShadowAlpha = 36;
        private const int EmptyOverlayCardAlpha = 235;
        private const int EmptyOverlayBorderAlpha = 80;

        private static readonly Color EmptyOverlayBorderColor = Color.FromArgb(60, 60, 60);
        private static readonly Color EmptyOverlayDashColor = Color.FromArgb(90, 90, 90, 90);

        private static readonly Color EmptyOverlayTitleColor = Color.FromArgb(35, 35, 35);
        private static readonly Color EmptyOverlayHintColor = Color.FromArgb(90, 90, 90);

        private static readonly TextFormatFlags EmptyOverlayTitleFlags =
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

        private static readonly TextFormatFlags EmptyOverlayHintFlags =
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

        private static Font? cachedTitleFont;
        private static Font? cachedHintFont;
        private static string? cachedFontSig;

        public static Control CreateOverlayControl(
            Control source,
            string title,
            string? hint = null,
            int offsetX = 0,
            int offsetY = 0)
        {
            return new DragPromptOverlayControl
            {
                Source = source,
                Title = title,
                Hint = hint,

                OffsetX = offsetX,
                OffsetY = offsetY,
                UseDarkTheme = false,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
            };
        }

        public static Control CreateOverlayControlDark(
            Control source,
            string title,
            string? hint = null,
            int offsetX = 0,
            int offsetY = 0)
        {
            return new DragPromptOverlayControl
            {
                Source = source,
                Title = title,
                Hint = hint,

                OffsetX = offsetX,
                OffsetY = offsetY,
                UseDarkTheme = true,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
            };
        }

        private sealed class DragPromptOverlayControl : Control
        {
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Control? Source { get; set; }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool UseDarkTheme
            {
                get => useDarkTheme;
                set
                {
                    useDarkTheme = value;
                    Invalidate();
                }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public string Title
            {
                get => title;
                set
                {
                    title = value ?? string.Empty;
                    Invalidate();
                }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public string? Hint
            {
                get => hint;
                set
                {
                    hint = value;
                    Invalidate();
                }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int OffsetX
            {
                get => offsetX;
                set
                {
                    offsetX = value;
                    Invalidate();
                }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int OffsetY
            {
                get => offsetY;
                set
                {
                    offsetY = value;
                    Invalidate();
                }
            }

            private bool useDarkTheme;
            private string title = string.Empty;
            private string? hint;
            private int offsetX;
            private int offsetY;

            public DragPromptOverlayControl()
            {
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.ResizeRedraw, true);
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);

                BackColor = Color.Transparent;
                TabStop = false;
            }

            protected override void OnPaintBackground(PaintEventArgs pevent)
            {
                if (BackColor == Color.Transparent && Parent != null)
                {
                    var g = pevent.Graphics;
                    var state = g.Save();

                    g.TranslateTransform(-Left, -Top);

                    var r = new Rectangle(Left, Top, Width, Height);
                    var pea = new PaintEventArgs(g, r);

                    InvokePaintBackground(Parent, pea);
                    InvokePaint(Parent, pea);

                    g.Restore(state);
                    return;
                }

                base.OnPaintBackground(pevent);
            }


            protected override void OnPaint(PaintEventArgs e)
            {
                var src = Source ?? Parent ?? this;

                if (UseDarkTheme)
                    DrawDark(e.Graphics, src, Title, Hint, OffsetX, OffsetY);
                else
                    Draw(e.Graphics, src, Title, Hint, OffsetX, OffsetY);
            }
        }

        public static void Draw(
            Graphics g,
            Control source,
            string title,
            string? hint = null,
            int offsetX = 0,
            int offsetY = 0)
        {
            DrawCore(g, source, title, hint, offsetX, offsetY, useDarkTheme: false);
        }

        public static void DrawDark(
            Graphics g,
            Control source,
            string title,
            string? hint = null,
            int offsetX = 0,
            int offsetY = 0)
        {
            DrawCore(g, source, title, hint, offsetX, offsetY, useDarkTheme: true);
        }

        private static void DrawCore(
            Graphics g,
            Control source,
            string title,
            string? hint,
            int offsetX,
            int offsetY,
            bool useDarkTheme)
        {
            int clientW = source.ClientSize.Width;
            int clientH = source.ClientSize.Height;

            if (clientW <= 0 || clientH <= 0)
                return;

            if (offsetX < 0) offsetX = 0;
            if (offsetY < 0) offsetY = 0;

            if (offsetX > clientW) offsetX = clientW;
            if (offsetY > clientH) offsetY = clientH;

            var overlayRect = new Rectangle(
                offsetX,
                offsetY,
                clientW - offsetX,
                clientH - offsetY);

            if (overlayRect.Width <= 0 || overlayRect.Height <= 0)
                return;

            DrawInternal(g, source, overlayRect, title, hint, useDarkTheme);
        }

        private static void DrawInternal(
            Graphics g,
            Control source,
            Rectangle overlayRect,
            string title,
            string? hint,
            bool useDarkTheme)
        {
            var baseFont = source.Font;

            var pageBackColor = source.BackColor;

            var cardFillColor = useDarkTheme
                ? Color.FromArgb(EmptyOverlayCardAlpha, 0, 0, 0)
                : Color.FromArgb(EmptyOverlayCardAlpha, 255, 255, 255);

            var shadowColor = Color.FromArgb(EmptyOverlayShadowAlpha, 0, 0, 0);

            var borderColor = useDarkTheme
                ? Color.FromArgb(EmptyOverlayBorderAlpha, 200, 200, 200)
                : Color.FromArgb(EmptyOverlayBorderAlpha, EmptyOverlayBorderColor);

            var dashColor = useDarkTheme
                ? Color.FromArgb(110, 235, 235, 235)
                : EmptyOverlayDashColor;

            var titleColor = useDarkTheme
                ? Color.FromArgb(245, 245, 245)
                : EmptyOverlayTitleColor;

            var hintColor = useDarkTheme
                ? Color.FromArgb(225, 225, 225)
                : EmptyOverlayHintColor;

            float stroke = useDarkTheme ? 2f : 1f;

            Region? oldClip = null;
            var oldSmoothing = g.SmoothingMode;

            try
            {
                oldClip = g.Clip?.Clone();
                g.SetClip(overlayRect, CombineMode.Replace);

                int cardW = (int)(overlayRect.Width * 0.62f);
                if (cardW < EmptyOverlayCardMinWidth) cardW = EmptyOverlayCardMinWidth;
                if (cardW > EmptyOverlayCardMaxWidth) cardW = EmptyOverlayCardMaxWidth;

                int cardX = overlayRect.Left + (overlayRect.Width - cardW) / 2;

                int cardY;
                int availableH = overlayRect.Height;

                if (availableH < EmptyOverlayCardHeight)
                {
                    cardY = overlayRect.Top + (availableH - EmptyOverlayCardHeight) / 2;
                }
                else
                {
                    int desiredY = overlayRect.Top + (availableH - EmptyOverlayCardHeight) / 3;

                    var form = source.FindForm();
                    if (form != null && !form.IsDisposed && form.ClientSize.Height > 0)
                    {
                        int overlayTopInFormClient;
                        try
                        {
                            var screenPt = source.PointToScreen(new Point(0, overlayRect.Top));
                            overlayTopInFormClient = form.PointToClient(screenPt).Y;
                        }
                        catch (Exception)
                        {
                            overlayTopInFormClient = overlayRect.Top;
                        }

                        int desiredCardTopInFormClient = (form.ClientSize.Height - EmptyOverlayCardHeight) / 3;
                        desiredY = overlayRect.Top + (desiredCardTopInFormClient - overlayTopInFormClient);
                    }

                    cardY = desiredY;

                    int minTop = 18;
                    int maxTop = overlayRect.Top + (availableH - EmptyOverlayCardHeight);

                    if (cardY < overlayRect.Top)
                        cardY = overlayRect.Top;

                    if (cardY > maxTop)
                        cardY = maxTop;

                    if (cardY < overlayRect.Top + minTop && maxTop >= overlayRect.Top + minTop)
                        cardY = overlayRect.Top + minTop;
                }

                var cardRect = new Rectangle(cardX, cardY, cardW, EmptyOverlayCardHeight);

                var shadowRect = cardRect;
                shadowRect.Offset(0, 2);

                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var shadowPath = CreateRoundRectPath(shadowRect, EmptyOverlayCardRadius))
                using (var shadowBrush = new SolidBrush(shadowColor))
                    g.FillPath(shadowBrush, shadowPath);

                using (var cardPath = CreateRoundRectPath(cardRect, EmptyOverlayCardRadius))
                using (var cardBrush = new SolidBrush(cardFillColor))
                using (var borderPen = new Pen(borderColor, stroke))
                {
                    g.FillPath(cardBrush, cardPath);
                    g.DrawPath(borderPen, cardPath);
                }

                var dashRect = Rectangle.Inflate(cardRect, -EmptyOverlayDashInset, -EmptyOverlayDashInset);
                dashRect.Y += 62;
                dashRect.Height = Math.Max(22, dashRect.Height - 78);

                using (var dashPath = CreateRoundRectPath(dashRect, 10))
                using (var dashPen = new Pen(dashColor, stroke) { DashStyle = DashStyle.Dash })
                    g.DrawPath(dashPen, dashPath);

                var (titleFont, hintFont) = ResolveFonts(baseFont);

                var titleRect = new Rectangle(
                    cardRect.Left + EmptyOverlayInnerPad,
                    cardRect.Top + 16,
                    cardRect.Width - (EmptyOverlayInnerPad * 2),
                    44);

                TextRenderer.DrawText(g, title, titleFont, titleRect, titleColor, EmptyOverlayTitleFlags);

                if (!string.IsNullOrWhiteSpace(hint))
                {
                    var hintRect = new Rectangle(
                        cardRect.Left + EmptyOverlayInnerPad,
                        cardRect.Top + 52,
                        cardRect.Width - (EmptyOverlayInnerPad * 2),
                        22);

                    TextRenderer.DrawText(g, hint, hintFont, hintRect, hintColor, EmptyOverlayHintFlags);
                }
            }
            finally
            {
                g.SmoothingMode = oldSmoothing;

                if (oldClip != null)
                {
                    g.SetClip(oldClip, CombineMode.Replace);
                    oldClip.Dispose();
                }
                else
                {
                    g.ResetClip();
                }
            }
        }


        private static (Font Title, Font Hint) ResolveFonts(Font baseFont)
        {
            var sig = $"{baseFont.FontFamily.Name}|{baseFont.SizeInPoints:0.###}|{(int)baseFont.Style}|{baseFont.Unit}";
            if (!string.Equals(sig, cachedFontSig, StringComparison.Ordinal) || cachedTitleFont == null || cachedHintFont == null)
            {
                cachedTitleFont?.Dispose();
                cachedHintFont?.Dispose();

                cachedTitleFont = new Font(baseFont.FontFamily, baseFont.SizeInPoints + 1.5f, FontStyle.Bold, GraphicsUnit.Point);
                cachedHintFont = new Font(baseFont.FontFamily, baseFont.SizeInPoints, FontStyle.Regular, GraphicsUnit.Point);

                cachedFontSig = sig;
            }

            return (cachedTitleFont, cachedHintFont);
        }

        private static GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            int d = radius * 2;

            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
