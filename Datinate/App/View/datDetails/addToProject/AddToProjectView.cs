using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;
using System.ComponentModel;
using datinate.app;

namespace com.RADIO.Datinate.App.View.datDetails.addToProject
{
    public partial class AddToProjectView : UserControl, IAddToProjectView
    {

        public event Action<
            DatVO,
            COLLECTION_SET_ENUM,
            DAT_GROUP_ENUM,
            DAT_GROUP_TARGET_ENUM,
            string?,
            DatSubsetFilter?>? AddDatToProjectEvt;

        public event Action<
            DatVO,            
            DAT_GROUP_ENUM>? AddResourceBatchToProjectEvt;

        public event Action<DatVO>? FetchDatSubsetInfoEvt;

        private readonly Dictionary<DAT_GROUP_ENUM, DatGroupChipButton> datGroupChipByEnum =
            new Dictionary<DAT_GROUP_ENUM, DatGroupChipButton>();

        private DAT_GROUP_ENUM selectedDatGroupEnum = DAT_GROUP_ENUM.NOT_SET;

        private static readonly HashSet<DAT_GROUP_ENUM> ExcludedDatGroups =
            new HashSet<DAT_GROUP_ENUM>
            {
                DAT_GROUP_ENUM.NOT_SET,
                DAT_GROUP_ENUM.RADIO_INTERNAL
            };

        private static readonly HashSet<DAT_GROUP_ENUM> ResourceDatGroups =
            new HashSet<DAT_GROUP_ENUM>
            {
                DAT_GROUP_ENUM.R2DAT_WEB
            };

        private static readonly IReadOnlyList<DAT_GROUP_ENUM> MediaDatGroups =
            new[]
            {
                DAT_GROUP_ENUM.MAME_MEDIA,
                DAT_GROUP_ENUM.R2DAT_EMUMOVIES,
                DAT_GROUP_ENUM.R2DAT_REPLACEMENT_DOCS,
                DAT_GROUP_ENUM.TOSEC_PIX,
                DAT_GROUP_ENUM.VGMARCHIVE
            };

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<R2DatResourceDTO> R2DatResources { get; set; } = new List<R2DatResourceDTO>();

        private COLLECTION_SET_ENUM? activeCollectionSetEnum = null;

        private DatVO? datVO;

        Dictionary<string, List<string>> datSubsetDictionary = new Dictionary<string, List<string>>();

        public AddToProjectView()
        {
            InitializeComponent();

            addAllContentBtn.Visible = false;
            Facade.RegisterActor(this);

            PopulateDatGroupChips();

            datGroupGroupBox.Enabled = false;
        }
        private void PopulateDatGroupChips()
        {
            softwareContainer.Controls.Clear();
            mediaContainer.Controls.Clear();
            resourceContainer.Controls.Clear();
            datGroupChipByEnum.Clear();

            HashSet<DAT_GROUP_ENUM> mediaLookup =
                new HashSet<DAT_GROUP_ENUM>(MediaDatGroups);

            DAT_GROUP_ENUM[] softwareDatGroups = Enum.GetValues<DAT_GROUP_ENUM>()
                .Where(x =>
                    !ExcludedDatGroups.Contains(x) &&
                    !ResourceDatGroups.Contains(x) &&
                    !mediaLookup.Contains(x))
                .ToArray();

            foreach (var datEnum in softwareDatGroups)
            {
                var datChip = CreateDatChip(datEnum);
                softwareContainer.Controls.Add(datChip);
            }

            foreach (var datEnum in MediaDatGroups)
            {
                var datChip = CreateDatChip(datEnum);
                mediaContainer.Controls.Add(datChip);
            }

            foreach (var datEnum in ResourceDatGroups)
            {
                var datChip = CreateDatChip(datEnum);
                resourceContainer.Controls.Add(datChip);
            }

            RefreshDatGroupChipSelection();
        }
        private void SetSelectedDatGroup(DAT_GROUP_ENUM datGroupEnum)
        {
            selectedDatGroupEnum = datGroupEnum;

            addAllContentBtn.Visible = false;

            ResetSubsetGroup();

            if (datGroupEnum == DAT_GROUP_ENUM.NOT_SET)
            {
                activeCollectionSetEnum = null;
                controlsGroup.Enabled = false;
                datSubsetGroup.Enabled = false;

                RefreshDatGroupChipSelection();
                SetControlBtns();

                return;
            }

            activeCollectionSetEnum = GetCollectionSetEnum(datGroupEnum);

            datSubsetGroup.Enabled = true;
            controlsGroup.Enabled = true;

            if (datGroupEnum == DAT_GROUP_ENUM.R2DAT_EMUMOVIES)
                addAllContentBtn.Visible = true;

            RefreshDatGroupChipSelection();
            SetControlBtns();
        }

