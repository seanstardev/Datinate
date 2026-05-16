using System.Reflection;
using Timer = System.Windows.Forms.Timer;
using System.Runtime.InteropServices;

namespace datinate.app
{
    public static class ScrollbarManager2
    {
        private static readonly object gate = new();

        private static readonly Dictionary<Control, VerticalState> bySurface = new();
        private static readonly Dictionary<Control, VerticalState> byViewport = new();

        private static WheelMessageFilter? wheelFilter;
        private static int wheelFilterUsers;
        private static LowLevelWheelHook? llWheelHook;
        private static int llWheelHookUsers;

        public static void AttachVertical(Control surface, VerticalConfig? config = null)
        {
            var cfg = config ?? VerticalConfig.Default;

            DetachVertical(surface);

            var st = new VerticalState(surface, cfg);

            EnsureViewport(st);
            TryEnableDoubleBuffering(st.Viewport);

            lock (gate)
            {
                bySurface[surface] = st;
                byViewport[st.Viewport] = st;
            }

            st.VWheel = (_, e) => OnMouseWheel(st, e);
            st.VDown = (_, e) => OnMouseDown(st, e);
            st.VMove = (_, e) => OnMouseMove(st, e);
            st.VUp = (_, e) => OnMouseUp(st, e);
            st.VPaint = (_, e) => OnPaint(st, e);
            st.VLayout = (_, __) => OnLayout(st);
            st.VResize = (_, __) => OnResize(st);
            st.VHandleDestroyed = (_, __) => DetachVertical(surface);
            st.VDisposed = (_, __) => DetachVertical(surface);
            st.VMouseCaptureChanged = (_, __) => OnMouseCaptureChanged(st);
            st.VHandleCreated = (_, __) => RequestRelayout(st);

            st.SLayout = (_, __) => RequestRelayout(st);
            st.SControlAdded = (_, __) => RequestRelayout(st);
            st.SControlRemoved = (_, __) => RequestRelayout(st);
            st.SVisibleChanged = (_, __) => RequestRelayout(st);

            st.SHandleDestroyed = (_, __) => DetachVertical(surface);
            st.SDisposed = (_, __) => DetachVertical(surface);

            var vp = st.Viewport;
            vp.MouseWheel += st.VWheel;
            vp.MouseDown += st.VDown;
            vp.MouseMove += st.VMove;
            vp.MouseUp += st.VUp;
            vp.Paint += st.VPaint;
            vp.Layout += st.VLayout;
            vp.Resize += st.VResize;
            vp.HandleCreated += st.VHandleCreated;
            vp.HandleDestroyed += st.VHandleDestroyed;
            vp.Disposed += st.VDisposed;
            vp.MouseCaptureChanged += st.VMouseCaptureChanged;

            surface.Layout += st.SLayout;
            surface.ControlAdded += st.SControlAdded;
            surface.ControlRemoved += st.SControlRemoved;
            surface.VisibleChanged += st.SVisibleChanged;

            surface.HandleDestroyed += st.SHandleDestroyed;
            surface.Disposed += st.SDisposed;

            if (cfg.CaptureWheelFromChildHwnds)
            {
                EnsureWheelFilterInstalled();
                EnsureLowLevelWheelHookInstalled();
            }

            OnLayout(st);
        }

