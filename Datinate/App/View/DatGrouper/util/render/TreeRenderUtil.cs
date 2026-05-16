using com.RADIO.Datinate.RMVC.Shared;
using Datinate.App.View.projects.gameFamily;
using RadioLibCore.RadioDat;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;
using static datinate.app.DatGrouperTreeView;

namespace datinate.app
{
    public static class TreeRenderUtil
    {
        private const int ALIAS_CONNECTOR_GRAY_20260218 = 160;


        private const bool FocusNodeDarkMode = true;
        private const int TRAILING_RIGHT_ALIGNED_ICON_OVERLAP_PX = 8; // gap between descriptor icons

        private const int TRAILING_ICON_RIGHT_INSET_PX = 0;
        private const bool TRAILING_ICONS_ASSUME_VSCROLL_PRESENT = true;

        private const bool FOCUS_NEIGHBOUR_SOLID_BLACK_BEFORE_ROW = true;

        private const int TRAILING_ICON_GAP_PX = 6;

        // Hover row styling (very subtle; designed for dense UI)
        private const bool HOVER_ROW_Enable = true;              // master toggle
        private const bool HOVER_ROW_SkipSelected = true;        // selection already has a strong cue
        private const bool HOVER_ROW_SkipDisabled = true;        // disabled should feel inert

        private const int HOVER_ROW_FillAlpha = 38;              // 0..255 (keep low)
        private const bool HOVER_ROW_DrawBorder = true;
        private const int HOVER_ROW_BorderAlpha = 55;            // 0..255
        private const int HOVER_ROW_BorderInsetPx = 0;

        private const bool HOVER_ROW_DrawLeftAccentBar = false;  // optional experiment
        private const int HOVER_ROW_LeftBarWidthPx = 3;
        private const int HOVER_ROW_LeftBarAlpha = 140;

        private const bool FOCUS_ROW_Enable = true;

        private const int DisabledRowFadeOpacityPercent = 70;

        private const float SpacerMessageFontScale = 1.5f;          

        private const int ChipGap = 8;

        private const int ChipLeadingGap = 6;
        private const int ChipToTextGap = 6;

        private const int VAlignEdgeInsetPx = 2;

        private const int AliasStartX = 300;
        private const int AliasMinGapAfterChips = 100;
        private const string AliasSeparator = "     ";

        private const string EmptySpacerMessage = "";

        private const bool DATINATE_CHIP_VTOP_VTopSecondaryChips = true;

        private const int NEIGHBOUR_FADE_PEAK_ALPHA = 80;

        private const int ExcludedRowFadeOpacityPercent = 40;
        private const int ExcludedHatchOpacityPercent = 35;

        private const HatchStyle ExcludedHatchStyle = HatchStyle.ForwardDiagonal;
        private static Bitmap GetChipBitmap(string label, bool small)
        {
            return small
                ? DatChipUtil.GetBitmapSmall(label)
                : DatChipUtil.GetBitmap(label);
        }
        private enum FocusOverlayKind
        {
            Before,
            Focus,
            After
        }
        private static readonly Color AlphaChipTextColor = Color.SlateGray;

        private static Font? cachedSpacerMessageFont;
        private static string? cachedSpacerMessageFontSig;
        
        private static readonly Color SpacerMessageColor = Color.FromArgb(255, 80, 80, 80);

        private static readonly ConditionalWeakTable<TreeNode, NodeRenderCache> RenderCache = [];
        private static readonly Color HOVER_ROW_FillRgb = Color.FromArgb(215, 228, 242);  // matches your selection family
        private static readonly Color HOVER_ROW_BorderRgb = Color.FromArgb(120, 145, 170); // neutral-ish
        private static readonly Color HOVER_ROW_LeftBarRgb = Color.FromArgb(0, 120, 215);  // “Windows accent”-ish

        private static readonly Color ExcludedNodeHatchColor = Color.FromArgb(255, 60, 60, 60);

        private static readonly TextFormatFlags NodeTextFlags =
            TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;

        private static Font? cachedAliasSeparatorFont;
        private static int cachedAliasSeparatorWidth;

        private enum FocusRelation
        {
            None,
            Focus,
            Above,
            Below
        }
        public enum NodeVAlign
        {
            Top,
            Center,
            Bottom
        }

        private static readonly TextFormatFlags SpacerMessageFlags =
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.NoPadding |
            TextFormatFlags.EndEllipsis;

