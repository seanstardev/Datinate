using System.Diagnostics;

namespace Datinate.App.WinForms.View.Util
{
    public static class BrowserUtil
    {
        public static void LoadInBrowser(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri,
                    UseShellExecute = true
                });
            }
            catch (Exception) { }
        }
    }
}
