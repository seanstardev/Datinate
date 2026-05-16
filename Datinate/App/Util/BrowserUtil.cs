using System.Diagnostics;

namespace datinate.app
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
