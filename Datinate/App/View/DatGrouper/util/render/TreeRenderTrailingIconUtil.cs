using com.RADIO.Datinate.RMVC.Shared;
using Datinate.App.View.projects.gameFamily;
using RadioLibCore.RadioDat;
using System.Drawing.Drawing2D;

namespace datinate.app
{
    internal static class TreeRenderTrailingIconUtil
    {
        private const int TRAILING_LEFT_COUNT_TEXT_GAP_PX = 0;
        private const int TRAILING_LEFT_COUNT_ICON_GAP_PX = 4;
        private const float TRAILING_LEFT_COUNT_FONT_SCALE = 1.08f;
        private const bool TRAILING_LEFT_COUNT_FONT_BOLD = true;
        private static Font? cachedTrailingLeftCountFont;
        private static string? cachedTrailingLeftCountFontSig;

        private const int TRAILING_RIGHT_ALIGNED_ICON_OVERLAP_PX = 8;
        private static int TRAILING_ICON_RIGHT_INSET_PX = 0;
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
                GetTrailingIconsLeftAligned(
                    node,
                    media,
                    focusNode);

            IReadOnlyList<Bitmap> trailingLeftAligned =
                trailingLeft.Icons;

            string? trailingLeftCountText =
                trailingLeft.CountText;

            IReadOnlyList<Bitmap> trailingRightAligned =
                GetTrailingIconsRightAligned(
                    node,
                    media);

            bool hasLeft =
                trailingLeftAligned.Count > 0;

            bool hasRight =
                trailingRightAligned.Count > 0;

            if (!hasLeft &&
                !hasRight)
            {
                return;
            }


            var fullRow =
                new Rectangle(
                    0,
                    rowBounds.Top,
                    rowWidth,
                    rowBounds.Height);

            var state =
                g.Save();

