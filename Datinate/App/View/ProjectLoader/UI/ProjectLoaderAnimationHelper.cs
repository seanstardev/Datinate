using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace datinate.app
{
    public static class ProjectLoaderAnimationHelper
    {

        public static readonly Dictionary<FlowLayoutPanel, System.Windows.Forms.Timer> reorderTimers = new();
        private static readonly Dictionary<FlowLayoutPanel, bool> reorderLayoutLocked = new();
        private static readonly Dictionary<FlowLayoutPanel, Action> reorderCleanupByPanel = new();
        private static void CancelFlowAnimation(FlowLayoutPanel p)
        {
            if (reorderTimers.TryGetValue(p, out var t))
            {
                t.Stop();
                t.Dispose();
                reorderTimers.Remove(p);
            }

            if (reorderCleanupByPanel.TryGetValue(p, out var cleanup))
            {
                reorderCleanupByPanel.Remove(p);
                cleanup();
            }
        }

        private static void SwapChildIndices(FlowLayoutPanel p, Control a, Control b)
        {
            var ia = p.Controls.GetChildIndex(a);
            var ib = p.Controls.GetChildIndex(b);
            if (ia == ib) return;

            p.SuspendLayout();
            try
            {
                p.Controls.SetChildIndex(b, ia);
                p.Controls.SetChildIndex(a, ib);
            }
            finally
            {
                p.ResumeLayout(false);
            }
        }

        private static Rectangle Lerp(Rectangle a, Rectangle b, double t)
        {
            int L(int x0, int x1) => x0 + (int)Math.Round((x1 - x0) * t);
            return new Rectangle(
                L(a.X, b.X),
                L(a.Y, b.Y),
                L(a.Width, b.Width),
                L(a.Height, b.Height)
            );
        }

        // A top-level overlay window: doesn't invalidate whole form each tick.
        private sealed class PanelSwapOverlayForm : Form
        {
            private const int WM_NCHITTEST = 0x0084;
            private const int HTTRANSPARENT = -1;

            private readonly Bitmap bmpA;
            private readonly Bitmap bmpB;

            private Rectangle rectA;
            private Rectangle rectB;

            public PanelSwapOverlayForm(Rectangle screenBounds, Bitmap bmpA, Bitmap bmpB)
            {
                this.bmpA = bmpA;
                this.bmpB = bmpB;

                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                Bounds = screenBounds;

                BackColor = Color.Magenta;
                TransparencyKey = Color.Magenta;

                // Reduce flicker in the overlay itself.
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
                UpdateStyles();
            }

            public void SetRects(Rectangle a, Rectangle b)
            {
                rectA = a;
                rectB = b;
            }

            public void ShowNoActivate()
            {
                // Show without stealing focus.
                // Using Show() is fine because we also prevent activation via CreateParams.
                Show();
            }

            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    const int WS_EX_TOOLWINDOW = 0x00000080;
                    const int WS_EX_NOACTIVATE = 0x08000000;

                    var cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                    return cp;
                }
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = (IntPtr)HTTRANSPARENT; // click-through
                    return;
                }
                base.WndProc(ref m);
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // Fill with transparency-key color to avoid any junk during repaints.
                using var b = new SolidBrush(BackColor);
                e.Graphics.FillRectangle(b, ClientRectangle);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                // Bitmap blits only.
                e.Graphics.DrawImage(bmpA, rectA);
                e.Graphics.DrawImage(bmpB, rectB);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    try { bmpA.Dispose(); } catch { }
                    try { bmpB.Dispose(); } catch { }
                }
                base.Dispose(disposing);
            }
        }

        public static void AnimateFlowSwap(
            FlowLayoutPanel p,
            ProjectDatUI a,
            ProjectDatUI b,
            int durationMs = 200,
            Action? onDone = null)
        {
            if (a.IsDisposed || b.IsDisposed || a == b)
            {
                onDone?.Invoke();
                return;
            }

            CancelFlowAnimation(p);

            // Ensure start bounds are current
            p.PerformLayout();

            var aStart = a.Bounds;
            var bStart = b.Bounds;

            if (aStart.Width <= 0 || aStart.Height <= 0 || bStart.Width <= 0 || bStart.Height <= 0)
            {
                SwapChildIndices(p, a, b);
                p.PerformLayout();
                a.EndDragPlaceholderMode();
                b.EndDragPlaceholderMode();
                onDone?.Invoke();
                return;
            }

            Bitmap? bmpA = null;
            Bitmap? bmpB = null;
            Bitmap? bmpPanel = null;
            PanelSwapOverlayForm? overlay = null;

            try
            {
                bmpA = CaptureBitmap(a);
                bmpB = CaptureBitmap(b);

                bmpPanel = CaptureBitmap(p, p.ClientSize);

                using (var g = Graphics.FromImage(bmpPanel))
                using (var br = new SolidBrush(p.BackColor))
                {
                    g.FillRectangle(br, aStart);
                    g.FillRectangle(br, bStart);
                }

                Rectangle aEnd;
                Rectangle bEnd;

                SetRedraw(p, false);
                try
                {
                    // compute end bounds
                    SwapChildIndices(p, a, b);
                    p.PerformLayout();
                    aEnd = a.Bounds;
                    bEnd = b.Bounds;

                    // restore original order for the duration of the animation
                    SwapChildIndices(p, a, b);
                    p.PerformLayout();
                }
                finally
                {
                    // keep redraw off during animation
                }

                if (aStart == aEnd && bStart == bEnd)
                {
                    SwapChildIndices(p, a, b);
                    p.PerformLayout();

                    SetRedraw(p, true);
                    p.Invalidate(true);
                    p.Update();

                    a.EndDragPlaceholderMode();
                    b.EndDragPlaceholderMode();

                    onDone?.Invoke();
                    return;
                }

                // Auto-duration by distance (still respects caller if they pass something bigger)
                var dist =
                    Math.Abs(aEnd.X - aStart.X) +
                    Math.Abs(aEnd.Y - aStart.Y);

                var auto = Math.Clamp(220 + (dist / 3), 240, 420);
                var ms = Math.Max(durationMs, auto);

                var screenBounds = p.RectangleToScreen(p.ClientRectangle);

                overlay = new PanelSwapOverlayForm(screenBounds, bmpA, bmpB);

                // overlay owns these from here on
                bmpPanel = null;
                bmpA = null;
                bmpB = null;

                // IMPORTANT: ensure it stays above your form and swallows mouse (see WndProc fix)
                overlay.Owner = p.FindForm();

                overlay.SetRects(aStart, bStart);
                overlay.ShowNoActivate();
                overlay.Update();

                reorderCleanupByPanel[p] = () =>
                {
                    try
                    {
                        SetRedraw(p, true);
                        p.Invalidate(true);
                        p.Update();
                    }
                    catch { }

                    // Always restore real UI visuals
                    try { a.EndDragPlaceholderMode(); } catch { }
                    try { b.EndDragPlaceholderMode(); } catch { }

                    try
                    {
                        if (overlay != null && !overlay.IsDisposed)
                        {
                            overlay.Close();
                            overlay.Dispose();
                        }
                    }
                    catch { }
                };

                var sw = Stopwatch.StartNew();
                var timer = new System.Windows.Forms.Timer { Interval = 10 }; // smoother than 15ms

                timer.Tick += (_, __) =>
                {
                    var t = sw.Elapsed.TotalMilliseconds / Math.Max(1, ms);
                    if (t >= 1.0)
                    {
                        timer.Stop();
                        timer.Dispose();
                        reorderTimers.Remove(p);

                        try
                        {
                            SwapChildIndices(p, a, b);
                            p.PerformLayout();
                        }
                        catch { }

                        CancelFlowAnimation(p); // re-enable redraw + close overlay + restore placeholder mode

                        onDone?.Invoke();
                        return;
                    }

                    // “real” movement: ease-in-out then a soft back-overshoot settle
                    var eased = EaseInOutCubic(t);
                    var u = EaseOutBack(eased, overshoot: 1.20);

                    overlay!.SetRects(
                        Lerp(aStart, aEnd, u),
                        Lerp(bStart, bEnd, u));

                    overlay.Invalidate();
                };

                reorderTimers[p] = timer;
                timer.Start();
            }
            catch
            {
                try { bmpA?.Dispose(); } catch { }
                try { bmpB?.Dispose(); } catch { }
                try { bmpPanel?.Dispose(); } catch { }

                try { SetRedraw(p, true); } catch { }
                try
                {
                    SwapChildIndices(p, a, b);
                    p.PerformLayout();
                    p.Invalidate(true);
                    p.Update();
                }
                catch { }

                try { a.EndDragPlaceholderMode(); } catch { }
                try { b.EndDragPlaceholderMode(); } catch { }

                onDone?.Invoke();
            }
        }

        private static double EaseInOutCubic(double t)
        {
            if (t < 0.5)
                return 4.0 * t * t * t;

            var x = (2.0 * t) - 2.0;
            return 0.5 * x * x * x + 1.0;
        }

        private static double EaseOutBack(double t, double overshoot = 1.20)
        {
            // Classic "back" easing: returns >1 near the end (overshoot), settles at 1 at t=1.
            var x = t - 1.0;
            return 1.0 + (x * x * ((overshoot + 1.0) * x + overshoot));
        }

        private static Bitmap CaptureBitmap(Control c)
        {
            var bmp = new Bitmap(Math.Max(1, c.Width), Math.Max(1, c.Height));
            c.DrawToBitmap(bmp, new Rectangle(Point.Empty, c.Size));
            return bmp;
        }
        private static Bitmap CaptureBitmap(Control c, Size size)
        {
            var bmp = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height));
            c.DrawToBitmap(bmp, new Rectangle(Point.Empty, size));
            return bmp;
        }


        private const int WM_SETREDRAW = 0x000B;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private static void SetRedraw(Control c, bool enabled)
        {
            if (!c.IsHandleCreated) return;

            SendMessage(c.Handle, WM_SETREDRAW, enabled ? (IntPtr)1 : IntPtr.Zero, IntPtr.Zero);

            // IMPORTANT: do NOT Invalidate(true) here (that repaints all children)
            if (enabled)
                c.Invalidate(); // just the panel; Flow will repaint what it needs
        }

        private static Bitmap CropFrom(Bitmap src, Rectangle r)
        {
            var srcRect = new Rectangle(Point.Empty, src.Size);
            var rr = Rectangle.Intersect(srcRect, r);

            // Keep output size identical to requested rectangle (even if partially clipped).
            var outBmp = new Bitmap(Math.Max(1, r.Width), Math.Max(1, r.Height), PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(outBmp))
            {
                g.Clear(Color.Transparent);

                if (rr.Width > 0 && rr.Height > 0)
                {
                    using var cropped = src.Clone(rr, PixelFormat.Format32bppArgb);
                    g.DrawImageUnscaled(cropped, rr.X - r.X, rr.Y - r.Y);
                }
            }

            return outBmp;
        }

        public static void AnimateFlowMoveToIndex(
            FlowLayoutPanel p,
            ProjectDatUI ui,
            int targetIndex,
            int durationMs = 280,
            Action? onDone = null)
        {
            if (ui.IsDisposed || p.IsDisposed) { onDone?.Invoke(); return; }

            CancelFlowAnimation(p);

            var startIndex = p.Controls.GetChildIndex(ui);
            var last = p.Controls.Count - 1;

            if (last <= 0) { onDone?.Invoke(); return; }
            if (targetIndex < 0) targetIndex = 0;
            if (targetIndex > last) targetIndex = last;
            if (targetIndex == startIndex) { onDone?.Invoke(); return; }

            p.PerformLayout();

            var uiStart = ui.Bounds;
            if (uiStart.Width <= 0 || uiStart.Height <= 0) { onDone?.Invoke(); return; }

            var affected = new List<ProjectDatUI>();
            if (targetIndex > startIndex)
            {
                for (int i = startIndex + 1; i <= targetIndex; i++)
                    if (p.Controls[i] is ProjectDatUI x) affected.Add(x);
            }
            else
            {
                for (int i = targetIndex; i <= startIndex - 1; i++)
                    if (p.Controls[i] is ProjectDatUI x) affected.Add(x);
            }

            var spriteCount = 1 + affected.Count;

            var startRects = new Rectangle[spriteCount];
            var endRects = new Rectangle[spriteCount];

            startRects[0] = uiStart;
            for (int i = 0; i < affected.Count; i++)
                startRects[i + 1] = affected[i].Bounds;

            Bitmap? bmpStart = null;
            Bitmap? bmpBase = null;
            Bitmap[]? bmps = null;
            Panel? placeholder = null;
            PanelMoveOverlayForm? overlay = null;

            try
            {
                bmpStart = CaptureBitmap(p, p.ClientSize);

                placeholder = new Panel
                {
                    Size = ui.Size,
                    MinimumSize = ui.Size,
                    MaximumSize = ui.Size,
                    Margin = ui.Margin,
                    BackColor = p.BackColor,
                    Enabled = false,
                    TabStop = false
                };

                SetRedraw(p, false);

                p.SuspendLayout();
                try
                {
                    p.Controls.Remove(ui);
                    p.Controls.Add(placeholder);
                    p.Controls.SetChildIndex(placeholder, startIndex);
                }
                finally
                {
                    p.ResumeLayout(false);
                }

                p.PerformLayout();

                p.Controls.SetChildIndex(placeholder, targetIndex);
                p.PerformLayout();

                endRects[0] = placeholder.Bounds;
                for (int i = 0; i < affected.Count; i++)
                    endRects[i + 1] = affected[i].Bounds;

                p.Controls.SetChildIndex(placeholder, startIndex);
                p.PerformLayout();

                bmpBase = (Bitmap)bmpStart.Clone();

                using (var g = Graphics.FromImage(bmpBase))
                using (var br = new SolidBrush(p.BackColor))
                {
                    for (int i = 0; i < startRects.Length; i++)
                        g.FillRectangle(br, startRects[i]);
                }

                bmps = new Bitmap[spriteCount];
                for (int i = 0; i < spriteCount; i++)
                    bmps[i] = CropFrom(bmpStart, startRects[i]);

                var liveRects = new Rectangle[spriteCount];
                Array.Copy(startRects, liveRects, spriteCount);

                var screenBounds = p.RectangleToScreen(p.ClientRectangle);
                overlay = new PanelMoveOverlayForm(screenBounds, bmpBase, bmps, liveRects);
                bmpBase = null;
                bmps = null;

                overlay.Owner = p.FindForm();
                overlay.ShowNoActivate();
                overlay.Update();

                reorderCleanupByPanel[p] = () =>
                {
                    try { SetRedraw(p, true); p.Invalidate(true); p.Update(); } catch { }

                    try
                    {
                        if (overlay != null && !overlay.IsDisposed)
                        {
                            overlay.Close();
                            overlay.Dispose();
                        }
                    }
                    catch { }
                };

                var dist = Math.Abs(endRects[0].X - startRects[0].X) + Math.Abs(endRects[0].Y - startRects[0].Y);
                var ms = Math.Clamp(Math.Max(durationMs, 200 + dist / 6), 220, 420);

                var sw = Stopwatch.StartNew();
                var timer = new System.Windows.Forms.Timer { Interval = 10 };

                timer.Tick += (_, __) =>
                {
                    var t = sw.Elapsed.TotalMilliseconds / Math.Max(1, ms);
                    if (t >= 1.0)
                    {
                        timer.Stop();
                        timer.Dispose();
                        reorderTimers.Remove(p);

                        try
                        {
                            p.SuspendLayout();
                            try
                            {
                                if (placeholder != null && placeholder.Parent == p)
                                    p.Controls.Remove(placeholder);

                                p.Controls.Add(ui);
                                p.Controls.SetChildIndex(ui, Math.Min(targetIndex, p.Controls.Count - 1));
                            }
                            finally
                            {
                                p.ResumeLayout(false);
                            }

                            p.PerformLayout();
                        }
                        catch { }

                        CancelFlowAnimation(p);

                        try { p.ScrollControlIntoView(ui); } catch { }

                        onDone?.Invoke();
                        return;
                    }

                    var eased = EaseInOutCubic(t);
                    var u = EaseOutBack(eased, overshoot: 1.12);

                    for (int i = 0; i < liveRects.Length; i++)
                        liveRects[i] = Lerp(startRects[i], endRects[i], u);

                    overlay!.Invalidate();
                };

                reorderTimers[p] = timer;
                timer.Start();
            }
            catch
            {
                try { bmpStart?.Dispose(); } catch { }
                try { bmpBase?.Dispose(); } catch { }
                if (bmps != null)
                {
                    for (int i = 0; i < bmps.Length; i++)
                        try { bmps[i].Dispose(); } catch { }
                }

                try
                {
                    SetRedraw(p, true);

                    if (placeholder != null && placeholder.Parent == p)
                        p.Controls.Remove(placeholder);

                    if (!ui.IsDisposed && ui.Parent != p)
                    {
                        p.Controls.Add(ui);
                        p.Controls.SetChildIndex(ui, Math.Min(startIndex, p.Controls.Count - 1));
                    }

                    p.PerformLayout();
                    p.Invalidate(true);
                    p.Update();
                }
                catch { }

                onDone?.Invoke();
            }
        }
    }
}
