using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal class SetDatGrouperFormVisibleCmd : RCommand
    {
        protected override void Run() 
        {
            base.ExecuteCommand(new SetMainControlsBtnEnabledCmd(MAIN_CONTROL_ENUM.DatGrouper, true));
            Facade.Instance?.Shell?.SetProjectsFormVisible(true);
        }
    }
}
