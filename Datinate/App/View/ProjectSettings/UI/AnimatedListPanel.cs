using com.RADIO.Datinate.RMVC.Shared;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace datinate.app
{
    public partial class AnimatedListPanel : UserControl
    {
        public event Action? OrderChanged;
        public event Action? ReorderStarted;
        public event Action? ReorderFinished;


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RowGapPx { get; set; } = 6;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SidePaddingPx { get; set; } = 0;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int DragThresholdPx { get; set; } = 6;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EnableDragReorder { get; set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AutoWireMoveButtons { get; set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<Control, bool>? IsDragOriginAllowed { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsAnimating { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDragging => dragging;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsReordering => reorderActive;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<Control> OrderedItems => items;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Type AllowedItemBaseType { get; set; } = typeof(ContentPathRow);


        private readonly Timer timer;

        private readonly Dictionary<Control, int> targetTopByCtrl = new();
        private readonly HashSet<Control> dragMouseDownWiredRoots = new();
        private readonly Dictionary<Control, MoveWire> moveWireByItem = new();

        private readonly Dictionary<Type, MethodInfo?> setMoveEnabledByType = new();
        private readonly Dictionary<Type, MethodInfo?> cancelPressesByType = new();

        private List<Control> items = new();
        private HashSet<Control>? animateOnly;

        private bool dragging;
        private bool reorderActive;

        private Control? dragItem;
        private Point dragStartScreen;
        private int dragStartTop;
        private int dragGrabOffsetY;

        private bool resizeQueued;
        private bool internalResize;
        private int lastResizeClientWidth = -1;

        private readonly Dictionary<Type, MethodInfo?> setStripeByType = new();

        private bool previousAutoSize;
        private bool limitInteraction = false;

        public AnimatedListPanel()
        {
            InitializeComponent();

            previousAutoSize = AutoSize;

            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            timer = new Timer { Interval = 15 };
            timer.Tick += Timer_Tick;

            MouseMove += Panel_MouseMove;
            MouseUp += Panel_MouseUp;
        }
        public IReadOnlyList<ContentPathRow> GetRowControls()
        {
            List<ContentPathRow> list = new List<ContentPathRow>();

            var ordered = Controls
                .OfType<Control>()
                .OrderBy(c => c.Top)
                .ThenBy(c => c.Left)
                .ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                if (ordered[i] is ContentPathRow row)
                    list.Add(row);
            }
            return list;
        }
        public IReadOnlyList<DatGrouperProjectEntry> GetRows()
        {
            var ordered = Controls
                .OfType<Control>()
                .OrderBy(c => c.Top)
                .ThenBy(c => c.Left)
                .ToList();

            var list = new List<DatGrouperProjectEntry>();

            for (int i = 0; i < ordered.Count; i++)
            {
                if (ordered[i] is ContentPathRow row)
                    list.Add(row.GetRow());
            }

            return list;
        }

        public void SetRows(IReadOnlyList<DatGrouperProjectEntry> dtos, bool limitInteraction)
        {
            this.limitInteraction = limitInteraction;

            AutoSize = previousAutoSize;

            var rows = new List<Control>();

            for (int i = 0; i < dtos.Count; i++)
            {
                var row = new ContentPathRow();
                row.SetRow(dtos[i], limitInteraction);
                rows.Add(row);
            }

            SetRows(rows, animate: false);

            AutoSize = false;

            Invalidate();
        }

        private void SetRows(IReadOnlyList<Control> newItems, bool animate, IReadOnlyCollection<Control>? animateOnlyControls = null)
        {
            items = newItems?.Where(c => c != null).ToList() ?? new List<Control>();

            animateOnly = (animate && animateOnlyControls != null && animateOnlyControls.Count > 0)
                ? new HashSet<Control>(animateOnlyControls)
                : null;

            var useRedrawScope = IsHandleCreated;
            RedrawScope redrawScope = default;

            if (useRedrawScope)
                redrawScope = new RedrawScope(this);

            SuspendLayout();
            try
            {
                SyncControlsToItems();
                UpdateStripeStates();
                ApplySizingRules();
                ComputeTargetsAndResize();

                if (!animate)
                {
                    ApplyTargetsImmediate();
                    StopAnim();
                    animateOnly = null;
                }
                else
                {
                    StartAnim();
                }

                if (AutoWireMoveButtons)
                    EnsureMoveWiredForAllItems();

                EnsureDragWiredForAllItems();

                UpdateMoveEnabledStates();
            }
            finally
            {
                ResumeLayout(true);

                if (useRedrawScope)
                    redrawScope.Dispose();
            }

            Invalidate(true);
        }
        private void HandleDisposing(bool disposing)
        {
            if (disposing)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;

                MouseMove -= Panel_MouseMove;
                MouseUp -= Panel_MouseUp;

                UnwireAll();
            }
        }

        private void RequestResizeLayout()
        {
            if (resizeQueued)
                return;

            resizeQueued = true;

            try
            {
                BeginInvoke(new Action(PerformResizeLayout));
            }
            catch
            {
                resizeQueued = false;
            }
        }

        private void PerformResizeLayout()
        {
            resizeQueued = false;

            if (IsDisposed || !IsHandleCreated)
                return;

            if (items.Count == 0)
                return;

            var w = ClientSize.Width;
            if (w <= 1)
                return;

            if (w == lastResizeClientWidth)
                return;

            lastResizeClientWidth = w;

            internalResize = true;
            try
            {
                using var _ = new RedrawScope(this);

                SuspendLayout();
                try
                {
                    ApplySizingRules();
                    ComputeTargetsAndResize();
                    UpdateStripeStates();
                    if (!IsAnimating && !IsDragging)
                        ApplyTargetsImmediate();
                }
                finally
                {
                    ResumeLayout(false);
                }
            }
            finally
            {
                internalResize = false;
            }

            Invalidate();
        }

        private readonly struct RedrawScope : IDisposable
        {
            private const int WM_SETREDRAW = 0x000B;
            private readonly Control c;

            public RedrawScope(Control c)
            {
                this.c = c;
                if (c.IsHandleCreated)
                    SendMessage(c.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            }

            public void Dispose()
            {
                if (!c.IsHandleCreated)
                    return;

                SendMessage(c.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                c.Invalidate(false);
            }


            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (internalResize)
                return;

            if (items.Count == 0)
                return;

            if (!IsHandleCreated || IsDisposed)
                return;

            RequestResizeLayout();
        }
        private void UpdateStripeStates()
        {
            if (items.Count == 0)
                return;

            List<Control> ordered;

            if (targetTopByCtrl.Count > 0)
            {
                ordered = Controls
                    .OfType<Control>()
                    .Where(c => targetTopByCtrl.ContainsKey(c))
                    .OrderBy(c => targetTopByCtrl[c])
                    .ThenBy(c => c.Left)
                    .ToList();
            }
            else
            {
                ordered = Controls
                    .OfType<Control>()
                    .OrderBy(c => c.Top)
                    .ThenBy(c => c.Left)
                    .ToList();
            }

            for (int i = 0; i < ordered.Count; i++)
            {
                if (ordered[i] is ContentPathRow row)
                    row.SetStripe((i & 1) == 1);
            }
        }

        private void SyncControlsToItems()
        {
            var keep = new HashSet<Control>(items);

            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                var c = Controls[i];
                if (!keep.Contains(c))
                {
                    UnwireDragRecursive(c);
                    UnwireMoveButtons(c);
                    Controls.RemoveAt(i);
                }
            }

            for (int i = 0; i < items.Count; i++)
            {
                var c = items[i];

                if (c.Parent != this)
                    Controls.Add(c);

                if (c.Dock != DockStyle.None)
                    c.Dock = DockStyle.None;

                c.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            }
        }

        private void ApplySizingRules()
        {
            int w = Math.Max(0, ClientSize.Width - (SidePaddingPx * 2));

            for (int i = 0; i < items.Count; i++)
            {
                var c = items[i];
                if (c.Left != SidePaddingPx || c.Width != w)
                    c.SetBounds(SidePaddingPx, c.Top, w, c.Height, BoundsSpecified.X | BoundsSpecified.Width);
            }
        }

        private void ComputeTargetsAndResize()
        {
            targetTopByCtrl.Clear();

            int y = 0;

            for (int i = 0; i < items.Count; i++)
            {
                var c = items[i];
                targetTopByCtrl[c] = y;
                y += c.Height + RowGapPx;
            }

            if (y > 0)
                y -= RowGapPx;

            if (Height != y)
                Height = y;
        }

        private void ApplyTargetsImmediate()
        {
            foreach (var kvp in targetTopByCtrl)
            {
                var c = kvp.Key;

                if (dragItem != null && ReferenceEquals(c, dragItem))
                    continue;

                int t = kvp.Value;
                if (c.Top != t)
                    c.Top = t;
            }
        }

        private void StartAnim()
        {
            if (!IsAnimating)
            {
                IsAnimating = true;
                timer.Start();
            }
        }

        private void StopAnim()
        {
            IsAnimating = false;
            timer.Stop();
        }

        private void BeginReorder()
        {
            if (reorderActive)
                return;

            reorderActive = true;
            ReorderStarted?.Invoke();
        }

        private void EndReorderIfIdle()
        {
            if (!reorderActive)
                return;

            if (IsDragging || IsAnimating)
                return;

            reorderActive = false;
            ReorderFinished?.Invoke();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            bool anyMoving = false;

            foreach (var kvp in targetTopByCtrl)
            {
                var c = kvp.Key;

                if (dragItem != null && ReferenceEquals(c, dragItem))
                    continue;

                int target = kvp.Value;

                if (animateOnly != null && !animateOnly.Contains(c))
                {
                    if (c.Top != target)
                        c.Top = target;
                    continue;
                }

                int cur = c.Top;
                int diff = target - cur;

                if (diff == 0)
                    continue;

                anyMoving = true;

                int ad = Math.Abs(diff);
                int step = (int)Math.Ceiling(ad * 0.32);
                if (step < 1) step = 1;

                int next = cur + Math.Sign(diff) * step;

                if ((diff > 0 && next > target) || (diff < 0 && next < target))
                    next = target;

                c.Top = next;
            }

            if (!anyMoving)
            {
                animateOnly = null;
                StopAnim();

                EndReorderIfIdle();
                UpdateMoveEnabledStates();
                UpdateStripeStates();
            }
        }

        private void EnsureDragWiredForAllItems()
        {
            for (int i = 0; i < items.Count; i++)
                WireDragRecursive(items[i]);
        }

        private void WireDragRecursive(Control root)
        {
            if (!dragMouseDownWiredRoots.Add(root))
                return;

            root.MouseDown += Item_MouseDown;

            for (int i = 0; i < root.Controls.Count; i++)
                WireDragRecursive(root.Controls[i]);
        }

        private void UnwireDragRecursive(Control root)
        {
            if (!dragMouseDownWiredRoots.Remove(root))
                return;

            root.MouseDown -= Item_MouseDown;

            for (int i = 0; i < root.Controls.Count; i++)
                UnwireDragRecursive(root.Controls[i]);
        }
        private void Item_MouseDown(object? sender, MouseEventArgs e)
        {
            if (limitInteraction) return;

            if (!EnableDragReorder) return;
            if (e.Button != MouseButtons.Left) return;
            if (IsAnimating) return;

            if (sender is not Control src) return;

            if (!DefaultIsDragOriginAllowed(src))
                return;

            if (IsDragOriginAllowed != null && !IsDragOriginAllowed(src))
                return;

            var item = FindItemAncestor(src);
            if (item == null) return;

            dragItem = item;
            dragStartScreen = src.PointToScreen(e.Location);
            dragStartTop = item.Top;
            dragging = false;

            Capture = true;
        }

        private void Panel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (limitInteraction) return;

            if (dragItem == null)
                return;

            if ((Control.MouseButtons & MouseButtons.Left) == 0)
                return;

            var curScreen = Control.MousePosition;

            if (!dragging)
            {
                int dx = Math.Abs(curScreen.X - dragStartScreen.X);
                int dy = Math.Abs(curScreen.Y - dragStartScreen.Y);

                if (dx < DragThresholdPx && dy < DragThresholdPx)
                    return;

                dragging = true;
                BeginReorder();

                Cursor = Cursors.SizeAll;

                TryInvokeCancelButtonPresses(dragItem);
                SetAllMoveEnabled(false, false, false, false);

                var startPanelPt = PointToClient(dragStartScreen);
                dragGrabOffsetY = startPanelPt.Y - dragStartTop;

                dragItem.BringToFront();
            }

            var pt = PointToClient(curScreen);

            int newTop = pt.Y - dragGrabOffsetY;
            int maxTop = Math.Max(0, Height - dragItem.Height);
            if (newTop < 0) newTop = 0;
            if (newTop > maxTop) newTop = maxTop;

            dragItem.Top = newTop;

            int probeMid = newTop + (dragItem.Height / 2);

            int idx = items.IndexOf(dragItem);
            if (idx < 0)
                return;

            int insert = ComputeInsertIndexStable(dragItem, probeMid);

            if (insert > items.Count - 1)
                insert = items.Count - 1;
            if (insert < 0)
                insert = 0;

            if (insert != idx)
            {
                items.RemoveAt(idx);
                if (insert > items.Count)
                    insert = items.Count;

                items.Insert(insert, dragItem);

                ComputeTargetsAndResize(); 
                UpdateStripeStates();

                animateOnly = new HashSet<Control>(items.Where(c => !ReferenceEquals(c, dragItem)));
                StartAnim();

                OrderChanged?.Invoke();
            }
        }

        private void Panel_MouseUp(object? sender, MouseEventArgs e)
        {
            if (limitInteraction) return;

            if (dragItem == null)
                return;

            Capture = false;
            Cursor = Cursors.Default;

            if (dragging)
            {
                dragging = false;

                var released = dragItem;
                dragItem = null;

                animateOnly = null;
                StartAnim();

                OrderChanged?.Invoke();
            }
            else
            {
                dragItem = null;
            }

            UpdateMoveEnabledStates();
            EndReorderIfIdle();
        }

        private int ComputeInsertIndexStable(Control dragged, int probeMidY)
        {
            int insert = 0;

            for (int i = 0; i < items.Count; i++)
            {
                var c = items[i];
                if (ReferenceEquals(c, dragged))
                    continue;

                if (!targetTopByCtrl.TryGetValue(c, out var t))
                    continue;

                int mid = t + (c.Height / 2);
                if (probeMidY <= mid)
                    return insert;

                insert++;
            }

            return insert;
        }

        private static bool DefaultIsDragOriginAllowed(Control src)
        {
            if (src is TextBoxBase) return false;
            if (src is ComboBox) return false;
            if (src is ButtonBase) return false;
            if (src is LinkLabel) return false;
            if (src is CheckBox) return false;
            if (src is RadioButton) return false;
            if (src is NumericUpDown) return false;
            if (src is DateTimePicker) return false;
            if (src.Cursor == Cursors.IBeam) return false;

            if (string.Equals(src.GetType().Name, "ArrowButton", StringComparison.Ordinal))
                return false;

            return true;
        }

        private Control? FindItemAncestor(Control src)
        {
            var c = src;

            while (c != null)
            {
                if (items.Contains(c))
                    return c;

                c = c.Parent;
            }

            return null;
        }

        private void EnsureMoveWiredForAllItems()
        {
            for (int i = 0; i < items.Count; i++)
                WireMoveButtons(items[i]);
        }

        private void WireMoveButtons(Control item)
        {
            if (moveWireByItem.ContainsKey(item))
                return;

            var t = item.GetType();

            var up = t.GetEvent("MoveUpRequested", BindingFlags.Instance | BindingFlags.Public);
            var down = t.GetEvent("MoveDownRequested", BindingFlags.Instance | BindingFlags.Public);
            var top = t.GetEvent("MoveTopRequested", BindingFlags.Instance | BindingFlags.Public);
            var bottom = t.GetEvent("MoveBottomRequested", BindingFlags.Instance | BindingFlags.Public);

            if (up == null && down == null && top == null && bottom == null)
                return;

            var wire = new MoveWire(
                up, up != null ? BuildOneArgDelegate(up, nameof(OnMoveUpRequested)) : null,
                down, down != null ? BuildOneArgDelegate(down, nameof(OnMoveDownRequested)) : null,
                top, top != null ? BuildOneArgDelegate(top, nameof(OnMoveTopRequested)) : null,
                bottom, bottom != null ? BuildOneArgDelegate(bottom, nameof(OnMoveBottomRequested)) : null);

            if (wire.UpEvent != null && wire.UpHandler != null) wire.UpEvent.AddEventHandler(item, wire.UpHandler);
            if (wire.DownEvent != null && wire.DownHandler != null) wire.DownEvent.AddEventHandler(item, wire.DownHandler);
            if (wire.TopEvent != null && wire.TopHandler != null) wire.TopEvent.AddEventHandler(item, wire.TopHandler);
            if (wire.BottomEvent != null && wire.BottomHandler != null) wire.BottomEvent.AddEventHandler(item, wire.BottomHandler);

            moveWireByItem[item] = wire;
        }

        private void UnwireMoveButtons(Control item)
        {
            if (!moveWireByItem.TryGetValue(item, out var wire))
                return;

            if (wire.UpEvent != null && wire.UpHandler != null) wire.UpEvent.RemoveEventHandler(item, wire.UpHandler);
            if (wire.DownEvent != null && wire.DownHandler != null) wire.DownEvent.RemoveEventHandler(item, wire.DownHandler);
            if (wire.TopEvent != null && wire.TopHandler != null) wire.TopEvent.RemoveEventHandler(item, wire.TopHandler);
            if (wire.BottomEvent != null && wire.BottomHandler != null) wire.BottomEvent.RemoveEventHandler(item, wire.BottomHandler);

            moveWireByItem.Remove(item);
        }

        private Delegate BuildOneArgDelegate(EventInfo ev, string handlerName)
        {
            var handlerMethod = GetType().GetMethod(handlerName, BindingFlags.Instance | BindingFlags.NonPublic);
            var invoke = ev.EventHandlerType?.GetMethod("Invoke");
            var ps = invoke?.GetParameters();

            if (handlerMethod == null || ps == null || ps.Length != 1 || ev.EventHandlerType == null)
                throw new InvalidOperationException();

            var p0 = Expression.Parameter(ps[0].ParameterType, "x");
            var body = Expression.Call(Expression.Constant(this), handlerMethod, Expression.Convert(p0, typeof(object)));
            return Expression.Lambda(ev.EventHandlerType, body, p0).Compile();
        }

        private void OnMoveUpRequested(object arg) => HandleMoveRequest(arg, -1, edge: 0, useEdge: false);
        private void OnMoveDownRequested(object arg) => HandleMoveRequest(arg, +1, edge: 0, useEdge: false);
        private void OnMoveTopRequested(object arg) => HandleMoveRequest(arg, 0, edge: 0, useEdge: true);
        private void OnMoveBottomRequested(object arg) => HandleMoveRequest(arg, 0, edge: -1, useEdge: true);

        private void HandleMoveRequest(object arg, int delta, int edge, bool useEdge)
        {
            if (limitInteraction) return;

            if (arg is not Control item)
                return;

            if (!items.Contains(item))
                return;

            if (IsReordering)
                return;

            BeginReorder();

            int idx = items.IndexOf(item);
            if (idx < 0)
            {
                EndReorderIfIdle();
                return;
            }

            int target;

            if (useEdge)
                target = edge == 0 ? 0 : items.Count - 1;
            else
                target = idx + delta;

            if (target < 0) target = 0;
            if (target > items.Count - 1) target = items.Count - 1;

            if (target == idx)
            {
                EndReorderIfIdle();
                return;
            }

            SetAllMoveEnabled(false, false, false, false);

            items.RemoveAt(idx);
            items.Insert(target, item);

            ComputeTargetsAndResize();
            UpdateStripeStates();

            var affected = new HashSet<Control>();
            int a = Math.Min(idx, target);
            int b = Math.Max(idx, target);

            for (int i = a; i <= b; i++)
                affected.Add(items[i]);

            animateOnly = affected;
            StartAnim();

            OrderChanged?.Invoke();
        }

        private void SetAllMoveEnabled(bool up, bool down, bool top, bool bottom)
        {
            if (limitInteraction) return;

            for (int i = 0; i < items.Count; i++)
                TryInvokeSetMoveEnabled(items[i], up, down, top, bottom);
        }
        private void UpdateMoveEnabledStates()
        {
            if (IsAnimating || IsDragging)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                bool canUp = i > 0;
                bool canDown = i < items.Count - 1;
                bool canTop = i > 0;
                bool canBottom = i < items.Count - 1;

                TryInvokeSetMoveEnabled(items[i], canUp, canDown, canTop, canBottom);
            }
        }

        private void TryInvokeSetMoveEnabled(Control item, bool up, bool down, bool top, bool bottom)
        {
            var t = item.GetType();

            if (!setMoveEnabledByType.TryGetValue(t, out var mi))
            {
                mi = t.GetMethod("SetMoveEnabled", BindingFlags.Instance | BindingFlags.Public, new[] { typeof(bool), typeof(bool), typeof(bool), typeof(bool) });
                setMoveEnabledByType[t] = mi;
            }

            mi?.Invoke(item, new object[] { up, down, top, bottom });
        }

        private void TryInvokeCancelButtonPresses(Control item)
        {
            var t = item.GetType();

            if (!cancelPressesByType.TryGetValue(t, out var mi))
            {
                mi = t.GetMethod("CancelButtonPresses", BindingFlags.Instance | BindingFlags.Public, Type.EmptyTypes);
                cancelPressesByType[t] = mi;
            }

            mi?.Invoke(item, Array.Empty<object>());
        }

        private void UnwireAll()
        {
            foreach (var kvp in moveWireByItem.ToArray())
                UnwireMoveButtons(kvp.Key);

            foreach (var root in dragMouseDownWiredRoots.ToArray())
                UnwireDragRecursive(root);

            moveWireByItem.Clear();
            dragMouseDownWiredRoots.Clear();
        }

        private readonly record struct MoveWire(
            EventInfo? UpEvent, Delegate? UpHandler,
            EventInfo? DownEvent, Delegate? DownHandler,
            EventInfo? TopEvent, Delegate? TopHandler,
            EventInfo? BottomEvent, Delegate? BottomHandler);
    }
}
