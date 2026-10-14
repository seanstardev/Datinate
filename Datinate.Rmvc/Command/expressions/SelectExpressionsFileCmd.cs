using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class SelectExpressionsFileCmd : CommandBase<RVoid>
    {
        private readonly EXPRESSIONS_FILE_TARGET_ENUM expressionsFileTargetEnum;

        public SelectExpressionsFileCmd(EXPRESSIONS_FILE_TARGET_ENUM expressionsFileTargetEnum) 
        {
            this.expressionsFileTargetEnum = expressionsFileTargetEnum;
        }

        protected override async Task<RVoid> Run()
        {
            if (base.ExpressionsProxy?.ExpressionsPath == null || base.Shell == null)
                return await Complete();

            var expressionsFullpath = await base.Shell.ShowLoadExpressionsDialog(ExpressionsProxy.ExpressionsPath);

            if (expressionsFullpath == null || string.IsNullOrWhiteSpace(expressionsFullpath))
                return await Complete();

            var filters =
                base.ExpressionsProxy.FetchExpressions(expressionsFullpath);

            if (expressionsFileTargetEnum == EXPRESSIONS_FILE_TARGET_ENUM.CUSTOM_LIST)
                await base.Execute(new ApplyExpressionsFileCmd(filters));
            else
                await base.Execute(new SetProjectLoaderDatExpressionsCmd(expressionsFullpath));

            return await Complete();
        }
    }
}
 