        public static void DrawTreeNode(
            TreeView tv,
            TreeNode? node,
            Rectangle bounds,
            Graphics g,
            bool renderAliases,
            string? highlightText,
            Color matchBackColor,
            bool renderNodeAsExcluded,
            IReadOnlySet<Type> disabledTagTypes,
            IReadOnlySet<IGameEntity> enabledEntities,
            TreeNode? focusNode,
            IReadOnlySet<TreeNode> hiddenNodes,
            IMediaCollection? media,
            bool suppressSelectionHighlight,
            int? rowWidthOverride = null,
            int? contentLeftOverride = null)
        {
            if (node is null)
                return;

            if (DatinatePerformanceUtil.TREEVIEW_UseStockDraw_SkipDrawFocusNode)
            {
#pragma warning disable CS0162
                focusNode = null;
#pragma warning restore CS0162
            }
            var trailingLeft = GetTrailingIconsLeftAligned(node, media, focusNode);
            IReadOnlyList<Bitmap> trailingLeftAligned = trailingLeft.Icons;
            string? trailingLeftCountText = trailingLeft.CountText;

            IReadOnlyList<Bitmap> trailingRightAligned = GetTrailingIconsRightAligned(node, media);

            //var g = e.Graphics;
            var nodeBounds = node.Bounds;
            int rowWidth = rowWidthOverride ?? tv.ClientSize.Width;

            bounds = NormalizeRowBounds(tv.ItemHeight, bounds);
            
            int contentLeft = contentLeftOverride ?? bounds.Left;
            
            var contentBounds = new Rectangle(
                contentLeft,
                bounds.Top,
                Math.Max(1, bounds.Width - Math.Max(0, contentLeft - bounds.Left)),
                bounds.Height);

            if (hiddenNodes != null && hiddenNodes.Count > 0 && hiddenNodes.Contains(node))
            {
                var fullRow = new Rectangle(0, bounds.Top, rowWidth, bounds.Height);

                var state = g.Save();
                try
                {
                    g.SetClip(fullRow);
                    using var bg = new SolidBrush(tv.BackColor);
                    g.FillRectangle(bg, fullRow);
                }
                finally
                {
                    g.Restore(state);
                }

                return;
            }

            if (DrawSpacerRowIfNeeded(g, tv, node, bounds, focusNode))
                return;

            IReadOnlyList<string>? datChips = null;
            IReadOnlyList<(string LabelKey, string Text)>? aliasItems = null;

            bool prependLabelChips = false;
            NodeVAlign align = NodeVAlign.Center;

            bool alphaPrimaryChipSet = false;
            bool dimTrailingAngleSuffixForNodeText = false;

            if (node.Tag is IGameFamily family)
            {
                align = NodeVAlign.Center;

                var chips = GetCachedLabelChips(node, family);

                bool showSingleChipForFamily =
                    chips.Count == 1 && (!node.IsExpanded || node.Nodes.Count == 0);

                datChips =
                    chips.Count >= 2 || showSingleChipForFamily
                        ? chips
                        : null;

                prependLabelChips = false;
                alphaPrimaryChipSet = datChips != null;
                dimTrailingAngleSuffixForNodeText = false;
            }
            else if (node.Tag is IGame game)
            {
                align = NodeVAlign.Bottom;

                datChips = GetCachedLabelChips(node, game);

                prependLabelChips = true;
                dimTrailingAngleSuffixForNodeText = false;
            }
            else if (node.Tag is IGamePart part)
            {
                align = NodeVAlign.Top;
                if (!renderAliases)
                    aliasItems = [];
                else
                    aliasItems = GetCachedAliasItems(node, part);

                dimTrailingAngleSuffixForNodeText = true;
            }


            bool isDisabled = IsDisabled(node.Tag, disabledTagTypes, enabledEntities);

            int contentRightMostDrawn = contentBounds.Left;

            DrawGameEntityNode(
                g,
                tv,
                node,
                contentBounds,
                highlightText,
                matchBackColor,
                datChips,
                prependLabelChips,
                align,
                aliasItems,
                alphaPrimaryChipSet,
                dimTrailingAngleSuffixForNodeText,
                isDisabled,
                focusNode,
                nodeBounds,
                rowWidth,
                suppressSelectionHighlight,
                contentBounds,
                out contentRightMostDrawn);

            if (renderNodeAsExcluded)
            {
                var fullRow = new Rectangle(0, bounds.Top, rowWidth, bounds.Height);

                var state = g.Save();
                try
                {
                    g.SetClip(fullRow);
                    ApplyExcludedOverlay(g, tv, node, bounds, rowWidth, suppressSelectionHighlight);
                }
                finally
                {
                    g.Restore(state);
                }
            }

            if (focusNode != null && ReferenceEquals(node, focusNode))
            {
                var fullRow = new Rectangle(0, bounds.Top, rowWidth, bounds.Height);

                var state = g.Save();
                try
                {
                    g.SetClip(fullRow);
                    RedrawNodeGlyphs(g, tv, node, bounds, contentBounds, rowWidth, suppressSelectionHighlight);
                }
                finally
                {
                    g.Restore(state);
                }
            }

            bool hasLeft = trailingLeftAligned != null && trailingLeftAligned.Count > 0;
            bool hasRight = trailingRightAligned != null && trailingRightAligned.Count > 0;

            if (hasLeft || hasRight)
            {
                var fullRow = new Rectangle(0, bounds.Top, rowWidth, bounds.Height);

                var state = g.Save();
                try
                {
                    g.SetClip(fullRow);

                    int rowH = bounds.Height > 0 ? bounds.Height : (tv.ItemHeight > 0 ? tv.ItemHeight : 1);

                    int effectiveRowWidth = GetTrailingIconsRowWidth(tv, rowWidth);
                    if (effectiveRowWidth < 0) effectiveRowWidth = 0;

                    int rightEdge = effectiveRowWidth - TRAILING_ICON_RIGHT_INSET_PX;
                    if (rightEdge < 0) rightEdge = 0;

                    int rightStartX = rightEdge;
                    if (hasRight)
                    {
                        int rightW = GetTrailingIconsTotalWidth(rowH, trailingRightAligned!);
                        rightStartX = rightEdge - rightW;
                        if (rightStartX < 0) rightStartX = 0;
                    }

                    int leftMaxRightEdge = hasRight
                        ? (rightStartX - TRAILING_ICON_GAP_PX)
                        : rightEdge;

                    if (leftMaxRightEdge < 0) leftMaxRightEdge = 0;

                    if (hasLeft)
                        DrawTrailingIconsLeftAligned(g, tv, bounds, rowWidth, contentRightMostDrawn, leftMaxRightEdge, trailingLeftAligned!, trailingLeftCountText);

                    if (hasRight)
                        DrawTrailingIcon(g, tv, bounds, rowWidth, contentRightMostDrawn, trailingRightAligned!);
                }
                finally
                {
                    g.Restore(state);
                }
            }

            //e.DrawDefault = false;
        }

