using datinate.app;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace app.datinate
{
    public sealed class FlowReorderDragManager
    {
        private const int WM_SETREDRAW = 0x000B;
        private const int DragThresholdPx = 5;
        private const int EdgeScrollPx = 24;
        private const int EdgeScrollStepPx = 18;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private readonly ProjectLoaderView owner;
        private readonly FlowLayoutPanel panel;

        private readonly HashSet<ProjectDatUI> registered = new();
        private readonly HashSet<Control> wiredRoots = new();

        private ProjectDatUI? mouseDownUi;
        private Point mouseDownScreen;

        private bool dragging;
        private ProjectDatUI? draggingUi;
        private DottedPlaceholderControl? placeholder;
        private int originalIndexReal;

        private InsertAdornerForm? insertAdorner;
        private Timer? dragTick;

        public FlowReorderDragManager(ProjectLoaderView owner, FlowLayoutPanel panel)
        {
            this.owner = owner;
            this.panel = panel;
        }

        public void Register(ProjectDatUI ui)
        {
            if (!registered.Add(ui))
                return;

            WireChildRootsRecursive(ui);
        }

        public void Unregister(ProjectDatUI ui)
        {
            _ = registered.Remove(ui);

            var roots = wiredRoots.Where(r => ReferenceEquals(FindParentUi(r), ui)).ToArray();
            foreach (var r in roots)
                UnwireRoot(r);

            if (ReferenceEquals(mouseDownUi, ui))
                mouseDownUi = null;

            if (ReferenceEquals(draggingUi, ui))
                CancelDrag();
        }

        private void WireChildRootsRecursive(Control root)
        {
            WireRoot(root);

            foreach (Control child in root.Controls)
                WireChildRootsRecursive(child);
        }

        private void WireRoot(Control ctrl)
        {
            if (!wiredRoots.Add(ctrl))
                return;

            ctrl.MouseDown += RootMouseDown;
            ctrl.MouseMove += RootMouseMove;
            ctrl.MouseUp += RootMouseUp;
        }

        private void UnwireRoot(Control ctrl)
        {
            if (!wiredRoots.Remove(ctrl))
                return;

            ctrl.MouseDown -= RootMouseDown;
            ctrl.MouseMove -= RootMouseMove;
            ctrl.MouseUp -= RootMouseUp;
        }

        private static ProjectDatUI? FindParentUi(object? sender)
        {
            if (sender is not Control c)
                return null;

            while (c != null)
            {
                if (c is ProjectDatUI ui)
                    return ui;

                c = c.Parent;
            }

            return null;
        }

        private void RootMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (sender is ButtonBase)
                return;

            if (dragging)
                return;

            var ui = FindParentUi(sender);
            if (ui == null)
                return;

            if (!registered.Contains(ui))
                return;

            if (ui.IsBeingRemoved)
                return;

            mouseDownUi = ui;
            mouseDownScreen = Control.MousePosition;
        }

        private void RootMouseMove(object? sender, MouseEventArgs e)
        {
            if (dragging)
                return;

            if (mouseDownUi == null)
                return;

            if ((Control.MouseButtons & MouseButtons.Left) == 0)
            {
                mouseDownUi = null;
                return;
            }

            var p = Control.MousePosition;
            var dx = Math.Abs(p.X - mouseDownScreen.X);
            var dy = Math.Abs(p.Y - mouseDownScreen.Y);

            if (dx < DragThresholdPx && dy < DragThresholdPx)
                return;

            BeginDrag(mouseDownUi);
        }

        private void RootMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (!dragging)
            {
                mouseDownUi = null;
                return;
            }

            CommitDrag();
        }

        private void BeginDrag(ProjectDatUI ui)
        {
            dragging = true;
            draggingUi = ui;

            DragDropPreviewForm.Initialise(ui, null, null);

            var startControlsIndex = panel.Controls.GetChildIndex(ui);

            placeholder = new DottedPlaceholderControl(panel.BackColor)
            {
                Size = ui.Size,
                Margin = ui.Margin
            };

            panel.SuspendLayout();
            panel.Controls.Remove(ui);
            panel.Controls.Add(placeholder);
            panel.Controls.SetChildIndex(placeholder, startControlsIndex);
            panel.ResumeLayout(false);
            panel.PerformLayout();

            originalIndexReal = GetInsertIndexFromPlaceholder();

            insertAdorner = new InsertAdornerForm(panel) { Owner = panel.FindForm() };
            insertAdorner.ShowNoActivate();

            dragTick = new Timer { Interval = 15 };
            dragTick.Tick += DragTick;
            dragTick.Start();

            panel.MouseUp += PanelMouseUp;
            panel.Capture = true;

            mouseDownUi = null;
        }

        private void PanelMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (!dragging)
                return;

            CommitDrag();
        }

        private void DragTick(object? sender, EventArgs e)
        {
            if (!dragging || draggingUi == null)
                return;

            var mouseScreen = Control.MousePosition;
            var mouseClient = panel.PointToClient(mouseScreen);

            insertAdorner?.SyncToPanelBounds();

            AutoScrollIfNeeded(mouseClient);

            if (!TryComputeInsert(mouseClient, out var idx, out var indicator))
            {
                panel.Cursor = Cursors.No;
                insertAdorner?.HideIndicator();
                return;
            }

            panel.Cursor = Cursors.Default;

            if (idx == originalIndexReal)
            {
                insertAdorner?.HideIndicator();
                return;
            }

            insertAdorner?.ShowIndicator(indicator);
        }
        private bool TryComputeInsert(Point mouseClient, out int insertIndexReal, out InsertIndicator indicator)
        {
            insertIndexReal = originalIndexReal;
            indicator = InsertIndicator.Hidden;

            if (!panel.ClientRectangle.Contains(mouseClient))
                return false;

            if (placeholder != null && placeholder.Bounds.Contains(mouseClient))
                return true;

            var items = panel.Controls
                .OfType<ProjectDatUI>()
                .Where(p => !ReferenceEquals(p, draggingUi))
                .ToList();

            if (items.Count == 0)
                return false;

            ProjectDatUI? hit = null;
            var hitIndex = -1;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Bounds.Contains(mouseClient))
                {
                    hit = items[i];
                    hitIndex = i;
                    break;
                }
            }

            if (hit == null)
                return false;

            var insertLeft = hitIndex;
            var insertRight = hitIndex + 1;

            var leftMoves = insertLeft != originalIndexReal;
            var rightMoves = insertRight != originalIndexReal;

            bool after;
            if (leftMoves ^ rightMoves)
            {
                after = rightMoves;
            }
            else
            {
                var midX = hit.Bounds.Left + (hit.Bounds.Width / 2);
                after = mouseClient.X >= midX;
            }

            insertIndexReal = after ? insertRight : insertLeft;

            var x = after ? hit.Bounds.Right : hit.Bounds.Left;
            x = Math.Max(0, Math.Min(panel.ClientSize.Width, x));

            var side = after ? InsertSide.Right : InsertSide.Left;

            indicator = new InsertIndicator(x, hit.Bounds.Top, hit.Bounds.Bottom, hit.Bounds, side);
            return true;
        }

        private void AutoScrollIfNeeded(Point mouseClient)
        {
            if (panel.Controls.Count == 0)
                return;

            if (mouseClient.X < EdgeScrollPx)
            {
                if (panel.HorizontalScroll.Visible)
                    SetScroll(panel.HorizontalScroll, panel.HorizontalScroll.Value - EdgeScrollStepPx);
            }
            else if (mouseClient.X > panel.ClientSize.Width - EdgeScrollPx)
            {
                if (panel.HorizontalScroll.Visible)
                    SetScroll(panel.HorizontalScroll, panel.HorizontalScroll.Value + EdgeScrollStepPx);
            }

            if (mouseClient.Y < EdgeScrollPx)
            {
                if (panel.VerticalScroll.Visible)
                    SetScroll(panel.VerticalScroll, panel.VerticalScroll.Value - EdgeScrollStepPx);
            }
            else if (mouseClient.Y > panel.ClientSize.Height - EdgeScrollPx)
            {
                if (panel.VerticalScroll.Visible)
                    SetScroll(panel.VerticalScroll, panel.VerticalScroll.Value + EdgeScrollStepPx);
            }
        }

        private static void SetScroll(ScrollProperties scroll, int value)
        {
            var min = scroll.Minimum;
            var max = scroll.Maximum - scroll.LargeChange + 1;
            if (max < min)
                max = min;

            value = Math.Max(min, Math.Min(max, value));

            try { scroll.Value = value; } catch { }
        }

        private int GetInsertIndexFromPlaceholder()
        {
            if (placeholder == null)
                return 0;

            var idx = 0;
            foreach (Control c in panel.Controls)
            {
                if (ReferenceEquals(c, placeholder))
                    break;

                if (c is ProjectDatUI)
                    idx++;
            }

            return idx;
        }

        private void CommitDrag()
        {
            var ui = draggingUi;
            var ph = placeholder;

            if (ui == null || ph == null)
            {
                CancelDrag();
                return;
            }

            var mouseClient = panel.PointToClient(Control.MousePosition);

            var canDrop = TryComputeInsert(mouseClient, out var computedIndex, out _);
            if (!canDrop)
                computedIndex = originalIndexReal;

            TearDownDragUi();

            if (computedIndex == originalIndexReal)
            {
                RestoreNoMove(ui, ph);
                return;
            }

            AnimateCommit(ui, ph, computedIndex);
        }

        private void RestoreNoMove(ProjectDatUI ui, DottedPlaceholderControl ph)
        {
            panel.SuspendLayout();
            panel.Controls.Remove(ph);
            panel.Controls.Add(ui);
            SetChildIndexByProjectIndex(ui, originalIndexReal);
            panel.ResumeLayout(false);
            panel.PerformLayout();

            try { ph.Dispose(); } catch { }

            owner.CheckParentSetup(panel);
        }

        private void AnimateCommit(ProjectDatUI ui, DottedPlaceholderControl ph, int targetIndexReal)
        {
            panel.PerformLayout();

            var viewport = panel.ClientRectangle;

            var startRects = new Dictionary<ProjectDatUI, Rectangle>();
            foreach (var p in panel.Controls.OfType<ProjectDatUI>())
            {
                if (p.Bounds.IntersectsWith(viewport))
                    startRects[p] = p.Bounds;
            }

            var startPanelBmp = CaptureBitmap(panel, panel.ClientSize);

            var bmpByUi = new Dictionary<ProjectDatUI, Bitmap>();
            foreach (var kv in startRects)
                bmpByUi[kv.Key] = CropFrom(startPanelBmp, kv.Value);

            startPanelBmp.Dispose();

            SetRedraw(panel, false);

            panel.SuspendLayout();
            panel.Controls.Remove(ph);
            panel.Controls.Add(ui);
            SetChildIndexByProjectIndex(ui, targetIndexReal);
            panel.ResumeLayout(false);
            panel.PerformLayout();

            try { ph.Dispose(); } catch { }

            var endRects = new Dictionary<ProjectDatUI, Rectangle>();
            foreach (var p in panel.Controls.OfType<ProjectDatUI>())
            {
                if (ReferenceEquals(p, ui))
                    continue;

                if (p.Bounds.IntersectsWith(viewport))
                    endRects[p] = p.Bounds;
            }

            var endPanelBmp = CaptureBitmap(panel, panel.ClientSize);

            foreach (var kv in endRects)
            {
                if (!bmpByUi.ContainsKey(kv.Key))
                    bmpByUi[kv.Key] = CropFrom(endPanelBmp, kv.Value);
            }

            endPanelBmp.Dispose();

            var spriteItems = new HashSet<ProjectDatUI>(startRects.Keys);
            foreach (var k in endRects.Keys)
                _ = spriteItems.Add(k);

            var spriteList = spriteItems
                .Where(p => !ReferenceEquals(p, ui))
                .ToList();

            if (spriteList.Count == 0)
            {
                SetRedraw(panel, true);
                panel.Invalidate(true);
                panel.Update();

                foreach (var b in bmpByUi.Values)
                    try { b.Dispose(); } catch { }

                owner.CheckParentSetup(panel);
                return;
            }

            var bitmaps = new Bitmap[spriteList.Count];
            var start = new Rectangle[spriteList.Count];
            var end = new Rectangle[spriteList.Count];
            var current = new Rectangle[spriteList.Count];

            for (int i = 0; i < spriteList.Count; i++)
            {
                var item = spriteList[i];

                bitmaps[i] = bmpByUi[item];

                start[i] = startRects.TryGetValue(item, out var s) ? s : endRects[item];
                end[i] = endRects.TryGetValue(item, out var e) ? e : startRects[item];
                current[i] = start[i];
            }

            var overlay = new ReorderOverlayForm(panel, bitmaps, current) { Owner = panel.FindForm() };
            overlay.ShowNoActivate();

            var sw = Stopwatch.StartNew();
            var durationMs = 180;
            var timer = new Timer { Interval = 15 };

            timer.Tick += (_, __) =>
            {
                var t = sw.Elapsed.TotalMilliseconds / durationMs;
                if (t >= 1.0)
                    t = 1.0;

                t = EaseInOutCubic(t);

                for (int i = 0; i < current.Length; i++)
                    current[i] = LerpRect(start[i], end[i], t);

                overlay.Invalidate();

                if (t >= 1.0)
                {
                    timer.Stop();
                    timer.Dispose();

                    try { overlay.Close(); } catch { }
                    try { overlay.Dispose(); } catch { }

                    SetRedraw(panel, true);
                    panel.Invalidate(true);
                    panel.Update();

                    foreach (var b in bmpByUi.Values)
                        try { b.Dispose(); } catch { }

                    owner.CheckParentSetup(panel);
                }
            };

            timer.Start();
        }

        private void SetChildIndexByProjectIndex(ProjectDatUI ui, int projectIndex)
        {
            var targetControlsIndex = ToControlsIndexForProjectInsert(projectIndex);
            panel.Controls.SetChildIndex(ui, targetControlsIndex);
        }

        private int ToControlsIndexForProjectInsert(int projectIndex)
        {
            if (projectIndex <= 0)
                return 0;

            var count = 0;
            for (int i = 0; i < panel.Controls.Count; i++)
            {
                if (panel.Controls[i] is ProjectDatUI)
                {
                    if (count == projectIndex)
                        return i;

                    count++;
                }
            }

            return panel.Controls.Count;
        }

        private void TearDownDragUi()
        {
            dragging = false;

            if (dragTick != null)
            {
                dragTick.Stop();
                dragTick.Tick -= DragTick;
                dragTick.Dispose();
                dragTick = null;
            }

            try { insertAdorner?.HideIndicator(); } catch { }

            try
            {
                if (insertAdorner != null && !insertAdorner.IsDisposed)
                {
                    insertAdorner.Close();
                    insertAdorner.Dispose();
                }
            }
            catch { }

            insertAdorner = null;

            DragDropPreviewForm.Teardown();

            panel.MouseUp -= PanelMouseUp;
            panel.Capture = false;
            panel.Cursor = Cursors.Default;

            mouseDownUi = null;
        }

        private void CancelDrag()
        {
            TearDownDragUi();

            if (draggingUi != null && placeholder != null)
            {
                RestoreNoMove(draggingUi, placeholder);
            }

            draggingUi = null;
            placeholder = null;
        }

        private static void SetRedraw(Control c, bool enabled)
        {
            if (!c.IsHandleCreated) return;
            _ = SendMessage(c.Handle, WM_SETREDRAW, enabled ? new IntPtr(1) : IntPtr.Zero, IntPtr.Zero);
        }

        private static Bitmap CaptureBitmap(Control c, Size size)
        {
            var w = Math.Max(1, size.Width);
            var h = Math.Max(1, size.Height);

            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            c.DrawToBitmap(bmp, new Rectangle(Point.Empty, new Size(w, h)));
            return bmp;
        }

        private static Bitmap CropFrom(Bitmap src, Rectangle r)
        {
            var outBmp = new Bitmap(Math.Max(1, r.Width), Math.Max(1, r.Height), PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(outBmp))
            {
                g.Clear(Color.Transparent);

                var srcRect = new Rectangle(Point.Empty, src.Size);
                var rr = Rectangle.Intersect(srcRect, r);

                if (rr.Width > 0 && rr.Height > 0)
                {
                    var dest = new Rectangle(rr.X - r.X, rr.Y - r.Y, rr.Width, rr.Height);
                    g.DrawImage(src, dest, rr, GraphicsUnit.Pixel);
                }
            }

            return outBmp;
        }

        private static double EaseInOutCubic(double t)
        {
            if (t < 0.5)
                return 4 * t * t * t;

            var p = -2 * t + 2;
            return 1 - (p * p * p) / 2;
        }

        private static Rectangle LerpRect(Rectangle a, Rectangle b, double t)
        {
            var x = (int)Math.Round(a.X + (b.X - a.X) * t);
            var y = (int)Math.Round(a.Y + (b.Y - a.Y) * t);
            var w = (int)Math.Round(a.Width + (b.Width - a.Width) * t);
            var h = (int)Math.Round(a.Height + (b.Height - a.Height) * t);
            return new Rectangle(x, y, w, h);
        }

        private enum InsertSide
        {
            None = 0,
            Left = 1,
            Right = 2
        }

        private readonly struct InsertIndicator
        {
            public static readonly InsertIndicator Hidden = new(
                0, 0, 0, Rectangle.Empty, InsertSide.None);

            public InsertIndicator(int x, int top, int bottom, Rectangle targetBounds, InsertSide side)
            {
                X = x;
                Top = top;
                Bottom = bottom;
                TargetBounds = targetBounds;
                Side = side;
            }

            public int X { get; }
            public int Top { get; }
            public int Bottom { get; }
            public Rectangle TargetBounds { get; }
            public InsertSide Side { get; }

            public bool IsVisible => Side != InsertSide.None && Bottom > Top && TargetBounds.Height > 0;
        }

        private sealed class DottedPlaceholderControl : Control
        {
            private readonly Color back;

            public DottedPlaceholderControl(Color back)
            {
                this.back = back;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
                TabStop = false;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var b = new SolidBrush(back);
                e.Graphics.FillRectangle(b, ClientRectangle);

                var r = Rectangle.Inflate(ClientRectangle, -2, -2);
                using var pen = new Pen(Color.FromArgb(140, 70, 70, 70), 2f) { DashStyle = DashStyle.Dot };
                e.Graphics.DrawRectangle(pen, r);
            }
        }
        private sealed class InsertAdornerForm : Form
        {
            private const int WM_NCHITTEST = 0x0084;
            private const int HTTRANSPARENT = -1;

            private const int OverlayPadPx = 14;

            private readonly FlowLayoutPanel panel;
            private InsertIndicator indicator = InsertIndicator.Hidden;

            public InsertAdornerForm(FlowLayoutPanel panel)
            {
                this.panel = panel;

                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                TopMost = true;

                BackColor = Color.Magenta;
                TransparencyKey = Color.Magenta;

                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
                UpdateStyles();
            }

            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= 0x00000020;
                    cp.ExStyle |= 0x08000000;
                    cp.ExStyle |= 0x00000080;
                    return cp;
                }
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = (IntPtr)HTTRANSPARENT;
                    return;
                }

                base.WndProc(ref m);
            }

            public void ShowNoActivate()
            {
                SyncToPanelBounds();
                if (!Visible)
                    Show();
            }

            public void SyncToPanelBounds()
            {
                if (panel.IsDisposed || !panel.IsHandleCreated)
                    return;

                var r = panel.RectangleToScreen(panel.ClientRectangle);
                r.Inflate(OverlayPadPx, OverlayPadPx);

                if (Bounds != r)
                {
                    Bounds = r;

                    _ = NativeMethods.SetWindowPos(
                        Handle,
                        NativeMethods.HWND_TOPMOST,
                        r.Left,
                        r.Top,
                        r.Width,
                        r.Height,
                        NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW | NativeMethods.SWP_NOSENDCHANGING);
                }
            }

            public void ShowIndicator(InsertIndicator i)
            {
                if (indicator.Equals(i) && Visible)
                    return;

                indicator = i;
                if (!Visible)
                    ShowNoActivate();

                Invalidate();
            }

            public void HideIndicator()
            {
                if (indicator.Equals(InsertIndicator.Hidden))
                    return;

                indicator = InsertIndicator.Hidden;
                Invalidate();
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                e.Graphics.Clear(BackColor);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (!indicator.IsVisible)
                    return;

                const int OutlineWhitePx = 2;
                const int OutlineBlackPx = 2;

                const int LineFillWidthPx = 6;
                const float LineHeightOfTarget = 0.56f;
                const int LineHalfHeightMinPx = 14;
                const int LineHalfHeightMaxPx = 90;

                const float ArrowSizeOfTargetHeight = 0.76f;
                const int ArrowSizeMinPx = 34;
                const int ArrowSizeMaxPx = 96;

                const int AssemblyPadInsideTargetPx = 6;

                const int GradTopDelta = 20;
                const int GradMidDelta = 12;
                const int GradBotDelta = 24;
                const float GradMidPos = 0.60f;

                const float SheenHeightOfShape = 0.40f;
                const float SheenStartStrength = 0.38f;
                const float SheenMidStrength = 0.20f;
                const float SheenEndStrength = 0.05f;

                const int InnerTopEdgeDelta = 14;
                const int InnerBottomEdgeDelta = 18;

                var fill = Color.FromArgb(215, 228, 242);

                e.Graphics.SmoothingMode = SmoothingMode.None;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.CompositingMode = CompositingMode.SourceCopy;
                e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;

                var ox = OverlayPadPx;
                var oy = OverlayPadPx;

                var tb = indicator.TargetBounds;
                if (tb.Width <= 0 || tb.Height <= 0)
                    return;

                tb.Offset(ox, oy);

                var safePad = OutlineWhitePx + OutlineBlackPx + 3;

                var cxLine = Clamp(indicator.X + ox, safePad, Width - safePad - 1);

                var midY = tb.Top + (tb.Height / 2);

                var lineHalf = Clamp((int)Math.Round(tb.Height * (LineHeightOfTarget * 0.5f)), LineHalfHeightMinPx, LineHalfHeightMaxPx);
                var lineTop = Clamp(midY - lineHalf, safePad, Height - safePad - 1);
                var lineBot = Clamp(midY + lineHalf, safePad, Height - safePad - 1);

                DrawOutlinedLine(e.Graphics, cxLine, lineTop, lineBot, LineFillWidthPx, fill);

                var arrowSize = Clamp((int)Math.Round(tb.Height * ArrowSizeOfTargetHeight), ArrowSizeMinPx, ArrowSizeMaxPx);
                arrowSize = Math.Min(arrowSize, Math.Max(12, tb.Height - (AssemblyPadInsideTargetPx * 2)));
                if ((arrowSize & 1) == 1) arrowSize--;

                var cxTarget = tb.Left + (tb.Width / 2);
                var arrowRect = new Rectangle(cxTarget - (arrowSize / 2), midY - (arrowSize / 2), arrowSize, arrowSize);
                arrowRect = ClampRectToForm(arrowRect, Width, Height, safePad);

                Point[] tri;
                if (indicator.Side == InsertSide.Left)
                {
                    tri = new[]
                    {
                        new Point(arrowRect.Left,  arrowRect.Top + (arrowRect.Height / 2)),
                        new Point(arrowRect.Right, arrowRect.Top),
                        new Point(arrowRect.Right, arrowRect.Bottom)
                    };
                }
                else
                {
                    tri = new[]
                    {
                        new Point(arrowRect.Right, arrowRect.Top + (arrowRect.Height / 2)),
                        new Point(arrowRect.Left,  arrowRect.Top),
                        new Point(arrowRect.Left,  arrowRect.Bottom)
                    };
                }

                var triBounds = GetBounds(tri);
                triBounds = ClampRectToForm(triBounds, Width, Height, safePad);

                using (var gpArrow = new GraphicsPath())
                {
                    gpArrow.AddPolygon(tri);

                    FillEmbossedPath(
                        e.Graphics,
                        gpArrow,
                        triBounds,
                        fill,
                        GradTopDelta,
                        GradMidDelta,
                        GradBotDelta,
                        GradMidPos,
                        SheenHeightOfShape,
                        SheenStartStrength,
                        SheenMidStrength,
                        SheenEndStrength,
                        InnerTopEdgeDelta,
                        InnerBottomEdgeDelta);

                    StrokeWhiteOuterBlackInner(e.Graphics, gpArrow, OutlineWhitePx, OutlineBlackPx);
                }
            }

            private static void StrokeWhiteOuterBlackInner(Graphics g, GraphicsPath gp, int whitePx, int blackPx)
            {
                var w = Math.Max(1, whitePx);
                var b = Math.Max(1, blackPx);

                using var inside = new Region(gp);

                using (var gpW = (GraphicsPath)gp.Clone())
                using (var penW = new Pen(Color.White, w * 2f) { LineJoin = LineJoin.Miter, StartCap = LineCap.Flat, EndCap = LineCap.Flat })
                {
                    gpW.Widen(penW);
                    using var outer = new Region(gpW);
                    outer.Exclude(inside);
                    using var brW = new SolidBrush(Color.White);
                    g.FillRegion(brW, outer);
                }

                using (var gpB = (GraphicsPath)gp.Clone())
                using (var penB = new Pen(Color.Black, b * 2f) { LineJoin = LineJoin.Miter, StartCap = LineCap.Flat, EndCap = LineCap.Flat })
                {
                    gpB.Widen(penB);
                    using var inner = new Region(gpB);
                    inner.Intersect(inside);
                    using var brB = new SolidBrush(Color.Black);
                    g.FillRegion(brB, inner);
                }
            }
            private static void DrawOutlinedLine(Graphics g, int x, int top, int bottom, int fillWidth, Color fill)
            {
                const int OutlineWhitePx = 2;
                const int OutlineBlackPx = 2;

                if (bottom < top)
                    return;

                var h = (bottom - top) + 1;

                var wFill = Math.Max(1, fillWidth);
                var totalW = wFill + ((OutlineWhitePx + OutlineBlackPx) * 2);

                if ((totalW & 1) == 1)
                    totalW++;

                var left = x - (totalW / 2);

                var outer = new Rectangle(left, top, totalW, h);
                var mid = Rectangle.Inflate(outer, -OutlineWhitePx, -OutlineWhitePx);
                var inner = Rectangle.Inflate(mid, -OutlineBlackPx, -OutlineBlackPx);

                using (var brW = new SolidBrush(Color.White))
                    g.FillRectangle(brW, outer);

                if (mid.Width > 0 && mid.Height > 0)
                {
                    using var brB = new SolidBrush(Color.Black);
                    g.FillRectangle(brB, mid);
                }

                if (inner.Width <= 0 || inner.Height <= 0)
                    return;

                var focusY = top + (h / 2);

                FillEmbossedRect(g, inner, fill, focusY);

                using (var pTop = new Pen(WithAlpha(Color.White, 110), 1f))
                    g.DrawLine(pTop, inner.Left, inner.Top, inner.Right - 1, inner.Top);

                using (var pBot = new Pen(WithAlpha(Color.Black, 120), 1f))
                    g.DrawLine(pBot, inner.Left, inner.Bottom - 1, inner.Right - 1, inner.Bottom - 1);

                using (var pL = new Pen(WithAlpha(Color.Black, 70), 1f))
                    g.DrawLine(pL, inner.Left, inner.Top, inner.Left, inner.Bottom - 1);

                using (var pR = new Pen(WithAlpha(Color.Black, 55), 1f))
                    g.DrawLine(pR, inner.Right - 1, inner.Top, inner.Right - 1, inner.Bottom - 1);
            }

            private static void FillEmbossedRect(Graphics g, Rectangle r, Color baseColor, int focusY)
            {
                var oldClip = g.Clip;
                var oldComp = g.CompositingMode;

                try
                {
                    using var clip = new Region(r);
                    g.Clip = clip;

                    var tFocus = (focusY - r.Top) / (float)Math.Max(1, r.Height);
                    if (tFocus < 0f) tFocus = 0f;
                    if (tFocus > 1f) tFocus = 1f;

                    var p1 = Clamp01(tFocus - 0.20f);
                    var p2 = Clamp01(tFocus - 0.06f);
                    var p3 = Clamp01(tFocus + 0.06f);
                    var p4 = Clamp01(tFocus + 0.22f);

                    var c0 = Adjust(baseColor, -26);
                    var c1 = Adjust(baseColor, +10);
                    var c2 = BlendToWhite(baseColor, 0.20f);
                    var c3 = BlendToWhite(baseColor, 0.14f);
                    var c4 = Adjust(baseColor, +6);
                    var c5 = Adjust(baseColor, -28);

                    using (var br = new LinearGradientBrush(r, c0, c5, LinearGradientMode.Vertical))
                    {
                        var cb = new ColorBlend(6)
                        {
                            Colors = new[] { c0, c1, c2, c3, c4, c5 },
                            Positions = new[] { 0f, p1, p2, p3, p4, 1f }
                        };
                        br.InterpolationColors = cb;
                        g.FillRectangle(br, r);
                    }

                    g.CompositingMode = CompositingMode.SourceOver;

                    var sheenH = Clamp((int)Math.Round(r.Height * 0.32f), 10, 86);
                    var sheenTop = Clamp(focusY - (sheenH / 2), r.Top, Math.Max(r.Top, r.Bottom - sheenH));
                    var sheenRect = new Rectangle(r.X, sheenTop, r.Width, sheenH);

                    using (var sheen = new LinearGradientBrush(sheenRect, Color.White, Color.White, LinearGradientMode.Vertical))
                    {
                        var cb = new ColorBlend(3)
                        {
                            Colors = new[]
                            {
                                WithAlpha(Color.White, 120),
                                WithAlpha(Color.White, 65),
                                WithAlpha(Color.White, 0)
                            },
                            Positions = new[] { 0f, 0.55f, 1f }
                        };
                        sheen.InterpolationColors = cb;
                        g.FillRectangle(sheen, sheenRect);
                    }

                    var shadowH = Clamp((int)Math.Round(r.Height * 0.24f), 8, 64);
                    var shadowTop = Clamp(focusY + (shadowH / 2), r.Top, Math.Max(r.Top, r.Bottom - shadowH));
                    var shadowRect = new Rectangle(r.X, shadowTop, r.Width, shadowH);

                    using (var sh = new LinearGradientBrush(shadowRect, Color.Black, Color.Black, LinearGradientMode.Vertical))
                    {
                        var cb = new ColorBlend(3)
                        {
                            Colors = new[]
                            {
                                WithAlpha(Color.Black, 0),
                                WithAlpha(Color.Black, 38),
                                WithAlpha(Color.Black, 0)
                            },
                            Positions = new[] { 0f, 0.55f, 1f }
                        };
                        sh.InterpolationColors = cb;
                        g.FillRectangle(sh, shadowRect);
                    }
                }
                finally
                {
                    g.CompositingMode = oldComp;
                    g.Clip = oldClip;
                }
            }

            private static void FillEmbossedPath(
                Graphics g,
                GraphicsPath gp,
                Rectangle bounds,
                Color baseColor,
                int topDelta,
                int midDelta,
                int botDelta,
                float midPos,
                float sheenHeightOfShape,
                float sheenStartStrength,
                float sheenMidStrength,
                float sheenEndStrength,
                int innerTopEdgeDelta,
                int innerBottomEdgeDelta)
            {
                var c0 = Adjust(baseColor, +topDelta + 8);
                var c1 = Adjust(baseColor, +midDelta + 6);
                var c2 = Adjust(baseColor, +2);
                var c3 = Adjust(baseColor, -(botDelta / 2));
                var c4 = Adjust(baseColor, -botDelta - 8);

                var pMid = Clamp01(midPos);
                var p1 = Math.Min(pMid - 0.18f, 0.44f);
                if (p1 < 0.10f) p1 = 0.10f;

                using (var br = new LinearGradientBrush(bounds, c0, c4, LinearGradientMode.Vertical))
                {
                    var cb = new ColorBlend(5)
                    {
                        Colors = new[] { c0, c1, c2, c3, c4 },
                        Positions = new[] { 0f, p1, pMid, 0.82f, 1f }
                    };
                    br.InterpolationColors = cb;
                    g.FillPath(br, gp);
                }

                var oldClip = g.Clip;
                var oldComp = g.CompositingMode;

                try
                {
                    g.SetClip(gp);

                    g.CompositingMode = CompositingMode.SourceOver;

                    var sheenH = Math.Max(3, (int)Math.Round(bounds.Height * sheenHeightOfShape));
                    var sheenRect = new Rectangle(bounds.X + 1, bounds.Y + 1, Math.Max(1, bounds.Width - 2), Math.Max(1, sheenH));

                    var a0 = (int)Math.Round(Clamp01(sheenStartStrength) * 255f);
                    var aM = (int)Math.Round(Clamp01(sheenMidStrength) * 255f);
                    var a1 = (int)Math.Round(Clamp01(sheenEndStrength) * 255f);

                    using (var sheen = new LinearGradientBrush(sheenRect, Color.White, Color.White, LinearGradientMode.Vertical))
                    {
                        var cb = new ColorBlend(4)
                        {
                            Colors = new[]
                            {
                                WithAlpha(Color.White, a0),
                                WithAlpha(Color.White, (int)Math.Round(a0 * 0.72f)),
                                WithAlpha(Color.White, aM),
                                WithAlpha(Color.White, a1)
                            },
                            Positions = new[] { 0f, 0.28f, 0.62f, 1f }
                        };
                        sheen.InterpolationColors = cb;
                        g.FillRectangle(sheen, sheenRect);
                    }

                    var hotH = Clamp((int)Math.Round(bounds.Height * 0.14f), 3, 22);
                    var hotRect = new Rectangle(bounds.X + 1, bounds.Y + 1, Math.Max(1, bounds.Width - 2), hotH);

                    using (var hot = new LinearGradientBrush(hotRect, Color.White, Color.White, LinearGradientMode.Vertical))
                    {
                        var cb = new ColorBlend(3)
                        {
                            Colors = new[]
                            {
                                WithAlpha(Color.White, Clamp((int)Math.Round(a0 * 1.05f), 0, 255)),
                                WithAlpha(Color.White, Clamp((int)Math.Round(a0 * 0.35f), 0, 255)),
                                WithAlpha(Color.White, 0)
                            },
                            Positions = new[] { 0f, 0.55f, 1f }
                        };
                        hot.InterpolationColors = cb;
                        g.FillRectangle(hot, hotRect);
                    }

                    using (var pTop = new Pen(WithAlpha(Color.White, 85), 1f))
                        g.DrawLine(pTop, bounds.Left + 1, bounds.Top + 1, bounds.Right - 2, bounds.Top + 1);

                    using (var pBot = new Pen(WithAlpha(Color.Black, 90), 1f))
                        g.DrawLine(pBot, bounds.Left + 1, bounds.Bottom - 2, bounds.Right - 2, bounds.Bottom - 2);

                    using (var edgeTop = new Pen(WithAlpha(Adjust(baseColor, +innerTopEdgeDelta), 140), 1f))
                        g.DrawLine(edgeTop, bounds.Left + 1, bounds.Top + 2, bounds.Right - 2, bounds.Top + 2);

                    using (var edgeBot = new Pen(WithAlpha(Adjust(baseColor, -innerBottomEdgeDelta), 140), 1f))
                        g.DrawLine(edgeBot, bounds.Left + 1, bounds.Bottom - 3, bounds.Right - 2, bounds.Bottom - 3);
                }
                finally
                {
                    g.CompositingMode = oldComp;
                    g.Clip = oldClip;
                }
            }

            private static Color WithAlpha(Color c, int a)
            {
                if (a < 0) a = 0;
                if (a > 255) a = 255;
                return Color.FromArgb(a, c.R, c.G, c.B);
            }

            private static float Clamp01(float v)
            {
                if (v < 0f) return 0f;
                if (v > 1f) return 1f;
                return v;
            }

            private static Rectangle GetBounds(Point[] pts)
            {
                var minX = pts[0].X;
                var maxX = pts[0].X;
                var minY = pts[0].Y;
                var maxY = pts[0].Y;

                for (int i = 1; i < pts.Length; i++)
                {
                    var p = pts[i];
                    if (p.X < minX) minX = p.X;
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y < minY) minY = p.Y;
                    if (p.Y > maxY) maxY = p.Y;
                }

                return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            }

            private static Rectangle ClampRectToForm(Rectangle r, int w, int h, int pad)
            {
                var x = r.X;
                var y = r.Y;
                var rw = r.Width;
                var rh = r.Height;

                if (rw < 1) rw = 1;
                if (rh < 1) rh = 1;

                var minX = pad;
                var minY = pad;
                var maxX = Math.Max(pad, w - pad - rw);
                var maxY = Math.Max(pad, h - pad - rh);

                if (x < minX) x = minX;
                if (y < minY) y = minY;
                if (x > maxX) x = maxX;
                if (y > maxY) y = maxY;

                return new Rectangle(x, y, rw, rh);
            }

            private static int Clamp(int v, int min, int max)
            {
                if (v < min) return min;
                if (v > max) return max;
                return v;
            }

            private static Color Adjust(Color c, int delta)
            {
                static int C(int v) => v < 0 ? 0 : (v > 255 ? 255 : v);
                return Color.FromArgb(c.A, C(c.R + delta), C(c.G + delta), C(c.B + delta));
            }

            private static Color BlendToWhite(Color c, float t)
            {
                if (t < 0f) t = 0f;
                if (t > 1f) t = 1f;

                var r = (int)Math.Round(c.R + (255 - c.R) * t);
                var g = (int)Math.Round(c.G + (255 - c.G) * t);
                var b = (int)Math.Round(c.B + (255 - c.B) * t);

                if (r < 0) r = 0; if (r > 255) r = 255;
                if (g < 0) g = 0; if (g > 255) g = 255;
                if (b < 0) b = 0; if (b > 255) b = 255;

                return Color.FromArgb(255, r, g, b);
            }
        }

        private sealed class ReorderOverlayForm : Form
        {
            private readonly FlowLayoutPanel panel;
            private readonly Bitmap[] bitmaps;
            private readonly Rectangle[] rects;

            public ReorderOverlayForm(FlowLayoutPanel panel, Bitmap[] bitmaps, Rectangle[] rects)
            {
                this.panel = panel;
                this.bitmaps = bitmaps;
                this.rects = rects;

                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;

                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
                UpdateStyles();

                var r = panel.RectangleToScreen(panel.ClientRectangle);
                Bounds = r;

                _ = NativeMethods.SetWindowPos(
                    Handle,
                    NativeMethods.HWND_TOPMOST,
                    r.Left,
                    r.Top,
                    r.Width,
                    r.Height,
                    NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
            }

            public void ShowNoActivate()
            {
                if (!Visible)
                    Show();
            }

            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= 0x08000000;
                    cp.ExStyle |= 0x00000008;
                    cp.ExStyle |= 0x00000080;
                    return cp;
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                using var b = new SolidBrush(panel.BackColor);
                e.Graphics.FillRectangle(b, ClientRectangle);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                for (int i = 0; i < bitmaps.Length; i++)
                    e.Graphics.DrawImage(bitmaps[i], rects[i]);
            }
        }

        private static class NativeMethods
        {
            public static readonly IntPtr HWND_TOPMOST = new(-1);

            public const uint SWP_NOACTIVATE = 0x0010;
            public const uint SWP_SHOWWINDOW = 0x0040;
            public const uint SWP_NOSENDCHANGING = 0x0400;

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool SetWindowPos(
                IntPtr hWnd,
                IntPtr hWndInsertAfter,
                int X,
                int Y,
                int cx,
                int cy,
                uint uFlags);
        }
    }
}
