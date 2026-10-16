using System;

namespace Datinate.App.Shared.Web
{
    public static class MediaExtensionHelper
    {
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
    }
}
