using com.RADIO.Datinate.RMVC.Shared;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public partial class Media2AssignControlsUI : UserControl
    {
        public event Action<MEDIA_ASSIGNMENT_ENUM>? AssignmentChangeEvt;
        public event Action<string>? PerformSearchEvt;
        public event Action? ToggleSortEvt;
        public event Action? BrowserBtnClickEvt;
        public event Action? NotFoundBtnClickEvt;
        public string? EntryName => entryNameTxt.Text == "<Unassigned>" ? null : entryNameTxt.Text;
        internal MEDIA_ASSIGNMENT_ENUM AssignmentStatus => assignmentBtnStrip.AssignmentStatus;

        private IReadOnlyList<string> searchPrompts = new List<string>();
        private readonly SearchPromptContextMenu searchPromptMenu = new();

        public Media2AssignControlsUI()
        {
            InitializeComponent();
            DatinateHelper.HideTabs(tabControl);
            assignmentBtnStrip.SelectedChanged += OnSegmentChange;

            filterTxt.Click += filterTxt_Click;

            var tt = new ToolTip
            {
                AutoPopDelay = 10000,
                InitialDelay = 400,
                ReshowDelay = 200,
                ShowAlways = true
            };

            tt.SetToolTip(
                filterTxt,
                "Double-click to clear Search text and toggle sorting (alphabetical / best match).");
        }

        internal void ResetUI()
        {
            searchPromptMenu.Close();

            entryNameTxt.Text = string.Empty;
            filterTxt.Text = string.Empty;
            matchPercentageLabel.Text = string.Empty;
        }
        internal void SetEntryName(string name)
        {
            entryNameTxt.Text = name;
        }
        internal void SetBestScore(int bestScorePercentage)
        {
            matchPercentageLabel.Text = bestScorePercentage.ToString() + "%";
            matchBarsUI.PercentMinWarning = 49;
            matchBarsUI.PercentFull = bestScorePercentage;
        }

        internal void SetSearchPrompts(IReadOnlyList<string>? searchPrompts)
        {
            this.searchPrompts = searchPrompts ?? new List<string>();
            searchPromptMenu.Close();
        }

        internal void ShowNamePage()
        {
            if (tabControl.SelectedTab != namePage)
                tabControl.SelectedTab = namePage;
        }
        internal void ShowListFilterPage()
        {
            if (tabControl.SelectedTab != listPage)
                tabControl.SelectedTab = listPage;
        }
        internal void ShowBestScorePage()
        {
            if (tabControl.SelectedTab != matchPage)
                tabControl.SelectedTab = matchPage;
        }
        internal void ShowMediaAssignmentControlsPage()
        {
            if (tabControl.SelectedTab != mediaPage)
                tabControl.SelectedTab = mediaPage;
        }

        internal void SetAssignment(MEDIA_ASSIGNMENT_ENUM assignmentStatus)
        {
            assignmentBtnStrip.Selected = assignmentStatus;
        }
        internal void SetBackColour(Color backColor)
        {
            BackColor = backColor;

            tabControl.SuspendLayout();

            try
            {
                tabControl.BackColor = backColor;

                foreach (TabPage page in tabControl.TabPages)
                {
                    page.BackColor = backColor;
                    page.UseVisualStyleBackColor = false; // NOTE: stops theme overriding.
                }
            }
            finally
            {
                tabControl.ResumeLayout(false);
            }
        }
        private void filterTxt_Click(object? sender, EventArgs e)
        {
            ShowSearchPrompts();
        }

        private void ShowSearchPrompts()
        {
            // NOTE: Prompts are only offered as shortcuts when the field is empty.
            if (filterTxt.Text.Length != 0)
                return;

            if (searchPrompts.Count == 0)
                return;

            searchPromptMenu.Close();
            searchPromptMenu.Items.Clear();

            foreach (var prompt in searchPrompts)
            {
                var item = new ToolStripMenuItem(prompt);

                item.Click += (_, _) =>
                {
                    filterTxt.Text = prompt;
                    filterTxt.SelectionStart = filterTxt.Text.Length;
                    filterTxt.Focus();
                };

                searchPromptMenu.Items.Add(item);
            }

            searchPromptMenu.ShowFor(filterTxt);
        }
        private void OnSegmentChange(MEDIA_ASSIGNMENT_ENUM segment)
        {
            AssignmentChangeEvt?.Invoke(segment);
        }
        private void REAL_browserBtn_Click(object sender, EventArgs e)
        {
            BrowserBtnClickEvt?.Invoke();
        }

        private void notFoundBtn_Click(object sender, EventArgs e)
        {
            NotFoundBtnClickEvt?.Invoke();
        }

        private void filterTxt_DoubleClick(object sender, EventArgs e)
        {
            filterTxt.Text = string.Empty;
            ToggleSortEvt?.Invoke();
        }


        private void filterTxt_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void filterTxt_TextChanged(object sender, EventArgs e)
        {
            if (filterTxt.Text.Length > 0)
                searchPromptMenu.Close();

            PerformSearchEvt?.Invoke(filterTxt.Text);
        }

        private void toggleSort_Click(object sender, EventArgs e)
        {
            filterTxt.Text = string.Empty;
            ToggleSortEvt?.Invoke();
        }
    }
}
