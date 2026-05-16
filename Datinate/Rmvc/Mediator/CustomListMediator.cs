using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using Datinate.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal class CustomListMediator : RMediator
    {
        private ICustomListView? view => (ICustomListView?)base.viewBase;

        public CustomListMediator(Type actor) : base(actor)
        {
        }

        protected override void Initialsed()
        {
            if (view != null)
            {
                view.CreateDatClickEvt += OnCreateDatClick;
                view.FormClosingEvt += OnFormClosing;

                view.LoadExpressionsEvt += OnLoadExpressions;
                view.SaveExpressionsEvt += OnSaveExpressions;
            }
        }

        protected override void Disposing()
        {
            if (view != null)
            {
                view.CreateDatClickEvt -= OnCreateDatClick;
                view.FormClosingEvt -= OnFormClosing;

                view.LoadExpressionsEvt -= OnLoadExpressions;
                view.SaveExpressionsEvt -= OnSaveExpressions;
            }
        }
        public void ApplyExpressions(DatFilter[] expressions) 
        {
            view?.ApplyExpressions(expressions);
        }

        public void SetView(DatVO datVO, Flag[] flags, Flag[] categories) 
        {
            view?.SetView(flags, datVO, categories);
        }

        public void SetView(DatVO datVO, Flag[] flags, Flag[] categories, DatFilter[] expressions) 
        {
            view?.SetView(flags, datVO, categories);
            ApplyExpressions(expressions);
        }

        private void OnFormClosing() 
        {
            view?.Hide();
            view?.ClearAll();
            base.ExecuteCommand(new ClearCustomiseDatModelCmd());
        }

        private void OnCreateDatClick(DatVO? datVO) 
        {
            if (datVO == null) {
                base.ExecuteCommand(new ShowMessageCmd("There is no DAT to create."));
            }
            else
                base.ExecuteCommand(new ShowCreateDatCmd(datVO));
        }

        private void OnLoadExpressions() 
        {
            base.ExecuteCommand(new SelectExpressionsFileCmd(EXPRESSIONS_FILE_TARGET_ENUM.CUSTOM_LIST));
        }
        private void OnSaveExpressions(DatFilter[] expressions) 
        {
            base.ExecuteCommand(new SaveExpressionsCmd(expressions));
        }
    }
}
