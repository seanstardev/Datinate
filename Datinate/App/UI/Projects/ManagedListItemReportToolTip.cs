using datinate.shared;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using static datinate.shared.DatFilterHelper;

namespace datinate.app
{
    internal static class ManagedListItemReportToolTip
    {
        private static readonly ToolTip decisionToolTip;
        private static readonly System.Windows.Forms.Timer mouseExitTimer;

        private static ManagedListItemReport? decisionToolTipReport;
        private static string? decisionToolTipCacheKey;

        private static TooltipLine[] decisionToolTipLines = Array.Empty<TooltipLine>();
        private static int decisionToolTipLineCount;
        private static int decisionToolTipLayoutWidth;
        private static int decisionToolTipLayoutHeight;

        private static Rectangle decisionToolTipScreenBounds;
        private static bool decisionToolTipPinned;

        private static Color decisionToolTipBackColor = SystemColors.Info;
        private static Color decisionToolTipForeColor = SystemColors.InfoText;
        private static Color decisionToolTipBorderColor = SystemColors.InfoText;

        private static Font? decisionToolTipBoldFont;
        private static Font? decisionToolTipHeaderFont;
        private static Font? decisionToolTipHeaderFont2;

        private static Control? activeOrigin;

        private const int DecisionToolTipMaxTextWidth = 900;
        private const int DecisionToolTipMaxTotalHeight = 720;
        private const int DecisionToolTipMinTotalHeight = 380;
        private const int DecisionToolTipIndentPx = 22;
        private const int DecisionToolTipHeaderGapPx = 12;

        private const TextFormatFlags DecisionToolTipTextFlags =
            TextFormatFlags.Left |
            TextFormatFlags.Top |
            TextFormatFlags.WordBreak |
            TextFormatFlags.NoPadding |
            TextFormatFlags.NoClipping;

        static ManagedListItemReportToolTip()
        {
            decisionToolTip = new ToolTip
            {
                OwnerDraw = true,
                UseFading = false,
                UseAnimation = false,
                AutoPopDelay = int.MaxValue,
                InitialDelay = 0,
                ReshowDelay = 0
            };

            decisionToolTip.Popup += DecisionToolTip_Popup;
            decisionToolTip.Draw += DecisionToolTip_Draw;

            mouseExitTimer = new System.Windows.Forms.Timer { Interval = 30 };
            mouseExitTimer.Tick += MouseExitTimer_Tick;
        }

        public static void Show(Control origin, ManagedListItemReport report, bool pinned = true, Point? screenPoint = null)
        {
            if (origin == null)
                return;

            if (report == null)
                return;

            if (!ReferenceEquals(activeOrigin, origin))
            {
                UnhookOrigin(activeOrigin);
                activeOrigin = origin;
                HookOrigin(activeOrigin);
            }

            decisionToolTipPinned = pinned;
            decisionToolTipReport = report;
            decisionToolTipCacheKey = null;

            ApplyTheme(report.IsExcluded);
            EnsureDecisionToolTipFonts(origin.Font);
            BuildDecisionToolTipLayout(report, origin.Font);

            var pt = screenPoint ?? Control.MousePosition;

            var offsetX = 16;
            var offsetY = 20;

            var x = pt.X + offsetX;
            var y = pt.Y + offsetY;

            var wa = Screen.FromPoint(pt).WorkingArea;

            if (x + decisionToolTipLayoutWidth > wa.Right)
                x = Math.Max(wa.Left, wa.Right - decisionToolTipLayoutWidth - 4);

            if (y + decisionToolTipLayoutHeight > wa.Bottom)
                y = Math.Max(wa.Top, wa.Bottom - decisionToolTipLayoutHeight - 4);

            decisionToolTipScreenBounds = new Rectangle(new Point(x, y), new Size(decisionToolTipLayoutWidth, decisionToolTipLayoutHeight));

            var local = origin.PointToClient(new Point(x, y));
            decisionToolTip.Show(" ", origin, local.X, local.Y);

            if (decisionToolTipPinned && decisionToolTipScreenBounds.Contains(Cursor.Position))
                mouseExitTimer.Start();
            else
                mouseExitTimer.Stop();
        }

