using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class SetDatGrouperFormVisibleCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run() 
        {
            await base.Execute(new SetMainControlsBtnEnabledCmd(
                MAIN_CONTROL_ENUM.DatGrouper, true));

            base.Shell?.SetProjectsFormVisible(true);

            return await Complete();
        }
    }
}