        public static void DetachVertical(Control surface)
        {
            VerticalState? st;

            lock (gate)
            {
                if (!bySurface.TryGetValue(surface, out st))
                    return;

                bySurface.Remove(surface);

                if (st.Viewport != null)
                    byViewport.Remove(st.Viewport);
            }

            StopWheelTimer(st);

            if (st.Config.CaptureWheelFromChildHwnds)
            {
                ReleaseWheelFilter();
                ReleaseLowLevelWheelHook();
            }

            if (!st.Surface.IsDisposed)
            {
                if (st.SLayout != null) st.Surface.Layout -= st.SLayout;
                if (st.SControlAdded != null) st.Surface.ControlAdded -= st.SControlAdded;
                if (st.SControlRemoved != null) st.Surface.ControlRemoved -= st.SControlRemoved;
                if (st.SVisibleChanged != null) st.Surface.VisibleChanged -= st.SVisibleChanged;

                if (st.SHandleDestroyed != null) st.Surface.HandleDestroyed -= st.SHandleDestroyed;
                if (st.SDisposed != null) st.Surface.Disposed -= st.SDisposed;
            }

            if (st.Viewport != null && !st.Viewport.IsDisposed)
            {
                var vp = st.Viewport;

                if (st.VWheel != null) vp.MouseWheel -= st.VWheel;
                if (st.VDown != null) vp.MouseDown -= st.VDown;
                if (st.VMove != null) vp.MouseMove -= st.VMove;
                if (st.VUp != null) vp.MouseUp -= st.VUp;
                if (st.VPaint != null) vp.Paint -= st.VPaint;
                if (st.VLayout != null) vp.Layout -= st.VLayout;
                if (st.VResize != null) vp.Resize -= st.VResize;
                if (st.VHandleCreated != null) vp.HandleCreated -= st.VHandleCreated;
                if (st.VHandleDestroyed != null) vp.HandleDestroyed -= st.VHandleDestroyed;
                if (st.VDisposed != null) vp.Disposed -= st.VDisposed;
                if (st.VMouseCaptureChanged != null) vp.MouseCaptureChanged -= st.VMouseCaptureChanged;

                if (st.DraggingThumb)
                {
                    st.DraggingThumb = false;
                    vp.Capture = false;
                }
            }

            UnwrapIfNeeded(st);
        }

        public static void ScrollToTop(Control surface)
        {
            if (!TryGetState(surface, out var st))
                return;

            if (st.Viewport == null || st.Surface == null)
                return;

            if (st.Viewport.IsDisposed || st.Surface.IsDisposed)
                return;

            StopWheelTimer(st);

            var applied = -st.Surface.Top;
            if (applied < 0) applied = 0;
            st.AppliedOffset = applied;

            st.Offset = 0;

            ApplyImmediate(st);

            try { st.Viewport.Invalidate(); } catch { }
        }

        public static bool IsScrolling(Control surface)
        {
            return TryGetState(surface, out var st) && st.DraggingThumb;
        }

        public static int GetAppliedVerticalOffset(Control surface)
        {
            return TryGetState(surface, out var st) ? st.AppliedOffset : 0;
        }

        private static void EnsureViewport(VerticalState st)
        {
            var surface = st.Surface;

            if (surface.Parent == null)
            {
                st.Viewport = surface;
                return;
            }

            if (surface.Dock == DockStyle.None)
            {
                st.Viewport = surface;
                return;
            }

            var parent = surface.Parent;

            var wrap = new WrapInfo
            {
                Parent = parent,
                Index = parent.Controls.GetChildIndex(surface, false),
                Dock = surface.Dock,
                Anchor = surface.Anchor,
                Location = surface.Location,
                Size = surface.Size,
                Margin = surface.Margin
            };

            if (parent is TableLayoutPanel tlp)
            {
                wrap.IsTable = true;
                wrap.Row = tlp.GetRow(surface);
                wrap.Col = tlp.GetColumn(surface);
                wrap.RowSpan = tlp.GetRowSpan(surface);
                wrap.ColSpan = tlp.GetColumnSpan(surface);
            }

            var viewport = new BufferedPanel
            {
                Dock = wrap.Dock,
                Anchor = wrap.Anchor,
                Location = wrap.Location,
                Size = wrap.Size,
                Margin = wrap.Margin,
                Padding = Padding.Empty,
                BackColor = parent.BackColor
            };


            parent.SuspendLayout();
            try
            {
                parent.Controls.Remove(surface);
                parent.Controls.Add(viewport);
                parent.Controls.SetChildIndex(viewport, wrap.Index);

                if (wrap.IsTable && parent is TableLayoutPanel tlp2)
                {
                    tlp2.SetRow(viewport, wrap.Row);
                    tlp2.SetColumn(viewport, wrap.Col);
                    tlp2.SetRowSpan(viewport, wrap.RowSpan);
                    tlp2.SetColumnSpan(viewport, wrap.ColSpan);
                }

                surface.Dock = DockStyle.None;
                surface.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                surface.Margin = Padding.Empty;
                surface.Location = new Point(0, 0);

                viewport.Controls.Add(surface);
                surface.Width = Math.Max(0, viewport.ClientSize.Width);
            }
            finally
            {
                parent.ResumeLayout(true);
            }

            st.Viewport = viewport;
            st.Wrapper = viewport;
            st.Wrap = wrap;
        }

