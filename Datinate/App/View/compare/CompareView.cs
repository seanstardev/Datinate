using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using Datinate.Shared;

namespace com.RADIO.Datinate.App.View.compare
{
    public partial class CompareView : UserControl, ICompareView 
    {
        public event Action<DatVO>? CustomiseClickEvt;
        public event Action<DatVO>? CreateDatClickEvt;
        public event Action? CompareClickEvt;
        public event Action? FormClosingEvt;
        public event Action? ClearClickEvt;

        CompareUI? selectedUI;

        public CompareView() 
        {
            InitializeComponent();

            leftUI.CustomiseClickEvt = new EventHandler(OnCustomiseClick);
            rightUI.CustomiseClickEvt = new EventHandler(OnCustomiseClick);
            middleUI.CustomiseClickEvt = new EventHandler(OnCustomiseClick);

            leftUI.CreateDatClickEvt = new EventHandler(OnCreateDatClick);
            rightUI.CreateDatClickEvt = new EventHandler(OnCreateDatClick);
            middleUI.CreateDatClickEvt = new EventHandler(OnCreateDatClick);

            UIHelper.PopSplitter(splitContainer1);
            UIHelper.PopSplitter(splitContainer2);

            UIHelper.PopButton(compareBtn);
            Facade.RegisterActor(this);
        }
        protected void HandleDisposing()
        {
            Facade.UnregisterActor(this);
        }

        public void ClearView() 
        {
            leftUI.ClearUI();
            rightUI.ClearUI();
            middleUI.ClearUI();
            filterUI1.Empty();
        }

        public void SetLeft(DatVO datVO) 
        {
            leftUI.SetUI(datVO, CompareUI.UiPosition.Left);
        }

        public void SetRight(DatVO datVO) 
        {
            rightUI.SetUI(datVO, CompareUI.UiPosition.Right);
        }

        public void SetMiddle(DatVO datVO) 
        {
            middleUI.SetUI(datVO, CompareUI.UiPosition.Middle);
        }

        public void UpdateUnits(UnitFormatHelper.Unit unit, bool showUnitInCells) 
        {
            leftUI.UpdateUnits(unit, showUnitInCells);
            rightUI.UpdateUnits(unit, showUnitInCells);
            middleUI.UpdateUnits(unit, showUnitInCells);
        }

        private void OnCompareClick(object sender, EventArgs e) 
        {
            CompareClickEvt?.Invoke();
        }

        private void OnCustomiseClick(object? sender, EventArgs e) 
        {
            if (sender == null || (sender as CompareUI) == null)
                return;

            selectedUI = sender as CompareUI;
            
            if (selectedUI != null)
            {
                var dat = selectedUI.GetDatVO();
                if (dat != null)
                    CustomiseClickEvt?.Invoke(dat);
            }
        }

        private void OnCreateDatClick(object? sender, EventArgs e) 
        {
            if (sender == null || sender as CompareUI == null)
                return;

            selectedUI = (CompareUI)sender;

            if (selectedUI != null)
            {
                var dat = selectedUI.GetDatVO();
                if (dat != null)
                    CreateDatClickEvt?.Invoke(dat);
            }
        }

        private void ClearBtn_Click(object sender, EventArgs e) 
        {
            ClearView();
            ClearClickEvt?.Invoke();
        }
    }
}
