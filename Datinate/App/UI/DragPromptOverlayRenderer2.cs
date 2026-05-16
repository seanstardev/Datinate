using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    public static partial class DragPromptOverlayRenderer
    {
        private const int STACK_CREATECONTROL_FIRST_CARD_TOP_GAP_Y_ADJUST_PX_202602xx = 0;

        private const int EmptyOverlayCardHeight = 176;
        private const int ICON_ALPHA_PERCENT = 40;

        private const int STACK_CARD_GAP_PX_202602xx = 12;
        private const int STACK_CARD_TEXT_ONLY_HEIGHT_PX_202602xx = 92;

        private const int STACK_CARD_TEXT_TO_DASH_GAP_PX_202602xx = 10;
        private const int STACK_CARD_DASH_BOTTOM_GAP_PX_202602xx = 12;

        private const int STACK_CARD_BG_ALPHA_PERCENT_202602xx = 50;

        private const bool STACK_CARDS_DEFAULT_TRANSPARENT_FILL_202602xx = true;

        private const int STACK_CARDS_SHADOW_ALPHA_202602xx = 36;

        private const int STACK_CARD_ICON_INSET_PX_202602xx = 2;
        private const int STACK_CARD_ICON_MAX_PX_202602xx = 100;
        private const int STACK_CARD_ICON_MULTI_GAP_PX_202602xx = 10;
        private const int STACK_DEFAULT_OFFSET_Y_202602xx = 0;

        private static void DrawImageWithAlpha(Graphics g, Image img, Rectangle dst, int alphaPercent)
        {
            if (alphaPercent >= 100)
            {
                g.DrawImage(img, dst);
                return;
            }

            if (alphaPercent <= 0)
                return;

            float a = alphaPercent / 100f;

            using var ia = new System.Drawing.Imaging.ImageAttributes();
            var cm = new System.Drawing.Imaging.ColorMatrix(new float[][]
            {
                new float[] {1,0,0,0,0},
                new float[] {0,1,0,0,0},
                new float[] {0,0,1,0,0},
                new float[] {0,0,0,a,0},
                new float[] {0,0,0,0,1}
            });

            ia.SetColorMatrix(cm, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);

            g.DrawImage(
                img,
                dst,
                0,
                0,
                img.Width,
                img.Height,
                GraphicsUnit.Pixel,
                ia);
        }

        private static int AlphaFromPercent(int percent)
        {
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;
            return (percent * 255 + 50) / 100;
        }

        private static bool IsRgbBlack(Color c) => c.R == 0 && c.G == 0 && c.B == 0;

        public readonly record struct OverlayDragHandlers(
            DragEventHandler? DragEnter,
            DragEventHandler? DragOver,
            EventHandler? DragLeave,
            DragEventHandler? DragDrop)
        {
            public bool HasAny =>
                DragEnter != null || DragOver != null || DragLeave != null || DragDrop != null;
        }

        public readonly record struct OverlayCardSpec(
            string Title,
            string? Hint = null,
            IReadOnlyList<Bitmap>? Icons = null,
            OverlayDragHandlers Drag = default,
            Color? BackgroundColor = null,
            bool Visible = true);

        public interface IOverlayCardStack
        {
            IReadOnlyList<OverlayCardSpec> Cards { get; }

            bool SetCardVisible(int index, bool visible);

            bool TryGetCard(int index, out OverlayCardSpec spec);

            bool TrySetCard(int index, OverlayCardSpec spec);

            void RefreshLayout();
        }

        public static Control CreateOverlayStackControl(
            Control source,
            IReadOnlyList<OverlayCardSpec> cards,
            int offsetX = 0,
            int offsetY = STACK_DEFAULT_OFFSET_Y_202602xx,
            Color? overlayBackgroundColor = null)
        {
            var c = new DragPromptOverlayStackControl
            {
                Source = source,
                OffsetX = offsetX,
                OffsetY = offsetY,
                UseDarkTheme = false,
                AlignFirstCardTopGapToInterCardGap = true,
                OverlayBackgroundColor = overlayBackgroundColor,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
            };

            c.SetCards(cards);
            return c;
        }

        public static Control CreateOverlayStackControlDark(
            Control source,
            IReadOnlyList<OverlayCardSpec> cards,
            int offsetX = 0,
            int offsetY = STACK_DEFAULT_OFFSET_Y_202602xx,
            Color? overlayBackgroundColor = null)
        {
            var c = new DragPromptOverlayStackControl
            {
                Source = source,
                OffsetX = offsetX,
                OffsetY = offsetY,
                UseDarkTheme = true,
                AlignFirstCardTopGapToInterCardGap = true,
                OverlayBackgroundColor = overlayBackgroundColor,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
            };

            c.SetCards(cards);
            return c;
        }

        private sealed class DragPromptOverlayStackControl : Control, IOverlayCardStack
        {
            public IReadOnlyList<OverlayCardSpec> Cards => cards;

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Control? Source { get; set; }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool UseDarkTheme
            {
                get => useDarkTheme;
                set { useDarkTheme = value; Invalidate(); }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int OffsetX
            {
                get => offsetX;
                set { offsetX = value; RequestLayout(); }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public int OffsetY
            {
                get => offsetY;
                set { offsetY = value; RequestLayout(); }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Color? OverlayBackgroundColor
            {
                get => overlayBackgroundColor;
                set { overlayBackgroundColor = value; Invalidate(); }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public bool AlignFirstCardTopGapToInterCardGap
            {
                get => alignFirstCardTopGapToInterCardGap;
                set { alignFirstCardTopGapToInterCardGap = value; RequestLayout(resetTargets: true); }
            }

            private int stickyStartY = int.MinValue;

            private bool useDarkTheme;
            private int offsetX;
            private int offsetY;
            private Color? overlayBackgroundColor;

            private readonly List<OverlayCardSpec> cards = new();
            private int cardsVersion;
            private int cachedCardsVersion = -1;

            private Rectangle cachedOverlayRect;
            private int cachedW = -1;
            private int cachedH = -1;
            private int cachedOffsetX = int.MinValue;
            private int cachedOffsetY = int.MinValue;

            private readonly Dictionary<int, Rectangle> currentRects = new();

            private int activeCardIndex = -1;
            private bool cachedCompactLayout;
            private int cachedStackCardHeight = EmptyOverlayCardHeight;
            private int cachedStackGap = STACK_CARD_GAP_PX_202602xx;

            private bool alignFirstCardTopGapToInterCardGap;

            public DragPromptOverlayStackControl()
            {
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.ResizeRedraw, true);
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);

                BackColor = Color.Transparent;
                TabStop = false;
            }

            public void SetCards(IReadOnlyList<OverlayCardSpec> input)
            {
                cards.Clear();

                if (input != null)
                {
                    for (int i = 0; i < input.Count; i++)
                        cards.Add(input[i]);
                }

                cardsVersion++;
                activeCardIndex = -1;

                UpdateAllowDrop();
                RequestLayout(resetTargets: true);
            }

            public bool SetCardVisible(int index, bool visible)
            {
                if ((uint)index >= (uint)cards.Count)
                    return false;

                var c = cards[index];
                if (c.Visible == visible)
                    return true;

                cards[index] = c with { Visible = visible };
                cardsVersion++;
                activeCardIndex = -1;

                UpdateAllowDrop();
                RequestLayout(resetTargets: true);
                return true;
            }

            public bool TryGetCard(int index, out OverlayCardSpec spec)
            {
                if ((uint)index >= (uint)cards.Count)
                {
                    spec = default;
                    return false;
                }

                spec = cards[index];
                return true;
            }

            public bool TrySetCard(int index, OverlayCardSpec spec)
            {
                if ((uint)index >= (uint)cards.Count)
                    return false;

                cards[index] = spec;
                cardsVersion++;
                activeCardIndex = -1;

                UpdateAllowDrop();
                RequestLayout(resetTargets: true);
                return true;
            }

            public void RefreshLayout()
            {
                cardsVersion++;
                activeCardIndex = -1;

                UpdateAllowDrop();
                RequestLayout(resetTargets: true);
            }

            private void UpdateAllowDrop()
            {
                bool any = false;

                for (int i = 0; i < cards.Count; i++)
                {
                    if (!cards[i].Visible)
                        continue;

                    if (cards[i].Drag.HasAny)
                    {
                        any = true;
                        break;
                    }
                }

                AllowDrop = any;
            }

            private void RequestLayout(bool resetTargets = false)
            {
                cachedW = -1;
                cachedH = -1;
                cachedOffsetX = int.MinValue;
                cachedOffsetY = int.MinValue;
                cachedCardsVersion = -1;
                cachedOverlayRect = Rectangle.Empty;

                cachedCompactLayout = false;
                cachedStackCardHeight = EmptyOverlayCardHeight;
                cachedStackGap = STACK_CARD_GAP_PX_202602xx;

                if (resetTargets)
                    currentRects.Clear();

                Invalidate();
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
                EnsureLayout(src);

                var drawIndices = new List<int>(cards.Count);
                var drawRects = new List<Rectangle>(cards.Count);

                for (int i = 0; i < cards.Count; i++)
                {
                    if (!TryGetDrawRect(i, out var rr))
                        continue;

                    int insertAt = drawRects.Count;
                    for (int j = 0; j < drawRects.Count; j++)
                    {
                        if (rr.Y < drawRects[j].Y)
                        {
                            insertAt = j;
                            break;
                        }
                    }

                    drawRects.Insert(insertAt, rr);
                    drawIndices.Insert(insertAt, i);
                }

                DrawStackInternal(
                    e.Graphics,
                    src,
                    cachedOverlayRect,
                    drawRects,
                    drawIndices,
                    cards,
                    UseDarkTheme,
                    OverlayBackgroundColor,
                    activeCardIndex,
                    cachedStackCardHeight,
                    cachedCompactLayout);

                base.OnPaint(e);
            }

            private void EnsureLayout(Control sourceControl)
            {
                int w = sourceControl.ClientSize.Width;
                int h = sourceControl.ClientSize.Height;

                if (w <= 0 || h <= 0)
                    return;

                if (cachedW == w &&
                    cachedH == h &&
                    cachedOffsetX == offsetX &&
                    cachedOffsetY == offsetY &&
                    cachedCardsVersion == cardsVersion)
                    return;

                cachedW = w;
                cachedH = h;
                cachedOffsetX = offsetX;
                cachedOffsetY = offsetY;
                cachedCardsVersion = cardsVersion;

                cachedOverlayRect = ComputeOverlayRect(w, h, offsetX, offsetY);

                cachedCompactLayout = false;
                cachedStackCardHeight = EmptyOverlayCardHeight;
                cachedStackGap = STACK_CARD_GAP_PX_202602xx;

                currentRects.Clear();

                if (cachedOverlayRect.Width <= 0 || cachedOverlayRect.Height <= 0)
                    return;

                var visibleIndices = new List<int>(cards.Count);
                for (int i = 0; i < cards.Count; i++)
                {
                    if (cards[i].Visible)
                        visibleIndices.Add(i);
                }

                int count = visibleIndices.Count;
                if (count <= 0)
                    return;

                int gap = STACK_CARD_GAP_PX_202602xx;

                int fullH = EmptyOverlayCardHeight;
                int fullTotalH = (count * fullH) + ((count - 1) * gap);

                int layoutH = fullH;
                bool compact = false;

                if (fullTotalH > cachedOverlayRect.Height)
                {
                    compact = true;
                    layoutH = STACK_CARD_TEXT_ONLY_HEIGHT_PX_202602xx;
                }

                cachedCompactLayout = compact;
                cachedStackCardHeight = layoutH;
                cachedStackGap = gap;

                var tmpRects = new List<Rectangle>(count);

                ComputeStackCardRects(
                    sourceControl,
                    cachedOverlayRect,
                    count,
                    layoutH,
                    gap,
                    tmpRects,
                    ref stickyStartY,
                    alignFirstCardTopGapToInterCardGap);

                for (int i = 0; i < count; i++)
                    currentRects[visibleIndices[i]] = tmpRects[i];
            }

            private bool TryGetDrawRect(int index, out Rectangle rect)
            {
                rect = Rectangle.Empty;

                if ((uint)index >= (uint)cards.Count)
                    return false;

                return currentRects.TryGetValue(index, out rect) &&
                       rect.Width > 0 &&
                       rect.Height > 0;
            }

            protected override void OnDragEnter(DragEventArgs drgevent)
            {
                RouteDragMove(drgevent);
                base.OnDragEnter(drgevent);
            }

            protected override void OnDragOver(DragEventArgs drgevent)
            {
                RouteDragMove(drgevent);
                base.OnDragOver(drgevent);
            }

            protected override void OnDragLeave(EventArgs e)
            {
                if (activeCardIndex >= 0 && activeCardIndex < cards.Count)
                    cards[activeCardIndex].Drag.DragLeave?.Invoke(this, e);

                activeCardIndex = -1;
                Invalidate();

                base.OnDragLeave(e);
            }

            protected override void OnDragDrop(DragEventArgs drgevent)
            {
                var src = Source ?? Parent ?? this;
                EnsureLayout(src);

                if (activeCardIndex >= 0 && activeCardIndex < cards.Count)
                    cards[activeCardIndex].Drag.DragDrop?.Invoke(this, drgevent);

                activeCardIndex = -1;
                Invalidate();

                base.OnDragDrop(drgevent);
            }

            private void RouteDragMove(DragEventArgs e)
            {
                var src = Source ?? Parent ?? this;
                EnsureLayout(src);

                var pt = PointToClient(new Point(e.X, e.Y));

                int newActive = -1;

                for (int i = 0; i < cards.Count; i++)
                {
                    if (!cards[i].Visible)
                        continue;

                    if (!TryGetDrawRect(i, out var rr))
                        continue;

                    if (rr.Contains(pt))
                    {
                        newActive = i;
                        break;
                    }
                }

                if (newActive != activeCardIndex)
                {
                    if (activeCardIndex >= 0 && activeCardIndex < cards.Count)
                        cards[activeCardIndex].Drag.DragLeave?.Invoke(this, EventArgs.Empty);

                    activeCardIndex = newActive;

                    if (activeCardIndex >= 0 && activeCardIndex < cards.Count)
                        cards[activeCardIndex].Drag.DragEnter?.Invoke(this, e);

                    Invalidate();
                }

                if (activeCardIndex >= 0 && activeCardIndex < cards.Count)
                    cards[activeCardIndex].Drag.DragOver?.Invoke(this, e);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Source = null;
                    cards.Clear();
                    currentRects.Clear();
                    cachedOverlayRect = Rectangle.Empty;
                }

                base.Dispose(disposing);
            }
        }

        private static Rectangle ComputeOverlayRect(int clientW, int clientH, int offsetX, int offsetY)
        {
            if (offsetX < 0) offsetX = 0;
            if (offsetY < 0) offsetY = 0;
            if (offsetX > clientW) offsetX = clientW;
            if (offsetY > clientH) offsetY = clientH;

            return new Rectangle(offsetX, offsetY, clientW - offsetX, clientH - offsetY);
        }

        private static void ComputeStackCardRects(
            Control source,
            Rectangle overlayRect,
            int count,
            List<Rectangle> outRects,
            ref int stickyStartY,
            bool alignFirstCardTopGapToInterCardGap)
        {
            ComputeStackCardRects(
                source,
                overlayRect,
                count,
                EmptyOverlayCardHeight,
                STACK_CARD_GAP_PX_202602xx,
                outRects,
                ref stickyStartY,
                alignFirstCardTopGapToInterCardGap);
        }

        private static void ComputeStackCardRects(
            Control source,
            Rectangle overlayRect,
            int count,
            int cardHeight,
            int gap,
            List<Rectangle> outRects,
            ref int stickyStartY,
            bool alignFirstCardTopGapToInterCardGap)
        {
            int cardW = (int)(overlayRect.Width * 0.62f);
            if (cardW < EmptyOverlayCardMinWidth) cardW = EmptyOverlayCardMinWidth;
            if (cardW > EmptyOverlayCardMaxWidth) cardW = EmptyOverlayCardMaxWidth;

            int totalH = (count * cardHeight) + ((count - 1) * gap);

            int cardX = overlayRect.Left + (overlayRect.Width - cardW) / 2;

            int availableH = overlayRect.Height;

            int minTop = overlayRect.Top + (alignFirstCardTopGapToInterCardGap ? gap : 18);
            int maxTop = overlayRect.Bottom - totalH;
            if (maxTop < overlayRect.Top) maxTop = overlayRect.Top;

            int startY;

            if (availableH < totalH)
            {
                startY = overlayRect.Top + (availableH - totalH) / 2;
            }
            else
            {
                if (alignFirstCardTopGapToInterCardGap)
                {
                    startY = overlayRect.Top + gap + STACK_CREATECONTROL_FIRST_CARD_TOP_GAP_Y_ADJUST_PX_202602xx;
                }
                else if (stickyStartY != int.MinValue)
                {
                    startY = stickyStartY;
                }
                else
                {
                    startY = overlayRect.Top + (availableH / 3);
                }

                if (startY < minTop && maxTop >= minTop)
                    startY = minTop;

                if (startY < overlayRect.Top)
                    startY = overlayRect.Top;

                if (startY > maxTop)
                    startY = maxTop;
            }

            stickyStartY = startY;

            for (int i = 0; i < count; i++)
            {
                int y = startY + (i * (cardHeight + gap));
                outRects.Add(new Rectangle(cardX, y, cardW, cardHeight));
            }
        }

        private static void DrawStackInternal(
            Graphics g,
            Control source,
            Rectangle overlayRect,
            List<Rectangle> visibleCardRects,
            List<int> visibleCardIndices,
            IReadOnlyList<OverlayCardSpec> cards,
            bool useDarkTheme,
            Color? overlayBackgroundColor,
            int activeCardIndex,
            int layoutCardHeight,
            bool compactLayout)
        {
            if (overlayRect.Width <= 0 || overlayRect.Height <= 0)
                return;

            if (visibleCardRects.Count == 0 || visibleCardIndices.Count == 0)
                return;

            var baseFont = source.Font;

            int bgAlpha = AlphaFromPercent(STACK_CARD_BG_ALPHA_PERCENT_202602xx);

            Region? oldClip = null;
            var oldSmoothing = g.SmoothingMode;

            try
            {
                oldClip = g.Clip?.Clone();
                g.SetClip(overlayRect, CombineMode.Replace);

                if (overlayBackgroundColor.HasValue && overlayBackgroundColor.Value.A > 0)
                {
                    using var bg = new SolidBrush(overlayBackgroundColor.Value);
                    g.FillRectangle(bg, overlayRect);
                }

                g.SmoothingMode = SmoothingMode.AntiAlias;

                var (titleFont, hintFont) = ResolveFonts(baseFont);

                int n = Math.Min(visibleCardRects.Count, visibleCardIndices.Count);
                for (int vi = 0; vi < n; vi++)
                {
                    int cardIndex = visibleCardIndices[vi];
                    if ((uint)cardIndex >= (uint)cards.Count)
                        continue;

                    var spec = cards[cardIndex];
                    var cardRect = visibleCardRects[vi];

                    if (cardRect.Width <= 0 || cardRect.Height <= 0)
                        continue;

                    bool hasHint = !string.IsNullOrWhiteSpace(spec.Hint);

                    bool cardDarkTheme =
                        useDarkTheme ||
                        (spec.BackgroundColor.HasValue && IsRgbBlack(spec.BackgroundColor.Value));

                    bool isActive = cardIndex == activeCardIndex;

                    var borderColor = cardDarkTheme
                        ? Color.FromArgb(EmptyOverlayBorderAlpha, 200, 200, 200)
                        : Color.FromArgb(EmptyOverlayBorderAlpha, EmptyOverlayBorderColor);

                    var borderColorActive = cardDarkTheme
                        ? Color.FromArgb(160, 245, 245, 245)
                        : Color.FromArgb(140, 0, 0, 0);

                    var dashColor = cardDarkTheme
                        ? Color.FromArgb(110, 235, 235, 235)
                        : EmptyOverlayDashColor;

                    var dashColorActive = cardDarkTheme
                        ? Color.FromArgb(160, 255, 255, 255)
                        : Color.FromArgb(150, 20, 20, 20);

                    var titleColor = cardDarkTheme
                        ? Color.FromArgb(245, 245, 245)
                        : EmptyOverlayTitleColor;

                    var hintColor = cardDarkTheme
                        ? Color.FromArgb(225, 225, 225)
                        : EmptyOverlayHintColor;

                    float stroke = cardDarkTheme ? 2f : 1f;
                    float strokeActive = stroke + 1f;

                    if (STACK_CARDS_SHADOW_ALPHA_202602xx > 0)
                    {
                        var shadowRect = cardRect;
                        shadowRect.Offset(0, 2);

                        using var shadowPath = CreateRoundRectPath(shadowRect, EmptyOverlayCardRadius);
                        using var shadowBrush = new SolidBrush(Color.FromArgb(STACK_CARDS_SHADOW_ALPHA_202602xx, 0, 0, 0));
                        g.FillPath(shadowBrush, shadowPath);
                    }

                    using (var cardPath = CreateRoundRectPath(cardRect, EmptyOverlayCardRadius))
                    using (var borderPen = new Pen(isActive ? borderColorActive : borderColor, isActive ? strokeActive : stroke))
                    {
                        bool shouldFill =
                            spec.BackgroundColor.HasValue ||
                            !STACK_CARDS_DEFAULT_TRANSPARENT_FILL_202602xx;

                        if (shouldFill)
                        {
                            Color fill;

                            if (spec.BackgroundColor.HasValue)
                            {
                                var c = spec.BackgroundColor.Value;
                                fill = Color.FromArgb(bgAlpha, c.R, c.G, c.B);
                            }
                            else
                            {
                                fill = cardDarkTheme
                                    ? Color.FromArgb(EmptyOverlayCardAlpha, 0, 0, 0)
                                    : Color.FromArgb(EmptyOverlayCardAlpha, 255, 255, 255);
                            }

                            using var cardBrush = new SolidBrush(fill);
                            g.FillPath(cardBrush, cardPath);
                        }

                        g.DrawPath(borderPen, cardPath);
                    }

                    Region? clipBeforeCard = null;
                    try
                    {
                        clipBeforeCard = g.Clip?.Clone();
                        g.SetClip(cardRect, CombineMode.Intersect);

                        var titleRect = new Rectangle(
                            cardRect.Left + EmptyOverlayInnerPad,
                            cardRect.Top + 16,
                            cardRect.Width - (EmptyOverlayInnerPad * 2),
                            44);

                        var hintRect = new Rectangle(
                            cardRect.Left + EmptyOverlayInnerPad,
                            cardRect.Top + 52,
                            cardRect.Width - (EmptyOverlayInnerPad * 2),
                            22);

                        int reservedTextBottom = hasHint ? hintRect.Bottom : titleRect.Bottom;

                        var titleRectClipped = Rectangle.Intersect(titleRect, cardRect);
                        if (titleRectClipped.Width > 0 && titleRectClipped.Height > 6)
                            TextRenderer.DrawText(g, spec.Title ?? string.Empty, titleFont, titleRectClipped, titleColor, EmptyOverlayTitleFlags);

                        if (hasHint)
                        {
                            var hintRectClipped = Rectangle.Intersect(hintRect, cardRect);
                            if (hintRectClipped.Width > 0 && hintRectClipped.Height > 6)
                                TextRenderer.DrawText(g, spec.Hint, hintFont, hintRectClipped, hintColor, EmptyOverlayHintFlags);
                        }

                        if (!compactLayout)
                        {
                            int dashLeft = cardRect.Left + EmptyOverlayDashInset;
                            int dashRight = cardRect.Right - EmptyOverlayDashInset;

                            int dashTop = reservedTextBottom + STACK_CARD_TEXT_TO_DASH_GAP_PX_202602xx;
                            int dashBottom = cardRect.Bottom - EmptyOverlayDashInset - STACK_CARD_DASH_BOTTOM_GAP_PX_202602xx;

                            int dashW = dashRight - dashLeft;
                            int dashH = dashBottom - dashTop;

                            if (dashW > 0 && dashH > 0)
                            {
                                var dashRect = new Rectangle(dashLeft, dashTop, dashW, dashH);

                                using (var dashPath = CreateRoundRectPath(dashRect, 10))
                                using (var dashPen = new Pen(isActive ? dashColorActive : dashColor, isActive ? strokeActive : stroke) { DashStyle = DashStyle.Dash })
                                    g.DrawPath(dashPen, dashPath);

                                var icons = spec.Icons;
                                if (icons != null && icons.Count > 0 && dashRect.Width > 8 && dashRect.Height > 8)
                                {
                                    var inner = Rectangle.Inflate(dashRect, -STACK_CARD_ICON_INSET_PX_202602xx, -STACK_CARD_ICON_INSET_PX_202602xx);

                                    int iconAlpha = ICON_ALPHA_PERCENT;

                                    var oldIM = g.InterpolationMode;
                                    var oldPO = g.PixelOffsetMode;
                                    try
                                    {
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                        g.PixelOffsetMode = PixelOffsetMode.Half;

                                        if (icons.Count == 1)
                                        {
                                            var img = icons[0];
                                            if (img != null)
                                            {
                                                int max = Math.Min(STACK_CARD_ICON_MAX_PX_202602xx, Math.Min(inner.Width, inner.Height));
                                                if (max > 0)
                                                {
                                                    int iw = img.Width;
                                                    int ih = img.Height;

                                                    if (iw > 0 && ih > 0)
                                                    {
                                                        float s = Math.Min((float)max / iw, (float)max / ih);
                                                        int dw = Math.Max(1, (int)(iw * s));
                                                        int dh = Math.Max(1, (int)(ih * s));

                                                        var dst = new Rectangle(
                                                            inner.Left + (inner.Width - dw) / 2,
                                                            inner.Top + (inner.Height - dh) / 2,
                                                            dw,
                                                            dh);

                                                        DrawImageWithAlpha(g, img, dst, iconAlpha);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            int gap = STACK_CARD_ICON_MULTI_GAP_PX_202602xx;
                                            int maxCellW = (inner.Width - (gap * (icons.Count - 1))) / icons.Count;
                                            int max = Math.Min(STACK_CARD_ICON_MAX_PX_202602xx, Math.Min(inner.Height, maxCellW));

                                            if (max > 0)
                                            {
                                                var dws = new int[icons.Count];
                                                var dhs = new int[icons.Count];

                                                int groupW = 0;

                                                for (int k = 0; k < icons.Count; k++)
                                                {
                                                    var img = icons[k];
                                                    if (img == null)
                                                    {
                                                        dws[k] = 0;
                                                        dhs[k] = 0;
                                                        continue;
                                                    }

                                                    int iw = img.Width;
                                                    int ih = img.Height;

                                                    if (iw <= 0 || ih <= 0)
                                                    {
                                                        dws[k] = 0;
                                                        dhs[k] = 0;
                                                        continue;
                                                    }

                                                    float s = Math.Min((float)max / iw, (float)max / ih);
                                                    int dw = Math.Max(1, (int)(iw * s));
                                                    int dh = Math.Max(1, (int)(ih * s));

                                                    dws[k] = dw;
                                                    dhs[k] = dh;

                                                    groupW += dw;
                                                    if (k != icons.Count - 1)
                                                        groupW += gap;
                                                }

                                                if (groupW > 0)
                                                {
                                                    int startX = inner.Left + (inner.Width - groupW) / 2;
                                                    int x = startX;

                                                    for (int k = 0; k < icons.Count; k++)
                                                    {
                                                        var img = icons[k];
                                                        int dw = dws[k];
                                                        int dh = dhs[k];

                                                        if (img != null && dw > 0 && dh > 0)
                                                        {
                                                            int y = inner.Top + (inner.Height - dh) / 2;
                                                            var dst = new Rectangle(x, y, dw, dh);
                                                            DrawImageWithAlpha(g, img, dst, iconAlpha);
                                                        }

                                                        x += dw;
                                                        if (k != icons.Count - 1)
                                                            x += gap;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        g.InterpolationMode = oldIM;
                                        g.PixelOffsetMode = oldPO;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (clipBeforeCard != null)
                        {
                            g.SetClip(clipBeforeCard, CombineMode.Replace);
                            clipBeforeCard.Dispose();
                        }
                    }
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
    }
}