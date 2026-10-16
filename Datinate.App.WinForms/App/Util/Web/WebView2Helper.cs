using Datinate.App.Shared.Web;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Datinate.App.WinForms.View.Util
{
    public static class WebView2Helper
    {
        private static readonly object SharedEnvLock = new();
        private static Task<CoreWebView2Environment>? sharedEnvTask;

        // =========================================================
        // WebView2 environment / creation
        // =========================================================

        public static Task<CoreWebView2Environment> GetSharedEnvironmentAsync()
        {
            lock (SharedEnvLock)
            {
                if (sharedEnvTask != null)
                    return sharedEnvTask;

                Directory.CreateDirectory(MediaWebHelper.MediaWebUserDataFolder);

                var options = new CoreWebView2EnvironmentOptions();

                if (DatinatePerformanceUtil.WV2_DoNotUseGpu)
                {
#pragma warning disable CS0162 // Unreachable code detected
                    options.AdditionalBrowserArguments =
                        "--disable-gpu --disable-gpu-compositing";
#pragma warning restore CS0162 // Unreachable code detected
                }

                sharedEnvTask = CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null,
                    userDataFolder: MediaWebHelper.MediaWebUserDataFolder,
                    options: options);

                return sharedEnvTask;
            }
        }

        public static void ApplyCreationProperties(WebView2 webView)
        {
            Directory.CreateDirectory(MediaWebHelper.MediaWebUserDataFolder);

            var creationProperties =
                webView.CreationProperties ??
                new CoreWebView2CreationProperties();

            creationProperties.UserDataFolder = MediaWebHelper.MediaWebUserDataFolder;

            if (DatinatePerformanceUtil.WV2_DoNotUseGpu)
            {
#pragma warning disable CS0162 // Unreachable code detected
                string arguments =
                    creationProperties.AdditionalBrowserArguments ??
                    string.Empty;

                if (!arguments.Contains(
                    "--disable-gpu",
                    StringComparison.OrdinalIgnoreCase))
                {
                    arguments =
                        (arguments +
                         " --disable-gpu --disable-gpu-compositing")
                        .Trim();
                }

                creationProperties.AdditionalBrowserArguments = arguments;
#pragma warning restore CS0162 // Unreachable code detected
            }

            webView.CreationProperties = creationProperties;
        }

        public static async Task<CoreWebView2> EnsureCoreAsync(WebView2 webView)
        {
            if (webView.CoreWebView2 != null)
                return webView.CoreWebView2;

            CoreWebView2Environment environment = await GetSharedEnvironmentAsync();

            await webView.EnsureCoreWebView2Async(environment);

            if (webView.CoreWebView2 != null)
                webView.CoreWebView2.Settings.AreDevToolsEnabled = false;

            return webView.CoreWebView2!;
        }


        // =========================================================
        // URI handling
        // =========================================================
        public static Uri? TryGetSource(WebView2 webView)
        {
            if (webView == null) return null;

            try
            {
                return webView.Source;
            }
            catch
            {
                return null;
            }
        }

        // =========================================================
        // Media playback
        // =========================================================

        /// <summary>
        /// Applies Datinate's reliable looping behaviour to the first
        /// HTML video element found in the current document.
        /// </summary>
        public static Task EnableVideoLoopAsync(CoreWebView2 core)
        {
            if (core == null)
                throw new ArgumentNullException(nameof(core));

            return core.ExecuteScriptAsync(MediaWebHelper.EnableVideoLoopJavascript);
        }

        // =========================================================
        // Download handling
        // =========================================================

        /// <summary>
        /// Embedded Datinate WebViews must never download content.
        /// </summary>
        public static void CancelDownload(CoreWebView2DownloadStartingEventArgs e)
        {
            try
            {
                // Hide WebView2's normal download UI and cancel underlying download itself.
                e.Handled = true;
                e.Cancel = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        // =========================================================
        // Basic navigation helpers
        // =========================================================

        public static void Stop(CoreWebView2? core)
        {
            if (core == null) return;

            try
            {
                core.Stop();
            }
            catch { }
        }

        public static void NavigateToAboutBlank(CoreWebView2? core)
        {
            if (core == null) return;

            try
            {
                core.Navigate(MediaWebHelper.AboutBlankUri);
            }
            catch { }
        }

        public static void NavigateToBlankPage(CoreWebView2? core)
        {
            if (core == null) return;

            try
            {
                core.NavigateToString(MediaWebHelper.LoadingHtmlBlack);
            }
            catch { }
        }
    }
}