using datinate.shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class SaveExpressionsCmd:RCommand 
    {
        private readonly DatFilter[] expressions;

        public SaveExpressionsCmd(DatFilter[] expressions) 
        {
            this.expressions = expressions;
        }

        protected override void Run() 
        {
            Facade? context = Facade.Instance;
            var expressionsProxy = context?.ExpressionsProxy;

            expressionsProxy?.SaveExpressions(expressions);
        }
    }
}
