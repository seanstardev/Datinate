using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class CheckDatGrouperProjectValidCmd : RCommand
    {
        public bool DatAndExpressionFilesExist { get; private set; }
        private readonly DatGrouperProjectDTO dto;

        public CheckDatGrouperProjectValidCmd(DatGrouperProjectDTO dto)
        {
            this.dto = dto;
        }

        protected override void Run()
        {
            DatAndExpressionFilesExist =
                DatGrouperProjectDTO.GetAllDatFilesExist(dto) &&
                DatGrouperProjectDTO.GetAllExpressionFilesExistOrAreEmpty(dto);
        }
    }
}
