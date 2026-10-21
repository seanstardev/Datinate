using Datinate.App.Shared.Media;
using Datinate.Shared;
using System.ComponentModel;

namespace Datinate.App.WinForms.View.UI.Media
{
    public sealed partial class FastEntryListUI : ListView
    {
        private readonly MediaEntryListSession entries = new();

        public event Action<MediaEntryInfo>? EntrySingleClicked;
        public event Action<MediaEntryInfo>? EntryDoubleClicked;

        private static readonly string[] PctTextCache = BuildPctTextCache();
        private string? pendingSelectedEntry;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? SelectedEntry
        {
            get
            {
                if (entries.VisibleCount == 0 || entries.EntryCount == 0)
                    return null;

                if (SelectedIndices.Count == 0)
                    return null;

                return entries.GetNameAtViewIndex(SelectedIndices[0]);
            }
            set
            {
                // NOTE: Allow callers (like Media2AssignUI init path) to set selection before handle exists.
                if (!IsHandleCreated)
                {
                    pendingSelectedEntry = value;
                    return;
                }

                pendingSelectedEntry = null;

                if (string.IsNullOrEmpty(value) || entries.VisibleCount == 0 || entries.EntryCount == 0)
                {
                    SelectedIndices.Clear();
                    return;
                }

                // NOTE: Ensure the underlying ListView knows the current virtual size before selecting.
                EnsureVirtualListSize();

                int backingIndex = entries.FindBackingIndex(value);
                if (backingIndex < 0)
                {
                    SelectedIndices.Clear();
                    return;
                }

                int virtualIndex = entries.FindViewIndex(backingIndex);

                if (virtualIndex < 0 || (uint)virtualIndex >= (uint)entries.VisibleCount)
                {
                    SelectedIndices.Clear();
                    return;
                }

                // NOTE: VirtualListSize can still be stale during some init sequences; keep it aligned.
                EnsureVirtualListSize();

                if ((uint)virtualIndex >= (uint)VirtualListSize)
                {
                    SelectedIndices.Clear();
                    return;
                }

                SelectedIndices.Clear();
                SelectedIndices.Add(virtualIndex);
            }
        }

        private readonly ColumnHeader nameCol;
        private readonly ColumnHeader scoreCol;

        private readonly Dictionary<int, ListViewItem> cache = new();
        private int cacheStart = -1;
        private int cacheEnd = -1;
        private bool sortModeLocked;

        private int hotVirtualIndex = -1;

        private Color lastBackColor;
        private SolidBrush? backBrush;
        private SolidBrush? hotBrush;

        private readonly Color defaultBackColour;

        private const int NamePadLeft = 6;
        private const int NamePadRight = 4;

        private const int ScorePadLeft = 2;
        private const int ScorePadRight = 6;

        private const int ScoreColumnWidthMin = 34;

        public const int HighConfidenceThresholdPct = MediaEntryListSession.HighConfidenceThresholdPct;

        private int lastAutoBackingIndex = -1;
        private int lastAutoScore = -1;
        private Font? boldFont;

        public FastEntryListUI()
        {
            entries.ViewOrderChanging += OnViewOrderChanging;

            View = System.Windows.Forms.View.Details;
            VirtualMode = true;
            FullRowSelect = true;
            MultiSelect = false;
            HideSelection = false;
            HeaderStyle = ColumnHeaderStyle.None;
            ShowItemToolTips = true;
            Activation = ItemActivation.TwoClick;

            nameCol = Columns.Add(string.Empty, 200, HorizontalAlignment.Left);
            scoreCol = Columns.Add(string.Empty, 0, HorizontalAlignment.Right);

            DoubleBuffered = true;

            Margin = new Padding(0);
            
            OwnerDraw = true;
            DrawItem += (_, e) => DrawItemInternal(e);
            DrawSubItem += (_, e) => DrawSubItemInternal(e);
            DrawColumnHeader += (_, e) => e.DrawDefault = true;

            lastBackColor = defaultBackColour = BackColor;

            SizeChanged += (_, __) => ResizeColumns();

            FontChanged += (_, __) =>
            {
                boldFont?.Dispose();
                boldFont = null;

                ClearCache();
                ResizeColumns();
                Invalidate();
            };

            BackColorChanged += (_, __) =>
            {
                DisposeBrushes();
                Invalidate();
            };
        }