        private static void DrawGameEntityNode(
            Graphics g,
            TreeView tv,
            TreeNode node,
            Rectangle bounds,
            string? highlightText,
            Color matchBackColor,
            IReadOnlyList<string>? datChips,
            bool prependLabelChips,
            NodeVAlign align,
            IReadOnlyList<(string LabelKey, string Text)>? aliasItems,
            bool alphaPrimaryChipSet,
            bool dimTrailingAngleSuffixForNodeText,
            bool isDisabled,
            TreeNode? focusNode,
            Rectangle nodeBounds,
            int rowWidth,
            bool suppressSelectionHighlight,
            Rectangle contentBounds,
            out int contentRightMostDrawn)
        {
            var nodeFont = node.NodeFont ?? tv.Font;
            var selected = !suppressSelectionHighlight && ReferenceEquals(node, tv.SelectedNode);

            var fullRowRect = new Rectangle(0, bounds.Top, rowWidth, bounds.Height);

            contentRightMostDrawn = bounds.Left;

            var state = g.Save();
            try
            {
                g.SetClip(fullRowRect);

                var focusRel = GetFocusRelation(node, focusNode);
                bool focusDark = FocusNodeDarkMode && focusRel == FocusRelation.Focus;

                var alphaPrimaryChipSetEffective = alphaPrimaryChipSet;


                int y = GetAlignedTop(bounds, nodeFont.Height, align);

                var titleColor = node.ForeColor.IsEmpty ? tv.ForeColor : node.ForeColor;
                if (focusDark)
                    titleColor = Color.White;

                bool hasSearch = !string.IsNullOrWhiteSpace(highlightText);
                bool hasChips = datChips != null && datChips.Count > 0;
                bool hasAliases = aliasItems != null && aliasItems.Count > 0;

                int textStartX = bounds.Left;

                if (prependLabelChips && hasChips)
                {
                    int chipsWidth = MeasureLabelChipsWidth(
                        g,
                        nodeFont,
                        bounds,
                        datChips!,
                        alphaPrimaryChipSetEffective,
                        includeLeadingGap: false,
                        includeTrailingGap: false);

                    textStartX = bounds.Left + chipsWidth + ChipToTextGap;
                }

                var rawText = node.Text ?? string.Empty;
                var text = rawText.TrimEnd();

                int textWidth = TextRenderer.MeasureText(g, text, nodeFont, Size.Empty, NodeTextFlags).Width;
                int textEndX = textStartX + textWidth;

                int chipsEndX = textEndX;

                if (!prependLabelChips && hasChips)
                {
                    int chipsWidthPost = MeasureLabelChipsWidth(
                        g,
                        nodeFont,
                        bounds,
                        datChips!,
                        alphaPrimaryChipSetEffective,
                        includeLeadingGap: true,
                        includeTrailingGap: false);

                    chipsEndX = textEndX + chipsWidthPost;
                }

                int aliasStartX = bounds.Left + AliasStartX;
                if (aliasStartX < chipsEndX)
                    aliasStartX = chipsEndX + AliasMinGapAfterChips;

                int aliasesEndX = bounds.Left;

                if (hasAliases)
                    aliasesEndX = MeasureAliasItemsEndX(g, nodeFont, aliasStartX, aliasItems!);

                int rightMost = Math.Max(bounds.Left, Math.Max(chipsEndX, aliasesEndX));

                using (var bg = new SolidBrush(focusDark ? Color.Black : tv.BackColor))
                    g.FillRectangle(bg, fullRowRect);

                var selectedBackColor = Color.FromArgb(215, 228, 242);
                if (selected && focusRel != FocusRelation.Focus)
                {
                    int maxW = rowWidth - bounds.Left;
                    int backWidth = Math.Max(0, rightMost - bounds.Left + 2);
                    if (maxW > 0 && backWidth > maxW) backWidth = maxW;

                    var backRect = new Rectangle(bounds.Left, bounds.Top, backWidth, bounds.Height);

                    using (var sel = new SolidBrush(selectedBackColor))
                        g.FillRectangle(sel, backRect);
                }

                bool hot =
                    !suppressSelectionHighlight &&
                    IsHotRow(tv, node) &&
                    (!HOVER_ROW_SkipSelected || !selected) &&
                    (!HOVER_ROW_SkipDisabled || !IsTagDisabledOnTv(tv, node));

                if (hot && !selected && focusRel == FocusRelation.None)
                    ApplyHotRowOverlay(g, tv, bounds, rightMost, rowWidth);

                var rowBackColor = focusDark
                    ? Color.Black
                    : (selected ? selectedBackColor : tv.BackColor);

                if (prependLabelChips && hasChips)
                {
                    DrawLabelChipsInternal(
                        g,
                        nodeFont,
                        bounds,
                        bounds.Left,
                        datChips!,
                        align,
                        includeLeadingGap: false,
                        includeTrailingGap: false,
                        alphaPrimaryChipSet: alphaPrimaryChipSetEffective,
                        rowBackColor: rowBackColor);
                }

                int afterTextX = textStartX;

                afterTextX = DrawNodeTextWithOptionalDimTrailingAngleSuffix(
                    g,
                    bounds,
                    text,
                    nodeFont,
                    afterTextX,
                    y,
                    titleColor,
                    hasSearch,
                    highlightText,
                    matchBackColor,
                    dimTrailingAngleSuffixForNodeText);

                if (!prependLabelChips && hasChips)
                {
                    afterTextX = DrawLabelChipsInternal(
                        g,
                        nodeFont,
                        bounds,
                        afterTextX,
                        datChips!,
                        align,
                        includeLeadingGap: true,
                        includeTrailingGap: false,
                        alphaPrimaryChipSet: alphaPrimaryChipSetEffective,
                        rowBackColor: rowBackColor);
                }

                int drawnRightMost = Math.Max(bounds.Left, Math.Max(rightMost, afterTextX));

                if (hasAliases)
                {
                    int sepW = GetAliasSeparatorWidth(g, nodeFont);

                    int aliasFirstChipX = aliasStartX + sepW;

                    int connectorStartX = afterTextX + 6;
                    int connectorEndX = aliasFirstChipX - 6;
                    int connectorY = y + nodeFont.Height - 3;

                    DrawAliasConnector(g, bounds, connectorStartX, connectorEndX, connectorY, AlphaChipTextColor);

                    int aliasEndXDrawn = DrawAliasItems(
                        g,
                        bounds,
                        nodeFont,
                        nodeFont,
                        aliasStartX,
                        y,
                        hasSearch,
                        highlightText,
                        matchBackColor,
                        aliasItems!,
                        align,
                        rowBackColor);

                    drawnRightMost = Math.Max(drawnRightMost, aliasEndXDrawn);
                }

                bool isNeighbour = focusRel == FocusRelation.Above || focusRel == FocusRelation.Below;

                if (!isNeighbour)
                    RedrawNodeGlyphs(g, tv, node, bounds, contentBounds, rowWidth, suppressSelectionHighlight);

                if (isDisabled && !selected && focusRel != FocusRelation.Focus)
                {
                    int backWidth = Math.Max(0, rightMost + 2);
                    int maxW = rowWidth;
                    if (maxW > 0 && backWidth > maxW) backWidth = maxW;

                    if (backWidth > 0)
                    {
                        var fadeRect = new Rectangle(0, bounds.Top, backWidth, bounds.Height);

                        int fadeA = (Clamp0To100(DisabledRowFadeOpacityPercent) * 255 + 50) / 100;
                        if (fadeA > 140) fadeA = 140;

                        if (isNeighbour && fadeA > 70) fadeA = 70;

                        using var fade = new SolidBrush(Color.FromArgb(fadeA, rowBackColor));
                        g.FillRectangle(fade, fadeRect);
                    }
                }

                if (isNeighbour)
                    ApplyNeighbourSeparatorOverlay(g, tv, fullRowRect, focusRel);

                contentRightMostDrawn = drawnRightMost;
            }
            finally
            {
                g.Restore(state);
            }
        }

        private static bool DrawSpacerRowIfNeeded(Graphics g, TreeView tv, TreeNode node, Rectangle bounds, TreeNode? focusNode)
        {
            if (node?.Tag is null)
                return false;

            if (node.Tag is SpacerNodeTag)
            {
                bounds = NormalizeRowBounds(tv.ItemHeight, bounds);
                var fullRow = new Rectangle(0, bounds.Top, tv.ClientSize.Width, bounds.Height);

                var state = g.Save();
                try
                {
                    g.SetClip(fullRow);

                    var focusRel = GetFocusRelation(node, focusNode);
                    bool focusDark = FocusNodeDarkMode && focusRel == FocusRelation.Focus;

                    using (var bg = new SolidBrush(focusDark ? Color.Black : tv.BackColor))
                        g.FillRectangle(bg, fullRow);

                    if (!focusDark && IsFirstSpacerNodeInTree(tv, node))
                        DrawSpacerGradient(g, fullRow);

                    if (focusRel == FocusRelation.Above || focusRel == FocusRelation.Below)
                        ApplyNeighbourSeparatorOverlay(g, tv, fullRow, focusRel);
                }
                finally
                {
                    g.Restore(state);
                }

                return true;
            }

            return false;
        }

