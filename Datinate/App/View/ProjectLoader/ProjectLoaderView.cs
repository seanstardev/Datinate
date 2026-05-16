using app.datinate;
using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using System.ComponentModel;
using System.Reflection;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public partial class ProjectLoaderView : UserControl, IProjectLoaderView
    {
        public event Action? ViewInitialisedEvt;
        public event Action<string?, string?, string?>? SaveGlobalDatPathsEvt;

        public event Action? LoadingProjectEvt;
        public event Action<DatGrouperProjectDTO>? BuildProjectEvt;
        public event Action<DatGrouperProjectDTO>? SaveProjectEvt;
        public event Action? LoadExpressionsFileEvt;
        public event Action<DatGrouperProjectEntry>? EditExpressionsFileEvt;
        public event Action<DatGrouperProjectDTO>? HighlightEvt;
        public event Action<DatGrouperProjectDTO>? DatFullpathsChangedEvt;
        public event Action<string>? ShowTreeViewEvt;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsProjectLoaded =>
            !isCreatingNewProject &&
            currentProject != null &&
            !string.IsNullOrWhiteSpace(currentProject.ProjectName);

        private ProjectDatUI? lastClickedrojectDatUI = null;
        private SortedDictionary<string, DatGrouperProjectDTO> projectModel = new SortedDictionary<string, DatGrouperProjectDTO>();

        private bool ignoreProjectSelectionChange = false;
        private bool isCreatingNewProject = false;
        private string? projectNameBeforeNewMode = null;
        private int hoveredProjectIndex = -1;
        private HashSet<ProjectDatUI> datUIs = new HashSet<ProjectDatUI>();

        private string? projectComment = null;
        private ProjectDatUI? activeCommentDatUI = null;

        private DatGrouperProjectDTO? currentProject = null;

        
        // FlowLayoutPanel drag-reorder support per container (same-panel only).
        private readonly Dictionary<FlowLayoutPanel, FlowReorderDragManager> dragManagers = new();

        public ProjectLoaderView()
        {
            InitializeComponent();
            EnableDoubleBuffer(gamesIncludeContainer);
            EnableDoubleBuffer(mediaIncludeContainer);

            dragManagers[gamesIncludeContainer] = new FlowReorderDragManager(this, gamesIncludeContainer);
            dragManagers[mediaIncludeContainer] = new FlowReorderDragManager(this, mediaIncludeContainer);
            Facade.RegisterActor(this);

            UIHelper.PopButton(saveProjectBtn);
            UIHelper.PopButton(buildProjectBtn);
            UIHelper.PopButton(newProjectBtn);
            UIHelper.PopButton(cancelNewProjectBtn);

            ConfigureProjectBrowser();
            projectNameText.TextChanged += projectNameText_TextChanged;

            ViewInitialisedEvt?.Invoke();
            ClearView();
            UpdateProjectBrowserState();
        }

        public void SetView(DatGrouperProjectDTO[] projects, string? projectToLoad = null)
        {
            // NOTE: Do NOT run in UI thread as projects get set before the UI gets rendered.

            if (isCreatingNewProject)
            {
                isCreatingNewProject = false;
                projectNameBeforeNewMode = null;
            }

            ClearView();

            ignoreProjectSelectionChange = true;
            try
            {
                projectListBox.Items.Clear();
                projectModel = new SortedDictionary<string, DatGrouperProjectDTO>();

                for (int i = 0; i < projects.Length; i++)
                {
                    projectListBox.Items.Add(projects[i].ProjectName);
                    projectModel.Add(projects[i].ProjectName, projects[i]);
                }

                projectListBox.ClearSelected();
            }
            finally
            {
                ignoreProjectSelectionChange = false;
            }

            UpdateProjectBrowserState();

            if (!string.IsNullOrWhiteSpace(projectToLoad) && SelectProjectInList(projectToLoad))
                return;

            if (projectListBox.Items.Count == 0)
            {
                BeginNewProjectMode();
                return;
            }

            currentProject = null;
            projectNameText.Text = string.Empty;
            projectNameText.Enabled = false;
        }

        public DatGrouperProjectEntry[] GetAllDatHeadlines()
        {
            List<DatGrouperProjectEntry> list = new List<DatGrouperProjectEntry>();

            list.AddRange(GetSoftwareIncludeDats());
            list.AddRange(GetIgnoreDats());
            list.AddRange(GetAuxIncludeEntries());

            return list.ToArray();
        }

        public void SetExpressionsFileForLastSelected(string expressionsXmlFullpath)
        {
            lastClickedrojectDatUI?.SetExpressionsFullpath(expressionsXmlFullpath);
        }

        public void AddDat(
            DatGrouperProjectEntry datHeadline,
            DAT_GROUP_TARGET_ENUM datGroupTargetEnum,
            bool forceAddAsFirst)
        {
            FlowLayoutPanel? container = GetContainer(datGroupTargetEnum);
            if (container == null)
                return;

            SuspendLayout();
            container.SuspendLayout();

            try
            {
                AddDatCore(datHeadline, datGroupTargetEnum, forceAddAsFirst, false);
            }
            finally
            {
                container.ResumeLayout(true);
                ResumeLayout(true);
            }
        }

        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }

        private DatGrouperProjectDTO? GetProject()
        {
            var projectName = projectNameText.Text.Trim();

            if (string.IsNullOrWhiteSpace(projectName))
                projectName = currentProject?.ProjectName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(projectName))
                return null;

            DatGrouperProjectDTO projectVO = new DatGrouperProjectDTO(
                projectName,
                GetSoftwareIncludeDats(),
                GetIgnoreDats(),
                GetAuxIncludeEntries(),
                projectComment ?? string.Empty,
                currentProject?.ExcludedDescriptorCodes ?? new HashSet<string>(),
                currentProject?.ScoringMediaTypes ?? new HashSet<string>(),
                currentProject?.ExportSoftwareOptionsDTO ?? ExportSoftwareOptionsDTO.CreateDefault(),
                currentProject?.MediaExports ?? new List<DatGrouperMediaExportEntryDTO>());

            return projectVO;
        }
        private void OnDatRemoved(ProjectDatUI datUI, Control parentUI)
        {
            if (parentUI is not FlowLayoutPanel p)
                return;

            p.SuspendLayout();
            try
            {
                p.Controls.Remove(datUI);
                DisposeDatUI(datUI);
                CheckParentSetup(p);
            }
            finally
            {
                p.ResumeLayout(true);
            }
        }
        DatGrouperProjectEntry[] GetAuxIncludeEntries()
        {
            List<DatGrouperProjectEntry> list = new List<DatGrouperProjectEntry>();

            for (int i = 0; i < mediaIncludeContainer.Controls.Count; i++)
            {
                var ui = mediaIncludeContainer.Controls[i] as ProjectDatUI;
                if (ui == null) continue;

                var dat = ui.GetData();

                if (dat != null)
                    list.Add(dat);
            }
            return list.ToArray();
        }

        DatGrouperProjectEntry[] GetSoftwareIncludeDats(bool excludeThoseBeingRemoved = false)
        {
            List<DatGrouperProjectEntry> list = new List<DatGrouperProjectEntry>();

            for (int i = 0; i < gamesIncludeContainer.Controls.Count; i++)
            {
                var ui = gamesIncludeContainer.Controls[i] as ProjectDatUI;
                if (ui == null) continue;
                // race condition:
                if (ui.IsBeingRemoved && excludeThoseBeingRemoved) continue;

                var data = ui.GetData();

                if (data != null)
                    list.Add(data);
            }
            return list.ToArray();
        }

        DatGrouperProjectEntry[] GetIgnoreDats()
        {
            List<DatGrouperProjectEntry> list = new List<DatGrouperProjectEntry>();
            return list.ToArray();
        }


        public void ClearView()
        {
            Ui(() => { 
                EmptyContainer(gamesIncludeContainer);
                EmptyContainer(mediaIncludeContainer);

                currentProject = null;
                commentLabel.Text = GetProjectCommentName();
                commentText.Text = "";
                projectComment = null;
                datUIs = new HashSet<ProjectDatUI>();
                activeCommentDatUI = null;
                hoveredProjectIndex = -1;

                SetCommentEditActive(false);
            });
        }

        void EmptyContainer(FlowLayoutPanel p)
        {
            List<ProjectDatUI> list = new List<ProjectDatUI>(p.Controls.Count);

            p.SuspendLayout();
            try
            {
                for (int i = p.Controls.Count - 1; i >= 0; i--)
                {
                    if (p.Controls[i] is ProjectDatUI ui)
                        list.Add(ui);
                }

                for (int i = 0; i < list.Count; i++)
                    p.Controls.Remove(list[i]);

                for (int i = 0; i < list.Count; i++)
                    DisposeDatUI(list[i]);
            }
            finally
            {
                p.ResumeLayout(true);
            }
        }
        public void CheckParentSetup(FlowLayoutPanel p)
        {
            for (int i = 0; i < p.Controls.Count; i++)
            {
                var ui = p.Controls[i] as ProjectDatUI;

                if (ui != null)
                {
                    ui.UpdateByIndex(i, i == p.Controls.Count - 1);
                }
            }
        }
        internal void UpdateAfterSwap(FlowLayoutPanel p, ProjectDatUI a, ProjectDatUI b)
        {
            var last = p.Controls.Count - 1;

            var ia = p.Controls.GetChildIndex(a);
            var ib = p.Controls.GetChildIndex(b);

            // Only these two indices changed for an adjacent swap.
            a.UpdateByIndex(ia, ia == last);
            b.UpdateByIndex(ib, ib == last);
        }
        void OnUiOrderChange(ProjectDatUI ui, Control parentUI)
        {
            var p = (FlowLayoutPanel)parentUI;

            var idx = p.Controls.GetChildIndex(ui);
            var max = p.Controls.Count - 1;

            const int PULSE_MS = 420;
            const int ANIM_BASE_MS = 260;

            switch (ui.LastOrderChangeAction)
            {
                case ProjectDatUI.ORDER_CHANGE_ENUM.MOVE_LEFT:
                    if (idx > 0 && p.Controls[idx - 1] is ProjectDatUI otherUp)
                    {
                        ProjectLoaderAnimationHelper.AnimateFlowSwap(
                            p,
                            ui,
                            otherUp,
                            durationMs: ANIM_BASE_MS,
                            onDone: () =>
                            {
                                UpdateAfterSwap(p, ui, otherUp);
                                ui.BeginDropPulse(PULSE_MS);
                                otherUp.BeginDropPulse(PULSE_MS);
                            });
                    }
                    break;

                case ProjectDatUI.ORDER_CHANGE_ENUM.MOVE_RIGHT:
                    if (idx < max && p.Controls[idx + 1] is ProjectDatUI otherDown)
                    {
                        ProjectLoaderAnimationHelper.AnimateFlowSwap(
                            p,
                            ui,
                            otherDown,
                            durationMs: ANIM_BASE_MS,
                            onDone: () =>
                            {
                                UpdateAfterSwap(p, ui, otherDown);
                                ui.BeginDropPulse(PULSE_MS);
                                otherDown.BeginDropPulse(PULSE_MS);
                            });
                    }
                    break;

                case ProjectDatUI.ORDER_CHANGE_ENUM.MOVE_TO_FIRST:
                    if (idx > 0)
                    {
                        ProjectLoaderAnimationHelper.AnimateFlowMoveToIndex(
                            p,
                            ui,
                            targetIndex: 0,
                            durationMs: 260,
                            onDone: () =>
                            {
                                CheckParentSetup(p);
                                ui.BeginDropPulse(PULSE_MS);
                            });
                    }
                    else
                    {
                        ui.BeginDropPulse(PULSE_MS);
                    }
                    break;

                case ProjectDatUI.ORDER_CHANGE_ENUM.MOVE_TO_LAST:
                    if (idx < max)
                    {
                        ProjectLoaderAnimationHelper.AnimateFlowMoveToIndex(
                            p,
                            ui,
                            targetIndex: p.Controls.Count - 1,
                            durationMs: 300,
                            onDone: () =>
                            {
                                CheckParentSetup(p);
                                ui.BeginDropPulse(PULSE_MS);
                            });
                    }
                    else
                    {
                        ui.BeginDropPulse(PULSE_MS);
                    }
                    break;
            }

            try
            {
                if (ui.LastOrderChangeAction != ProjectDatUI.ORDER_CHANGE_ENUM.MOVE_TO_LAST)
                    p.VerticalScroll.Value = 0;
            }
            catch { }
        }

        void OnLoadExpressions(ProjectDatUI ui, Control parentUI)
        {
            lastClickedrojectDatUI = ui;
            LoadExpressionsFileEvt?.Invoke();
        }

        void OnEditExpressions(ProjectDatUI ui, Control parentUI)
        {
            if (ui.GetData() != null)
                EditExpressionsFileEvt?.Invoke(ui!.GetData());
        }

        void buildProjectBtn_Click(object sender, EventArgs e)
        {
            SetCommentEditActive(false);
            SetProjectCommentShowing();

            if (!GetAllDatsHaveOkNames())
                return;

            var project = GetProject();

            if (project == null) return;

            if (project.SoftwareEntries.Count == 0)
            {
                ShowError("Add at least one Software DAT to build the DAT Grouper Project.");
                return;
            }

            var allDatsExist = DatGrouperProjectDTO.GetAllDatsExist(project);
            if (allDatsExist == false)
            {
                ShowError("Dat Grouper cannot be run as one or more DAT files do not exist.");
                return;
            }

            BuildProjectEvt?.Invoke(project);
        }
        private static void EnableDoubleBuffer(Control c)
        {
            // Control.DoubleBuffered is protected; reflection is fine here and avoids a new subclass.
            try
            {
                typeof(Control)
                    .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(c, true, null);
            }
            catch
            {
                // ignore
            }
        }

        private bool GetAllDatsHaveOkNames()
        {
            var project = GetProject();
            if (project == null)
            {
                ShowError("No Project is loaded.");
                return false;
            }

            static string Norm(string? s) => (s ?? string.Empty).Trim().ToLowerInvariant();

            bool CheckSet(IReadOnlyList<DatGrouperProjectEntry> datHeadlines, string setName)
            {
                var seen = new HashSet<(DAT_GROUP_ENUM Group, string Ref, string Internal)>();

                foreach (var headline in datHeadlines)
                {
                    var reference = headline.FriendlyName;

                    if (!DatinateHelper.IsDatFriendlyNameAcceptable(reference))
                    {
                        ShowError(
                            $"The Quick Reference Name '{reference}' in '{setName}' is not in the correct format." +
                            "Please ensure the Reference is alphanumeric and brief (underscores are allowed)."
                            );

                        return false;
                    }

                    var internalDesc = headline.InternalDescriptor;

                    var key = (headline.DatGroupEnum, Norm(reference), Norm(internalDesc));

                    if (!seen.Add(key))
                    {
                        ShowError(
                            $"Two or more DAT items in '{setName}' have the same Dat Group, Reference, and Internal Descriptor. Ensure the combination is unique within that set."
                            + Environment.NewLine + Environment.NewLine
                            + $"Group: '{headline.DatGroupEnum}'"
                            + Environment.NewLine
                            + $"Reference: '{(reference ?? string.Empty).Trim()}'"
                            + Environment.NewLine
                            + $"Internal: '{(internalDesc ?? string.Empty).Trim()}'"
                        );
                        return false;
                    }
                }

                return true;
            }

            if (!CheckSet(project.SoftwareEntries, "Game DATs")) return false;
            if (!CheckSet(project.AuxEntries, "Media DATs")) return false;
            
            return true;
        }


        void saveBtn_Click(object sender, EventArgs e)
        {
            if (!GetAllDatsHaveOkNames())
                return;

            var project = GetProject();
            if (project == null)
                return;

            string newName = projectNameText.Text.Trim();

            if (string.IsNullOrWhiteSpace(newName))
                newName = currentProject?.ProjectName?.Trim() ?? string.Empty;

            if (isCreatingNewProject)
            {
                foreach (KeyValuePair<string, DatGrouperProjectDTO> pair in projectModel)
                {
                    if (string.Equals(newName, pair.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        ShowError("The new Project Name must be unique. The Project Name '" + newName + "' is already in use.");
                        return;
                    }
                }
            }

            if (Path.HasExtension(newName))
            {
                ShowError("The Project Name cannot contain an extension.");
                return;
            }

            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                ShowError("The Project Name is not valid.");
                return;
            }
            if (string.IsNullOrWhiteSpace(newName))
            {
                ShowError("Enter a Project Name to Save.");
                return;
            }

            SaveProjectEvt?.Invoke(project);
        }

        public void ReloadCurrentProject()
        {
            Ui(() =>
            {
                if (isCreatingNewProject)
                {
                    LoadNewProject();
                    return;
                }

                if (projectListBox.SelectedItem is not string projectName ||
                    !projectModel.TryGetValue(projectName, out var project))
                {
                    BeginNewProjectMode();
                    return;
                }

                LoadProject(project);
            });
        }

        private void Ui(Action action)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(action);
                return;
            }

            action();
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(
                msg
                , "There was a Problem"
                , MessageBoxButtons.OK
                , MessageBoxIcon.Exclamation);
        }

        private void ConfigureProjectBrowser()
        {
            projectListBox.DrawMode = DrawMode.OwnerDrawFixed;
            projectListBox.ItemHeight = 34;
            projectListBox.IntegralHeight = false;
            projectListBox.BorderStyle = BorderStyle.None;
            projectListBox.BackColor = Color.White;
            projectListBox.ForeColor = Color.FromArgb(25, 25, 25);
            projectListBox.Cursor = Cursors.Default;
            projectNameText.Enabled = false;
            createProjectPanel.Visible = false;

            projectListBox.Resize -= projectListBox_Resize;
            projectListBox.Resize += projectListBox_Resize;

            projectListBox.MouseDown -= projectListBox_MouseDown;
            projectListBox.MouseDown += projectListBox_MouseDown;
        }
        private void projectListBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || ignoreProjectSelectionChange || isCreatingNewProject)
                return;

            int index = projectListBox.IndexFromPoint(e.Location);
            if (index < 0 || index >= projectListBox.Items.Count)
                return;

            string? clickedProjectName = projectListBox.Items[index]?.ToString();
            if (string.IsNullOrWhiteSpace(clickedProjectName))
                return;

            if (!string.Equals(currentProject?.ProjectName, clickedProjectName, StringComparison.Ordinal))
                return;

            if (!projectModel.TryGetValue(clickedProjectName, out var project))
                return;

            LoadingProjectEvt?.Invoke();
            LoadProject(project);
        }
        private void projectListBox_Resize(object? sender, EventArgs e)
        {
            EnsureSelectedProjectVisibleDeferred();
        }
        private void UpdateProjectBrowserState()
        {
            bool hasProjects = projectListBox.Items.Count > 0;
            bool commentsEnabled = !isCreatingNewProject;

            projectListBox.Enabled = hasProjects && !isCreatingNewProject;
            projectListHostPanel.BackColor = isCreatingNewProject
                ? Color.FromArgb(224, 224, 224)
                : Color.FromArgb(205, 212, 221);

            newProjectBtn.Enabled = !isCreatingNewProject;
            projectListBox.Visible = !isCreatingNewProject;
            createProjectPanel.Visible = isCreatingNewProject;

            if (isCreatingNewProject)
                createProjectPanel.BringToFront();
            else
                projectListBox.BringToFront();
            cancelNewProjectBtn.Enabled = hasProjects;

            commentText.Enabled = commentsEnabled;
            commentText.ReadOnly = true;
            editCommentBtn.Enabled = commentsEnabled;
            applyCommentEditBtn.Enabled = false;
            cancelCommentEditBtn.Enabled = false;
            commentGroup.Enabled = true;

            UpdateCurrentProjectNameLabel();
            EnsureSelectedProjectVisibleDeferred();
            projectListBox.Invalidate();
        }
        private bool SelectProjectInList(string projectName)
        {
            for (int i = 0; i < projectListBox.Items.Count; i++)
            {
                if (string.Equals(projectListBox.Items[i]?.ToString(), projectName, StringComparison.Ordinal))
                {
                    projectListBox.SelectedIndex = i;
                    EnsureSelectedProjectVisibleDeferred();
                    return true;
                }
            }

            return false;
        }
        private void BeginNewProjectMode()
        {
            projectNameBeforeNewMode = currentProject?.ProjectName;
            isCreatingNewProject = true;

            LoadNewProject();

            activeCommentDatUI = null;
            commentLabel.Text = GetProjectCommentName();
            commentText.Text = string.Empty;

            ignoreProjectSelectionChange = true;
            try
            {
                projectListBox.ClearSelected();
            }
            finally
            {
                ignoreProjectSelectionChange = false;
            }

            UpdateProjectBrowserState();
            projectNameText.Focus();
            projectNameText.SelectAll();
        }
        private void ExitNewProjectMode()
        {
            isCreatingNewProject = false;

            SetCommentEditActive(false);
            SetProjectCommentShowing();
            UpdateProjectBrowserState();
        }

        private void projectListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ignoreProjectSelectionChange || isCreatingNewProject)
                return;

            if (projectListBox.SelectedItem is not string projectName ||
                !projectModel.TryGetValue(projectName, out var project))
                return;

            if (string.Equals(currentProject?.ProjectName, projectName, StringComparison.Ordinal))
                return;

            LoadingProjectEvt?.Invoke();
            LoadProject(project);
        }

        private void newProjectBtn_Click(object sender, EventArgs e)
        {
            BeginNewProjectMode();
        }

        private void cancelNewProjectBtn_Click(object sender, EventArgs e)
        {
            if (!isCreatingNewProject)
                return;

            ExitNewProjectMode();

            if (!string.IsNullOrWhiteSpace(projectNameBeforeNewMode) && SelectProjectInList(projectNameBeforeNewMode))
                return;
        }

        private void projectListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0 || e.Index >= projectListBox.Items.Count)
                return;

            var bounds = e.Bounds;
            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var hovered = e.Index == hoveredProjectIndex && !isCreatingNewProject;

            Color backColor = Color.White;
            Color textColor = Color.FromArgb(24, 24, 24);
            Color accentColor = Color.FromArgb(35, 114, 198);

            if (selected)
            {
                backColor = Color.FromArgb(226, 238, 252);
                textColor = Color.FromArgb(16, 52, 92);
            }
            else if (hovered)
            {
                backColor = Color.FromArgb(242, 247, 252);
            }
            else if ((e.Index & 1) == 1)
            {
                backColor = Color.FromArgb(249, 250, 252);
            }

            using var backBrush = new SolidBrush(backColor);
            e.Graphics.FillRectangle(backBrush, bounds);

            using (var textBrush = new SolidBrush(textColor))
            using (var font = new Font(Font, selected ? FontStyle.Bold : FontStyle.Regular))
            {
                var textRect = Rectangle.Inflate(bounds, -12, 0);
                TextRenderer.DrawText(
                    e.Graphics,
                    projectListBox.Items[e.Index]?.ToString() ?? string.Empty,
                    font,
                    textRect,
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            if (selected)
            {
                using var accentBrush = new SolidBrush(accentColor);
                e.Graphics.FillRectangle(accentBrush, bounds.Left, bounds.Top, 4, bounds.Height);
            }
            else
            {
                using var dividerPen = new Pen(Color.FromArgb(232, 236, 241));
                e.Graphics.DrawLine(dividerPen, bounds.Left + 8, bounds.Bottom - 1, bounds.Right - 8, bounds.Bottom - 1);
            }

            e.DrawFocusRectangle();
        }

        private void projectListBox_MouseMove(object sender, MouseEventArgs e)
        {
            int index = projectListBox.IndexFromPoint(e.Location);
            projectListBox.Cursor = index >= 0 ? Cursors.Hand : Cursors.Default;

            if (hoveredProjectIndex == index)
                return;

            int oldIndex = hoveredProjectIndex;
            hoveredProjectIndex = index;

            if (oldIndex >= 0)
                projectListBox.Invalidate(projectListBox.GetItemRectangle(oldIndex));
            if (hoveredProjectIndex >= 0)
                projectListBox.Invalidate(projectListBox.GetItemRectangle(hoveredProjectIndex));
        }

        private void projectListBox_MouseLeave(object sender, EventArgs e)
        {
            projectListBox.Cursor = Cursors.Default;

            if (hoveredProjectIndex < 0)
                return;

            int oldIndex = hoveredProjectIndex;
            hoveredProjectIndex = -1;
            projectListBox.Invalidate(projectListBox.GetItemRectangle(oldIndex));
        }
        void LoadNewProject()
        {
            ClearView();
            currentProject = null;
            projectNameText.Text = string.Empty;
            projectNameText.Enabled = true;
            UpdateCurrentProjectNameLabel();
            EnsureSelectedProjectVisibleDeferred();
        }
        void LoadProject(DatGrouperProjectDTO projectVO)
        {
            SuspendLayout();
            gamesIncludeContainer.SuspendLayout();
            mediaIncludeContainer.SuspendLayout();

            try
            {
                ClearView();

                currentProject = projectVO;
                ExitNewProjectMode();

                projectNameText.Enabled = false;
                projectNameText.Text = projectVO.ProjectName;

                AddDatRange(projectVO.SoftwareEntries, DAT_GROUP_TARGET_ENUM.GAMES_INCLUDE_GROUP);
                AddDatRange(projectVO.AuxEntries, DAT_GROUP_TARGET_ENUM.MEDIA_INCLUDE_GROUP);

                DatGrouperProjectEntry[] allVOs = DatGrouperProjectDTO.GetAllProjectEntries(projectVO);

                for (int i = 0; i < allVOs.Length; i++)
                    if (!File.Exists(allVOs[i].DatFullpath))
                        break;

                projectComment = commentText.Text =
                    string.IsNullOrWhiteSpace(projectVO.Comment)
                        ? string.Empty
                        : projectVO.Comment;
            }
            finally
            {
                mediaIncludeContainer.ResumeLayout(true);
                gamesIncludeContainer.ResumeLayout(true);
                ResumeLayout(true);
            }

            UpdateCurrentProjectNameLabel();
            EnsureSelectedProjectVisibleDeferred();

            var project = GetProject();
            if (project == null) return;

            HighlightEvt?.Invoke(project);
            DatFullpathsChangedEvt?.Invoke(project);
        }
        private void highlightBtn_Click(object sender, EventArgs e)
        {
            if (currentProject != null)
                HighlightEvt?.Invoke(currentProject);
        }

        private void familyTreeBtn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SetCommentEditActive(false);
            SetProjectCommentShowing();
            ShowTreeViewEvt?.Invoke(projectNameText.Text);
        }

        private void cancelCommentEditBtn_Click(object sender, EventArgs e)
        {
            SetProjectCommentShowing();
            activeCommentDatUI = null;
            SetCommentEditActive(false);
        }

        private void applyCommentEditBtn_Click(object sender, EventArgs e)
        {

            if (activeCommentDatUI != null)
            {
                activeCommentDatUI.SetComment(commentText.Text.Trim());
            }
            else
            {
                projectComment = commentText.Text.Trim();
            }
            activeCommentDatUI = null;

            SetCommentEditActive(false);
            SetProjectCommentShowing();
        }

        private void editCommentBtn_Click(object sender, EventArgs e)
        {
            SetProjectCommentShowing();
            SetCommentEditActive(true);
        }

        private void OnDatCommentEdit(ProjectDatUI ui, Control parentUI)
        {
            if (!commentText.ReadOnly) return;

            if (!string.IsNullOrWhiteSpace(ui.GetComment())) commentText.Text = ui.GetComment();
            else commentText.Text = string.Empty;

            activeCommentDatUI = ui;
            commentLabel.Text = ui.GetCommentName();
            SetCommentEditActive(true);
        }


        private void OnDatCommentPreviewStart(ProjectDatUI ui, Control parentUI)
        {
            if (!commentText.ReadOnly) return;

            commentText.Text = string.IsNullOrWhiteSpace(ui.GetComment())
                ? string.Empty
                : ui.GetComment();

            commentLabel.Text = ui.GetCommentName();
        }
        private void OnDatCommentPreviewEnd(ProjectDatUI ui, Control paremntUI)
        {
            if (!commentText.ReadOnly) return;
            commentLabel.Text = GetProjectCommentName();
            commentText.Text = string.IsNullOrWhiteSpace(projectComment) ? string.Empty : projectComment;
        }


        private void SetCommentEditActive(bool isActive)
        {
            applyCommentEditBtn.Enabled = isActive;
            cancelCommentEditBtn.Enabled = isActive;
            editCommentBtn.Enabled = !isActive;
            commentText.ReadOnly = !isActive;
        }
        private string GetProjectCommentName()
        {
            return "Project Level Comment";
        }
        private void SetProjectCommentShowing()
        {
            activeCommentDatUI = null;
            commentLabel.Text = GetProjectCommentName();
            commentText.Text = string.IsNullOrWhiteSpace(projectComment) ? string.Empty : projectComment;
        }


        private FlowLayoutPanel? GetContainer(DAT_GROUP_TARGET_ENUM datGroupTargetEnum)
        {
            return datGroupTargetEnum switch
            {
                DAT_GROUP_TARGET_ENUM.GAMES_INCLUDE_GROUP => gamesIncludeContainer,
                DAT_GROUP_TARGET_ENUM.MEDIA_INCLUDE_GROUP => mediaIncludeContainer,
                DAT_GROUP_TARGET_ENUM.RESOURCE_INCLUDE_GROUP => mediaIncludeContainer,
                _ => null
            };
        }

        private ProjectDatUI CreateDatUI(DatGrouperProjectEntry datHeadline)
        {
            ProjectDatUI datUI = new ProjectDatUI();

            datUI.RemovedEvt += OnDatRemoved;
            datUI.CommentPreviewStartEvt += OnDatCommentPreviewStart;
            datUI.CommentPreviewEndEvt += OnDatCommentPreviewEnd;
            datUI.CommentEditEvt += OnDatCommentEdit;
            datUI.OrderChangeEvt += OnUiOrderChange;
            datUI.EditExpressionsEvt += OnEditExpressions;
            datUI.LoadExpressionsEvt += OnLoadExpressions;

            datUI.SetUI(datHeadline);

            datUIs.Add(datUI);

            return datUI;
        }

        private void AddDatCore(
            DatGrouperProjectEntry datHeadline,
            DAT_GROUP_TARGET_ENUM datGroupTargetEnum,
            bool forceAddAsFirst,
            bool deferParentSetup)
        {
            FlowLayoutPanel? container = GetContainer(datGroupTargetEnum);
            if (container == null)
                return;

            ProjectDatUI datUI = CreateDatUI(datHeadline);

            container.Controls.Add(datUI);

            if (forceAddAsFirst)
            {
                container.Controls.SetChildIndex(datUI, 0);
                container.ScrollControlIntoView(datUI);
            }

            if (dragManagers.TryGetValue(container, out var mgr))
                mgr.Register(datUI);

            if (!deferParentSetup)
                CheckParentSetup(container);
        }

        private void AddDatRange(
            IReadOnlyList<DatGrouperProjectEntry> entries,
            DAT_GROUP_TARGET_ENUM datGroupTargetEnum)
        {
            if (entries == null || entries.Count == 0)
                return;

            FlowLayoutPanel? container = GetContainer(datGroupTargetEnum);
            if (container == null)
                return;

            container.SuspendLayout();
            try
            {
                for (int i = 0; i < entries.Count; i++)
                    AddDatCore(entries[i], datGroupTargetEnum, false, true);

                CheckParentSetup(container);
            }
            finally
            {
                container.ResumeLayout(true);
            }
        }

        private void DisposeDatUI(ProjectDatUI datUI)
        {
            datUI.RemovedEvt -= OnDatRemoved;
            datUI.CommentPreviewStartEvt -= OnDatCommentPreviewStart;
            datUI.CommentPreviewEndEvt -= OnDatCommentPreviewEnd;
            datUI.CommentEditEvt -= OnDatCommentEdit;
            datUI.OrderChangeEvt -= OnUiOrderChange;
            datUI.EditExpressionsEvt -= OnEditExpressions;
            datUI.LoadExpressionsEvt -= OnLoadExpressions;

            datUIs.Remove(datUI);

            if (datUI.Parent is FlowLayoutPanel p && dragManagers.TryGetValue(p, out var mgr))
                mgr.Unregister(datUI);

            datUI.Dispose();
        }
        private void projectNameText_TextChanged(object? sender, EventArgs e)
        {
            UpdateCurrentProjectNameLabel();
        }
        private void UpdateCurrentProjectNameLabel()
        {
            string text;

            if (isCreatingNewProject)
            {
                string pendingName = projectNameText.Text.Trim();

                text = string.IsNullOrWhiteSpace(pendingName)
                    ? "New Project"
                    : "New Project\r\n" + pendingName;
            }
            else if (currentProject != null && !string.IsNullOrWhiteSpace(currentProject.ProjectName))
            {
                text = currentProject.ProjectName;
            }
            else
            {
                text = "No Project Loaded";
            }

            if (!string.Equals(currentProjectNameLabel.Text, text, StringComparison.Ordinal))
                currentProjectNameLabel.Text = text;
        }
        private void EnsureSelectedProjectVisible()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            if (projectListBox.IsDisposed || !projectListBox.IsHandleCreated)
                return;

            if (!projectListBox.Visible || projectListBox.Items.Count == 0)
                return;

            int selectedIndex = projectListBox.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= projectListBox.Items.Count)
                return;

            int itemHeight = Math.Max(1, projectListBox.ItemHeight);
            int visibleCount = Math.Max(1, projectListBox.ClientSize.Height / itemHeight);
            int topIndex = projectListBox.TopIndex;
            int bottomIndex = topIndex + visibleCount - 1;

            int targetTopIndex = topIndex;

            if (selectedIndex < topIndex)
            {
                targetTopIndex = selectedIndex;
            }
            else if (selectedIndex > bottomIndex)
            {
                targetTopIndex = selectedIndex - visibleCount + 1;
            }
            else
            {
                return;
            }

            int maxTopIndex = Math.Max(0, projectListBox.Items.Count - visibleCount);
            targetTopIndex = Math.Max(0, Math.Min(targetTopIndex, maxTopIndex));

            if (projectListBox.TopIndex != targetTopIndex)
            {
                projectListBox.TopIndex = targetTopIndex;
                projectListBox.Invalidate();
            }
        }
        private void EnsureSelectedProjectVisibleDeferred()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            if (projectListBox.IsDisposed || !projectListBox.IsHandleCreated)
                return;

            projectListBox.BeginInvoke((Action)(() =>
            {
                if (IsDisposed || !IsHandleCreated)
                    return;

                if (projectListBox.IsDisposed || !projectListBox.IsHandleCreated)
                    return;

                EnsureSelectedProjectVisible();
            }));
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            EnsureSelectedProjectVisibleDeferred();
        }
    }
}
