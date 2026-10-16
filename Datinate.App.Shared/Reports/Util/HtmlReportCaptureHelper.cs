using System.Text.Json;

namespace Datinate.App.Shared.Reports
{
    internal static class HtmlReportCaptureHelper
    {
        internal const string MessageChannel = "datinate-report-capture";

        // The capture host replaces #report's contents without navigating between clicks.
        internal static string CreateCaptureDocument()
            => "<!doctype html><html><head><meta charset=\"utf-8\">" +
               "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">" +
               "<style>html,body{margin:0;padding:0;overflow:hidden;background:white;}" +
               "#report{display:flow-root;width:100%;}" +
               "</style></head><body><div id=\"report\"></div></body></html>";

        // The host supplies its native message bridge; layout and readiness are shared.
        internal static string CreateMeasurementJavascript(
            string requestId,
            string contentHtml,
            int maximumWidth,
            int maximumHeight,
            bool replaceContent,
            string postMessageFunction)
        {
            string request = JsonSerializer.Serialize(new
            {
                requestId,
                contentHtml,
                maximumWidth,
                maximumHeight,
                replaceContent,
                channel = MessageChannel
            });

            return $@"
                (function () {{
                    const request = {request};
                    const post = {postMessageFunction};
                    const report = document.getElementById('report');

                    if (!report)
                        throw new Error('Capture document needs a #report element.');

                    if (request.replaceContent)
                        report.innerHTML = request.contentHtml;

                    const measure = async () => {{
                        await document.fonts.ready;

                        await Promise.all(Array.from(report.querySelectorAll('img')).map(image =>
                            image.complete ? Promise.resolve() : new Promise(resolve => {{
                                image.addEventListener('load', resolve, {{ once: true }});
                                image.addEventListener('error', resolve, {{ once: true }});
                            }})));

                        // Wait for layout/paint opportunities before requesting a capture.
                        await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));

                        const scale = window.devicePixelRatio || 1;
                        const rect = report.getBoundingClientRect();

                        post({{
                            channel: request.channel,
                            requestId: request.requestId,
                            width: Math.max(1, Math.min(request.maximumWidth, Math.ceil(rect.width * scale))),
                            height: Math.max(1, Math.min(request.maximumHeight, Math.ceil(rect.height * scale)))
                        }});
                    }};

                    void measure().catch(error => post({{
                        channel: request.channel, requestId: request.requestId, error: String(error)
                    }}));

                    return true;
                }})();";
        }
    }
}