        private static void UnwrapIfNeeded(VerticalState st)
        {
            if (st.Wrapper == null || st.Wrap == null)
                return;

            var surface = st.Surface;
            var viewport = st.Wrapper;
            var wrap = st.Wrap;

            if (surface.IsDisposed)
                return;

            var parent = wrap.Parent;
            if (parent.IsDisposed)
                return;

            parent.SuspendLayout();
            try
            {
                viewport.Controls.Remove(surface);

                surface.Dock = wrap.Dock;
                surface.Anchor = wrap.Anchor;
                surface.Location = wrap.Location;
                surface.Size = wrap.Size;
                surface.Margin = wrap.Margin;

                parent.Controls.Remove(viewport);
                parent.Controls.Add(surface);
                parent.Controls.SetChildIndex(surface, wrap.Index);

                if (wrap.IsTable && parent is TableLayoutPanel tlp)
                {
                    tlp.SetRow(surface, wrap.Row);
                    tlp.SetColumn(surface, wrap.Col);
                    tlp.SetRowSpan(surface, wrap.RowSpan);
                    tlp.SetColumnSpan(surface, wrap.ColSpan);
                }

                viewport.Dispose();
            }
            finally
            {
                parent.ResumeLayout(true);
            }

            st.Wrapper = null;
            st.Wrap = null;
        }

        private static void OnResize(VerticalState st)
        {
            if (st.Viewport.IsDisposed)
                return;

            st.Viewport.Invalidate();

            RequestRelayout(st);
        }
        private static void RequestRelayout(VerticalState st)
        {
            if (st.RelayoutQueued)
                return;

            if (st.Viewport == null || st.Surface == null)
                return;

            if (st.Viewport.IsDisposed || st.Surface.IsDisposed)
                return;

            var invoker = st.Viewport;

            if (!invoker.IsHandleCreated)
                return;

            st.RelayoutQueued = true;

            try
            {
                invoker.BeginInvoke(new Action(() =>
                {
                    st.RelayoutQueued = false;

                    if (st.Viewport.IsDisposed || st.Surface.IsDisposed)
                        return;

                    bool ok;
                    lock (gate)
                        ok = bySurface.TryGetValue(st.Surface, out var cur) && ReferenceEquals(cur, st);

                    if (!ok)
                        return;

                    OnLayout(st);
                }));
            }
            catch
            {
                st.RelayoutQueued = false;
            }
        }
        private static void OnLayout(VerticalState st)
        {
            if (st.AdjustDepth > 0)
                return;

            bool ok;
            lock (gate)
                ok = bySurface.TryGetValue(st.Surface, out var cur) && ReferenceEquals(cur, st);

            if (!ok)
                return;

            if (st.Viewport.IsDisposed || st.Surface.IsDisposed)
                return;

            StopWheelTimer(st);

            st.AdjustDepth++;
            try
            {
                var vp = st.Viewport;
                var surface = st.Surface;

                int viewportH = vp.ClientSize.Height;
                int viewportW = vp.ClientSize.Width;
                if (viewportH < 0) viewportH = 0;
                if (viewportW < 0) viewportW = 0;

                int contentH = GetContentHeight(st, vp, surface);
                if (contentH < 0) contentH = 0;

                int maxY = contentH - viewportH;
                if (maxY < 0) maxY = 0;

                st.MaxScrollY = maxY;
                st.Visible = maxY > 0;

                int desired = st.Offset;
                if (!st.Visible)
                    desired = 0;
                else
                {
                    if (desired < 0) desired = 0;
                    if (desired > st.MaxScrollY) desired = st.MaxScrollY;
                }

                st.Offset = desired;

                int surfaceW = viewportW;
                if (st.Config.ReserveScrollbarSpace)
                {
                    int sbw = SystemInformation.VerticalScrollBarWidth;
                    surfaceW = viewportW - sbw;
                    if (surfaceW < 0) surfaceW = 0;
                }

                if (surface.Width != surfaceW)
                    surface.Width = surfaceW;

                if (surface.Height != contentH)
                    surface.Height = contentH;

                int applied = -surface.Top;
                if (applied < 0) applied = 0;
                if (applied > st.MaxScrollY) applied = st.MaxScrollY;
                st.AppliedOffset = applied;

                int delta = st.Offset - st.AppliedOffset;
                if (delta != 0)
                {
                    vp.SuspendLayout();
                    surface.SuspendLayout();
                    try
                    {
                        surface.Top -= delta;
                    }
                    finally
                    {
                        surface.ResumeLayout(false);
                        vp.ResumeLayout(false);
                    }

                    st.AppliedOffset += delta;
                }
                else
                {
                    int wantTop = -st.AppliedOffset;
                    if (surface.Top != wantTop)
                        surface.Top = wantTop;
                }

                vp.Invalidate();
            }
            finally
            {
                st.AdjustDepth--;
            }
        }

