using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Properties;
using Datinate.Shared;
using System.Diagnostics;
using static com.RADIO.Datinate.RMVC.Shared.UnitFormatHelper;

namespace datinate.app
{
    public partial class MainControlsView : UserControl, IMainControlsView
    {
        public event Action<Unit, bool>? UnitViewChangeEvt;
        public event Action? ShowCustomiseViewEvt;
        public event Action? ShowCompareViewEvt;
        public event Action? ShowProjectsEvt;
        public event Action? DatPathRemovedEvt;
        public event Action? ToggleMainViewEvt;

        private const int MaxWideButtonWidthPx = 240;
        private const int MinWideButtonWidthPx = 48;
        private const int HomeButtonWidthPx = 48;
        private const int ButtonGapPx = 12;
        private const int ButtonPanelRightPaddingPx = 3;
        private const int ButtonPanelTopPx = 13;
        private const int ButtonPanelHeightPx = 39;
        private const int MinGapFromUnitsPx = 12;

        protected object? selectedItem = null;
        protected UnitFormatHelper.Unit unit;

        public MainControlsView()
        {
            InitializeComponent();
            splashPanel.BringToFront();
            unitsCombo.SelectedIndex = 3;

            unitsCombo.MouseWheel += new MouseEventHandler(onUnitsMouseWheel);
            controlsPanel.Resize += ControlsPanel_Resize;

            controlsLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            controlsLayoutPanel.AutoSize = false;
            controlsLayoutPanel.Margin = Padding.Empty;
            controlsLayoutPanel.Padding = Padding.Empty;

            Facade.RegisterActor(this);

            ApplyRightButtonLayout();
        }

        public void SetMainControlEnabled(DatinateEnums.MAIN_CONTROL_ENUM mainCtrlEnum, bool doSetEnabled)
        {
            Ui(() =>
            {
                switch (mainCtrlEnum)
                {
                    case DatinateEnums.MAIN_CONTROL_ENUM.DatGrouper:
                        datGrouperBtn.Enabled = doSetEnabled;
                        break;
                    case DatinateEnums.MAIN_CONTROL_ENUM.DatCompare:
                        compareBtn2.Enabled = doSetEnabled;
                        break;
                    case DatinateEnums.MAIN_CONTROL_ENUM.DatCustomiser:
                        customiseBtn2.Enabled = doSetEnabled;
                        break;
                }
            });
        }

        public void SetActiveMainView(DatinateEnums.DAT_SCREEN_ENUM currentView)
        {
            Ui(() =>
            {
                datManagerBtn.ButtonImage = currentView == DatinateEnums.DAT_SCREEN_ENUM.Landing
                    ? Resources.datAction_home
                    : Resources.datAction_main;
            });
        }

        public bool ShowUnitInCells
            => showUnit.Checked;

        public UnitFormatHelper.Unit Unit => unit;

        public void ActivateView()
        {
            Ui(() =>
            {
                if (splashPanel.Visible)
                    splashPanel.Visible = false;

                ApplyRightButtonLayout();
            });
        }

        private void onUnitChange(object? sender, EventArgs e)
        {
            if (unitsCombo.SelectedItem == selectedItem || unitsCombo.SelectedItem == null)
                return;
            else
                selectedItem = unitsCombo.SelectedItem!;

            switch (unitsCombo.SelectedItem.ToString()!.ToLower().Split()[0])
            {
                case "bytes":
                    unit = UnitFormatHelper.Unit.B;
                    break;

                case "kilobytes":
                    unit = UnitFormatHelper.Unit.KB;
                    break;

                case "megabytes":
                    unit = UnitFormatHelper.Unit.MB;
                    break;

                case "gigabytes":
                    unit = UnitFormatHelper.Unit.GB;
                    break;

                case "terabytes":
                    unit = UnitFormatHelper.Unit.TB;
                    break;

                default:
                    Debug.Print(this + ": Error: Unknown unit. ");
                    break;
            }

            UnitViewChangeEvt?.Invoke(Unit, ShowUnitInCells);
        }

