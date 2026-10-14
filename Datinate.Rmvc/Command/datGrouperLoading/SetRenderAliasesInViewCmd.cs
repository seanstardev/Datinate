using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetRenderAliasesInViewCmd : CommandBase<RVoid>
    {
        private readonly bool doRender;

        public SetRenderAliasesInViewCmd(bool doRender)
        {
            this.doRender = doRender;
        }

        protected override async Task<RVoid> Run()
        {
            base.DatGrouperMediator?.SetRenderAliases(doRender);

            return await Complete();
        }
    }
}