        private static int GetContentHeight(VerticalState st, Control vp, Control surface)
        {
            if (st.Config.GetContentHeight != null)
                return st.Config.GetContentHeight(surface);

            int maxBottom = 0;

            for (int i = 0; i < surface.Controls.Count; i++)
            {
                var c = surface.Controls[i];
                if (!c.Visible)
                    continue;

                int mb = c.Margin.Bottom; if (mb < 0) mb = 0;
                int b = c.Bottom + mb;
                if (b > maxBottom)
                    maxBottom = b;
            }

            int pt = surface.Padding.Top;
            int pb = surface.Padding.Bottom;
            int total = maxBottom + pt + pb;
            return total;
        }

        private static void SetTargetOffset(VerticalState st, int value, bool immediate)
        {
            if (!st.Visible)
                return;

            if (value < 0) value = 0;
            if (value > st.MaxScrollY) value = st.MaxScrollY;

            st.Offset = value;

            if (immediate || st.Config.WheelCoalesceMs <= 0)
            {
                StopWheelTimer(st);
                ApplyImmediate(st);
                return;
            }

            EnsureWheelTimer(st);
            StartWheelTimer(st);
            InvalidateScrollbar(st);
        }

        private static void ApplyImmediate(VerticalState st)
        {
            int total = st.Offset - st.AppliedOffset;
            if (total == 0)
                return;

            st.Viewport.SuspendLayout();
            st.Surface.SuspendLayout();
            try
            {
                st.Surface.Top -= total;
            }
            finally
            {
                st.Surface.ResumeLayout(false);
                st.Viewport.ResumeLayout(false);
            }

            st.AppliedOffset += total;
            InvalidateScrollbar(st);
        }

        private static void OnMouseWheel(VerticalState st, MouseEventArgs e)
        {
            if (!st.Visible)
                return;

            int notches = e.Delta / 120;
            if (notches == 0)
                return;

            SetTargetOffset(st, st.Offset - (notches * st.Config.WheelStepPx), immediate: false);
        }

        private static void OnMouseWheelDelta(VerticalState st, int delta)
        {
            if (!st.Visible)
                return;

            int notches = delta / 120;
            if (notches == 0)
                return;

            SetTargetOffset(st, st.Offset - (notches * st.Config.WheelStepPx), immediate: false);
        }

        private static void OnMouseDown(VerticalState st, MouseEventArgs e)
        {
            if (!st.Visible || e.Button != MouseButtons.Left)
                return;

            var sb = GetScrollBarRect(st.Viewport);
            if (!sb.Contains(e.Location))
                return;

            var thumb = GetThumbRect(st, sb);

            if (thumb.Contains(e.Location))
            {
                StopWheelTimer(st);
                st.DraggingThumb = true;
                st.ThumbDragOffsetY = e.Y - thumb.Y;
                st.Viewport.Capture = true;
                return;
            }

            int page = st.Viewport.ClientSize.Height;
            if (e.Y < thumb.Y)
                SetTargetOffset(st, st.Offset - page, immediate: true);
            else if (e.Y > thumb.Bottom)
                SetTargetOffset(st, st.Offset + page, immediate: true);
        }

