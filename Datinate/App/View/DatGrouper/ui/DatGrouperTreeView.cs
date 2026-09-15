using RadioLibCore.RadioDat;
using System.ComponentModel;
using System.Runtime.InteropServices;

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
        

        private const bool HotTrackingEnabled = true;

        private const int TVS_NOHSCROLL = 0x8000;
        private const int WM_NOTIFY = 0x004E;

        private const int TTN_FIRST = -520;
        private const int TTN_GETDISPINFOW = TTN_FIRST - 10;

        private const int TVM_GETTOOLTIPS = TVM_FIRST + 25;

        private const int TTM_SETDELAYTIME = 0x0400 + 3;
        private const int TTM_POP = 0x0400 + 28;

        private const int TTDT_AUTOPOP = 2;

        private readonly DatGrouperTreeViewScrollManager scrollManager;

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
            scrollManager =
                new DatGrouperTreeViewScrollManager(this);

            defaultBackColour = BackColor;

            if (IsNodeInteractableTag == null)
                IsNodeInteractableTag =
                    static tag => tag is not SpacerNodeTag;

            if (IsSelectableTag == null)
                IsSelectableTag =
                    static tag => tag is IGameEntity;

            Scrollable = true;
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

            _ = SendMessage(
                Handle,
                TVM_SETEXTENDEDSTYLE,
                (IntPtr)TVS_EX_DOUBLEBUFFER,
                (IntPtr)TVS_EX_DOUBLEBUFFER);

            // NOTE: Keep node tooltips visible longer while the cursor remains
            // over the node image. They can still be popped immediately when
            // the mouse leaves the image.
            SetNodeToolTipAutoPopDelay(15000);

            scrollManager.HandleHostHandleCreated();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            scrollManager.HandleHostHandleDestroyed();

            base.OnHandleDestroyed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                scrollManager.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (!IsHandleCreated)
                return;

            scrollManager.HandleHostStateChanged();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (!IsHandleCreated)
                return;

            scrollManager.HandleHostStateChanged();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (!IsHandleCreated)
                return;

            scrollManager.HandleHostStateChanged();
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

            scrollManager.HandleHostMouseEnter();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            UpdateHotNodeFromPoint(e.Location);
            scrollManager.HandleHostMouseMove();

            // NOTE: Node tooltips are only allowed while directly over the node image.
            if (!IsPointOverNodeImage(e.Location))
                PopNodeToolTip();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            SetHotNode(null);

            scrollManager.HandleHostMouseLeave();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (IsEmptyTreeOverlayLikelyActive())
                Invalidate();

            UpdateHotNodeFromCursor();
            scrollManager.HandleHostSizeChanged();
        }

        protected override void WndProc(ref Message m)
        {
            // NOTE: TreeView asks for tooltip text via TTN_GETDISPINFOW.
            // Do not allow it to obtain tooltip text unless the mouse is over a node image.
            if (m.Msg == WM_NOTIFY &&
                m.LParam != IntPtr.Zero &&
                Marshal.ReadInt32(
                    m.LParam,
                    IntPtr.Size * 2) == TTN_GETDISPINFOW)
            {
                var mousePoint =
                    PointToClient(Cursor.Position);

                if (!IsPointOverNodeImage(mousePoint))
                {
                    m.Result = IntPtr.Zero;
                    return;
                }
            }

            base.WndProc(ref m);
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

        private bool IsPointOverNodeImage(Point point)
        {
            if (!ClientRectangle.Contains(point))
                return false;

            var hit = HitTest(point);

            return hit.Node != null &&
                   (hit.Location & TreeViewHitTestLocations.Image) != 0;
        }

        private void PopNodeToolTip()
        {
            if (!IsHandleCreated)
                return;

            var toolTipHandle = SendMessage(
                Handle,
                TVM_GETTOOLTIPS,
                IntPtr.Zero,
                IntPtr.Zero);

            if (toolTipHandle != IntPtr.Zero)
            {
                _ = SendMessage(
                    toolTipHandle,
                    TTM_POP,
                    IntPtr.Zero,
                    IntPtr.Zero);
            }
        }
        private void SetNodeToolTipAutoPopDelay(int milliseconds)
        {
            if (!IsHandleCreated)
                return;

            var toolTipHandle = SendMessage(
                Handle,
                TVM_GETTOOLTIPS,
                IntPtr.Zero,
                IntPtr.Zero);

            if (toolTipHandle == IntPtr.Zero)
                return;

            _ = SendMessage(
                toolTipHandle,
                TTM_SETDELAYTIME,
                (IntPtr)TTDT_AUTOPOP,
                (IntPtr)milliseconds);
        }
        public sealed class SpacerNodeTag : IGameEntityProxy
        {
            public static readonly SpacerNodeTag Instance = new();
            private SpacerNodeTag() { }
        }
    }
}
