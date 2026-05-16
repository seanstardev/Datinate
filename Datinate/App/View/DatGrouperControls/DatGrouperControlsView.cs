using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Properties;
using Datinate.Shared;
using RadioLibCore.RadioDat;
using System.ComponentModel;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.app
{
    public partial class DatGrouperControlsView : UserControl, IDatGrouperControlsView
    {
        public event Action? ExitMediaModeEvt;
        public event Action<bool, IGameEntity>? AssignMediaEvt;
        public event Action<bool>? EnterMediaModeEvt;
        public event Action<bool, IGamePart>? ShowPartGroupingReportsEvt;

        public event Action? SwapGrouperColumnsEvt;
        public event Action<bool, string>? SearchEvt;

        public event Action<bool>? HideAliasesEvt;
        public event Action<bool>? ShowExcludedFamiliesEvt;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IGameEntity? SelectedEntity { get; private set; }

        private bool? entitySourceIsAuto = null;
        private string? entityName = null;

        private bool mediaModeOn = false;

        public DatGrouperControlsView()
        {
            InitializeComponent();
            Facade.RegisterActor(this);

            UIHelper.PopButton(assignMediaBtn);
            UIHelper.PopButton(mediaToggleBtn);
            UIHelper.PopButton(partReportsBtn);

            swapPanelsLeftBtn.Visible = false;

            ClearView();
        }

        public IDatGrouperNodePreview DatGrouperNodePreview => datGrouperNodePreview;

        public void SetCompletionStats(long curatedParts, long totalParts)
        {
            Ui(() =>
            {
                double percent = totalParts == 0
                    ? 0
                    : (curatedParts * 100.0) / totalParts;

                curationTalliesLabel.Text =
                    "Curated Parts: " +
                    DatinateHelper.GetReadableNumber(curatedParts) +
                    " / " +
                    DatinateHelper.GetReadableNumber(totalParts) +
                    "    ■    " +
                    percent.ToString("0.0") + "%";
            });
        }

        public void ClearView()
        {
            entitySourceIsAuto = null;
            entityName = null;
            SelectedEntity = null;

            Ui(() =>
            {
                hideExcludedBtn.Checked = false;
                hideAliasesBtn.Checked = false;
                entityIconPic.Image = null;
                partReportsBtn.Enabled = false;
                searchNameBtn.Enabled = false;
                assignMediaBtn.Enabled = false;
                treeNodePic.Image = null;
            });
        }
        public void SetDatGrouperScreenLayout(DAT_GROUPER_LAYOUT_ENUM layout)
        {
            Ui(() =>
            {
                mediaModeOn = DatinateHelper.IsMediaLayout(layout);
            });
        }
        public void SetView(IGameEntity? entity, bool isFromAuto)
        {
            var newName = DatinateHelper.GetGameEntityName(entity);

            if (object.ReferenceEquals(entity, SelectedEntity) &&
                entitySourceIsAuto == isFromAuto &&
                object.Equals(newName, entityName))
                return;

            entitySourceIsAuto = isFromAuto;

            entityName = newName;

            SelectedEntity = entity;

            Ui(() =>
            {
                searchNameBtn.Enabled = !string.IsNullOrEmpty(entityName);

                partReportsBtn.Enabled = entity is IGamePart;

                mediaToggleBtn.Enabled = entity != null;

                assignMediaBtn.Enabled = !isFromAuto;
                treeNodePic.BackgroundImageLayout = ImageLayout.Center;

                if (SelectedEntity != null)
                {
                    if (isFromAuto)
                        DatinateHelper.SetTitleQueued(titleUI);
                    else
                        DatinateHelper.SetTitleCurated(titleUI);

                    titleUI.Visible = true;
                }
                else
                    titleUI.Visible = false;

                Bitmap? img = null;

                if (SelectedEntity is IGameFamily)
                    img = Resources.media_icons_Family;
                else if (SelectedEntity is IGame)
                    img = Resources.media_icons_Game;
                else if (SelectedEntity is IGamePart)
                    img = Resources.media_icons_GamePart;

                entityIconPic.Image = img;
            });
        }

        internal void SetSwapColumnsButtonPosition(bool placeLeft)
        {
            swapPanelsLeftBtn.Visible = placeLeft;
            swapPanelsRightBtn.Visible = !placeLeft;
        }

        private void partReportsBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEntity is IGamePart part && entitySourceIsAuto != null)
                ShowPartGroupingReportsEvt?.Invoke((bool)entitySourceIsAuto, part);
        }

        private void swapPanelsBtn_Click(object sender, EventArgs e)
        {
            if (swapPanelsLeftBtn.Visible)
            {
                swapPanelsRightBtn.Visible = true;
                swapPanelsLeftBtn.Visible = false;
            }
            else
            {
                swapPanelsRightBtn.Visible = false;
                swapPanelsLeftBtn.Visible = true;
            }
            SwapGrouperColumnsEvt?.Invoke();
        }

        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }

        private void SearchNameBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(entityName) && entitySourceIsAuto != null)
                SearchEvt?.Invoke((bool)entitySourceIsAuto, entityName);
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

        private void assignMediaBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEntity != null && entitySourceIsAuto == false)
                AssignMediaEvt?.Invoke(entitySourceIsAuto ?? true, SelectedEntity);
        }

        private void previewMediaBtn_Click(object sender, EventArgs e)
        {
            if (mediaModeOn)
            {
                mediaModeOn = false;
                ExitMediaModeEvt?.Invoke();
            }
            else
            {
                mediaModeOn = true;
                EnterMediaModeEvt?.Invoke(entitySourceIsAuto ?? true);
            }
        }

        private void hideAliasesBtn_CheckedChanged(object sender, EventArgs e)
        {
            HideAliasesEvt?.Invoke(!hideAliasesBtn.Checked);
        }

        private void hideExcludedBtn_CheckedChanged(object sender, EventArgs e)
        {
            ShowExcludedFamiliesEvt?.Invoke(hideExcludedBtn.Checked);
        }
    }
}
