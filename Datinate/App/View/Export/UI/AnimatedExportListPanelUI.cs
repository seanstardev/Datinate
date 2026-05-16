using com.RADIO.Datinate.RMVC.Shared;
using System.ComponentModel;

namespace datinate.app
{
    public class AnimatedExportListPanelUI : Panel
    {
        private const int AnimationIntervalMs = 15;
        private const int BottomPaddingPx = 4;

        private readonly List<ExportPriorityItemUI> items = [];
        private readonly Dictionary<ExportPriorityItemUI, int> targetTops = [];
        private readonly System.Windows.Forms.Timer timer = new();
        private readonly VScrollBar scrollBar = new();

        private int scrollOffset;
        private int maxScroll;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ItemHeightPx { get; set; } = 50;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ItemGapPx { get; set; } = 3;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool AutoScroll
        {
            get => false;
            set => base.AutoScroll = false;
        }

        public AnimatedExportListPanelUI()
        {
            base.AutoScroll = false;
            DoubleBuffered = true;
            BackColor = Color.White;
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            scrollBar.Visible = false;
            scrollBar.ValueChanged += (_, _) => SetScrollValue(scrollBar.Value);

            Controls.Add(scrollBar);
            scrollBar.BringToFront();

            timer.Interval = AnimationIntervalMs;
            timer.Tick += (_, _) => TickAnimation();

            Resize += (_, _) => Relayout(false);
        }

        public bool ContainsItem(ExportPriorityItemUI item)
        {
            return items.Contains(item);
        }

        public void ClearItems(bool dispose)
        {
            timer.Stop();
            targetTops.Clear();

            foreach (var item in items.ToArray())
            {
                Controls.Remove(item);

                if (dispose)
                    item.Dispose();
            }

            items.Clear();
            scrollOffset = 0;
            maxScroll = 0;
            Relayout(false);
        }

        public void AddItem(ExportPriorityItemUI item, bool animate)
        {
            InsertItem(items.Count, item, animate);
        }

        public void InsertItem(int index, ExportPriorityItemUI item, bool animate)
        {
            if (items.Contains(item))
                items.Remove(item);

            index = Math.Max(0, Math.Min(index, items.Count));

            item.Width = GetItemWidth();
            item.Height = ItemHeightPx;

            if (item.Parent != this)
                Controls.Add(item);

            items.Insert(index, item);
            item.BringToFront();
            scrollBar.BringToFront();

            Relayout(animate);

            if (animate)
                item.PlayTransferAnimation();
        }

        public void RemoveItem(ExportPriorityItemUI item, bool dispose)
        {
            if (!items.Remove(item))
                return;

            targetTops.Remove(item);
            Controls.Remove(item);

            if (dispose)
                item.Dispose();

            Relayout(true);
        }

        public void MoveItemUp(ExportPriorityItemUI item)
        {
            int index = items.IndexOf(item);

            if (index <= 0)
                return;

            items.RemoveAt(index);
            items.Insert(index - 1, item);

            Relayout(true);
        }

        public void MoveItemDown(ExportPriorityItemUI item)
        {
            int index = items.IndexOf(item);

            if (index < 0 || index >= items.Count - 1)
                return;

            items.RemoveAt(index);
            items.Insert(index + 1, item);

            Relayout(true);
        }

        public IReadOnlyList<MediaExportPriorityItemDTO> GetDTOs()
        {
            var list = new List<MediaExportPriorityItemDTO>();

            foreach (var item in items)
                if (item.DTO != null)
                    list.Add(item.DTO);

            return list;
        }

        public void Relayout(bool animate)
        {
            SuspendLayout();

            ConfigureScrollBar();

            int width = GetItemWidth();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                item.Width = width;
                item.Height = ItemHeightPx;
                item.SetIndex(i + 1);
                item.SetMoveButtonsEnabled(i > 0, i < items.Count - 1);

                int targetTop = GetTargetTop(i);
                targetTops[item] = targetTop;

                if (!animate)
                    item.Location = new Point(0, targetTop - scrollOffset);
                else if (item.Left != 0)
                    item.Left = 0;
            }

