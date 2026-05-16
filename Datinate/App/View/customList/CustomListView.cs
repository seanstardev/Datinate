using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using datinate.shared;
using Datinate.Shared;
using static datinate.shared.DatFilterHelper;

namespace com.RADIO.Datinate.App.View.customList
{
    public partial class CustomListView : UserControl, ICustomListView
    {
        public event Action<DatVO?>? CreateDatClickEvt;
        public event Action? FormClosingEvt;
        public event Action? LoadExpressionsEvt;
        public event Action<DatFilter[]>? SaveExpressionsEvt;

        private (string Text, EXPRESSION_ACTION_ENUM Action)[] includedRows = Array.Empty<(string Text, EXPRESSION_ACTION_ENUM Action)>();
        private (string Text, EXPRESSION_ACTION_ENUM Action)[] excludedRows = Array.Empty<(string Text, EXPRESSION_ACTION_ENUM Action)>();


        private Flag[] categories = Array.Empty<Flag>();

        private List<DatFilter> expressions = new List<DatFilter>();
        private DatVO? sourceDatVO;
        private int totalNumberOfFiles;
        private string[] flagStrings = Array.Empty<string>();
        private DatGameVO[] completeGameArray = Array.Empty<DatGameVO>();

        private readonly Dictionary<string, string[]> fileFlagsCache =
            new Dictionary<string, string[]>(StringComparer.Ordinal);

        private readonly Dictionary<string, ulong> fileSizeCache =
            new Dictionary<string, ulong>(StringComparer.Ordinal);

        private bool suppressFlagSelectionChanged;
        private string? flagSearchText;
        private ManagedList? lastManagedList;

        public CustomListView()
        {
            InitializeComponent();
            PerformSearchInputChanges();

            UIHelper.PopSplitter(splitContainer1);
            UIHelper.PopSplitter(splitContainer2);
            UIHelper.PopSplitter(splitContainer3);

            UIHelper.PopButton(createDatBtn);

            regexAnythingLabel.Text = DatFilterHelper.REGEX_ZERO_OR_MORE_OF_ANYTHING;
            regexLetterLabel.Text = DatFilterHelper.REGEX_ONE_OR_MORE_LETTERS;
            regexNumberLabel.Text = DatFilterHelper.REGEX_ONE_OR_MORE_NUMBERS;

            expressionFiltersUI.ExpressionsChangedEvt += OnExpressionsChanged;
            expressionFiltersUI.LoadExpressionsEvt += OnLoadExpressions;
            expressionFiltersUI.SaveExpressionsEvt += OnSaveExpressions;

            included_lv.RowClicked += IncludedList_RowClicked;
            excluded_lv.RowClicked += ExcludedList_RowClicked;

            categoryListView_lv.SetColumnHeaderAsCategory();

            ClearAll();

            Facade.RegisterActor(this);
        }
        private void IncludedList_RowClicked(string rowText)
        {
            List_RowClicked(included_lv, rowText);
        }

        private void ExcludedList_RowClicked(string rowText)
        {
            List_RowClicked(excluded_lv, rowText);
        }

        private void List_RowClicked(FastListUI origin, string rowText)
        {
            if (string.IsNullOrWhiteSpace(rowText))
                return;

            var verboseKeys = Keys.Control | Keys.Shift;
            var verbose = (Control.ModifierKeys & verboseKeys) == verboseKeys;

            var report = BuildDecisionReport(rowText, verbose);
            if (report == null)
                return;

            ManagedListItemReportToolTip.Show(origin, report, pinned: true, screenPoint: Control.MousePosition);
        }

        private void OnSaveExpressions(DatFilter[] filters)
        {
            this.expressions = filters.ToList();
            SaveExpressionsEvt?.Invoke(filters);
        }

        private void OnLoadExpressions()
        {
            LoadExpressionsEvt?.Invoke();
        }

        private void OnExpressionsChanged(List<DatFilter> expressions)
        {
            this.expressions = expressions;
            searchTextinput.Text = string.Empty;
            if (!freezeExpressionsUpdateCheckbox.Checked)
                UpdateAllListViews();
        }