        public static void Hide()
        {
            mouseExitTimer.Stop();

            decisionToolTipPinned = false;
            decisionToolTipReport = null;
            decisionToolTipCacheKey = null;
            decisionToolTipLines = Array.Empty<TooltipLine>();
            decisionToolTipLineCount = 0;

            if (activeOrigin != null)
            {
                decisionToolTip.Hide(activeOrigin);
                UnhookOrigin(activeOrigin);
                activeOrigin = null;
            }
        }

        private static string RenderToPngBase64(
            ManagedListItemReport report, 
            out int widthPx, 
            out int heightPx, 
            Font? font = null)
        {
            widthPx = 0;
            heightPx = 0;

            if (report == null)
                return string.Empty;

            var baseFont = font ?? SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;

            ApplyTheme(report.IsExcluded);
            EnsureDecisionToolTipFonts(baseFont);
            BuildDecisionToolTipLayout(report, baseFont);

            var w = Math.Max(1, decisionToolTipLayoutWidth);
            var h = Math.Max(1, decisionToolTipLayoutHeight);

            widthPx = w;
            heightPx = h;

            decisionToolTipReport = report;

            return GdiScreenBitmapRenderer.RenderToPngBase64(w, h, g =>
            {
                DrawDecisionToolTipCore(g, new Rectangle(0, 0, w, h), baseFont);
            });
        }

        public static string? RenderToPngDataUri(ManagedListItemReport report, out int widthPx, out int heightPx, Font? font = null)
        {
            var b64 = RenderToPngBase64(report, out widthPx, out heightPx, font);
            if (string.IsNullOrEmpty(b64))
                return null;

            return "data:image/png;base64," + b64;
        }

        public static string? CreateHtmlFragment(ManagedListItemReport report, Font? font = null)
        {
            var uri = RenderToPngDataUri(report, out var w, out var h, font);
            if (string.IsNullOrEmpty(uri))
                return null;

            return
                "<img " +
                "src=\"" + uri + "\" " +
                "width=\"" + w + "\" " +
                "height=\"" + h + "\" " +
                "style=\"display:block;width:" + w + "px;height:" + h + "px;\" " +
                "alt=\"\" />";
        }


        private static void HookOrigin(Control? origin)
        {
            if (origin == null)
                return;

            origin.MouseLeave += Origin_MouseLeave;
            origin.Disposed += Origin_Disposed;
        }
        private static void DrawDecisionToolTipCore(Graphics g, Rectangle bounds, Font baseFont)
        {
            if (decisionToolTipReport == null)
                return;

            EnsureDecisionToolTipFonts(baseFont);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (var backBrush = new SolidBrush(decisionToolTipBackColor))
            using (var borderPen = new Pen(decisionToolTipBorderColor))
            {
                g.FillRectangle(backBrush, bounds);
                g.DrawRectangle(borderPen, new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1));
            }

            var content = Rectangle.Inflate(bounds, -10, -8);

            var headerFont = decisionToolTipHeaderFont ?? baseFont;
            var headerFont2 = decisionToolTipHeaderFont2 ?? baseFont;
            var boldFont = decisionToolTipBoldFont ?? baseFont;

            var w = Math.Max(1, content.Width);
            var y = content.Y;