            scrollBar.BringToFront();

            ResumeLayout(false);

            if (animate)
                timer.Start();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!scrollBar.Visible)
            {
                base.OnMouseWheel(e);
                return;
            }

            int lines = Math.Max(1, SystemInformation.MouseWheelScrollLines);
            int amount = Math.Max(1, lines * (ItemHeightPx / 2));
            int direction = e.Delta > 0 ? -1 : 1;

            SetScrollValue(scrollOffset + (amount * direction));
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            base.AutoScroll = false;
            scrollBar.BringToFront();
        }

        private void ConfigureScrollBar()
        {
            int contentHeight = GetContentHeight();

            maxScroll = Math.Max(0, contentHeight - ClientSize.Height);

            bool needsScrollBar = maxScroll > 0;
            scrollBar.Visible = needsScrollBar;

            if (!needsScrollBar)
            {
                scrollOffset = 0;
                scrollBar.Bounds = Rectangle.Empty;
                return;
            }

            scrollBar.SetBounds(
                ClientSize.Width - SystemInformation.VerticalScrollBarWidth,
                0,
                SystemInformation.VerticalScrollBarWidth,
                ClientSize.Height);

            int largeChange = Math.Max(1, ClientSize.Height - ItemHeightPx);
            largeChange = Math.Min(largeChange, Math.Max(1, maxScroll));

            scrollBar.Minimum = 0;
            scrollBar.SmallChange = Math.Max(1, ItemHeightPx / 2);
            scrollBar.LargeChange = largeChange;
            scrollBar.Maximum = maxScroll + largeChange - 1;

            if (scrollOffset > maxScroll)
                scrollOffset = maxScroll;

            if (scrollBar.Value != scrollOffset)
                scrollBar.Value = scrollOffset;
        }

        private int GetContentHeight()
        {
            if (items.Count == 0)
                return 0;

            return GetTargetTop(items.Count - 1) + ItemHeightPx + BottomPaddingPx;
        }

        private int GetItemWidth()
        {
            int width = ClientSize.Width;

            if (scrollBar.Visible)
                width -= scrollBar.Width;

            return Math.Max(40, width);
        }

        private void SetScrollValue(int value)
        {
            int clamped = Math.Max(0, Math.Min(value, maxScroll));

            if (scrollOffset == clamped)
                return;

            scrollOffset = clamped;

            if (scrollBar.Visible && scrollBar.Value != clamped)
                scrollBar.Value = clamped;

            ApplyScrollOffset();
        }

        private void ApplyScrollOffset()
        {
            int width = GetItemWidth();

            foreach (var item in items)
            {
                if (!targetTops.TryGetValue(item, out var targetTop))
                    continue;

                item.Width = width;
                item.Location = new Point(0, targetTop - scrollOffset);
            }

            scrollBar.BringToFront();
        }

        private int GetTargetTop(int index)
        {
            return index * (ItemHeightPx + ItemGapPx);
        }

        private void TickAnimation()
        {
            bool anyMoving = false;

            foreach (var item in items)
            {
                if (!targetTops.TryGetValue(item, out var targetTop))
                    continue;

                int targetDisplayTop = targetTop - scrollOffset;
                int nextY = StepTowards(item.Top, targetDisplayTop);

                if (nextY != item.Top || item.Left != 0)
                {
                    item.Location = new Point(0, nextY);
                    anyMoving = true;
                }
            }

            if (!anyMoving)
                timer.Stop();
        }

        private static int StepTowards(int current, int target)
        {
            int delta = target - current;

            if (delta == 0)
                return current;

            int step = Math.Max(1, Math.Abs(delta) / 4);

            if (Math.Abs(delta) <= step)
                return target;

            return current + Math.Sign(delta) * step;
        }
    }
}