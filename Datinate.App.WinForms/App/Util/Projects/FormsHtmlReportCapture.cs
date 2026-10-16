using Datinate.App.Shared.Reports;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Datinate.App.WinForms.View.Util
{
    // Native browser adapter only. HtmlReportCaptureBase owns the document,
    // measurement protocol, capture sequence, timeouts and request coordination.
    // Create, use and dispose this host on the same WinForms UI thread.
    internal sealed class FormsHtmlReportCapture : HtmlReportCaptureBase<Bitmap>
    {
        private const int OffscreenCoordinate = -32000;

        private readonly int uiThreadId = Environment.CurrentManagedThreadId;
        private CaptureHostForm host = null!;
        private WebView2 browser = null!;
        private CancellationTokenSource lifetime = null!;

        private CoreWebView2? core;
        private TaskCompletionSource<bool>? navigationCompletion;
        private bool browserReleased;

        protected override string PostMessageFunction => "message => window.chrome.webview.postMessage(message)";

        private void CreateBrowser()
        {
            browserReleased = false;
            lifetime = new CancellationTokenSource();
            host = new CaptureHostForm
            {
                Text = "HTML report capture host",
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(OffscreenCoordinate, OffscreenCoordinate),
                ClientSize = new Size(900, 720),
                FormBorderStyle = FormBorderStyle.None
            };

            browser = new WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = Color.White,
                AllowExternalDrop = false,
                TabStop = false
            };

            try
            {
                WebView2Helper.ApplyCreationProperties(browser);
                host.Controls.Add(browser);

                // CapturePreview needs a real browser viewport. Keep
                // the native window shown, without activation or a taskbar entry.
                host.Show();

            }
            catch
            {
                host.Dispose();
                throw;
            }
        }

        protected override async Task InitialiseBrowserAsync(string documentHtml, CancellationToken cancellationToken)
        {
            CheckThreadAccess();

            if (host == null || browserReleased)
                CreateBrowser();

            CheckAccess();
            using var operation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifetime.Token);
            core = await WebView2Helper.EnsureCoreAsync(browser).WaitAsync(operation.Token);
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreBrowserAcceleratorKeysEnabled = false;
            core.WebMessageReceived += OnWebMessageReceived;

            navigationCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            core.NavigationCompleted += OnNavigationCompleted;

            try
            {
                core.NavigateToString(documentHtml);

                // CapturePreview must follow the current document's ContentLoading.
                await navigationCompletion.Task.WaitAsync(operation.Token);
            }
            finally
            {
                if (!browserReleased)
                    core.NavigationCompleted -= OnNavigationCompleted;

                navigationCompletion = null;
            }
        }

        protected override async Task<string> ExecuteScriptAsync(string javascript, CancellationToken cancellationToken)
        {
            CheckAccess();
            using var operation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifetime.Token);

            return await browser.CoreWebView2.ExecuteScriptAsync(javascript).WaitAsync(operation.Token);
        }

        protected override void SetCaptureViewport(int width, int height, int targetDpi)
        {
            CheckAccess();

            // The offscreen window may belong to a different-DPI monitor from
            // the tooltip origin. Browser zoom renders at the requested pixel scale.
            browser.ZoomFactor = targetDpi / (double)host.DeviceDpi;
            host.ClientSize = new Size(width, height);
        }

        protected override async Task<Bitmap> CaptureImageAsync(CancellationToken cancellationToken)
        {
            CheckAccess();
            using var operation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lifetime.Token);
            var stream = new MemoryStream();
            Task? capture = null;

            try
            {
                capture = browser.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
                await capture.WaitAsync(operation.Token);
                stream.Position = 0;

                using var image = Image.FromStream(stream);

                return new Bitmap(image);
            }
            finally
            {
                // A timeout does not cancel the native capture. Keep its stream
                // alive until that callback completes, and observe any late fault.
                if (capture == null || capture.IsCompleted)
                    stream.Dispose();
                else
                    _ = DisposeCaptureStreamAsync(capture, stream);
            }
        }

        private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
                navigationCompletion?.TrySetResult(true);
            else
                navigationCompletion?.TrySetException(new InvalidOperationException(
                    $"Capture document navigation failed: {e.WebErrorStatus}."));
        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
            => base.ReceiveBrowserMessage(e.WebMessageAsJson);

        private static async Task DisposeCaptureStreamAsync(Task capture, Stream stream)
        {
            try { await capture; }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
            finally { stream.Dispose(); }
        }

        private void CheckAccess()
        {
            ObjectDisposedException.ThrowIf(browserReleased, this);

            CheckThreadAccess();
        }

        private void CheckThreadAccess()
        {
            if (Environment.CurrentManagedThreadId != uiThreadId)
                throw new InvalidOperationException("HTML capture must run on its owning UI thread.");
        }

        protected override void ReleaseBrowserResources()
        {
            if (browserReleased || host == null)
                return;

            CheckThreadAccess();
            browserReleased = true;
            lifetime.Cancel();
            navigationCompletion?.TrySetCanceled();

            if (core != null)
            {
                core.WebMessageReceived -= OnWebMessageReceived;
                core.NavigationCompleted -= OnNavigationCompleted;
            }

            host.Dispose();
            lifetime.Dispose();
        }

        private sealed class CaptureHostForm : Form
        {
            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams parameters = base.CreateParams;

                    const int NoActivate = 0x08000000;
                    const int ToolWindow = 0x00000080;

                    parameters.ExStyle |= NoActivate | ToolWindow;

                    return parameters;
                }
            }
        }
    }
}
