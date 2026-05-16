using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using System.ComponentModel;

namespace datinate.app
{
    public partial class DatGrouperSearchUI : UserControl
    {
        public static Color HighlightTextBackColour_ => Color.Khaki;

        public Action? SearchStateChangedEvt;
        public Action<object, bool>? JumpToItemEvt;
        public string? HighlightText => searchHighlightText;
        public Color HighlightTextBackColour => HighlightTextBackColour_;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object? SelectedNode
        {
            get => selectedNode;
            set
            {
                if (!ReferenceEquals(value, selectedNode))
                {
                    if (value is not null)
                        lastJumpedItem = value;

                    selectedNode = value;
                }
            }
        }

        private object? selectedNode = null;

        private Dictionary<IGameFamily, object> familyDictionary = new();
        private List<KeyValuePair<IGameFamily, object>>? searchList;

        private string? searchHighlightText;
        private string? lastSearchTerm = null;
        private bool _searchAllSelectedOnMouseDown = false;
        private object? lastJumpedItem = null;

        public DatGrouperSearchUI()
        {
            InitializeComponent();
        }

        public void SetUI(Dictionary<IGameFamily, object> familyRendererDictionary)
        {
            ClearUI();
            this.familyDictionary = familyRendererDictionary;
        }

        public void SetSearchText(string value)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() => SetSearchText(value)));
                return;
            }

            searchTextBox.Text = value;
        }

        public void ClearSearchHighlights()
        {
            if (string.IsNullOrWhiteSpace(searchHighlightText))
                return;

            searchHighlightText = null;
            
            SearchStateChangedEvt?.Invoke();
        }

        public void ClearUI()
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() => ClearUI()));
                return;
            }
            selectedNode = null;
            familyDictionary.Clear();
            searchHighlightText = null;
            lastSearchTerm = null;
            searchList = null;
            lastJumpedItem = null;
            _searchAllSelectedOnMouseDown = false;
        }

        private void EnsureSearchList()
        {
            if (searchList != null && searchList.Count == familyDictionary.Count)
                return;

            searchList = familyDictionary
                .OrderBy(kp => kp.Key.GetFamilyDisplayName(), StringComparer.OrdinalIgnoreCase).ToList();
        }

        private bool MatchesFilter(IGameFamily family, string text)
        {
            bool Match(string name)
            {
                if (string.IsNullOrEmpty(name))
                    return false;

                return !searchStartsWithCheckBox.Checked
                    ? name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0
                    : name.StartsWith(text, StringComparison.OrdinalIgnoreCase);
            }

            if (Match(family.GetFamilyDisplayName()))
                return true;

            foreach (var part in family.GetAllGameParts(true))
            {
                if (Match(part.GetName()))
                    return true;
            }

            return false;
        }

        private void SetSearchCount()
        {
            if (InvokeRequired)
            {
                if (!IsDisposed && IsHandleCreated)
                    BeginInvoke(new Action(SetSearchCount));

                return;
            }

            if (string.IsNullOrWhiteSpace(searchTextBox.Text) || familyDictionary.Count == 0)
            {
                countLabel.Text = "-";
                return;
            }

            EnsureSearchList();

            if (searchList == null || searchList.Count == 0)
            {
                countLabel.Text = "-";
                return;
            }

            var text = searchTextBox.Text;

            countLabel.Text = DatinateHelper.GetReadableNumber(
                searchList.Count(kp => MatchesFilter(kp.Key, text)));
        }

        private void searchLeftBtn_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
                JumpToNearestFamily(false);
            else
                Search(false);

            searchTextBox.Focus();
            searchTextBox.Select(searchTextBox.TextLength, 0);
        }

        private void searchRightBtn_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
                JumpToNearestFamily(true);
            else
                Search(true);

            searchTextBox.Focus();
            searchTextBox.Select(searchTextBox.TextLength, 0);
        }

        private void searchTxt_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                bool searchInNode = searchTextBox.Text != lastSearchTerm;
                lastSearchTerm = searchTextBox.Text;
                Search(true, searchInNode);

                searchTextBox.Focus();
                searchTextBox.Select(searchTextBox.TextLength, 0);
            }
        }

        private void searchTxt_TextChanged(object? sender, EventArgs e)
        {
            countLabel.Text = "-";
        }

        private void Search(bool forwards, bool includeCurrentlySelectedNode = false)
        {
            bool refresh;

            if (string.IsNullOrWhiteSpace(searchTextBox.Text) || familyDictionary.Count == 0)
            {
                refresh = searchHighlightText != null;

                searchHighlightText = null;

                if (refresh)
                    SearchStateChangedEvt?.Invoke();
                
                SetSearchCount();
                return;
            }

            EnsureSearchList();

            if (searchList == null || searchList.Count == 0)
            {
                refresh = searchHighlightText != null;

                searchHighlightText = null;

                if (refresh)
                    SearchStateChangedEvt?.Invoke();

                SetSearchCount();
                return;
            }

            var text = searchTextBox.Text;

            refresh = searchHighlightText != text;

            searchHighlightText = text;

            var count = searchList.Count;
            var direction = forwards ? 1 : -1;

            int startIndex = -1;

            var anchor = SelectedNode ?? lastJumpedItem;
            if (anchor != null)
                startIndex = searchList.FindIndex(kp => ReferenceEquals(kp.Value, anchor));

            if (includeCurrentlySelectedNode && startIndex >= 0)
            {
                var currentKp = searchList[startIndex];
                if (MatchesFilter(currentKp.Key, text))
                {
                    SetSearchCount();
                    
                    if (refresh)
                        SearchStateChangedEvt?.Invoke();
                    
                    return;
                }
            }

            if (startIndex < 0)
                startIndex = forwards ? -1 : 0;

            for (int step = 1; step <= count; step++)
            {
                var idx = (startIndex + direction * step + count) % count;
                var kp = searchList[idx];

                if (!MatchesFilter(kp.Key, text))
                    continue;

                var n = kp.Value;

                lastJumpedItem = n;
                SelectNodeIfNeeded(n, forwards);

                SetSearchCount();
                if (refresh)
                    SearchStateChangedEvt?.Invoke();
                return;
            }

            SetSearchCount();

            if (refresh)
                SearchStateChangedEvt?.Invoke();
        }

        private void SelectNodeIfNeeded(object? item, bool forwards)
        {
            if (item != null)
                JumpToItemEvt?.Invoke(item, forwards);
        }

        private void searchTxt_Enter(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() => searchTextBox.SelectAll()));
        }
        private void searchTxt_MouseDown(object sender, MouseEventArgs e)
        {
            _searchAllSelectedOnMouseDown =
                searchTextBox.TextLength > 0 &&
                searchTextBox.SelectionLength == searchTextBox.TextLength;
        }

        private void searchTxt_MouseUp(object sender, MouseEventArgs e)
        {
            if (_searchAllSelectedOnMouseDown)
            {
                searchTextBox.SelectionLength = 0;
                searchTextBox.SelectionStart = searchTextBox.TextLength;
                _searchAllSelectedOnMouseDown = false;
            }
        }

        private void clearSearchBtn_Click(object sender, EventArgs e)
        {
            searchTextBox.Text = string.Empty;

            bool refresh = searchHighlightText != null;

            searchHighlightText = null;
            SetSearchCount();

            if (refresh)
                SearchStateChangedEvt?.Invoke();
        }

        private void searchStartsWithCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Search(true, true);
        }
        private void JumpToNearestFamily(bool forwards)
        {
            if (familyDictionary.Count == 0)
                return;

            bool refresh = searchHighlightText != null;

            searchHighlightText = null;

            EnsureSearchList();

            if (searchList == null || searchList.Count == 0)
                return;

            var count = searchList.Count;
            var direction = forwards ? 1 : -1;

            int startIndex = -1;

            var anchor = SelectedNode ?? lastJumpedItem;
            if (anchor != null)
                startIndex = searchList.FindIndex(kp => ReferenceEquals(kp.Value, anchor));

            if (startIndex < 0)
                startIndex = forwards ? -1 : 0;

            var idx = (startIndex + direction + count) % count;
            var target = searchList[idx].Value;

            lastJumpedItem = target;
            SelectNodeIfNeeded(target, forwards);

            SetSearchCount();

            if (refresh)
                SearchStateChangedEvt?.Invoke();
        }
    }
}
