using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class SwitchDatViewCmd : CommandBase<RVoid>
    {
        private readonly DAT_SCREEN_ENUM datScreenEnum;

        public SwitchDatViewCmd(DAT_SCREEN_ENUM datScreenEnum) 
        {
            this.datScreenEnum = datScreenEnum;
        }
        protected override async Task<RVoid> Run() 
        {
            base.MainMediator?.ToggleDatListView(datScreenEnum);

            return await Complete();
        }
    }
}
