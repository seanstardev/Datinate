using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ExitDatGrouperProjectsCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            await base.Execute(new ClearDatGrouperSessionCmd());

            await base.Execute(new LoadDatGrouperProjectsCmd(
                null,
                false,
                true));

            base.Shell?.SetProjectsFormVisible(false);

            return await Complete();
        }
    }
}
