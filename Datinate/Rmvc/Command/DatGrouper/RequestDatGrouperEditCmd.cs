using app.datinate;
using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatGrouperEditRequestDTO;

namespace com.RADIO.Datinate.RMVC
{
    public class RequestDatGrouperEditCmd : RCommandAsync
    {
        private readonly DatGrouperEditRequestDTO dto;

        public RequestDatGrouperEditCmd(DatGrouperEditRequestDTO dto)
        {
            this.dto = dto;
        }

        protected override async Task RunAsync()
        {
            if (Facade.Instance?.DatGrouperModel == null ||
                dto.EditActionEnum == EDIT_ACTION_ENUM.NOT_SET)
            {
                return;
            }

            var model =
                Facade.Instance.DatGrouperModel;

            var radioDatModel =
                Facade.Instance.RadioDatModel;

            var familiesAtRisk =
                model.GetUpdateMediaAssociationRisks(
                    dto);

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

            DatGrouperEditDelta? delta =
                model.PerformUpdate(dto);

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