using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public partial class ExportView : UserControl, IExportView
    {
        private const int MaxContentWidth = 1200;
        private const int PriorityUiBottomGapPx = 8;
        private const int PriorityUiMinWidthPx = 320;

        public event Action? BackEvt;
        public event Action<IReadOnlyList<DatGrouperMediaExportEntryDTO>, ExportSoftwareOptionsDTO, DAT_GROUPER_EXPORT_ENUM>? ExportProjectEvt;
        public event Action<IReadOnlyList<DatGrouperMediaExportEntryDTO>, ExportSoftwareOptionsDTO>? SaveSettingsEvt;

        public ExportView()
        {
            InitializeComponent();
            Facade.RegisterActor(this);
            BackColor = SystemColors.Control;
            bodyPanel.BackColor = SystemColors.Control;

            UIHelper.PopButton(exportMediaBtn);
            UIHelper.PopButton(exportSoftwareBtn);

            ConfigureMediaContainer();

            Resize += OnResize;
        }

        public void SetView(
            bool everyGameHasExactlyOnePart,
            ExportSoftwareOptionsDTO exportSoftwareOptions,
            IReadOnlyDictionary<DatinateEnums.MEDIA_TYPE_ENUM, IReadOnlyList<MediaExportPriorityItemDTO>> exportMediaDictionary,
            string exportSettingsMessage)
        {
            Ui(() =>
            {
                tabControl.SelectedIndex = 0;

                export1g1rCB.Checked = exportSoftwareOptions.ExportAs1G1R;
                skipExcludedGamesCB.Checked = exportSoftwareOptions.SkipExcludedGames;
                skipExcludedDescriptorFamiliesCB.Checked = exportSoftwareOptions.SkipScoringExemptFamilies;
                exportAsM3uCB.Checked = everyGameHasExactlyOnePart ? exportSoftwareOptions.ExportM3Us : true;
                datStructureGroup.Enabled = everyGameHasExactlyOnePart;

                mediaContainer.SuspendLayout();

                try
                {
                    ClearMediaContainerControls();

                    foreach (var kvp in exportMediaDictionary)
                    {
                        var mediaEnum = kvp.Key;
                        IReadOnlyList<MediaExportPriorityItemDTO> dtos = kvp.Value;

                        if (dtos.Count == 0)
                            continue;

                        var ctrl = new ExportPriorityUI
                        {
                            Margin = new Padding(0, 0, 0, PriorityUiBottomGapPx),
                            Width = GetPriorityUiWidth()
                        };

                        ctrl.SetUI(mediaEnum, dtos, dtos.First().IsScoringItem);

                        mediaContainer.Controls.Add(ctrl);
                    }

                    ResizePriorityUis();
                }
                finally
                {
                    mediaContainer.ResumeLayout(true);
                    ResetMediaContainerScroll();
                }
            });
        }

        private void ClearMediaContainerControls()
        {
            while (mediaContainer.Controls.Count > 0)
            {
                var control = mediaContainer.Controls[0];
                mediaContainer.Controls.RemoveAt(0);
                control.Dispose();
            }
        }

        private void ResetMediaContainerScroll()
        {
            if (mediaContainer.IsDisposed)
                return;

            void Reset()
            {
                if (mediaContainer.IsDisposed)
                    return;

                mediaContainer.AutoScrollPosition = Point.Empty;
                mediaContainer.PerformLayout();
                mediaContainer.Invalidate();
            }

            Reset();

            if (!mediaContainer.IsHandleCreated)
                return;

            try
            {
                mediaContainer.BeginInvoke(new Action(Reset));
            }
            catch (InvalidOperationException)
            {
            }
        }
        private void ConfigureMediaContainer()
        {
            mediaContainer.FlowDirection = FlowDirection.TopDown;
            mediaContainer.WrapContents = false;
            mediaContainer.AutoScroll = true;
            mediaContainer.Padding = Padding.Empty;
            mediaContainer.Margin = Padding.Empty;

            mediaContainer.HorizontalScroll.Enabled = false;
            mediaContainer.HorizontalScroll.Visible = false;

            mediaContainer.SizeChanged += (_, _) => ResizePriorityUis();
        }

        private void ResizePriorityUis()
        {
            if (mediaContainer.IsDisposed)
                return;

            int width = GetPriorityUiWidth();

            mediaContainer.SuspendLayout();

            try
            {
                foreach (Control control in mediaContainer.Controls)
                    control.Width = width;

                mediaContainer.HorizontalScroll.Enabled = false;
                mediaContainer.HorizontalScroll.Visible = false;
            }
            finally
            {
                mediaContainer.ResumeLayout(false);
            }
        }

        private int GetPriorityUiWidth()
        {
            int width =
                mediaContainer.ClientSize.Width
                - mediaContainer.Padding.Horizontal
                - SystemInformation.VerticalScrollBarWidth
                - 2;

            return Math.Max(PriorityUiMinWidthPx, width);
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

                ResizePriorityUis();
            }
            finally
            {
                rootLayout.ResumeLayout(true);
                autoSizeContainer.ResumeLayout(true);
            }
        }

        private void BackBtn_Click(object? sender, EventArgs e)
        {
            BackEvt?.Invoke();
        }

        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }

        private void exportMediaBtn_Click(object sender, EventArgs e)
        {
            ExportProjectEvt?.Invoke(GetExportMedia(), GetExportSoftwareOptions(), DAT_GROUPER_EXPORT_ENUM.Media);
        }

        private void exportSoftwareBtn_Click(object sender, EventArgs e)
        {
            ExportProjectEvt?.Invoke(GetExportMedia(), GetExportSoftwareOptions(), DAT_GROUPER_EXPORT_ENUM.Software);
        }
        private ExportSoftwareOptionsDTO GetExportSoftwareOptions()
        {
            return new ExportSoftwareOptionsDTO(
                export1g1rCB.Checked,
                skipExcludedDescriptorFamiliesCB.Checked,
                skipExcludedGamesCB.Checked,
                exportAsM3uCB.Checked);
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

        private void saveSettingsBtn_Click(object sender, EventArgs e)
        {
            if (SaveSettingsEvt == null)
                return;

            SaveSettingsEvt.Invoke(GetExportMedia(), GetExportSoftwareOptions());
        }
        private IReadOnlyList<DatGrouperMediaExportEntryDTO> GetExportMedia()
        {
            var mediaList = new List<DatGrouperMediaExportEntryDTO>();

            foreach (Control control in mediaContainer.Controls)
            {
                if (control is not ExportPriorityUI priorityUI)
                    continue;

                var entries = new List<DatGrouperMediaExportSourceDTO>();
                var addedSourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var dto in priorityUI.GetIncludedPriorityItems())
                {
                    if (string.IsNullOrWhiteSpace(dto.SourceId))
                        continue;

                    if (!addedSourceIds.Add(dto.SourceId))
                        continue;

                    entries.Add(new DatGrouperMediaExportSourceDTO(
                        dto.SourceId,
                        true));
                }

                foreach (var dto in priorityUI.GetExcludedPriorityItems())
                {
                    if (string.IsNullOrWhiteSpace(dto.SourceId))
                        continue;

                    if (!addedSourceIds.Add(dto.SourceId))
                        continue;

                    entries.Add(new DatGrouperMediaExportSourceDTO(
                        dto.SourceId,
                        false));
                }

                if (entries.Count == 0)
                    continue;

                mediaList.Add(new DatGrouperMediaExportEntryDTO(
                    priorityUI.MediaType,
                    entries));
            }

            return mediaList;
        }
    }
}