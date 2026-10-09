using System.Drawing.Imaging;

namespace datinate.app
{
    internal static class DatGrouperPartToolTip
    {
        private const int MaxWidthPx = 720;
        private const int MaxHeightPx = 520;

        private const int PadLeftPx = 10;
        private const int PadTopPx = 8;
        private const int PadRightPx = 10;
        private const int PadBottomPx = 8;

        private const int IndentPx = 14;
        private const int HeaderTopPadPx = 6;
        private const int HeaderBottomPadPx = 6;
        private const int AfterHeaderGapPx = 6;

        private const int TitleTopPadPx = 8;
        private const int TitleBottomPadPx = 8;
        private const int AfterTitleGapPx = 8;

        private const int SubHeaderTopGapPx = 4;
        private const int SubHeaderBottomGapPx = 2;

        private const int KeyValueGapPx = 12;
        private const int LineGapPx = 2;
        private const int BulletGapPx = 6;

        private static readonly Color HeaderBack = Color.FromArgb(200, 255, 255, 255);
        private static readonly Color TitleBack = Color.FromArgb(225, 255, 255, 240);

        private static Font? baseFont;
        private static Font? headerFont;
        private static Font? boldFont;
        private static Font? titleFont;

        private enum LayoutMode
        {
            Tooltip,
            FullImage
        }

        private static string RenderToPngBase64(string text, out int widthPx, out int heightPx, Font font, LayoutMode mode)
        {
            widthPx = 0;
            heightPx = 0;

            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            EnsureFonts(font);

            Layout layout;
            using (var gMeasure = Graphics.FromHwnd(IntPtr.Zero))
                layout = BuildLayout(text, gMeasure, mode);

            var w = layout.WidthPx;
            var h = layout.HeightPx;

            if (w < 1) w = 1;
            if (h < 1) h = 1;

            widthPx = w;
            heightPx = h;

            return GdiScreenBitmapRenderer.RenderToPngBase64(w, h, g =>
            {
                g.FillRectangle(SystemBrushes.Info, new Rectangle(0, 0, w, h));
                DrawLayout(g, new Rectangle(0, 0, w, h), layout);
                using var pen = new Pen(SystemColors.InfoText, 1f);
                g.DrawRectangle(pen, 0, 0, w - 1, h - 1);
            });
        }

        public static string? RenderToPngDataUri(string text, out int widthPx, out int heightPx, Control control, bool fullHeight = false)
        {
            Font font = control.Font;

            var b64 = RenderToPngBase64(
                text,
                out widthPx,
                out heightPx,
                font,
                fullHeight ? LayoutMode.FullImage : LayoutMode.Tooltip);

            if (string.IsNullOrEmpty(b64))
                return null;

            return "data:image/png;base64," + b64;
        }

        public static string? CreateHtmlFragment(string text, Control dpiSource)
        {
            var dataUri = RenderToPngDataUri(text, out int widthPx, out int heightPx, dpiSource, fullHeight: true);
            if (string.IsNullOrEmpty(dataUri))
                return null;

            var scale = 1f;

            if (dpiSource != null && !dpiSource.IsDisposed)
                scale = Math.Max(1f, dpiSource.DeviceDpi / 96f);

            var w = (int)Math.Round(widthPx / scale);
            var h = (int)Math.Round(heightPx / scale);

            if (w < 1) w = 1;
            if (h < 1) h = 1;

            return
                "<img " +
                "src=\"" + dataUri + "\" " +
                "width=\"" + w + "\" " +
                "height=\"" + h + "\" " +
                "style=\"display:block; width:" + w + "px; height:" + h + "px;\" " +
                "alt=\"\" />";
        }

        private static void EnsureFonts(Font font)
        {
            if (baseFont != null &&
                headerFont != null &&
                boldFont != null &&
                titleFont != null &&
                baseFont.Name == font.Name &&
                Math.Abs(baseFont.Size - font.Size) < 0.01f &&
                baseFont.Style == font.Style)
            {
                return;
            }

            baseFont = font;

            headerFont?.Dispose();
            boldFont?.Dispose();
            titleFont?.Dispose();

            headerFont = new Font(baseFont.FontFamily, Math.Max(8f, baseFont.Size + 0.5f), FontStyle.Bold);
            boldFont = new Font(baseFont, FontStyle.Bold);
            titleFont = new Font(baseFont.FontFamily, Math.Max(9f, baseFont.Size + 1.5f), FontStyle.Bold);
        }

