using RMVC;

namespace Datinate.Rmvc.Command
{
    public class SetExcludeFamiliesInViewCmd : RCommand
    {
        private readonly bool doExclude;

        public SetExcludeFamiliesInViewCmd(bool doExclude)
        {
            this.doExclude = doExclude;
        }

        protected override void Run()
        {
            Facade.Instance?.DatGrouperMediator?.SetExcludeFamiliesVisible(doExclude);
        }
    }
}
