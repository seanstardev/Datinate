using System.ComponentModel;

namespace datinate.app
{
    public sealed partial class FlagListUI : ListView
    {
        private readonly ColumnHeader flag_col;
        private readonly ColumnHeader count_col;
        private readonly FlagListSorter sorter;

        private int desiredCountWidth;

        public event EventHandler? SelectedFlagChanged;

        public FlagListUI()
        {
            flag_col = new ColumnHeader();
            count_col = new ColumnHeader();
            sorter = new FlagListSorter();

            Columns.AddRange(new[] { flag_col, count_col });

            View = View.Details;
            FullRowSelect = true;
            HideSelection = false;
            MultiSelect = false;
            HeaderStyle = ColumnHeaderStyle.Clickable;
            UseCompatibleStateImageBehavior = false;

            flag_col.Text = "Flag";
            flag_col.TextAlign = HorizontalAlignment.Left;

            count_col.Text = "Count";
            count_col.TextAlign = HorizontalAlignment.Right;

            ListViewItemSorter = sorter;

            ColumnClick += OnColumnClick;
            SelectedIndexChanged += OnSelectedIndexChanged;

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                FitColumns();
        }

        public void SetColumnHeaderAsCategory()
        {
            flag_col.Text = "Category";
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? SelectedFlag
            => SelectedItems.Count == 0 ? null : (SelectedItems[0].Tag as string);


        public void FitColumns()
        {
            if (!IsHandleCreated)
                return;

            if (Columns.Count < 2)
                return;

            var w = ClientSize.Width;
            if (BorderStyle != BorderStyle.None)
                w -= 2;

            var itemHeight = TextRenderer.MeasureText("A", Font).Height + 1;
            var hasVScroll = Items.Count * itemHeight > ClientSize.Height;

            if (hasVScroll)
                w -= SystemInformation.VerticalScrollBarWidth;

            if (w < 140)
                w = 140;

            var countW = desiredCountWidth > 0 ? desiredCountWidth : 60;

            if (countW < 40)
                countW = 40;
            if (countW > 140)
                countW = 140;

            var flagW = w - countW;

            if (flagW < 60)
            {
                flagW = 60;
                countW = w - flagW;
                if (countW < 40)
                    countW = 40;
            }

            Columns[0].Width = flagW;
            Columns[1].Width = countW;
        }

        public void SetRows((string Flag, int Count)[] rows, string? selectFlag = null)
        {
            BeginUpdate();
            try
            {
                Items.Clear();

                var maxCount = 0;

                for (int i = 0; i < rows.Length; i++)
                {
                    var (flag, count) = rows[i];
                    if (count > maxCount)
                        maxCount = count;

                    var item = new ListViewItem(flag)
                    {
                        Name = flag,
                        Tag = flag
                    };

                    item.UseItemStyleForSubItems = false;

                    var countText = count > 0 ? count.ToString() : string.Empty;
                    var countSub = item.SubItems.Add(countText);
                    countSub.ForeColor = SystemColors.GrayText;

                    Items.Add(item);
                }

                desiredCountWidth = MeasureCountColumnWidth(maxCount);

                FitColumns();

                if (!string.IsNullOrWhiteSpace(selectFlag) && Items.ContainsKey(selectFlag))
                {
                    var item = Items[selectFlag];
                    if (item != null)
                    {
                        item.Selected = true;
                        item.Focused = true;
                        item.EnsureVisible();
                    }
                }

                Sort();
            }
            finally
            {
                EndUpdate();
            }
        }

        public void ClearRows()
        {
            BeginUpdate();
            try
            {
                Items.Clear();
                desiredCountWidth = 0;
                FitColumns();
            }
            finally
            {
                EndUpdate();
            }
        }

        public void SetSort(int columnIndex, SortOrder order)
        {
            if (columnIndex < 0 || columnIndex > 1)
                return;

            sorter.SortColumn = columnIndex;
            sorter.Order = order == SortOrder.None ? SortOrder.Ascending : order;
            Sort();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                FitColumns();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                FitColumns();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            desiredCountWidth = 0;

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                FitColumns();
        }

        private int MeasureCountColumnWidth(int maxCount)
        {
            var headerW = TextRenderer.MeasureText(count_col.Text ?? string.Empty, Font).Width;
            var valueW = maxCount > 0
                ? TextRenderer.MeasureText(maxCount.ToString(), Font).Width
                : 0;

            var w = headerW > valueW ? headerW : valueW;
            w += 18;
            return w;
        }

        private void OnSelectedIndexChanged(object? sender, EventArgs e)
            => SelectedFlagChanged?.Invoke(this, EventArgs.Empty);

        private void OnColumnClick(object? sender, ColumnClickEventArgs e)
        {
            if (e.Column < 0 || e.Column > 1)
                return;

            if (sorter.SortColumn == e.Column)
                sorter.Order = sorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            else
            {
                sorter.SortColumn = e.Column;
                sorter.Order = e.Column == 1 ? SortOrder.Descending : SortOrder.Ascending;
            }

            Sort();
        }

        private sealed class FlagListSorter : System.Collections.IComparer
        {
            public int SortColumn { get; set; }
            public SortOrder Order { get; set; } = SortOrder.Ascending;

            public int Compare(object? x, object? y)
            {
                var a = (ListViewItem)x!;
                var b = (ListViewItem)y!;

                int result;

                if (SortColumn == 1)
                {
                    var av = int.TryParse(a.SubItems.Count > 1 ? a.SubItems[1].Text : string.Empty, out var ai) ? ai : int.MinValue;
                    var bv = int.TryParse(b.SubItems.Count > 1 ? b.SubItems[1].Text : string.Empty, out var bi) ? bi : int.MinValue;

                    result = av.CompareTo(bv);

                    if (result == 0)
                        result = string.Compare(a.Text, b.Text, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    result = string.Compare(a.Text, b.Text, StringComparison.OrdinalIgnoreCase);
                }

                return Order == SortOrder.Descending ? -result : result;
            }
        }
    }
}
