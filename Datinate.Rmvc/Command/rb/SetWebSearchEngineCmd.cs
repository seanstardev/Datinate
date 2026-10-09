using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class SetWebSearchEngineCmd : RCommand
    {
        private readonly WEB_SOURCE_ENUM engine;
        private readonly bool invokeSearchNow;

        public SetWebSearchEngineCmd(WEB_SOURCE_ENUM engine, bool invokeSearchNow)
        {
            this.engine = engine;
            this.invokeSearchNow = invokeSearchNow;
        }

        protected override void Run()
        {
            Facade.Instance?.RbWebSearchMediator?.SetSearchEngine(engine, invokeSearchNow);
        }
    }
}
