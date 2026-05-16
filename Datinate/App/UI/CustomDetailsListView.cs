using System.ComponentModel;
using System.Runtime.InteropServices;
using com.RADIO.Datinate.RMVC.Shared;

namespace datinate.app
{
    public class CustomDetailsListView : ListView
    {
        private const int LVM_FIRST = 0x1000;
        private const int LVM_GETHEADER = LVM_FIRST + 31;
        private const int WM_SETFONT = 0x0030;

        private bool boldHeader = true;
        private int headerExtraWidthPx = 28;
        private bool autoBestFitOnColumnTextChange = true;
        private IntPtr headerFontHandle = IntPtr.Zero;

        [DefaultValue(true)]
        public bool BoldHeader
        {
            get => boldHeader;
            set
            {
                if (boldHeader == value)
                    return;

                boldHeader = value;
                ApplyHeaderFont();

                if (Columns.Count > 0)
                    ResizeColumnsToBestFit();
            }
        }

        [DefaultValue(28)]
        public int HeaderExtraWidthPx
        {
            get => headerExtraWidthPx;
            set
            {
                int newValue = Math.Max(0, value);
                if (headerExtraWidthPx == newValue)
                    return;

                headerExtraWidthPx = newValue;

                if (Columns.Count > 0)
                    ResizeColumnsToBestFit();
            }
        }

        [DefaultValue(true)]
        public bool AutoBestFitOnColumnTextChange
        {
            get => autoBestFitOnColumnTextChange;
            set => autoBestFitOnColumnTextChange = value;
        }

        public CustomDetailsListView()
        {
            View = View.Details;
        }

        public void AddColumns(ColumnHeaderVO.NameEnum[] columns)
        {
            BeginUpdate();
            try
            {
                Items.Clear();
                Columns.Clear();

                for (int i = 0; i < columns.Length; i++)
                {
                    ColumnHeader column = new ColumnHeader();
                    column.Name = GetColumnKey(columns[i]);
                    column.Text = columns[i].ToString();
                    column.Tag = new ColumnHeaderVO(columns[i]);
                    Columns.Add(column);
                }

                ResizeColumnsToHeaders();
            }
            finally
            {
                EndUpdate();
            }
        }

