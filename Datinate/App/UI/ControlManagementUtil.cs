using System.Runtime.InteropServices;

namespace datinate.app.ui
{
    public static class ControlManagementUtil
    {
        public static bool RedrawSuppressionEnabled { get; set; } = true;

        public static bool WebView2ReparentFixEnabled { get; set; } = true;

        private const int WM_SETREDRAW = 0x000B;

        private const uint RDW_INVALIDATE = 0x0001;
        private const uint RDW_ERASE = 0x0004;
        private const uint RDW_ALLCHILDREN = 0x0080;
        private const uint RDW_UPDATENOW = 0x0100;
        private const uint RDW_FRAME = 0x0400;

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        private static readonly object _redrawSync = new();
        private static readonly Dictionary<IntPtr, int> _redrawCounts = new();

        public static IDisposable SuspendRedraw(params Control[] controls)
        {
            if (!RedrawSuppressionEnabled || controls == null || controls.Length == 0)
                return NoopDisposable.Instance;

            return new RedrawScope(controls);
        }
        public static void ForceRedraw(Control control)
        {
            if (control == null || control.IsDisposed)
                return;

            Form? f = control as Form ?? control.FindForm();
            if (f != null && !f.IsDisposed && f.IsHandleCreated)
            {
                _ = RedrawWindow(
                    f.Handle,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    RDW_INVALIDATE | RDW_ERASE | RDW_FRAME | RDW_ALLCHILDREN | RDW_UPDATENOW);
                return;
            }

            if (!control.IsHandleCreated)
                return;

            _ = RedrawWindow(
                control.Handle,
                IntPtr.Zero,
                IntPtr.Zero,
                RDW_INVALIDATE | RDW_ERASE | RDW_FRAME | RDW_ALLCHILDREN | RDW_UPDATENOW);
        }
        private sealed class RedrawScope : IDisposable
        {
            private readonly List<IntPtr> _handles = new();
            private bool _disposed;

