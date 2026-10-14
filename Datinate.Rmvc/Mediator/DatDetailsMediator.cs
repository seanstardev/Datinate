using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.Dat;
using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    internal class DatDetailsMediator : RMediator<IDatDetailsView>
    {
        public void SetView(DatVO datVO, UnitFormatHelper.Unit unit, bool showUnitInCells) 
            => actor?.SetView(datVO, unit, showUnitInCells);
        
        public void UpdateUnitsView(UnitFormatHelper.Unit unit, bool showUnitInCells) 
            => actor?.UpdateUnits(unit, showUnitInCells);
        
        public void ResetView() 
            => actor?.ClearView();
        
        private void OnCompareRight() 
            => base.Execute(new SetCompareViewSideCmd(false));
        
        private void OnCompareLeft() 
            => base.Execute(new SetCompareViewSideCmd(true));
        
        private void OnCustomiseClick(DatVO datVO) 
            => base.Execute(new SetCustomiseViewCmd(datVO));
        
        private void OnShowAddToProject(DatVO datVO) 
            => base.Execute(new ShowAddToProjectViewCmd(datVO));
        
        private void OnDefaultAppFail() 
        {
            base.Execute(
                new ShowMessageCmd(
                    "The selected DAT's file type does not have a default application associated with it, and cannot be viewed. The "
                    + Constants.APP_NAME
                    + " team recommends setting DAT file types (.dat and .xml) to automatically open with a tool like Notepad++."
            ));
        }

        protected override void Initialised()
        {
            if (actor != null)
            {
                actor.CustomiseClickEvt += OnCustomiseClick;
                actor.CompareLeftEvt += OnCompareLeft;
                actor.CompareRightEvt += OnCompareRight;
                actor.ReadDatInDefaultAppFailEvt += OnDefaultAppFail;
                actor.ShowAddToProjectEvt += OnShowAddToProject;
            }
        }

        protected override void Disposing()
        {
            if (actor != null)
            {
                actor.CustomiseClickEvt -= OnCustomiseClick;
                actor.CompareLeftEvt -= OnCompareLeft;
                actor.CompareRightEvt -= OnCompareRight;
                actor.ReadDatInDefaultAppFailEvt -= OnDefaultAppFail;
                actor.ShowAddToProjectEvt -= OnShowAddToProject;
            }
        }
    }
}