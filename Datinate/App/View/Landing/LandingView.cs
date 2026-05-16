using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using com.RADIO.Datinate.view.datPaths.ui;
using Datinate.Shared;

namespace datinate.app
{
    public partial class LandingView : UserControl, ILandingView
    {
        public event Action<DatRootDTO[], string>? LoadDatManagerEvt;
        public event Action<DatRootDTO[], string>? SaveDatRootPathsEvt;
        public event Action? RootDatPathRemovedEvt;
        public event Action? DatRootPathAddedEvt;
        public event Action<string?>? LoadDatGrouperViewEvt;
        private const int PathUiGapPx = 6;
        private const int PathUiInsetPx = 6;

        private const int ProjectUiGapPx = 6;
        private const int ProjectUiInsetPx = 6;
        private const int DisabledLoadBackColorValue = 210;
        private const int DisabledLoadBorderColorValue = 170;
        private const int DisabledLoadForeColorValue = 128;

        private DatGrouperProjectListItem? selectedProjectUi;
        private bool ensureSelectedProjectVisibleQueued;

        public LandingView()
        {
            InitializeComponent();
            ApplyVisualTheme();

            pathsContainer.Resize += onPathsContainerResize;
            datGrouperContainer.Resize += onDatGrouperContainerResize;
            loadDatGrouperProjectBtn.Click += onLoadDatGrouperProjectClick;
            newDatGrouperProjectBtn.Click += onNewDatGrouperProjectClick;

            UpdateDatGrouperLoadState();
            Facade.RegisterActor(this);
        }

        public void SetDatGrouperProjects(DatGrouperProjectDTO[] projectVOs)
        {
            Ui(() =>
            {
                ClearDatGrouperProjectUIs();
                selectedProjectUi = null;

                foreach (var project in projectVOs)
                {
                    AddDatGrouperProjectUI(project.ProjectName);
                }

                RelayoutDatGrouperProjectUIs();
                UpdateDatGrouperLoadState();
            });
        }

        public void SetDatRootPaths(DatRootDTO[] paths, string mameHashPath)
        {
            if (pathsContainer.InvokeRequired)
            {
                pathsContainer.Invoke(new Action(() => SetDatRootPaths(paths, mameHashPath)));
                return;
            }

            mameHashTextBox.Text = mameHashPath;

            var toRemove = new List<DatPathUI>();

            for (int i = 0; i < pathsContainer.Controls.Count; i++)
            {
                if (pathsContainer.Controls[i] is DatPathUI ui)
                    toRemove.Add(ui);
            }

            foreach (var item in toRemove)
            {
                item.removePathEvent -= onRemovePathUIPress;
                item.Parent?.Controls.Remove(item);
                item.Dispose();
            }

            foreach (var item in paths)
                AddDatRootPathUI(item.RootPath, item.ReferenceName);
        }

        public void ActivateView()
        {
            Ui(() =>
            {
                mainLayoutPanel.Visible = true;
                RelayoutPathUIs();
                RelayoutDatGrouperProjectUIs();
                EnsureSelectedProjectVisibleDeferred();
                UpdateDatGrouperLoadState();
            });
        }

        private DatRootDTO[] GetDatRootPaths()
        {
            List<DatRootDTO> list = new List<DatRootDTO>();

            for (int i = 0; i < pathsContainer.Controls.Count; i++)
            {
                if (pathsContainer.Controls[i] is DatPathUI ui)
                {
                    list.Add(
                        new DatRootDTO(ui.GetPath().Trim(), ui.GetReferenceName().Trim()));
                }
            }

            return list.ToArray();
        }

