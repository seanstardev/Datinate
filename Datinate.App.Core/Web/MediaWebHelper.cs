using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Datinate.App.Core.Web
{
    public static class MediaWebHelper
    {
        public const string AboutBlankUri = "about:blank";

        public static string MediaWebUserDataFolder
        {
            get
            {
                var applicationName = Assembly.GetEntryAssembly()?.GetName().Name
                    ?? throw new InvalidOperationException(
                        "The host application's name could not be determined.");

                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    applicationName,
                    "Web");
            }
        }

        private static readonly string TempHtmlFolder =
            Path.Combine(
                Path.GetTempPath(),
                "Datinate",
                "MediaWebView");

        public static async Task<Uri> CreateTempHtmlAsync(string html)
        {
            Directory.CreateDirectory(TempHtmlFolder);

            string path = Path.Combine(
                TempHtmlFolder,
                Guid.NewGuid().ToString("N") + ".html");

            using (var stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 4096,
                options: FileOptions.Asynchronous))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                await writer.WriteAsync(html ?? string.Empty).ConfigureAwait(false);

                await writer.FlushAsync().ConfigureAwait(false);
            }

            return new Uri(path);
        }

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

        public const string HideScrollbarsJavascript =
            "document.documentElement.style.overflow='hidden';" +
            "if(document.body) document.body.style.overflow='hidden';";

        public const string EnableVideoLoopJavascript =
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

        public static string CreateContentUnavailableHtml(string title, string body)
        {
            string safeTitle = WebUtility.HtmlEncode(title);
            string safeBody = WebUtility.HtmlEncode(body);

            return
$@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        html, body {{
            width: 100%;
            height: 100%;
            margin: 0;
            background: #000000;
            color: #ffffff;
            font-family: Arial, sans-serif;
        }}

        body {{
            display: flex;
            align-items: center;
            justify-content: center;
        }}

        .message {{
            text-align: center;
            padding: 24px;
            max-width: 600px;
        }}

        .title {{
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 10px;
        }}

        .body {{
            font-size: 14px;
            color: #d0d0d0;
        }}
    </style>
</head>
<body>
    <div class=""message"">
        <div class=""title"">{safeTitle}</div>
        <div class=""body"">{safeBody}</div>
    </div>
</body>
</html>";
        }

        public const string LoadingHtmlBlack =
@"<!DOCTYPE html>
<html lang=""en"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
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
</html>";
    }
}