            for (int i = 0; i < decisionToolTipLineCount; i++)
            {
                var line = decisionToolTipLines[i];

                y += line.ExtraTopPx;
                if (y > content.Bottom)
                    break;

                var x = content.X + line.IndentPx;

                if (line.IsHeader)
                {
                    var useFont = line.UseHeader2 ? headerFont2 : headerFont;

                    var headerTextH = TextRenderer.MeasureText("A", useFont).Height;
                    var rectY = y - 2;
                    var rectH = headerTextH + 8;

                    var headerBack = Color.FromArgb(180, 255, 255, 255);

                    var bgRect = new Rectangle(content.X, rectY, content.Width, rectH);
                    using (var hb = new SolidBrush(headerBack))
                        g.FillRectangle(hb, bgRect);

                    var headerRect = new Rectangle(x, rectY + 3, w, rectH);

                    TextRenderer.DrawText(
                        g,
                        line.Text,
                        useFont,
                        headerRect,
                        decisionToolTipForeColor,
                        TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.NoClipping | TextFormatFlags.SingleLine);

                    var underlineY = rectY + rectH - 2;
                    if (underlineY < content.Bottom)
                    {
                        using (var underlinePen = new Pen(Color.FromArgb(140, decisionToolTipBorderColor)))
                            g.DrawLine(underlinePen, content.X, underlineY, content.Right - 1, underlineY);
                    }

                    y += line.HeightPx;
                    continue;
                }

                if (!string.IsNullOrEmpty(line.ChipText))
                {
                    var chipTextSize = TextRenderer.MeasureText(
                        line.ChipText,
                        boldFont,
                        new Size(DecisionToolTipMaxTextWidth, int.MaxValue),
                        TextFormatFlags.NoPadding | TextFormatFlags.NoClipping | TextFormatFlags.SingleLine);

                    const int ChipPadXPx = 10;
                    const int ChipPadYPx = 4;

                    var chipW = chipTextSize.Width + (ChipPadXPx * 2);
                    var chipH = chipTextSize.Height + (ChipPadYPx * 2);

                    var chipRect = new Rectangle(x, y, chipW, chipH);

                    using (var b = new SolidBrush(line.ChipBackColor))
                        g.FillRectangle(b, chipRect);

                    using (var p = new Pen(line.ChipBorderColor))
                        g.DrawRectangle(p, chipRect);

                    var tx = chipRect.X + ChipPadXPx;
                    var ty = chipRect.Y + ChipPadYPx;

                    TextRenderer.DrawText(
                        g,
                        line.ChipText,
                        boldFont,
                        new Rectangle(tx, ty, Math.Max(1, chipRect.Width - (ChipPadXPx * 2)), Math.Max(1, chipRect.Height - (ChipPadYPx * 2))),
                        decisionToolTipForeColor,
                        TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.NoClipping | TextFormatFlags.SingleLine);

                    y += line.HeightPx;
                    continue;
                }

                if (line.PrefixWidthPx > 0 && !string.IsNullOrEmpty(line.BoldTail))
                {
                    const int KeyValueGapPx = 12;

                    var keyRect = new Rectangle(x, y, line.PrefixWidthPx, line.HeightPx);
                    var valRect = new Rectangle(
                        x + line.PrefixWidthPx + KeyValueGapPx,
                        y,
                        Math.Max(1, w - line.PrefixWidthPx - KeyValueGapPx),
                        line.HeightPx);

                    TextRenderer.DrawText(
                        g,
                        line.BoldTail,
                        boldFont,
                        keyRect,
                        decisionToolTipForeColor,
                        TextFormatFlags.NoPadding | TextFormatFlags.NoClipping | TextFormatFlags.Right | TextFormatFlags.Top | TextFormatFlags.SingleLine);

                    TextRenderer.DrawText(
                        g,
                        line.Text,
                        baseFont,
                        valRect,
                        decisionToolTipForeColor,
                        DecisionToolTipTextFlags);

                    y += line.HeightPx;
                    continue;
                }

                TextRenderer.DrawText(
                    g,
                    line.Text,
                    baseFont,
                    new Rectangle(x, y, w - line.IndentPx, line.HeightPx),
                    decisionToolTipForeColor,
                    DecisionToolTipTextFlags);

                y += line.HeightPx;
            }
        }

