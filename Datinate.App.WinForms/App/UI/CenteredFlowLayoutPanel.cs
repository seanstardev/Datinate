using System.ComponentModel;

namespace datinate.app
{
    internal sealed class CenteredFlowLayoutPanel : FlowLayoutPanel
    {
        public enum RowSpacingMode
        {
            Center,
            SpaceBetween,
            SpaceEvenly
        }

        private bool centering;

        private RowSpacingMode spacingMode = RowSpacingMode.Center;

        [Browsable(true)]
        [Category("Layout")]
        [DefaultValue(RowSpacingMode.Center)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public RowSpacingMode SpacingMode
        {
            get => spacingMode;
            set
            {
                if (spacingMode == value)
                    return;

                spacingMode = value;
                RequestRelayout();
            }
        }

        private HorizontalAlignment stragglerAlignment = HorizontalAlignment.Center;

        [Browsable(true)]
        [Category("Layout")]
        [DefaultValue(HorizontalAlignment.Center)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public HorizontalAlignment StragglerAlignment
        {
            get => stragglerAlignment;
            set
            {
                if (stragglerAlignment == value)
                    return;

                stragglerAlignment = value;
                RequestRelayout();
            }
        }

        private bool centerVertically;

        [Browsable(true)]
        [Category("Layout")]
        [DefaultValue(false)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public bool CenterVertically
        {
            get => centerVertically;
            set
            {
                if (centerVertically == value)
                    return;

                centerVertically = value;
                RequestRelayout();
            }
        }

        private void RequestRelayout()
        {
            PerformLayout();
            Invalidate();

            Parent?.PerformLayout();
            Parent?.Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (centering)
                return;

            base.OnLayout(levent);

            if (DatinatePerformanceUtil.FLOW_LAYOUT_UseStock == false)
            {
#pragma warning disable CS0162 // Unreachable code detected
                centering = true;
                try
                {
                    LayoutRows();
                }
                finally
                {
                    centering = false;
                }
#pragma warning restore CS0162 // Unreachable code detected
            }
        }

        private void LayoutRows()
        {
            if (Controls.Count == 0)
                return;

            int availableWidth = ClientSize.Width - Padding.Left - Padding.Right;
            if (availableWidth <= 0)
                return;

            var visible = new List<Control>(Controls.Count);
            for (int i = 0; i < Controls.Count; i++)
            {
                var c = Controls[i];
                if (c.Visible)
                    visible.Add(c);
            }

            if (visible.Count == 0)
                return;

            visible.Sort(static (a, b) =>
            {
                int t = a.Top.CompareTo(b.Top);
                return t != 0 ? t : a.Left.CompareTo(b.Left);
            });

            var rows = new List<List<Control>>(16);

            List<Control>? current = null;
            int currentBottom = int.MinValue;

            for (int i = 0; i < visible.Count; i++)
            {
                var c = visible[i];

                if (current == null || c.Top >= currentBottom)
                {
                    current = new List<Control>(8);
                    rows.Add(current);
                    currentBottom = c.Bottom;
                }
                else
                {
                    if (c.Bottom > currentBottom)
                        currentBottom = c.Bottom;
                }

                current.Add(c);
            }

            if (rows.Count == 0)
                return;

            for (int r = 0; r < rows.Count; r++)
                rows[r].Sort(static (a, b) => a.Left.CompareTo(b.Left));

            int maxRowCount = 0;
            int maxRowOuter = 0;

            for (int r = 0; r < rows.Count; r++)
            {
                int cnt = rows[r].Count;
                if (cnt > maxRowCount)
                    maxRowCount = cnt;

                int outer = 0;
                for (int i = 0; i < rows[r].Count; i++)
                {
                    var c = rows[r][i];
                    int ml = c.Margin.Left; if (ml < 0) ml = 0;
                    int mr = c.Margin.Right; if (mr < 0) mr = 0;
                    outer += c.Width + ml + mr;
                }

                if (outer > maxRowOuter)
                    maxRowOuter = outer;
            }

            if (maxRowOuter <= 0)
                return;

            int rowBoxWidth = SpacingMode == RowSpacingMode.Center ? maxRowOuter : availableWidth;
            if (rowBoxWidth > availableWidth)
                rowBoxWidth = availableWidth;

            int rowBoxLeft = Padding.Left + (availableWidth - rowBoxWidth) / 2;

            int lastRowIndex = rows.Count - 1;

            for (int r = 0; r < rows.Count; r++)
            {
                var row = rows[r];
                if (row.Count == 0)
                    continue;

                int totalOuter = 0;
                for (int i = 0; i < row.Count; i++)
                {
                    var c = row[i];
                    int ml = c.Margin.Left; if (ml < 0) ml = 0;
                    int mr = c.Margin.Right; if (mr < 0) mr = 0;
                    totalOuter += c.Width + ml + mr;
                }

                int extra = availableWidth - totalOuter;
                if (extra < 0)
                    extra = 0;

                bool isStragglerRow = (r == lastRowIndex) && (row.Count < maxRowCount);

                if (isStragglerRow && row.Count > 0 && StragglerAlignment != HorizontalAlignment.Center)
                {
                    int x = rowBoxLeft;

                    if (StragglerAlignment == HorizontalAlignment.Right)
                    {
                        int slack = rowBoxWidth - totalOuter;
                        if (slack < 0)
                            slack = 0;

                        x = rowBoxLeft + slack;
                    }

                    for (int i = 0; i < row.Count; i++)
                    {
                        var c = row[i];
                        int ml = c.Margin.Left; if (ml < 0) ml = 0;
                        int mr = c.Margin.Right; if (mr < 0) mr = 0;

                        c.Left = x + ml;
                        x += c.Width + ml + mr;
                    }

                    continue;
                }

                if (SpacingMode == RowSpacingMode.Center || row.Count == 1)
                {
                    int x = Padding.Left + (extra / 2);

                    for (int i = 0; i < row.Count; i++)
                    {
                        var c = row[i];
                        int ml = c.Margin.Left; if (ml < 0) ml = 0;
                        int mr = c.Margin.Right; if (mr < 0) mr = 0;

                        c.Left = x + ml;
                        x += c.Width + ml + mr;
                    }

                    continue;
                }

                if (SpacingMode == RowSpacingMode.SpaceBetween)
                {
                    int gaps = row.Count - 1;
                    if (gaps <= 0)
                        continue;

                    int baseGap = extra / gaps;
                    int rem = extra % gaps;

                    int x = Padding.Left;

                    for (int i = 0; i < row.Count; i++)
                    {
                        var c = row[i];
                        int ml = c.Margin.Left; if (ml < 0) ml = 0;
                        int mr = c.Margin.Right; if (mr < 0) mr = 0;

                        c.Left = x + ml;
                        x += c.Width + ml + mr;

                        if (i < row.Count - 1)
                            x += baseGap + (i < rem ? 1 : 0);
                    }

                    continue;
                }

                if (SpacingMode == RowSpacingMode.SpaceEvenly)
                {
                    int gaps = row.Count + 1;
                    int baseGap = extra / gaps;
                    int rem = extra % gaps;

                    int lead = baseGap + (0 < rem ? 1 : 0);
                    int x = Padding.Left + lead;

                    for (int i = 0; i < row.Count; i++)
                    {
                        var c = row[i];
                        int ml = c.Margin.Left; if (ml < 0) ml = 0;
                        int mr = c.Margin.Right; if (mr < 0) mr = 0;

                        c.Left = x + ml;
                        x += c.Width + ml + mr;

                        if (i < row.Count - 1)
                        {
                            int gapIndex = i + 1;
                            x += baseGap + (gapIndex < rem ? 1 : 0);
                        }
                    }
                }
            }

            if (CenterVertically)
                ApplyVerticalCentering(visible);
        }

        private void ApplyVerticalCentering(List<Control> visible)
        {
            int availableHeight = ClientSize.Height - Padding.Top - Padding.Bottom;
            if (availableHeight <= 0)
                return;

            int minTop = int.MaxValue;
            int maxBottom = int.MinValue;

            for (int i = 0; i < visible.Count; i++)
            {
                var c = visible[i];
                if (c.Top < minTop) minTop = c.Top;
                if (c.Bottom > maxBottom) maxBottom = c.Bottom;
            }

            int contentHeight = maxBottom - minTop;
            if (contentHeight <= 0 || contentHeight >= availableHeight)
                return;

            int targetTop = Padding.Top + (availableHeight - contentHeight) / 2;
            int delta = targetTop - minTop;
            if (delta == 0)
                return;

            for (int i = 0; i < visible.Count; i++)
                visible[i].Top += delta;
        }
    }
}