            public RedrawScope(Control[] controls)
            {
                var set = new HashSet<IntPtr>();

                for (int i = 0; i < controls.Length; i++)
                {
                    var c = controls[i];
                    if (c == null || c.IsDisposed || !c.IsHandleCreated)
                        continue;

                    var h = c.Handle;
                    if (h != IntPtr.Zero)
                        set.Add(h);

                    var f = c.FindForm();
                    if (f != null && !f.IsDisposed && f.IsHandleCreated)
                    {
                        var hf = f.Handle;
                        if (hf != IntPtr.Zero)
                            set.Add(hf);
                    }
                }

                lock (_redrawSync)
                {
                    foreach (var h in set)
                    {
                        if (!_redrawCounts.TryGetValue(h, out var n))
                            n = 0;

                        if (n == 0)
                            SendMessage(h, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

                        _redrawCounts[h] = n + 1;
                        _handles.Add(h);
                    }
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                lock (_redrawSync)
                {
                    for (int i = 0; i < _handles.Count; i++)
                    {
                        var h = _handles[i];
                        if (!_redrawCounts.TryGetValue(h, out var n))
                            continue;

                        n--;
                        if (n <= 0)
                        {
                            _redrawCounts.Remove(h);

                            SendMessage(h, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                            RedrawWindow(
                                h,
                                IntPtr.Zero,
                                IntPtr.Zero,
                                RDW_INVALIDATE | RDW_ERASE | RDW_FRAME | RDW_ALLCHILDREN | RDW_UPDATENOW);
                        }
                        else
                        {
                            _redrawCounts[h] = n;
                        }
                    }
                }
            }
        }

        private sealed class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new();
            public void Dispose() { }
        }
        public static void EnsureParent(Control child, Control Parent, bool priorityChild)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (Parent == null) throw new ArgumentNullException(nameof(Parent));
            if (ReferenceEquals(child, Parent))
                throw new ArgumentException("A control cannot be parented to itself.", nameof(Parent));

            var currentParent = child.Parent;
            var fromParent = currentParent;

            var needsWebFix = WebView2ReparentFixEnabled && ContainsWebView2(child);
            var wasVisible = child.Visible;

            void ApplyDockAndOrder()
            {
                if (child.IsDisposed || Parent.IsDisposed)
                    return;

                child.Dock = DockStyle.Fill;

                if (priorityChild)
                    child.BringToFront();
                else
                    child.SendToBack();
            }

            void QueueWebView2NudgeIfNeeded(bool didReparent)
            {
                if (!didReparent || !needsWebFix)
                    return;

                if (Parent.IsDisposed || child.IsDisposed)
                    return;

                if (!Parent.IsHandleCreated)
                    return;

                Parent.BeginInvoke(new Action(() =>
                {
                    if (Parent.IsDisposed || child.IsDisposed)
                        return;

                    if (!Parent.IsHandleCreated || !child.IsHandleCreated)
                        return;

                    ForceWebView2Nudge(child);
                    ForceRepaintNow(Parent);
                }));
            }

            if (ReferenceEquals(currentParent, Parent))
            {
                using var _ = SuspendRedraw(Parent);

                Parent.SuspendLayout();
                try
                {
                    ApplyDockAndOrder();
                }
                finally
                {
                    Parent.ResumeLayout(true);
                }

                ForceRepaintNow(Parent);
                return;
            }

            IDisposable redrawScope =
                fromParent != null && !ReferenceEquals(fromParent, Parent)
                    ? SuspendRedraw(fromParent, Parent)
                    : SuspendRedraw(Parent);

            using (redrawScope)
            {
                fromParent?.SuspendLayout();
                Parent.SuspendLayout();

                try
                {
                    if (needsWebFix)
                        child.Visible = false;

                    if (fromParent != null && !fromParent.IsDisposed && fromParent.Controls.Contains(child))
                        fromParent.Controls.Remove(child);

                    if (!Parent.Controls.Contains(child))
                        Parent.Controls.Add(child);

                    ApplyDockAndOrder();

                    if (needsWebFix)
                        child.Visible = wasVisible;
                }
                finally
                {
                    Parent.ResumeLayout(true);
                    fromParent?.ResumeLayout(true);
                }
            }

            ForceRepaintNow(Parent);
            if (fromParent != null && !ReferenceEquals(fromParent, Parent))
                ForceRepaintNow(fromParent);

            QueueWebView2NudgeIfNeeded(true);
        }
        public static void Swap(Control a, Control b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (ReferenceEquals(a, b)) return;

            var parentA = a.Parent ?? throw new InvalidOperationException("Control 'a' has no Parent.");
            var parentB = b.Parent ?? throw new InvalidOperationException("Control 'b' has no Parent.");

            var slotA = CaptureSlot(parentA, a);
            var slotB = CaptureSlot(parentB, b);

            var needsWebFix = WebView2ReparentFixEnabled && (ContainsWebView2(a) || ContainsWebView2(b));
            var aWasVisible = a.Visible;
            var bWasVisible = b.Visible;

            using var _ = SuspendRedraw(parentA, parentB, a, b);

            parentA.SuspendLayout();
            if (!ReferenceEquals(parentA, parentB)) parentB.SuspendLayout();

            try
            {
                if (needsWebFix)
                {
                    a.Visible = false;
                    b.Visible = false;
                }

                RemoveFromParent(slotA, a);
                RemoveFromParent(slotB, b);

                ForceRepaintNow(parentA);
                if (!ReferenceEquals(parentA, parentB))
                    ForceRepaintNow(parentB);

                PlaceIntoSlot(slotA, b);
                PlaceIntoSlot(slotB, a);

                b.Dock = DockStyle.Fill;
                a.Dock = DockStyle.Fill;

                FinaliseLayout(parentA);
                if (!ReferenceEquals(parentA, parentB))
                    FinaliseLayout(parentB);

                ForceRepaintNow(parentA);
                if (!ReferenceEquals(parentA, parentB))
                    ForceRepaintNow(parentB);

                if (needsWebFix)
                {
                    a.Visible = aWasVisible;
                    b.Visible = bWasVisible;

                    ForceWebView2Nudge(a);
                    ForceWebView2Nudge(b);

                    ForceRepaintNow(parentA);
                    if (!ReferenceEquals(parentA, parentB))
                        ForceRepaintNow(parentB);
                }
            }
            finally
            {
                parentA.ResumeLayout(true);
                if (!ReferenceEquals(parentA, parentB)) parentB.ResumeLayout(true);
            }
        }

        private static void RemoveFromParent(SwapSlot slot, Control child)
        {
            if (slot.TableLayoutParent != null)
            {
                slot.TableLayoutParent.Controls.Remove(child);
                return;
            }

            if (slot.Parent is SplitterPanel && slot.WasOnlyChild)
            {
                slot.Parent.Controls.Clear();
                return;
            }

            slot.Parent.Controls.Remove(child);
        }

