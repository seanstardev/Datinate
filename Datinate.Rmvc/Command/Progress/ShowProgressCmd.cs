using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ShowProgressCmd : CommandBase<RVoid>
    {
        private readonly string message;
        private readonly int part;
        private readonly int total;

        public ShowProgressCmd(string message, int part, int total)
        {
            this.message = message;
            this.part = part;
            this.total = total;
        }

        protected override async Task<RVoid> Run()
        {
            base.Shell?.SetProgressFormVisible(true);
            base.ProgressMediator?.UpdateProgress(message, part, total);

            return await Complete();
        }
    }
}