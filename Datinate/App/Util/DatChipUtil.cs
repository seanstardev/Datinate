using com.RADIO.Datinate.RMVC.Shared;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public static class DatChipUtil
    {
        private static bool initialised;

        private sealed class CacheEntry
        {
            public CacheEntry(ChipDraw normal, ChipDraw small, Bitmap legacyNormal, Bitmap legacySmall)
            {
                Normal = normal;
                Small = small;
                LegacyNormal = legacyNormal;
                LegacySmall = legacySmall;
            }

            public ChipDraw Normal { get; }
            public ChipDraw Small { get; }
            public Bitmap LegacyNormal { get; }
            public Bitmap LegacySmall { get; }
        }

        private readonly record struct ChipDraw(
            Bitmap Background,
            string LeftText,
            string RightText,
            bool HasRight,
            int LeftWidth,
            int DividerW,
            Size LeftSize,
            Size RightSize,
            Font Font,
            Color TextColor,
            int Radius,
            float Scale);

        private static readonly object Sync = new();

        // Single cache so hatch usage + chosen style is consistent regardless of which API you call.
        private static readonly Dictionary<string, CacheEntry> Dic = new(StringComparer.Ordinal);

        private const int ChipCornerRadius = 2;
        private const int ChipVerticalPadding = 2;

        private const float ChipOutlineDarkenFactor = 0.65f;

        private const int ChipSecondaryOpacityPercent = 100;
        private const int ChipSecondaryMuteToWhitePercent = 35;

        private const float SecondaryChipScale = 0.78f;

        private static readonly Color SmallChipTextColor = Color.SlateGray;
        private const int SegmentTextInsetPx = 1;

        private const int StrongOutlineThicknessPx = 1;
        private const int StrongOutlineWhitePercent = 10;



        private static readonly TextFormatFlags ChipTextFlagsCentered =
            ChipTextFlags |
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter;

        private static readonly TextFormatFlags ChipTextFlags =
            TextFormatFlags.NoPrefix |
            TextFormatFlags.NoPadding |
            TextFormatFlags.SingleLine;

        // Positive = nudge text RIGHT by N px. Negative = nudge LEFT by N px.
        private const int LeftSegmentCenterBiasPx = 1;//-2;
        // Empirically, the right segment can look 1px left with TextRenderer on short strings.
        // Keep it explicit so you can tune (0 or 1).
        private const int RightSegmentCenterBiasPx = 0;
        private static Rectangle GetSegmentTextRect(int x, int y, int w, int h, int insetPx, int centerBiasPx)
        {
            // We achieve an exact center shift of `centerBiasPx` by ensuring:
            // (leftInset - rightInset) / 2 == centerBiasPx  =>  leftInset - rightInset == 2 * centerBiasPx
            //
            // We do that safely by increasing only one side’s inset (never going negative),
            // which keeps the rect inside the segment.
            int extra = Math.Abs(centerBiasPx) * 2;

            int leftInset = insetPx + (centerBiasPx > 0 ? extra : 0);
            int rightInset = insetPx + (centerBiasPx < 0 ? extra : 0);

            int xx = x + leftInset;
            int ww = w - leftInset - rightInset;

            if (ww < 1)
                ww = 1;

            return new Rectangle(xx, y, ww, h);
        }

        private static readonly Color[] DatChipColours =
        {
            Color.FromArgb(255, 198, 229, 255),
            Color.FromArgb(255, 220, 255, 220),
            Color.FromArgb(255, 255, 240, 180),
            Color.FromArgb(255, 255, 215, 190),
            Color.FromArgb(255, 255, 230, 210),
            Color.FromArgb(255, 230, 206, 255),
            Color.FromArgb(255, 204, 242, 204),
            Color.FromArgb(255, 255, 204, 229),
            Color.FromArgb(255, 190, 245, 245),
            Color.FromArgb(255, 220, 230, 255),
            Color.FromArgb(255, 240, 220, 255),
            Color.FromArgb(255, 235, 235, 235)
        };

        private static readonly HatchStyle[] DatChipHatches =
        {
            HatchStyle.WideDownwardDiagonal,
            HatchStyle.WideUpwardDiagonal,
            HatchStyle.DarkHorizontal,
            HatchStyle.DarkVertical,
            HatchStyle.LargeGrid,
            HatchStyle.DiagonalCross,
            HatchStyle.DashedHorizontal,
            HatchStyle.DashedVertical,
            HatchStyle.DarkDownwardDiagonal,
            HatchStyle.DarkUpwardDiagonal,
            HatchStyle.DiagonalBrick,
            HatchStyle.LargeCheckerBoard
        };

        public static Control CreateChip(string key, bool strong = false) =>
            new DatChipUI() { DatKey = key, Strong = strong };
        public static Control CreateChipSmall(string key, bool strong = false) =>
            new DatChipUI() { DatKey = key, Small = true, Strong = strong };


        private static readonly int[] HatchUseCounts = new int[DatChipHatches.Length];

        // Font cache for scaled variants (so we don't allocate per chip).
        private static readonly Dictionary<string, Font> FontCache = new(StringComparer.Ordinal);

        public static Bitmap GetBitmap(string key) => GetEntry(key, strongText: false).LegacyNormal;
        public static Bitmap GetBitmapSmall(string key) => GetEntry(key, strongText: false).LegacySmall;

        public static int Draw(Graphics g, string key, int x, int y, bool strongBorder = false)
            => DrawInternal(g, GetEntry(key, strongText: strongBorder).Normal, x, y, strongBorder);

        public static int DrawSmall(Graphics g, string key, int x, int y, bool strongBorder = false)
            => DrawInternal(g, GetEntry(key, strongText: strongBorder).Small, x, y, strongBorder);

        private static int DrawInternal(Graphics g, ChipDraw chip, int x, int y, bool strongBorder)
        {
            g.DrawImageUnscaled(chip.Background, x, y);

            int h = chip.Background.Height;

            var leftRect = GetSegmentTextRect(
                x,
                y,
                chip.LeftWidth,
                h,
                SegmentTextInsetPx,
                LeftSegmentCenterBiasPx);

            DrawChipText(g, chip.LeftText, chip.Font, leftRect, chip.TextColor);

            if (chip.HasRight)
            {
                int rightStart = chip.LeftWidth + chip.DividerW;
                int rightAreaW = chip.Background.Width - rightStart;

                var rightRect = GetSegmentTextRect(
                    x + rightStart,
                    y,
                    rightAreaW,
                    h,
                    SegmentTextInsetPx,
                    RightSegmentCenterBiasPx);

                DrawChipText(g, chip.RightText, chip.Font, rightRect, chip.TextColor);
            }

            if (strongBorder)
                DrawStrongOutline(g, x, y, chip.Background.Width, chip.Background.Height, chip.Radius, chip.Scale);

            return chip.Background.Width;
        }

        private static void DrawStrongOutline(Graphics g, int x, int y, int w, int h, int radius, float scale)
        {
            int thickness = Math.Max(1, (int)Math.Round(StrongOutlineThicknessPx * scale));
            int white = Math.Max(0, Math.Min(100, StrongOutlineWhitePercent));
            int v = (255 * white) / 100;

            var c = Color.FromArgb(255, v, v, v);

            var r = new Rectangle(x, y, w - 1, h - 1);
            using var path = CreateRoundedRectPath(r, radius);

            using var pen = new Pen(c, thickness) { Alignment = PenAlignment.Inset };
            var old = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawPath(pen, path);
            g.SmoothingMode = old;
        }
        private static CacheEntry GetEntry(string key, bool strongText = false)
        {
            Initialise();

            var input = (key ?? string.Empty).Trim();
            if (input.Length == 0)
                input = DAT_GROUP_ENUM.NOT_SET.ToString();

            var (enumContender, rawLabel) = SplitEnumContenderAndLabel(input);

            var datEnum = ParseEnumOrDefault(enumContender, DAT_GROUP_ENUM.NOT_SET);
            var enumKey = datEnum.ToString().ToLowerInvariant();

            var friendly = DatinateHelper.GetDatFriendlyName(input) ?? string.Empty;
            string? rightLabel = friendly.Length == 0 ? null : friendly;

            var cacheKey =
                rightLabel == null
                    ? enumKey
                    : (enumKey + " - " + rightLabel);

            cacheKey = cacheKey.ToLowerInvariant();

            if (strongText)
                cacheKey += "|strong";

            lock (Sync)
            {
                if (Dic.TryGetValue(cacheKey, out var existing))
                    return existing;

                var (colour, hatch) = GetStyle(datEnum);

                if (rightLabel != null)
                    hatch = GetNextHatchCore();
                else
                    BumpHatchUsage(hatch);

                var normal = RenderChipBackgroundAndLayout(
                    datEnum, colour, hatch, rightLabel,
                    scale: 1f,
                    opacityPercent: 100,
                    textColor: Color.Black,
                    strongText: strongText);

                var smallColour = BlendToWhite(colour, ChipSecondaryMuteToWhitePercent);

                var small = RenderChipBackgroundAndLayout(
                    datEnum, smallColour, hatch, rightLabel,
                    scale: SecondaryChipScale,
                    opacityPercent: ChipSecondaryOpacityPercent,
                    textColor: SmallChipTextColor,
                    strongText: strongText);

                var legacyNormal = CreateLegacyBitmapWithText(normal);
                var legacySmall = CreateLegacyBitmapWithText(small);

                var entry = new CacheEntry(normal, small, legacyNormal, legacySmall);
                Dic[cacheKey] = entry;
                return entry;
            }
        }

        private static void Initialise()
        {
            if (initialised) return;

            lock (Sync)
            {
                if (initialised) return;
                initialised = true;

                foreach (var value in Enum.GetValues<DAT_GROUP_ENUM>())
                {
                    var (colour, hatch) = GetStyle(value);

                    var key = value.ToString().ToLowerInvariant();
                    if (Dic.ContainsKey(key))
                        continue;

                    BumpHatchUsage(hatch);

                    var normal = RenderChipBackgroundAndLayout(
                        value, colour, hatch, rightLabel: null,
                        scale: 1f,
                        opacityPercent: 100,
                        textColor: Color.Black);

                    var smallColour = BlendToWhite(colour, ChipSecondaryMuteToWhitePercent);

                    var small = RenderChipBackgroundAndLayout(
                        value, smallColour, hatch, rightLabel: null,
                        scale: SecondaryChipScale,
                        opacityPercent: ChipSecondaryOpacityPercent,
                        textColor: SmallChipTextColor);

                    var legacyNormal = CreateLegacyBitmapWithText(normal);
                    var legacySmall = CreateLegacyBitmapWithText(small);

                    Dic[key] = new CacheEntry(normal, small, legacyNormal, legacySmall);
                }
            }
        }

        private static Bitmap CreateLegacyBitmapWithText(ChipDraw chip)
        {
            var bmp = (Bitmap)chip.Background.Clone();

            using (var gg = Graphics.FromImage(bmp))
            {
                int h = bmp.Height;

                var leftRect = GetSegmentTextRect(
                    0,
                    0,
                    chip.LeftWidth,
                    h,
                    SegmentTextInsetPx,
                    LeftSegmentCenterBiasPx);

                DrawChipText(gg, chip.LeftText, chip.Font, leftRect, chip.TextColor);

                if (chip.HasRight)
                {
                    int rightStart = chip.LeftWidth + chip.DividerW;
                    int rightAreaW = bmp.Width - rightStart;

                    var rightRect = GetSegmentTextRect(
                        rightStart,
                        0,
                        rightAreaW,
                        h,
                        SegmentTextInsetPx,
                        RightSegmentCenterBiasPx);

                    DrawChipText(gg, chip.RightText, chip.Font, rightRect, chip.TextColor);
                }
            }

            return bmp;
        }

        private static ChipDraw RenderChipBackgroundAndLayout(
            DAT_GROUP_ENUM datEnum,
            Color colour,
            HatchStyle hatch,
            string? rightLabel,
            float scale,
            int opacityPercent,
            Color textColor,
            bool strongText = false)
        {
            var leftText = datEnum.ToString().Replace('_', ' ');

            var hasRight = !string.IsNullOrWhiteSpace(rightLabel);
            var rightText = hasRight ? rightLabel!.Trim() : string.Empty;

            var baseFont = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;
            var style = strongText ? FontStyle.Bold : FontStyle.Regular;
            var font = GetCachedScaledFont(baseFont, scale, style);

            int baseChipHeight = baseFont.Height + ChipVerticalPadding * 2;
            int chipHeight = Math.Max(1, (int)Math.Round(baseChipHeight * scale));
            int minSegmentW = Math.Max(1, chipHeight);

            Size leftSize;
            Size rightSize;

            using (var tmpBmp = new Bitmap(1, 1, PixelFormat.Format32bppPArgb))
            using (var g = Graphics.FromImage(tmpBmp))
            {
                leftSize = MeasureChipTextSize(g, leftText, font);
                rightSize = hasRight ? MeasureChipTextSize(g, rightText, font) : Size.Empty;
            }

            int leftWidth = Math.Max(minSegmentW, leftSize.Width + minSegmentW / 3);
            int rightWidth = hasRight ? Math.Max(minSegmentW, rightSize.Width + minSegmentW / 3) : 0;
            int dividerW = hasRight ? 1 : 0;

            int chipWidth = leftWidth + (hasRight ? (rightWidth + dividerW) : 0);
            chipWidth = Math.Max(chipWidth, chipHeight);

            int radius = Math.Max(1, (int)Math.Round(ChipCornerRadius * scale));

            var bmp = new Bitmap(chipWidth, chipHeight, PixelFormat.Format32bppPArgb);

            using (var gg = Graphics.FromImage(bmp))
            {
                gg.SmoothingMode = SmoothingMode.AntiAlias;
                gg.Clear(Color.Transparent);

                var outlineRect = new Rectangle(0, 0, chipWidth - 1, chipHeight - 1);

                using var path = CreateRoundedRectPath(outlineRect, radius);

                var fill = ApplyOpacity(Color.FromArgb(255, colour), opacityPercent);
                var outline = ApplyOpacity(DarkenColor(Color.FromArgb(255, colour)), Math.Max(35, opacityPercent));

                using (var pen = new Pen(outline) { Alignment = PenAlignment.Inset })
                {
                    if (!hasRight)
                    {
                        using (var fillBrush = new SolidBrush(fill))
                            gg.FillPath(fillBrush, path);

                        ApplyHatchPattern(gg, path, new Rectangle(0, 0, chipWidth, chipHeight), hatch);

                        gg.DrawPath(pen, path);
                    }
                    else
                    {
                        int dividerX = leftWidth - 1;

                        var leftRect = new Rectangle(1, 1, Math.Max(1, leftWidth - 2), Math.Max(1, chipHeight - 2));
                        var leftFillRect = leftRect;
                        leftFillRect.Width = Math.Max(1, leftFillRect.Width - 1);

                        using (var fillBrush = new SolidBrush(fill))
                            gg.FillPath(fillBrush, path);

                        ApplyHatchPattern(gg, path, leftFillRect, hatch);

                        gg.DrawLine(pen, dividerX, 1, dividerX, chipHeight - 2);
                        gg.DrawPath(pen, path);
                    }
                }
            }

            return new ChipDraw(
                Background: bmp,
                LeftText: leftText,
                RightText: rightText,
                HasRight: hasRight,
                LeftWidth: leftWidth,
                DividerW: dividerW,
                LeftSize: leftSize,
                RightSize: rightSize,
                Font: font,
                TextColor: textColor,
                Radius: radius,
                Scale: scale);
        }

        private static Font GetCachedScaledFont(Font baseFont, float scale, FontStyle style)
        {
            if (scale == 1f && style == FontStyle.Regular)
                return baseFont;

            var sig =
                baseFont.FontFamily.Name + "|" +
                baseFont.SizeInPoints.ToString("R") + "|" +
                baseFont.GdiCharSet + "|" +
                baseFont.GdiVerticalFont + "|" +
                scale.ToString("R") + "|" +
                ((int)style).ToString();

            lock (Sync)
            {
                if (FontCache.TryGetValue(sig, out var f))
                    return f;

                var created = new Font(
                    baseFont.FontFamily,
                    baseFont.SizeInPoints * scale,
                    style,
                    GraphicsUnit.Point,
                    baseFont.GdiCharSet,
                    baseFont.GdiVerticalFont);

                FontCache[sig] = created;
                return created;
            }
        }

        private static Size MeasureChipTextSize(Graphics g, string text, Font font)
        {
            if (string.IsNullOrEmpty(text))
                return Size.Empty;

            return TextRenderer.MeasureText(g, text, font, Size.Empty, ChipTextFlags);
        }

        private static void DrawChipText(Graphics g, string text, Font font, Rectangle rect, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;

            TextRenderer.DrawText(g, text, font, rect, color, ChipTextFlagsCentered);
        }

        private static void ApplyHatchPattern(
            Graphics gg,
            GraphicsPath clipPath,
            Rectangle fillArea,
            HatchStyle hatch)
        {
            var ink = Color.FromArgb(120, 255, 255, 255);
            var bg = Color.FromArgb(0, 255, 255, 255);

            (int dx, int dy)[] offsets =
            {
                (0, 0), (1, 0), (2, 0),
                (0, 1), (1, 1), (2, 1),
                (0, 2), (1, 2), (2, 2)
            };

            var state = gg.Save();
            try
            {
                gg.SetClip(clipPath, CombineMode.Replace);
                gg.SetClip(fillArea, CombineMode.Intersect);

                for (int i = 0; i < offsets.Length; i++)
                {
                    using var hb = new HatchBrush(hatch, ink, bg);

                    var pass = gg.Save();
                    try
                    {
                        gg.TranslateTransform(offsets[i].dx, offsets[i].dy);
                        gg.FillRectangle(hb, fillArea);
                    }
                    finally
                    {
                        gg.Restore(pass);
                    }
                }
            }
            finally
            {
                gg.Restore(state);
            }
        }

        private static Color DarkenColor(Color c)
        {
            float factor = ChipOutlineDarkenFactor;
            int r = Math.Max(0, Math.Min(255, (int)Math.Round(c.R * factor)));
            int g = Math.Max(0, Math.Min(255, (int)Math.Round(c.G * factor)));
            int b = Math.Max(0, Math.Min(255, (int)Math.Round(c.B * factor)));
            return Color.FromArgb(c.A, r, g, b);
        }

        private static Color ApplyOpacity(Color c, int opacityPercent)
        {
            if (opacityPercent >= 100) return c;
            if (opacityPercent <= 0) return Color.FromArgb(0, c);
            int a = (c.A * opacityPercent) / 100;
            return Color.FromArgb(a, c);
        }

        private static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

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

        private static HatchStyle GetNextHatchCore()
        {
            int bestIdx = 0;
            int bestCount = int.MaxValue;

            for (int i = 0; i < HatchUseCounts.Length; i++)
            {
                int c = HatchUseCounts[i];
                if (c < bestCount)
                {
                    bestCount = c;
                    bestIdx = i;
                }
            }

            HatchUseCounts[bestIdx]++;
            return DatChipHatches[bestIdx];
        }

        private static void BumpHatchUsage(HatchStyle hatch)
        {
            for (int i = 0; i < DatChipHatches.Length; i++)
            {
                if (DatChipHatches[i] == hatch)
                {
                    HatchUseCounts[i]++;
                    return;
                }
            }
        }

        private static (string enumContender, string? label) SplitEnumContenderAndLabel(string s)
        {
            s = s.Trim();

            var dash = s.IndexOf(" - ", StringComparison.Ordinal);
            string? label = null;
            if (dash >= 0)
            {
                label = s.Substring(dash + 3).Trim();
                s = s.Substring(0, dash).TrimEnd();
            }

            var firstColon = s.IndexOf(':');
            var start = firstColon >= 0 ? firstColon + 1 : 0;
            while (start < s.Length && s[start] == ' ')
                start++;

            int end = start;

            int nextColon = s.IndexOf(':', start);
            if (nextColon >= 0)
                end = nextColon;

            int space = s.IndexOf(' ', start);
            if (space >= 0 && (end == start || space < end))
                end = space;

            if (end == start)
                end = s.Length;

            var contender = s.Substring(start, end - start).Trim();
            return (contender, string.IsNullOrWhiteSpace(label) ? null : label);
        }

        private static TEnum ParseEnumOrDefault<TEnum>(string enumContender, TEnum defaultValue)
            where TEnum : struct, Enum
        {
            return Enum.TryParse(enumContender, ignoreCase: true, out TEnum value) ? value : defaultValue;
        }
        private static (Color colour, HatchStyle hatch) GetStyle(DAT_GROUP_ENUM datGroupEnum)
        {
            Color colour;
            HatchStyle hatch;

            switch (datGroupEnum)
            {
                case DAT_GROUP_ENUM.MAME:
                case DAT_GROUP_ENUM.MAME_SL:
                case DAT_GROUP_ENUM.MAME_MEDIA:
                    colour = DatChipColours[0];
                    hatch = DatChipHatches[0];
                    break;

                case DAT_GROUP_ENUM.NO_INTRO:
                    colour = DatChipColours[1];
                    hatch = DatChipHatches[1];
                    break;

                case DAT_GROUP_ENUM.REDUMP:
                    colour = DatChipColours[2];
                    hatch = DatChipHatches[2];
                    break;

                case DAT_GROUP_ENUM.TOSEC:
                case DAT_GROUP_ENUM.TOSEC_ISO:
                case DAT_GROUP_ENUM.TOSEC_PIX:
                    colour = DatChipColours[3];
                    hatch = DatChipHatches[3];
                    break;

                case DAT_GROUP_ENUM.T_EN:
                    colour = DatChipColours[4];
                    hatch = DatChipHatches[4];
                    break;

                //case DAT_GROUP_ENUM.R2DAT_MAME:
                case DAT_GROUP_ENUM.R2DAT_WEB:
                    colour = DatChipColours[5];
                    hatch = DatChipHatches[5];
                    break;

                case DAT_GROUP_ENUM.R2DAT_REPLACEMENT_DOCS:
                case DAT_GROUP_ENUM.R2DAT_EMUMOVIES:
                    colour = DatChipColours[6];
                    hatch = DatChipHatches[6];
                    break;

                case DAT_GROUP_ENUM.TDC:
                    colour = DatChipColours[7];
                    hatch = DatChipHatches[7];
                    break;

                case DAT_GROUP_ENUM.VGMARCHIVE:
                    colour = DatChipColours[8];
                    hatch = DatChipHatches[8];
                    break;

                default:
                    colour = DatChipColours[9];
                    hatch = DatChipHatches[9];
                    break;
            }

            return (colour, hatch);
        }

        public static Size Measure(string key, bool small, bool strongText = false)
        {
            var entry = GetEntry(key, strongText);
            return small ? entry.Small.Background.Size : entry.Normal.Background.Size;
        }
        private static Color BlendToWhite(Color c, int whitePercent)
        {
            if (whitePercent <= 0) return c;
            if (whitePercent >= 100) return Color.FromArgb(c.A, 255, 255, 255);

            int wp = whitePercent;
            int cp = 100 - wp;

            int r = (c.R * cp + 255 * wp + 50) / 100;
            int g = (c.G * cp + 255 * wp + 50) / 100;
            int b = (c.B * cp + 255 * wp + 50) / 100;

            if (r < 0) r = 0; else if (r > 255) r = 255;
            if (g < 0) g = 0; else if (g > 255) g = 255;
            if (b < 0) b = 0; else if (b > 255) b = 255;

            return Color.FromArgb(c.A, r, g, b);
        }
    }
}
