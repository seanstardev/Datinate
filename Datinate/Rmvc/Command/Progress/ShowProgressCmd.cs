using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class ShowProgressCmd : RCommand
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

        protected override void Run()
        {
            Facade.Instance?.Shell?.SetProgressFormVisible(true);
            Facade.Instance?.ProgressMediator?.UpdateProgress(message, part, total);
        }
    }
}