        private static void OnMouseMove(VerticalState st, MouseEventArgs e)
        {
            if (!st.DraggingThumb || !st.Visible)
                return;

            if (st.Config.DragThrottleMs > 0)
            {
                long now = Environment.TickCount64;
                if ((now - st.LastDragTick) < st.Config.DragThrottleMs)
                    return;

                st.LastDragTick = now;
            }

            var sb = GetScrollBarRect(st.Viewport);
            var thumb = GetThumbRect(st, sb);

            int trackY = sb.Y;
            int trackH = sb.Height;
            int thumbH = thumb.Height;

            int thumbMinY = trackY;
            int thumbMaxY = trackY + trackH - thumbH;

            int newThumbY = e.Y - st.ThumbDragOffsetY;
            if (newThumbY < thumbMinY) newThumbY = thumbMinY;
            if (newThumbY > thumbMaxY) newThumbY = thumbMaxY;

            if (st.MaxScrollY <= 0 || thumbMaxY == thumbMinY)
            {
                SetTargetOffset(st, 0, immediate: true);
                return;
            }

            double t = (double)(newThumbY - thumbMinY) / (thumbMaxY - thumbMinY);
            int newScroll = (int)Math.Round(t * st.MaxScrollY);
            SetTargetOffset(st, newScroll, immediate: true);
        }

        private static void OnMouseUp(VerticalState st, MouseEventArgs e)
        {
            if (!st.DraggingThumb)
                return;

            st.DraggingThumb = false;
            st.Viewport.Capture = false;
        }

        private static void OnMouseCaptureChanged(VerticalState st)
        {
            if (st.DraggingThumb && !st.Viewport.Capture)
                st.DraggingThumb = false;
        }

        private static void OnPaint(VerticalState st, PaintEventArgs e)
        {
            if (!st.Visible)
                return;

            var sb = GetScrollBarRect(st.Viewport);
            if (sb.Width <= 0 || sb.Height <= 0)
                return;

            using (var b = new SolidBrush(st.Config.Style.TrackBackColor))
                e.Graphics.FillRectangle(b, sb);

            var thumb = GetThumbRect(st, sb);

            using (var b = new SolidBrush(st.Config.Style.ThumbBackColor))
                e.Graphics.FillRectangle(b, thumb);

            using (var p = new Pen(st.Config.Style.BorderColor))
                e.Graphics.DrawRectangle(p, sb.X, sb.Y, sb.Width - 1, sb.Height - 1);
        }

        private static Rectangle GetScrollBarRect(Control viewport)
        {
            int w = SystemInformation.VerticalScrollBarWidth;
            int x = viewport.ClientSize.Width - w;
            if (x < 0) x = 0;
            return new Rectangle(x, 0, w, viewport.ClientSize.Height);
        }
        private static bool TryGetState(Control surface, out VerticalState st)
        {
            lock (gate)
                return bySurface.TryGetValue(surface, out st!);
        }

        private static Rectangle GetThumbRect(VerticalState st, Rectangle sb)
        {
            int viewport = sb.Height;
            int content = viewport + st.MaxScrollY;

            double ratio = content <= 0 ? 1.0 : (double)viewport / content;
            int thumbH = (int)Math.Round(sb.Height * ratio);

            int minH = st.Config.MinThumbHeightPx;
            if (thumbH < minH) thumbH = minH;
            if (thumbH > sb.Height) thumbH = sb.Height;

            int trackSpan = sb.Height - thumbH;
            int thumbY = sb.Y;

            if (trackSpan > 0 && st.MaxScrollY > 0)
            {
                double t = (double)st.Offset / st.MaxScrollY;
                thumbY = sb.Y + (int)Math.Round(trackSpan * t);
            }

            int inset = 1;
            int x = sb.X + inset;
            int y = thumbY + inset;
            int w = sb.Width - (inset * 2);
            int h = thumbH - (inset * 2);

            if (w < 1) w = 1;
            if (h < 1) h = 1;

            return new Rectangle(x, y, w, h);
        }

