using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Datinate.App.Shared.Reports
{
    // Owns HTML measurement, capture sequencing and browser lifetime. Use on the
    // host's UI thread; native operations are not automatically dispatched.
    public abstract class HtmlReportCaptureBase<TImage> : IHtmlReportCapture<TImage>
        where TImage : class, IDisposable
    {
        private const int MaximumWidth = 918;
        private const int MaximumHeight = 720;
        private const int OperationTimeoutSeconds = 15;

        private readonly CancellationTokenSource lifetime = new();
        private readonly SemaphoreSlim captureGate = new(1, 1);
        private Task<bool>? initialisationTask;
        private TaskCompletionSource<CaptureDimensions>? layoutCompletion;
        private string? layoutRequestId;
        private bool disposed;

        protected abstract string PostMessageFunction { get; }
        protected abstract Task InitialiseBrowserAsync(string documentHtml, CancellationToken cancellationToken);
        protected abstract Task<string> ExecuteScriptAsync(string javascript, CancellationToken cancellationToken);
        protected abstract void SetCaptureViewport(int width, int height, int targetDpi);

        // Return a detached image. Keep native callback resources alive until those
        // callbacks finish, even if cancellation abandons the capture in the meantime.
        protected abstract Task<TImage> CaptureImageAsync(CancellationToken cancellationToken);
        protected abstract void ReleaseBrowserResources();

        public Task WarmUpAsync()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            return EnsureInitialisedAsync();
        }

        private Task<bool> EnsureInitialisedAsync()
        {
            if (initialisationTask == null || initialisationTask.IsFaulted || initialisationTask.IsCanceled)
                initialisationTask = InitialiseCoreAsync();

            return initialisationTask;
        }

        private async Task<bool> InitialiseCoreAsync()
        {
            try
            {
                return await RunOperationAsync(async token =>
                {
                    await InitialiseBrowserAsync(HtmlReportCaptureHelper.CreateCaptureDocument(), token);
                    return true;
                });
            }
            catch
            {
                ReleaseBrowserResources();
                throw;
            }
        }

        // The caller owns the returned image. Capture is independent of tooltip presentation.
        public async Task<TImage> CaptureAsync(
            string reportHtml,
            ReportCaptureOptions options,
            CancellationToken cancellationToken = default)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            using var request = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, cancellationToken);
            CancellationToken token = request.Token;
            await captureGate.WaitAsync(token);

            try
            {
                token.ThrowIfCancellationRequested();
                await AwaitWithCancellationAsync(EnsureInitialisedAsync(), token);

                int width = Math.Min(MaximumWidth, options.MaximumWidth);
                int height = Math.Min(MaximumHeight, options.MaximumHeight);
                SetCaptureViewport(width, height, options.TargetDpi);

                CaptureDimensions measured = await MeasureAsync(reportHtml, width, height, replaceContent: true, token);
                SetCaptureViewport(measured.Width, measured.Height, options.TargetDpi);

                // Resizing the viewport requires another layout/paint opportunity.
                await MeasureAsync(reportHtml, width, height, replaceContent: false, token);

                return await RunOperationAsync(CaptureImageAsync, image => image.Dispose(), token);
            }
            catch
            {
                ResetBrowser();
                throw;
            }
            finally
            {
                layoutCompletion = null;
                layoutRequestId = null;
                captureGate.Release();
            }
        }

        private async Task<CaptureDimensions> MeasureAsync(string contentHtml, int width, int height, bool replaceContent, CancellationToken cancellationToken)
        {
            string requestId = Guid.NewGuid().ToString("N");
            var completion = new TaskCompletionSource<CaptureDimensions>(TaskCreationOptions.RunContinuationsAsynchronously);
            layoutRequestId = requestId;
            layoutCompletion = completion;

            string script = HtmlReportCaptureHelper.CreateMeasurementJavascript(
                requestId, contentHtml, width, height, replaceContent, PostMessageFunction);

            CaptureDimensions measured = await RunOperationAsync(async token =>
            {
                string result = await ExecuteScriptAsync(script, token);

                if (result != "true")
                    throw new InvalidOperationException("The HTML measurement script did not start.");

                return await AwaitWithCancellationAsync(completion.Task, token);
            }, cancellationToken: cancellationToken);

            if (measured.Width > width || measured.Height > height)
                throw new InvalidOperationException("The measured report exceeds its capture limits.");

            return measured;
        }

        protected void ReceiveBrowserMessage(string json)
        {
            if (disposed || layoutRequestId == null)
                return;

            try
            {
                using JsonDocument message = JsonDocument.Parse(json);
                JsonElement data = message.RootElement;

                if (data.ValueKind != JsonValueKind.Object ||
                    !data.TryGetProperty("channel", out JsonElement channel) || channel.ValueKind != JsonValueKind.String ||
                    channel.GetString() != HtmlReportCaptureHelper.MessageChannel ||
                    !data.TryGetProperty("requestId", out JsonElement requestId) || requestId.ValueKind != JsonValueKind.String ||
                    requestId.GetString() != layoutRequestId)
                {
                    return;
                }

                if (data.TryGetProperty("error", out JsonElement error))
                {
                    layoutCompletion?.TrySetException(new InvalidOperationException(error.ToString()));
                }
                else if (data.TryGetProperty("width", out JsonElement width) && width.ValueKind == JsonValueKind.Number &&
                         data.TryGetProperty("height", out JsonElement height) && height.ValueKind == JsonValueKind.Number &&
                         width.TryGetInt32(out int widthValue) && height.TryGetInt32(out int heightValue) &&
                         widthValue > 0 && heightValue > 0)
                {
                    layoutCompletion?.TrySetResult(new CaptureDimensions(widthValue, heightValue));
                }
                else
                {
                    layoutCompletion?.TrySetException(new InvalidOperationException("The report measurement is invalid."));
                }
            }
            catch (JsonException) { }
        }

        private async Task<T> RunOperationAsync<T>(
            Func<CancellationToken, Task<T>> action,
            Action<T>? releaseAbandonedResult = null,
            CancellationToken cancellationToken = default)
        {
            using var operation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, cancellationToken);
            operation.CancelAfter(TimeSpan.FromSeconds(OperationTimeoutSeconds));

            try
            {
                operation.Token.ThrowIfCancellationRequested();

                return await AwaitWithCancellationAsync(action(operation.Token), operation.Token, releaseAbandonedResult);
            }
            catch (OperationCanceledException) when (operation.IsCancellationRequested && !lifetime.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("The report browser operation has timed out.");
            }
        }

        // NOTE: netstandard2.0 has no Task.WaitAsync(CancellationToken). The native
        // operation receives the same token; late results still need ownership cleanup.
        private static async Task<T> AwaitWithCancellationAsync<T>(
            Task<T> task,
            CancellationToken token,
            Action<T>? releaseAbandonedResult = null)
        {
            if (task.IsCompleted)
                return await task;

            var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (token.Register(() => cancelled.TrySetResult(true)))
            {
                if (await Task.WhenAny(task, cancelled.Task) != task)
                {
                    _ = ObserveAbandonedAsync(task, releaseAbandonedResult);
                    token.ThrowIfCancellationRequested();
                }

                return await task;
            }
        }

        private static async Task ObserveAbandonedAsync<T>(Task<T> task, Action<T>? releaseResult)
        {
            try
            {
                T result = await task;
                releaseResult?.Invoke(result);
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }

        private void ResetBrowser()
        {
            layoutCompletion?.TrySetCanceled();
            layoutCompletion = null;
            layoutRequestId = null;
            initialisationTask = null;
            ReleaseBrowserResources();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            lifetime.Cancel();

            try { ResetBrowser(); }
            finally { lifetime.Dispose(); }
        }

        private readonly struct CaptureDimensions
        {
            public int Width { get; }
            public int Height { get; }

            public CaptureDimensions(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }
    }
}
