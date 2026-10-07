using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal class SwitchDatViewCmd:RCommand 
    {
        private readonly DAT_SCREEN_ENUM datScreenEnum;

        public SwitchDatViewCmd(DAT_SCREEN_ENUM datScreenEnum) 
        {
            this.datScreenEnum = datScreenEnum;
        }
        protected override void Run() 
        {
            Facade.Instance?.MainMediator?.ToggleDatListView(datScreenEnum);
        }
    }
}