        private static void EnsureWheelTimer(VerticalState st)
        {
            int ms = st.Config.WheelCoalesceMs;
            if (ms <= 0)
                return;

            if (st.WheelTimer != null)
            {
                if (st.WheelTimer.Interval != ms)
                    st.WheelTimer.Interval = ms;
                return;
            }

            st.WheelTimer = new Timer();
            st.WheelTimer.Interval = ms;

            st.HWheelTick = (_, __) => OnWheelTick(st);
            st.WheelTimer.Tick += st.HWheelTick;
        }

        private static void StartWheelTimer(VerticalState st)
        {
            if (st.WheelTimer == null)
                return;

            if (!st.WheelTimer.Enabled)
                st.WheelTimer.Start();
        }

        private static void StopWheelTimer(VerticalState st)
        {
            if (st.WheelTimer == null)
                return;

            try { st.WheelTimer.Stop(); } catch { }

            if (st.HWheelTick != null)
            {
                try { st.WheelTimer.Tick -= st.HWheelTick; } catch { }
                st.HWheelTick = null;
            }

            try { st.WheelTimer.Dispose(); } catch { }

            st.WheelTimer = null;
        }

        private static void OnWheelTick(VerticalState st)
        {
            if (st.Viewport.IsDisposed)
            {
                StopWheelTimer(st);
                return;
            }

            bool ok;
            lock (gate)
                ok = bySurface.TryGetValue(st.Surface, out var cur) && ReferenceEquals(cur, st);

            if (!ok)
            {
                StopWheelTimer(st);
                return;
            }

            if (!st.Visible)
            {
                StopWheelTimer(st);
                return;
            }

            if (st.Offset < 0) st.Offset = 0;
            if (st.Offset > st.MaxScrollY) st.Offset = st.MaxScrollY;

            int total = st.Offset - st.AppliedOffset;
            if (total == 0)
            {
                StopWheelTimer(st);
                return;
            }

            int delta = total;

            int maxPerTick = st.Config.WheelMaxDeltaPerTickPx;
            if (maxPerTick > 0)
            {
                if (delta > maxPerTick) delta = maxPerTick;
                else if (delta < -maxPerTick) delta = -maxPerTick;
            }

            st.Viewport.SuspendLayout();
            st.Surface.SuspendLayout();
            try
            {
                st.Surface.Top -= delta;
            }
            finally
            {
                st.Surface.ResumeLayout(false);
                st.Viewport.ResumeLayout(false);
            }

            st.AppliedOffset += delta;
            InvalidateScrollbar(st);

            if (st.Offset == st.AppliedOffset)
                StopWheelTimer(st);
        }

        private static void EnsureLowLevelWheelHookInstalled()
        {
            lock (gate)
            {
                if (llWheelHookUsers == 0)
                    llWheelHook = new LowLevelWheelHook();

                llWheelHookUsers++;
            }
        }
        private static void ReleaseLowLevelWheelHook()
        {
            LowLevelWheelHook? toDispose = null;

            lock (gate)
            {
                if (llWheelHookUsers <= 0)
                    return;

                llWheelHookUsers--;

                if (llWheelHookUsers == 0)
                {
                    toDispose = llWheelHook;
                    llWheelHook = null;
                }
            }

            toDispose?.Dispose();
        }

        private static void EnsureWheelFilterInstalled()
        {
            lock (gate)
            {
                if (wheelFilterUsers == 0)
                {
                    wheelFilter ??= new WheelMessageFilter();
                    Application.AddMessageFilter(wheelFilter);
                }

                wheelFilterUsers++;
            }
        }

        private static void ReleaseWheelFilter()
        {
            lock (gate)
            {
                if (wheelFilterUsers <= 0)
                    return;

                wheelFilterUsers--;

                if (wheelFilterUsers == 0 && wheelFilter != null)
                    Application.RemoveMessageFilter(wheelFilter);
            }
        }
        private static void TryEnableDoubleBuffering(Control c)
        {
            try
            {
                var pi = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                pi?.SetValue(c, true);
            }
            catch { }
        }

        private static void InvalidateScrollbar(VerticalState st)
        {
            var vp = st.Viewport;
            if (vp == null || vp.IsDisposed)
                return;

            try { vp.Invalidate(GetScrollBarRect(vp)); } catch { }
        }

