using Datinate.Shared.DatGrouper;

namespace Datinate.Rmvc.Command
{
    internal class CheckDatGrouperProjectValidCmd : CommandBase<bool>
    {
        private readonly DatGrouperProjectDTO dto;

        public CheckDatGrouperProjectValidCmd(DatGrouperProjectDTO dto)
        {
            this.dto = dto;
        }

        protected override async Task<bool> Run()
        {
            var allDatAndExpressionFilesExist =
                DatGrouperProjectDTO.GetAllDatFilesExist(dto) &&
                DatGrouperProjectDTO.GetAllExpressionFilesExistOrAreEmpty(dto) &&
                dto.GetAllDatContentPathsAreValidOrEmpty();

            return await Complete(allDatAndExpressionFilesExist);
        }
    }
}