            try
            {
                g.SetClip(
                    fullRow);


                /*
                 * The right-aligned descriptor lane deliberately uses a stable
                 * width which ignores the extra width added by the scrollbar
                 * manager.
                 *
                 * This prevents descriptors jumping when the scrollbar is
                 * exposed/clipped.
                 */
                int stableRowWidth =
                    GetTrailingIconsRowWidth(
                        tv,
                        rowWidth);

                if (stableRowWidth < 0)
                    stableRowWidth = 0;

                int stableRightEdge =
                    stableRowWidth -
                    TRAILING_ICON_RIGHT_INSET_PX;

                if (stableRightEdge < 0)
                    stableRightEdge = 0;


                /*
                 * Left-trailing content is different.
                 *
                 * It has higher priority and may use ALL of the currently visible
                 * client area, including the space reclaimed when the native
                 * scrollbar has been moved outside the parent.
                 */
                int visibleRightEdge =
                    GetVisibleTrailingContentRightEdge(
                        tv,
                        rowWidth);

                if (visibleRightEdge < 0)
                    visibleRightEdge = 0;


                /*
                 * Priority:
                 *
                 * 1. Normal node text/chips/aliases.
                 * 2. Clapper/count/speech-bubble trailing content.
                 * 3. Right-aligned descriptor icons.
                 */
                int contentAndLeftRightMost =
                    contentRightMostDrawn;


                if (hasLeft)
                {
                    int leftRightMost =
                        DrawTrailingIconsLeftAligned(
                            g,
                            tv,
                            rowBounds,
                            rowWidth,
                            contentRightMostDrawn,
                            visibleRightEdge,
                            trailingLeftAligned,
                            trailingLeftCountText);

                    if (leftRightMost >
                        contentAndLeftRightMost)
                    {
                        contentAndLeftRightMost =
                            leftRightMost;
                    }
                }


                if (hasRight)
                {
                    /*
                     * DrawTrailingIcon(...) still uses GetTrailingIconsRowWidth(),
                     * so its preferred right-aligned position remains stable.
                     *
                     * Passing contentAndLeftRightMost means it can never move left
                     * over higher-priority node/clapper/count content.
                     */
                    DrawTrailingIcon(
                        g,
                        tv,
                        rowBounds,
                        rowWidth,
                        contentAndLeftRightMost,
                        trailingRightAligned);
                }
            }
            finally
            {
                g.Restore(
                    state);
            }
        }
        private static int GetVisibleTrailingContentRightEdge(
    TreeView tv,
    int rowWidth)
        {
            if (rowWidth <= 0)
                return 0;


            /*
             * Preview/alternate rendering surfaces should simply use their
             * supplied width.
             */
            if (rowWidth != tv.ClientSize.Width)
                return rowWidth;


            var parent =
                tv.Parent;

            if (parent == null)
                return rowWidth;


            /*
             * The TreeView can extend beyond its parent because the scrollbar
             * manager deliberately pushes the native scrollbar outside the
             * parent's clipping boundary.
             *
             * The parent's right edge expressed in TreeView coordinates is the
             * maximum actually visible content position.
             *
             * When the scrollbar is exposed:
             *
             *     rowWidth is normally smaller, because the scrollbar consumes
             *     client width.
             *
             * When the scrollbar is clipped outside:
             *
             *     rowWidth grows, and the parent's right edge becomes the limiting
             *     visible boundary.
             */
            int parentVisibleRightInTree =
                parent.ClientSize.Width -
                tv.Left;

            int visibleRightEdge =
                Math.Min(
                    rowWidth,
                    parentVisibleRightInTree);

            return Math.Max(
                0,
                visibleRightEdge);
        }
        private static void DrawTrailingIcon(
            Graphics g,
            TreeView tv,
            Rectangle rowBounds,
            int rowWidth,
            int contentRightMostDrawn,
            IReadOnlyList<Bitmap> mediaAssignmentIcons)
        {
            if (rowWidth <= 0 ||
                mediaAssignmentIcons.Count == 0)
            {
                return;
            }


            /*
             * This is the normal/stable right boundary.
             *
             * GetTrailingIconsRowWidth(...) deliberately compensates for the
             * scrollbar manager so this position does not jump when the native
             * scrollbar is exposed or clipped.
             */
            int effectiveRowWidth =
                GetTrailingIconsRowWidth(
                    tv,
                    rowWidth);

            if (effectiveRowWidth <= 0)
                return;

            int preferredRightEdge =
                effectiveRowWidth -
                TRAILING_ICON_RIGHT_INSET_PX;

            if (preferredRightEdge <= 0)
                return;


            int rowH =
                rowBounds.Height > 0
                    ? rowBounds.Height
                    : (tv.ItemHeight > 0
                        ? tv.ItemHeight
                        : 1);

            int totalW =
                GetTrailingIconsTotalWidth(
                    rowH,
                    mediaAssignmentIcons);

            if (totalW <= 0)
                return;


            /*
             * Normal position: right aligned against the stable scrollbar-aware
             * boundary.
             */
            int preferredX =
                preferredRightEdge -
                totalW;


            /*
             * Everything already rendered to the left has priority.
             */
            int minimumX =
                contentRightMostDrawn +
                TRAILING_ICON_GAP_PX;


            bool horizontallyCrunched =
                preferredX < minimumX;


            /*
             * Plenty of space:
             *
             *     TEXT                  [NG][Co][BL]
             *                           < stable right offset >
             *
             * Crunched:
             *
             *     TEXT [NG][Co][BL...
             *
             * In the crunched case the group is parked directly after the
             * higher-priority content instead of being allowed to overlap it.
             */
            int x =
                horizontallyCrunched
                    ? minimumX
                    : preferredX;

            if (x < 0)
                x = 0;


            /*
             * Normally we continue to obey the stable preferred right edge.
             *
             * When crunched, however, use every actually visible pixel to the
             * right. This allows descriptor icons to extend into the normally
             * reserved scrollbar-width area, with partial icons visible where
             * necessary.
             *
             * Importantly, this changes only clipping -- not the starting X --
             * so scrollbar appearance/disappearance cannot make the icons jump.
             */
            int clipRightEdge =
                horizontallyCrunched
                    ? GetVisibleTrailingContentRightEdge(
                        tv,
                        rowWidth)
                    : preferredRightEdge;

            if (clipRightEdge <= x)
                return;


            var clipState =
                g.Save();

            try
            {
                var descriptorClip =
                    new Rectangle(
                        0,
                        rowBounds.Top,
                        clipRightEdge,
                        rowH);

                g.SetClip(
                    descriptorClip,
                    CombineMode.Intersect);


                int overlap =
                    TRAILING_RIGHT_ALIGNED_ICON_OVERLAP_PX;

                if (overlap < 0)
                    overlap = 0;


                bool interpolationUpgraded =
                    false;

                var oldInterpolation =
                    g.InterpolationMode;

                try
                {
                    for (int i = 0;
                         i < mediaAssignmentIcons.Count;
                         i++)
                    {
                        var bmp =
                            mediaAssignmentIcons[i];

                        if (bmp is null)
                            continue;

                        int srcW =
                            bmp.Width;

                        int srcH =
                            bmp.Height;

                        if (srcW <= 0 ||
                            srcH <= 0)
                        {
                            continue;
                        }


                        int dstW =
                            srcW;

                        int dstH =
                            srcH;

                        if (dstH > rowH)
                        {
                            if (!interpolationUpgraded)
                            {
                                g.InterpolationMode =
                                    InterpolationMode.HighQualityBicubic;

                                interpolationUpgraded =
                                    true;
                            }

                            float s =
                                rowH /
                                (float)dstH;

                            dstH =
                                rowH;

                            dstW =
                                (int)Math.Round(
                                    dstW * s,
                                    MidpointRounding.AwayFromZero);

                            if (dstW < 1)
                                dstW = 1;
                        }


                        /*
                         * Nothing further can be visible once an icon's left edge
                         * is beyond the clipping boundary.
                         */
                        if (x >= clipRightEdge)
                            break;


                        int y =
                            rowBounds.Top +
                            ((rowH - dstH) / 2);


                        /*
                         * Always draw the complete bitmap.
                         *
                         * The clipping region handles partial visibility, so every
                         * right-hand descriptor can remain partially visible where
                         * pixels are still available.
                         */
                        g.DrawImage(
                            bmp,
                            x,
                            y,
                            dstW,
                            dstH);


                        int step =
                            dstW -
                            overlap;

                        if (step < 1)
                            step = 1;

                        x +=
                            step;
                    }
                }
                finally
                {
                    if (interpolationUpgraded)
                    {
                        g.InterpolationMode =
                            oldInterpolation;
                    }
                }
            }
            finally
            {
                g.Restore(
                    clipState);
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

        private static int GetTrailingIconsRowWidth(
            TreeView tv,
            int rowWidth)
        {
            if (rowWidth <= 0)
                return 0;

            /*
             * A rowWidth override is used when rendering a node preview.
             * That is a separate drawing surface, so don't apply the live
             * TreeView clipping compensation to it.
             */
            if (rowWidth != tv.ClientSize.Width)
                return rowWidth;

            var parent =
                tv.Parent;

            if (parent == null)
                return rowWidth;

            /*
             * When the scrollbar is hidden, DatGrouperTreeViewScrollManager
             * deliberately widens the TreeView beyond its parent's right edge.
             *
             * rowWidth grows by the same amount.
             *
             * Remove ONLY that extra width here so trailing content retains the
             * same logical right edge regardless of scrollbar visibility.
             */
            int hiddenOverflow =
                Math.Max(
                    0,
                    tv.Right - parent.ClientSize.Width);

            int stableRowWidth =
                rowWidth - hiddenOverflow;

            return Math.Max(
                0,
                stableRowWidth);
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
        private static int DrawTrailingIconsLeftAligned(
            Graphics g,
            TreeView tv,
            Rectangle rowBounds,
            int rowWidth,
            int contentRightMostDrawn,
            int maxRightEdgeExclusive,
            IReadOnlyList<Bitmap> icons,
            string? countText)
        {
            if (rowWidth <= 0 ||
                icons.Count == 0)
            {
                return contentRightMostDrawn;
            }

            if (maxRightEdgeExclusive <= 0)
                return contentRightMostDrawn;


            int rowH =
                rowBounds.Height > 0
                    ? rowBounds.Height
                    : (tv.ItemHeight > 0
                        ? tv.ItemHeight
                        : 1);


            int x =
                contentRightMostDrawn +
                TRAILING_ICON_GAP_PX;

            if (x < 0)
                x = 0;


            // No visible room at all for the left-trailing content.
            if (x >= maxRightEdgeExclusive)
                return maxRightEdgeExclusive;


            /*
             * Left-trailing content has priority over the right-aligned
             * descriptor icons.
             *
             * Allow the final left item to render partially rather than
             * disappearing completely as the viewport becomes narrow.
             *
             * Anything beyond maxRightEdgeExclusive is simply clipped.
             */
            var clipState =
                g.Save();

            try
            {
                var leftTrailingClip =
                    new Rectangle(
                        0,
                        rowBounds.Top,
                        maxRightEdgeExclusive,
                        rowH);

                g.SetClip(
                    leftTrailingClip,
                    CombineMode.Intersect);


                bool interpolationUpgraded =
                    false;

                var oldInterpolation =
                    g.InterpolationMode;

                try
                {
                    for (int i = 0;
                         i < icons.Count;
                         i++)
                    {
                        var bmp =
                            icons[i];

                        if (bmp is null)
                            continue;


                        int srcW =
                            bmp.Width;

                        int srcH =
                            bmp.Height;

                        if (srcW <= 0 ||
                            srcH <= 0)
                        {
                            continue;
                        }


                        int dstW =
                            srcW;

                        int dstH =
                            srcH;


                        if (dstH > rowH)
                        {
                            if (!interpolationUpgraded)
                            {
                                g.InterpolationMode =
                                    InterpolationMode.HighQualityBicubic;

                                interpolationUpgraded =
                                    true;
                            }

                            float s =
                                rowH /
                                (float)dstH;

                            dstH =
                                rowH;

                            dstW =
                                (int)Math.Round(
                                    dstW * s,
                                    MidpointRounding.AwayFromZero);

                            if (dstW < 1)
                                dstW = 1;
                        }


                        // Nothing at all remains visible.
                        if (x >= maxRightEdgeExclusive)
                            return maxRightEdgeExclusive;


                        int y =
                            rowBounds.Top +
                            ((rowH - dstH) / 2);


                        /*
                         * Draw the complete image.
                         *
                         * The Graphics clip above ensures that if only part of
                         * the image still fits, that visible part is retained.
                         */
                        g.DrawImage(
                            bmp,
                            x,
                            y,
                            dstW,
                            dstH);


                        bool iconWasClipped =
                            x + dstW >
                            maxRightEdgeExclusive;

                        x +=
                            dstW;


                        /*
                         * If the icon itself reached the edge, there cannot be
                         * any visible room for its associated count or later
                         * left-trailing items.
                         */
                        if (iconWasClipped)
                            return maxRightEdgeExclusive;


                        /*
                         * The count belongs to the first clapperboard icon.
                         */
                        if (i == 0 &&
                            !string.IsNullOrWhiteSpace(countText))
                        {
                            x +=
                                TRAILING_LEFT_COUNT_TEXT_GAP_PX;

                            if (x >= maxRightEdgeExclusive)
                                return maxRightEdgeExclusive;


                            var textFont =
                                GetTrailingLeftCountFont(
                                    tv.Font);

                            var textSize =
                                TextRenderer.MeasureText(
                                    g,
                                    countText,
                                    textFont,
                                    Size.Empty,
                                    CountTextFlags);


                            int textY =
                                rowBounds.Top +
                                ((rowH - textFont.Height) / 2);

                            if (textY < rowBounds.Top)
                                textY = rowBounds.Top;


                            /*
                             * PreserveGraphicsClipping matters here because
                             * TextRenderer uses GDI rather than normal GDI+ drawing.
                             *
                             * This lets the count text itself remain partially
                             * visible at the right edge.
                             */
                            TextRenderer.DrawText(
                                g,
                                countText,
                                textFont,
                                new Point(
                                    x,
                                    textY),
                                CountTextColor,
                                CountTextFlags |
                                TextFormatFlags.PreserveGraphicsClipping);


                            bool countWasClipped =
                                x + textSize.Width >
                                maxRightEdgeExclusive;

                            x +=
                                textSize.Width;


                            if (countWasClipped)
                                return maxRightEdgeExclusive;


                            x +=
                                TRAILING_LEFT_COUNT_ICON_GAP_PX;
                        }
                    }


                    return Math.Min(
                        x,
                        maxRightEdgeExclusive);
                }
                finally
                {
                    if (interpolationUpgraded)
                    {
                        g.InterpolationMode =
                            oldInterpolation;
                    }
                }
            }
            finally
            {
                g.Restore(
                    clipState);
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