        private static void UnhookOrigin(Control? origin)
        {
            if (origin == null)
                return;

            origin.MouseLeave -= Origin_MouseLeave;
            origin.Disposed -= Origin_Disposed;
        }

        private static void Origin_Disposed(object? sender, EventArgs e)
        {
            Hide();
        }

        private static void Origin_MouseLeave(object? sender, EventArgs e)
        {
            if (!decisionToolTipPinned)
            {
                Hide();
                return;
            }

            var mouse = Control.MousePosition;
            if (decisionToolTipScreenBounds.Contains(mouse))
            {
                mouseExitTimer.Start();
                return;
            }

            Hide();
        }

        private static void MouseExitTimer_Tick(object? sender, EventArgs e)
        {
            if (!decisionToolTipPinned)
                return;

            if (!decisionToolTipScreenBounds.Contains(Cursor.Position))
                Hide();
        }

        private static void ApplyTheme(bool themeIsExclude)
        {
            if (themeIsExclude)
            {
                decisionToolTipBackColor = Color.FromArgb(255, 240, 240);
                decisionToolTipBorderColor = Color.FromArgb(220, 70, 70);
                decisionToolTipForeColor = SystemColors.InfoText;
            }
            else
            {
                decisionToolTipBackColor = Color.FromArgb(240, 255, 240);
                decisionToolTipBorderColor = Color.FromArgb(70, 170, 70);
                decisionToolTipForeColor = SystemColors.InfoText;
            }
        }

        private static void DecisionToolTip_Popup(object? sender, PopupEventArgs e)
        {
            if (decisionToolTipReport == null)
            {
                e.Cancel = true;
                return;
            }

            var baseFont = e.AssociatedControl?.Font ?? SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont; ;
            EnsureDecisionToolTipFonts(baseFont);

            var key = CreateCacheKey(decisionToolTipReport);
            if (!string.Equals(decisionToolTipCacheKey, key, StringComparison.Ordinal))
                BuildDecisionToolTipLayout(decisionToolTipReport, baseFont);

            e.ToolTipSize = new Size(decisionToolTipLayoutWidth, decisionToolTipLayoutHeight);
        }
        private static void DecisionToolTip_Draw(object? sender, DrawToolTipEventArgs e)
        {
            if (decisionToolTipReport == null)
                return;

            var baseFont = e.Font ?? SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;
            DrawDecisionToolTipCore(e.Graphics, e.Bounds, baseFont);
        }

        private static string CreateCacheKey(ManagedListItemReport report)
        {
            var decisiveIdx = report.DecisiveFilterPriorityIndex1Based?.ToString() ?? string.Empty;
            var decisiveExpr = report.DecisiveFilterExpression ?? string.Empty;
            var decisiveEff = report.DecisiveFilterEffectName ?? string.Empty;

            var cat = report.EntryCategoryLabel ?? string.Empty;

            return string.Join("|",
                report.EntryName,
                cat,
                report.IsExcluded ? "X" : "I",
                report.FinalAction.ToString(),
                report.MatchedFilterCount.ToString(),
                decisiveIdx,
                report.DecisiveFilterIsCategory ? "CAT" : string.Empty,
                decisiveEff,
                decisiveExpr,
                report.ConditionalStripCount.ToString(),
                report.StrippedName ?? string.Empty,
                report.CleanedKey ?? string.Empty,
                report.BestInGroupScore?.ToString() ?? string.Empty);
        }

