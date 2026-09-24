using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using static datinate.app.DatGrouperTreeView;

namespace datinate.app
{
    public static class TreeRenderUtil
    {
        private const int ALIAS_CONNECTOR_GRAY = 160;

        private const bool FOCUS_NODE_DARK_MODE = true;

        // NOTE: Hover row styling (very subtle; designed for dense UI)
        private const bool HOVER_ROW_SkipSelected = true;        // selection already has a strong cue
        private const bool HOVER_ROW_SkipDisabled = true;        // disabled should feel inert

        private const int HOVER_ROW_FillAlpha = 38;              // 0..255 (keep low)
        private const int HOVER_ROW_BorderAlpha = 55;            // 0..255
        private const int HOVER_ROW_BorderInsetPx = 0;

        private const int DisabledRowFadeOpacityPercent = 70;

        private const int ChipGap = 8;

        private const int ChipLeadingGap = 6;
        private const int ChipToTextGap = 6;

        private const int VAlignEdgeInsetPx = 2;

        private const int AliasStartX = 300;
        private const int AliasMinGapAfterChips = 100;
        private const string AliasSeparator = "     ";

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
        private static readonly ConditionalWeakTable<TreeNode, NodeRenderCache> RenderCache = [];
        private static readonly Color HOVER_ROW_FillRgb = Color.FromArgb(215, 228, 242);  // matches your selection family
        private static readonly Color HOVER_ROW_BorderRgb = Color.FromArgb(120, 145, 170); // neutral-ish

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

            bool preserveFocusedFamilyBackground =
                FOCUS_NODE_DARK_MODE &&
                node.Tag is IGameFamily &&
                ReferenceEquals(node, focusNode);

            if (renderNodeAsExcluded && !preserveFocusedFamilyBackground)
            {
                var excludedClip =
                    new Rectangle(0, bounds.Top, rowWidth, bounds.Height);

                // NOTE: The row immediately beneath the focus node uses its top half
                // as part of the media/focus visual treatment. Do not hatch over it.
                if (FOCUS_NODE_DARK_MODE &&
                    GetFocusRelation(node, focusNode) == FocusRelation.Below)
                {
                    int blackY = bounds.Top + (bounds.Height / 2);

                    // Preserve the top half plus the central black separator line.
                    int exclusionTop = blackY + 1;

                    excludedClip.Y = exclusionTop;
                    excludedClip.Height = Math.Max(0, bounds.Bottom - exclusionTop);
                }

                if (excludedClip.Height > 0)
                {
                    var state = g.Save();

                    try
                    {
                        g.SetClip(excludedClip);

                        ApplyExcludedOverlay(
                            g,
                            tv,
                            node,
                            bounds,
                            rowWidth,
                            suppressSelectionHighlight);
                    }
                    finally
                    {
                        g.Restore(state);
                    }
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
            TreeRenderTrailingIconUtil.Draw(
                g,
                tv,
                node,
                bounds,
                rowWidth,
                contentRightMostDrawn,
                media,
                focusNode);
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
                bool focusDark = FOCUS_NODE_DARK_MODE && focusRel == FocusRelation.Focus;

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
                    bool focusDark = FOCUS_NODE_DARK_MODE && focusRel == FocusRelation.Focus;

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

        public static void DrawOverlay(
            string title,
            string body,
            Graphics g,
            TreeView tv,
            Rectangle bounds)
        {
            // NOTE: DrawNode is called once per visible row.
            // ... Only allow the final visible row to perform the full overlay render.
            var currentNode = tv.GetNodeAt(
                Math.Max(1, bounds.Left + 1),
                bounds.Top + Math.Max(1, bounds.Height / 2));

            if (currentNode != null)
            {
                var next = currentNode.NextVisibleNode;

                if (next != null &&
                    next.Bounds.Height > 0 &&
                    next.Bounds.Top < tv.ClientSize.Height)
                {
                    return;
                }
            }

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
            bool suppressSelectionHighlight)
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

            int gray = ALIAS_CONNECTOR_GRAY;
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

            var solidColor = FOCUS_NODE_DARK_MODE ? Color.Black : tv.BackColor;

            var hairlineColor = SystemColors.ControlDarkDark;

            using var solid = new SolidBrush(solidColor);
            using var hair = new SolidBrush(hairlineColor);

            bool forceSolidBeforeRow = FOCUS_NODE_DARK_MODE;

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


        // NOTE: Gradient: dark gray at top -> fully transparent at bottom, spanning the whole row width.
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

        // NOTE: "First node is a spacer" in both rootless and rooted trees.
        private static bool IsFirstSpacerNodeInTree(TreeView tv, TreeNode spacerNode)
        {
            // NOTE: Rootless: first top-level node.
            if (spacerNode.Parent == null)
                return tv.Nodes.Count > 0 && ReferenceEquals(tv.Nodes[0], spacerNode);

            // NOTE: Rooted: single root, first child is the spacer.
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

        private static int MeasureTextWidth(Graphics g, string text, Font font) =>
            TextRenderer.MeasureText(g, text, font, Size.Empty, NodeTextFlags).Width;
        
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

                tmp.Add((labelKey, TreeNodeNameUtil.GetGamePartNameRender(ap)));
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
                    small
                        ? NodeVAlign.Top
                        : align;

                // NOTE: We still grab the bitmap purely for height/measurement.
                var bmp = GetChipBitmap(chips[i], small);

                int y = GetAlignedTop(nodeBounds, bmp.Height, chipAlign);

                // NOTE: Important: draw using DatChipUtil.Draw/DrawSmall so the text is rendered on the final Graphics surface.
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

        private static bool IsHotRow(TreeView tv, TreeNode node) =>
            tv is DatGrouperTreeView dgtv && ReferenceEquals(dgtv.HotNode, node);
        
        private static bool IsTagDisabledOnTv(TreeView tv, TreeNode? node) =>
            tv is DatGrouperTreeView dgtv && dgtv.IsNodeDisabled(node);
        
        private static void ApplyHotRowOverlay(
            Graphics g,
            TreeView tv,
            Rectangle bounds,
            int rightMost,
            int rowWidth)
        {
            // NOTE: Keep the overlay within the visible client width.
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
    }
}
