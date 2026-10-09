using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    public class SetMainControlsBtnEnabledCmd : RCommand
    {
        private readonly MAIN_CONTROL_ENUM mainCtrlEnum;
        private readonly bool doSetEnabled;

        public SetMainControlsBtnEnabledCmd(MAIN_CONTROL_ENUM mainCtrlEnum, bool doSetEnabled)
        {
            this.mainCtrlEnum = mainCtrlEnum;
            this.doSetEnabled = doSetEnabled;
        }

        protected override void Run()
        {
            Facade.Instance?.MainControlsMediator?.SetMainControlEnabled(mainCtrlEnum, doSetEnabled);
        }
    }
}