        public int GetColumnIndex(ColumnHeaderVO.NameEnum name)
        {
            string key = GetColumnKey(name);

            for (int i = 0; i < Columns.Count; i++)
            {
                ColumnHeader column = Columns[i];

                if (string.Equals(column.Name, key, StringComparison.Ordinal))
                    return i;

                if (string.Equals(column.Text, key, StringComparison.Ordinal))
                    return i;

                string firstToken = GetFirstToken(column.Text);
                if (string.Equals(firstToken, key, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        public ColumnHeader? GetColumn(ColumnHeaderVO.NameEnum name)
        {
            int index = GetColumnIndex(name);
            if (index < 0)
                return null;

            return Columns[index];
        }

        public ListViewItem.ListViewSubItem? GetRowSubItem(ColumnHeaderVO.NameEnum columnName, ListViewItem row)
        {
            int columnIndex = GetColumnIndex(columnName);
            if (columnIndex < 0)
                return null;

            EnsureSubItemCount(row, columnIndex + 1);
            return row.SubItems[columnIndex];
        }

        public void SetRowSubItemText(ListViewItem row, ColumnHeaderVO.NameEnum columnName, string text)
        {
            ListViewItem.ListViewSubItem? subItem = GetRowSubItem(columnName, row);
            if (subItem == null)
                return;

            subItem.Text = text ?? string.Empty;
        }

        public void SetColumnText(ColumnHeaderVO.NameEnum columnName, string text)
        {
            ColumnHeader? column = GetColumn(columnName);
            if (column == null)
                return;

            column.Text = text ?? string.Empty;

            if (autoBestFitOnColumnTextChange)
                ResizeColumnsToBestFit();
        }

        public void AddRow(RowVO[] rows, object tag)
        {
            if (Columns.Count == 0)
                return;

            string[] values = new string[Columns.Count];

            for (int i = 0; i < values.Length; i++)
                values[i] = string.Empty;

            for (int i = 0; i < rows.Length; i++)
            {
                int columnIndex = GetColumnIndex(rows[i].columnName);
                if (columnIndex < 0)
                    continue;

                values[columnIndex] = rows[i].value ?? string.Empty;
            }

            ListViewItem item = new ListViewItem(values[0]);

            for (int i = 1; i < values.Length; i++)
                item.SubItems.Add(values[i]);

            item.Tag = tag;
            Items.Add(item);
        }

        public void ResizeColumnsToHeaders()
        {
            if (Columns.Count == 0)
                return;

            BeginUpdate();
            try
            {
                for (int i = 0; i < Columns.Count; i++)
                    Columns[i].Width = MeasureHeaderWidth(Columns[i]);
            }
            finally
            {
                EndUpdate();
            }
        }

        public void ResizeColumnsToBestFit()
        {
            if (Columns.Count == 0)
                return;

            if (Items.Count == 0)
            {
                ResizeColumnsToHeaders();
                return;
            }

            int[] headerWidths = new int[Columns.Count];
            int[] contentWidths = new int[Columns.Count];

            BeginUpdate();
            try
            {
                for (int i = 0; i < Columns.Count; i++)
                    headerWidths[i] = MeasureHeaderWidth(Columns[i]);

                for (int i = 0; i < Columns.Count; i++)
                {
                    AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                    contentWidths[i] = Columns[i].Width;
                }

                for (int i = 0; i < Columns.Count; i++)
                    Columns[i].Width = Math.Max(headerWidths[i], contentWidths[i]);
            }
            finally
            {
                EndUpdate();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyHeaderFont();

            if (Columns.Count > 0)
                ResizeColumnsToBestFit();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            ReleaseHeaderFontHandle();
            base.OnHandleDestroyed(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            ApplyHeaderFont();

            if (Columns.Count > 0)
                ResizeColumnsToBestFit();
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseHeaderFontHandle();
            base.Dispose(disposing);
        }

        private int MeasureHeaderWidth(ColumnHeader column)
        {
            using Font measureFont = new Font(Font, GetHeaderFontStyle());

            Size textSize = TextRenderer.MeasureText(
                column.Text ?? string.Empty,
                measureFont,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            return Math.Max(1, textSize.Width + headerExtraWidthPx);
        }

        private FontStyle GetHeaderFontStyle()
        {
            return boldHeader
                ? Font.Style | FontStyle.Bold
                : Font.Style;
        }

        private void ApplyHeaderFont()
        {
            if (!IsHandleCreated)
                return;

            IntPtr headerHandle = SendMessage(Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            if (headerHandle == IntPtr.Zero)
                return;

            using Font fontToApply = new Font(Font, GetHeaderFontStyle());
            IntPtr newFontHandle = fontToApply.ToHfont();

            IntPtr oldFontHandle = headerFontHandle;
            headerFontHandle = newFontHandle;

            SendMessage(headerHandle, WM_SETFONT, headerFontHandle, (IntPtr)1);

            if (oldFontHandle != IntPtr.Zero)
                DeleteObject(oldFontHandle);
        }

        private void ReleaseHeaderFontHandle()
        {
            if (headerFontHandle == IntPtr.Zero)
                return;

            DeleteObject(headerFontHandle);
            headerFontHandle = IntPtr.Zero;
        }

        private static void EnsureSubItemCount(ListViewItem item, int count)
        {
            while (item.SubItems.Count < count)
                item.SubItems.Add(string.Empty);
        }

        private static string GetColumnKey(ColumnHeaderVO.NameEnum name)
        {
            return name.ToString();
        }

        private static string GetFirstToken(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            int spaceIndex = text.IndexOf(' ');
            if (spaceIndex < 0)
                return text;

            return text.Substring(0, spaceIndex);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}