        public void SetListInFocus(bool isInFocus)
        {
            BackColor = isInFocus ? defaultBackColour : SystemColors.Control;

            Scrollable = isInFocus;

            if (!isInFocus)
            {
                if (hotVirtualIndex != -1)
                    SetHotVirtualIndex(-1);

                Cursor = Cursors.Default;
            }
        }

        public void SetEntries(string[] newEntries, HashSet<string>? alreadyAssigned = null)
        {
            // NOTE: New dataset => never carry selection/pending from previous visit.
            pendingSelectedEntry = null;
            if (IsHandleCreated)
                SelectedIndices.Clear();

            entries.SetEntries(newEntries, alreadyAssigned);
            lastAutoBackingIndex = -1;
            lastAutoScore = -1;

            BeginUpdate();
            VirtualListSize = entries.VisibleCount;
            EndUpdate();

            ApplyPendingSelectionIfAny();

            ClearCache();
            ResizeColumns();
            Invalidate();
        }

        public void ToggleSort()
        {
            // NOTE: Any view rebuild must not preserve old selection/pending.
            pendingSelectedEntry = null;

            if (IsHandleCreated)
                SelectedIndices.Clear();

            entries.ToggleSort();
            RefreshVirtualView();
            ScrollToTop();
        }

        public void FilterByText(string searchText)
        {
            lastAutoBackingIndex = -1;
            lastAutoScore = -1;

            if (!IsHandleCreated)
            {
                entries.FilterByText(searchText);
                return;
            }

            BeginUpdate();

            try
            {
                entries.FilterByText(searchText);

                if (VirtualListSize != entries.VisibleCount)
                    VirtualListSize = entries.VisibleCount;
            }
            finally
            {
                EndUpdate();
            }

            ApplyPendingSelectionIfAny();
            ResizeColumns();

            if (VirtualListSize > 0)
                RedrawItems(0, VirtualListSize - 1, true);

            Invalidate();
        }

        private void OnViewOrderChanging(int newCount)
        {
            if (IsHandleCreated)
            {
                // NOTE: Clear selection while the old VirtualListSize and old viewOrder still agree.
                SelectedIndices.Clear();

                // NOTE: During the transition, expose only indexes that are valid in BOTH
                // the old and new viewOrder arrays.
                int transitionSize = Math.Min(entries.VisibleCount, newCount);

                if (VirtualListSize != transitionSize)
                    VirtualListSize = transitionSize;
            }

            ClearCache();
        }

        public void SetAlreadyAssigned(HashSet<string> alreadyAssigned)
        {
            entries.SetAlreadyAssigned(alreadyAssigned);
            ClearCache();
            Invalidate();
        }

        public int SetEntryToScoreAgainst(
            IReadOnlySet20<string> textsToMatch,
            bool excludeAlreadyAssigned)
        {
            // NOTE: New scoring session => never keep whatever was selected last time.
            pendingSelectedEntry = null;

            if (IsHandleCreated)
                SelectedIndices.Clear();

            int bestScore = entries.SetScoringReferences(textsToMatch, excludeAlreadyAssigned);
            RefreshVirtualView();
            ScrollToTop();

            lastAutoBackingIndex = -1;
            lastAutoScore = -1;
            ApplyAutomaticSelection();

            return bestScore;
        }

