using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class ApplyDatGrouperUndoRedoCmd : RCommand
    {
        private readonly bool performUndo;

        public ApplyDatGrouperUndoRedoCmd(bool performUndo)
        {
            this.performUndo = performUndo;
        }

        protected override void Run()
        {
            IDatGrouperDelta? delta = null;

            if (performUndo)
                delta = Facade.Instance?.DatGrouperModel?.ApplyUndo();
            else
                delta = Facade.Instance?.DatGrouperModel?.ApplyRedo();

            if (delta != null)
                base.ExecuteCommand(new ApplyDatGrouperEditCmd(delta, true));
        }
    }
}
