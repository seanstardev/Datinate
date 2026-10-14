using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class SetMainControlsBtnEnabledCmd : CommandBase<RVoid>
    {
        private readonly MAIN_CONTROL_ENUM mainCtrlEnum;
        private readonly bool doSetEnabled;

        public SetMainControlsBtnEnabledCmd(MAIN_CONTROL_ENUM mainCtrlEnum, bool doSetEnabled)
        {
            this.mainCtrlEnum = mainCtrlEnum;
            this.doSetEnabled = doSetEnabled;
        }

        protected override async Task<RVoid> Run()
        {
            base.MainControlsMediator?.SetMainControlEnabled(
                mainCtrlEnum, doSetEnabled);

            return await Complete();
        }
    }
}