        private void ApplyAutomaticSelection()
        {
            if (entries.SingleHighConfidenceMatch is not MediaEntryInfo match)
                return;

            if (match.BackingIndex == lastAutoBackingIndex && match.ScorePct == lastAutoScore)
                return;

            lastAutoBackingIndex = match.BackingIndex;
            lastAutoScore = match.ScorePct;

            if (IsHandleCreated)
            {
                SelectedIndices.Clear();
                SelectedIndices.Add(entries.FindViewIndex(match.BackingIndex));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var hit = HitTest(e.Location);
            int idx = hit.Item?.Index ?? -1;

            if (idx != hotVirtualIndex)
                SetHotVirtualIndex(idx);

            Cursor = idx >= 0 ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (hotVirtualIndex != -1)
                SetHotVirtualIndex(-1);

            Cursor = Cursors.Default;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            const int WM_MOUSEWHEEL = 0x020A;
            const int WM_VSCROLL = 0x0115;

            if (m.Msg == WM_MOUSEWHEEL || m.Msg == WM_VSCROLL)
                RefreshHotFromCursor();
        }

        private void RefreshHotFromCursor()
        {
            if (!IsHandleCreated)
                return;

            var p = PointToClient(Cursor.Position);
            if (p.X < 0 || p.Y < 0 || p.X >= ClientSize.Width || p.Y >= ClientSize.Height)
            {
                if (hotVirtualIndex != -1)
                    SetHotVirtualIndex(-1);

                Cursor = Cursors.Default;
                return;
            }

            var hit = HitTest(p);
            int idx = hit.Item?.Index ?? -1;

            if (idx != hotVirtualIndex)
                SetHotVirtualIndex(idx);

            Cursor = idx >= 0 ? Cursors.Hand : Cursors.Default;
        }

        private void SetHotVirtualIndex(int newIndex)
        {
            if (newIndex == hotVirtualIndex)
                return;

            int old = hotVirtualIndex;
            hotVirtualIndex = newIndex;

            if (!IsHandleCreated)
                return;

            if (old >= 0 && old < VirtualListSize)
                RedrawItems(old, old, true);

            if (newIndex >= 0 && newIndex < VirtualListSize)
                RedrawItems(newIndex, newIndex, true);
        }

        private void DisposeBrushes()
        {
            backBrush?.Dispose();
            backBrush = null;

            hotBrush?.Dispose();
            hotBrush = null;

            lastBackColor = BackColor;
        }

        private void EnsureBrushes()
        {
            if (backBrush != null && hotBrush != null && lastBackColor == BackColor)
                return;

            DisposeBrushes();

            backBrush = new SolidBrush(BackColor);
            hotBrush = new SolidBrush(Color.FromArgb(28, SystemColors.Highlight));

            lastBackColor = BackColor;
        }

        private void DrawItemInternal(DrawListViewItemEventArgs e)
        {
        }

        private void DrawSubItemInternal(DrawListViewSubItemEventArgs e)
        {
            EnsureBrushes();

            bool selected = e.Item.Selected;
            bool hot = !selected && e.ItemIndex == hotVirtualIndex;

            if (selected)
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            else
                e.Graphics.FillRectangle(hot ? hotBrush! : backBrush!, e.Bounds);

            Color fore = selected
                ? SystemColors.HighlightText
                : (e.SubItem.ForeColor.IsEmpty ? ForeColor : e.SubItem.ForeColor);

            var flags =
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine |
                TextFormatFlags.VerticalCenter;

            bool isScoreCol = e.ColumnIndex == 1;

            if (!isScoreCol)
                flags |= TextFormatFlags.EndEllipsis;

            flags |= e.Header?.TextAlign switch
            {
                HorizontalAlignment.Right => TextFormatFlags.Right,
                HorizontalAlignment.Center => TextFormatFlags.HorizontalCenter,
                _ => TextFormatFlags.Left
            };

            var font = e.SubItem?.Font ?? e.Item.Font ?? Font;

            int padL = isScoreCol ? ScorePadLeft : NamePadLeft;
            int padR = isScoreCol ? ScorePadRight : NamePadRight;

            var r = e.Bounds;
            r.X += padL;
            r.Width -= (padL + padR);
            if (r.Width < 0) r.Width = 0;

            TextRenderer.DrawText(e.Graphics, e.SubItem?.Text, font, r, fore, flags);

            if (selected && Focused && e.ColumnIndex == 0)
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Item.Bounds, fore, SystemColors.Highlight);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style &= ~0x00100000; // WS_HSCROLL
                return cp;
            }
        }

        protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        {
            if ((uint)e.ItemIndex >= (uint)entries.VisibleCount)
            {
                e.Item = CreateEmptyVirtualItem();
                return;
            }

            if (!cache.TryGetValue(e.ItemIndex, out var item))
            {
                item = CreateItem(e.ItemIndex);
                cache[e.ItemIndex] = item;
            }

            e.Item = item;
        }
        private static ListViewItem CreateEmptyVirtualItem()
        {
            var item = new ListViewItem(string.Empty);
            item.SubItems.Add(string.Empty);
            return item;
        }
        protected override void OnCacheVirtualItems(CacheVirtualItemsEventArgs e)
        {
            if (entries.VisibleCount == 0)
            {
                ClearCache();
                return;
            }

            int startIndex = Math.Max(0, e.StartIndex);
            int endIndex = Math.Min(e.EndIndex, entries.VisibleCount - 1);

            if (startIndex > endIndex)
            {
                ClearCache();
                return;
            }

            if (startIndex >= cacheStart && endIndex <= cacheEnd)
                return;

            cache.Clear();
            cacheStart = startIndex;
            cacheEnd = endIndex;

            for (int i = startIndex; i <= endIndex; i++)
                cache[i] = CreateItem(i);
        }
        /// <summary>
        /// Do not remove - cause selected list item to go into media mode.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left || e.Clicks != 1)
                return;

            var hit = HitTest(e.Location);
            if (hit.Item == null)
                return;

