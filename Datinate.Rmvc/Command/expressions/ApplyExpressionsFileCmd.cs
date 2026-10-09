using Datinate.Rmvc.Mediator;
using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ApplyExpressionsFileCmd:RCommand 
    {
        private readonly DatFilter[] expressions;

        public ApplyExpressionsFileCmd(DatFilter[] expressions) 
        {
            this.expressions = expressions;
        }
        protected override void Run() 
        {
            Facade? context = Facade.Instance;
            CustomListMediator? customListMediator = context?.CustomListMediator;
            customListMediator?.ApplyExpressions(expressions);
        }
    }
}