        protected void HandleDisposing()
        {
            included_lv.RowClicked -= IncludedList_RowClicked;
            excluded_lv.RowClicked -= ExcludedList_RowClicked;

            Facade.UnregisterActor(this);
        }
        private ManagedListItemReport? BuildDecisionReport(string entryName, bool verbose)
        {
            var ml = lastManagedList;
            if (ml == null)
                return null;

            return ml.BuildDecisionReport(entryName, verbose);
        }

        public void ClearAll()
        {
            categories = Array.Empty<Flag>();
            flagStrings = Array.Empty<string>();
            expressions = new List<DatFilter>();

            totalNumberOfFiles = 0;

            sourceDatVO = null;

            searchTextinput.Text = string.Empty;

            excludedCount_lbl.Text = string.Empty;
            flagSummaryLabel.Text = string.Empty;
            categorySummaryLabel.Text = string.Empty;
            fileCountLabel.Text = string.Empty;
            includeSizeLabel.Text = string.Empty;
            excludeSizeLabel.Text = string.Empty;

            freezeExpressionsUpdateCheckbox.Checked = false;

            expressionFiltersUI.ClearUI();

            flagListView_lv.Items.Clear();

            categoryListView_lv.Items.Clear();
            included_lv.ClearVirtualRows();
            excluded_lv.ClearVirtualRows();

            included_lv.ResetScrollToTopAndPaint();
            excluded_lv.ResetScrollToTopAndPaint();

            included_lv.Invalidate();
            excluded_lv.Invalidate();

            PerformSearchInputChanges();

            mainContainer.Enabled = false;

            flagsCategoriesTabControl.SelectedIndex = 0;

            suppressFlagSelectionChanged = false;
            flagSearchText = null;
            lastManagedList = null;

            completeGameArray = Array.Empty<DatGameVO>();
            fileFlagsCache.Clear();
            fileSizeCache.Clear();
        }
        public void SetView(Flag[] flags, DatVO datVO, Flag[] categories)
        {
            ClearAll();

            sourceDatVO = datVO;
            this.categories = categories;
            flagStrings = flags.Select(f => f.GetName()).ToArray();

            var all = new HashSet<DatGameVO>();

            if (flags.Length > 0)
            {
                for (int i = 0; i < flags.Length; i++)
                {
                    var count = flags[i].GetCount();
                    for (int j = 0; j < count; j++)
                        all.Add(flags[i].GetGameAt(j));
                }
            }
            else
            {
                foreach (var entry in datVO.Entries)
                    all.Add(entry);
            }

            var completeGameList = new List<DatGameVO>(all.Count);
            completeGameList.AddRange(all);

            completeGameList.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));

            completeGameArray = completeGameList.ToArray();

            fileSizeCache.Clear();

            for (int i = 0; i < completeGameArray.Length; i++)
                fileSizeCache[completeGameArray[i].Name] = completeGameArray[i].GetTotalSize();

            totalNumberOfFiles = completeGameList.Count;
            fileCountLabel.Text = totalNumberOfFiles.ToString();

            UpdateAllListViews();
            searchTextinput.Focus();

            datNameLabel.Text = datVO.DatHeaderVO.Name;
            datTypeLabel.Text = datVO.DatHeaderVO.DatTypeEnum.ToString();
            datPathLabel.Text = datVO.DatFullpath ?? Constants.NO_VALUE;