        private static SwapSlot CaptureSlot(Control parent, Control child)
        {
            var slot = new SwapSlot
            {
                Parent = parent,
                ChildIndex = parent.Controls.Contains(child) ? parent.Controls.GetChildIndex(child, false) : 0,
                Dock = child.Dock,
                Anchor = child.Anchor,
                Margin = child.Margin,
                Bounds = child.Bounds,
                WasOnlyChild = parent is SplitterPanel && parent.Controls.Count == 1 && ReferenceEquals(parent.Controls[0], child)
            };

            if (parent is TableLayoutPanel tlp)
            {
                var pos = tlp.GetPositionFromControl(child);
                if (pos.Row < 0 || pos.Column < 0)
                    throw new InvalidOperationException("Control is not in a valid TableLayoutPanel cell.");

                slot.TableLayoutParent = tlp;
                slot.CellPosition = pos;
                slot.RowSpan = tlp.GetRowSpan(child);
                slot.ColumnSpan = tlp.GetColumnSpan(child);
            }

            return slot;
        }

        private static void PlaceIntoSlot(SwapSlot slot, Control child)
        {
            if (slot.TableLayoutParent != null)
            {
                var tlp = slot.TableLayoutParent;
                var pos = slot.CellPosition;

                tlp.Controls.Add(child, pos.Column, pos.Row);
                tlp.SetRowSpan(child, slot.RowSpan);
                tlp.SetColumnSpan(child, slot.ColumnSpan);
                return;
            }

            slot.Parent.Controls.Add(child);

            int max = slot.Parent.Controls.Count - 1;
            int idx = slot.ChildIndex;
            if (idx < 0) idx = 0;
            if (idx > max) idx = max;

            slot.Parent.Controls.SetChildIndex(child, idx);
        }

        private static void FinaliseLayout(Control parent)
        {
            parent.PerformLayout();
            parent.Invalidate(true);
        }

        private static void ForceRepaintNow(Control c)
        {
            if (c == null || c.IsDisposed || !c.IsHandleCreated)
                return;

            RedrawWindow(
                c.Handle,
                IntPtr.Zero,
                IntPtr.Zero,
                RDW_INVALIDATE | RDW_ERASE | RDW_FRAME | RDW_ALLCHILDREN | RDW_UPDATENOW);
        }

        private static bool ContainsWebView2(Control root)
        {
            if (root == null)
                return false;

            var stack = new Stack<Control>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var c = stack.Pop();
                if (c == null || c.IsDisposed)
                    continue;

                var tn = c.GetType().FullName;
                if (!string.IsNullOrEmpty(tn) && tn.IndexOf("WebView2", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                for (int i = 0; i < c.Controls.Count; i++)
                    stack.Push(c.Controls[i]);
            }

            return false;
        }

        private static void ForceWebView2Nudge(Control root)
        {
            if (root == null)
                return;

            var stack = new Stack<Control>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var c = stack.Pop();
                if (c == null || c.IsDisposed)
                    continue;

                var tn = c.GetType().FullName;
                var isWv2 = !string.IsNullOrEmpty(tn) && tn.IndexOf("WebView2", StringComparison.OrdinalIgnoreCase) >= 0;

                if (isWv2 && c.IsHandleCreated)
                {
                    var w = c.Width;
                    var h = c.Height;

                    if (w > 2 && h > 2)
                    {
                        SetWindowPos(c.Handle, IntPtr.Zero, 0, 0, w + 1, h, SWP_NOZORDER | SWP_NOACTIVATE);
                        SetWindowPos(c.Handle, IntPtr.Zero, 0, 0, w, h, SWP_NOZORDER | SWP_NOACTIVATE);
                    }

                    ForceRepaintNow(c);
                }

                for (int i = 0; i < c.Controls.Count; i++)
                    stack.Push(c.Controls[i]);
            }
        }

        private struct SwapSlot
        {
            public Control Parent;
            public int ChildIndex;

            public DockStyle Dock;
            public AnchorStyles Anchor;
            public Padding Margin;
            public Rectangle Bounds;

            public bool WasOnlyChild;

            public TableLayoutPanel? TableLayoutParent;
            public TableLayoutPanelCellPosition CellPosition;
            public int RowSpan;
            public int ColumnSpan;
        }
    }
}
