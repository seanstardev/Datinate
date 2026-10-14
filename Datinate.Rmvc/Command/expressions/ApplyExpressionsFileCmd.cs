using Datinate.Rmvc.Mediator;
using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class ApplyExpressionsFileCmd : CommandBase<RVoid> 
    {
        private readonly DatFilter[] expressions;

        public ApplyExpressionsFileCmd(DatFilter[] expressions) 
        {
            this.expressions = expressions;
        }
        protected override async Task<RVoid> Run() 
        {
            base.CustomListMediator?.ApplyExpressions(expressions);

            return await Complete();
        }
    }
}