        public static void DrawOverlay(string title, string body, Graphics g, TreeView tv, Rectangle bounds)
        {
            bounds = NormalizeRowBounds(tv.ItemHeight, bounds);
            var fullRow = new Rectangle(0, bounds.Top, tv.ClientSize.Width, bounds.Height);

            var msgRect = fullRow;
            msgRect.Inflate(-6, 0);

            const int SpacerMessageLeftInsetPx = 30;
            msgRect.X += SpacerMessageLeftInsetPx;
            msgRect.Width = Math.Max(0, msgRect.Width - SpacerMessageLeftInsetPx);

            var msgFont = GetSpacerMessageFont(tv.Font);

            TextRenderer.DrawText(
                g,
                EmptySpacerMessage,
                msgFont,
                msgRect,
                SpacerMessageColor,
                SpacerMessageFlags);

            DragPromptOverlayRenderer.Draw(
                g,
                tv,
                title,
                body,
                offsetX: DatinateHelper.TeeViewHorizontalOffset);
        }
        private static void RedrawNodeGlyphs(
            Graphics g,
            TreeView tv, 
            TreeNode node, 
            Rectangle rowBounds, 
            Rectangle nodeBounds,
            int rowWidth,
            bool suppressSelectionHighlight
            )
        {
            var clipState = g.Save();
            try
            {
                var clipRect = new Rectangle(0, rowBounds.Top, rowWidth, rowBounds.Height);
                g.SetClip(clipRect);

                var textBounds = nodeBounds;

                var imgList = tv.ImageList;
                var stateList = tv.StateImageList;

                bool isSelected = ReferenceEquals(node, tv.SelectedNode) && !suppressSelectionHighlight;

                int imageIndex = node.ImageIndex;
                int selectedImageIndex = node.SelectedImageIndex;
                string imageKey = node.ImageKey ?? string.Empty;
                string selectedImageKey = node.SelectedImageKey ?? string.Empty;

                int stateIndex = node.StateImageIndex;

                Image? nodeImage = null;
                Image? stateImage = null;

                if (imgList != null)
                {
                    if (isSelected && !string.IsNullOrEmpty(selectedImageKey) && imgList.Images.ContainsKey(selectedImageKey))
                        nodeImage = imgList.Images[selectedImageKey];
                    else if (!string.IsNullOrEmpty(imageKey) && imgList.Images.ContainsKey(imageKey))
                        nodeImage = imgList.Images[imageKey];
                    else
                    {
                        int idx = isSelected ? selectedImageIndex : imageIndex;
                        if (idx < 0)
                            idx = isSelected ? tv.SelectedImageIndex : tv.ImageIndex;

                        if (idx >= 0 && idx < imgList.Images.Count)
                            nodeImage = imgList.Images[idx];
                    }
                }

                if (stateList != null && stateIndex >= 0 && stateIndex < stateList.Images.Count)
                    stateImage = stateList.Images[stateIndex];

                if (nodeImage == null && stateImage == null)
                    return;

                int imageW = nodeImage?.Width ?? 0;
                int imageH = nodeImage?.Height ?? 0;

                int stateW = stateImage?.Width ?? 0;
                int stateH = stateImage?.Height ?? 0;

                int h = Math.Max(imageH, stateH);
                if (h <= 0)
                    return;

                int rowH = rowBounds.Height > 0 ? rowBounds.Height : (tv.ItemHeight > 0 ? tv.ItemHeight : 1);

                int y = rowBounds.Top + ((rowH - h) / 2);
                int yMin = rowBounds.Top;
                int yMax = rowBounds.Bottom - h;
                if (y < yMin) y = yMin;
                if (y > yMax) y = yMax;

                int xImage = textBounds.Left;
                if (nodeImage != null)
                    xImage -= (imageW + 3);

                int xState = xImage;
                if (stateImage != null)
                    xState -= (stateW + 3);

                if (stateImage != null)
                    g.DrawImage(stateImage, xState, y, stateW, stateH);

                if (nodeImage != null)
                    g.DrawImage(nodeImage, xImage, y, imageW, imageH);
            }
            finally
            {
                g.Restore(clipState);
            }
        }

        private static Rectangle NormalizeRowBounds(int itemHeight, Rectangle bounds)
        {
            int itemH = itemHeight;
            if (itemH <= 0)
                itemH = bounds.Height;
            if (itemH <= 0)
                itemH = 1;

            return new Rectangle(bounds.Left, bounds.Top, bounds.Width, itemH);
        }

        private static int DrawAliasItems(
            Graphics g,
            Rectangle bounds,
            Font nodeFont,
            Font chipFont,
            int aliasStartX,
            int y,
            bool hasSearch,
            string? highlightText,
            Color matchBackColor,
            IReadOnlyList<(string LabelKey, string Text)> aliasItems,
            NodeVAlign align,
            Color rowBackColor)
        {
            int xx = aliasStartX;
            int sepW = GetAliasSeparatorWidth(g, nodeFont);

            var aliasTextColor = AlphaChipTextColor;

            for (int i = 0; i < aliasItems.Count; i++)
            {
                TextRenderer.DrawText(g, AliasSeparator, nodeFont, new Point(xx, y), aliasTextColor, NodeTextFlags);
                xx += sepW;

                xx = DrawLabelChipsInternal(
                    g,
                    chipFont,
                    bounds,
                    xx,
                    new[] { aliasItems[i].LabelKey },
                    align,
                    includeLeadingGap: false,
                    includeTrailingGap: false,
                    alphaPrimaryChipSet: false,
                    rowBackColor: rowBackColor,
                    forceSecondaryStyle: true,
                    textColorOverride: null);

                xx += ChipToTextGap;

                if (hasSearch)
                    xx = DrawTextWithOptionalHighlight(g, bounds, aliasItems[i].Text, nodeFont, xx, y, aliasTextColor, highlightText!, matchBackColor);
                else
                {
                    TextRenderer.DrawText(g, aliasItems[i].Text, nodeFont, new Point(xx, y), aliasTextColor, NodeTextFlags);
                    xx += MeasureTextWidth(g, aliasItems[i].Text, nodeFont);
                }
            }

            return xx;
        }