        public sealed class VerticalConfig
        {
            public static VerticalConfig Default => new VerticalConfig
            {
                ReserveScrollbarSpace = true,
                CaptureWheelFromChildHwnds = true,
                WheelStepPx = 64,
                WheelCoalesceMs = 16,
                WheelMaxDeltaPerTickPx = 96
            };

            public Func<Control, int>? GetContentHeight { get; set; }

            public VerticalStyle Style { get; set; } = new VerticalStyle();

            public int MinThumbHeightPx { get; set; } = 28;
            public int DragThrottleMs { get; set; } = 0;

            public int WheelStepPx { get; set; } = 48;
            public int WheelCoalesceMs { get; set; } = 16;
            public int WheelMaxDeltaPerTickPx { get; set; } = 96;

            public bool ReserveScrollbarSpace { get; set; } = true;
            public bool CaptureWheelFromChildHwnds { get; set; } = true;
        }

        public sealed class VerticalStyle
        {
            public static VerticalStyle Default => new VerticalStyle
            {
                TrackBackColor = Color.Black,
                ThumbBackColor = Color.FromArgb(70, 70, 70),
                BorderColor = Color.FromArgb(110, 110, 110)
            };

            public Color TrackBackColor { get; set; } = Color.Black;
            public Color ThumbBackColor { get; set; } = Color.FromArgb(70, 70, 70);
            public Color BorderColor { get; set; } = Color.FromArgb(110, 110, 110);

            public VerticalStyle Clone()
            {
                return new VerticalStyle
                {
                    TrackBackColor = TrackBackColor,
                    ThumbBackColor = ThumbBackColor,
                    BorderColor = BorderColor
                };
            }
        }

        private sealed class WrapInfo
        {
            public Control Parent = default!;
            public int Index;
            public DockStyle Dock;
            public AnchorStyles Anchor;
            public Point Location;
            public Size Size;
            public Padding Margin;

            public bool IsTable;
            public int Row;
            public int Col;
            public int RowSpan;
            public int ColSpan;
        }

        private sealed class WheelMessageFilter : IMessageFilter
        {
            private const int WM_MOUSEWHEEL = 0x020A;

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != WM_MOUSEWHEEL)
                    return false;

                int delta = (short)((m.WParam.ToInt64() >> 16) & 0xFFFF);
                if (delta == 0)
                    return false;

                var mouse = Control.MousePosition;

                KeyValuePair<Control, VerticalState>[] snap;
                lock (gate)
                    snap = byViewport.ToArray();

                VerticalState? st = null;

                for (int i = 0; i < snap.Length; i++)
                {
                    var vp = snap[i].Key;
                    if (vp.IsDisposed || !vp.IsHandleCreated || !vp.Visible)
                        continue;

                    Rectangle r;
                    try { r = vp.RectangleToScreen(vp.ClientRectangle); }
                    catch { continue; }

                    if (!r.Contains(mouse))
                        continue;

                    st = snap[i].Value;
                    break;
                }

                if (st == null)
                    return false;

                if (!st.Config.CaptureWheelFromChildHwnds || !st.Visible)
                    return false;

