using Datinate.Shared.DatGrouper;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class CheckDatGrouperProjectValidCmd : RCommand
    {
        public bool AllDatAndExpressionFilesExist { get; private set; }
        private readonly DatGrouperProjectDTO dto;

        public CheckDatGrouperProjectValidCmd(DatGrouperProjectDTO dto)
        {
            this.dto = dto;
        }

        protected override void Run()
        {
            AllDatAndExpressionFilesExist =
                DatGrouperProjectDTO.GetAllDatFilesExist(dto) &&
                DatGrouperProjectDTO.GetAllExpressionFilesExistOrAreEmpty(dto) &&
                dto.GetAllDatContentPathsAreValidOrEmpty();
        }
    }
}
