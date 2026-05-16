using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using Datinate.App.View.projects.gameFamily;
using RadioLibCore.RadioDat;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using static app.datinate.DatGrouperEditDelta;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;
using static datinate.app.DatGrouperTreeView;

namespace datinate.app
{
    public partial class DatGrouperUiBase : UserControl
    {
        public static IDatGrouperNodePreview? NodePreview;
        public event Action? InitialisedEvt;

        public event Action<IGameEntity?>? SelectedNodeChangedEvt;

        public event Action<string>? SearchGameNameEvt;
        public event Action<DatGrouperEntryDTO>? ShowGameMediaEvt;
        public event Action<IGameEntity>? ShowGroupingReportEvt;

        public event Action<DatGrouperEntryDTO>? GameEntityDragStartEvt;
        public event Action? GameEntityDragStopEvt;

        public event Action<DatGrouperUiBase, string?>? SurrogateToggleEvt;
        public event Action<bool>? ToggleRenderAliasesEvt;
        public Action? ClearDragTargetDisabledNodesEvt;
        public Action<IReadOnlySet<Type>>? SetDragTargetDisabledNodesEvt;

        public event Action<ISet<string>>? DatChipsRefreshEvt;

        public Action<DAT_GROUPER_ACTION_ENUM, IGameEntity?>? ActionEvt;

        public event Action<DatGrouperEditRequestDTO>? EditRequestEvt;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AllowInteraction { get; internal set; } = true;

        public bool IsSurrogateUI => isSurrogate;

        protected void InvokeClearDragTargetDisabledNodesEvt()
            => ClearDragTargetDisabledNodesEvt?.Invoke();
        protected void InvokeSearchGameNameEvt(string name)
            => SearchGameNameEvt?.Invoke(name);
        protected void InvokeShowGameMediaEvt(DatGrouperEntryDTO entry)
            => ShowGameMediaEvt?.Invoke(entry);
        protected void InvokeGameEntityDragStartEvt(DatGrouperEntryDTO entry) 
            => GameEntityDragStartEvt?.Invoke(entry);
        protected void InvokeEditRequestEvt(DatGrouperEditRequestDTO editRequest)
            => EditRequestEvt?.Invoke(editRequest);
        protected void InvokeGameEntityDragStopEvt()
            => GameEntityDragStopEvt?.Invoke();

        protected enum DAT_GROUPER_UI_ENUM
        {
            NOT_SET,
            Automated,
            Curated
        }

        protected enum DRAG_INSERT_MODE
        {
            None,
            Before,
            After
        }

        protected virtual void HandleTreeViewItemDrag(DatGrouperDragData dragData, TreeNode node)
            { } 
        protected virtual DragDropEffects HandleTreeViewDragEnter(DatGrouperDragData dragData)
            => DragDropEffects.None;
        protected virtual DragDropEffects HandleTreeViewDragOver(DatGrouperDragData dragData, Point point)
            => DragDropEffects.None;
        protected virtual DragDropEffects HandleTreeViewDragDrop(DatGrouperDragData dragData, Point point)
            => DragDropEffects.None;

        protected DAT_GROUPER_MEDIA_MODE mediaModeEnum = DAT_GROUPER_MEDIA_MODE.NOT_SET;

        private TreeNodeCollection Nodes => treeView.ContentNodes;
        private bool suppressFamilyKeysInitialisedEvt;
        private bool renderAliases = true;

        private readonly Font rootNodeFont;
        private readonly Font PartNodeFont;
        private readonly Font familyNodeFont;

        private readonly Dictionary<IGameFamily, TreeNode> nodesDictionary = new();
        private readonly List<IGameFamily> familyOrder = new List<IGameFamily>();
        private IReadOnlySet<IGamePart> allCuratedAutoParts = new HashSet<IGamePart>();

        private IReadOnlyDictionary<IGameFamily, IMediaCollection>? mediaCache;

        private DatGrouperTreeView? oldTreeView = null;
        
        private bool isDraggedOver = false;
        private bool isBeingDragged = false;

        private bool isSurrogate = false;
        private bool isAutoUI = true;

        private Dictionary<string, CurationPartReport>? partFingerprintReportDic;
        private bool suppressOverlayEvents = false;

        private readonly DatGrouperContextMenu contextMenu;

        public DatGrouperUiBase()
        {
            InitializeComponent();

            treeView.ShowNodeToolTips = false;
            treeView.HideSelection = false;

            PartNodeFont = new Font(treeView.Font.FontFamily, 10f, FontStyle.Regular);
            familyNodeFont = new Font(treeView.Font.FontFamily, 10f, FontStyle.Bold);
            rootNodeFont = new Font(treeView.Font.FontFamily, 14f, FontStyle.Bold);

            searchUI.SearchStateChangedEvt += OnSearchStateChanged;
            searchUI.JumpToItemEvt += OnJumpToSearchItem;

            treeView.IsNodeInteractableTag = tag => tag is not SpacerNodeTag;
            treeView.IsSelectableTag = tag => tag is IGameEntity;
            treeView.ClearSelectionOnEmptySpaceClick = true;
            treeView.ClearSelectionOnNonSelectableClick = true;
            treeView.SelectionCleared += treeView_SelectionCleared;

            contextMenu = new DatGrouperContextMenu(this as CuratedUI is { });
            contextMenu.UndoEvt += OnUndo;
            contextMenu.RedoEvt += OnRedo;
            contextMenu.PartIncludeExcludeEvt += OnPartIncludeExclude;
            contextMenu.GameMoveTopOrBottomEvt += OnGameMoveTopOrBottom;
            contextMenu.ToggleExcludedFamiliesShowHideEvt += OnToggleExcludedFamiliesShowHide;
            contextMenu.ToggleAliasesShowHideEvt += OnToggleAliasesShowHide;
            contextMenu.CopyNameEvt += OnCopyNameEvt;
        }
        public void SetView(
            IGameFamily[] gameFamilies,
            string projectName,
            Dictionary<string, CurationPartReport> partFingerprintReportDic,
            bool isSurrogate,
            bool isAutoUI)
        {
            var stopwatch = Stopwatch.StartNew();

            if (InvokeRequired)
            {
                if (IsDisposed)
                    return;

                if (DatinatePerformanceUtil.TREEVIEW_AwaitHandleCreation == true)
                {
#pragma warning disable CS0162 // Unreachable code detected
                    if (!IsHandleCreated)
                    {
                        EventHandler? h = null;
                        h = (_, __) =>
                        {
                            HandleCreated -= h;
                            SetView(
                                gameFamilies,
                                projectName,
                                partFingerprintReportDic,
                                isSurrogate,
                                isAutoUI);
                        };
                        HandleCreated += h;
                        return;
                    }
#pragma warning restore CS0162 // Unreachable code detected
                }

                BeginInvoke(new Action(() => SetView(
                    gameFamilies,
                    projectName,
                    partFingerprintReportDic,
                    isSurrogate,
                    isAutoUI)));

                return;
            }

            if (isAutoUI && !isSurrogate)
                _ = new DatGrouperPerformanceUtil(
                    this,
                    onEnd: r => InitialisedEvt?.Invoke(),
                    weakCallback: false,
                    runCallbackOnThreadPool: false);

            suppressOverlayEvents = true;

            try
            {
                // the order of these lines is vital:
                this.isSurrogate = isSurrogate;
                this.isAutoUI = isAutoUI;
                ResetView();

                this.partFingerprintReportDic = partFingerprintReportDic;

                familyOrder.AddRange(gameFamilies);

                SpawnFreshTreeView();

                if (DatinatePerformanceUtil.TREEVIEW_UseStockDraw == false)
                {
#pragma warning disable CS0162 // Unreachable code detected
                    treeView.DrawMode = TreeViewDrawMode.OwnerDrawText;
#pragma warning restore CS0162 // Unreachable code detected
                }
                else
                {
#pragma warning disable CS0162 // Unreachable code detected
                    treeView.DrawMode = TreeViewDrawMode.Normal;
#pragma warning restore CS0162 // Unreachable code detected
                }

                treeView.BeginUpdate();

                try
                {
                    int iconH = treeView.ImageList?.ImageSize.Height ?? 0;
                    int baseH = Math.Max(treeView.Font.Height, iconH);

                    treeView.ItemHeight = Math.Max(treeView.ItemHeight, Math.Max(baseH, DatGrouperIconUtil.NodeItemHeight));

                    BuildTreeView();
                    
                    treeView.ExpandAll();

                    //NOTE: Do NOT remove this line from this place for pain of death:
                    using var g = treeView.CreateGraphics();

                    // TODO:
                    treeView.DrawNode -= treeView_DrawNode;
                    treeView.DrawNode += treeView_DrawNode;
                }
                finally
                {
                    treeView.EndUpdate();
                }

                hideExcludedLabel.Text = !isSurrogate ? "✓" : "";
                oldTreeView?.Dispose();
                oldTreeView = null;

                OnFamiliesChanged();
            }
            finally
            {
                suppressOverlayEvents = false;
                if (isSurrogate)
                    Visible = false;
                else
                    Visible = true;

                contextMenu.Reset();
            }
        }
        public void SetMediaCache(IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache)
        {
            this.mediaCache = mediaCache;
            treeView.Invalidate();
        }

