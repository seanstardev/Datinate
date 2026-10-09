using RMVC;

namespace com.RADIO.Datinate.RMVC
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
