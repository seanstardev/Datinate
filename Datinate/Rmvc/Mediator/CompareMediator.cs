using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class CompareMediator : RMediator 
    {
        public CompareMediator(Type actor) : base(actor)
        {
        }

        private ICompareView? view => (ICompareView?)base.viewBase;

        protected override void Initialsed()
        {
            if (view != null)
            {
                view.CustomiseClickEvt += OnCustomiseClick;
                view.CompareClickEvt += OnCompareClick;
                view.FormClosingEvt += OnFormClosing;
                view.CreateDatClickEvt += OnCreateDatClick;
            }
        }

        protected override void Disposing()
        {
            if (view != null)
            {
                view.CustomiseClickEvt -= OnCustomiseClick;
                view.CompareClickEvt -= OnCompareClick;
                view.FormClosingEvt -= OnFormClosing;
                view.CreateDatClickEvt -= OnCreateDatClick;
            }
        }
        public void ClearView() 
        {
            view?.ClearView();
        }

        public void SetLeft(DatVO datVO)
        {
            view?.SetLeft(datVO);
            base.ExecuteCommand(new SetCompareViewVisibleCmd(true));
        }

        public void SetRight(DatVO datVO)
        {
            view?.SetRight(datVO);
            base.ExecuteCommand(new SetCompareViewVisibleCmd(true));
        }

        public void UpdateUnitsView(UnitFormatHelper.Unit unit, bool showUnitInCells)
        {
            view?.UpdateUnits(unit, showUnitInCells);
        }

        public void SetComparisonResult(DatComparisonVO vo)
        {
            view?.SetLeft(vo.leftDatVO);
            view?.SetRight(vo.rightDatVO);
            view?.SetMiddle(vo.diffDatVO);
            base.ExecuteCommand(new SetCompareViewVisibleCmd(true));
        }
        private void OnCompareClick() =>
            base.ExecuteCommand(new CreateDatComparisonCmd());
        

        private void OnFormClosing() 
        {
            ClearView();
            base.ExecuteCommand(new ClearCompareDatsModelAndViewCmd());
        }

        private void OnCustomiseClick(DatVO datVO) 
        {
            base.ExecuteCommand(new SetCustomiseViewCmd(datVO));
        }

        private void OnCreateDatClick(DatVO datVO) 
        {
            base.ExecuteCommand(new ShowCreateDatCmd(datVO));
        }
    }
}
