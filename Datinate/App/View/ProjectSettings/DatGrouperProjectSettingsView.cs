using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using System.Collections.Frozen;
using System.Diagnostics;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public partial class DatGrouperProjectSettingsView : UserControl, IDatGrouperProjectSettingsView
    {
        public event Action<IReadOnlyList<DatGrouperProjectEntry>>? ContentPathsChanged;
        public event Action<DatGrouperProjectDTO>? SaveEvt;
        public event Action? BackEvt;
        public event Action<int>? LoadingPercentEvt;
        public event Action? ClearLoadingEvt;

        public const int AuxRowHeightPx = 48;
        private const int MaxContentWidth = 1200;

        private DatGrouperProjectDTO? currentProject = null;

        public DatGrouperProjectSettingsView() 
        { 
            InitializeComponent(); 
            Facade.RegisterActor(this); 
            BackColor = SystemColors.Control; 
            bodyPanel.BackColor = SystemColors.Control; 
            Resize += OnResize; 
        }

        private void OnResize(object? sender, EventArgs e)
        {
            if (autoSizeContainer.ColumnCount < 3 || autoSizeContainer.ColumnStyles.Count < 3)
                return;

            int targetWidth = Math.Min(ClientSize.Width, MaxContentWidth);

            autoSizeContainer.SuspendLayout();
            rootLayout.SuspendLayout();

            try
            {
                autoSizeContainer.ColumnStyles[1].SizeType = SizeType.Absolute;
                autoSizeContainer.ColumnStyles[1].Width = targetWidth;

                rootLayout.Width = targetWidth;
                rootLayout.Height = autoSizeContainer.ClientSize.Height;
            }
            finally
            {
                rootLayout.ResumeLayout(true);
                autoSizeContainer.ResumeLayout(true);
            }
        }

        public void ClearView()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => ClearView()));
                return;
            }

            SuspendLayout();
            rootLayout.SuspendLayout();
            bodyPanel.SuspendLayout();
            bodyStackPanel.SuspendLayout();
            sectionsLayout.SuspendLayout();

            try
            {
                mediaDragDropUI.ClearUI();
                descriptorsDragDropUI.ClearUI();
                ClearSection(softwarePanel);
                ClearSection(auxPanel);
                ClearSection(supportPanel);
            }
            finally
            {
                sectionsLayout.ResumeLayout(true);
                bodyStackPanel.ResumeLayout(true);
                bodyPanel.ResumeLayout(true);
                rootLayout.ResumeLayout(true);
                ResumeLayout(true);
            }

            PerformLayout();
            Invalidate(true);
            Update();
        }
        public void SetView(DatGrouperProjectDTO project, IReadOnlySet<DescriptorDefinitionDTO> descriptors)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => SetView(project, descriptors)));
                return;
            }

            SuspendLayout();
            rootLayout.SuspendLayout();
            bodyPanel.SuspendLayout();
            bodyStackPanel.SuspendLayout();
            sectionsLayout.SuspendLayout();

            try
            {
                ClearView();
                currentProject = project;

                SetSection(softwarePanel, "No Software Entries", project.SoftwareEntries.ToList(), true);
                SetSection(auxPanel, "No Media Entries", project.AuxEntries.ToList(), false);
                SetSection(supportPanel, "No Support Entries", [], true);

                HashSet<string> mvpHash = [];
                HashSet<string> fullAlphaItems = [];

                foreach (var dto in project.AuxEntries)
                {
                    if (dto.CollectionSetEnum == COLLECTION_SET_ENUM.Resource)
                    {
                        _ = fullAlphaItems.Add(MEDIA_TYPE_ENUM.Info.ToString());
                    }
                    else if (!string.IsNullOrWhiteSpace(dto.InternalDescriptor))
                    {
                        _ = fullAlphaItems.Add(dto.InternalDescriptor);
                    }
                }

                foreach (var scoring in project.ScoringMediaTypes)
                {
                    if (!mvpHash.Contains(scoring))
                    {
                        _ = mvpHash.Add(scoring);

                        var mediaItemUI = new MediaItemUI()
                        {
                            MediaDescription = scoring,
                            IconImageKey = scoring,
                            DatChipKey = string.Empty,
                            IconAlpha = fullAlphaItems.Contains(scoring) ? 100 : 80
                        };

                        mediaDragDropUI.AddToSelectedPanel(mediaItemUI);
                    }
                }

                foreach (var item in DatinateHelper.ScoringMediaMasterSet)
                {
                    var enumStr = item.ToString();

                    if (!mvpHash.Contains(enumStr))
                    {
                        var mediaItemUI = new MediaItemUI()
                        {
                            MediaDescription = enumStr,
                            IconImageKey = enumStr,
                            DatChipKey = string.Empty,
                            IconAlpha = fullAlphaItems.Contains(enumStr) ? 100 : 80
                        };

                        mediaDragDropUI.AddToAllPanel(mediaItemUI);
                    }
                }

                foreach (var desc in descriptors)
                {
                    var chip = new DescriptorChipUI()
                    {
                        TagColor = desc.Colour,
                        Description = desc.Description,
                        Code = desc.Code,
                        HideCheckbox = true
                    };

                    if (project.ExcludedDescriptorCodes.Contains(desc.Code))
                        descriptorsDragDropUI.AddToSelectedPanel(chip);
                    else
                        descriptorsDragDropUI.AddToAllPanel(chip);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                sectionsLayout.ResumeLayout(true);
                bodyStackPanel.ResumeLayout(true);
                bodyPanel.ResumeLayout(true);
                rootLayout.ResumeLayout(true);
                ResumeLayout(true);
            }

            PerformLayout();
            Invalidate(true);
            Update();
        }
        private void SetSection(
            AnimatedListPanel animatedPanel,
            string emptyText,
            IReadOnlyList<DatGrouperProjectEntry> dtos,
            bool limitInteraction)
        {
            ClearSection(animatedPanel);
            animatedPanel.SetRows(dtos, limitInteraction);
        }

        private void ClearSection(AnimatedListPanel section)
        {
            section.Controls.Clear();
        }

        private void SaveBtn_Click(object? sender, EventArgs e)
        {
            var softwareDTOs = softwarePanel.GetRows();
            var auxDTOs = auxPanel.GetRows();
            var supportDTOs = supportPanel.GetRows();

            if (currentProject == null) return;

            foreach (var dto in softwareDTOs)
                if (!ValidatePathOrEmpty("Software", dto.ID, dto.ContentPath ?? string.Empty)) return;

            foreach (var dto in auxDTOs)
                if (!ValidatePathOrEmpty("Media", dto.ID, dto.ContentPath ?? string.Empty)) return;

            foreach (var dto in supportDTOs)
                if (!ValidatePathOrEmpty("Support", dto.ID, dto.ContentPath ?? string.Empty)) return;

            var ctrls = mediaDragDropUI.GetSelectedItems();
            var scoringMedia = new List<string>();
            foreach (var c in ctrls)
            {
                if (c is MediaItemUI ui)
                    scoringMedia.Add(ui.MediaDescription);
            }

            ctrls = descriptorsDragDropUI.GetSelectedItems();
            var excludedDescriptors = new List<string>();
            foreach (var c in ctrls)
            {
                if (c is DescriptorChipUI ui)
                    excludedDescriptors.Add(ui.Code);
            }

            var proj = new DatGrouperProjectDTO(
                currentProject.ProjectName,
                softwareDTOs,
                [],
                auxDTOs,
                currentProject.Comment,
                excludedDescriptors.ToFrozenSet(),
                scoringMedia.ToFrozenSet(),
                currentProject.ExportSoftwareOptionsDTO,
                currentProject.MediaExports);

            SaveEvt?.Invoke(proj);
        }

        private bool ValidatePathOrEmpty(string tableName, string id, string path)
        {
            if (path.Length == 0)
                return true;

            var exists = Directory.Exists(path) || File.Exists(path);
            if (exists)
                return true;

            MessageBox.Show(
                this,
                $"Invalid path in {tableName}:\r\n\r\nName: {id}\r\nPath: {path}",
                "Invalid path",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return false;
        }

        private void BackBtn_Click(object? sender, EventArgs e)
        {
            BackEvt?.Invoke();
        }

        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);

            ClearSection(softwarePanel);
            ClearSection(auxPanel);
            ClearSection(supportPanel);
        }

        // TODO: Extend this for R2DAT Web at least
        private void batchUpdateBtn_Click(object sender, EventArgs e)
        {
            var ctrls = auxPanel.GetRowControls();
            var emuMoviesCtrls = new List<ContentPathRow>();
            
            string? knownGoodPath = null;

            foreach (var ctrl in ctrls)
            {
                if (ctrl.DatGroupEnum == DAT_GROUP_ENUM.R2DAT_EMUMOVIES)
                {
                    emuMoviesCtrls.Add(ctrl);

                    if (string.IsNullOrWhiteSpace(knownGoodPath))
                        if (!string.IsNullOrWhiteSpace(ctrl.Path) && Directory.Exists(ctrl.Path))    
                            knownGoodPath = Path.GetDirectoryName(ctrl.Path);       
                }
            }

            if (!string.IsNullOrWhiteSpace(knownGoodPath))
            {
                foreach (var emuCtrl in emuMoviesCtrls)
                {
                    var pointer = emuCtrl.PointerId;
                    if (!string.IsNullOrWhiteSpace(pointer))
                    {
                        var subset = DatinateHelper.GetPointerSubset(pointer);
                        if (!string.IsNullOrWhiteSpace(subset)) {
                            var path = Path.Combine(knownGoodPath, subset);
                            if (Directory.Exists(path))
                            {
                                emuCtrl.Path = path;
                                emuCtrl.UpdateFilesFoldersCount();
                            }
                        } 
                    }
                }
            }
        }
    }
}
