using Datinate.Rmvc;
using Datinate.Shared;
using Datinate.Shared.Dat;
using Datinate.Shared.Rmvc;
using Datinate.App.WinForms.View.UI.Compare;
using Datinate.App.WinForms.View.Util;

namespace Datinate.App.WinForms.View
{
    public partial class CompareView : UserControl, ICompareView 
    {
        private readonly Datinate.App.WinForms.View.Util.UiDispatcher uiDispatcher;

        public event Action<DatVO>? CustomiseClickEvt;
        public event Action<DatVO>? CreateDatClickEvt;
        public event Action? CompareClickEvt;
        public event Action? FormClosingEvt;
        public event Action? ClearClickEvt;

        CompareUI? selectedUI;

        public CompareView() 
        {
            uiDispatcher = new Datinate.App.WinForms.View.Util.UiDispatcher(this);
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

            Facade.RegisterActor<ICompareView>(this);
        }
        protected void HandleDisposing()
            => Facade.UnregisterActor<ICompareView>(this);
        
        public void ClearView() 
        {
            if (!uiDispatcher.CheckAccess)
            {
                uiDispatcher.Run(() => ClearView());
                return;
            }

            leftUI.ClearUI();
            rightUI.ClearUI();
            middleUI.ClearUI();
        }

        public void SetLeft(DatVO datVO) 
        {
            if (!uiDispatcher.CheckAccess)
            {
                uiDispatcher.Run(() => SetLeft(datVO));
                return;
            }

            leftUI.SetUI(datVO, CompareUI.UiPosition.Left);
        }

        public void SetRight(DatVO datVO) 
        {
            if (!uiDispatcher.CheckAccess)
            {
                uiDispatcher.Run(() => SetRight(datVO));
                return;
            }

            rightUI.SetUI(datVO, CompareUI.UiPosition.Right);
        }

        public void SetMiddle(DatVO datVO) 
        {
            if (!uiDispatcher.CheckAccess)
            {
                uiDispatcher.Run(() => SetMiddle(datVO));
                return;
            }

            middleUI.SetUI(datVO, CompareUI.UiPosition.Middle);
        }

        public void UpdateUnits(UnitFormatHelper.Unit unit, bool showUnitInCells) 
        {
            if (!uiDispatcher.CheckAccess)
            {
                uiDispatcher.Run(() => UpdateUnits(unit, showUnitInCells));
                return;
            }

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
