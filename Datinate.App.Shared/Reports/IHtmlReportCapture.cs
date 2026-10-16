using System;
using System.Threading;
using System.Threading.Tasks;

namespace Datinate.App.Shared.Reports
{
    // The caller owns each returned image. The service owns its browser resources.
    public interface IHtmlReportCapture<TImage> : IDisposable
        where TImage : class, IDisposable
    {
        Task WarmUpAsync();

        Task<TImage> CaptureAsync(
            string reportHtml,
            ReportCaptureOptions options,
            CancellationToken cancellationToken = default);
    }
}