        /**
         * Ensure references will make sense in tables later on
         */
        private bool GetAllReferencesAreOK()
        {
            if (string.IsNullOrWhiteSpace(mameHashTextBox.Text) || Directory.Exists(mameHashTextBox.Text) == false)
            {
                MessageBox.Show(
                    "Cannot proceed. Please ensure the MAME Software List DAT folder (MAME hash folder) is set. ", 
                    "Attention");

                return false;
            }

            var dtos = GetDatRootPaths();

            if (dtos.Length == 0)
            {
                MessageBox.Show("Cannot proceed. Ensure at least one DAT Root Path Exists.", "Attention");
                return false;
            }

            HashSet<string> referenceNames = new HashSet<string>();
            HashSet<string> paths = new HashSet<string>();

            foreach (var dto in dtos)
            {
                if (string.IsNullOrWhiteSpace(dto.ReferenceName))
                {
                    MessageBox.Show("Cannot proceed. Ensure all DAT Root Paths have a Reference Name.", "Attention");
                    return false;
                }
                if (referenceNames.Add(dto.ReferenceName.Trim().ToLower()) == false)
                {
                    MessageBox.Show("Cannot proceed. Ensure all DAT Root Paths have unique Reference Names.", "Attention");
                    return false;
                }
                if (paths.Add(dto.RootPath.Trim().ToLower()) == false)
                {
                    MessageBox.Show("Cannot proceed. Ensure all DAT Root Paths are unique.", "Attention");
                    return false;
                }
            }
            return true;
        }

        private void onAddPath(object sender, EventArgs e)
        {
            DialogResult result = folderBrowser.ShowDialog();

            if (result == DialogResult.OK)
            {
                var datRoot = folderBrowser.SelectedPath;
                AddDatRootPathUI(datRoot, string.Empty);
                DatRootPathAddedEvt?.Invoke();

                loadDatManagerBtn.Enabled = true;
            }
        }


        protected void AddDatRootPathUI(string path, string referenceName)
        {
            DatPathUI ui = new DatPathUI();

            string[] arr = path.Split('\\');
            string friendlyName = arr[arr.Length - 1];

            // NOTE: Last check in case a path like 'C:\' is chosen:
            if (friendlyName == "")
                friendlyName = path.Split(':')[0];

            ui.SetUI(path, referenceName);
            ui.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            ui.Width = GetPathsUiWidth();

            pathsContainer.Controls.Add(ui);

            ui.removePathEvent += new EventHandler(onRemovePathUIPress);
            RelayoutPathUIs();
        }

        /**
         * Remove a path - immediately invalidate what is currently loaded
         */
        private void onRemovePathUIPress(object? sender, EventArgs e)
        {
            DatPathUI? ui = sender as DatPathUI;
            if (ui == null)
                return;

            // remove evt listener so garbage collection can cick in.
            ui.removePathEvent -= onRemovePathUIPress;
            pathsContainer.Controls.Remove(ui);
            ui.Dispose();
            RelayoutPathUIs();

            RootDatPathRemovedEvt?.Invoke();
        }

        /**
         * Unfortunately we have to manually relayout our path UIs
         * on each add and remove operation
         */
        protected void RelayoutPathUIs()
        {
            int availableWidth = GetPathsUiWidth();
            int y = PathUiInsetPx;

            for (int i = 0; i < pathsContainer.Controls.Count; i++)
            {
                Control ui = pathsContainer.Controls[i];
                ui.Width = availableWidth;
                ui.Location = new Point(PathUiInsetPx, y);
                y += ui.Height + PathUiGapPx;
            }

            pathsContainer.AutoScrollMinSize = new Size(0, y + PathUiInsetPx);

            if (pathsContainer.Controls.Count == 0)
                loadDatManagerBtn.Enabled = false;
            else
                loadDatManagerBtn.Enabled = true;
        }

        private void AddDatGrouperProjectUI(string projectName)
        {
            DatGrouperProjectListItem ui = new DatGrouperProjectListItem
            {
                ProjectName = projectName,
                Width = GetDatGrouperUiWidth(),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };

            ui.Click += onDatGrouperProjectUiClick;
            ui.DoubleClick += onDatGrouperProjectUiDoubleClick;

            datGrouperContainer.Controls.Add(ui);
        }

        private void ClearDatGrouperProjectUIs()
        {
            var toRemove = new List<DatGrouperProjectListItem>();

            for (int i = 0; i < datGrouperContainer.Controls.Count; i++)
            {
                if (datGrouperContainer.Controls[i] is DatGrouperProjectListItem ui)
                    toRemove.Add(ui);
            }

            foreach (var item in toRemove)
            {
                item.Click -= onDatGrouperProjectUiClick;
                item.DoubleClick -= onDatGrouperProjectUiDoubleClick;
                item.Parent?.Controls.Remove(item);
                item.Dispose();
            }
        }

