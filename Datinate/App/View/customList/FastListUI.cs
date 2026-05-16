using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace datinate.app
{
    public partial class FastListUI : ListView
    {
        private struct Row
        {
            public string Text;
            public Color Colour;
        }

        private ListViewItem?[] _itemCache = Array.Empty<ListViewItem?>();
        private Row[] _rows = Array.Empty<Row>();
        private Func<int, string>? _getText;
        private Func<int, Color>? _getColour;

        private bool _defaultConfigured;

        public event Action<string>? RowClicked;

        private static readonly ListViewItem EmptyVirtualItem = new ListViewItem(string.Empty);

        private const int LVM_FIRST = 0x1000;
        private const int LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
        private const int LVS_EX_DOUBLEBUFFER = 0x00010000;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public FastListUI()
        {
            InitializeComponent();
            EnsureConfigured();
        }

        public void ConfigureVirtualIndexAndTextColumns()
        {
            EnsureConfigured();
            FitIndexAndTextColumns();
        }

        public void ClearVirtualRows()
        {
            _rows = Array.Empty<Row>();
            _itemCache = Array.Empty<ListViewItem?>();
            _getText = null;
            _getColour = null;

            VirtualListSize = 0;
            Invalidate();
        }

        public void SetRows((string Text, Color Colour)[] rows)
        {
            EnsureConfigured();

            if (rows.Length == 0)
            {
                ClearVirtualRows();
                return;
            }

            var newRows = new Row[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                newRows[i] = new Row
                {
                    Text = rows[i].Text,
                    Colour = rows[i].Colour
                };
            }

            _rows = newRows;
            _getText = null;
            _getColour = null;

            _itemCache = new ListViewItem?[rows.Length];
            VirtualListSize = rows.Length;

            FitIndexAndTextColumns();
            Invalidate();
        }

        public void SetRowAccessors(int count, Func<int, string> getText, Func<int, Color> getColour)
        {
            EnsureConfigured();

            _rows = Array.Empty<Row>();
            _getText = getText;
            _getColour = getColour;

            _itemCache = count == 0 ? Array.Empty<ListViewItem?>() : new ListViewItem?[count];
            VirtualListSize = count;

            FitIndexAndTextColumns();
            Invalidate();
        }
        public void FitIndexAndTextColumns()
        {
            if (Columns.Count == 0)
                return;

            var w = ClientSize.Width;
            if (BorderStyle != BorderStyle.None)
                w -= 2;

            var itemHeight = TextRenderer.MeasureText("A", Font).Height + 1;
            var hasVScroll = VirtualListSize * itemHeight > ClientSize.Height;

            if (hasVScroll)
                w -= SystemInformation.VerticalScrollBarWidth;

            if (w < 50)
                w = 50;

            if (Columns.Count == 1)
            {
                Columns[0].Width = w;
                return;
            }

            var idxW = 60;
            if (idxW > w - 50)
                idxW = Math.Max(40, w / 4);

            Columns[0].Width = idxW;
            Columns[1].Width = Math.Max(50, w - idxW);
        }

        public void ResetScrollToTopAndPaint()
        {
            if (VirtualListSize > 0)
                EnsureVisible(0);

            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            EnsureConfigured();
            EnableNativeDoubleBuffer();
            FitIndexAndTextColumns();
        }

        protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        {
            var index = e.ItemIndex;

            if ((uint)index >= (uint)VirtualListSize)
            {
                e.Item = EmptyVirtualItem;
                return;
            }

            var cache = _itemCache;
            if ((uint)index < (uint)cache.Length)
            {
                var cached = cache[index];
                if (cached != null)
                {
                    e.Item = cached;
                    return;
                }
            }

            var text = GetTextAtIndex(index);
            var colour = _getColour != null
                ? _getColour(index)
                : ((uint)index < (uint)_rows.Length ? _rows[index].Colour : ForeColor);

            ListViewItem item;

            if (Columns.Count >= 2)
            {
                var idxText = string.Equals(text, "(none)", StringComparison.Ordinal) ? "0" : (index + 1).ToString();

                item = new ListViewItem(idxText);
                item.UseItemStyleForSubItems = false;

                item.SubItems[0].ForeColor = SystemColors.GrayText;

                var sub = item.SubItems.Add(text);
                sub.ForeColor = colour;
            }
            else
            {
                item = new ListViewItem(text);
                item.ForeColor = colour;
            }

            if ((uint)index < (uint)cache.Length)
                cache[index] = item;

            e.Item = item;
        }

        protected override void OnCacheVirtualItems(CacheVirtualItemsEventArgs e)
        {
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            FitIndexAndTextColumns();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            var item = GetItemAt(e.X, e.Y);
            if (item == null)
                return;

            var index = item.Index;
            if ((uint)index >= (uint)VirtualListSize)
                return;

            var text = GetTextAtIndex(index);
            if (text.Length == 0)
                return;

            RowClicked?.Invoke(text);
        }

        private string GetTextAtIndex(int index)
        {
            var gt = _getText;
            if (gt != null)
                return gt(index);

            var rows = _rows;
            return (uint)index < (uint)rows.Length ? rows[index].Text : string.Empty;
        }

        private void EnsureConfigured()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            if (!_defaultConfigured)
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
                _defaultConfigured = true;
            }

            UseCompatibleStateImageBehavior = false;
            View = View.Details;
            HeaderStyle = ColumnHeaderStyle.None;
            FullRowSelect = true;
            HideSelection = false;
            MultiSelect = false;
            VirtualMode = true;

            if (Columns.Count == 0)
            {
                Columns.Add("#", 40, HorizontalAlignment.Right);
                Columns.Add(string.Empty, 300, HorizontalAlignment.Left);
            }
        }

        private void EnableNativeDoubleBuffer()
        {
            if (!IsHandleCreated)
                return;

            SendMessage(Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, (IntPtr)LVS_EX_DOUBLEBUFFER, (IntPtr)LVS_EX_DOUBLEBUFFER);
        }
    }
}
