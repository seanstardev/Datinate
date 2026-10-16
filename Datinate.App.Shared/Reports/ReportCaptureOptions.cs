using System;

namespace Datinate.App.Shared.Reports
{
    // Native client-pixel limits and the DPI of the tooltip's originating view.
    public sealed class ReportCaptureOptions
    {
        public int MaximumWidth { get; }
        public int MaximumHeight { get; }
        public int TargetDpi { get; }

        public ReportCaptureOptions(int maximumWidth, int maximumHeight, int targetDpi)
        {
            if (maximumWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumWidth));

            if (maximumHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumHeight));

            if (targetDpi <= 0)
                throw new ArgumentOutOfRangeException(nameof(targetDpi));

            MaximumWidth = maximumWidth;
            MaximumHeight = maximumHeight;
            TargetDpi = targetDpi;
        }
    }
}
