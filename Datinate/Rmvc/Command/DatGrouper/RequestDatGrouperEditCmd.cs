using app.datinate;
using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatGrouperEditRequestDTO;

namespace com.RADIO.Datinate.RMVC
{
    public class RequestDatGrouperEditCmd : RCommand
    {
        private readonly DatGrouperEditRequestDTO dto;

        public RequestDatGrouperEditCmd(DatGrouperEditRequestDTO dto)
        {
            this.dto = dto;
        }

        protected override void Run()
        {
            if (Facade.Instance?.DatGrouperModel  == null || dto.EditActionEnum == EDIT_ACTION_ENUM.NOT_SET)
                return;

            var model = Facade.Instance.DatGrouperModel;
            DatGrouperEditDelta? delta = model.PerformUpdate(dto);

            if (delta != null)
                base.ExecuteCommand(new ApplyDatGrouperEditCmd(delta, true));
        }
    }
}