                OnMouseWheelDelta(st, delta);
                return true;
            }
        }

        private sealed class LowLevelWheelHook : IDisposable
        {
            private const int WH_MOUSE_LL = 14;
            private const int WM_MOUSEWHEEL = 0x020A;

            private readonly HookProc proc;
            private IntPtr hook;

            private readonly uint selfPid = (uint)Environment.ProcessId;

            private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

            [StructLayout(LayoutKind.Sequential)]
            private struct POINT
            {
                public int x;
                public int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct MSLLHOOKSTRUCT
            {
                public POINT pt;
                public uint mouseData;
                public uint flags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll")]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string? lpModuleName);

            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            private static extern IntPtr WindowFromPoint(POINT pt);

            [DllImport("user32.dll")]
            private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            public LowLevelWheelHook()
            {
                proc = HookCallback;
                var mod = GetModuleHandle(null);
                hook = SetWindowsHookEx(WH_MOUSE_LL, proc, mod, 0);
            }

            public void Dispose()
            {
                var h = hook;
                hook = IntPtr.Zero;

                if (h != IntPtr.Zero)
                    UnhookWindowsHookEx(h);
            }

            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode < 0)
                    return CallNextHookEx(hook, nCode, wParam, lParam);

                int msg = wParam.ToInt32();
                if (msg != WM_MOUSEWHEEL)
                    return CallNextHookEx(hook, nCode, wParam, lParam);

                var info = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                int delta = unchecked((short)((info.mouseData >> 16) & 0xFFFF));
                if (delta == 0)
                    return CallNextHookEx(hook, nCode, wParam, lParam);

                if (TryRouteWheel(info.pt, delta))
                    return (IntPtr)1;

                return CallNextHookEx(hook, nCode, wParam, lParam);
            }

            private bool TryRouteWheel(POINT pt, int delta)
            {
                KeyValuePair<Control, VerticalState>[] snap;
                lock (gate)
                    snap = byViewport.ToArray();

                VerticalState? st = null;

                for (int i = 0; i < snap.Length; i++)
                {
                    var vp = snap[i].Key;
                    if (vp.IsDisposed || !vp.IsHandleCreated || !vp.Visible)
                        continue;

                    Rectangle r;
                    try { r = vp.RectangleToScreen(vp.ClientRectangle); }
                    catch { continue; }

                    if (!r.Contains(pt.x, pt.y))
                        continue;

                    st = snap[i].Value;
                    break;
                }

                if (st == null)
                    return false;

                if (!st.Config.CaptureWheelFromChildHwnds || !st.Visible)
                    return false;

                var vp2 = st.Viewport;
                if (vp2.IsDisposed || !vp2.IsHandleCreated)
                    return true;

                var form = vp2.FindForm();
                if (form == null || form.IsDisposed || !form.IsHandleCreated)
                    return false;

                if (GetForegroundWindow() != form.Handle)
                    return false;

                var hwnd = WindowFromPoint(pt);
                if (hwnd != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(hwnd, out var pid);
                    if (pid == selfPid)
                        return false;
                }

                if (vp2.InvokeRequired)
                {
                    try
                    {
                        vp2.BeginInvoke(new Action(() =>
                        {
                            if (vp2.IsDisposed)
                                return;

                            lock (gate)
                            {
                                if (!bySurface.TryGetValue(st.Surface, out var curSt) || !ReferenceEquals(curSt, st))
                                    return;
                            }

                            if (!st.Visible)
                                return;

                            OnMouseWheelDelta(st, delta);
                        }));
                    }
                    catch { }
                }
                else
                {
                    bool ok;
                    lock (gate)
                        ok = bySurface.TryGetValue(st.Surface, out var curSt) && ReferenceEquals(curSt, st);

                    if (ok && st.Visible)
                        OnMouseWheelDelta(st, delta);
                }

                return true;
            }
        }

        private sealed class VerticalState
        {
            public EventHandler? SHandleDestroyed;
            public EventHandler? SDisposed;

            public readonly Control Surface;
            public readonly VerticalConfig Config;

            public Control Viewport = default!;
            public Panel? Wrapper;
            public WrapInfo? Wrap;

            public bool Visible;
            public int MaxScrollY;

            public int Offset;
            public int AppliedOffset;

            public bool DraggingThumb;
            public int ThumbDragOffsetY;
            public long LastDragTick;

            public int AdjustDepth;

            public Timer? WheelTimer;
            public EventHandler? HWheelTick;

            public MouseEventHandler? VWheel;
            public MouseEventHandler? VDown;
            public MouseEventHandler? VMove;
            public MouseEventHandler? VUp;
            public PaintEventHandler? VPaint;
            public LayoutEventHandler? VLayout;
            public EventHandler? VResize;
            public EventHandler? VHandleDestroyed;
            public EventHandler? VDisposed;
            public EventHandler? VMouseCaptureChanged;

            public EventHandler? VHandleCreated;

            public LayoutEventHandler? SLayout;
            public ControlEventHandler? SControlAdded;
            public ControlEventHandler? SControlRemoved;
            public EventHandler? SVisibleChanged;

            public bool RelayoutQueued;

            public VerticalState(Control surface, VerticalConfig? cfg)
            {
                Surface = surface;
                Config = cfg ?? VerticalConfig.Default;
            }
        }

        private sealed class BufferedPanel : Panel
        {
            public BufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }
    }
}
