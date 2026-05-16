using System.Diagnostics;

namespace datinate.app
{
    internal sealed class Media2AssignHoverManager : IDisposable
    {
        private readonly Control root;
        private readonly System.Windows.Forms.Timer pollTimer;

        private Media2AssignUI? current;
        private Media2AssignUI? candidate;

        private long candidateSinceTicks;
        private long lastOverAnyTileTicks;

        private bool disposed;

        public int PollIntervalMs { get; set; } = 16;
        public int EnterDelayMs { get; set; } = 45;
        public int LeaveDelayMs { get; set; } = 200;

        public Media2AssignHoverManager(Control root)
        {
            this.root = root;

            pollTimer = new System.Windows.Forms.Timer();
            pollTimer.Interval = PollIntervalMs;
            pollTimer.Tick += PollTimer_Tick;
            pollTimer.Start();

            root.HandleDestroyed += Root_HandleDestroyed;
            root.Disposed += Root_Disposed;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            pollTimer.Stop();
            pollTimer.Tick -= PollTimer_Tick;
            pollTimer.Dispose();

            root.HandleDestroyed -= Root_HandleDestroyed;
            root.Disposed -= Root_Disposed;

            ClearCurrent();
        }

        private void Root_HandleDestroyed(object? sender, EventArgs e) => ClearCurrent();

        private void Root_Disposed(object? sender, EventArgs e) => Dispose();

        private void PollTimer_Tick(object? sender, EventArgs e)
        {
            if (disposed || root.IsDisposed || !root.IsHandleCreated)
                return;

            if (pollTimer.Interval != PollIntervalMs)
                pollTimer.Interval = PollIntervalMs;

            UpdateHover();
        }

        private void UpdateHover()
        {
            var now = Stopwatch.GetTimestamp();

            var hit = HitTestTile();

            if (hit != null)
                lastOverAnyTileTicks = now;

            if (hit == current)
            {
                candidate = null;
                candidateSinceTicks = 0;
                return;
            }

            if (hit == null)
            {
                if (current != null && (now - lastOverAnyTileTicks) >= MsToTicks(LeaveDelayMs))
                    ClearCurrent();

                candidate = null;
                candidateSinceTicks = 0;
                return;
            }

            if (current != null)
            {
                if (!current.IsDisposed)
                    current.SetHoverState(false);

                current = null;
            }

            if (candidate != hit)
            {
                candidate = hit;
                candidateSinceTicks = now;
                return;
            }

            if ((now - candidateSinceTicks) >= MsToTicks(EnterDelayMs))
            {
                SetCurrent(hit);
                candidate = null;
                candidateSinceTicks = 0;
            }
        }

        private Media2AssignUI? HitTestTile()
        {
            if (!root.Visible)
                return null;

            var sp = Control.MousePosition;

            Rectangle rootRect;
            try { rootRect = root.RectangleToScreen(root.ClientRectangle); }
            catch (Exception) { return null; }

            if (!rootRect.Contains(sp))
                return null;

            for (int i = root.Controls.Count - 1; i >= 0; i--)
            {
                if (root.Controls[i] is not Media2AssignUI ui)
                    continue;

                if (!ui.Visible || ui.IsDisposed)
                    continue;

                Rectangle r;
                try { r = ui.RectangleToScreen(ui.ClientRectangle); }
                catch (Exception) { continue; }

                if (r.Contains(sp))
                    return ui;
            }

            return null;
        }

        private void SetCurrent(Media2AssignUI next)
        {
            if (current == next)
                return;

            current = next;

            if (!current.IsDisposed)
                current.SetHoverState(true);
        }

        private void ClearCurrent()
        {
            if (current != null && !current.IsDisposed)
                current.SetHoverState(false);

            current = null;
            candidate = null;
            candidateSinceTicks = 0;
            lastOverAnyTileTicks = 0;
        }

        private static long MsToTicks(int ms)
        {
            return (long)((double)ms * Stopwatch.Frequency / 1000.0);
        }
    }
}
