using com.RADIO.Datinate.RMVC.Shared;
using Datinate.App.View.projects.gameFamily;
using RadioLibCore.RadioDat;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    internal static class TreeRenderTrailingIconUtil
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private const int GWL_STYLE = -16;
        private const int WS_VSCROLL = 0x00200000;

        private const int TRAILING_LEFT_COUNT_TEXT_GAP_PX = 0;
        private const int TRAILING_LEFT_COUNT_ICON_GAP_PX = 4;
        private const float TRAILING_LEFT_COUNT_FONT_SCALE = 1.08f;
        private const bool TRAILING_LEFT_COUNT_FONT_BOLD = true;
        private static Font? cachedTrailingLeftCountFont;
        private static string? cachedTrailingLeftCountFontSig;

        private const int TRAILING_RIGHT_ALIGNED_ICON_OVERLAP_PX = 8;
        private const int TRAILING_ICON_RIGHT_INSET_PX = 0;
        private const bool TRAILING_ICONS_ASSUME_VSCROLL_PRESENT = true;
        private const int TRAILING_ICON_GAP_PX = 6;

        private static readonly Color CountTextColor = Color.SlateGray;

        private static readonly TextFormatFlags CountTextFlags =
            TextFormatFlags.NoPrefix |
            TextFormatFlags.NoPadding;


        public static void Draw(
            Graphics g,
            TreeView tv,
            TreeNode node,
            Rectangle rowBounds,
            int rowWidth,
            int contentRightMostDrawn,
            IMediaCollection? media,
            TreeNode? focusNode)
        {
            var trailingLeft =
                GetTrailingIconsLeftAligned(node, media, focusNode);

            IReadOnlyList<Bitmap> trailingLeftAligned =
                trailingLeft.Icons;

            string? trailingLeftCountText =
                trailingLeft.CountText;

            IReadOnlyList<Bitmap> trailingRightAligned =
                GetTrailingIconsRightAligned(node, media);

            bool hasLeft =
                trailingLeftAligned.Count > 0;

            bool hasRight =
                trailingRightAligned.Count > 0;

            if (!hasLeft && !hasRight)
                return;

            var fullRow = new Rectangle(
                0,
                rowBounds.Top,
                rowWidth,
                rowBounds.Height);

            var state = g.Save();

            try
            {
                g.SetClip(fullRow);

                int rowH =
                    rowBounds.Height > 0
                        ? rowBounds.Height
                        : (tv.ItemHeight > 0 ? tv.ItemHeight : 1);

                int effectiveRowWidth =
                    GetTrailingIconsRowWidth(tv, rowWidth);

                if (effectiveRowWidth < 0)
                    effectiveRowWidth = 0;

                int rightEdge =
                    effectiveRowWidth - TRAILING_ICON_RIGHT_INSET_PX;

                if (rightEdge < 0)
                    rightEdge = 0;

                int rightStartX = rightEdge;

                if (hasRight)
                {
                    int rightW =
                        GetTrailingIconsTotalWidth(
                            rowH,
                            trailingRightAligned);

                    rightStartX = rightEdge - rightW;

                    if (rightStartX < 0)
                        rightStartX = 0;
                }

                int leftMaxRightEdge =
                    hasRight
                        ? rightStartX - TRAILING_ICON_GAP_PX
                        : rightEdge;

                if (leftMaxRightEdge < 0)
                    leftMaxRightEdge = 0;

                if (hasLeft)
                {
                    DrawTrailingIconsLeftAligned(
                        g,
                        tv,
                        rowBounds,
                        rowWidth,
                        contentRightMostDrawn,
                        leftMaxRightEdge,
                        trailingLeftAligned,
                        trailingLeftCountText);
                }

                if (hasRight)
                {
                    DrawTrailingIcon(
                        g,
                        tv,
                        rowBounds,
                        rowWidth,
                        contentRightMostDrawn,
                        trailingRightAligned);
                }
            }
            finally
            {
                g.Restore(state);
            }
        }

        private static void DrawTrailingIcon(
            Graphics g,
            TreeView tv,
            Rectangle rowBounds,
            int rowWidth,
            int contentRightMostDrawn,
            IReadOnlyList<Bitmap> mediaAssignmentIcons)
        {
            if (rowWidth <= 0 || mediaAssignmentIcons.Count == 0)
                return;

            int effectiveRowWidth = GetTrailingIconsRowWidth(tv, rowWidth);
            if (effectiveRowWidth <= 0)
                return;

            int rightEdge = effectiveRowWidth - TRAILING_ICON_RIGHT_INSET_PX;
            if (rightEdge <= 0)
                return;

            int rowH = rowBounds.Height > 0 ? rowBounds.Height : (tv.ItemHeight > 0 ? tv.ItemHeight : 1);

            int totalW = GetTrailingIconsTotalWidth(rowH, mediaAssignmentIcons);
            if (totalW <= 0)
                return;

            int x = rightEdge - totalW;
            if (x < 0) x = 0;

            int overlap = TRAILING_RIGHT_ALIGNED_ICON_OVERLAP_PX;
            if (overlap < 0) overlap = 0;

            bool interpolationUpgraded = false;
            var oldInterpolation = g.InterpolationMode;

            try
            {
                for (int i = 0; i < mediaAssignmentIcons.Count; i++)
                {
                    var bmp = mediaAssignmentIcons[i];
                    if (bmp is null)
                        continue;

                    int srcW = bmp.Width;
                    int srcH = bmp.Height;
                    if (srcW <= 0 || srcH <= 0)
                        continue;

                    int dstW = srcW;
                    int dstH = srcH;

                    if (dstH > rowH)
                    {
                        if (!interpolationUpgraded)
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            interpolationUpgraded = true;
                        }

                        float s = rowH / (float)dstH;
                        dstH = rowH;
                        dstW = (int)Math.Round(dstW * s, MidpointRounding.AwayFromZero);
                        if (dstW < 1) dstW = 1;
                    }

                    if (x >= rightEdge)
                        break;

                    int y = rowBounds.Top + ((rowH - dstH) / 2);

                    g.DrawImage(bmp, x, y, dstW, dstH);

                    int step = dstW - overlap;
                    if (step < 1) step = 1;

                    x += step;
                }
            }
            finally
            {
                if (interpolationUpgraded)
                    g.InterpolationMode = oldInterpolation;
            }
        }

        private static int GetTrailingIconsTotalWidth(int rowH, IReadOnlyList<Bitmap> mediaAssignmentIcons)
        {
            int overlap = TRAILING_RIGHT_ALIGNED_ICON_OVERLAP_PX;
            if (overlap < 0) overlap = 0;

            bool havePrev = false;
            int prevW = 0;
            int total = 0;

            for (int i = 0; i < mediaAssignmentIcons.Count; i++)
            {
                var bmp = mediaAssignmentIcons[i];
                if (bmp is null)
                    continue;

                int srcW = bmp.Width;
                int srcH = bmp.Height;
                if (srcW <= 0 || srcH <= 0)
                    continue;

                int dstW = srcW;
                int dstH = srcH;

                if (dstH > rowH)
                {
                    float s = rowH / (float)dstH;
                    dstH = rowH;

                    dstW = (int)Math.Round(dstW * s, MidpointRounding.AwayFromZero);
                    if (dstW < 1) dstW = 1;
                }

                if (!havePrev)
                {
                    prevW = dstW;
                    havePrev = true;
                    continue;
                }

                int step = prevW - overlap;
                if (step < 1) step = 1;

                total += step;
                prevW = dstW;
            }

            if (!havePrev)
                return 0;

            total += prevW;

            return total;
        }

        private static bool IsVScrollVisible(TreeView tv)
        {
            if (!tv.IsHandleCreated)
                return false;

            int style = GetWindowLong(tv.Handle, GWL_STYLE);
            return (style & WS_VSCROLL) != 0;
        }

        private static int GetTrailingIconsRowWidth(TreeView tv, int rowWidth)
        {
            if (!TRAILING_ICONS_ASSUME_VSCROLL_PRESENT)
                return rowWidth;

            if (IsVScrollVisible(tv))
                return rowWidth;

            int w = rowWidth - SystemInformation.VerticalScrollBarWidth;
            return w < 0 ? 0 : w;
        }

        private static Font GetTrailingLeftCountFont(Font source)
        {
            var fam = source.FontFamily;

            var style = TRAILING_LEFT_COUNT_FONT_BOLD
                ? FontStyle.Bold
                : source.Style;

            var sig =
                fam.Name + "|" +
                source.SizeInPoints.ToString("R") + "|" +
                TRAILING_LEFT_COUNT_FONT_SCALE.ToString("R") + "|" +
                ((int)style).ToString() + "|" +
                source.GdiCharSet.ToString() + "|" +
                source.GdiVerticalFont.ToString();

            if (cachedTrailingLeftCountFont != null &&
                string.Equals(cachedTrailingLeftCountFontSig, sig, StringComparison.Ordinal))
            {
                return cachedTrailingLeftCountFont;
            }

            cachedTrailingLeftCountFont?.Dispose();
            cachedTrailingLeftCountFontSig = sig;

            cachedTrailingLeftCountFont = new Font(
                fam,
                source.SizeInPoints * TRAILING_LEFT_COUNT_FONT_SCALE,
                style,
                GraphicsUnit.Point,
                source.GdiCharSet,
                source.GdiVerticalFont);

            return cachedTrailingLeftCountFont;
        }
        private static void DrawTrailingIconsLeftAligned(
            Graphics g,
            TreeView tv,
            Rectangle rowBounds,
            int rowWidth,
            int contentRightMostDrawn,
            int maxRightEdgeExclusive,
            IReadOnlyList<Bitmap> icons,
            string? countText)
        {
            if (rowWidth <= 0 || icons.Count == 0)
                return;

            if (maxRightEdgeExclusive <= 0)
                return;

            int rowH = rowBounds.Height > 0 ? rowBounds.Height : (tv.ItemHeight > 0 ? tv.ItemHeight : 1);

            int x = contentRightMostDrawn + TRAILING_ICON_GAP_PX;
            if (x < 0) x = 0;

            if (x >= maxRightEdgeExclusive)
                return;

            bool interpolationUpgraded = false;
            var oldInterpolation = g.InterpolationMode;

            try
            {
                for (int i = 0; i < icons.Count; i++)
                {
                    var bmp = icons[i];
                    if (bmp is null)
                        continue;

                    int srcW = bmp.Width;
                    int srcH = bmp.Height;
                    if (srcW <= 0 || srcH <= 0)
                        continue;

                    int dstW = srcW;
                    int dstH = srcH;

                    if (dstH > rowH)
                    {
                        if (!interpolationUpgraded)
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            interpolationUpgraded = true;
                        }

                        float s = rowH / (float)dstH;
                        dstH = rowH;
                        dstW = (int)Math.Round(dstW * s, MidpointRounding.AwayFromZero);
                        if (dstW < 1) dstW = 1;
                    }

                    if (x + dstW > maxRightEdgeExclusive)
                        break;

                    int y = rowBounds.Top + ((rowH - dstH) / 2);

                    g.DrawImage(bmp, x, y, dstW, dstH);

                    x += dstW;

                    if (i == 0 && !string.IsNullOrWhiteSpace(countText))
                    {
                        x += TRAILING_LEFT_COUNT_TEXT_GAP_PX;

                        var textFont = GetTrailingLeftCountFont(tv.Font);

                        var textSize = TextRenderer.MeasureText(
                             g,
                             countText,
                             textFont,
                             Size.Empty,
                             CountTextFlags);

                        int textY = rowBounds.Top + ((rowH - textFont.Height) / 2);
                        if (textY < rowBounds.Top)
                            textY = rowBounds.Top;

                        if (x + textSize.Width <= maxRightEdgeExclusive)
                        {
                            TextRenderer.DrawText(
                                g,
                                countText,
                                textFont,
                                new Point(x, textY),
                                CountTextColor,
                                CountTextFlags);

                            x += textSize.Width;
                        }

                        x += TRAILING_LEFT_COUNT_ICON_GAP_PX;
                    }
                }
            }
            finally
            {
                if (interpolationUpgraded)
                    g.InterpolationMode = oldInterpolation;
            }
        }

        private static (IReadOnlyList<Bitmap> Icons, string? CountText) GetTrailingIconsLeftAligned(
            TreeNode? node,
            IMediaCollection? mc,
            TreeNode? focusNode)
        {
            if (node?.Tag is null || node.Tag is not IGameFamily)
                return ([], null);

            if (mc == null || mc.IsEmptyForExport)
                return ([], null);

            List<Bitmap> list = new List<Bitmap>();

            int assignedCount =
                mc.SourceIdAssignmentDictionary.Values.Count(m => m.AssignmentEnum == DatinateEnums.MEDIA_ASSIGNMENT_ENUM.Assigned);

            string? countText = assignedCount > 0 ? assignedCount.ToString() : null;

            if (mc.IsScoringExempt)
            {
                var bmp = node == focusNode
                    ? DatGrouperIconUtil.Instance.ClapperBoardCuratedUnavailableInvertedImage
                    : DatGrouperIconUtil.Instance.ClapperBoardCuratedUnavailableImage;

                list.Add(bmp);
            }
            else if (assignedCount > 0)
            {
                var bmp = node == focusNode
                    ? DatGrouperIconUtil.Instance.ClapperBoardCuratedInvertedImage
                    : DatGrouperIconUtil.Instance.ClapperBoardCuratedImage;

                list.Add(bmp);
            }

            if (!string.IsNullOrWhiteSpace(mc.FamilyNotes))
                list.Add(DatGrouperIconUtil.Instance.SpeechBubbleLeftIconImage);

            return (list, countText);
        }
        private static IReadOnlyList<Bitmap> GetTrailingIconsRightAligned(
            TreeNode node,
            IMediaCollection? mc)
        {
            if (mc == null || mc.IsEmptyForExport) return [];

            List<Bitmap> list = new List<Bitmap>();

            foreach (var desc in mc.CheckedDescriptorCodes)
                list.Add(DescriptorChipUtil.GetDescriptorBitmap(desc));

            return list;
        }
    }
}
