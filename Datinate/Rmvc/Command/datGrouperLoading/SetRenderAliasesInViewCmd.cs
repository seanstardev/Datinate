using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SetRenderAliasesInViewCmd : RCommand
    {
        private readonly bool doRender;

        public SetRenderAliasesInViewCmd(bool doRender)
        {
            this.doRender = doRender;
        }

        protected override void Run()
        {
            Facade.Instance?.DatGrouperMediator?.SetRenderAliases(doRender);
        }
    }
}
