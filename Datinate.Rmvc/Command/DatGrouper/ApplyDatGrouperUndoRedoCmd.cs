using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ApplyDatGrouperUndoRedoCmd : CommandBase<RVoid>
    {
        private readonly bool performUndo;

        public ApplyDatGrouperUndoRedoCmd(bool performUndo)
        {
            this.performUndo = performUndo;
        }

        protected override async Task<RVoid> Run()
        {
            if (base.DatGrouperModel == null)
                return await Complete();

            var familiesAtRisk =
                performUndo
                    ? base.DatGrouperModel.GetUndoMediaAssociationRisks()
                    : base.DatGrouperModel.GetRedoMediaAssociationRisks();

            var assignedMediaWouldBeLost =
                familiesAtRisk.Any(
                    family =>
                        base.RadioDatModel?.HasAssignedMedia(family) == true);

            if (assignedMediaWouldBeLost)
            {
                var confirmed =
                    await ConfirmYesNoAsync(
                        "This operation may cause media associations to be lost. Do you want to continue?",
                        base.Shell);

                if (!confirmed)
                    return await Complete();
            }

            IDatGrouperDelta? delta =
                performUndo
                    ? base.DatGrouperModel.ApplyUndo()
                    : base.DatGrouperModel.ApplyRedo();

            if (delta != null)
            {
                await base.Execute(
                    new ApplyDatGrouperEditCmd(
                        delta,
                        true));
            }

            return await Complete();
        }

        private static Task<bool> ConfirmYesNoAsync(string message, IShell? shell)
        {
            return shell == null
                ? Task.FromResult(true)
                : shell.ShowMessageBox("Attention", message, true);
        }
    }
}