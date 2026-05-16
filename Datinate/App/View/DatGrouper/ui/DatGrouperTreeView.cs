using RadioLibCore.RadioDat;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Timer = System.Windows.Forms.Timer;

namespace datinate.app
{
    public interface IDatGrouperTree
    {
        IReadOnlySet<Type> DisabledTagTypes { get; }
        IReadOnlySet<IGameEntity> EnabledEntities { get; }
        TreeNode? FocusNode { get; }
        TreeNode? HotNode { get; }
        bool IsNodeDisabled(TreeNode? node);
        IReadOnlySet<TreeNode> HiddenNodes { get; }
        int ClientSizeWidth { get; }
        int ItemHeight { get; }
        TreeNode? SelectedNode { get; }
        Color BackColor { get; }
        Color ForeColor { get; }
        Font Font { get; }
    }

    public sealed class DatGrouperTreeView : TreeView, IDatGrouperTree
    {        
        public event Action? SelectionCleared;
        
        private Timer? hoverScrollPollTimer;

        private const bool HotTrackingEnabled = true;
        private const int HoverScrollPollIntervalMs = 200;
        private const int TVS_NOHSCROLL = 0x8000;
        private bool appliedVScrollVisible;
        private bool enforcingVScroll;



        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Predicate<object?>? IsNodeInteractableTag { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Predicate<object?>? IsSelectableTag { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ClearSelectionOnEmptySpaceClick { get; set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ClearSelectionOnNonSelectableClick { get; set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlySet<Type> DisabledTagTypes { get; private set; } = new HashSet<Type>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlySet<IGameEntity> EnabledEntities { get; private set; } = new HashSet<IGameEntity>();


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlySet<TreeNode> HiddenNodes { get; private set; } = new HashSet<TreeNode>();


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public TreeNode? HotNode { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AllNodesDisabled { get; private set; } = false;

        private const int TVM_FIRST = 0x1100;
        private const int TVM_SETEXTENDEDSTYLE = TVM_FIRST + 44;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        private TreeNode? focusNode;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public TreeNode? FocusNode => focusNode;

        public int ClientSizeWidth => ClientSize.Width;


        private Color defaultBackColour;

        public DatGrouperTreeView()
        {
            defaultBackColour = BackColor;

            if (IsNodeInteractableTag == null)
                IsNodeInteractableTag = static tag => tag is not SpacerNodeTag;

            if (IsSelectableTag == null)
                IsSelectableTag = static tag => tag is IGameEntity;

            Scrollable = true;
        }

        private void EnsureHoverScrollPollTimer()
        {
            if (hoverScrollPollTimer != null)
                return;

            hoverScrollPollTimer = new System.Windows.Forms.Timer
            {
                Interval = HoverScrollPollIntervalMs
            };

            hoverScrollPollTimer.Tick += HoverScrollPollTimer_Tick;
        }

        private void StartHoverScrollPoll()
        {
            if (IsDesignTime() || !IsHandleCreated)
                return;

            EnsureHoverScrollPollTimer();

            if (hoverScrollPollTimer != null && !hoverScrollPollTimer.Enabled)
                hoverScrollPollTimer.Start();
        }

        private void StopHoverScrollPoll()
        {
            if (hoverScrollPollTimer != null && hoverScrollPollTimer.Enabled)
                hoverScrollPollTimer.Stop();
        }
        
        private bool ShouldPoll()
        {
            if (IsDesignTime())
                return false;

            if (!IsHandleCreated)
                return false;

            if (Parent == null)
                return false;

            if (!Visible || !Enabled)
                return false;

            return true;
        }

        private void UpdatePollingStateAndApplyImmediate()
        {
            if (!ShouldPoll())
            {
                StopHoverScrollPoll();
                ApplyVScrollVisible(false, force: true);
                return;
            }

            StartHoverScrollPoll();
            PollAndApplyVScroll(force: true);
        }

        private void HoverScrollPollTimer_Tick(object? sender, EventArgs e)
        {
            if (!ShouldPoll())
            {
                StopHoverScrollPoll();
                ApplyVScrollVisible(false, force: true);
                return;
            }

            PollAndApplyVScroll(force: false);
        }

        private void PollAndApplyVScroll(bool force)
        {
            bool over = IsCursorActuallyOverThisTreeOrChild();
            bool canScroll = CanTreeActuallyScrollVertically();
            ApplyVScrollVisible(over && canScroll, force: force);
        }

        public void SetFocusNode(TreeNode? node)
        {
            if (ReferenceEquals(focusNode, node))
                return;
            Ui(() =>
            {
                BackColor = node != null
                ? Color.FromArgb(250, 250, 250)
                : defaultBackColour;

                var old = focusNode;
                focusNode = node;

                Invalidate();
                InvalidateFocusRegion(old);
                InvalidateFocusRegion(focusNode);
            });
        }
        private void Ui(Action action)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(action);
                return;
            }

            action();
        }

        private void InvalidateFocusRegion(TreeNode? node)
        {
            if (node == null)
                return;

            InvalidateNodeRow(node);

            var prev = GetPrevVisibleNonSpacer(node);
            if (prev != null)
                InvalidateNodeRow(prev);

            var next = GetNextVisibleNonSpacer(node);
            if (next != null)
                InvalidateNodeRow(next);
        }

        private static TreeNode? GetPrevVisibleNonSpacer(TreeNode node)
        {
            var n = node.PrevVisibleNode;
            while (n != null && n.Tag is SpacerNodeTag)
                n = n.PrevVisibleNode;

            return n;
        }

        private static TreeNode? GetNextVisibleNonSpacer(TreeNode node)
        {
            var n = node.NextVisibleNode;
            while (n != null && n.Tag is SpacerNodeTag)
                n = n.NextVisibleNode;

            return n;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            _ = SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);

            appliedVScrollVisible = false;
            ApplyVScrollVisible(false, force: true);

            UpdatePollingStateAndApplyImmediate();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (hoverScrollPollTimer != null)
            {
                hoverScrollPollTimer.Stop();
                hoverScrollPollTimer.Tick -= HoverScrollPollTimer_Tick;
                hoverScrollPollTimer.Dispose();
                hoverScrollPollTimer = null;
            }

            base.OnHandleDestroyed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (hoverScrollPollTimer != null)
                {
                    hoverScrollPollTimer.Stop();
                    hoverScrollPollTimer.Tick -= HoverScrollPollTimer_Tick;
                    hoverScrollPollTimer.Dispose();
                    hoverScrollPollTimer = null;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (!IsHandleCreated)
                return;

            UpdatePollingStateAndApplyImmediate();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (!IsHandleCreated)
                return;

            UpdatePollingStateAndApplyImmediate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (!IsHandleCreated)
                return;

            UpdatePollingStateAndApplyImmediate();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style |= TVS_NOHSCROLL;
                return cp;
            }
        }

        public TreeNode RootSpacerNode
        {
            get
            {
                EnsureRootSpacerNode();
                var root = Nodes[0];
                if (!root.IsExpanded) root.Expand();
                return root;
            }
        }

        public TreeNodeCollection ContentNodes => RootSpacerNode.Nodes;

        public void ResetToEmptyRoot()
        {
            Nodes.Clear();
            _ = RootSpacerNode;
            ClearSelection();

            SetHotNode(null);
            SetFocusNode(null);
        }

        public void AppendContentNodeWithSpacer(TreeNode node)
        {
            _ = ContentNodes.Add(node);
            AddSpacerNode(this, ContentNodes);
        }

        public TreeNode? RemoveContentNodeAndTrailingSpacer(TreeNode node)
        {
            if (node == null)
                return null;

            if (ReferenceEquals(HotNode, node))
                SetHotNode(null);

            var previousVisibleNode = node.PrevVisibleNode;

            bool isTopLevelContentNode =
                node.Parent != null &&
                ReferenceEquals(node.Parent, RootSpacerNode);

            TreeNodeCollection? siblings = node.Parent?.Nodes;

            if (siblings == null)
            {
                node.Remove();
                return previousVisibleNode;
            }

            int removeIndex = node.Index;
            node.Remove();

            if (isTopLevelContentNode &&
                removeIndex < siblings.Count &&
                siblings[removeIndex].Tag is SpacerNodeTag)
            {
                if (ReferenceEquals(HotNode, siblings[removeIndex]))
                    SetHotNode(null);

                siblings[removeIndex].Remove();
            }

            return previousVisibleNode;
        }
        public void ClearSelection()
        {
            ClearSelectionCore(true);
        }

        public void ClearSelectionKeepDisabled()
        {
            ClearSelectionCore(false);
        }

        private void ClearSelectionCore(bool clearDisabledState)
        {
            if (SelectedNode != null)
                SelectedNode = null;

            if (clearDisabledState)
            {
                DisabledTagTypes = new HashSet<Type>();
                EnabledEntities = new HashSet<IGameEntity>();
                HiddenNodes = new HashSet<TreeNode>();

                SetFocusNode(null);
            }

            SelectionCleared?.Invoke();
        }

        public void ClearHiddenNodes()
        {
            SetDisabledNodes(DisabledTagTypes, [], new HashSet<TreeNode>());
            Invalidate();
        }
        public void SetDisabledNodes(IEnumerable<Type> types, IEnumerable<IGameEntity>? enabledEntities, HashSet<TreeNode>? hiddenNodes)
        {
            var disabledTagTypes = types.ToHashSet();
            var enabledEntitySet = enabledEntities?.ToHashSet() ?? new HashSet<IGameEntity>();

            AllNodesDisabled =
                disabledTagTypes.Contains(typeof(IGameEntity)) &&
                enabledEntitySet.Count == 0;

            DisabledTagTypes = disabledTagTypes;
            EnabledEntities = enabledEntitySet;
            HiddenNodes = hiddenNodes ?? new HashSet<TreeNode>();

            Invalidate();
        }
        
        public void ClearDisabledNodes()
        {
            AllNodesDisabled = false;
            DisabledTagTypes = new HashSet<Type>();
            EnabledEntities = new HashSet<IGameEntity>();
            HiddenNodes = new HashSet<TreeNode>();
            Invalidate();
        }

        public bool IsNodeDisabled(TreeNode? node)
        {
            if (node == null) return false;
            if (HiddenNodes.Contains(node))
                return true;
            else 
                return IsNodeTagDisabled(node.Tag);
        }
        public bool IsNodeSpacer(TreeNode node) => node.Tag is IGameEntityProxy;
        private bool IsNodeTagDisabled(object? tag)
        {
            if (tag == null) return false;

            if (DisabledTagTypes.Count == 0) return false;

            var tagType = tag.GetType();

            var disabledByType =
                DisabledTagTypes.Contains(tagType) ||
                DisabledTagTypes.Any(t => t.IsAssignableFrom(tagType));

            if (!disabledByType) return false;


            if (tag is IGameEntity entity)
                return !EnabledEntities.Contains(entity);

            return true;
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            if (e.Node != null && (!IsInteractable(e.Node.Tag) || !IsSelectable(e.Node.Tag) || IsNodeDisabled(e.Node)))
            {
                e.Cancel = true;

                if (SelectedNode != null)
                    ClearSelectionKeepDisabled();

                return;
            }

            base.OnBeforeSelect(e);
        }

        protected override void OnBeforeCollapse(TreeViewCancelEventArgs e)
        {
            if (e.Node != null && (!IsInteractable(e.Node.Tag) || IsNodeTagDisabled(e.Node.Tag)))
            {
                e.Cancel = true;
                return;
            }

            base.OnBeforeCollapse(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            var hit = HitTest(e.Location);
            var node = hit.Node;

            if (node == null)
            {
                if (ClearSelectionOnEmptySpaceClick)
                    ClearSelectionKeepDisabled();

                base.OnMouseDown(e);
                return;
            }

            if (IsNodeDisabled(node) || !IsSelectable(node.Tag))
            {
                if (ClearSelectionOnNonSelectableClick)
                    ClearSelectionKeepDisabled();

                return;
            }

            base.OnMouseDown(e);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            UpdateHotNodeFromPoint(e.Location);
            PollAndApplyVScroll(force: false);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            SetHotNode(null);
            ApplyVScrollVisible(false, force: true);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (IsEmptyTreeOverlayLikelyActive())
                Invalidate();

            UpdateHotNodeFromCursor();
            PollAndApplyVScroll(force: true);
        }

        protected override void WndProc(ref Message m)
        {
            bool ncSensitive =
                m.Msg == WM_NCCALCSIZE ||
                m.Msg == WM_NCPAINT ||
                m.Msg == WM_NCACTIVATE ||
                m.Msg == WM_WINDOWPOSCHANGED ||
                m.Msg == WM_WINDOWPOSCHANGING ||
                m.Msg == WM_STYLECHANGED ||
                m.Msg == WM_SIZE;

            bool scrollSensitive =
                m.Msg == WM_VSCROLL ||
                m.Msg == WM_MOUSEWHEEL ||
                m.Msg == WM_MOUSEHWHEEL;

            bool mouseSensitive =
                m.Msg == WM_MOUSEMOVE ||
                m.Msg == WM_MOUSELEAVE ||
                m.Msg == WM_NCMOUSEMOVE ||
                m.Msg == WM_NCMOUSELEAVE;

            if (!IsDesignTime() && IsHandleCreated && (ncSensitive || scrollSensitive))
            {
                if (!IsCursorActuallyOverThisTreeOrChild())
                    ApplyVScrollVisible(false, force: true);
            }

            base.WndProc(ref m);

            if (!IsDesignTime() && IsHandleCreated && (ncSensitive || scrollSensitive || mouseSensitive))
                PollAndApplyVScroll(force: true);
        }

        private bool IsEmptyTreeOverlayLikelyActive()
        {
            if (Nodes.Count == 1 &&
                Nodes[0].Tag is SpacerNodeTag &&
                Nodes[0].Nodes.Count == 0)
                return true;

            if (Nodes.Count == 1)
            {
                var root = Nodes[0];
                if (root.Nodes.Count == 1 &&
                    root.Nodes[0].Tag is SpacerNodeTag &&
                    root.Nodes[0].Nodes.Count == 0)
                    return true;
            }

            return false;
        }

        private static TreeNode AddSpacerNode(TreeView tv, TreeNodeCollection nodes)
        {
            TreeNode spacer = nodes.Add(string.Empty);
            spacer.Tag = SpacerNodeTag.Instance;
            spacer.ImageKey = string.Empty;
            spacer.SelectedImageKey = string.Empty;
            spacer.ImageIndex = -1;
            spacer.SelectedImageIndex = -1;
            spacer.StateImageIndex = -1;
            spacer.ForeColor = tv.BackColor;

            return spacer;
        }

        private void EnsureRootSpacerNode()
        {
            if (Nodes.Count == 0 || Nodes[0].Tag is not SpacerNodeTag)
            {
                Nodes.Clear();
                _ = AddSpacerNode(this, Nodes);
                SetHotNode(null);
            }
        }

        private bool IsInteractable(object? tag) =>
            IsNodeInteractableTag == null || IsNodeInteractableTag(tag);
        
        private bool IsSelectable(object? tag)
        {
            return IsSelectableTag == null || IsSelectableTag(tag);
        }

        private void UpdateHotNodeFromCursor()
        {
            if (!IsHandleCreated || !HotTrackingEnabled)
                return;

            var p = PointToClient(Cursor.Position);
            UpdateHotNodeFromPoint(p);
        }

        private void UpdateHotNodeFromPoint(Point p)
        {
            if (!HotTrackingEnabled)
            {
                if (HotNode != null)
                    SetHotNode(null);
                return;
            }

            if (MouseButtons != MouseButtons.None || !ClientRectangle.Contains(p))
            {
                if (HotNode != null)
                    SetHotNode(null);
                return;
            }

            var hit = HitTest(p);
            var node = hit.Node;

            if (node != null && node.Tag is SpacerNodeTag)
                node = null;

            SetHotNode(node);
        }

        public void SetHotNode(TreeNode? node)
        {
            if (ReferenceEquals(HotNode, node))
                return;

            var old = HotNode;
            HotNode = node;

            if (old != null)
                InvalidateNodeRow(old);

            if (HotNode != null)
                InvalidateNodeRow(HotNode);
        }

        private void InvalidateNodeRow(TreeNode node)
        {
            if (!IsHandleCreated)
                return;

            if (node.TreeView != this)
                return;

            if (!node.IsVisible)
                return;

            var b = node.Bounds;
            if (b.Height <= 0)
                return;

            Invalidate(new Rectangle(0, b.Top, ClientSize.Width, b.Height));
        }

        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;
        private const int WM_VSCROLL = 0x0115;
        private const int WM_HSCROLL = 0x0114;

        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_MOUSELEAVE = 0x02A3;
        private const int WM_NCMOUSEMOVE = 0x00A0;
        private const int WM_NCMOUSELEAVE = 0x02A2;

        private const int WM_NCCALCSIZE = 0x0083;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_NCACTIVATE = 0x0086;
        private const int WM_SIZE = 0x0005;
        private const int WM_WINDOWPOSCHANGED = 0x0047;
        private const int WM_WINDOWPOSCHANGING = 0x0046;
        private const int WM_STYLECHANGED = 0x007D;

        private const int SB_VERT = 1;

        private const uint SIF_RANGE = 0x0001;
        private const uint SIF_PAGE = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SCROLLINFO
        {
            public uint cbSize;
            public uint fMask;
            public int nMin;
            public int nMax;
            public uint nPage;
            public int nPos;
            public int nTrackPos;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetCapture();

        [DllImport("user32.dll")]
        private static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT pt);

        [DllImport("user32.dll")]
        private static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

        [DllImport("user32.dll")]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        [DllImport("user32.dll")]
        private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        private const uint RDW_INVALIDATE = 0x0001;
        private const uint RDW_UPDATENOW = 0x0100;
        private const uint RDW_FRAME = 0x0400;

        private bool IsDesignTime()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site?.DesignMode ?? false);
        }

        private bool IsCursorActuallyOverThisTreeOrChild()
        {
            if (!IsHandleCreated)
                return false;

            var cap = GetCapture();
            if (cap != IntPtr.Zero)
                return cap == Handle || IsChild(Handle, cap);

            if (!GetCursorPos(out var p))
                return false;

            var hwnd = WindowFromPoint(p);
            if (hwnd == IntPtr.Zero)
                return false;

            return hwnd == Handle || IsChild(Handle, hwnd);
        }

        private bool CanTreeActuallyScrollVertically()
        {
            if (!IsHandleCreated)
                return false;

            if (!Scrollable)
                return false;

            var si = new SCROLLINFO
            {
                cbSize = (uint)Marshal.SizeOf<SCROLLINFO>(),
                fMask = SIF_RANGE | SIF_PAGE
            };

            if (!GetScrollInfo(Handle, SB_VERT, ref si))
                return false;

            long total = (long)si.nMax - si.nMin + 1;
            long page = (long)si.nPage;

            if (page <= 0)
                return false;

            return total > page;
        }

        private void ApplyVScrollVisible(bool visible, bool force)
        {
            if (!IsHandleCreated)
                return;

            if (enforcingVScroll)
                return;

            enforcingVScroll = true;
            try
            {
                if (!force && appliedVScrollVisible == visible)
                    return;

                appliedVScrollVisible = visible;

                _ = ShowScrollBar(Handle, SB_VERT, visible);
                _ = RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_FRAME | RDW_UPDATENOW);
            }
            finally
            {
                enforcingVScroll = false;
            }
        }

        public sealed class SpacerNodeTag : IGameEntityProxy
        {
            public static readonly SpacerNodeTag Instance = new();
            private SpacerNodeTag() { }
        }
    }
}