        internal void EnterMediaMode(bool isReadOnlyMode)
        {
            Ui(() =>
            {
                mediaModeEnum = !isReadOnlyMode 
                    ? DAT_GROUPER_MEDIA_MODE.MediaAssign
                    : DAT_GROUPER_MEDIA_MODE.MediaReadOnly;
                
                if (treeView.SelectedNode != null)
                {
                    treeView.SetFocusNode(treeView.SelectedNode);
                    ApplyTopNodeStable(treeView.SelectedNode);
                    if (isReadOnlyMode == false)
                    {
                        treeView.SetDisabledNodes(
                            [typeof(IGame), typeof(IGamePart)],
                            [], []);
                    }
                }
            });
        }
        internal void ExitMediaMode()
        {
            mediaModeEnum = DAT_GROUPER_MEDIA_MODE.NOT_SET;
            ClearSelections();
        }

        protected void ClearSelections()
        {
            treeView.ClearSelection();
        }

        public void SetRenderAliases(bool doRender)
        {
            renderAliases = doRender;
            treeView.Invalidate();
        }
        private void OnRedo() => ActionEvt?.Invoke(DAT_GROUPER_ACTION_ENUM.StateRedo, null);
        
        private void OnUndo() => ActionEvt?.Invoke(DAT_GROUPER_ACTION_ENUM.StateUndo, null);
        private void OnGameMoveTopOrBottom(bool moveToTop, IGame game)
        {
            var requestEnum = moveToTop
                    ? DatGrouperEditRequestDTO.EDIT_ACTION_ENUM.GameMoveToTop
                    : DatGrouperEditRequestDTO.EDIT_ACTION_ENUM.GameMoveToBottom;

            var dto = new DatGrouperEditRequestDTO(requestEnum)
            {
                SourceGame = game
            };

            EditRequestEvt?.Invoke(dto);
        }
        private void OnPartIncludeExclude(bool setToInclude, IGamePart part)
        {
            var requestEnum = setToInclude
                    ? DatGrouperEditRequestDTO.EDIT_ACTION_ENUM.PartSetAsInclude
                    : DatGrouperEditRequestDTO.EDIT_ACTION_ENUM.PartSetAsExclude;

            var dto = new DatGrouperEditRequestDTO(requestEnum)
            {
                SourcePart = part
            };

            EditRequestEvt?.Invoke(dto);
        }

        private void OnToggleAliasesShowHide()
        {
            ToggleRenderAliasesEvt?.Invoke(!renderAliases);
        }

        private void OnToggleExcludedFamiliesShowHide()
        {
            SurrogateToggleEvt?.Invoke(this, VisibleGameFamilyName);
        }

