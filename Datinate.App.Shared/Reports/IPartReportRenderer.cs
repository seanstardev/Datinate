using Datinate.Shared;

namespace Datinate.App.Shared.Reports
{
    public interface IPartReportRenderer
    {
        ReportImage? RenderAutomatedReport(string report);

        ReportImage? RenderCustomisedReport(ManagedListItemReport report);
    }
}