        private COLLECTION_SET_ENUM GetCollectionSetEnum(DAT_GROUP_ENUM datGroupEnum)
        {
            if (datGroupEnum == DAT_GROUP_ENUM.NOT_SET)
                return COLLECTION_SET_ENUM.NOT_SET;

            if (ResourceDatGroups.Contains(datGroupEnum))
                return COLLECTION_SET_ENUM.Resource;

            if (MediaDatGroups.Contains(datGroupEnum))
                return COLLECTION_SET_ENUM.Media;

            return COLLECTION_SET_ENUM.Software;
        }

        private void RefreshDatGroupChipSelection()
        {
            bool hasSelection = selectedDatGroupEnum != DAT_GROUP_ENUM.NOT_SET;

            foreach (var kvp in datGroupChipByEnum)
            {
                bool isSelected = kvp.Key == selectedDatGroupEnum;

                kvp.Value.Selected = false;
                kvp.Value.Muted = hasSelection && !isSelected;
            }
        }
        private DatGroupChipButton CreateDatChip(DAT_GROUP_ENUM datGroupEnum)
        {
            var datChip = new DatGroupChipButton()
            {
                DatGroupEnum = datGroupEnum,
                Margin = new Padding(4, 4, 4, 4),
                Cursor = Cursors.Hand
            };

            datChip.Click += datGroupChip_Click;
            datGroupChipByEnum[datGroupEnum] = datChip;

            return datChip;
        }
        private void datGroupChip_Click(object? sender, EventArgs e)
        {
            if (sender is not DatGroupChipButton datChip)
                return;

            SetSelectedDatGroup(datChip.DatGroupEnum);
        }
        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }

        public void SetProjectUiDatSubsetInfo(Dictionary<string, List<string>> datSubsetDictionary)
        {

            if (datSubsetDictionary.Count == 0)
            {
                MessageBox.Show("This DAT does not organise content into subfolders.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                radioDatContentUseAll.Checked = true;
                return;
            }

            this.datSubsetDictionary = datSubsetDictionary;

            datSubsetNameCombo.Items.AddRange(datSubsetDictionary.Keys.ToArray());
            datSubsetNameCombo.SelectedIndex = 0;
        }

        public void SetView(DatVO datVO)
        {
            Reset();

            this.datVO = datVO;

            datNameLabel.Text = datVO.GetDatNameWithoutExt();

            var values = Enum.GetValues(typeof(DAT_GROUP_ENUM));

            datGroupGroupBox.Enabled = true;
        }
        public DAT_GROUP_ENUM GetDatGroupEnum()
        {
            return selectedDatGroupEnum;
        }
        public void Reset()
        {
            activeCollectionSetEnum = null;
            selectedDatGroupEnum = DAT_GROUP_ENUM.NOT_SET;

            datNameLabel.Text = string.Empty;

            datGroupGroupBox.Enabled = false;
            datSubsetGroup.Enabled = false;
            controlsGroup.Enabled = false;

            contentTypeGroup.Enabled = false;
            contentTypeCombo.Items.Clear();

            RefreshDatGroupChipSelection();
            SetControlBtns();
            ResetSubsetGroup();
        }
        private DatVO? GetDatVO()
        {
            return datVO;
        }

        // NOTE: Will return null if not applicable
        private DatSubsetFilter? GetDatSubsetFilter()
        {
            if (datSubsetNameCombo.SelectedIndex < 0 || datSubsetFolderCombo.SelectedIndex < 0)
                return null;

            return new DatSubsetFilter(
                datSubsetNameCombo.SelectedItem!.ToString()!
                , datSubsetFolderCombo.SelectedIndex == 0 ? null : datSubsetFolderCombo.SelectedItem!.ToString());
        }

        private void SetControlBtns()
        {
            switch (activeCollectionSetEnum)
            {
                case COLLECTION_SET_ENUM.Software:
                    mediaIncludeBtn.Enabled = false;
                    includeGamesBtn.Enabled = true;
                    break;

                case COLLECTION_SET_ENUM.Resource:
                case COLLECTION_SET_ENUM.Media:
                    mediaIncludeBtn.Enabled = true;
                    includeGamesBtn.Enabled = false;
                    break;

                default:
                    mediaIncludeBtn.Enabled = false;
                    includeGamesBtn.Enabled = false;
                    break;
            }
            SetContentTypeControls();
        }

        private void SetContentTypeControls()
        {
            contentTypeCombo.Items.Clear();
            contentTypeGroup.Enabled = false;

            switch (activeCollectionSetEnum)
            {
                case COLLECTION_SET_ENUM.Media:
                    contentTypeCombo.Items.AddRange(GetValuesSortedForCombo<MEDIA_TYPE_ENUM>());
                    contentTypeCombo.SelectedIndex = 0;
                    contentTypeGroup.Enabled = true;
                    break;

                case COLLECTION_SET_ENUM.Resource:
                    List<string> list = new List<string>();
                    list.Add("NOT_SET");

                    foreach (var resource in R2DatResources
                                 .OrderBy(r => r.Name == "NOT_SET" ? 0 : 1)
                                 .ThenBy(r => r.Name, StringComparer.Ordinal))
                    {
                        if (!string.IsNullOrWhiteSpace(resource.Name))
                            list.Add(resource.Name);
                    }

                    contentTypeCombo.Items.AddRange(list.Cast<object>().ToArray());
                    contentTypeCombo.SelectedIndex = 0;
                    contentTypeGroup.Enabled = true;
                    break;

                default:
                    break;
            }
        }
        private static object[] GetValuesSortedForCombo<TEnum>() where TEnum : struct, Enum
            => GetValuesSorted<TEnum>().Cast<object>().ToArray();

        private static List<TEnum> GetValuesSorted<TEnum>() where TEnum : struct, Enum
        {
            var values = Enum.GetValues<TEnum>();

            return values
                .OrderBy(v => string.Equals(v.ToString(), "NOT_SET", StringComparison.Ordinal) ? 0 : 1)
                .ThenBy(v => v.ToString(), StringComparer.Ordinal)
                .ToList();
        }
        
        void ResetSubsetGroup()
        {
            radioDatContentUseAll.Checked = true;

            datSubsetFolderCombo.Items.Clear();
            datSubsetNameCombo.Items.Clear();

            datSubsetNameCombo.Enabled = false;
            datSubsetFolderCombo.Enabled = false;
            datSubsetDictionary = new Dictionary<string, List<string>>();
        }

        private void radioDatContentUseAll_CheckedChanged(object sender, EventArgs e)
        {
            if (radioDatContentUseAll.Checked)
            {
                ResetSubsetGroup();
            }
            else
            {
                var dat = GetDatVO();
                if (dat != null)
                {
                    FetchDatSubsetInfoEvt?.Invoke(dat);
                    datSubsetFolderCombo.Enabled = true;
                    datSubsetNameCombo.Enabled = true;
                }
            }
        }

        private void datSubsetNameCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            datSubsetFolderCombo.Items.Clear();

            datSubsetFolderCombo.Items.AddRange(
                datSubsetDictionary[(string)datSubsetNameCombo.SelectedItem!].ToArray()

            );
            datSubsetFolderCombo.SelectedIndex = 0;
        }