            var info = GetEntryAtVirtualIndex(hit.Item.Index);
            if (info.HasValue)
                EntrySingleClicked?.Invoke(info.Value);
        }
        private MediaEntryInfo? GetEntryAtVirtualIndex(int virtualIndex)
            => entries.GetCachedEntryAtViewIndex(virtualIndex);
        private ListViewItem CreateItem(int virtualIndex)
        {
            if (!entries.TryGetEntryAtViewIndex(virtualIndex, out var entry))
                return CreateEmptyVirtualItem();

            string name = entry.Name;
            bool isAssigned = entry.IsAlreadyAssigned;
            int pct = entry.ScorePct;
            string pctText = (uint)pct <= 100u ? PctTextCache[pct] : string.Empty;

            var item = new ListViewItem(name);
            item.UseItemStyleForSubItems = false;
            item.ToolTipText = name;
            item.SubItems.Add(pctText);

            if (pct == 100)
                item.Font = GetBoldFont();

            if (isAssigned)
            {
                item.ForeColor = Color.SlateGray;
                item.SubItems[0].ForeColor = Color.SlateGray;
                item.SubItems[1].ForeColor = Color.SlateGray;
            }
            else
            {
                item.ForeColor = ForeColor;
                item.SubItems[0].ForeColor = ForeColor;
                item.SubItems[1].ForeColor = Color.DimGray;
            }

            return item;
        }

        public MediaEntryInfo? GetSingleHighConfidenceMatch() => entries.SingleHighConfidenceMatch;

        public IReadOnlyList<MediaEntryInfo> GetAllHighConfidenceMatches() => entries.HighConfidenceMatches;

        private void ResizeColumns()
        {
            int w = DisplayRectangle.Width;
            if (w <= 0)
                w = ClientSize.Width;

            if (w <= 0)
                return;

            int scoreW = CalcScoreColumnWidth();

            int usable = w - 1;
            if (usable < 0) usable = 0;

            if (scoreW > usable)
                scoreW = usable;

            int nameW = usable - scoreW;
            if (nameW < 0) nameW = 0;

            scoreCol.Width = scoreW;
            nameCol.Width = nameW;

            HideHorizontalScrollBar();
        }

        private int CalcScoreColumnWidth()
        {
            var flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

            int w1 = TextRenderer.MeasureText("100%", Font, Size.Empty, flags).Width;
            int w2 = TextRenderer.MeasureText("100%", GetBoldFont(), Size.Empty, flags).Width;

            int measured = w1 > w2 ? w1 : w2;

            int w = measured + ScorePadLeft + ScorePadRight;
            if (w < ScoreColumnWidthMin) w = ScoreColumnWidthMin;
            return w;
        }

        private void ClearCache()
        {
            cache.Clear();
            cacheStart = -1;
            cacheEnd = -1;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            HideHorizontalScrollBar();

            EnsureVirtualListSize();
            ApplyPendingSelectionIfAny();
        }

        private void HideHorizontalScrollBar()
        {
            if (!IsHandleCreated)
                return;

            Native.ShowScrollBar(Handle, Native.SB_HORZ, false);

            nint style = Native.GetWindowStyle(Handle);
            if ((style & Native.WS_HSCROLL) != 0)
                Native.SetWindowStyle(Handle, style & ~Native.WS_HSCROLL);
        }

        private static class Native
        {
            public const int GWL_STYLE = -16;
            public const int WS_HSCROLL = 0x00100000;

            public const int SB_HORZ = 0;

            [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
            private static extern nint GetWindowLongPtr64(nint hWnd, int nIndex);

            [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
            private static extern nint GetWindowLong32(nint hWnd, int nIndex);

            [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
            private static extern nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

            [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
            private static extern nint SetWindowLong32(nint hWnd, int nIndex, nint dwNewLong);

            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            public static extern bool ShowScrollBar(nint hWnd, int wBar, bool bShow);

            public static nint GetWindowStyle(nint hWnd) =>
                IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, GWL_STYLE) : GetWindowLong32(hWnd, GWL_STYLE);

            public static void SetWindowStyle(nint hWnd, nint style)
            {
                if (IntPtr.Size == 8)
                    SetWindowLongPtr64(hWnd, GWL_STYLE, style);
                else
                    SetWindowLong32(hWnd, GWL_STYLE, style);
            }
        }

        private void RefreshVirtualView()
        {
            ClearCache();

            BeginUpdate();
            VirtualListSize = entries.VisibleCount;
            EndUpdate();
            ApplyPendingSelectionIfAny();

            ResizeColumns();

            if (IsHandleCreated && VirtualListSize > 0)
                RedrawItems(0, VirtualListSize - 1, true);

            Invalidate();
        }

        private Font GetBoldFont()
        {
            var f = Font;
            if ((f.Style & FontStyle.Bold) != 0)
                return f;

            var wantStyle = f.Style | FontStyle.Bold;

            if (boldFont == null ||
                !string.Equals(boldFont.Name, f.Name, StringComparison.Ordinal) ||
                boldFont.SizeInPoints != f.SizeInPoints ||
                boldFont.Style != wantStyle)
            {
                boldFont = new Font(f, wantStyle);
            }

            return boldFont;
        }

        private void ScrollToTop()
        {
            if (!IsHandleCreated)
                return;

            if (VirtualListSize <= 0)
                return;

            EnsureVisible(0);
        }

        private static string[] BuildPctTextCache()
        {
            var arr = new string[101];
            for (int i = 0; i <= 100; i++)
                arr[i] = i.ToString() + "%";
            return arr;
        }
        private void EnsureVirtualListSize()
        {
            if (!IsHandleCreated)
                return;

            int want = entries.VisibleCount;
            if (VirtualListSize == want)
                return;

            BeginUpdate();
            VirtualListSize = want;
            EndUpdate();
        }

        private void ApplyPendingSelectionIfAny()
        {
            if (!IsHandleCreated)
                return;

            if (string.IsNullOrEmpty(pendingSelectedEntry))
                return;

            // NOTE: Don't consume pending until the list is actually ready.
            if (entries.EntryCount == 0 || entries.VisibleCount == 0)
                return;

            var s = pendingSelectedEntry;
            pendingSelectedEntry = null;

            _ = TrySelectEntry(s);
        }

        public bool TrySelectEntry(string? value)
        {
            if (string.IsNullOrEmpty(value) || entries.VisibleCount == 0 || entries.EntryCount == 0)
            {
                pendingSelectedEntry = null;
                SelectedIndices.Clear();
                return false;
            }

            int backingIndex = entries.FindBackingIndex(value);
            if (backingIndex < 0)
            {
                pendingSelectedEntry = null;
                SelectedIndices.Clear();
                return false;
            }

            // NOTE: If handle doesn't exist yet, queue it.
            if (!IsHandleCreated)
            {
                pendingSelectedEntry = value;
                return true;
            }

            pendingSelectedEntry = null;

            EnsureVirtualListSize();

            int virtualIndex = entries.FindViewIndex(backingIndex);

            if ((uint)virtualIndex >= (uint)entries.VisibleCount)
            {
                SelectedIndices.Clear();
                return false;
            }

            EnsureVirtualListSize();

            if ((uint)virtualIndex >= (uint)VirtualListSize)
            {
                SelectedIndices.Clear();
                return false;
            }

            SelectedIndices.Clear();
            SelectedIndices.Add(virtualIndex);
            return true;
        }

    }
}