        private void RelayoutDatGrouperProjectUIs()
        {
            int availableWidth = GetDatGrouperUiWidth();
            int y = ProjectUiInsetPx;

            for (int i = 0; i < datGrouperContainer.Controls.Count; i++)
            {
                Control ui = datGrouperContainer.Controls[i];
                ui.Width = availableWidth;
                ui.Location = new Point(ProjectUiInsetPx, y);
                y += ui.Height + ProjectUiGapPx;
            }

            datGrouperContainer.AutoScrollMinSize = new Size(0, y + ProjectUiInsetPx);
        }

        private void onDatGrouperProjectUiClick(object? sender, EventArgs e)
        {
            if (sender is not DatGrouperProjectListItem clickedUi)
                return;

            SelectDatGrouperProject(clickedUi);
        }

        private void onDatGrouperProjectUiDoubleClick(object? sender, EventArgs e)
        {
            if (sender is not DatGrouperProjectListItem clickedUi)
                return;

            SelectDatGrouperProject(clickedUi);
            InvokeSelectedDatGrouperProject();
        }

        private void SelectDatGrouperProject(DatGrouperProjectListItem selectedUi)
        {
            selectedProjectUi = selectedUi;

            for (int i = 0; i < datGrouperContainer.Controls.Count; i++)
            {
                if (datGrouperContainer.Controls[i] is DatGrouperProjectListItem ui)
                    ui.IsSelected = ReferenceEquals(ui, selectedUi);
            }

            EnsureSelectedProjectVisibleDeferred();
            UpdateDatGrouperLoadState();
        }

        private void UpdateDatGrouperLoadState()
        {
            loadDatGrouperProjectBtn.Enabled = selectedProjectUi != null;
        }

        private void InvokeSelectedDatGrouperProject()
        {
            if (selectedProjectUi == null)
                return;

            LoadDatGrouperViewEvt?.Invoke(selectedProjectUi.ProjectName);
        }

        private void EnsureSelectedProjectVisibleDeferred()
        {
            if (ensureSelectedProjectVisibleQueued)
                return;

            if (selectedProjectUi == null || selectedProjectUi.IsDisposed)
                return;

            if (datGrouperContainer.IsDisposed || !datGrouperContainer.IsHandleCreated)
                return;

            ensureSelectedProjectVisibleQueued = true;

            BeginInvoke(new Action(() =>
            {
                ensureSelectedProjectVisibleQueued = false;

                if (IsDisposed || !IsHandleCreated)
                    return;

                if (datGrouperContainer.IsDisposed || !datGrouperContainer.IsHandleCreated)
                    return;

                if (selectedProjectUi == null || selectedProjectUi.IsDisposed || selectedProjectUi.Parent != datGrouperContainer)
                    return;

                datGrouperContainer.ScrollControlIntoView(selectedProjectUi);
            }));
        }

        private void onLoadDatManagerClick(object sender, EventArgs e)
        {
            if (GetAllReferencesAreOK())
                LoadDatManagerEvt?.Invoke(
                    GetDatRootPaths(),
                    mameHashTextBox.Text.Trim());
        }

        private void onSaveDatRootsClick(object sender, EventArgs e)
        {
            if (GetAllReferencesAreOK())
                SaveDatRootPathsEvt?.Invoke(
                    GetDatRootPaths(),
                    mameHashTextBox.Text.Trim());
        }

        private void onLoadDatGrouperProjectClick(object? sender, EventArgs e)
            => InvokeSelectedDatGrouperProject();

        private void onNewDatGrouperProjectClick(object? sender, EventArgs e)
            => LoadDatGrouperViewEvt?.Invoke(null);

        private void onPathsContainerResize(object? sender, EventArgs e)
            => RelayoutPathUIs();

        private void onDatGrouperContainerResize(object? sender, EventArgs e)
        {
            RelayoutDatGrouperProjectUIs();
            EnsureSelectedProjectVisibleDeferred();
        }