        private void onShowUnitChange(object? sender, EventArgs e)
        {
            UnitViewChangeEvt?.Invoke(Unit, ShowUnitInCells);
        }

        protected void HandleDisposing()
        {
            controlsPanel.Resize -= ControlsPanel_Resize;
            Facade.UnregisterActor(this);
        }

        /**
         * Stop units combo from being changed unknowingly:
         */
        void onUnitsMouseWheel(object? sender, EventArgs e)
        {
            if (e is HandledMouseEventArgs args)
                args.Handled = true;
        }

        void onShowCustomiseClick(object? sender, EventArgs e)
        {
            ShowCustomiseViewEvt?.Invoke();
        }

        void onShowCompareClick(object? sender, EventArgs e)
        {
            ShowCompareViewEvt?.Invoke();
        }

        void projectsBtn_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowProjectsEvt?.Invoke();
        }

        private void compareBtn2_Click(object sender, EventArgs e)
        {
            ShowCompareViewEvt?.Invoke();
        }

        private void customiseBtn2_Click(object sender, EventArgs e)
        {
            ShowCustomiseViewEvt?.Invoke();
        }

        private void datGrouperBtn_Click(object sender, EventArgs e)
        {
            ShowProjectsEvt?.Invoke();
        }

        private void datManagerBtn_Click(object sender, EventArgs e)
        {
            ToggleMainViewEvt?.Invoke();
        }

        private void ControlsPanel_Resize(object? sender, EventArgs e)
        {
            ApplyRightButtonLayout();
        }

        private void ApplyRightButtonLayout()
        {
            if (IsDisposed || !IsHandleCreated || controlsPanel.IsDisposed)
                return;

            int availableRightEdge = controlsPanel.ClientSize.Width - ButtonPanelRightPaddingPx;
            int leftLimit = unitsCombo.Right + MinGapFromUnitsPx;
            int availableWidth = availableRightEdge - leftLimit;

            if (availableWidth <= 0)
                return;

            int availableForWideButtons = availableWidth - HomeButtonWidthPx - (ButtonGapPx * 3);
            int wideButtonWidth = availableForWideButtons / 3;

            if (wideButtonWidth > MaxWideButtonWidthPx)
                wideButtonWidth = MaxWideButtonWidthPx;

            if (wideButtonWidth < MinWideButtonWidthPx)
                wideButtonWidth = MinWideButtonWidthPx;

            int wideColumnWidth = ButtonGapPx + wideButtonWidth;
            int layoutWidth = HomeButtonWidthPx + (wideColumnWidth * 3);
            int layoutLeft = availableRightEdge - layoutWidth;

            controlsLayoutPanel.SuspendLayout();

            controlsLayoutPanel.ColumnStyles.Clear();
            controlsLayoutPanel.ColumnCount = 4;
            controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, HomeButtonWidthPx));
            controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, wideColumnWidth));
            controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, wideColumnWidth));
            controlsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, wideColumnWidth));

            controlsLayoutPanel.Location = new Point(layoutLeft, ButtonPanelTopPx);
            controlsLayoutPanel.Size = new Size(layoutWidth, ButtonPanelHeightPx);

            datManagerBtn.Margin = new Padding(0);
            datGrouperBtn.Margin = new Padding(ButtonGapPx, 0, 0, 0);
            customiseBtn2.Margin = new Padding(ButtonGapPx, 0, 0, 0);
            compareBtn2.Margin = new Padding(ButtonGapPx, 0, 0, 0);

            datManagerBtn.Dock = DockStyle.Fill;
            datGrouperBtn.Dock = DockStyle.Fill;
            customiseBtn2.Dock = DockStyle.Fill;
            compareBtn2.Dock = DockStyle.Fill;

            controlsLayoutPanel.ResumeLayout();
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
    }
}