        private static void DrawAliasConnector(
            Graphics g,
            Rectangle rowBounds,
            int startX,
            int endX,
            int centerY,
            Color baseColor)
        {
            const int HeadW = 8;
            const int HeadH = 6;
            const int MinLen = HeadW + 10;

            if (endX - startX < MinLen)
                return;

            int y = Math.Max(rowBounds.Top + 2, Math.Min(rowBounds.Bottom - 2, centerY));

            int baseX = endX - HeadW;

            int maxHeadH = y - (rowBounds.Top + 2);
            int headH = Math.Min(HeadH, maxHeadH);

            if (headH < 3)
                return;

            int topY = y - headH;

            int gray = ALIAS_CONNECTOR_GRAY_20260218;
            if (gray < 0) gray = 0;
            if (gray > 255) gray = 255;

            var connectorRgb = Color.FromArgb(gray, gray, gray);

            var lineColor = Color.FromArgb(150, connectorRgb);
            var fillColor = Color.FromArgb(190, connectorRgb);

            var old = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var pen = new Pen(lineColor, 1f))
            using (var brush = new SolidBrush(fillColor))
            {
                g.DrawLine(pen, startX, y, baseX, y);

                var pts = new[]
                {
                    new Point(baseX, y),
                    new Point(baseX, topY),
                    new Point(endX, y),
                };

                g.FillPolygon(brush, pts);
            }

            g.SmoothingMode = old;
        }