        private static Layout BuildLayout(string text, Graphics g, LayoutMode mode)
        {
            var bf = baseFont ?? SystemFonts.MessageBoxFont;
            var hf = headerFont ?? bf;
            var tf = titleFont ?? hf;
            var bdf = boldFont ?? bf;

            var rawLines = text.Replace("\r\n", "\n").Split('\n');

            var items = new List<Item>(rawLines.Length);
            var keyColWidth = 0;
            var foundTitle = false;

            for (var i = 0; i < rawLines.Length; i++)
            {
                var raw = rawLines[i].TrimEnd();

                if (raw.Length == 0)
                {
                    if (items.Count == 0 || items[^1].Kind == ItemKind.Blank)
                        continue;

                    items.Add(new Item(ItemKind.Blank, 0, string.Empty, null));
                    continue;
                }

                var tabCount = 0;
                while (tabCount < raw.Length && raw[tabCount] == '\t')
                    tabCount++;

                var s = tabCount == 0 ? raw : raw.Substring(tabCount);

                if (s.StartsWith("## ", StringComparison.Ordinal))
                {
                    var heading = s.Substring(3).Trim();
                    var kind = foundTitle ? ItemKind.Header : ItemKind.TitleHeader;
                    foundTitle = true;
                    items.Add(new Item(kind, 0, heading, null));
                    continue;
                }

                var parts = s.Split('\t');
                if (parts.Length >= 2 && parts[0].Length > 0)
                {
                    var key = parts[0].TrimEnd();
                    var val = string.Join("\t", parts.Skip(1)).Trim();

                    if (string.IsNullOrWhiteSpace(val))
                        continue;

                    var w = TextRenderer.MeasureText(g, key, bdf, new Size(int.MaxValue, int.MaxValue),
                        TextFormatFlags.NoPrefix | TextFormatFlags.NoClipping).Width;

                    if (w > keyColWidth)
                        keyColWidth = w;

                    items.Add(new Item(ItemKind.KeyValue, tabCount, key, val));
                }
                else if (s.StartsWith("- ", StringComparison.Ordinal))
                {
                    items.Add(new Item(ItemKind.Bullet, tabCount, s.Substring(2).Trim(), null));
                }
                else if (tabCount > 0 && s.EndsWith(":", StringComparison.Ordinal))
                {
                    items.Add(new Item(ItemKind.SubHeader, tabCount, s, null));
                }
                else
                {
                    items.Add(new Item(ItemKind.Text, tabCount, s, null));
                }
            }

            while (items.Count > 0 && items[^1].Kind == ItemKind.Blank)
                items.RemoveAt(items.Count - 1);

            var innerMaxWidth = MaxWidthPx - PadLeftPx - PadRightPx;
            if (innerMaxWidth < 120)
                innerMaxWidth = 120;

            var y = PadTopPx;
            var widthUsed = 0;

            var measured = new List<MeasuredItem>(items.Count);

            for (var i = 0; i < items.Count; i++)
            {
                var it = items[i];

                if (it.Kind == ItemKind.Blank)
                {
                    var h = TextRenderer.MeasureText(g, "A", bf).Height + 4;
                    measured.Add(new MeasuredItem(it, 0, h));
                    y += h;
                    continue;
                }

                if (it.Kind == ItemKind.TitleHeader)
                {
                    var hText = TextRenderer.MeasureText(g, it.Text, tf, new Size(innerMaxWidth, int.MaxValue),
                        TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping).Height;

                    var h = TitleTopPadPx + hText + TitleBottomPadPx + AfterTitleGapPx;
                    measured.Add(new MeasuredItem(it, innerMaxWidth, h));
                    y += h;
                    widthUsed = Math.Max(widthUsed, innerMaxWidth);
                    continue;
                }

                if (it.Kind == ItemKind.Header)
                {
                    var hText = TextRenderer.MeasureText(g, it.Text, hf, new Size(innerMaxWidth, int.MaxValue),
                        TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping).Height;

                    var h = HeaderTopPadPx + hText + HeaderBottomPadPx + AfterHeaderGapPx;
                    measured.Add(new MeasuredItem(it, innerMaxWidth, h));
                    y += h;
                    widthUsed = Math.Max(widthUsed, innerMaxWidth);
                    continue;
                }

                var indent = it.IndentTabs * IndentPx;
                var available = innerMaxWidth - indent;
                if (available < 80)
                    available = 80;

                if (it.Kind == ItemKind.SubHeader)
                {
                    var hText = TextRenderer.MeasureText(g, it.Text, bdf, new Size(available, int.MaxValue),
                        TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping).Height;

                    var h = SubHeaderTopGapPx + hText + SubHeaderBottomGapPx;
                    measured.Add(new MeasuredItem(it, available, h));
                    y += h;
                    widthUsed = Math.Max(widthUsed, indent + available);
                    continue;
                }

                if (it.Kind == ItemKind.Bullet)
                {
                    var bulletW = TextRenderer.MeasureText(g, "•", bf, new Size(int.MaxValue, int.MaxValue),
                        TextFormatFlags.NoPrefix | TextFormatFlags.NoClipping).Width + BulletGapPx;

                    var textW = available - bulletW;
                    if (textW < 60)
                        textW = 60;

                    var hText = TextRenderer.MeasureText(g, it.Text, bf, new Size(textW, int.MaxValue),
                        TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping).Height;

                    var h = hText + LineGapPx;
                    measured.Add(new MeasuredItem(it, available, h));
                    y += h;
                    widthUsed = Math.Max(widthUsed, indent + available);
                    continue;
                }

                if (it.Kind == ItemKind.Text)
                {
                    var hText = TextRenderer.MeasureText(g, it.Text, bf, new Size(available, int.MaxValue),
                        TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping).Height;

                    var h = hText + LineGapPx;
                    measured.Add(new MeasuredItem(it, available, h));
                    y += h;
                    widthUsed = Math.Max(widthUsed, indent + available);
                    continue;
                }

                var keyW = Math.Min(keyColWidth, Math.Max(80, available / 2));
                var valW = available - keyW - KeyValueGapPx;

                if (valW < 80)
                {
                    keyW = Math.Min(keyW, available - 80 - KeyValueGapPx);
                    valW = available - keyW - KeyValueGapPx;
                    if (valW < 80)
                        valW = 80;
                }

                var keyH = TextRenderer.MeasureText(g, it.Text, bdf, new Size(keyW, int.MaxValue),
                    TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping).Height;

                var valText = it.Value ?? string.Empty;
                var valH = TextRenderer.MeasureText(g, valText, bf, new Size(valW, int.MaxValue),
                    TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping).Height;

                var hKV = Math.Max(keyH, valH) + LineGapPx;

                measured.Add(new MeasuredItem(it, available, hKV));
                y += hKV;

                widthUsed = Math.Max(widthUsed, indent + available);
            }

            var totalW = PadLeftPx + widthUsed + PadRightPx;
            var unclippedTotalH = y + PadBottomPx;

            if (totalW < 140) totalW = 140;
            if (unclippedTotalH < 40) unclippedTotalH = 40;

            if (totalW > MaxWidthPx) totalW = MaxWidthPx;

            var clippedHeight = mode == LayoutMode.Tooltip
                ? Math.Min(unclippedTotalH, MaxHeightPx)
                : unclippedTotalH;

            var wasHeightClipped = clippedHeight < unclippedTotalH;

            return new Layout(measured, keyColWidth, totalW, clippedHeight, wasHeightClipped);
        }

