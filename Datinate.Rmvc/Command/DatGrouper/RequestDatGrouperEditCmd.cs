using Datinate.Rmvc.Dto;
using Datinate.Shared.DatGrouper;
using RMVC;
using static Datinate.Shared.DatGrouper.DatGrouperEditRequestDTO;

namespace Datinate.Rmvc.Command
{
    internal class RequestDatGrouperEditCmd : CommandBase<RVoid>
    {
        private readonly DatGrouperEditRequestDTO dto;

        public RequestDatGrouperEditCmd(DatGrouperEditRequestDTO dto)
        {
            this.dto = dto;
        }

        protected override async Task<RVoid> Run()
        {
            if (base.DatGrouperModel == null ||
                dto.EditActionEnum == EDIT_ACTION_ENUM.NOT_SET)
            {
                return await Complete();
            }

            var familiesAtRisk =
                base.DatGrouperModel.GetUpdateMediaAssociationRisks(
                    dto);

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

            DatGrouperEditDelta? delta =
                base.DatGrouperModel.PerformUpdate(dto);

            if (delta != null)
            {
                await base.Execute(
                    new ApplyDatGrouperEditCmd(
                        delta,
                        true));
            }

            return await Complete();
        }

        private static Task<bool> ConfirmYesNoAsync(
            string message, IShell? shell)
        {
            return shell == null
                ? Task.FromResult(true)
                : shell.ShowMessageBox("Attention", message, true);
        }
    }
}