namespace Datinate.App.Shared.Reports
{
    // Display dimensions account for the native renderer's DPI scaling.
    public sealed class ReportImage
    {
        public string PngBase64 { get; }

        public int DisplayWidth { get; }

        public int DisplayHeight { get; }

        public ReportImage(
            string pngBase64,
            int displayWidth,
            int displayHeight)
        {
            PngBase64 = pngBase64;

            DisplayWidth = displayWidth;
            DisplayHeight = displayHeight;
        }
    }
}
