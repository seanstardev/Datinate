using RMVC;

namespace Datinate.Rmvc.Command
{
    public class ShowDatHierarchyCmd : RCommandAsync
    {
        private readonly string[] paths;

        public ShowDatHierarchyCmd(string[] paths)
        {
            this.paths = paths;
        }

        protected override async Task RunAsync()
        {
            base.ExecuteCommand(new ShowProgressCmd("Loading View", 1, 1));

            Facade.Instance?.Shell?.SetProgressFormVisible(false);
            base.ExecuteCommand(new ClearProgressCmd());

            Facade.Instance?.Shell?.ApplicationDoEventsHack();
            //Application.DoEvents();
        }
    }
}
