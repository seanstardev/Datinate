using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    [DesignerCategory("Code")]
    [ToolboxItem(true)]
    [DefaultProperty(nameof(Percent))]
    [DefaultEvent(nameof(Click))]
    public sealed class SegmentedProgressBarUI : Control
    {
        private readonly object stateLock = new();

        private int percent;
        private long completedParts;
        private long totalParts;
        private string statusText = string.Empty;

        private int segmentCount = 100;
        private int segmentGapPx = 1;
        private int innerPaddingPx = 4;
        private int textGapPx = 4;
        private int minBarHeightPx = 8;

        private Color filledSegmentColor = Color.Black;
        private Color emptySegmentColor = Color.Silver;
        private Color segmentBorderColor = Color.FromArgb(140, 140, 140);
        private Color textColor = Color.Black;
        private Color surfaceColor = SystemColors.Control;
        private bool useParentBackColor = true;
        private bool drawSegmentBorders = false;

        public SegmentedProgressBarUI()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 8f, FontStyle.Regular, GraphicsUnit.Point);
            Size = new Size(320, 42);
        }

        protected override Size DefaultSize => new(320, 42);

        [Category("Data")]
        [DefaultValue(0)]
        public int Percent
        {
            get
            {
                lock (stateLock)
                    return percent;
            }
            set => SetProgress(value);
        }

        [Category("Data")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long CompletedParts
        {
            get
            {
                lock (stateLock)
                    return completedParts;
            }
        }

        [Category("Data")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long TotalParts
        {
            get
            {
                lock (stateLock)
                    return totalParts;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        public override string Text
        {
            get
            {
                lock (stateLock)
                    return statusText;
            }
            set => SetStatus(value);
        }

        [Category("Appearance")]
        [DefaultValue(100)]
        public int SegmentCount
        {
            get => segmentCount;
            set
            {
                int v = value;
                if (v < 1) v = 1;
                if (v > 100) v = 100;
                if (segmentCount == v) return;
                segmentCount = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(1)]
        public int SegmentGapPx
        {
            get => segmentGapPx;
            set
            {
                int v = value;
                if (v < 0) v = 0;
                if (segmentGapPx == v) return;
                segmentGapPx = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(4)]
        public int InnerPaddingPx
        {
            get => innerPaddingPx;
            set
            {
                int v = value;
                if (v < 0) v = 0;
                if (innerPaddingPx == v) return;
                innerPaddingPx = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(4)]
        public int TextGapPx
        {
            get => textGapPx;
            set
            {
                int v = value;
                if (v < 0) v = 0;
                if (textGapPx == v) return;
                textGapPx = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(8)]
        public int MinBarHeightPx
        {
            get => minBarHeightPx;
            set
            {
                int v = value;
                if (v < 1) v = 1;
                if (minBarHeightPx == v) return;
                minBarHeightPx = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color FilledSegmentColor
        {
            get => filledSegmentColor;
            set
            {
                if (filledSegmentColor == value) return;
                filledSegmentColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color EmptySegmentColor
        {
            get => emptySegmentColor;
            set
            {
                if (emptySegmentColor == value) return;
                emptySegmentColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color SegmentBorderColor
        {
            get => segmentBorderColor;
            set
            {
                if (segmentBorderColor == value) return;
                segmentBorderColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ProgressTextColor
        {
            get => textColor;
            set
            {
                if (textColor == value) return;
                textColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color SurfaceColor
        {
            get => surfaceColor;
            set
            {
                if (surfaceColor == value) return;
                surfaceColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool UseParentBackColor
        {
            get => useParentBackColor;
            set
            {
                if (useParentBackColor == value) return;
                useParentBackColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool DrawSegmentBorders
        {
            get => drawSegmentBorders;
            set
            {
                if (drawSegmentBorders == value) return;
                drawSegmentBorders = value;
                Invalidate();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            string t;
            lock (stateLock)
                t = statusText;

            base.Text = t;
        }

        public void SetStatus(string? message)
        {
            ApplyState(null, null, message ?? string.Empty);
        }

        public void SetProgress(int percentValue)
        {
            ApplyState(percentValue, null, null);
        }

        public void SetProgress(int percentValue, string? message)
        {
            ApplyState(percentValue, null, message ?? string.Empty);
        }

        public void SetProgress(long parts, long total)
        {
            ApplyState(CalcPercent(parts, total), (parts, total), null);
        }

        public void SetProgress(long parts, long total, string? message)
        {
            ApplyState(CalcPercent(parts, total), (parts, total), message ?? string.Empty);
        }

        public void SetProgress(int parts, int total)
        {
            SetProgress((long)parts, (long)total);
        }

        public void SetProgress(int parts, int total, string? message)
        {
            SetProgress((long)parts, (long)total, message);
        }

        private void ApplyState(int? newPercent, (long parts, long total)? partsInfo, string? newText)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired && IsHandleCreated)
            {
                try
                {
                    BeginInvoke(new Action(() => ApplyState(newPercent, partsInfo, newText)));
                }
                catch
                {
                }
                return;
            }

            bool changed = false;
            string? textToSync = null;

            lock (stateLock)
            {
                if (newPercent.HasValue)
                {
                    int p = newPercent.Value;
                    if (p < 0) p = 0;
                    if (p > 100) p = 100;

                    if (percent != p)
                    {
                        percent = p;
                        changed = true;
                    }
                }

                if (partsInfo.HasValue)
                {
                    var (p, t) = partsInfo.Value;

                    if (p < 0) p = 0;
                    if (t < 0) t = 0;

                    if (completedParts != p || totalParts != t)
                    {
                        completedParts = p;
                        totalParts = t;
                        changed = true;
                    }
                }

                if (newText != null && !string.Equals(statusText, newText, StringComparison.Ordinal))
                {
                    statusText = newText;
                    textToSync = newText;
                    changed = true;
                }
            }

            if (textToSync != null && IsHandleCreated)
                base.Text = textToSync;

            if (changed)
                Invalidate();
        }

        private static int CalcPercent(long parts, long total)
        {
            if (total <= 0)
                return 0;

            if (parts <= 0)
                return 0;

            if (parts >= total)
                return 100;

            double p = (parts * 100d) / total;
            int v = (int)Math.Round(p, MidpointRounding.AwayFromZero);

            if (v < 0) v = 0;
            if (v > 100) v = 100;

            return v;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int p;
            string msg;
            int segs;
            int gap;
            int pad;
            int textGap;
            int minBarH;
            Color fillColor;
            Color emptyColor;
            Color borderColor;
            Color txtColor;
            Color surfColor;
            bool useParentBack;
            bool drawBorders;

            lock (stateLock)
            {
                p = percent;
                msg = statusText ?? string.Empty;
                segs = segmentCount;
                gap = segmentGapPx;
                pad = innerPaddingPx;
                textGap = textGapPx;
                minBarH = minBarHeightPx;
                fillColor = filledSegmentColor;
                emptyColor = emptySegmentColor;
                borderColor = segmentBorderColor;
                txtColor = textColor;
                surfColor = surfaceColor;
                useParentBack = useParentBackColor;
                drawBorders = drawSegmentBorders;
            }

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            var bg = useParentBack && Parent is not null ? Parent.BackColor : surfColor;

            using (var b = new SolidBrush(bg))
                g.FillRectangle(b, ClientRectangle);

            var r = ClientRectangle;
            if (r.Width <= 2 || r.Height <= 2)
                return;

            var content = Rectangle.FromLTRB(
                r.Left + pad,
                r.Top + pad,
                r.Right - pad,
                r.Bottom - pad);

            if (content.Width <= 2 || content.Height <= 2)
                return;

            string percentText = p.ToString() + "%";

            int textHeight = TextRenderer.MeasureText(g, "Mg", Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Height;
            if (textHeight < 10)
                textHeight = 10;

            int messageRowHeight = Math.Min(textHeight + 2, Math.Max(0, content.Height - minBarH - textGap));
            if (messageRowHeight < 0)
                messageRowHeight = 0;

            Rectangle messageRow;
            Rectangle barsRect;

            if (content.Height >= (textHeight + textGap + minBarH))
            {
                messageRow = new Rectangle(content.X, content.Y, content.Width, messageRowHeight);
                barsRect = new Rectangle(content.X, messageRow.Bottom + textGap, content.Width, content.Bottom - (messageRow.Bottom + textGap));
            }
            else
            {
                int forcedBarH = Math.Max(1, Math.Min(content.Height, minBarH));
                barsRect = new Rectangle(content.X, content.Bottom - forcedBarH, content.Width, forcedBarH);
                messageRow = new Rectangle(content.X, content.Y, content.Width, Math.Max(0, barsRect.Y - content.Y - textGap));
            }

            if (messageRow.Height > 0 && messageRow.Width > 4)
            {
                int pctW = TextRenderer.MeasureText(g, "100%", Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width + 4;
                if (pctW < 40) pctW = 40;
                if (pctW > messageRow.Width) pctW = messageRow.Width;

                var pctRect = new Rectangle(messageRow.Right - pctW, messageRow.Y, pctW, messageRow.Height);
                var msgRect = new Rectangle(messageRow.X, messageRow.Y, Math.Max(0, messageRow.Width - pctW - 4), messageRow.Height);

                TextRenderer.DrawText(
                    g,
                    msg,
                    Font,
                    msgRect,
                    txtColor,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPrefix |
                    TextFormatFlags.NoPadding);

                TextRenderer.DrawText(
                    g,
                    percentText,
                    Font,
                    pctRect,
                    txtColor,
                    TextFormatFlags.Right |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPrefix |
                    TextFormatFlags.NoPadding);
            }

            if (barsRect.Width <= 0 || barsRect.Height <= 0)
                return;

            int drawSegs = segs;
            if (drawSegs < 1)
                drawSegs = 1;

            if (barsRect.Width < drawSegs)
                drawSegs = Math.Max(1, barsRect.Width);

            int actualGap = gap;
            while (actualGap > 0 && (drawSegs + ((drawSegs - 1) * actualGap)) > barsRect.Width)
                actualGap--;

            int totalGapWidth = (drawSegs - 1) * actualGap;
            int drawableWidth = barsRect.Width - totalGapWidth;
            if (drawableWidth < drawSegs)
            {
                drawSegs = Math.Max(1, barsRect.Width);
                actualGap = 0;
                totalGapWidth = 0;
                drawableWidth = barsRect.Width;
            }

            int filledCount = (int)Math.Round((p / 100d) * drawSegs, MidpointRounding.AwayFromZero);
            if (filledCount < 0) filledCount = 0;
            if (filledCount > drawSegs) filledCount = drawSegs;

            int baseSegW = drawableWidth / drawSegs;
            int rem = drawableWidth % drawSegs;

            int x = barsRect.X;

            using var fillBrush = new SolidBrush(fillColor);
            using var emptyBrush = new SolidBrush(emptyColor);
            using var borderPen = new Pen(borderColor, 1f);

            for (int i = 0; i < drawSegs; i++)
            {
                int w = baseSegW + (i < rem ? 1 : 0);
                if (w <= 0)
                    continue;

                var segRect = new Rectangle(x, barsRect.Y, w, barsRect.Height);

                g.FillRectangle(i < filledCount ? fillBrush : emptyBrush, segRect);

                if (drawBorders && segRect.Width > 1 && segRect.Height > 1)
                    g.DrawRectangle(borderPen, segRect.X, segRect.Y, segRect.Width - 1, segRect.Height - 1);

                x += w + actualGap;
            }
        }
    }
}