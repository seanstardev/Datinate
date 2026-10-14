using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SaveExpressionsCmd : CommandBase<RVoid>
    {
        private readonly DatFilter[] expressions;

        public SaveExpressionsCmd(DatFilter[] expressions) 
        {
            this.expressions = expressions;
        }

        protected override async Task<RVoid> Run() 
        {
            if (base.ExpressionsProxy?.ExpressionsPath == null || Shell == null)
                return await Complete();

            var expressionsFullpath = await base.Shell.ShowSaveExpressionsDialog(
                base.ExpressionsProxy.ExpressionsPath);

            if (string.IsNullOrWhiteSpace(expressionsFullpath))
                return await Complete();

            bool success = base.ExpressionsProxy.SaveExpressions(
                expressions, 
                expressionsFullpath!);

            if (success)
                await base.Shell.ShowMessageBox("OK", "Expressions file saved.");
            else
                await base.Shell.ShowMessageBox(
                    "There was a problem",
                    "The Expressions file could not be saved");

            return await Complete();
        }
    }
}