        private static void BuildDecisionToolTipLayout(ManagedListItemReport report, Font baseFont)
        {
            const int KeyValueGapPx = 12;
            const int ChipPadXPx = 10;
            const int ChipPadYPx = 4;

            EnsureDecisionToolTipFonts(baseFont);

            var boldFont = decisionToolTipBoldFont ?? baseFont;
            var headerFont = decisionToolTipHeaderFont ?? baseFont;
            var headerFont2 = decisionToolTipHeaderFont2 ?? baseFont;

            var themeIsExclude = report.IsExcluded;
            var chipBack = themeIsExclude ? Color.FromArgb(255, 230, 230) : Color.FromArgb(230, 255, 230);
            var chipBorder = decisionToolTipBorderColor;

            var tmp = new List<TooltipLine>(80);

            var maxW = 0;
            var totalH = 0;

            void AddHeader(string text, bool useHeader2 = false)
            {
                var extraTop = tmp.Count == 0 ? 0 : DecisionToolTipHeaderGapPx;
                var font = useHeader2 ? headerFont2 : headerFont;

                var sz = TextRenderer.MeasureText(
                    text,
                    font,
                    new Size(DecisionToolTipMaxTextWidth, int.MaxValue),
                    TextFormatFlags.NoPadding | TextFormatFlags.NoClipping | TextFormatFlags.SingleLine);

                var h = sz.Height + 10;

                tmp.Add(new TooltipLine(text, null, 0, isHeader: true, useHeader2, extraTop, 0, h, null, Color.Empty, Color.Empty));
                totalH += extraTop + h;
                maxW = Math.Max(maxW, sz.Width);
            }

            void AddParagraph(string text, int indentPx = 0, int extraTopPx = 0)
            {
                var sz = TextRenderer.MeasureText(
                    text,
                    baseFont,
                    new Size(Math.Max(1, DecisionToolTipMaxTextWidth - indentPx), int.MaxValue),
                    DecisionToolTipTextFlags);

                var h = sz.Height + 6;

                tmp.Add(new TooltipLine(text, null, indentPx, isHeader: false, useHeader2: false, extraTopPx, 0, h, null, Color.Empty, Color.Empty));
                totalH += extraTopPx + h;
                maxW = Math.Max(maxW, indentPx + sz.Width);
            }

            void AddKeyValues((string Key, string Value)[] items, int indentPx = 0, int extraTopPxFirst = 0)
            {
                if (items.Length == 0)
                    return;

                var keyW = 0;
                for (int i = 0; i < items.Length; i++)
                {
                    var ks = TextRenderer.MeasureText(
                        items[i].Key,
                        boldFont,
                        new Size(DecisionToolTipMaxTextWidth, int.MaxValue),
                        TextFormatFlags.NoPadding | TextFormatFlags.NoClipping | TextFormatFlags.SingleLine);

                    keyW = Math.Max(keyW, ks.Width);
                }

                for (int i = 0; i < items.Length; i++)
                {
                    var extraTop = i == 0 ? extraTopPxFirst : 0;

                    var availValW = Math.Max(1, DecisionToolTipMaxTextWidth - indentPx - keyW - KeyValueGapPx);

                    var vs = TextRenderer.MeasureText(
                        items[i].Value,
                        baseFont,
                        new Size(availValW, int.MaxValue),
                        DecisionToolTipTextFlags);

                    var lineH = Math.Max(vs.Height + 6, TextRenderer.MeasureText("A", baseFont).Height + 6);

                    tmp.Add(new TooltipLine(items[i].Value, items[i].Key, indentPx, isHeader: false, useHeader2: false, extraTop, keyW, lineH, null, Color.Empty, Color.Empty));
                    totalH += extraTop + lineH;

                    maxW = Math.Max(maxW, indentPx + keyW + KeyValueGapPx + vs.Width);
                }
            }

            void AddChip(string chipText, int indentPx = DecisionToolTipIndentPx, int extraTopPx = 0)
            {
                var chipTextSize = TextRenderer.MeasureText(
                    chipText,
                    boldFont,
                    new Size(DecisionToolTipMaxTextWidth, int.MaxValue),
                    TextFormatFlags.NoPadding | TextFormatFlags.NoClipping | TextFormatFlags.SingleLine);

                var chipW = chipTextSize.Width + (ChipPadXPx * 2);
                var chipH = chipTextSize.Height + (ChipPadYPx * 2);

                tmp.Add(new TooltipLine(string.Empty, null, indentPx, isHeader: false, useHeader2: false, extraTopPx, 0, chipH, chipText, chipBack, chipBorder));
                totalH += extraTopPx + chipH;

                maxW = Math.Max(maxW, indentPx + chipW);
            }

            string OutcomeDetail()
            {
                if (report.IsExcluded)
                {
                    if (report.HasDecisiveFilter && report.DecisiveFilterIsCategory)
                        return "Excluded by category rule";

                    if (report.HasDecisiveFilter)
                        return "Excluded by explicit filter";

                    if (report.FinalAction == EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL)
                        return "Conditional reject";

                    return "Rejected";
                }

                if (report.HasDecisiveFilter && report.DecisiveFilterIsCategory)
                    return "Included by category rule";

                if (report.HasDecisiveFilter)
                    return "Included by explicit filter";

                if (report.FinalAction == EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL)
                    return "Conditional include";

                if (report.FinalAction == EXPRESSION_ACTION_ENUM.INCLUDE)
                    return "Include";

                return "Implicit include";
            }

            AddHeader("Dat Customisation Outcome");

            var outcome = report.IsExcluded ? "EXCLUDED" : "INCLUDED";

            AddKeyValues(
                new[]
                {
                    (outcome, OutcomeDetail()),
                    ("Entry", report.EntryName),
                    ("Category", string.IsNullOrWhiteSpace(report.EntryCategoryLabel) ? "(none)" : report.EntryCategoryLabel!)
                },
                indentPx: 0,
                extraTopPxFirst: 10);

            AddHeader("Decision");

            if (report.HasDecisiveFilter)
            {
                var idx = report.DecisiveFilterPriorityIndex1Based!.Value.ToString();
                var eff = report.DecisiveFilterEffectName ?? string.Empty;
                var expr = report.DecisiveFilterExpression ?? string.Empty;
                var tag = report.DecisiveFilterIsCategory ? " (category)" : string.Empty;

                AddParagraph("Decisive filter.", DecisionToolTipIndentPx, 6);
                AddKeyValues(
                    new[]
                    {
                        ("Priority", idx),
                        ("Effect", eff + tag),
                        ("Expression", expr)
                    },
                    indentPx: DecisionToolTipIndentPx,
                    extraTopPxFirst: 4);
            }
            else
            {
                AddParagraph("Resolved by conditional scoring.", DecisionToolTipIndentPx, 6);
                AddKeyValues(
                    new[]
                    {
                        ("Your strips", report.ConditionalStripCount.ToString()),
                        ("Best in group", report.BestInGroupScore.HasValue ? report.BestInGroupScore.Value.ToString() : "n/a")
                    },
                    indentPx: DecisionToolTipIndentPx,
                    extraTopPxFirst: 4);
            }

            AddHeader("Name Management");

            AddParagraph("The name after removing conditional segments from the entry name. Entries with the same cleaned key are treated as competing variants.", 0, 6);
            AddParagraph("");

            AddKeyValues(
                new[]
                {
                    ("Original", report.EntryName),
                    ("Cleaned key", report.CleanedKey ?? "n/a")
                },
                indentPx: 0,
                extraTopPxFirst: 6);

            AddHeader("Conditional Expression Matches");

            if (report.MatchedConditionals.Count == 0)
            {
                AddParagraph("(none)", DecisionToolTipIndentPx, 6);
            }
            else
            {
                for (int i = 0; i < report.MatchedConditionals.Count; i++)
                {
                    var m = report.MatchedConditionals[i];
                    var prefix = m.IsCategory ? "CAT " : string.Empty;
                    var chip = string.Concat(prefix, m.EffectName, "   ", m.Expression);
                    AddChip(chip, DecisionToolTipIndentPx, 6);
                }
            }

            AddHeader("Matched Expressions");

            AddKeyValues(
                new[]
                {
                    ("Count", report.MatchedFilterCount.ToString())
                },
                indentPx: DecisionToolTipIndentPx,
                extraTopPxFirst: 6);

            if (report.Trace.Count > 0)
            {
                var max = Math.Min(report.Trace.Count, 48);
                for (int i = 0; i < max; i++)
                {
                    var t = report.Trace[i];
                    var catTag = t.IsCategory ? " [CAT]" : string.Empty;
                    var line = string.Concat(t.Index1Based.ToString(), ". ", t.ExpressionActionName, catTag, "   ", t.UserFriendlyExpression);
                    AddParagraph(line, DecisionToolTipIndentPx, i == 0 ? 10 : 4);
                }

                if (report.Trace.Count > max)
                    AddParagraph("...", DecisionToolTipIndentPx, 4);
            }

            decisionToolTipLines = tmp.ToArray();
            decisionToolTipLineCount = decisionToolTipLines.Length;

            decisionToolTipLayoutWidth = Math.Min(DecisionToolTipMaxTextWidth, maxW) + 18;

            var h = totalH + 16;
            if (h < DecisionToolTipMinTotalHeight) h = DecisionToolTipMinTotalHeight;
            if (h > DecisionToolTipMaxTotalHeight) h = DecisionToolTipMaxTotalHeight;

            decisionToolTipLayoutHeight = h;
            decisionToolTipCacheKey = CreateCacheKey(report);
        }

