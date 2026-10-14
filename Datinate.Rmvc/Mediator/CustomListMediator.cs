using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.Dat;
using Datinate.Shared.Rmvc;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Mediator
{
    internal class CustomListMediator : RMediator<ICustomListView>
    {
        protected override void Initialised()
        {
            if (actor != null)
            {
                actor.CreateDatClickEvt += OnCreateDatClick;
                actor.FormClosingEvt += OnFormClosing;

                actor.LoadExpressionsEvt += OnLoadExpressions;
                actor.SaveExpressionsEvt += OnSaveExpressions;
            }
        }

        protected override void Disposing()
        {
            if (actor != null)
            {
                actor.CreateDatClickEvt -= OnCreateDatClick;
                actor.FormClosingEvt -= OnFormClosing;

                actor.LoadExpressionsEvt -= OnLoadExpressions;
                actor.SaveExpressionsEvt -= OnSaveExpressions;
            }
        }
        public void ApplyExpressions(DatFilter[] expressions) 
            => base.actor?.ApplyExpressions(expressions);
        
        public void SetView(DatVO datVO, Flag[] flags, Flag[] categories)
            => base.actor?.SetView(flags, datVO, categories);
        
        public void SetView(DatVO datVO, Flag[] flags, Flag[] categories, DatFilter[] expressions) 
        {
            actor?.SetView(flags, datVO, categories);
            ApplyExpressions(expressions);
        }

        private void OnFormClosing() 
        {
            actor?.Hide();
            actor?.ClearAll();
            base.Execute(new ClearCustomiseDatModelCmd());
        }

        private void OnCreateDatClick(DatVO? datVO) 
        {
            if (datVO == null) {
                base.Execute(new ShowMessageCmd("There is no DAT to create."));
            }
            else
                base.Execute(new ShowCreateDatCmd(datVO));
        }

        private void OnLoadExpressions()
            => base.Execute(new SelectExpressionsFileCmd(
                EXPRESSIONS_FILE_TARGET_ENUM.CUSTOM_LIST));
        
        private void OnSaveExpressions(DatFilter[] expressions) 
            => base.Execute(new SaveExpressionsCmd(expressions));
        
    }
}
