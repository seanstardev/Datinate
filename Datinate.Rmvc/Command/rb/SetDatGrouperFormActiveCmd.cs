using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetDatGrouperFormActiveCmd : CommandBase<RVoid>
    {
        private readonly string? title;

        public SetDatGrouperFormActiveCmd(string? title = null)
        {
            this.title = title;
        }

        protected override async Task<RVoid> Run()
        {
            base.Shell?.ShowDatGrouperWindowView();

            if (string.IsNullOrWhiteSpace(title) == false && title != null)
                base.Shell?.SetProjectsFormTitle(title);

            return await Complete();
        }
    }
}
