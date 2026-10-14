using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class SetWebSearchEngineCmd : CommandBase<RVoid>
    {
        private readonly WEB_SOURCE_ENUM engine;
        private readonly bool invokeSearchNow;

        public SetWebSearchEngineCmd(WEB_SOURCE_ENUM engine, bool invokeSearchNow)
        {
            this.engine = engine;
            this.invokeSearchNow = invokeSearchNow;
        }

        protected override async Task<RVoid> Run()
        {
            base.RbWebSearchMediator?.SetSearchEngine(engine, invokeSearchNow);

            return await Complete();
        }
    }
}
