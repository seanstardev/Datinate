using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class OpenMediaCollectionCmd : CommandBase<RVoid>
    {
        private readonly bool enterPreviewMode;
        private readonly DatGrouperEntryDTO dto;

        public OpenMediaCollectionCmd(
            bool enterPreviewMode,
            DatGrouperEntryDTO dto)
        {
            this.enterPreviewMode = enterPreviewMode;
            this.dto = dto;
        }

        protected override async Task<RVoid> Run()
        {
            await base.Execute(
                new EnterMediaModeCmd(enterPreviewMode, dto.IsFromAuto));

            if (!ErrorOrAbort)
                await base.Execute(new ShowMediaCollectionCmd(dto));

            return await Complete();
        }
    }
}