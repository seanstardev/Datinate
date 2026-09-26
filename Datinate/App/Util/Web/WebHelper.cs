using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Net;
using System.Text;

namespace datinate.app
{
    public static class WebHelper
    {
        private static readonly object SharedEnvLock = new();
        private static Task<CoreWebView2Environment>? sharedEnvTask;

        private static readonly string WebView2UserDataFolder =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Datinate",
                "WebView2");

        private static readonly string TempHtmlFolder =
            Path.Combine(
                Path.GetTempPath(),
                "Datinate",
                "MediaWebView");


        // =========================================================
        // WebView2 environment / creation
        // =========================================================

        public static Task<CoreWebView2Environment> GetSharedEnvironmentAsync()
        {
            lock (SharedEnvLock)
            {
                if (sharedEnvTask != null)
                    return sharedEnvTask;

                Directory.CreateDirectory(WebView2UserDataFolder);

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
                    userDataFolder: WebView2UserDataFolder,
                    options: options);

                return sharedEnvTask;
            }
        }

        public static void ApplyCreationProperties(WebView2 webView)
        {
            Directory.CreateDirectory(WebView2UserDataFolder);

            var creationProperties =
                webView.CreationProperties ??
                new CoreWebView2CreationProperties();

            creationProperties.UserDataFolder = WebView2UserDataFolder;

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

        /// <summary>
        /// Ensures the WebView2 Core exists using Datinate's shared
        /// WebView2 environment.
        ///
        /// This does not apply UI/browser policy such as muting,
        /// context menu behaviour, zoom, download handling, etc.
        /// </summary>
        public static async Task<CoreWebView2> EnsureCoreAsync(WebView2 webView)
        {
            if (webView.CoreWebView2 != null)
                return webView.CoreWebView2;

            CoreWebView2Environment environment = await GetSharedEnvironmentAsync();

            await webView.EnsureCoreWebView2Async(environment);

            return webView.CoreWebView2!;
        }


        // =========================================================
        // URI handling
        // =========================================================
        public static bool TryGetSupportedUri(
            string uri,
            out Uri parsed)
        {
            parsed = null!;

            if (string.IsNullOrWhiteSpace(uri)) return false;

            if (!Uri.TryCreate(uri, UriKind.Absolute, out parsed)) return false;

            return
                parsed.Scheme.Equals(
                    Uri.UriSchemeHttp,
                    StringComparison.OrdinalIgnoreCase)
                ||
                parsed.Scheme.Equals(
                    Uri.UriSchemeHttps,
                    StringComparison.OrdinalIgnoreCase)
                ||
                parsed.Scheme.Equals(
                    Uri.UriSchemeFile,
                    StringComparison.OrdinalIgnoreCase);
        }

        public static string GetExtensionNoQuery(Uri uri)
        {
            if (uri == null) return string.Empty;

            try
            {
                string path = uri.IsFile
                    ? uri.LocalPath
                    : uri.AbsolutePath;

                return Path.GetExtension(path) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

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

        public static bool IsAboutBlank(Uri? uri)
        {
            if (uri == null) return false;

            if (!uri.IsAbsoluteUri) return false;

            if (!uri.Scheme.Equals( "about", StringComparison.OrdinalIgnoreCase))
                return false;
            
            return string.Equals(
                uri.AbsoluteUri,
                "about:blank",
                StringComparison.OrdinalIgnoreCase);
        }


        // =========================================================
        // Media / extension classification
        // =========================================================

        public static bool IsVideoExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) return false;

            extension = NormaliseExtension(extension);

            return
                extension.Equals(
                    ".mp4",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".webm",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".m4v",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".mov",
                    StringComparison.OrdinalIgnoreCase);
        }
        public static bool IsMusicStandardExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            extension = NormaliseExtension(extension);

            return
                extension.Equals(
                    ".mp3",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".wav",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".flac",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".m4a",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".ogg",
                    StringComparison.OrdinalIgnoreCase);
        }
        public static bool IsMusicVgmExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            extension = NormaliseExtension(extension);

            return
                extension.Equals(
                    ".spc",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".nsf",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".nsfe",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".gbs",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".hes",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".ay",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".kss",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".vgm",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".vgz",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".gym",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".sap",
                    StringComparison.OrdinalIgnoreCase);
        }
        public static bool IsImageExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            extension = NormaliseExtension(extension);

            return
                extension.Equals(
                    ".png",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".jpg",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".jpeg",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".gif",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".bmp",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".webp",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".tif",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".tiff",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".ico",
                    StringComparison.OrdinalIgnoreCase);
        }

        public static bool LooksLikeHtmlOrDocument(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return true;

            extension = NormaliseExtension(extension);

            return
                extension.Equals(
                    ".htm",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".html",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".php",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase)
                ||
                extension.Equals(
                    ".aspx",
                    StringComparison.OrdinalIgnoreCase);
        }

        private static string NormaliseExtension(string extension)
        {
            extension = extension.Trim();

            if (!extension.StartsWith(
                ".",
                StringComparison.Ordinal))
            {
                extension = "." + extension;
            }

            return extension;
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

            const string javascript =
                "(function(){"
                + "let tries=0;"
                + "const id=setInterval(()=>{"
                + "  const v=document.querySelector('video');"
                + "  if(v){"
                + "    try{v.loop=true;}catch(e){}"
                + "    try{v.muted=true;}catch(e){}"
                + "    try{v.addEventListener('ended',()=>{try{v.currentTime=0;}catch(e){};v.play().catch(()=>{});});}catch(e){}"
                + "    try{v.play().catch(()=>{});}catch(e){}"
                + "    clearInterval(id);"
                + "  }"
                + "  if(++tries>60) clearInterval(id);"
                + "},100);"
                + "})();";

            return core.ExecuteScriptAsync(javascript);
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
        // Raw HTML temporary files
        // =========================================================

        public static Uri CreateTempHtml(string html)
        {
            Directory.CreateDirectory(TempHtmlFolder);

            string path =
                Path.Combine(
                    TempHtmlFolder,
                    Guid.NewGuid().ToString("N") + ".html");

            File.WriteAllText(
                path,
                html ?? string.Empty,
                new UTF8Encoding(false));

            return new Uri(path);
        }

        public static async Task<Uri> CreateTempHtmlAsync(string html)
        {
            Directory.CreateDirectory(TempHtmlFolder);

            string path = Path.Combine(
                TempHtmlFolder,
                Guid.NewGuid().ToString("N") + ".html");

            await File.WriteAllTextAsync(
                path,
                html ?? string.Empty,
                new UTF8Encoding(false));

            return new Uri(path);
        }

        public static void DeleteFile(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch { }
        }

        public static void DeleteFile(Uri? uri)
        {
            if (uri == null || !uri.IsFile) return;

            DeleteFile(uri.LocalPath);
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
                core.Navigate("about:blank");
            }
            catch { }
        }

        public static void NavigateToBlankPage(CoreWebView2? core)
        {
            if (core == null) return;

            try
            {
                core.NavigateToString(LoadingHtmlBlack);
            }
            catch
            { }
        }

        public const string LoadingHtmlBlack =
"""
<!DOCTYPE html>
<html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Black Page</title>
        <style>
            body {
                background-color: #000000; /* Sets background to black */
                margin: 0;                /* Removes default margins */
                height: 100vh;            /* Ensures full viewport height */
            }
        </style>
    </head>
<body>
</body>
</html>
""";

        public static string CreateContentUnavailableHtml(string title, string body)
        {
            string safeTitle = WebUtility.HtmlEncode(title);
            string safeBody = WebUtility.HtmlEncode(body);

            return
$$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <style>
        html, body {
            width: 100%;
            height: 100%;
            margin: 0;
            background: #000000;
            color: #ffffff;
            font-family: Arial, sans-serif;
        }

        body {
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .message {
            text-align: center;
            padding: 24px;
            max-width: 600px;
        }

        .title {
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 10px;
        }

        .body {
            font-size: 14px;
            color: #d0d0d0;
        }
    </style>
</head>
<body>
    <div class="message">
        <div class="title">{{safeTitle}}</div>
        <div class="body">{{safeBody}}</div>
    </div>
</body>
</html>
""";
        }
    }
}