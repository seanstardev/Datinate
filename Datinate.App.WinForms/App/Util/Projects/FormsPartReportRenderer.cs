using Datinate.App.Shared.Reports;
using Datinate.Shared;

namespace Datinate.App.WinForms.View.Util
{
    internal sealed class FormsPartReportRenderer : IPartReportRenderer
    {
        private readonly Control fontAndDpiSource;

        public FormsPartReportRenderer(Control fontAndDpiSource)
            => this.fontAndDpiSource = fontAndDpiSource;

        public ReportImage? RenderAutomatedReport(string report)
        {
            string pngBase64 = DatGrouperPartToolTip.RenderFullReportToPngBase64(
                report,
                out int widthPx,
                out int heightPx,
                fontAndDpiSource.Font);

            if (string.IsNullOrEmpty(pngBase64))
                return null;

            float scale = 1f;

            if (!fontAndDpiSource.IsDisposed)
                scale = Math.Max(1f, fontAndDpiSource.DeviceDpi / 96f);

            int displayWidth = Math.Max(1, (int)Math.Round(widthPx / scale));
            int displayHeight = Math.Max(1, (int)Math.Round(heightPx / scale));

            return new ReportImage(pngBase64, displayWidth, displayHeight);
        }

        public ReportImage? RenderCustomisedReport(ManagedListItemReport report)
        {
            // Preserve the existing system font and unscaled dimensions for this report.
            string pngBase64 = ManagedListItemReportToolTip.RenderToPngBase64(
                report,
                out int widthPx,
                out int heightPx);

            if (string.IsNullOrEmpty(pngBase64))
                return null;

            return new ReportImage(pngBase64, widthPx, heightPx);
        }
    }
}