            mainContainer.Enabled = true;
        }

        public void ApplyExpressions(DatFilter[] filters)
        {
            freezeExpressionsUpdateCheckbox.Checked = true;
            expressions = filters.ToList();
            expressionFiltersUI.SetExpressionsUI(expressions);
            freezeExpressionsUpdateCheckbox.Checked = false;
            UpdateAllListViews();
        }

        private bool IsValidSearch()
        {
            string? problem = null;

            if (!DatFilterHelper.IsValidSearch(searchTextinput.Text, out problem))
            {
                if (problem != null)
                    MessageBox.Show(problem, "There was a Problem");

                return false;
            }

            return true;
        }

        private void OnSearch(object? sender, EventArgs? e)
        {
            if (!IsValidSearch())
                return;

            UpdateAllListViews();
        }
        private void UpdateAllListViews()
        {
            included_lv.ResetScrollToTopAndPaint();
            excluded_lv.ResetScrollToTopAndPaint();

            flagListView_lv.BeginUpdate();
            categoryListView_lv.BeginUpdate();
            included_lv.BeginUpdate();
            excluded_lv.BeginUpdate();

            try
            {
                includedRows = Array.Empty<(string Text, EXPRESSION_ACTION_ENUM Action)>();
                excludedRows = Array.Empty<(string Text, EXPRESSION_ACTION_ENUM Action)>();
                included_lv.ClearVirtualRows();
                excluded_lv.ClearVirtualRows();

                var searchState = BuildSearchState();

                if (sourceDatVO == null)
                {
                    ApplyNoDatState();
                    return;
                }

                var ml = new ManagedList(completeGameArray, expressions);
                lastManagedList = ml;

                var inc = BuildFileRows(ml.GetIncludedAndImplict(), ml, searchState);
                var exc = BuildFileRows(ml.GetExcluded(), ml, searchState);

                var includeCount = inc.Count;
                var excludeCount = exc.Count;

                includedRows = includeCount == 0
                    ? new[] { ("(none)", EXPRESSION_ACTION_ENUM.INCLUDE) }
                    : inc.ToArray();

                excludedRows = excludeCount == 0
                    ? new[] { ("(none)", EXPRESSION_ACTION_ENUM.EXCLUDE) }
                    : exc.ToArray();

                included_lv.SetRows(BuildVirtualRows(includedRows));
                excluded_lv.SetRows(BuildVirtualRows(excludedRows));

                included_lv.FitIndexAndTextColumns();
                excluded_lv.FitIndexAndTextColumns();

                fileCountLabel.Text = "Showing: " +
                    DatinateHelper.GetReadableNumber(includeCount) +
                    " / " + DatinateHelper.GetReadableNumber(totalNumberOfFiles);

                excludedCount_lbl.Text = "Excluded: " +
                    DatinateHelper.GetReadableNumber(excludeCount);

                includeSizeLabel.Text = "" + FormatBytes(GetCombinedSize(inc));
                excludeSizeLabel.Text = "" + FormatBytes(GetCombinedSize(exc));

                var prevFlag = searchState.UseFlagSearch ? flagSearchText : flagListView_lv.SelectedFlag;

                suppressFlagSelectionChanged = true;
                try
                {
                    UpdateFlagListUI(inc, includeCount, searchState, prevFlag);
                }
                finally
                {
                    suppressFlagSelectionChanged = false;
                }

                var prevCategory = categoryListView_lv.SelectedFlag;

                suppressFlagSelectionChanged = true;
                try
                {
                    UpdateCategoryListUI(inc, ml, prevCategory);
                }
                finally
                {
                    suppressFlagSelectionChanged = false;
                }
            }
            finally
            {
                excluded_lv.EndUpdate();
                included_lv.EndUpdate();
                flagListView_lv.EndUpdate();
                categoryListView_lv.EndUpdate();
            }
        }

        private readonly struct SearchState
        {
            public bool HasSearch { get; }
            public string SearchText { get; }
            public bool UseFlagSearch { get; }
            public bool IsCategorySearch { get; }
            public ExpressionHandler? Handler { get; }

            public SearchState(bool hasSearch, string searchText, bool useFlagSearch, bool isCategorySearch, ExpressionHandler? handler)
            {
                HasSearch = hasSearch;
                SearchText = searchText;
                UseFlagSearch = useFlagSearch;
                IsCategorySearch = isCategorySearch;
                Handler = handler;
            }
        }

        private SearchState BuildSearchState()
        {
            var raw = searchTextinput.Text;

            var hasSearch = !string.IsNullOrWhiteSpace(raw);
            var searchText = hasSearch ? raw.Trim() : string.Empty;

            var useFlagSearch =
                hasSearch &&
                !string.IsNullOrWhiteSpace(flagSearchText) &&
                string.Equals(searchText, flagSearchText, StringComparison.Ordinal);

            var isCategorySearch =
                hasSearch &&
                searchText.Length >= 2 &&
                searchText[0] == '<' &&
                searchText[searchText.Length - 1] == '>';

            ExpressionHandler? handler =
                (hasSearch && !useFlagSearch && !isCategorySearch)
                    ? new ExpressionHandler(searchText)
                    : null;

            return new SearchState(hasSearch, searchText, useFlagSearch, isCategorySearch, handler);
        }

        private void ApplyNoDatState()
        {
            var totalCategoryOptions = CountNonEmptyCategoryOptions();

            suppressFlagSelectionChanged = true;
            try
            {
                flagListView_lv.SetRows(new[] { ("(none)", 0) });
                categoryListView_lv.SetRows(new[] { ("(none)", 0) });
            }
            finally
            {
                suppressFlagSelectionChanged = false;
            }

            includedRows = new[] { ("(none)", EXPRESSION_ACTION_ENUM.INCLUDE) };
            excludedRows = new[] { ("(none)", EXPRESSION_ACTION_ENUM.EXCLUDE) };

            included_lv.SetRows(BuildVirtualRows(includedRows));
            excluded_lv.SetRows(BuildVirtualRows(excludedRows));

            included_lv.FitIndexAndTextColumns();
            excluded_lv.FitIndexAndTextColumns();

            included_lv.Invalidate();
            excluded_lv.Invalidate();

            fileCountLabel.Text = "Showing: 0 / " + DatinateHelper.GetReadableNumber(totalNumberOfFiles);
            excludedCount_lbl.Text = "Excluded: 0";
            flagSummaryLabel.Text = "Flags: 0 / " + DatinateHelper.GetReadableNumber(flagStrings.Length);
            categorySummaryLabel.Text = "Categories: 0 / " + DatinateHelper.GetReadableNumber(totalCategoryOptions);
            includeSizeLabel.Text = "0 B";
            excludeSizeLabel.Text = "0 B";
        }

        private List<(string Text, EXPRESSION_ACTION_ENUM Action)> BuildFileRows(
            IEnumerable<KeyValuePair<string, EXPRESSION_ACTION_ENUM>> rows,
            ManagedList ml,
            SearchState searchState)
        {
            var list = rows is ICollection<KeyValuePair<string, EXPRESSION_ACTION_ENUM>> c
                ? new List<(string Text, EXPRESSION_ACTION_ENUM Action)>(c.Count)
                : new List<(string Text, EXPRESSION_ACTION_ENUM Action)>();

            foreach (var p in rows)
            {
                if (SearchMatchesFile(p.Key, ml, searchState))
                    list.Add((p.Key, p.Value));
            }

            return list;
        }

        private bool SearchMatchesFile(string fileName, ManagedList ml, SearchState searchState)
        {
            if (!searchState.HasSearch)
                return true;

            if (searchState.UseFlagSearch)
            {
                if (string.Equals(flagSearchText, DatFilterHelper.NO_FLAGS, StringComparison.Ordinal))
                    return GetFlagsCached(fileName).Length == 0;

                return fileName.Contains(flagSearchText!, StringComparison.Ordinal);
            }

            if (searchState.IsCategorySearch)
            {
                if (!ml.TryGetEntry(fileName, out var entry))
                    return false;

                var cat = entry.Category;
                if (string.IsNullOrWhiteSpace(cat))
                    return false;

                var label = "<" + cat.Trim() + ">";
                return string.Equals(label, searchState.SearchText, StringComparison.Ordinal);
            }

            return searchState.Handler!.Match(fileName);
        }

        private static bool SearchMatchesFlagLabel(string label, SearchState searchState, string? flagSearchText)
        {
            if (!searchState.HasSearch)
                return true;

            if (searchState.UseFlagSearch)
                return string.Equals(label, flagSearchText, StringComparison.Ordinal);

            if (searchState.Handler == null)
                return true;

            return searchState.Handler.Match(label);
        }

        private static (string Text, Color Colour)[] BuildVirtualRows((string Text, EXPRESSION_ACTION_ENUM Action)[] rows)
        {
            var ui = new (string Text, Color Colour)[rows.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                var text = rows[i].Text;

                var colour = string.Equals(text, "(none)", StringComparison.Ordinal)
                    ? SystemColors.GrayText
                    : DatFilterHelper.GetExpressionColour(rows[i].Action);

                ui[i] = (text, colour);
            }

            return ui;
        }

        private void UpdateFlagListUI(
            List<(string Text, EXPRESSION_ACTION_ENUM Action)> includedRowsList,
            int includeCount,
            SearchState searchState,
            string? prevFlag)
        {
            if (searchState.UseFlagSearch && !string.IsNullOrWhiteSpace(prevFlag))
            {
                var selected = prevFlag;

                if (string.Equals(selected, "(none)", StringComparison.Ordinal))
                {
                    flagSummaryLabel.Text = "Flags: 0 / " + DatinateHelper.GetReadableNumber(flagStrings.Length);
                    flagListView_lv.SetRows(new[] { ("(none)", 0) });
                    return;
                }

                var totalFlagOptions = flagStrings.Length + (ShouldAddNoFlagsOptionForSelected(selected) ? 1 : 0);

                flagSummaryLabel.Text = "Flags: 1 / " + DatinateHelper.GetReadableNumber(totalFlagOptions);
                flagListView_lv.SetRows(new[] { (selected, includeCount) }, selected);
                return;
            }

            var flagCounts = CountFlagOccurrences(includedRowsList);

            var noFlagsAvailable = flagCounts.TryGetValue(DatFilterHelper.NO_FLAGS, out var noFlagsCount);
            var noFlagsIsInFlagStrings = IsNoFlagsInFlagStrings();

            var tmp = new (string Flag, int Count)[flagStrings.Length + 1];
            var rowCount = 0;

            if (noFlagsAvailable && SearchMatchesFlagLabel(DatFilterHelper.NO_FLAGS, searchState, flagSearchText))
                tmp[rowCount++] = (DatFilterHelper.NO_FLAGS, noFlagsCount);

            for (int i = 0; i < flagStrings.Length; i++)
            {
                var flagLabel = flagStrings[i];

                if (noFlagsAvailable && string.Equals(flagLabel, DatFilterHelper.NO_FLAGS, StringComparison.Ordinal))
                    continue;

                if (!SearchMatchesFlagLabel(flagLabel, searchState, flagSearchText))
                    continue;

                if (!flagCounts.TryGetValue(flagLabel, out var count))
                    continue;

                tmp[rowCount++] = (flagLabel, count);
            }

            var totalOptions = flagStrings.Length + (noFlagsAvailable && !noFlagsIsInFlagStrings ? 1 : 0);
            flagSummaryLabel.Text = "Flags: " +
                DatinateHelper.GetReadableNumber(rowCount) +
                " / " + DatinateHelper.GetReadableNumber(totalOptions);

            if (rowCount == 0)
            {
                flagListView_lv.SetRows(new[] { ("(none)", 0) });
                return;
            }

            var rows = new (string Flag, int Count)[rowCount];
            Array.Copy(tmp, rows, rowCount);
            flagListView_lv.SetRows(rows, prevFlag);
        }

        private Dictionary<string, int> CountFlagOccurrences(List<(string Text, EXPRESSION_ACTION_ENUM Action)> includedRowsList)
        {
            var flagCounts = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < includedRowsList.Count; i++)
            {
                var fs = GetFlagsCached(includedRowsList[i].Text);

                if (fs.Length == 0)
                {
                    ref var v = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(
                        flagCounts,
                        DatFilterHelper.NO_FLAGS,
                        out _);

                    v++;
                    continue;
                }

                for (int k = 0; k < fs.Length; k++)
                {
                    var f = fs[k];

                    ref var v = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(
                        flagCounts,
                        f,
                        out _);

                    v++;
                }
            }

            return flagCounts;
        }

        private bool ShouldAddNoFlagsOptionForSelected(string selected)
        {
            if (!string.Equals(selected, DatFilterHelper.NO_FLAGS, StringComparison.Ordinal))
                return false;

            return !IsNoFlagsInFlagStrings();
        }

        private bool IsNoFlagsInFlagStrings()
        {
            for (int i = 0; i < flagStrings.Length; i++)
            {
                if (string.Equals(flagStrings[i], DatFilterHelper.NO_FLAGS, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private void UpdateCategoryListUI(
            List<(string Text, EXPRESSION_ACTION_ENUM Action)> includedRowsList,
            ManagedList ml,
            string? prevCategory)
        {
            var totalCategoryOptions = CountNonEmptyCategoryOptions();

            if (totalCategoryOptions == 0)
            {
                categorySummaryLabel.Text = "Categories: 0 / 0";
                categoryListView_lv.SetRows(new[] { ("(none)", 0) });
                return;
            }

            var categoryCounts = CountCategoryOccurrences(includedRowsList, ml);

            var tmp = new (string Flag, int Count)[totalCategoryOptions];
            var rowCount = 0;

            for (int i = 0; i < categories.Length; i++)
            {
                var name = categories[i].GetName();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (!categoryCounts.TryGetValue(name, out var c))
                    continue;

                if (c <= 0)
                    continue;

                tmp[rowCount++] = (name, c);
            }

            categorySummaryLabel.Text = "Categories: " +
                DatinateHelper.GetReadableNumber(rowCount) +
                " / " + DatinateHelper.GetReadableNumber(totalCategoryOptions);

            if (rowCount == 0)
            {
                categoryListView_lv.SetRows(new[] { ("(none)", 0) }, prevCategory);
                return;
            }

            var rows = new (string Flag, int Count)[rowCount];
            Array.Copy(tmp, rows, rowCount);
            categoryListView_lv.SetRows(rows, prevCategory);
        }

        private Dictionary<string, int> CountCategoryOccurrences(
            List<(string Text, EXPRESSION_ACTION_ENUM Action)> includedRowsList,
            ManagedList ml)
        {
            var categoryCounts = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < includedRowsList.Count; i++)
            {
                if (!ml.TryGetEntry(includedRowsList[i].Text, out var entry))
                    continue;

                var cat = entry.Category;
                if (string.IsNullOrWhiteSpace(cat))
                    continue;

                var label = "<" + cat.Trim() + ">";

                ref var v = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(
                    categoryCounts,
                    label,
                    out _);

                v++;
            }

            return categoryCounts;
        }

        private int CountNonEmptyCategoryOptions()
        {
            var total = 0;

            for (int i = 0; i < categories.Length; i++)
            {
                var n = categories[i].GetName();
                if (!string.IsNullOrWhiteSpace(n))
                    total++;
            }

            return total;
        }

        private ulong GetCombinedSize(IEnumerable<(string Text, EXPRESSION_ACTION_ENUM Action)> rows)
        {
            ulong total = 0;

            foreach (var row in rows)
            {
                if (fileSizeCache.TryGetValue(row.Text, out var size))
                    total += size;
            }

            return total;
        }

        private static string FormatBytes(ulong bytes)
        {
            if (bytes == 0)
                return "0 B";

            var units = new[] { "B", "KB", "MB", "GB", "TB", "PB" };
            double value = bytes;
            var unitIndex = 0;

            while (value >= 1024d && unitIndex < units.Length - 1)
            {
                value /= 1024d;
                unitIndex++;
            }

            return value.ToString("0.##") + " " + units[unitIndex];
        }

        private void OnExcludeBtnClick(object sender, EventArgs e)
        {
            expressionFiltersUI.AddExpression(
                new DatFilter(searchTextinput.Text, EXPRESSION_ACTION_ENUM.EXCLUDE));
        }

        private void OnConditionalExcludeBtn(object sender, EventArgs e)
        {
            expressionFiltersUI.AddExpression(
                new DatFilter(searchTextinput.Text, EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL));
        }

        private void OnIncludeBtnClick(object sender, EventArgs e)
        {
            expressionFiltersUI.AddExpression(
                new DatFilter(searchTextinput.Text, EXPRESSION_ACTION_ENUM.INCLUDE));
        }

        private void flagListView_lv_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (suppressFlagSelectionChanged)
                return;

            var flag = flagListView_lv.SelectedFlag;

            if (string.IsNullOrWhiteSpace(flag) || string.Equals(flag, "(none)", StringComparison.Ordinal))
                return;

            flagSearchText = flag;
            searchTextinput.Text = flagSearchText;
            UpdateAllListViews();
        }

        private void categoryListView_lv_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (suppressFlagSelectionChanged)
                return;

            var flag = categoryListView_lv.SelectedFlag;

            if (string.IsNullOrWhiteSpace(flag) || string.Equals(flag, "(none)", StringComparison.Ordinal))
                return;

            searchTextinput.Text = flag;
            UpdateAllListViews();
        }

        private void OnSearchTextChange(object sender, EventArgs e)
        {
            PerformSearchInputChanges();
        }

        protected void PerformSearchInputChanges()
        {
            var textOK = !string.IsNullOrEmpty(searchTextinput.Text.Trim());

            includeBtn.Enabled = textOK;
            excludeBtn.Enabled = textOK;
            conditionalBtn.Enabled = textOK;
            clearBtn.Enabled = true;
            goBtn.Enabled = textOK;

            if (freezeExpressionsUpdateCheckbox.Checked)
            {
                clearBtn.Enabled = false;
                goBtn.Enabled = false;
            }
        }

        private void OnSearchClear(object sender, EventArgs e)
        {
            clearSearchTextAndUpdate();
        }

        private void search_tb_DoubleClick(object sender, EventArgs e)
        {
            clearSearchTextAndUpdate();
        }
        protected void clearSearchTextAndUpdate()
        {
            expressionFiltersUI.UnselectAll();

            suppressFlagSelectionChanged = true;

            try
            {
                foreach (var item in flagListView_lv.SelectedItems.Cast<ListViewItem>().ToArray())
                    item.Selected = false;

                foreach (var item in categoryListView_lv.SelectedItems.Cast<ListViewItem>().ToArray())
                    item.Selected = false;
            }
            finally
            {
                suppressFlagSelectionChanged = false;
            }

            flagSearchText = null;

            searchTextinput.Text = string.Empty;
            UpdateAllListViews();
        }


        private void createDatBtn_Click(object sender, EventArgs e)
        {
            CreateDatClickEvt?.Invoke(CreateCustomDatVO());
        }

        private string[] GetFlagsCached(string fileName)
        {
            if (!fileFlagsCache.TryGetValue(fileName, out var flags))
            {
                flags = DatFilterHelper.GetIndividualFlags(fileName);
                fileFlagsCache.Add(fileName, flags);
            }

            return flags;
        }

        public DatVO? CreateCustomDatVO()
        {
            if (sourceDatVO == null)
                return null;

            var include = new HashSet<string>(
                includedRows.Select(r => r.Text).Where(s => s.Length > 0 && s != "(none)"),
                StringComparer.Ordinal);

            var filteredGames = new List<DatGameVO>(include.Count);

            foreach (var entry in sourceDatVO.Entries)
            {
                if (include.Contains(entry.Name))
                    filteredGames.Add(entry);
            }

            return DatVO.CreateDatFrom(sourceDatVO, filteredGames);
        }

        private void searchTextinput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                OnSearch(null, null);
        }

        private void CopyFlagListToClipboardBtn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (flagListView_lv.Items.Count == 0)
                return;

            var lines = new List<string>(flagListView_lv.Items.Count);

            foreach (ListViewItem item in flagListView_lv.Items)
            {
                var flagText = item.Text;
                if (string.Equals(flagText, "(none)", StringComparison.Ordinal))
                    continue;

                var countText = item.SubItems.Count > 1 ? item.SubItems[1].Text : string.Empty;

                lines.Add(string.IsNullOrWhiteSpace(countText) ? flagText : (flagText + "\t" + countText));
            }

            if (lines.Count == 0)
                return;

            Clipboard.SetText(string.Join(Environment.NewLine, lines));
        }

        private void flagsCategoriesTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearSearchTextAndUpdate();
        }
    }
}