        private void mediaIncludeBtn_Click(object sender, EventArgs e) =>
            InvokeAddToProjectEvent(DAT_GROUP_TARGET_ENUM.MEDIA_INCLUDE_GROUP);

        private void resourceIncludeBtn_Click(object sender, EventArgs e) =>
            InvokeAddToProjectEvent(DAT_GROUP_TARGET_ENUM.RESOURCE_INCLUDE_GROUP);

        private void includeBtn_Click(object sender, EventArgs e) =>
            InvokeAddToProjectEvent(DAT_GROUP_TARGET_ENUM.GAMES_INCLUDE_GROUP);

        private void InvokeAddToProjectEvent(DAT_GROUP_TARGET_ENUM datGroupTargetEnum)
        {
            var dat = GetDatVO();

            if (dat == null) return;

            string? internalDescriptor = null;

            if (activeCollectionSetEnum == COLLECTION_SET_ENUM.Media || activeCollectionSetEnum == COLLECTION_SET_ENUM.Resource)
            {
                // First index is 'NOT_SET':
                if (contentTypeCombo.SelectedIndex < 1)
                {
                    MessageBox.Show(
                        "Cannot proceed. A Content Type is required.",
                        "Attention",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    return;
                }
                else
                    internalDescriptor = contentTypeCombo.SelectedItem!.ToString()!;
            }

            AddDatToProjectEvt?.Invoke(
                dat,
                activeCollectionSetEnum ?? COLLECTION_SET_ENUM.NOT_SET,
                GetDatGroupEnum(),
                datGroupTargetEnum,
                internalDescriptor,
                GetDatSubsetFilter());
        }

        private void addAllContentBtn_Click(object sender, EventArgs e)
        {
            var datGroupEnum = GetDatGroupEnum();

            if (datGroupEnum != DAT_GROUP_ENUM.R2DAT_EMUMOVIES ||
                datVO == null)
            {
                return;
            }

            AddResourceBatchToProjectEvt?.Invoke(datVO, datGroupEnum);
        }
    }
}