        private void OnCopyNameEvt(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Clipboard.SetText(gameName)));
                return;
            }

            Clipboard.SetText(gameName);
        }

        private void PulseSelectedNodeChanged()
        {
            var node = treeView.SelectedNode;

            SelectedNodeChangedEvt?.Invoke(node?.Tag as IGameEntity);

            if (node?.Tag is IGameEntity)
            {
                NodePreview?.SetPreviewSource(this, node);
                NodePreview?.DrawPreview();
            }
            else
            {
                NodePreview?.ClearPreview();
            }
        }
        internal void DrawPreviewNode(TreeNode node, Rectangle previewBounds, Graphics graphics)
        {
            if (node.TreeView != treeView)
                return;

            if (previewBounds.Width <= 0 || previewBounds.Height <= 0)
                return;

            int rowHeight = treeView.ItemHeight > 0
                ? treeView.ItemHeight
                : Math.Max(1, previewBounds.Height);

            int top = previewBounds.Top + Math.Max(0, (previewBounds.Height - rowHeight) / 2);

            var rowBounds = new Rectangle(
                previewBounds.Left,
                top,
                previewBounds.Width,
                rowHeight);

            int previewContentLeft = previewBounds.Left;

            if (treeView.ImageList is not null)
                previewContentLeft += treeView.ImageList.ImageSize.Width + 3;

            TreeRenderUtil.DrawTreeNode(
                treeView,
                node,
                rowBounds,
                graphics,
                renderAliases,
                null,
                searchUI.HighlightTextBackColour,
                false,
                new HashSet<Type>(),
                new HashSet<IGameEntity>(),
                null,
                new HashSet<TreeNode>(),
                GetNodeMediaCollection(node),
                suppressSelectionHighlight: true,
                rowWidthOverride: previewBounds.Width,
                contentLeftOverride: previewContentLeft);
        }
        private void BuildTreeView()
        {
            treeView.ResetToEmptyRoot();
            nodesDictionary.Clear();

            var uniquePointers = new HashSet<string>();

            for (int fi = 0; fi < familyOrder.Count; fi++)
            {
                var family = familyOrder[fi];

                var familyNode = BuildFamilyNode(
                    family,
                    fi,
                    partFingerprintReportDic,
                    uniquePointers,
                    out var hasGreenCandidate,
                    out var hasAmberCandidate);

                if (familyNode == null)
                    continue;

                var familyIconKey =
                    hasGreenCandidate ? DatGrouperIconUtil.Instance.GameFamilyGreenIcon :
                    hasAmberCandidate ? DatGrouperIconUtil.Instance.GameFamilyAmberIcon :
                                        DatGrouperIconUtil.Instance.GameFamilyRedIcon;

                familyNode.ImageKey = familyNode.SelectedImageKey = familyIconKey;

                var shouldAdd =
                    !isSurrogate ||
                    hasGreenCandidate ||
                    hasAmberCandidate;

                if (!shouldAdd)
                    continue;

                treeView.AppendContentNodeWithSpacer(familyNode);
                _ = nodesDictionary.TryAdd(family, familyNode);

                familyNode.ExpandAll();
            }

            if (isAutoUI && !isSurrogate && !suppressFamilyKeysInitialisedEvt)
            {
                DatChipsRefreshEvt?.Invoke(uniquePointers);
            }
        }

        protected static IReadOnlyCollection<TreeNode> CollectGameEntityNodesAndDescendants(TreeNode? node)
        {
            if (node == null) return [];

            if (node.Parent == null)
            {
                var tv = node?.TreeView;
                if (tv != null && tv.Nodes.Count == 1 && ReferenceEquals(tv.Nodes[0], node))
                    return [];
            }

            var result = new List<TreeNode>();

            var stack = new Stack<TreeNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var n = stack.Pop();

                if (n.Tag is IGameEntity entity && entity is not IGameEntityProxy)
                    result.Add(n);

                for (int i = n.Nodes.Count - 1; i >= 0; i--)
                    stack.Push(n.Nodes[i]);
            }

            return result;
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
        public void ApplyDatGrouperDelta(
            DELTA_NATURE_ENUM deltaNatureEnum,
            IReadOnlyList<IGameFamily> familiesToAdd,
            IReadOnlyList<IGameFamily> familiesToRemove,
            IReadOnlySet<IGamePart> allCuratedAutoParts,
            int undoCount,
            int redoCount, 
            IReadOnlySet<IGameEntity> affectedEntities)
        {
            Ui(() =>
            {
                contextMenu.SetUndoRedoEnabled(undoCount > 0, redoCount > 0);

                bool isImport = deltaNatureEnum == DELTA_NATURE_ENUM.Import;

                var selectedNode = treeView.SelectedNode;

                var addedFamilyNodes = new List<TreeNode>();
                var removedNodeProxies = new List<TreeNode>();

                IGameFamily? jumpToFamily = null;
                TreeNode? jumpToNode = null;

                this.allCuratedAutoParts = allCuratedAutoParts;

                treeView.BeginUpdate();

                try
                {
                    foreach (var remove in familiesToRemove)
                    {
                        if (!nodesDictionary.TryGetValue(remove, out var node))
                            continue;

                        _ = nodesDictionary.Remove(remove);
                        _ = familyOrder.Remove(remove);

                        var proxyNode = treeView.RemoveContentNodeAndTrailingSpacer(node);
                        
                        if (proxyNode != null)
                            removedNodeProxies.Add(proxyNode);
                    }

                    foreach (var add in familiesToAdd)
                    {
                        if (jumpToFamily is null)
                            jumpToFamily = add;

                        var newFamilyNode = InsertFamilyAndNode(add);

                        if (!isImport && newFamilyNode is not null)
                            addedFamilyNodes.Add(newFamilyNode);
                    }

                    if (jumpToFamily is not null && nodesDictionary.TryGetValue(jumpToFamily, out var jumpNode))
                        jumpToNode = jumpNode;
                }
                finally
                {
                    treeView.EndUpdate();
                }

                List<TreeNode> nodesToPulse = [];

                switch (deltaNatureEnum)
                {
                    case DELTA_NATURE_ENUM.Import:
                        nodesToPulse = [];
                        break;

                    case DELTA_NATURE_ENUM.UndoRedo:
                    case DELTA_NATURE_ENUM.Update:
                        nodesToPulse = GetMatchingNodes(addedFamilyNodes, affectedEntities).ToList();

                        if (addedFamilyNodes.Count == 0)
                            nodesToPulse.AddRange(removedNodeProxies);
                        break;

                    case DELTA_NATURE_ENUM.PartIncludeExclude:
                        nodesToPulse = GetMatchingNodes(nodesDictionary.Values.ToList(), affectedEntities).ToList();

                        HashSet<IGameFamily> newlyIncludedFamilies;
                        HashSet<IGameFamily> newlyExcludedFamilies;

                        UpdatePartExcludeStatus(
                            nodesToPulse,
                            out newlyIncludedFamilies,
                            out newlyExcludedFamilies);
                        break;
                }

                nodesToPulse = nodesToPulse.Distinct().ToList();
                nodesToPulse = RemoveRedundantSpacerPulses(nodesToPulse);

                TreeNode? ensureVisible = null;

                if (isImport)
                {
                    if (isAutoUI)
                    {
                        ensureVisible =
                            selectedNode != null && selectedNode.TreeView == treeView
                                ? selectedNode
                                : treeView.RootSpacerNode;
                    }
                    else
                    {
                        ensureVisible = treeView.RootSpacerNode;
                    }
                }
                else
                {
                    ensureVisible = addedFamilyNodes.FirstOrDefault();
                }

                if (ensureVisible != null)
                {
                    if (isImport)
                        ApplyTopNodeStable(ensureVisible);
                    else if (familiesToAdd.Count > 0)
                        JumpToNodeOnlyIfNeeded(ensureVisible);
                }

                foreach (var pulseNode in nodesToPulse)
                {
                    if (deltaNatureEnum != DELTA_NATURE_ENUM.UndoRedo)
                    {
                        if (pulseNode.Tag is SpacerNodeTag)
                            DatGrouperUiAnimationHelper.StartNodePulseAmbiguous(treeView, pulseNode);
                        else
                            DatGrouperUiAnimationHelper.StartNodePulse(treeView, pulseNode);
                    }
                    else
                        DatGrouperUiAnimationHelper.StartNodePulseAmbiguous(treeView, pulseNode);
                }

                OnFamiliesChanged();
                treeView.Invalidate();
            });
        }
        private List<TreeNode> RemoveRedundantSpacerPulses(IReadOnlyList<TreeNode> nodesToPulse)
        {
            if (nodesToPulse.Count <= 1)
                return nodesToPulse.ToList();

            var pulseSet = new HashSet<TreeNode>(nodesToPulse);
            var filtered = new List<TreeNode>(nodesToPulse.Count);

            for (int i = 0; i < nodesToPulse.Count; i++)
            {
                var node = nodesToPulse[i];

                if (!treeView.IsNodeSpacer(node))
                {
                    filtered.Add(node);
                    continue;
                }

                var prevFamily = FindAdjacentFamilyNode(node.PrevNode);
                var nextFamily = FindAdjacentFamilyNode(node.NextNode);

                var prevFamilyHasPulse = prevFamily != null && FamilyOrDescendantsContainPulse(prevFamily, pulseSet);
                var nextFamilyHasPulse = nextFamily != null && FamilyOrDescendantsContainPulse(nextFamily, pulseSet);

                if (prevFamilyHasPulse || nextFamilyHasPulse)
                    continue;

                filtered.Add(node);
            }

            return filtered;
        }

        private static TreeNode? FindAdjacentFamilyNode(TreeNode? node)
        {
            while (node != null)
            {
                if (node.Tag is IGameFamily)
                    return node;

                if (node.Tag is SpacerNodeTag)
                    return null;

                node = node.Parent;
            }

            return null;
        }

        private static bool FamilyOrDescendantsContainPulse(TreeNode familyNode, HashSet<TreeNode> pulseSet)
        {
            var stack = new Stack<TreeNode>();
            stack.Push(familyNode);

            while (stack.Count > 0)
            {
                var node = stack.Pop();

                if (pulseSet.Contains(node))
                    return true;

                for (int i = node.Nodes.Count - 1; i >= 0; i--)
                    stack.Push(node.Nodes[i]);
            }

            return false;
        }
        private void UpdatePartExcludeStatus(
            List<TreeNode> nodesToPulse, 
            out HashSet<IGameFamily> newlyIncludedFamilies,
            out HashSet<IGameFamily> newlyExcludedFamilies)
        {
            var touchedGames = new HashSet<TreeNode>();
            var touchedFamilies = new HashSet<TreeNode>();

            for (int i = 0; i < nodesToPulse.Count; i++)
            {
                var node = nodesToPulse[i];

                if (node.Tag is not IGamePart part)
                    continue;

                ApplyPartNodeVisual(node, part);

                if (node.Parent != null)
                {
                    _ = touchedGames.Add(node.Parent);

                    if (node.Parent.Parent != null)
                        _ = touchedFamilies.Add(node.Parent.Parent);
                }
            }

            newlyIncludedFamilies = new HashSet<IGameFamily>();
            newlyExcludedFamilies = new HashSet<IGameFamily>();

            foreach (var gameNode in touchedGames)
                RefreshGameNodeVisual(gameNode);

            foreach (var familyNode in touchedFamilies)
                RefreshFamilyNodeVisual(familyNode);
        }

        private static IReadOnlyList<TreeNode> GetMatchingNodes(
            IReadOnlyList<TreeNode> familyNodes,
            IReadOnlySet<IGameEntity> entities)
        {
            var list = new List<TreeNode>();

            void Visit(TreeNode node)
            {
                if (node.Tag is IGameEntity entity && entities.Contains(entity))
                    list.Add(node);

                // Go through all children and deeeper descendants of parent and add to
                // list if Tag is in entities.
                foreach (TreeNode child in node.Nodes)
                    Visit(child);
            }

            foreach (var parent in familyNodes)
                Visit(parent);

            return list;
        }
        
        /// <summary>
        /// Create node and add to tree. Update records on index position and location in tree.
        /// </summary>
        /// <param name="newFamily"></param>
        /// <param name="expandNow"></param>
        private TreeNode? InsertFamilyAndNode(IGameFamily newFamily, bool expandNow = true)
        {
            var newNode = BuildGameFamilyNode(
                newFamily,
                partFingerprintReportDic,
                out var hasGreenCandidate,
                out var hasAmberCandidate);

            newNode.ImageKey = newNode.SelectedImageKey =
                hasGreenCandidate ? DatGrouperIconUtil.Instance.GameFamilyGreenIcon :
                hasAmberCandidate ? DatGrouperIconUtil.Instance.GameFamilyAmberIcon :
                                    DatGrouperIconUtil.Instance.GameFamilyRedIcon;

            var shouldAdd =
                !isSurrogate ||
                hasGreenCandidate ||
                hasAmberCandidate;

            if (!shouldAdd)
                return null;

            treeView.AppendContentNodeWithSpacer(newNode);

            TreeNode? insertBefore = null;
            var newName = newNode.Text;

            for (int i = 0; i < Nodes.Count; i++)
            {
                var n = Nodes[i];
                if (ReferenceEquals(n, newNode)) continue;
                if (n.Tag is not IGameFamily) continue;

                if (StringComparer.OrdinalIgnoreCase.Compare(n.Text, newName) > 0)
                {
                    insertBefore = n;
                    break;
                }
            }

            if (insertBefore != null)
            {
                var trailingSpacer = newNode.NextNode;

                if (trailingSpacer != null) Nodes.Remove(trailingSpacer);

                Nodes.Remove(newNode);

                var insertAt = insertBefore.Index;
                Nodes.Insert(insertAt, newNode);

                if (trailingSpacer != null)
                    Nodes.Insert(insertAt + 1, trailingSpacer);
            }

            nodesDictionary[newFamily] = newNode;

            if (insertBefore == null)
                familyOrder.Add(newFamily);

            else
            {
                int familyOrdinal = 0;
                int listInsert = familyOrder.Count;

                for (int i = 0; i < Nodes.Count; i++)
                {
                    var n = Nodes[i];
                    if (ReferenceEquals(n, newNode))
                    {
                        listInsert = familyOrdinal;
                        break;
                    }

                    if (n.Tag is IGameFamily) familyOrdinal++;
                }

                listInsert = Math.Max(0, Math.Min(listInsert, familyOrder.Count));
                familyOrder.Insert(listInsert, newFamily);
            }

            if (expandNow)
                newNode.ExpandAll();

            return newNode;
        }

        private TreeNode? BuildFamilyNode(
            IGameFamily gameFamily,
            int familyIndex,
            Dictionary<string, CurationPartReport>? partFingerprintReportDic,
            HashSet<string> uniquePointerIds,
            out bool hasGreenCandidate,
            out bool hasAmberCandidate)
        {
            var gameFamilyNode = new TreeNode(gameFamily.GetFamilyDisplayName())
            {
                Tag = gameFamily,
                NodeFont = familyNodeFont
            };

            hasGreenCandidate = false;
            hasAmberCandidate = false;

            var games = gameFamily.GetAllGames();

            bool familyHasAnyAvailablePart = false;
            int addedGames = 0;

            for (int gi = 0; gi < games.Length; gi++)
            {
                var game = games[gi];

                var parts = game.GetGameParts(false);
                if (parts.Length == 0)
                    continue;

                var gameNode = gameFamilyNode.Nodes.Add(string.Empty);
                gameNode.Tag = game;
                gameNode.ForeColor = Color.DarkSlateGray;

                var partsForCounts = new List<IGamePart>(parts.Length);

                for (int pi = 0; pi < parts.Length; pi++)
                {
                    var gamePart = parts[pi];
                    partsForCounts.Add(gamePart);

                    familyHasAnyAvailablePart = true;

                    string? pointer = gamePart.GetDirectoryId();
                    if (!string.IsNullOrWhiteSpace(pointer))
                        _ = uniquePointerIds.Add(pointer);

                    var aliases = gamePart.GetSoftwareAliases();
                    foreach (var alias in aliases)
                    {
                        pointer = alias.GetDirectoryId();
                        if (!string.IsNullOrWhiteSpace(pointer))
                            _ = uniquePointerIds.Add(pointer);
                    }

                    var partNode = gameNode.Nodes.Add(DatinateHelper.GetGamePartNameRender(gamePart));
                    partNode.Tag = gamePart;

                    ApplyPartNodeVisual(partNode, gamePart);
                }

                gameNode.Text = GetGameNameRender(partsForCounts, game.GetNameWithoutExt());

                var gameStatus = EvaluateGameStatus(partsForCounts);

                if (gameStatus.Strict)
                    hasGreenCandidate = true;
                else if (gameStatus.Lenient)
                    hasAmberCandidate = true;

                gameNode.ImageKey = gameNode.SelectedImageKey =
                    GetGameIconKey(isParent: gi == 0, strict: gameStatus.Strict, lenient: gameStatus.Lenient);

                addedGames++;
            }

            if (addedGames == 0)
                return null;

            if (!familyHasAnyAvailablePart)
                return null;

            return gameFamilyNode;
        }
        private TreeNode BuildGameFamilyNode(
            IGameFamily gameFamily,
            Dictionary<string, CurationPartReport>? partFingerprintReportDic,
            out bool hasGreenCandidate,
            out bool hasAmberCandidate)
        {
            var gameFamilyNode = new TreeNode(gameFamily.GetFamilyDisplayName())
            {
                Tag = gameFamily,
                NodeFont = familyNodeFont
            };

            hasGreenCandidate = false;
            hasAmberCandidate = false;

            var games = gameFamily.GetAllGames();

            for (int i = 0; i < games.Length; i++)
            {
                var game = games[i];

                var gameNode = gameFamilyNode.Nodes.Add(
                    GetGameNameRender(game.GetGameParts(false), game.GetNameWithoutExt()));
                gameNode.Tag = game;
                gameNode.ForeColor = Color.DarkSlateGray;

                var parts = game.GetGameParts(false);

                for (int pi = 0; pi < parts.Length; pi++)
                {
                    var gamePart = parts[pi];

                    var partNode = gameNode.Nodes.Add(DatinateHelper.GetGamePartNameRender(gamePart));
                    partNode.Tag = gamePart;

                    ApplyPartNodeVisual(partNode, gamePart);
                }

                var gameStatus = EvaluateGameStatus(parts);

                if (gameStatus.Strict)
                    hasGreenCandidate = true;
                else if (gameStatus.Lenient)
                    hasAmberCandidate = true;

                gameNode.ImageKey = gameNode.SelectedImageKey =
                    GetGameIconKey(isParent: i == 0, strict: gameStatus.Strict, lenient: gameStatus.Lenient);
            }

            return gameFamilyNode;
        }
        private static string GetGameIconKey(bool isParent, bool strict, bool lenient)
        {
            if (strict)
                return isParent
                    ? DatGrouperIconUtil.Instance.GameParentGreenIcon
                    : DatGrouperIconUtil.Instance.GameChildGreenIcon;

            if (lenient)
                return isParent
                    ? DatGrouperIconUtil.Instance.GameParentAmberIcon
                    : DatGrouperIconUtil.Instance.GameChildAmberIcon;

            return isParent
                ? DatGrouperIconUtil.Instance.GameParentRedIcon
                : DatGrouperIconUtil.Instance.GameChildRedIcon;
        }

        private static string GetPartIconKey(bool IsShallowReference, DAT_GROUPER_MEMBERSHIP_ENUM membershipStatusEnum)
        {
            if (IsShallowReference)
            {
                return DatGrouperIconUtil.Instance.ShallowIconAlpha;
            }
            return membershipStatusEnum switch
            {
                DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_EXPLICIT => DatGrouperIconUtil.Instance.IncludeExtraIcon,
                DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_IMPLICIT => DatGrouperIconUtil.Instance.IncludeIcon,
                DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_RELATIVE => DatGrouperIconUtil.Instance.IncludedReluctantIcon,
                DAT_GROUPER_MEMBERSHIP_ENUM.EXCLUDE_RELATIVE => DatGrouperIconUtil.Instance.ExcludeIcon,
                DAT_GROUPER_MEMBERSHIP_ENUM.EXCLUDE_EXPLICIT => DatGrouperIconUtil.Instance.ExcludeExtraIcon,
                _ => DatGrouperIconUtil.Instance.ExcludeIcon
            };
        }

        public void ResetView()
        {
            if (InvokeRequired)
            {
                if (IsDisposed) return;
                BeginInvoke(new Action(() => ResetView()));
                return;
            }

            if (IsDisposed)
                return;

            renderAliases = true;

            allCuratedAutoParts = new HashSet<IGamePart>();

            var prevSuppress = suppressOverlayEvents;
            suppressOverlayEvents = true;
            
            isDraggedOver = false;
            isBeingDragged = false;

            mediaCache = null;
            mediaModeEnum = DAT_GROUPER_MEDIA_MODE.NOT_SET;

            try
            {
                treeView.ResetToEmptyRoot();

                searchUI.ClearUI();

                nodesDictionary.Clear();
                familyOrder.Clear();

                partFingerprintReportDic = null;
            }
            finally
            {
                suppressOverlayEvents = prevSuppress;
            }
        }
        private string GetGameNameRender(IReadOnlyList<IGamePart> partsArr, string? gameName)
        {
            var sb = new StringBuilder();

            int roms = 0;
            int aliases = 0;

            for (int i = 0; i < partsArr.Count; i++)
            {
                var p = partsArr[i];
                roms += p.GetChecksums().Length;
                aliases += p.GetSoftwareAliases().Length;
            }

            int parts = partsArr.Count;

            sb.Append("ROMs: " + roms);
            if (parts > 1) sb.Append(", Parts: " + parts);
            if (aliases > 0) sb.Append(", Aliases: " + aliases);

            var info = sb.ToString();

            var result = !string.IsNullOrWhiteSpace(gameName)
                ? gameName + " │ " + info
                : info;

            return result;
        }
        
        protected void HandleDisposing()
        {
            searchUI.SearchStateChangedEvt -= OnSearchStateChanged;
            searchUI.JumpToItemEvt -= OnJumpToSearchItem;

            treeView.AfterSelect -= treeView_AfterSelect;
            treeView.DrawNode -= treeView_DrawNode;
            treeView.NodeMouseClick -= treeView_NodeMouseClick;
            treeView.MouseUp -= treeView_MouseUp;
            treeView.SelectionCleared -= treeView_SelectionCleared;

            ResetView();

            rootNodeFont?.Dispose();
            familyNodeFont?.Dispose();
            PartNodeFont?.Dispose();
        }

        private void SpawnFreshTreeView()
        {
            var size = treeView.Size;
            var parent = treeView.Parent!;

            treeView.DrawNode -= treeView_DrawNode;

            treeView.SelectionCleared -= treeView_SelectionCleared;
            treeView.AfterSelect -= treeView_AfterSelect;
            treeView.NodeMouseClick -= treeView_NodeMouseClick;
            treeView.MouseUp -= treeView_MouseUp;


            if (DatinatePerformanceUtil.TREEVIEW_SkipFastSpawn == false)
            {
#pragma warning disable CS0162 // Unreachable code detected

            oldTreeView = treeView;
            parent.Controls.Remove(treeView);

            treeView = new DatGrouperTreeView();
            parent.Controls.Add(treeView);

#pragma warning restore CS0162 // Unreachable code detected
            }

            treeView.Location = new Point(-DatinateHelper.TeeViewHorizontalOffset, 0);
            treeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            treeView.BorderStyle = BorderStyle.None;
            treeView.Margin = new Padding(0, 0, 0, 0);
            treeView.Name = "treeView";
            treeView.ShowPlusMinus = false;
            treeView.Size = size;
            treeView.TabIndex = 0;
            treeView.ShowLines = false;

            treeView.ImageList = DatGrouperIconUtil.Instance.TreeImageList;
            treeView.ImageKey = treeView.SelectedImageKey = DatGrouperIconUtil.Instance.GameFamilyIcon;

            treeView.Indent = 0;
            treeView.ShowNodeToolTips = false;
            treeView.HideSelection = true;
            treeView.FullRowSelect = true; 

            treeView.IsNodeInteractableTag = tag => tag is not SpacerNodeTag;
            treeView.IsSelectableTag = tag => tag is IGameEntity;
            treeView.ClearSelectionOnEmptySpaceClick = true;
            treeView.ClearSelectionOnNonSelectableClick = true;

            treeView.SelectionCleared += treeView_SelectionCleared;
            treeView.AfterSelect += treeView_AfterSelect;
            treeView.NodeMouseClick += treeView_NodeMouseClick;
            treeView.MouseUp += treeView_MouseUp;

            WireFamilyDragDropHooks();
        }

        private void treeView_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            var node = e.Node;

            if (node?.Tag is not IGameEntity entity)
            {
                ClearSelections();
                return;
            }

            var before = treeView.SelectedNode;
            treeView.SelectedNode = node;

            if (ReferenceEquals(before, node))
                PulseSelectedNodeChanged();
        }

        private void treeView_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            TreeNode? node = e.Node;

            while (node != null && node.Tag is not IGameFamily)
                node = node.Parent;

            searchUI.SelectedNode = node;
            PulseSelectedNodeChanged();
        }
        private void JumpToNodeOnlyIfNeeded(TreeNode node)
        {
            if (!treeView.IsHandleCreated)
                return;

            if (node.TreeView != treeView)
                return;

            if (!node.IsVisible || node.Bounds.Height <= 0)
            {
                node.EnsureVisible();
                ApplyTopNodeStable(node);
                return;
            }

            var viewport = treeView.ClientRectangle;
            var bounds = node.Bounds;

            int topBuffer = Math.Max(2, treeView.ItemHeight / 3);
            int bottomBuffer = Math.Max(2, treeView.ItemHeight / 3);

            bool comfortablyVisible =
                bounds.Top >= viewport.Top + topBuffer &&
                bounds.Bottom <= viewport.Bottom - bottomBuffer;

            if (comfortablyVisible)
                return;

            node.EnsureVisible();
            ApplyTopNodeStable(node);
        }
        private void OnJumpToSearchItem(object target, bool forwards)
        {
            if (target is not TreeNode node)
                return;

            if (!treeView.IsHandleCreated)
                return;

            var viewport = treeView.ClientRectangle;

            var visibleRows = GetVisibleCount(treeView.Handle);
            if (visibleRows <= 0)
                visibleRows = Math.Max(1, viewport.Height / Math.Max(1, treeView.ItemHeight));

            var lastTreeNodeUnderParent = GetLastVisibleDescendant(node);

            var alreadyFullyVisible = GetNodeAndDescendantsAreFullyVisible(node);

            if (alreadyFullyVisible)
            {
                var before = treeView.SelectedNode;

                if (!ReferenceEquals(before, node))
                    treeView.SelectedNode = node;
                else
                    PulseSelectedNodeChanged();

                return;
            }

            TreeNode desiredTop;

            if (forwards)
            {
                desiredTop = node;
                node.EnsureVisible();
            }
            else
            {
                var blockRows = GetVisibleBlockRowCount(node);

                if (blockRows >= visibleRows)
                {
                    desiredTop = node;
                    node.EnsureVisible();
                }
                else
                {
                    lastTreeNodeUnderParent.EnsureVisible();

                    var top = lastTreeNodeUnderParent;
                    for (int i = 0; i < visibleRows - 1 && top.PrevVisibleNode != null; i++)
                        top = top.PrevVisibleNode;

                    desiredTop = top;
                }
            }

            
            var before_ = treeView.SelectedNode;

            if (!ReferenceEquals(before_, node))
                treeView.SelectedNode = node;
            else
                PulseSelectedNodeChanged();


            ApplyTopNodeStable(desiredTop);
        }

        private bool GetNodeAndDescendantsAreFullyVisible(TreeNode node)
        {
            var viewport = treeView.ClientRectangle;
            var lastTreeNodeUnderParent = GetLastVisibleDescendant(node);

            var alreadyFullyVisible =
                node.IsVisible &&
                lastTreeNodeUnderParent.IsVisible &&
                node.Bounds.Height > 0 &&
                lastTreeNodeUnderParent.Bounds.Height > 0 &&
                node.Bounds.Top >= viewport.Top &&
                lastTreeNodeUnderParent.Bounds.Bottom <= viewport.Bottom;

            return alreadyFullyVisible;
        }

        protected void ApplyTopNodeStable(TreeNode top)
        {
            // NOTE: One line buffer:
            if (top.PrevNode != null)
                top = top.PrevNode;
            else if (top.Parent != null)
                top = top.Parent;

            if (!ReferenceEquals(treeView.TopNode, top))
                treeView.TopNode = top;

            treeView.BeginInvoke(new Action(() =>
            {
                if (treeView.IsDisposed || !treeView.IsHandleCreated)
                    return;

                if (!ReferenceEquals(treeView.TopNode, top))
                    treeView.TopNode = top;
            }));
        }

        private static TreeNode GetLastVisibleDescendant(TreeNode node)
        {
            while (node.IsExpanded && node.Nodes.Count > 0)
                node = node.Nodes[node.Nodes.Count - 1];

            return node;
        }

        private static int GetVisibleBlockRowCount(TreeNode node)
        {
            int count = 1;

            if (!node.IsExpanded || node.Nodes.Count == 0)
                return count;

            for (int i = 0; i < node.Nodes.Count; i++)
                count += GetVisibleBlockRowCount(node.Nodes[i]);

            return count;
        }

        private static int GetVisibleCount(IntPtr handle)
        {
            const int TV_FIRST = 0x1100;
            const int TVM_GETVISIBLECOUNT = TV_FIRST + 16;

            return (int)SendMessage(handle, TVM_GETVISIBLECOUNT, IntPtr.Zero, IntPtr.Zero);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private void OnSearchStateChanged()
        {
            treeView.Invalidate();
            PulseSelectedNodeChanged();
        }

        /// <summary>
        /// Called when switching between primary and surrogate instances of this class.
        /// Attempts to make the jump in treeView scroll position less pronounced.
        /// </summary>
        /// <param name="targetFamilyName"></param>
        public void PrepForToggle(string? targetFamilyName)
        {
            if (InvokeRequired)
            {
                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(new Action(() => PrepForToggle(targetFamilyName)));
                return;
            }
            searchUI.SetSearchText(string.Empty);
            ClearSelections();

            if (string.IsNullOrWhiteSpace(targetFamilyName) || Nodes.Count == 0)
                return;

            var nodes = Nodes;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                if (node.Tag is not IGameFamily) continue;
                if (!string.Equals(node.Text, targetFamilyName, StringComparison.Ordinal))
                    continue;

                OnJumpToSearchItem(node, true);

                return;
            }

            if (nodes.Count > 0)
                nodes[0].EnsureVisible();
        }
        private void treeView_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            if (!AllowInteraction)
                return;

            var node = treeView.GetNodeAt(e.Location);

            if (mediaModeEnum == DAT_GROUPER_MEDIA_MODE.NOT_SET)
            {
                if (node != null && !isAutoUI && node.Tag is IGamePart part)
                    contextMenu.SetPartIncludeExclude(part.Exclude);
                else
                    contextMenu.DisableSetPartIncludeExclude();

                contextMenu.SetAliasesShowHide(!renderAliases);
                contextMenu.SetExcludedFamiliesShowHide(isSurrogate);

                contextMenu.SetCopyNameEnabled(
                    node != null && (node.Tag is IGameEntity && node.Tag is not IGameEntityProxy));

                var gamePermissions = GetMoveGameTopBottomPermissions(node);
                contextMenu.SetCanMoveGameTopBottom(gamePermissions.allowMoveToBottom, gamePermissions.allowMoveToTop);

                contextMenu.Show(treeView, e.Location, node?.Tag as IGameEntity);
            }
        }

        private (bool allowMoveToTop, bool allowMoveToBottom) GetMoveGameTopBottomPermissions(TreeNode? node)
        {
            if (isAutoUI == false &&
                node?.Tag is IGame &&
                node.Parent is TreeNode parent &&
                parent.Tag is IGameFamily &&
                parent.Nodes.Count > 1)
            {
                int index = parent.Nodes.IndexOf(node);

                if (index == 0)
                    return (false, true);

                if (index == parent.Nodes.Count - 1)
                    return (true, false);

                return (true, true);
            }

            return (false, false);
        }

        private void treeView_DrawNode(object? sender, DrawTreeNodeEventArgs e)
        {
            if (sender is not DatGrouperTreeView treeView || e.Node is null)
                return;

            DrawNode(e.Node, e.Bounds, e.Graphics);

            if (DatinatePerformanceUtil.TREEVIEW_UseStockDraw == false)
            {
#pragma warning disable CS0162 // Unreachable code detected
                e.DrawDefault = false;
#pragma warning restore CS0162 // Unreachable code detected
            }

            DatGrouperUiAnimationHelper.DrawPulseOverlay(treeView, e);

            if (!isAutoUI && isDraggedOver)
                TreeViewAdornerUtil.Draw(treeView, e);


            // 1 root node with 0 or 1 spacer nodes = empty:
            if (isAutoUI == false &&
                treeView.Nodes.Count == 1 &&
                treeView.Nodes[0].Nodes.Count < 2)
            {
                TreeRenderUtil.DrawOverlay(
                    "Drag a Family here to Curate",
                    "Drop from Queued → Here",
                    e.Graphics,
                    treeView,
                    e.Bounds);
            }
            else if (isAutoUI && treeView.AllNodesDisabled && isBeingDragged == false)
            {
                TreeRenderUtil.DrawOverlay(
                    "Drag an Entity here to Reset",
                    "Drop from Curated → Here",
                    e.Graphics,
                    treeView,
                    e.Bounds);
            }
        }

        private void DrawNode(TreeNode node, Rectangle bounds, Graphics graphics) { 
            
            if (DatinatePerformanceUtil.TREEVIEW_UseStockDraw == false) 
            {
#pragma warning disable CS0162 // Unreachable code detected
                TreeRenderUtil.DrawTreeNode(
                        treeView,
                        node,
                        bounds, // is this ever a different value to e.Node.Bounds?
                        graphics,
                        renderAliases,
                        searchUI.HighlightText,
                        searchUI.HighlightTextBackColour,
                        IsExcludedAutoNode(node),
                        treeView.DisabledTagTypes,
                        treeView.EnabledEntities,
                        treeView.FocusNode,
                        treeView.HiddenNodes,
                        GetNodeMediaCollection(node),
                        false);
#pragma warning restore CS0162 // Unreachable code detected
            }
        }

        private IMediaCollection? GetNodeMediaCollection(TreeNode? node)
        {
            if (node?.Tag is null || node.Tag is not IGameFamily family) return null;
            if (mediaCache == null) return null;

            if (!mediaCache.TryGetValue(family, out var mc)) return null;

            if (mc.IsEmptyForExport) return null;
            else return mc;
        }

        protected bool IsExcludedAutoNode(IGameEntity? entity)
        {
            if (entity == null) 
                return false;
            
            if (entity is IGameFamily family)
            {
                var games = family.GetAllGames();
                foreach (var game_ in games)
                {
                    var parts = game_.GetGameParts(false);
                    foreach (var part_ in parts)
                        if (!allCuratedAutoParts.Contains(part_)) return false;   
                }
                return true;
            }
            
            if (entity is IGame game)
            {
                var parts = game.GetGameParts(false);
                foreach (var part_ in parts)
                    if (!allCuratedAutoParts.Contains(part_)) return false;
                return true;
            }
            
            if (entity is IGamePart part)
                return allCuratedAutoParts.Contains(part);
             
            return false;
        }
        
        private bool IsExcludedAutoNode(TreeNode? node)
        {
            if (node == null) return false;
            if (!isAutoUI) return false;

            if (node.Tag is IGameEntity entity)
                return IsExcludedAutoNode(entity);

            return false;
        }

        private void RefreshSearchUI()
        {
            var dict = new Dictionary<IGameFamily, object>(nodesDictionary.Count);
            foreach (var kv in nodesDictionary)
                dict[kv.Key] = kv.Value;

            searchUI.SetUI(dict);
        }

        private void OnFamiliesChanged()
        {
            RefreshSearchUI();
            PulseSelectedNodeChanged();
        }

        // TODO: This needs review but ethos is right. We need to start by looking for the identical entity.
        public string? VisibleGameFamilyName 
        { 
            get
            {
                if (!treeView.IsHandleCreated) return null;

                var top = treeView.TopNode;
                if (top == null) return null;

                static TreeNode? FindFamilyNode(TreeNode? n)
                {
                    while (n != null && n.Tag is not IGameFamily)
                        n = n.Parent;

                    return n;
                }

                static bool IsAcceptableFamilyNode(TreeNode famNode)
                {
                    var redKey = DatGrouperIconUtil.Instance.GameFamilyRedIcon;
                    return !string.Equals(famNode.ImageKey, redKey, StringComparison.Ordinal) &&
                           !string.Equals(famNode.SelectedImageKey, redKey, StringComparison.Ordinal);
                }

                var viewport = treeView.ClientRectangle;

                string? target = null;

                var seen = new HashSet<TreeNode>();

                for (TreeNode? n = top; n != null; n = n.NextVisibleNode)
                {
                    if (n.Bounds.Height > 0 && n.Bounds.Top >= viewport.Bottom)
                        break;

                    if (n.Bounds.Height > 0 && n.Bounds.Bottom <= viewport.Top)
                        continue;

                    var famNode = FindFamilyNode(n);
                    if (famNode == null)
                        continue;

                    if (!seen.Add(famNode))
                        continue;

                    if (IsAcceptableFamilyNode(famNode) && famNode.Tag is IGameFamily fam)
                    {
                        target = fam.GetFamilyDisplayName();
                        break;
                    }
                }

                if (target == null)
                {
                    var topFam = FindFamilyNode(top);
                    var seenUp = new HashSet<TreeNode>();

                    for (TreeNode? n = topFam?.PrevVisibleNode; n != null; n = n.PrevVisibleNode)
                    {
                        var famNode = FindFamilyNode(n);
                        if (famNode == null)
                            continue;

                        if (!seenUp.Add(famNode))
                            continue;

                        if (IsAcceptableFamilyNode(famNode) && famNode.Tag is IGameFamily fam)
                        {
                            target = fam.GetFamilyDisplayName();
                            break;
                        }
                    }
                }
                return target;
            } 
        }

        private void treeView_SelectionCleared()
        {
            searchUI.ClearSearchHighlights();
            searchUI.SelectedNode = null;
            PulseSelectedNodeChanged();
        }

        internal void ClearDisabledNodes()
        {
            treeView.ClearDisabledNodes();
        }

        internal void SetDisabledNodes(IReadOnlySet<Type> set)
        {
            treeView.SetDisabledNodes(set, null, null);
        }
        private (DAT_GROUPER_MEMBERSHIP_ENUM Membership, bool IsShallow) GetPartVisualState(IGamePart gamePart)
        {
            if (partFingerprintReportDic?.TryGetValue(gamePart.Fingerprint, out var report) == true)
            {
                return (report.MembershipStatusEnum, report.IsShallowReference);
            }

            return (
                gamePart.Exclude
                    ? DAT_GROUPER_MEMBERSHIP_ENUM.EXCLUDE_RELATIVE
                    : DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_IMPLICIT,
                false);
        }
        private void ApplyPartNodeVisual(TreeNode partNode, IGamePart gamePart)
        {
            var (membership, isShallow) = GetPartVisualState(gamePart);

            partNode.Text = DatinateHelper.GetGamePartNameRender(gamePart);
            partNode.ForeColor = DatFilterHelper.GetExpressionColour(membership);

            var partIconKey = GetPartIconKey(isShallow, membership);
            partNode.ImageKey = partNode.SelectedImageKey = partIconKey;
        }

        private (bool Strict, bool Lenient) EvaluateGameStatus(IEnumerable<IGamePart> parts)
        {
            bool gameAllStrictInclude = true;
            bool gameAllLenientInclude = true;

            foreach (var gamePart in parts)
            {
                var (membership, isShallow) = GetPartVisualState(gamePart);

                if (isShallow)
                {
                    gameAllStrictInclude = false;
                }
                else
                {
                    if (membership != DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_EXPLICIT &&
                        membership != DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_IMPLICIT)
                    {
                        gameAllStrictInclude = false;
                    }
                }

                if (membership != DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_EXPLICIT &&
                    membership != DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_IMPLICIT &&
                    membership != DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_RELATIVE)
                {
                    gameAllLenientInclude = false;
                }
            }

            return (gameAllStrictInclude, gameAllLenientInclude);
        }
        private void RefreshGameNodeVisual(TreeNode gameNode)
        {
            if (gameNode.Tag is not IGame game)
                return;

            var parts = new List<IGamePart>(gameNode.Nodes.Count);

            foreach (TreeNode child in gameNode.Nodes)
            {
                if (child.Tag is not IGamePart part)
                    continue;

                parts.Add(part);
            }

            gameNode.Text = GetGameNameRender(parts, game.GetNameWithoutExt());

            bool isParent =
                gameNode.Parent != null &&
                gameNode.Parent.Nodes.Count > 0 &&
                ReferenceEquals(gameNode.Parent.Nodes[0], gameNode);

            var status = EvaluateGameStatus(parts);

            gameNode.ImageKey = gameNode.SelectedImageKey =
                GetGameIconKey(isParent: isParent, strict: status.Strict, lenient: status.Lenient);
        }
        private void RefreshFamilyNodeVisual(TreeNode familyNode)
        {
            if (familyNode.Tag is not IGameFamily)
                return;

            bool hasGreenCandidate = false;
            bool hasAmberCandidate = false;

            foreach (TreeNode gameNode in familyNode.Nodes)
            {
                var parts = new List<IGamePart>(gameNode.Nodes.Count);

                foreach (TreeNode child in gameNode.Nodes)
                {
                    if (child.Tag is IGamePart part)
                        parts.Add(part);
                }

                var status = EvaluateGameStatus(parts);

                if (status.Strict)
                    hasGreenCandidate = true;
                else if (status.Lenient)
                    hasAmberCandidate = true;
            }

            familyNode.ImageKey = familyNode.SelectedImageKey =
                hasGreenCandidate ? DatGrouperIconUtil.Instance.GameFamilyGreenIcon :
                hasAmberCandidate ? DatGrouperIconUtil.Instance.GameFamilyAmberIcon :
                                    DatGrouperIconUtil.Instance.GameFamilyRedIcon;
        }



        //
        // DRAG N DROP
        //
        protected sealed class DatGrouperDragData : IGameEntityDataPacket
        {
            public DAT_GROUPER_UI_ENUM SourceEnum;
            
            public bool IsFamily => Family != null;
            public bool IsGame => Game != null;
            public bool IsPart => Part != null;

            public IGameEntity Entity { get; }
            public IGameFamily? Family { get; } = null;
            public IGame? Game { get; } = null;
            public IGamePart? Part { get; } = null;

            public DatGrouperDragData(DAT_GROUPER_UI_ENUM datGrouperUiEnum, IGameEntity entity)
            {
                SourceEnum = datGrouperUiEnum;

                Entity = entity;
                if (entity is IGameFamily family) Family = family;
                else if (entity is IGame game) Game = game;
                else if (entity is IGamePart part) Part = part;
            }
        }

        /// <summary>
        /// Returns a list of entities that the a node may interact with in the curated list
        /// ... move part, change part's game owner, change game order.
        /// ;thisEntity' is the entity being manupilated and is passed to ensure it doesn't interact with itself.
        /// </summary>
        /// <param name="dragNode"></param>
        /// <param name="draggingPart"></param>
        /// <param name="thisEntity"></param>
        /// <returns></returns>
        protected static List<IGameEntity> BuildCuratedEnabledEntitiesForDrag(
            TreeNode dragNode,
            IGameEntity thisEntity)
        {
            TreeNode? familyNode = dragNode;

            while (familyNode != null && familyNode.Tag is not IGameFamily)
                familyNode = familyNode.Parent;

            var enabledList = new List<IGameEntity>();

            if (familyNode == null)
                return enabledList;

            if (dragNode.Tag is IGameFamily fam && !object.ReferenceEquals(fam, thisEntity))
            {
                enabledList.Add(fam);
                return enabledList;
            }

            if (thisEntity is IGamePart)
            {
                for (int gi = 0; gi < familyNode.Nodes.Count; gi++)
                {
                    var gameNode = familyNode.Nodes[gi];

                    for (int pi = 0; pi < gameNode.Nodes.Count; pi++)
                    {
                        if (gameNode.Nodes[pi].Tag is IGamePart part && !object.ReferenceEquals(part, thisEntity))
                            enabledList.Add(part);
                    }
                }

                return enabledList;
            }

            for (int gi = 0; gi < familyNode.Nodes.Count; gi++)
            {
                if (familyNode.Nodes[gi].Tag is IGame game && !object.ReferenceEquals(game, thisEntity))
                    enabledList.Add(game);
            }

            return enabledList;
        }
        private void WireFamilyDragDropHooks()
        {
            treeView.ItemDrag -= treeView_ItemDrag;
            treeView.DragEnter -= treeView_DragEnter;
            treeView.DragOver -= treeView_DragOver;
            treeView.DragDrop -= treeView_DragDrop;
            treeView.DragLeave -= treeView_DragLeave;

            treeView.AllowDrop = true;

            treeView.ItemDrag += treeView_ItemDrag;
            treeView.DragEnter += treeView_DragEnter;
            treeView.DragOver += treeView_DragOver;
            treeView.DragDrop += treeView_DragDrop;
            treeView.DragLeave += treeView_DragLeave;
        }
        protected static void ClearDragCaption() =>
            DragDropPreviewForm.UpdateCaption(null);
        
        protected static void UpdateDragCaption(string main, string? sub = null) =>
            DragDropPreviewForm.UpdateCaption(main, sub);

        protected static DRAG_INSERT_MODE GetInsertMode(TreeNode node, int mouseYClient)
        {
            var midY = node.Bounds.Top + (node.Bounds.Height / 2);
            return mouseYClient >= midY ? DRAG_INSERT_MODE.After : DRAG_INSERT_MODE.Before;
        }

        protected void ApplyDragFeedback(
            TreeNode? hotNode,
            TreeNode? adornerNode,
            DRAG_INSERT_MODE mode,
            string? main,
            string? sub = null)
        {
            treeView.SetHotNode(hotNode);

            if (string.IsNullOrWhiteSpace(main))
                ClearDragCaption();
            else
                UpdateDragCaption(main, sub);

            var adornerMode =
                mode == DRAG_INSERT_MODE.Before ? TreeViewAdornerUtil.INSERTION_POINT_ENUM.Before :
                mode == DRAG_INSERT_MODE.After ? TreeViewAdornerUtil.INSERTION_POINT_ENUM.After :
                                                  TreeViewAdornerUtil.INSERTION_POINT_ENUM.None;

            TreeViewAdornerUtil.Update(treeView, adornerNode, adornerMode);
        }

        protected void ClearDragFeedback()
        {
            treeView.SetHotNode(null);
            ClearDragCaption();
            TreeViewAdornerUtil.Clear();
        }
        
        protected void SetIsDraggedOver(bool value)
        {
            if (isDraggedOver == value)
                return;

            isDraggedOver = value;
            treeView.Invalidate();
        }

        protected void SetIsBeingDragged(bool value)
        {
            if (isBeingDragged == value)
                return;

            isBeingDragged = value;
            treeView.Invalidate();
        }
        private void treeView_DragLeave(object? sender, EventArgs e)
        {
            SetIsDraggedOver(false);
            ClearDragFeedback();
        }

        private void treeView_DragDrop(object? sender, DragEventArgs e)
        {
            var hasPayload = e.Data?.GetDataPresent(typeof(DatGrouperDragData)) == true;
            var dragData = hasPayload ? (e.Data?.GetData(typeof(DatGrouperDragData)) as DatGrouperDragData) : null;

            if (dragData == null)
            {
                SetIsDraggedOver(false);
                ClearDragFeedback();
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = HandleTreeViewDragDrop(dragData, new Point(e.X, e.Y));
            ClearDragFeedback();
        }

        private void treeView_DragOver(object? sender, DragEventArgs e)
        {
            var hasPayload = e.Data?.GetDataPresent(typeof(DatGrouperDragData)) == true;
            var dragData = hasPayload ? (e.Data?.GetData(typeof(DatGrouperDragData)) as DatGrouperDragData) : null;

            if (dragData == null)
            {
                SetIsDraggedOver(false);
                ClearDragFeedback();
                e.Effect = DragDropEffects.None;
                return;
            }

            var screenPt = new Point(e.X, e.Y);
            DragDropPreviewForm.UpdatePosition(screenPt);

            e.Effect = HandleTreeViewDragOver(dragData, screenPt);
        }
        private void treeView_DragEnter(object? sender, DragEventArgs e)
        {
            var hasPayload = e.Data?.GetDataPresent(typeof(DatGrouperDragData)) == true;
            var dragData = hasPayload ? (e.Data?.GetData(typeof(DatGrouperDragData)) as DatGrouperDragData) : null;

            if (dragData == null)
            {
                SetIsDraggedOver(false);
                ClearDragFeedback();
                e.Effect = DragDropEffects.None;
                return;
            }

            var screenPt = new Point(e.X, e.Y);
            DragDropPreviewForm.UpdatePosition(screenPt);

            e.Effect = HandleTreeViewDragEnter(dragData);
        }

        private void treeView_ItemDrag(object? sender, ItemDragEventArgs e)
        {
            if (!AllowInteraction)
                return;

            if (e.Item is not TreeNode node) return;

            if (node.Tag is IGameEntityProxy || node.Tag is not IGameEntity entity) return;

            if (treeView.IsNodeDisabled(node)) return;

            treeView.SelectedNode = node;

            if (isAutoUI)
                HandleTreeViewItemDrag(
                    new DatGrouperDragData(DAT_GROUPER_UI_ENUM.Automated, entity), node);

            else
                HandleTreeViewItemDrag(
                    new DatGrouperDragData(DAT_GROUPER_UI_ENUM.Curated, entity), node);
        }
        protected void InitialiseDragDropPreviewWithoutTreeChrome(TreeNode node)
        {
            var selectedNode = treeView.SelectedNode;
            var hotNode = treeView.FocusNode;

            treeView.SetHotNode(null);

            if (treeView.SelectedNode != null)
                treeView.SelectedNode = null;

            treeView.Refresh();
            treeView.Update();

            try
            {
                DragDropPreviewForm.Initialise(treeView, node);
            }
            finally
            {
                if (!ReferenceEquals(treeView.SelectedNode, selectedNode))
                    treeView.SelectedNode = selectedNode;

                treeView.SetHotNode(hotNode);

                treeView.Refresh();
                treeView.Update();
            }
        }
    }
}
