using Datinate.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SaveExpressionsCmd : RCommandAsync 
    {
        private readonly DatFilter[] expressions;

        public SaveExpressionsCmd(DatFilter[] expressions) 
        {
            this.expressions = expressions;
        }

        protected override async Task RunAsync() 
        {
            var expressionsProxy = Facade.Instance?.ExpressionsProxy;
            var shell = Facade.Instance?.Shell;

            if (expressionsProxy?.ExpressionsPath == null || shell == null)
                return;

            var expressionsFullpath = await shell.ShowSaveExpressionsDialog(
                expressionsProxy.ExpressionsPath);

            if (string.IsNullOrWhiteSpace(expressionsFullpath))
                return;

            bool success = expressionsProxy.SaveExpressions(
                expressions, 
                expressionsFullpath!);

            if (success)
                await shell.ShowMessageBox("OK", "Expressions file saved.");
            else
                await shell.ShowMessageBox(
                    "There was a problem",
                    "The Expressions file could not be saved");
        }
    }
}
