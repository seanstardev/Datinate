using com.RADIO.Datinate.RMVC.Shared;
using System.ComponentModel;

namespace datinate.app
{
    public sealed partial class FastEntryListUI : ListView
    {

        public event Action<EntryInfo>? EntrySingleClicked;
        public event Action<EntryInfo>? EntryDoubleClicked;

        private string[] entriesFlaglessUpper = Array.Empty<string>();

        private string lastReferenceFlaglessUpper = string.Empty;

        private static readonly string[] PctTextCache = BuildPctTextCache();
        private readonly Dictionary<string, int> backingIndexByFull = new(StringComparer.Ordinal);
        private string? pendingSelectedEntry;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? SelectedEntry
        {
            get
            {
                if (viewOrder.Length == 0 || entriesFull.Length == 0)
                    return null;

                if (SelectedIndices.Count == 0)
                    return null;

                int virtualIndex = SelectedIndices[0];
                if ((uint)virtualIndex >= (uint)viewOrder.Length)
                    return null;

                int backingIndex = viewOrder[virtualIndex];
                if ((uint)backingIndex >= (uint)entriesFull.Length)
                    return null;

                return entriesFull[backingIndex];
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

                if (string.IsNullOrEmpty(value) || viewOrder.Length == 0 || entriesFull.Length == 0)
                {
                    SelectedIndices.Clear();
                    return;
                }

                // NOTE: Ensure the underlying ListView knows the current virtual size before selecting.
                EnsureVirtualListSize();

                int backingIndex = FindBackingIndexForSelection(value);
                if (backingIndex < 0)
                {
                    SelectedIndices.Clear();
                    return;
                }

                int virtualIndex = -1;
                for (int i = 0; i < viewOrder.Length; i++)
                {
                    if (viewOrder[i] == backingIndex)
                    {
                        virtualIndex = i;
                        break;
                    }
                }

                if (virtualIndex < 0 || (uint)virtualIndex >= (uint)viewOrder.Length)
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

        public readonly record struct EntryInfo(int BackingIndex, string Name, int ScorePct, bool IsAlreadyAssigned);

        private string[] entriesFull = Array.Empty<string>();
        private string[] entriesFlagless = Array.Empty<string>();
        private readonly Dictionary<string, string> flaglessByFull = new(StringComparer.Ordinal);

        private bool[] assigned = Array.Empty<bool>();

        private int[] viewOrder = Array.Empty<int>();
        private int[] scorePct = Array.Empty<int>();
        private int[] scratch = Array.Empty<int>();

        private readonly ColumnHeader nameCol;
        private readonly ColumnHeader scoreCol;

        private readonly Dictionary<int, ListViewItem> cache = new();
        private int cacheStart = -1;
        private int cacheEnd = -1;
        private bool sortModeLocked;

        private bool alphaSort = false;
        private string lastReferenceFlagless = string.Empty;
        private int scoreGeneration = 1;
        private int[] scoreGen = Array.Empty<int>();

        private string lastFilterFull = string.Empty;
        private string lastFilterFlagless = string.Empty;

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

        public const int HighConfidenceThresholdPct  = 100;

        private int lastAutoBackingIndex = -1;
        private int lastAutoScore = -1;
        private Font? boldFont;

        public FastEntryListUI()
        {
            View = View.Details;
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

            entriesFull = newEntries ?? Array.Empty<string>();
            alphaSort = false;

            entriesFlagless = new string[entriesFull.Length];
            entriesFlaglessUpper = new string[entriesFull.Length];

            flaglessByFull.Clear();
            backingIndexByFull.Clear();

            for (int i = 0; i < entriesFull.Length; i++)
            {
                var full = entriesFull[i] ?? string.Empty;
                var flagless = DatinateHelper.GetFlaglessName(full) ?? string.Empty;

                entriesFlagless[i] = flagless;
                entriesFlaglessUpper[i] = flagless.ToUpperInvariant();

                flaglessByFull[full] = flagless;

                if (!backingIndexByFull.ContainsKey(full))
                    backingIndexByFull.Add(full, i);
            }

            assigned = new bool[entriesFull.Length];
            if (alreadyAssigned != null && alreadyAssigned.Count > 0)
            {
                for (int i = 0; i < entriesFull.Length; i++)
                    assigned[i] = alreadyAssigned.Contains(entriesFull[i]);
            }

            viewOrder = new int[entriesFull.Length];
            scorePct = new int[entriesFull.Length];
            scoreGen = new int[entriesFull.Length];

            scoreGeneration = 1;

            for (int i = 0; i < entriesFull.Length; i++)
            {
                viewOrder[i] = i;
                scorePct[i] = -1;
                scoreGen[i] = 0;
            }

            EnsureScratchCapacity(entriesFull.Length);

            lastReferenceFlagless = string.Empty;
            lastReferenceFlaglessUpper = string.Empty;

            lastFilterFull = string.Empty;
            lastFilterFlagless = string.Empty;

            lastAutoBackingIndex = -1;
            lastAutoScore = -1;

            singleHighConfidenceMatch = null;
            highConfidenceMatches = Array.Empty<EntryInfo>();

            BeginUpdate();
            VirtualListSize = viewOrder.Length;
            EndUpdate();

            ApplyPendingSelectionIfAny();

            ClearCache();
            ResizeColumns();
            Invalidate();
        }

        public void ToggleSort()
        {
            SetSortModeShowAll(!alphaSort);
            ScrollToTop();
        }

        public void FilterByText(string searchText)
        {
            alphaSort = true;

            string filterFull = searchText.Trim();
            string filterFlagless = DatinateHelper.GetFlaglessName(filterFull) ?? string.Empty;

            lastFilterFull = filterFull;
            lastFilterFlagless = filterFlagless;

            EnsureScratchCapacity(entriesFull.Length);

            int count = 0;

            if (filterFull.Length == 0 && filterFlagless.Length == 0)
            {
                for (int i = 0; i < entriesFull.Length; i++)
                    scratch[count++] = i;
            }
            else
            {
                for (int i = 0; i < entriesFull.Length; i++)
                {
                    if (filterFull.Length > 0 &&
                        entriesFull[i].IndexOf(filterFull, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        scratch[count++] = i;
                        continue;
                    }

                    if (filterFlagless.Length > 0 &&
                        entriesFlagless[i].IndexOf(filterFlagless, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        scratch[count++] = i;
                    }
                }
            }

            if (count == 0)
            {
                viewOrder = Array.Empty<int>();

                lastAutoBackingIndex = -1;
                lastAutoScore = -1;

                if (IsHandleCreated)
                    SelectedIndices.Clear();

                RefreshVirtualView();
                return;
            }

            viewOrder = new int[count];
            Array.Copy(scratch, viewOrder, count);

            Array.Sort(viewOrder, CompareAlpha);

            lastAutoBackingIndex = -1;
            lastAutoScore = -1;

            if (IsHandleCreated)
                SelectedIndices.Clear();

            RefreshVirtualView();
        }

        public void SetAlreadyAssigned(HashSet<string> alreadyAssigned)
        {
            Array.Clear(assigned, 0, assigned.Length);

            if (alreadyAssigned.Count > 0)
            {
                for (int i = 0; i < entriesFull.Length; i++)
                    assigned[i] = alreadyAssigned.Contains(entriesFull[i]);
            }

            ClearCache();
            Invalidate();
        }
        public int SetEntryToScoreAgainst(string textToMatch)
        {
            alphaSort = false;

            // NOTE: New scoring session => never keep whatever was selected last time.
            pendingSelectedEntry = null;
            if (IsHandleCreated)
                SelectedIndices.Clear();

            textToMatch ??= string.Empty;

            string refFull = textToMatch.Trim();
            string refFlagless = DatinateHelper.GetFlaglessName(refFull) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(refFlagless))
                refFlagless = refFull;

            if (string.IsNullOrWhiteSpace(refFlagless))
            {
                SetReferenceFlagless(string.Empty, forceReset: true);
                singleHighConfidenceMatch = null; // NOTE: clear stale.
                highConfidenceMatches = Array.Empty<EntryInfo>();
                RefreshVirtualView();
                ScrollToTop();
                return 0;
            }

            SetReferenceFlagless(refFlagless, forceReset: false);

            int bestScore = 0;

            for (int i = 0; i < entriesFull.Length; i++)
            {
                int s = GetScorePct(i);
                if (s > bestScore)
                    bestScore = s;
            }

            SetSortModeShowAll(alpha: false);
            ScrollToTop();

            lastAutoBackingIndex = -1;
            lastAutoScore = -1;

            IdentifyHighConfidenceMatches();

            return bestScore;
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

        private void SetSortModeShowAll(bool alpha)
        {
            alphaSort = alpha;

            // NOTE: Any view rebuild must not preserve old selection/pending.
            pendingSelectedEntry = null;

            if (IsHandleCreated)
                SelectedIndices.Clear();

            lastFilterFull = string.Empty;
            lastFilterFlagless = string.Empty;

            int n = entriesFull.Length;

            if (viewOrder.Length != n)
                viewOrder = new int[n];

            for (int i = 0; i < n; i++)
                viewOrder[i] = i;

            if (alphaSort || string.IsNullOrWhiteSpace(lastReferenceFlagless))
            {
                Array.Sort(viewOrder, CompareAlpha);
            }
            else
            {
                EnsureScoresForViewOrder();
                Array.Sort(viewOrder, CompareScoreDescThenAlpha);
            }

            RefreshVirtualView();
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
            if (!cache.TryGetValue(e.ItemIndex, out var item))
            {
                item = CreateItem(e.ItemIndex);
                cache[e.ItemIndex] = item;
            }

            e.Item = item;
        }

        protected override void OnCacheVirtualItems(CacheVirtualItemsEventArgs e)
        {
            if (e.StartIndex >= cacheStart && e.EndIndex <= cacheEnd)
                return;

            cache.Clear();
            cacheStart = e.StartIndex;
            cacheEnd = e.EndIndex;

            for (int i = e.StartIndex; i <= e.EndIndex; i++)
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
        private EntryInfo? GetEntryAtVirtualIndex(int virtualIndex)
        {
            if ((uint)virtualIndex >= (uint)viewOrder.Length)
                return null;

            int backingIndex = viewOrder[virtualIndex];
            return new EntryInfo(
                backingIndex,
                entriesFull[backingIndex],
                scorePct[backingIndex],
                assigned[backingIndex]);
        }
        private ListViewItem CreateItem(int virtualIndex)
        {
            int backingIndex = viewOrder[virtualIndex];

            string name = entriesFull[backingIndex];
            bool isAssigned = assigned[backingIndex];

            int pct = GetScorePct(backingIndex);
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

        private int CompareAlpha(int a, int b)
        {
            var sa = entriesFull[a];
            var sb = entriesFull[b];

            int c = StringComparer.OrdinalIgnoreCase.Compare(sa, sb);
            if (c != 0) return c;

            return StringComparer.Ordinal.Compare(sa, sb);
        }

        private int CompareScoreDescThenAlpha(int a, int b)
        {
            int pa = scorePct[a];
            int pb = scorePct[b];

            int c = pb.CompareTo(pa);
            if (c != 0) return c;

            return CompareAlpha(a, b);
        }
        private void IdentifyHighConfidenceMatches()
        {
            singleHighConfidenceMatch = null;
            highConfidenceMatches = Array.Empty<EntryInfo>();

            if (viewOrder.Length == 0)
            {
                lastAutoBackingIndex = -1;
                lastAutoScore = -1;
                return;
            }

            int bestVirtualIndex = -1;
            int bestBackingIndex = -1;
            int bestScore = -1;

            int countHigh = 0;

            for (int i = 0; i < viewOrder.Length; i++)
            {
                int backingIndex = viewOrder[i];
                int score = GetScorePct(backingIndex);

                if (score < HighConfidenceThresholdPct)
                    continue;

                if (assigned[backingIndex])
                    continue;

                if (countHigh == 0)
                {
                    bestVirtualIndex = i;
                    bestBackingIndex = backingIndex;
                    bestScore = score;
                }

                countHigh++;
            }

            if (countHigh == 0)
            {
                lastAutoBackingIndex = -1;
                lastAutoScore = -1;
                return;
            }

            var matches = new EntryInfo[countHigh];
            int w = 0;

            for (int i = 0; i < viewOrder.Length; i++)
            {
                int backingIndex = viewOrder[i];
                int score = GetScorePct(backingIndex);

                if (score < HighConfidenceThresholdPct)
                    continue;

                if (assigned[backingIndex])
                    continue;

                matches[w++] = new EntryInfo(
                    backingIndex,
                    entriesFull[backingIndex],
                    score,
                    assigned[backingIndex]);
            }

            highConfidenceMatches = matches;

            if (countHigh != 1)
            {
                lastAutoBackingIndex = -1;
                lastAutoScore = -1;
                return;
            }

            if (bestBackingIndex == lastAutoBackingIndex && bestScore == lastAutoScore)
            {
                singleHighConfidenceMatch = matches[0];
                return;
            }

            lastAutoBackingIndex = bestBackingIndex;
            lastAutoScore = bestScore;

            if (IsHandleCreated)
            {
                SelectedIndices.Clear();
                SelectedIndices.Add(bestVirtualIndex);
            }

            singleHighConfidenceMatch = matches[0];
        }


        private EntryInfo? singleHighConfidenceMatch = null;
        private EntryInfo[] highConfidenceMatches = Array.Empty<EntryInfo>();

        public EntryInfo? GetSingleHighConfidenceMatch()
        {
            return singleHighConfidenceMatch;
        }

        public IReadOnlyList<EntryInfo> GetAllHighConfidenceMatches()
        {
            return highConfidenceMatches;
        }

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

        private void EnsureScratchCapacity(int required)
        {
            if (scratch.Length >= required)
                return;

            scratch = new int[required];
        }
        private static int ComputePercentUpper(string sUpper, string tUpper)
        {
            int maxLen = sUpper.Length >= tUpper.Length ? sUpper.Length : tUpper.Length;
            if (maxLen == 0)
                return 100;

            int dist = ComputeDistanceUpper(sUpper, tUpper);
            int pct = (int)Math.Round(100.0 * (1.0 - (double)dist / maxLen));

            if (pct < 0) return 0;
            if (pct > 100) return 100;
            return pct;
        }

        private static int ComputeDistanceUpper(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;

            if (n == 0) return m;
            if (m == 0) return n;

            if (m > n)
            {
                var tmp = s; s = t; t = tmp;
                n = s.Length;
                m = t.Length;
            }

            int mPlus1 = m + 1;

            Span<int> prev = mPlus1 <= 512 ? stackalloc int[mPlus1] : new int[mPlus1];
            Span<int> curr = mPlus1 <= 512 ? stackalloc int[mPlus1] : new int[mPlus1];

            for (int j = 0; j <= m; j++)
                prev[j] = j;

            for (int i = 1; i <= n; i++)
            {
                curr[0] = i;

                char sc = s[i - 1];

                for (int j = 1; j <= m; j++)
                {
                    char tc = t[j - 1];
                    int cost = sc == tc ? 0 : 1;

                    int del = prev[j] + 1;
                    int ins = curr[j - 1] + 1;
                    int sub = prev[j - 1] + cost;

                    int v = del < ins ? del : ins;
                    if (sub < v) v = sub;

                    curr[j] = v;
                }

                var swap = prev;
                prev = curr;
                curr = swap;
            }

            return prev[m];
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
        private void SetReferenceFlagless(string newReferenceFlagless, bool forceReset)
        {
            newReferenceFlagless ??= string.Empty;

            if (!forceReset && string.Equals(lastReferenceFlagless, newReferenceFlagless, StringComparison.Ordinal))
                return;

            lastReferenceFlagless = newReferenceFlagless;
            lastReferenceFlaglessUpper = newReferenceFlagless.Length == 0 ? string.Empty : newReferenceFlagless.ToUpperInvariant();

            scoreGeneration++;
            if (scoreGeneration == int.MaxValue)
            {
                Array.Clear(scoreGen, 0, scoreGen.Length);
                scoreGeneration = 1;
            }

            lastAutoBackingIndex = -1;
            lastAutoScore = -1;

            if (IsHandleCreated)
                SelectedIndices.Clear();

            ClearCache();
        }
        private int GetScorePct(int backingIndex)
        {
            if (string.IsNullOrWhiteSpace(lastReferenceFlaglessUpper))
                return -1;

            if (scoreGen[backingIndex] != scoreGeneration)
            {
                scorePct[backingIndex] = ComputePercentUpper(lastReferenceFlaglessUpper, entriesFlaglessUpper[backingIndex]);
                scoreGen[backingIndex] = scoreGeneration;
            }

            return scorePct[backingIndex];
        }
        private void EnsureScoresForViewOrder()
        {
            if (string.IsNullOrWhiteSpace(lastReferenceFlaglessUpper))
                return;

            for (int i = 0; i < viewOrder.Length; i++)
                _ = GetScorePct(viewOrder[i]);
        }

        private void RefreshVirtualView()
        {
            ClearCache();

            BeginUpdate();
            VirtualListSize = viewOrder.Length;
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

            int want = viewOrder.Length;
            if (VirtualListSize == want)
                return;

            BeginUpdate();
            VirtualListSize = want;
            EndUpdate();
        }

        private int FindBackingIndexForSelection(string value)
        {
            // NOTE: Fast exact match.
            if (backingIndexByFull.TryGetValue(value, out int idx))
                return idx;

            // NOTE: Trim fallback (common when values come from persisted text)
            var trimmed = value.Trim();
            if (trimmed.Length != value.Length && backingIndexByFull.TryGetValue(trimmed, out idx))
                return idx;

            // NOTE: Ignore-case fallback
            for (int i = 0; i < entriesFull.Length; i++)
            {
                if (string.Equals(entriesFull[i], value, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            // NOTE: Flagless fallback (only if persisted value might be flagless)
            var wantFlaglessUpper = DatinateHelper.GetFlaglessName(value) ?? value;
            wantFlaglessUpper = wantFlaglessUpper.ToUpperInvariant();

            for (int i = 0; i < entriesFlaglessUpper.Length; i++)
            {
                if (entriesFlaglessUpper[i] == wantFlaglessUpper)
                    return i;
            }

            return -1;
        }

        private void ApplyPendingSelectionIfAny()
        {
            if (!IsHandleCreated)
                return;

            if (string.IsNullOrEmpty(pendingSelectedEntry))
                return;

            // NOTE: Don't consume pending until the list is actually ready.
            if (entriesFull.Length == 0 || viewOrder.Length == 0)
                return;

            var s = pendingSelectedEntry;
            pendingSelectedEntry = null;

            _ = TrySelectEntry(s);
        }

        public bool TrySelectEntry(string? value)
        {
            if (string.IsNullOrEmpty(value) || viewOrder.Length == 0 || entriesFull.Length == 0)
            {
                pendingSelectedEntry = null;
                SelectedIndices.Clear();
                return false;
            }

            int backingIndex = FindBackingIndexForSelection(value);
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

            int virtualIndex = -1;
            for (int i = 0; i < viewOrder.Length; i++)
            {
                if (viewOrder[i] == backingIndex)
                {
                    virtualIndex = i;
                    break;
                }
            }

            if ((uint)virtualIndex >= (uint)viewOrder.Length)
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
