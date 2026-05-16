using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal class SelectExpressionsFileCmd:RCommand 
    {
        private readonly EXPRESSIONS_FILE_TARGET_ENUM expressionsFileTargetEnum;

        public SelectExpressionsFileCmd(EXPRESSIONS_FILE_TARGET_ENUM expressionsFileTargetEnum) 
        {
            this.expressionsFileTargetEnum = expressionsFileTargetEnum;
        }

        protected override void Run() 
        {
            var context = Facade.Instance;
            var expressionsProxy = context?.ExpressionsProxy;

            expressionsProxy?.ShowLoadExpressionsDialog(expressionsFileTargetEnum);
        }
    }
}
