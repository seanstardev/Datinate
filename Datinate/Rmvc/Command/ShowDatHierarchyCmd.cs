using RMVC;

namespace com.RADIO.Datinate.RMVC
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
            await base.ExecuteCommandAsync(new ShowProgressCmd("Loading View", 1, 1));

            Facade.Instance?.Shell?.SetProgressFormVisible(false);
            base.ExecuteCommand(new ClearProgressCmd());
            Application.DoEvents();
        }
    }
}