        private static int DrawTextWithOptionalHighlight(
            Graphics g,
            Rectangle rowBounds,
            string text,
            Font font,
            int x,
            int y,
            Color textColor,
            string highlightText,
            Color matchBackColor)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(highlightText))
            {
                TextRenderer.DrawText(g, text ?? string.Empty, font, new Point(x, y), textColor, NodeTextFlags);
                return x + TextRenderer.MeasureText(g, text ?? string.Empty, font, Size.Empty, NodeTextFlags).Width;
            }

            int matchIndex = text.IndexOf(highlightText, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
            {
                TextRenderer.DrawText(g, text, font, new Point(x, y), textColor, NodeTextFlags);
                return x + TextRenderer.MeasureText(g, text, font, Size.Empty, NodeTextFlags).Width;
            }

            int matchLen = highlightText.Length;

            string prefix = text[..matchIndex];
            string match = text.Substring(matchIndex, matchLen);
            string suffix = text[(matchIndex + matchLen)..];

            int xx = x;

            if (prefix.Length > 0)
            {
                TextRenderer.DrawText(g, prefix, font, new Point(xx, y), textColor, NodeTextFlags);
                xx += TextRenderer.MeasureText(g, prefix, font, Size.Empty, NodeTextFlags).Width;
            }

            if (match.Length > 0)
            {
                var matchSize = TextRenderer.MeasureText(g, match, font, Size.Empty, NodeTextFlags);

                var highlightRect = new Rectangle(xx, rowBounds.Top, matchSize.Width, rowBounds.Height);
                using (var hb = new SolidBrush(matchBackColor))
                    g.FillRectangle(hb, highlightRect);

                TextRenderer.DrawText(g, match, font, new Point(xx, y), textColor, NodeTextFlags);
                xx += matchSize.Width;
            }

            if (suffix.Length > 0)
            {
                TextRenderer.DrawText(g, suffix, font, new Point(xx, y), textColor, NodeTextFlags);
                xx += TextRenderer.MeasureText(g, suffix, font, Size.Empty, NodeTextFlags).Width;
            }

            return xx;
        }

        private static NodeRenderCache GetCache(TreeNode node, object? tag)
        {
            var c = RenderCache.GetOrCreateValue(node);

            if (!ReferenceEquals(c.TagRef, tag))
            {
                c.TagRef = tag;

                c.Chips = null;
                c.AliasItems = null;

                c.ChipsComputed = false;
                c.AliasComputed = false;

                c.AliasRef = null;
                c.AliasSig = 0;
            }

            return c;
        }

        private static int ComputeAliasSig(IGamePart[]? aliases)
        {
            if (aliases == null || aliases.Length == 0)
                return 0;

            unchecked
            {
                int h = 17;
                h = (h * 31) ^ aliases.Length;

                for (int i = 0; i < aliases.Length; i++)
                {
                    var ap = aliases[i];
                    if (ap == null)
                    {
                        h = (h * 31);
                        continue;
                    }

                    h = (h * 31) ^ System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(ap);

                    var id = ap.GetDirectoryId();
                    h = (h * 31) ^ (id != null ? StringComparer.Ordinal.GetHashCode(id) : 0);
                }

                return h;
            }
        }
        private static IReadOnlyList<(string LabelKey, string Text)> GetCachedAliasItems(TreeNode node, IGamePart part)
        {
            var c = GetCache(node, part);

            var aliases = part.GetSoftwareAliases();

            int sig = ComputeAliasSig(aliases);

            bool shouldRebuild =
                !c.AliasComputed ||
                !ReferenceEquals(c.AliasRef, aliases) ||
                c.AliasSig != sig;

            if (shouldRebuild)
            {
                var built = BuildAliasItems(part);

                c.AliasRef = aliases;
                c.AliasSig = sig;

                if (aliases != null && aliases.Length > 0 && (built == null || built.Count == 0))
                {
                    c.AliasItems = null;
                    c.AliasComputed = false;
                    return Array.Empty<(string LabelKey, string Text)>();
                }

                c.AliasItems = built;
                c.AliasComputed = true;
            }

            return c.AliasItems ?? Array.Empty<(string LabelKey, string Text)>();
        }

        private static IReadOnlyList<string> GetCachedLabelChips(TreeNode node, object tag)
        {
            var c = GetCache(node, tag);

            if (!c.ChipsComputed)
            {
                c.Chips = tag is IGameFamily fam
                    ? GetLabelChips(fam)
                    : GetLabelChips((IGame)tag);

                c.ChipsComputed = true;
            }

            return c.Chips ?? Array.Empty<string>();
        }

        private static Font GetSpacerMessageFont(Font source)
        {
            // Cache one scaled bold font per "signature" to avoid allocating per draw.
            var fam = source.FontFamily;

            var sig =
                fam.Name + "|" +
                source.SizeInPoints.ToString("R") + "|" +
                SpacerMessageFontScale.ToString("R") + "|" +
                source.GdiCharSet.ToString() + "|" +
                source.GdiVerticalFont.ToString();

            if (cachedSpacerMessageFont != null && string.Equals(cachedSpacerMessageFontSig, sig, StringComparison.Ordinal))
                return cachedSpacerMessageFont;

            cachedSpacerMessageFont?.Dispose();
            cachedSpacerMessageFontSig = sig;

            cachedSpacerMessageFont = new Font(
                fam,
                source.SizeInPoints * SpacerMessageFontScale,
                FontStyle.Bold,
                GraphicsUnit.Point,
                source.GdiCharSet,
                source.GdiVerticalFont);

            return cachedSpacerMessageFont;
        }

        private static bool IsDisabled(
            object? tag,
            IReadOnlySet<Type> disabledTagTypes,
            IReadOnlySet<IGameEntity> enabledEntities)
        {
            if (tag == null)
                return false;

            if (tag is IGameEntity entity && enabledEntities.Contains(entity))
                return false;

            if (disabledTagTypes.Count == 0)
                return false;

            var tagType = tag.GetType();

            if (disabledTagTypes.Contains(tagType))
                return true;

            foreach (var t in disabledTagTypes)
                if (t.IsAssignableFrom(tagType))
                    return true;

            return false;
        }

        private static int MeasureAliasItemsEndX(
            Graphics g,
            Font nodeFont,
            int aliasStartX,
            IReadOnlyList<(string LabelKey, string Text)> aliasItems)
        {
            int sepW = GetAliasSeparatorWidth(g, nodeFont);

            int x = aliasStartX;

            for (int i = 0; i < aliasItems.Count; i++)
            {
                x += sepW;

                var bmp = GetChipBitmap(aliasItems[i].LabelKey, small: true);
                x += bmp.Width;

                x += ChipToTextGap;

                x += MeasureTextWidth(g, aliasItems[i].Text, nodeFont);
            }

            return x;
        }

        private static FocusRelation GetFocusRelation(TreeNode node, TreeNode? focusNode)
        {
            if (!FOCUS_ROW_Enable)
                return FocusRelation.None;

            if (focusNode == null)
                return FocusRelation.None;

            if (ReferenceEquals(node, focusNode))
                return FocusRelation.Focus;

            var prev = focusNode.PrevVisibleNode;
            if (prev != null && ReferenceEquals(node, prev))
                return FocusRelation.Above;

            var next = focusNode.NextVisibleNode;
            if (next != null && ReferenceEquals(node, next))
                return FocusRelation.Below;

            return FocusRelation.None;
        }

        private static void ApplyNeighbourSeparatorOverlay(Graphics g, TreeView tv, Rectangle rowRect, FocusRelation rel)
        {
            if (rowRect.Width <= 0 || rowRect.Height <= 0)
                return;

            int yTop = rowRect.Top;
            int yBotEx = rowRect.Bottom;
            int yBot = yBotEx - 1;

            int yMid = yTop + (rowRect.Height / 2);
            if (yMid <= yTop) yMid = yTop + 1;
            if (yMid > yBot) yMid = yBot;

            var solidColor = FocusNodeDarkMode ? Color.Black : tv.BackColor;

            var hairlineColor = SystemColors.ControlDarkDark;

            using var solid = new SolidBrush(solidColor);
            using var hair = new SolidBrush(hairlineColor);

            bool forceSolidBeforeRow = FocusNodeDarkMode && FOCUS_NEIGHBOUR_SOLID_BLACK_BEFORE_ROW;

            if (rel == FocusRelation.Above)
            {
                int blackY = yMid - 1;
                if (blackY < yTop) blackY = yTop;

                int gradBottomY = blackY - 1;
                if (gradBottomY < yTop) gradBottomY = yTop;

                int hairY = blackY + 1;
                if (hairY > yBot) hairY = yBot;

                var solidRect = new Rectangle(rowRect.Left, yMid, rowRect.Width, yBotEx - yMid);
                if (solidRect.Height > 0)
                    g.FillRectangle(solid, solidRect);

                var cTop = Color.FromArgb(0, 0, 0, 0);
                var cBot = Color.FromArgb(NEIGHBOUR_FADE_PEAK_ALPHA, 0, 0, 0);

                var gradRect = new Rectangle(rowRect.Left, yTop, rowRect.Width, (gradBottomY - yTop) + 1);
                if (gradRect.Height > 0)
                {
                    if (forceSolidBeforeRow)
                        g.FillRectangle(Brushes.Black, gradRect);
                    else
                        FillVerticalGradientNoGaps(g, gradRect, cTop, cBot);
                }

                g.FillRectangle(Brushes.Black, new Rectangle(rowRect.Left, blackY, rowRect.Width, 1));

                if (hairY >= yTop && hairY <= yBot && hairY != blackY)
                    g.FillRectangle(hair, new Rectangle(rowRect.Left, hairY, rowRect.Width, 1));

                return;
            }

            if (rel == FocusRelation.Below)
            {
                int blackY = yMid;
                if (blackY < yTop) blackY = yTop;
                if (blackY > yBot) blackY = yBot;

                int hairY = blackY - 1;
                if (hairY < yTop) hairY = yTop;

                var solidRect = new Rectangle(rowRect.Left, yTop, rowRect.Width, blackY - yTop);
                if (solidRect.Height > 0)
                    g.FillRectangle(solid, solidRect);

                if (hairY >= yTop && hairY <= yBot && hairY != blackY)
                    g.FillRectangle(hair, new Rectangle(rowRect.Left, hairY, rowRect.Width, 1));

                g.FillRectangle(Brushes.Black, new Rectangle(rowRect.Left, blackY, rowRect.Width, 1));

                int gradTopY = blackY + 1;
                if (gradTopY > yBot)
                    return;

                var cTop = Color.FromArgb(NEIGHBOUR_FADE_PEAK_ALPHA, 0, 0, 0);
                var cBot = Color.FromArgb(0, 0, 0, 0);

                var gradRect = new Rectangle(rowRect.Left, gradTopY, rowRect.Width, yBotEx - gradTopY);
                if (gradRect.Height > 0)
                    FillVerticalGradientNoGaps(g, gradRect, cTop, cBot);

                return;
            }
        }

        private static void FillVerticalGradientNoGaps(Graphics g, Rectangle clipRect, Color top, Color bottom)
        {
            if (clipRect.Width <= 0 || clipRect.Height <= 0)
                return;

            var state = g.Save();
            try
            {
                g.SetClip(clipRect);

                var r = clipRect;
                r.Inflate(0, 1);

                using var lg = new LinearGradientBrush(r, top, bottom, LinearGradientMode.Vertical)
                {
                    WrapMode = WrapMode.TileFlipY
                };

                g.FillRectangle(lg, r);
            }
            finally
            {
                g.Restore(state);
            }
        }

        private static int GetAlignedTop(Rectangle bounds, int contentHeight, NodeVAlign align)
        {
            if (contentHeight < 1)
                contentHeight = 1;

            int top;

            switch (align)
            {
                case NodeVAlign.Top:
                    top = bounds.Top + VAlignEdgeInsetPx;
                    break;

                case NodeVAlign.Bottom:
                    top = bounds.Bottom - contentHeight - VAlignEdgeInsetPx;
                    break;

                default:
                    top = bounds.Top + (bounds.Height - contentHeight) / 2;
                    break;
            }

            int minTop = bounds.Top;
            int maxTop = bounds.Bottom - contentHeight;

            if (top < minTop)
                top = minTop;

            if (top > maxTop)
                top = maxTop;

            return top;
        }

        private static List<string> GetLabelChipsCore(
            IEnumerable<IGamePart> parts)
        {
            var chips = new List<string>();
            var distinctLabels = new HashSet<string>(StringComparer.Ordinal);

            foreach (var p in parts)
            {
                if (p is not IGamePart part)
                    continue;

                var labelKey = CreateDatReferenceLabel(part);

                if (distinctLabels.Add(labelKey))
                    chips.Add(labelKey);
            }

            return chips;
        }


        // Gradient: dark gray at top -> fully transparent at bottom, spanning the whole row width.
        private static void DrawSpacerGradient(Graphics g, Rectangle rowRect)
        {
            if (rowRect.Width <= 0 || rowRect.Height <= 0)
                return;

            // Tweak the top alpha - stronger/weaker.
            var top = Color.FromArgb(90, Color.DimGray);
            var bottom = Color.FromArgb(0, Color.DimGray);

            using var brush = new LinearGradientBrush(rowRect, top, bottom, LinearGradientMode.Vertical);
            g.FillRectangle(brush, rowRect);
        }

        // "First node is a spacer" in both rootless and rooted trees.
        private static bool IsFirstSpacerNodeInTree(TreeView tv, TreeNode spacerNode)
        {
            // Rootless: first top-level node.
            if (spacerNode.Parent == null)
                return tv.Nodes.Count > 0 && ReferenceEquals(tv.Nodes[0], spacerNode);

            // Rooted: single root, first child is the spacer.
            if (tv.Nodes.Count == 1 && ReferenceEquals(tv.Nodes[0], spacerNode.Parent))
                return spacerNode.Parent.Nodes.Count > 0 && ReferenceEquals(spacerNode.Parent.Nodes[0], spacerNode);

            return false;
        }

        private static List<string> GetLabelChips(IGame game)
        {
            var chips = GetLabelChipsCore(game.GetGameParts(true));

            if (chips.Count == 0)
                return chips;

            return new List<string> { chips[0] };
        }

        private static List<string> GetLabelChips(IGameFamily gameFamily)
        {
            var list = gameFamily.GetAllGameParts(true);
            if (list.Length == 0)
                return new List<string>();

            return GetLabelChipsCore(list);
        }

        private static int MeasureLabelChipsWidth(
            Graphics g,
            Font font,
            Rectangle nodeBounds,
            IReadOnlyList<string> chips,
            bool alphaPrimaryChipSet,
            bool includeLeadingGap,
            bool includeTrailingGap)
        {
            if (chips == null || chips.Count == 0)
                return 0;

            int x = includeLeadingGap ? ChipLeadingGap : 0;

            for (int i = 0; i < chips.Count; i++)
            {
                bool small = IsSecondaryChip(alphaPrimaryChipSet, i, chips.Count, forceSecondaryStyle: false);
                var bmp = GetChipBitmap(chips[i], small);
                x += bmp.Width + ChipGap;
            }

            if (!includeTrailingGap)
                x -= ChipGap;

            return x;
        }

        private static string CreateDatReferenceLabel(IGamePart part) =>
            part.GetDirectoryId() ?? string.Empty;
       
        private static int Clamp0To100(int v) => v < 0 ? 0 : (v > 100 ? 100 : v);

        private static int MeasureTextWidth(Graphics g, string text, Font font)
        {
            return TextRenderer.MeasureText(g, text, font, Size.Empty, NodeTextFlags).Width;
        }

        private static IReadOnlyList<(string LabelKey, string Text)>? BuildAliasItems(
            IGamePart part)
        {
            var aliases = part.GetSoftwareAliases();
            if (aliases == null || aliases.Length == 0)
                return null;

            var tmp = new List<(string LabelKey, string Text)>(aliases.Length);

            foreach (var a in aliases)
            {
                if (a is not IGamePart ap)
                    continue;

                var labelKey = CreateDatReferenceLabel(ap);

                tmp.Add((labelKey, DatinateHelper.GetGamePartNameRender(ap)));
            }

            return tmp.Count == 0 ? null : tmp;
        }

        private static int DrawLabelChipsInternal(
            Graphics g,
            Font font,
            Rectangle nodeBounds,
            int startX,
            IReadOnlyList<string> chips,
            NodeVAlign align,
            bool includeLeadingGap,
            bool includeTrailingGap,
            bool alphaPrimaryChipSet,
            Color rowBackColor,
            bool forceSecondaryStyle = false,
            Color? textColorOverride = null)
        {
            if (chips == null || chips.Count == 0)
                return startX;

            int x = startX + (includeLeadingGap ? ChipLeadingGap : 0);

            for (int i = 0; i < chips.Count; i++)
            {
                bool small = IsSecondaryChip(alphaPrimaryChipSet, i, chips.Count, forceSecondaryStyle);

                NodeVAlign chipAlign =
                    (DATINATE_CHIP_VTOP_VTopSecondaryChips && small)
                        ? NodeVAlign.Top
                        : align;

                // We still grab the bitmap purely for height/measurement.
                var bmp = GetChipBitmap(chips[i], small);

                int y = GetAlignedTop(nodeBounds, bmp.Height, chipAlign);

                // IMPORTANT: draw using DatChipUtil.Draw/DrawSmall so the text is rendered on the final Graphics surface.
                int w = DrawChip(g, chips[i], small, x, y);

                x += w + ChipGap;
            }

            if (!includeTrailingGap)
                x -= ChipGap;

            return x;
        }

        private static bool TrySplitTrailingAngleSuffix(string text, out string mainText, out string angleSuffix)
        {
            mainText = text;
            angleSuffix = string.Empty;

            if (string.IsNullOrEmpty(text))
                return false;

            var t = text.TrimEnd();
            if (!t.EndsWith(">", StringComparison.Ordinal))
                return false;

            int lt = t.LastIndexOf('<');
            if (lt < 0)
                return false;

            mainText = t.Substring(0, lt);
            angleSuffix = t.Substring(lt);
            return angleSuffix.Length > 0;
        }

        private static int DrawNodeTextWithOptionalDimTrailingAngleSuffix(
            Graphics g,
            Rectangle rowBounds,
            string text,
            Font font,
            int x,
            int y,
            Color textColor,
            bool hasSearch,
            string? highlightText,
            Color matchBackColor,
            bool dimTrailingAngleSuffix)
        {
            if (!dimTrailingAngleSuffix || !TrySplitTrailingAngleSuffix(text, out var main, out var suffix))
            {
                if (hasSearch && !string.IsNullOrWhiteSpace(highlightText))
                    return DrawTextWithOptionalHighlight(g, rowBounds, text, font, x, y, textColor, highlightText!, matchBackColor);

                TextRenderer.DrawText(g, text, font, new Point(x, y), textColor, NodeTextFlags);
                return x + TextRenderer.MeasureText(g, text, font, Size.Empty, NodeTextFlags).Width;
            }

            int xx = x;

            if (!string.IsNullOrEmpty(main))
            {
                if (hasSearch && !string.IsNullOrWhiteSpace(highlightText))
                    xx = DrawTextWithOptionalHighlight(g, rowBounds, main, font, xx, y, textColor, highlightText!, matchBackColor);
                else
                {
                    TextRenderer.DrawText(g, main, font, new Point(xx, y), textColor, NodeTextFlags);
                    xx += TextRenderer.MeasureText(g, main, font, Size.Empty, NodeTextFlags).Width;
                }
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                TextRenderer.DrawText(g, suffix, font, new Point(xx, y), AlphaChipTextColor, NodeTextFlags);
                xx += TextRenderer.MeasureText(g, suffix, font, Size.Empty, NodeTextFlags).Width;
            }

            return xx;
        }

        private static void ApplyExcludedOverlay(Graphics g, TreeView tv, TreeNode node, Rectangle bounds, int rowWidth, bool suppressSelectionHighlight)
        {
            var selectedBackColor = Color.FromArgb(215, 228, 242);
            var rowBackColor = (node == tv.SelectedNode && !suppressSelectionHighlight) ? selectedBackColor : tv.BackColor;

            var rowRect = new Rectangle(0, bounds.Top, rowWidth, bounds.Height);

            int fadeA = (Clamp0To100(ExcludedRowFadeOpacityPercent) * 255 + 50) / 100;
            using (var fade = new SolidBrush(Color.FromArgb(fadeA, rowBackColor)))
                g.FillRectangle(fade, rowRect);

            int hatchA = (Clamp0To100(ExcludedHatchOpacityPercent) * 255 + 50) / 100;
            using (var hatch = new HatchBrush(
                ExcludedHatchStyle,
                Color.FromArgb(hatchA, ExcludedNodeHatchColor),
                Color.FromArgb(0, 0, 0, 0)))
            {
                g.FillRectangle(hatch, rowRect);
            }
        }
        private static int DrawChip(Graphics g, string label, bool small, int x, int y)
        {
            return small
                ? DatChipUtil.DrawSmall(g, label, x, y)
                : DatChipUtil.Draw(g, label, x, y);
        }

        private static bool IsSecondaryChip(
            bool alphaPrimaryChipSet,
            int chipIndex,
            int chipCount,
            bool forceSecondaryStyle)
        {
            if (forceSecondaryStyle)
                return true;

            return alphaPrimaryChipSet && chipCount > 1 && chipIndex > 0;
        }

        private static bool IsHotRow(TreeView tv, TreeNode node)
        {
            if (!HOVER_ROW_Enable)
                return false;

            return tv is DatGrouperTreeView dgtv && ReferenceEquals(dgtv.HotNode, node);
        }

        private static bool IsTagDisabledOnTv(TreeView tv, TreeNode? node)
        {
            return tv is DatGrouperTreeView dgtv && dgtv.IsNodeDisabled(node);
        }

        private static void ApplyHotRowOverlay(
            Graphics g,
            TreeView tv,
            Rectangle bounds,
            int rightMost,
            int rowWidth)
        {
            // Keep the overlay within the visible client width.
            int maxW = rowWidth - bounds.Left;
            if (maxW <= 0)
                return;

            int w = rightMost - bounds.Left + 2;
            if (w < 0) w = 0;
            if (w > maxW) w = maxW;
            if (w <= 0 || bounds.Height <= 0)
                return;

            var r = new Rectangle(bounds.Left, bounds.Top, w, bounds.Height);

            using (var fill = new SolidBrush(Color.FromArgb(HOVER_ROW_FillAlpha, HOVER_ROW_FillRgb)))
                g.FillRectangle(fill, r);

            if (HOVER_ROW_DrawBorder)
            {
                var rr = r;
                rr.Width -= 1;
                rr.Height -= 1;

                if (HOVER_ROW_BorderInsetPx != 0)
                    rr.Inflate(-HOVER_ROW_BorderInsetPx, -HOVER_ROW_BorderInsetPx);

                if (rr.Width > 0 && rr.Height > 0)
                {
                    using var pen = new Pen(Color.FromArgb(HOVER_ROW_BorderAlpha, HOVER_ROW_BorderRgb), 1f);
                    g.DrawRectangle(pen, rr);
                }
            }

            if (HOVER_ROW_DrawLeftAccentBar)
            {
                var bar = new Rectangle(
                    r.Left,
                    r.Top + 1,
                    HOVER_ROW_LeftBarWidthPx,
                    Math.Max(0, r.Height - 2));

                if (bar.Width > 0 && bar.Height > 0)
                {
                    using var b = new SolidBrush(Color.FromArgb(HOVER_ROW_LeftBarAlpha, HOVER_ROW_LeftBarRgb));
                    g.FillRectangle(b, bar);
                }
            }
        }
        private static int GetAliasSeparatorWidth(Graphics g, Font font)
        {
            if (!ReferenceEquals(cachedAliasSeparatorFont, font))
            {
                cachedAliasSeparatorFont = font;
                cachedAliasSeparatorWidth = TextRenderer.MeasureText(g, AliasSeparator, font, Size.Empty, NodeTextFlags).Width;
            }

            return cachedAliasSeparatorWidth;
        }


        private sealed class NodeRenderCache
        {
            public object? TagRef;

            public IReadOnlyList<string>? Chips;
            public IReadOnlyList<(string LabelKey, string Text)>? AliasItems;

            public bool ChipsComputed;
            public bool AliasComputed;

            public object? AliasRef;
            public int AliasSig;
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

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private const int GWL_STYLE = -16;
        private const int WS_VSCROLL = 0x00200000;

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

        private const int TRAILING_LEFT_COUNT_TEXT_GAP_PX = 0;
        private const int TRAILING_LEFT_COUNT_ICON_GAP_PX = 4;
        private const float TRAILING_LEFT_COUNT_FONT_SCALE = 1.08f;
        private const bool TRAILING_LEFT_COUNT_FONT_BOLD = true;
        private static Font? cachedTrailingLeftCountFont;
        private static string? cachedTrailingLeftCountFontSig;
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
                        var textSize = TextRenderer.MeasureText(g, countText, textFont, Size.Empty, NodeTextFlags);

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
                                AlphaChipTextColor,
                                NodeTextFlags);

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
