using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class DatDetailsMediator : RMediator 
    {
        public DatDetailsMediator(Type actor) : base(actor)
        {
        }

        private IDatDetailsView? view => (IDatDetailsView?)base.viewBase;

        public void SetView(DatVO datVO, UnitFormatHelper.Unit unit, bool showUnitInCells) 
        {
            view?.SetView(datVO, unit, showUnitInCells);
        }
        public void UpdateUnitsView(UnitFormatHelper.Unit unit, bool showUnitInCells) 
        {
            view?.UpdateUnits(unit, showUnitInCells);
        }

        public void ResetView() 
        {
            view?.ClearView();
        }

        private void OnCompareRight() 
        {
            base.ExecuteCommand(new SetCompareViewSideCmd(false));
        }

        private void OnCompareLeft() 
        {
            base.ExecuteCommand(new SetCompareViewSideCmd(true));
        }

        private void OnCustomiseClick(DatVO datVO) 
        {
            base.ExecuteCommand(new SetCustomiseViewCmd(datVO));
        }
        private void OnShowAddToProject(DatVO datVO) 
        {
            base.ExecuteCommand(new ShowAddToProjectViewCmd(datVO));
        }
        private void OnDefaultAppFail() 
        {
            base.ExecuteCommand(
                new ShowMessageCmd(
                    "The selected DAT's file type does not have a default application associated with it, and cannot be viewed. The "
                    + Constants.APP_NAME
                    + " team recommends setting DAT file types (.dat and .xml) to automatically open with a tool like Notepad++."
            ));
        }

        protected override void Initialsed()
        {
            if (view != null)
            {
                view.CustomiseClickEvt += OnCustomiseClick;
                view.CompareLeftEvt += OnCompareLeft;
                view.CompareRightEvt += OnCompareRight;
                view.ReadDatInDefaultAppFailEvt += OnDefaultAppFail;
                view.ShowAddToProjectEvt += OnShowAddToProject;
            }
        }

        protected override void Disposing()
        {
            if (view != null)
            {
                view.CustomiseClickEvt -= OnCustomiseClick;
                view.CompareLeftEvt -= OnCompareLeft;
                view.CompareRightEvt -= OnCompareRight;
                view.ReadDatInDefaultAppFailEvt -= OnDefaultAppFail;
                view.ShowAddToProjectEvt -= OnShowAddToProject;
            }
        }
    }
}
