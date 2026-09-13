using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class ApplyDatGrouperUndoRedoCmd : RCommandAsync
    {
        private readonly bool performUndo;

        public ApplyDatGrouperUndoRedoCmd(bool performUndo)
        {
            this.performUndo = performUndo;
        }

        protected override async Task RunAsync()
        {
            var model =
                Facade.Instance?.DatGrouperModel;

            if (model == null)
                return;

            var radioDatModel =
                Facade.Instance?.RadioDatModel;

            var familiesAtRisk =
                performUndo
                    ? model.GetUndoMediaAssociationRisks()
                    : model.GetRedoMediaAssociationRisks();

            var assignedMediaWouldBeLost =
                familiesAtRisk.Any(
                    family =>
                        radioDatModel?.HasAssignedMedia(family) == true);

            if (assignedMediaWouldBeLost)
            {
                var confirmed =
                    await ConfirmYesNoAsync(
                        "This operation may cause media associations to be lost. Do you want to continue?");

                if (!confirmed)
                    return;
            }

            IDatGrouperDelta? delta =
                performUndo
                    ? model.ApplyUndo()
                    : model.ApplyRedo();

            if (delta != null)
            {
                base.ExecuteCommand(
                    new ApplyDatGrouperEditCmd(
                        delta,
                        true));
            }
        }

        private static async Task<bool> ConfirmYesNoAsync(
            string message)
        {
            var shell =
                Facade.Instance?.Shell;

            if (shell == null)
                return true;

            return await shell.ShowMessageBox(
                "Attention",
                message,
                true);
        }
    }
}