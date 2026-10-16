using Datinate.Shared.Radio;
using RMVC;
using System.Diagnostics;

namespace Datinate.Rmvc.Command
{
    internal class ShowRbPartGroupingReportsCmd : CommandBase<RVoid>
    {
        private readonly IGamePart part;

        public ShowRbPartGroupingReportsCmd(IGamePart part)
        {
            this.part = part;
        }

        protected override async Task<RVoid> Run()
        {
            if (base.MainWebMediator == null || base.RadioDatModel?.AutoGroupTraceStore == null)
            {
                Debug.WriteLine("parts cmd: " + base.MainWebMediator + " -- " + base.RadioDatModel?.AutoGroupTraceStore);
                
                return await Complete();
            }

            base.RadioDatModel.AutoGroupTraceStore.TryGetPartReport(part, out var automatedReport);

            var managedReport = base.RadioDatModel.AutoGroupTraceStore.GetManagedReport(
                part.GetName(), part.GetDirectoryId()!);

            string? htmlPage = base.MainWebMediator.RenderGamePartGroupingReports(
                part, managedReport, automatedReport);

            if (!string.IsNullOrWhiteSpace(htmlPage) && htmlPage != null)
                base.MainWebMediator.LoadPageContent(htmlPage);

            return await Complete();
        }
    }
}
