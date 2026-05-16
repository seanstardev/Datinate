using RadioLibCore.RadioDat;
using RMVC;
using System.Diagnostics;

namespace com.RADIO.Datinate.RMVC
{
    public class ShowRbPartGroupingReportsCmd : RCommand
    {
        private readonly IGamePart part;

        public ShowRbPartGroupingReportsCmd(IGamePart part)
        {
            this.part = part;
        }

        protected override void Run()
        {
            var web = Facade.Instance?.MainWebMediator;
            var radioDat = Facade.Instance?.RadioDatModel;

            if (web == null || radioDat?.AutoGroupTraceStore == null)
            {
                Debug.WriteLine("parts cmd: " + web + " -- " + radioDat?.AutoGroupTraceStore);
                return;
            }

            radioDat.AutoGroupTraceStore.TryGetPartReport(part, out var customiseReport);

            var managedReport = radioDat.AutoGroupTraceStore.GetManagedReport(
                part.GetName(), part.GetDirectoryId()!);

            string? htmlPage = web.RenderGamePartGroupingReports(part, managedReport, customiseReport);

            if (!string.IsNullOrWhiteSpace(htmlPage)) 
                web.LoadPageContent(htmlPage);
        }
    }
}
