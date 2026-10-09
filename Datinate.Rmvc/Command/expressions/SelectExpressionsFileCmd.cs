using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class SelectExpressionsFileCmd :RCommandAsync
    {
        private readonly EXPRESSIONS_FILE_TARGET_ENUM expressionsFileTargetEnum;

        public SelectExpressionsFileCmd(EXPRESSIONS_FILE_TARGET_ENUM expressionsFileTargetEnum) 
        {
            this.expressionsFileTargetEnum = expressionsFileTargetEnum;
        }

        protected override async Task RunAsync()
        {
            var expressionsProxy = Facade.Instance?.ExpressionsProxy;
            var shell = Facade.Instance?.Shell;

            if (expressionsProxy?.ExpressionsPath == null || shell == null)
                return;

            var expressionsFullpath = await shell.ShowLoadExpressionsDialog(
                expressionsProxy.ExpressionsPath);

            if (expressionsFullpath == null || string.IsNullOrWhiteSpace(expressionsFullpath))
                return;

            var filters =
                expressionsProxy.FetchExpressions(expressionsFullpath);

            if (expressionsFileTargetEnum == EXPRESSIONS_FILE_TARGET_ENUM.CUSTOM_LIST)
                base.ExecuteCommand(new ApplyExpressionsFileCmd(filters));
            else
                base.ExecuteCommand(new SetProjectLoaderDatExpressionsCmd(expressionsFullpath));
        }
    }
}
 