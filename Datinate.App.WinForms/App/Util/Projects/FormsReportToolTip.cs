using Datinate.App.Shared.Reports;

namespace Datinate.App.WinForms.View.Util
{
    internal sealed class FormsReportToolTip : ReportToolTipSessionBase<Bitmap>
    {
        private const int ScreenMarginPx = 4;
        private const int PointerOffsetX = 16;
        private const int PointerOffsetY = 20;

        private readonly Control owner;
        private readonly ToolTip toolTip;
        private readonly System.Windows.Forms.Timer mouseExitTimer;

        private Control? activeOrigin;
        private Form? originForm;
        private Point screenPoint;
        private Rectangle screenBounds;
        private bool pinned;
        private Control? busyOrigin;
        private Cursor? previousCursor;

        protected override bool CanPresentReport
            => activeOrigin != null && !activeOrigin.IsDisposed && activeOrigin.Visible && owner.Visible;

        protected override IHtmlReportCapture<Bitmap> CreateCapture()
            => new FormsHtmlReportCapture();

        internal FormsReportToolTip(Control owner)
        {
            this.owner = owner;

            toolTip = new ToolTip
            {
                OwnerDraw = true,
                UseFading = false,
                UseAnimation = false,
                AutoPopDelay = int.MaxValue,
                InitialDelay = 0,
                ReshowDelay = 0
            };

            toolTip.Popup += OnPopup;
            toolTip.Draw += OnDraw;

            mouseExitTimer = new System.Windows.Forms.Timer { Interval = 30 };
            mouseExitTimer.Tick += OnMouseExitTimerTick;

            owner.VisibleChanged += OnOwnerVisibleChanged;
            owner.Disposed += OnOwnerDisposed;
        }

        internal void Show(Control origin, string reportHtml, bool pinned = true, Point? screenPoint = null)
        {
            if (base.IsDisposed || origin.IsDisposed || !origin.Visible || !owner.Visible)
                return;

            base.Hide();

            activeOrigin = origin;
            this.pinned = pinned;
            this.screenPoint = screenPoint ?? Control.MousePosition;
            origin.MouseLeave += OnOriginMouseLeave;
            origin.Disposed += OnOriginDisposed;
            origin.VisibleChanged += OnOriginVisibleChanged;

            originForm = origin.FindForm();

            if (originForm != null)
                originForm.Deactivate += OnFormDeactivate;

            Rectangle workingArea = Screen.FromPoint(this.screenPoint).WorkingArea;

            base.QueueReport(reportHtml, new ReportCaptureOptions(
                Math.Max(1, workingArea.Width - ScreenMarginPx * 2),
                Math.Max(1, workingArea.Height - ScreenMarginPx * 2),
                origin.DeviceDpi));
        }

        protected override void ShowImage(Bitmap image)
        {
            Rectangle workingArea = Screen.FromPoint(screenPoint).WorkingArea;
            int x = Math.Clamp(
                screenPoint.X + PointerOffsetX,
                workingArea.Left,
                Math.Max(workingArea.Left, workingArea.Right - image.Width - ScreenMarginPx));
            int y = Math.Clamp(
                screenPoint.Y + PointerOffsetY,
                workingArea.Top,
                Math.Max(workingArea.Top, workingArea.Bottom - image.Height - ScreenMarginPx));

            screenBounds = new Rectangle(new Point(x, y), image.Size);
            Point local = activeOrigin!.PointToClient(screenBounds.Location);
            toolTip.Show(" ", activeOrigin, local.X, local.Y);

            if (pinned && screenBounds.Contains(Cursor.Position))
                mouseExitTimer.Start();
        }

        protected override void HideImage()
        {
            mouseExitTimer.Stop();

            if (activeOrigin != null)
            {
                if (!activeOrigin.IsDisposed)
                    toolTip.Hide(activeOrigin);

                activeOrigin.MouseLeave -= OnOriginMouseLeave;
                activeOrigin.Disposed -= OnOriginDisposed;
                activeOrigin.VisibleChanged -= OnOriginVisibleChanged;
                activeOrigin = null;
            }

            if (originForm != null)
            {
                originForm.Deactivate -= OnFormDeactivate;
                originForm = null;
            }

            pinned = false;
            screenBounds = Rectangle.Empty;
        }

        protected override void SetBusy(bool busy)
        {
            if (busy)
            {
                busyOrigin = activeOrigin!;
                previousCursor = busyOrigin.Cursor;
                busyOrigin.Cursor = Cursors.AppStarting;

                if (busyOrigin.ClientRectangle.Contains(busyOrigin.PointToClient(Control.MousePosition)))
                    Cursor.Current = Cursors.AppStarting;
            }
            else
            {
                if (busyOrigin != null && !busyOrigin.IsDisposed)
                {
                    busyOrigin.Cursor = previousCursor;

                    if (busyOrigin.ClientRectangle.Contains(busyOrigin.PointToClient(Control.MousePosition)))
                        Cursor.Current = busyOrigin.Cursor;
                }

                busyOrigin = null;
                previousCursor = null;
            }
        }

        private void OnPopup(object? sender, PopupEventArgs e)
        {
            if (base.CurrentImage == null)
                e.Cancel = true;
            else
                e.ToolTipSize = base.CurrentImage.Size;
        }

        private void OnDraw(object? sender, DrawToolTipEventArgs e)
        {
            if (base.CurrentImage is Bitmap image)
                e.Graphics.DrawImageUnscaled(image, Point.Empty);
        }

        private void OnOriginMouseLeave(object? sender, EventArgs e)
        {
            if (pinned && base.CurrentImage != null && screenBounds.Contains(Control.MousePosition))
                mouseExitTimer.Start();
            else
                base.Hide();
        }

        private void OnMouseExitTimerTick(object? sender, EventArgs e)
        {
            if (!screenBounds.Contains(Cursor.Position))
                base.Hide();
        }

        private void OnOriginVisibleChanged(object? sender, EventArgs e)
        {
            if (activeOrigin?.Visible == false)
                base.Hide();
        }

        private void OnOwnerVisibleChanged(object? sender, EventArgs e)
        {
            if (!owner.Visible)
                base.Hide();
        }

        private void OnOriginDisposed(object? sender, EventArgs e) => base.Hide();

        private void OnFormDeactivate(object? sender, EventArgs e) => base.Hide();

        private void OnOwnerDisposed(object? sender, EventArgs e) => base.Dispose();

        protected override void Disposing()
        {
            owner.VisibleChanged -= OnOwnerVisibleChanged;
            owner.Disposed -= OnOwnerDisposed;
            mouseExitTimer.Dispose();
            toolTip.Dispose();

            base.Disposing();
        }
    }
}
