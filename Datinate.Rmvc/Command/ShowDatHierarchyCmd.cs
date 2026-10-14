using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ShowDatHierarchyCmd : CommandBase<RVoid>
    {
        private readonly string[] paths;

        public ShowDatHierarchyCmd(string[] paths)
        {
            this.paths = paths;
        }

        protected override async Task<RVoid> Run()
        {
            await base.Execute(new ShowProgressCmd("Loading View", 1, 1));

            base.Shell?.SetProgressFormVisible(false);
            await base.Execute(new ClearProgressCmd());

            base.Shell?.ApplicationDoEventsHack();

            return await Complete();
        }
    }
}
