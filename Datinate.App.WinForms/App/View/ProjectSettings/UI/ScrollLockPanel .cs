using Timer = System.Windows.Forms.Timer;

namespace datinate.app.ui
{
    public sealed class ScrollLockPanel : Panel
    {
        private const int WM_HSCROLL = 0x0114;
        private const int WM_VSCROLL = 0x0115;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;
        private const int WM_GESTURE = 0x0119;
        private const int WM_GESTURENOTIFY = 0x011A;
        private const int WM_POINTERWHEEL = 0x024B;
        private const int WM_POINTERHWHEEL = 0x024C;

        private int lockDepth;
        private bool pendingUnlock;
        private bool restoring;
        private Point lockedScroll;

        private readonly Timer unlockTimer;

        public ScrollLockPanel()
        {
            unlockTimer = new Timer();
            unlockTimer.Tick += (_, __) => UnlockNow();
        }

        public void AcquireScrollLock()
        {
            unlockTimer.Stop();

            if (lockDepth == 0 && !pendingUnlock)
                lockedScroll = GetScrollOffset();

            pendingUnlock = false;
            lockDepth++;

            RestoreNow();
        }

        public void ReleaseScrollLock(int holdMs)
        {
            if (lockDepth <= 0)
                return;

            lockDepth--;

            if (lockDepth != 0)
                return;

            pendingUnlock = true;

            unlockTimer.Stop();
            unlockTimer.Interval = Math.Max(1, holdMs);
            unlockTimer.Start();
        }

        private void UnlockNow()
        {
            unlockTimer.Stop();
            pendingUnlock = false;
        }

        private bool IsLocked => lockDepth > 0 || pendingUnlock;

        private Point GetScrollOffset()
        {
            var p = AutoScrollPosition;
            return new Point(-p.X, -p.Y);
        }

        private void RestoreNow()
        {
            if (!IsLocked)
                return;

            if (restoring)
                return;

            restoring = true;
            try
            {
                if (AutoScroll)
                    AutoScrollPosition = lockedScroll;
            }
            finally
            {
                restoring = false;
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            if (IsLocked)
                RestoreNow();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            if (!IsLocked)
                return;

            if (!IsHandleCreated || IsDisposed)
                return;

            try { BeginInvoke(new Action(RestoreNow)); } catch { }
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            if (IsLocked)
                return DisplayRectangle.Location;

            return base.ScrollToControl(activeControl);
        }

        protected override void WndProc(ref Message m)
        {
            if (IsLocked)
            {
                int msg = m.Msg;

                if (msg == WM_MOUSEWHEEL ||
                    msg == WM_MOUSEHWHEEL ||
                    msg == WM_POINTERWHEEL ||
                    msg == WM_POINTERHWHEEL ||
                    msg == WM_VSCROLL ||
                    msg == WM_HSCROLL ||
                    msg == WM_GESTURE ||
                    msg == WM_GESTURENOTIFY)
                {
                    m.Result = IntPtr.Zero;
                    return;
                }
            }

            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { unlockTimer.Stop(); } catch { }
                try { unlockTimer.Dispose(); } catch { }
            }

            base.Dispose(disposing);
        }
    }
}