        private static void DrawLayout(Graphics g, Rectangle bounds, Layout layout)
        {
            var bf = baseFont ?? SystemFonts.MessageBoxFont;
            var hf = headerFont ?? bf;
            var tf = titleFont ?? hf;
            var bdf = boldFont ?? bf;

            var innerMaxWidth = bounds.Width - PadLeftPx - PadRightPx;

            var x0 = bounds.Left + PadLeftPx;
            var y = bounds.Top + PadTopPx;

            var bottomLimit = bounds.Bottom - PadBottomPx;

            for (var i = 0; i < layout.Items.Count; i++)
            {
                var mi = layout.Items[i];
                var it = mi.Item;

                if (y >= bottomLimit)
                    break;

                if (it.Kind == ItemKind.Blank)
                {
                    y += mi.HeightPx;
                    continue;
                }

                if (it.Kind == ItemKind.TitleHeader)
                {
                    var titleRect = new Rectangle(x0, y, innerMaxWidth, mi.HeightPx - AfterTitleGapPx);
                    using (var b = new SolidBrush(TitleBack))
                        g.FillRectangle(b, titleRect);

                    var textRect = new Rectangle(x0 + 4, y + TitleTopPadPx, innerMaxWidth - 8, titleRect.Height - TitleTopPadPx - TitleBottomPadPx);

                    TextRenderer.DrawText(
                        g,
                        it.Text,
                        tf,
                        textRect,
                        SystemColors.InfoText,
                        TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping);

                    y += mi.HeightPx;
                    continue;
                }

                if (it.Kind == ItemKind.Header)
                {
                    var headerRect = new Rectangle(x0, y, innerMaxWidth, mi.HeightPx - AfterHeaderGapPx);
                    using (var b = new SolidBrush(HeaderBack))
                        g.FillRectangle(b, headerRect);

                    var textRect = new Rectangle(x0, y + HeaderTopPadPx, innerMaxWidth, headerRect.Height - HeaderTopPadPx - HeaderBottomPadPx);

                    TextRenderer.DrawText(
                        g,
                        it.Text,
                        hf,
                        textRect,
                        SystemColors.InfoText,
                        TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping);

                    y += mi.HeightPx;
                    continue;
                }

                var indent = it.IndentTabs * IndentPx;
                var x = x0 + indent;

                if (it.Kind == ItemKind.SubHeader)
                {
                    var rect = new Rectangle(x, y + SubHeaderTopGapPx, innerMaxWidth - indent, mi.HeightPx - SubHeaderTopGapPx - SubHeaderBottomGapPx);

                    TextRenderer.DrawText(
                        g,
                        it.Text,
                        bdf,
                        rect,
                        SystemColors.InfoText,
                        TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping);

                    y += mi.HeightPx;
                    continue;
                }

                if (it.Kind == ItemKind.Bullet)
                {
                    var available = innerMaxWidth - indent;
                    var bulletW = TextRenderer.MeasureText(g, "•", bf, new Size(int.MaxValue, int.MaxValue),
                        TextFormatFlags.NoPrefix | TextFormatFlags.NoClipping).Width + BulletGapPx;

                    var bulletRect = new Rectangle(x, y, bulletW, mi.HeightPx);
                    var textRect = new Rectangle(x + bulletW, y, available - bulletW, mi.HeightPx);

                    TextRenderer.DrawText(
                        g,
                        "•",
                        bf,
                        bulletRect,
                        SystemColors.InfoText,
                        TextFormatFlags.NoPrefix | TextFormatFlags.NoClipping);

                    TextRenderer.DrawText(
                        g,
                        it.Text,
                        bf,
                        textRect,
                        SystemColors.InfoText,
                        TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping);

                    y += mi.HeightPx;
                    continue;
                }

                if (it.Kind == ItemKind.Text)
                {
                    var rect = new Rectangle(x, y, innerMaxWidth - indent, mi.HeightPx);
                    TextRenderer.DrawText(
                        g,
                        it.Text,
                        bf,
                        rect,
                        SystemColors.InfoText,
                        TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping);

                    y += mi.HeightPx;
                    continue;
                }

                var availableKv = innerMaxWidth - indent;
                var keyW = Math.Min(layout.KeyColWidthPx, Math.Max(80, availableKv / 2));
                var valW = availableKv - keyW - KeyValueGapPx;

                if (valW < 80)
                {
                    keyW = Math.Min(keyW, availableKv - 80 - KeyValueGapPx);
                    valW = availableKv - keyW - KeyValueGapPx;
                    if (valW < 80)
                        valW = 80;
                }

                var keyRect = new Rectangle(x, y, keyW, mi.HeightPx);
                var valRect = new Rectangle(x + keyW + KeyValueGapPx, y, valW, mi.HeightPx);

                TextRenderer.DrawText(
                    g,
                    it.Text,
                    bdf,
                    keyRect,
                    SystemColors.InfoText,
                    TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping);

                var val = it.Value ?? string.Empty;

                TextRenderer.DrawText(
                    g,
                    val,
                    bf,
                    valRect,
                    SystemColors.InfoText,
                    TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoClipping);

                y += mi.HeightPx;
            }

            if (y < bottomLimit && layout.WasHeightClipped)
            {
                var ell = "…";
                var h = TextRenderer.MeasureText(g, ell, bf).Height;
                var rect = new Rectangle(x0, bottomLimit - h, innerMaxWidth, h);
                TextRenderer.DrawText(
                    g,
                    ell,
                    bf,
                    rect,
                    SystemColors.InfoText,
                    TextFormatFlags.NoPrefix | TextFormatFlags.NoClipping | TextFormatFlags.Right);
            }
        }

        private enum ItemKind { TitleHeader, Header, SubHeader, KeyValue, Text, Bullet, Blank }

        private readonly record struct Item(ItemKind Kind, int IndentTabs, string Text, string? Value);

        private readonly record struct MeasuredItem(Item Item, int WidthPx, int HeightPx);

        private sealed class Layout
        {
            public readonly List<MeasuredItem> Items;
            public readonly int KeyColWidthPx;
            public readonly int WidthPx;
            public readonly int HeightPx;
            public readonly bool WasHeightClipped;

            public Layout(List<MeasuredItem> items, int keyColWidthPx, int widthPx, int heightPx, bool wasHeightClipped)
            {
                Items = items;
                KeyColWidthPx = keyColWidthPx;
                WidthPx = widthPx;
                HeightPx = heightPx;
                WasHeightClipped = wasHeightClipped;
            }
        }
    }
}
