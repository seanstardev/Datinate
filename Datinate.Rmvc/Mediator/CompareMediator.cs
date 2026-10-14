using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.Dat;
using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    internal class CompareMediator : RMediator<ICompareView>
    {
        protected override void Initialised()
        {
            if (actor != null)
            {
                actor.CustomiseClickEvt += OnCustomiseClick;
                actor.CompareClickEvt += OnCompareClick;
                actor.FormClosingEvt += OnFormClosing;
                actor.CreateDatClickEvt += OnCreateDatClick;
            }
        }

        protected override void Disposing()
        {
            if (actor != null)
            {
                actor.CustomiseClickEvt -= OnCustomiseClick;
                actor.CompareClickEvt -= OnCompareClick;
                actor.FormClosingEvt -= OnFormClosing;
                actor.CreateDatClickEvt -= OnCreateDatClick;
            }
        }
        public void ClearView() 
            => actor?.ClearView();

        public void SetLeft(DatVO datVO)
        {
            actor?.SetLeft(datVO);
            base.Execute(new SetCompareViewVisibleCmd(true));
        }

        public void SetRight(DatVO datVO)
        {
            actor?.SetRight(datVO);
            base.Execute(new SetCompareViewVisibleCmd(true));
        }

        public void UpdateUnitsView(UnitFormatHelper.Unit unit, bool showUnitInCells)
            => actor?.UpdateUnits(unit, showUnitInCells);
        
        public void SetComparisonResult(DatComparisonVO vo)
        {
            actor?.SetLeft(vo.leftDatVO);
            actor?.SetRight(vo.rightDatVO);
            actor?.SetMiddle(vo.diffDatVO);
            base.Execute(new SetCompareViewVisibleCmd(true));
        }
        private void OnCompareClick() 
            => base.Execute(new CreateDatComparisonCmd());
        
        private void OnFormClosing() 
        {
            ClearView();
            base.Execute(new ClearCompareDatsModelAndViewCmd());
        }

        private void OnCustomiseClick(DatVO datVO) 
            => base.Execute(new SetCustomiseViewCmd(datVO));

        private void OnCreateDatClick(DatVO datVO) 
            => base.Execute(new ShowCreateDatCmd(datVO));
    }
}
