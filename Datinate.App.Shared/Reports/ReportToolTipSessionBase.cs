using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Datinate.App.Shared.Reports
{
    /// <summary>
    /// Owns latest-click tooltip scheduling, busy state and displayed image lifetime.
    /// Use the session on the host's UI thread; this
    /// class does not dispatch native operations or add a thread-safety boundary.
    /// </summary>
    public abstract class ReportToolTipSessionBase<TImage> : IDisposable
        where TImage : class, IDisposable
    {
        private IHtmlReportCapture<TImage>? reportCapture;
        private PendingReport? pendingReport;
        private TImage? reportImage;
        private bool processing;
        private bool disposed;
        private int requestVersion;

        protected bool IsDisposed => disposed;

        // The native view borrows this image for drawing. Only this session disposes it.
        protected TImage? CurrentImage => reportImage;

        protected abstract bool CanPresentReport { get; }
        // Create a fresh service for warm-up/first use and again after a failure.
        // The session owns and disposes it; native tooltip hosts need no browser hooks.
        protected abstract IHtmlReportCapture<TImage> CreateCapture();

        protected abstract void ShowImage(TImage image);
        protected abstract void HideImage();
        protected abstract void SetBusy(bool busy);

        protected virtual void Disposing() { }

        // Warm up when the owning view loads. First clicks share the same
        // initialisation task rather than starting a second browser.
        public async Task WarmUpAsync()
        {
            if (disposed)
                return;

            try { await GetCapture().WarmUpAsync(); }
            catch (Exception ex)
            {
                if (!disposed)
                    Debug.WriteLine($"[REPORT TOOLTIP] Browser warm-up failed: {ex}");

                ResetCapture();
            }
        }

        // The native Show method calls Hide(), attaches its new origin and then
        // queues the report. Controls and pointer coordinates never enter Shared.
        protected void QueueReport(string reportHtml, ReportCaptureOptions options)
        {
            if (disposed || !CanPresentReport)
                return;

            pendingReport = new PendingReport(++requestVersion, reportHtml, options);
            SetBusy(true);

            if (!processing)
                _ = ProcessPendingReportsAsync();
        }

        public void Hide()
        {
            if (!disposed)
                HideCore();
        }

        private void HideCore()
        {
            // Invalidate both the pending click and any capture already in flight.
            requestVersion++;
            pendingReport = null;

            try { SetBusy(false); }
            finally
            {
                try { HideImage(); }
                finally
                {
                    TImage? image = reportImage;
                    reportImage = null;
                    image?.Dispose();
                }
            }
        }

        private async Task ProcessPendingReportsAsync()
        {
            processing = true;

            try
            {
                // One active capture plus one replaceable pending click. A newer
                // click never waits behind a queue of obsolete report captures.
                while (!disposed && pendingReport != null)
                {
                    PendingReport request = pendingReport;
                    pendingReport = null;

                    try
                    {
                        await GetCapture().WarmUpAsync();

                        if (!IsCurrent(request))
                            continue;

                        TImage image = await GetCapture().CaptureAsync(request.Html, request.Options);

                        if (!IsCurrent(request))
                        {
                            image.Dispose();
                            continue;
                        }

                        reportImage?.Dispose();
                        reportImage = image;
                        ShowImage(image);
                    }
                    catch (Exception ex)
                    {
                        if (!disposed)
                        {
                            Debug.WriteLine($"[REPORT TOOLTIP] Capture failed: {ex}");
                            ResetCapture();

                            if (IsCurrent(request))
                                Hide();
                        }
                    }
                    finally
                    {
                        if (IsCurrent(request))
                            SetBusy(false);
                    }
                }
            }
            finally
            {
                processing = false;
            }
        }

        private bool IsCurrent(PendingReport request)
            => !disposed && request.Version == requestVersion && CanPresentReport;

        private IHtmlReportCapture<TImage> GetCapture()
            => reportCapture ??= CreateCapture();

        private void ResetCapture()
        {
            IHtmlReportCapture<TImage>? capture = reportCapture;
            reportCapture = null;
            capture?.Dispose();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            try { HideCore(); }
            finally
            {
                try { ResetCapture(); }
                finally { Disposing(); }
            }
        }

        private sealed class PendingReport
        {
            public int Version { get; }
            public string Html { get; }
            public ReportCaptureOptions Options { get; }

            public PendingReport(int version, string reportHtml, ReportCaptureOptions options)
            {
                Version = version;
                Html = reportHtml;
                Options = options;
            }
        }
    }
}