        private static void EnsureDecisionToolTipFonts(Font baseFont)
        {
            if (decisionToolTipHeaderFont != null &&
                decisionToolTipBoldFont != null &&
                decisionToolTipHeaderFont2 != null &&
                decisionToolTipHeaderFont.Name == baseFont.Name &&
                Math.Abs(decisionToolTipHeaderFont.Size - (baseFont.Size + 0.0f)) < 0.01f &&
                decisionToolTipBoldFont.Name == baseFont.Name &&
                Math.Abs(decisionToolTipBoldFont.Size - baseFont.Size) < 0.01f &&
                decisionToolTipHeaderFont2.Name == baseFont.Name &&
                Math.Abs(decisionToolTipHeaderFont2.Size - (baseFont.Size + 1.0f)) < 0.01f)
            {
                return;
            }

            decisionToolTipBoldFont?.Dispose();
            decisionToolTipHeaderFont?.Dispose();
            decisionToolTipHeaderFont2?.Dispose();

            decisionToolTipBoldFont = new Font(baseFont, FontStyle.Bold);
            decisionToolTipHeaderFont = new Font(baseFont.FontFamily, baseFont.Size + 0.0f, FontStyle.Bold);
            decisionToolTipHeaderFont2 = new Font(baseFont.FontFamily, baseFont.Size + 1.0f, FontStyle.Bold);
        }

        private readonly struct TooltipLine
        {
            public readonly string Text;
            public readonly string? BoldTail;
            public readonly int IndentPx;
            public readonly bool IsHeader;
            public readonly bool UseHeader2;
            public readonly int ExtraTopPx;
            public readonly int PrefixWidthPx;
            public readonly int HeightPx;

            public readonly string? ChipText;
            public readonly Color ChipBackColor;
            public readonly Color ChipBorderColor;

            public TooltipLine(
                string text,
                string? boldTail,
                int indentPx,
                bool isHeader,
                bool useHeader2,
                int extraTopPx,
                int prefixWidthPx,
                int heightPx,
                string? chipText,
                Color chipBackColor,
                Color chipBorderColor)
            {
                Text = text;
                BoldTail = boldTail;
                IndentPx = indentPx;
                IsHeader = isHeader;
                UseHeader2 = useHeader2;
                ExtraTopPx = extraTopPx;
                PrefixWidthPx = prefixWidthPx;
                HeightPx = heightPx;
                ChipText = chipText;
                ChipBackColor = chipBackColor;
                ChipBorderColor = chipBorderColor;
            }
        }
    }
}