        private int GetPathsUiWidth()
        {
            int width = pathsContainer.ClientSize.Width - (PathUiInsetPx * 2);

            if (pathsContainer.VerticalScroll.Visible)
                width -= SystemInformation.VerticalScrollBarWidth;

            return Math.Max(40, width);
        }

        private int GetDatGrouperUiWidth()
        {
            int width = datGrouperContainer.ClientSize.Width - (ProjectUiInsetPx * 2);

            if (datGrouperContainer.VerticalScroll.Visible)
                width -= SystemInformation.VerticalScrollBarWidth;

            return Math.Max(40, width);
        }

        private void ApplyVisualTheme()
        {
            datRootsPanel.BackColor = Color.FromArgb(246, 246, 246);
            panel2.BackColor = Color.FromArgb(246, 246, 246);

            pathsContainer.BackColor = Color.FromArgb(246, 241, 241);
            datGrouperContainer.BackColor = Color.FromArgb(241, 246, 249);

            label1.ForeColor = Color.FromArgb(72, 72, 72);
            label2.ForeColor = Color.FromArgb(72, 72, 72);

            loadDatManagerBtn.DisabledBackColor = Color.FromArgb(DisabledLoadBackColorValue, DisabledLoadBackColorValue, DisabledLoadBackColorValue);
            loadDatManagerBtn.DisabledBorderColor = Color.FromArgb(DisabledLoadBorderColorValue, DisabledLoadBorderColorValue, DisabledLoadBorderColorValue);
            loadDatManagerBtn.DisabledForeColor = Color.FromArgb(DisabledLoadForeColorValue, DisabledLoadForeColorValue, DisabledLoadForeColorValue);

            loadDatGrouperProjectBtn.DisabledBackColor = Color.FromArgb(DisabledLoadBackColorValue, DisabledLoadBackColorValue, DisabledLoadBackColorValue);
            loadDatGrouperProjectBtn.DisabledBorderColor = Color.FromArgb(DisabledLoadBorderColorValue, DisabledLoadBorderColorValue, DisabledLoadBorderColorValue);
            loadDatGrouperProjectBtn.DisabledForeColor = Color.FromArgb(DisabledLoadForeColorValue, DisabledLoadForeColorValue, DisabledLoadForeColorValue);
        }

        private void Ui(Action action)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated)
                    return;

                BeginInvoke(action);
                return;
            }

            action();
        }

        protected void HandleDisposing()
        {
            pathsContainer.Resize -= onPathsContainerResize;
            datGrouperContainer.Resize -= onDatGrouperContainerResize;
            loadDatGrouperProjectBtn.Click -= onLoadDatGrouperProjectClick;
            newDatGrouperProjectBtn.Click -= onNewDatGrouperProjectClick;

            for (int i = 0; i < pathsContainer.Controls.Count; i++)
            {
                if (pathsContainer.Controls[i] is DatPathUI ui)
                    ui.removePathEvent -= onRemovePathUIPress;
            }

            ClearDatGrouperProjectUIs();
            Facade.UnregisterActor(this);
        }

        private void setMameHashBtn_Click(object sender, EventArgs e)
        {
            string existingPath = mameHashTextBox.Text.Trim();

            if (Directory.Exists(existingPath))
                folderBrowser.SelectedPath = existingPath;

            while (true)
            {
                DialogResult result = folderBrowser.ShowDialog();

                if (result != DialogResult.OK)
                    return;

                string selectedPath = folderBrowser.SelectedPath?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(selectedPath) || !Directory.Exists(selectedPath))
                    return;

                bool containsXml;
                try
                {
                    containsXml = Directory.EnumerateFiles(selectedPath, "*.xml", SearchOption.TopDirectoryOnly).Any();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to inspect MAME hash folder: " + ex);
                    MessageBox.Show(
                        "Unable to inspect the selected folder. Please try again.",
                        "Attention");
                    continue;
                }

                if (!containsXml)
                {
                    MessageBox.Show(
                        "This folder does not contain any MAME Software List DATs. Please select the correct hash folder.",
                        "Attention");
                    continue;
                }

                mameHashTextBox.Text = selectedPath;
                return;
            }
        }
    }
}