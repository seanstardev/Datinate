using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetExcludeFamiliesInViewCmd : CommandBase<RVoid>
    {
        private readonly bool doExclude;

        public SetExcludeFamiliesInViewCmd(bool doExclude)
        {
            this.doExclude = doExclude;
        }

        protected override async Task<RVoid> Run()
        {
            base.DatGrouperMediator?.SetExcludeFamiliesVisible(doExclude);

            return await Complete();
        }
    }
}
