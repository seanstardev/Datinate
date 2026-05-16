using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using static com.RADIO.Datinate.RMVC.Shared.DatGrouperEditRequestDTO;
using static datinate.app.DatGrouperTreeView;

namespace datinate.app
{
    public class CuratedUI : DatGrouperUiBase
    {
        protected override void HandleTreeViewItemDrag(DatGrouperDragData dragData, TreeNode node)
        {
            var entity = dragData.Entity;
            var curatedPayload = new DatGrouperDragData(DAT_GROUPER_UI_ENUM.Curated, entity);

            InitialiseDragDropPreviewWithoutTreeChrome(node);

            var hiddenNodes = CollectGameEntityNodesAndDescendants(node).ToHashSet();

            SetDragTargetDisabledNodesEvt?.Invoke(
                new HashSet<Type>() { typeof(IGameEntity) });

            var enabledEntities = BuildCuratedEnabledEntitiesForDrag(node, entity);
            List<Type> types = [typeof(IGameEntity)];

            // Allow curated parts and games to be moved to other curated familes.
            // Allow curated families to be merged with other curated families.
            if (entity is IGamePart)
                types = [typeof(IGame), typeof(IGameFamily), typeof(IGameEntityProxy)];
            if (entity is IGame)
                types = [typeof(IGamePart), typeof(IGameFamily), typeof(IGameEntityProxy)];
            if (entity is IGameFamily)
                types = [typeof(IGamePart), typeof(IGame), typeof(IGameEntityProxy)];

            treeView.SetDisabledNodes(types, enabledEntities, hiddenNodes);
            treeView.Refresh();

            var data = new DataObject();
            data.SetData(typeof(DatGrouperDragData), curatedPayload);

            var dto = new DatGrouperEntryDTO(entity, false);

            base.InvokeGameEntityDragStartEvt(dto);

            SetIsBeingDragged(true);
            
            try
            {
                var effect = treeView.DoDragDrop(data, DragDropEffects.Move);

                if (effect != DragDropEffects.None &&
                    data.GetDataPresent(DatinateHelper.WEB_BROWSER_MAIN_SearchGame))
                {
                    var name = DatinateHelper.GetGameEntityName(entity);

                    if (!string.IsNullOrWhiteSpace(name))
                        base.InvokeSearchGameNameEvt(name);
                }

                else if (effect != DragDropEffects.None &&
                    data.GetDataPresent(DatinateHelper.WEB_BROWSER_MEDIA_ShowMedia_2))
                { }
                
                else if (effect != DragDropEffects.None &&
                    data.GetDataPresent(DatinateHelper.WEB_BROWSER_MEDIA_ShowMedia))
                {
                    var dto_ = new DatGrouperEntryDTO(entity, false);
                    base.InvokeShowGameMediaEvt(dto_);
                }
            }
            finally
            {
                SetIsBeingDragged(false);
                DragDropPreviewForm.Teardown();

                base.InvokeGameEntityDragStopEvt();
                base.InvokeClearDragTargetDisabledNodesEvt();

                if (base.mediaModeEnum != DatinateEnums.DAT_GROUPER_MEDIA_MODE.MediaAssign)
                    treeView.ClearDisabledNodes();
                else
                    treeView.ClearHiddenNodes();
                
                treeView.Refresh();
            }
        }

        protected override DragDropEffects HandleTreeViewDragDrop(DatGrouperDragData dragData, Point point)
        {
            if (mediaModeEnum != DatinateEnums.DAT_GROUPER_MEDIA_MODE.NOT_SET)
                return DragDropEffects.None;

            SetIsDraggedOver(false);

            var effect = DragDropEffects.None;

            ClearSelections();
            base.InvokeClearDragTargetDisabledNodesEvt();

            var ptAny = treeView.PointToClient(point);
            var hoverAny = treeView.GetNodeAt(ptAny) ?? treeView.RootSpacerNode;

            if (hoverAny == null)
            {
                DragDropPreviewForm.Teardown();
                base.InvokeGameEntityDragStopEvt();
                return effect;
            }

            if (dragData.SourceEnum == DAT_GROUPER_UI_ENUM.Curated)
            {
                if (dragData.IsPart)
                {
                    if (TryAcceptCuratedPartDrop(dragData, hoverAny, ptAny))
                        effect = DragDropEffects.Move;
                }
                else if (dragData.IsGame)
                {
                    if (TryAcceptCuratedGameDrop(dragData, hoverAny, ptAny))
                        effect = DragDropEffects.Move;
                }
                else if (dragData.IsFamily)
                {
                    if (TryAcceptCuratedFamilyDrop(dragData, hoverAny, ptAny))
                        effect = DragDropEffects.Move;
                }

                DragDropPreviewForm.Teardown();
                base.InvokeGameEntityDragStopEvt();
                return effect;
            }

            var pt = ptAny;
            var hover = hoverAny;

            if (dragData.IsPart)
            {
                if (TryAcceptPartDrop(dragData, hover, pt))
                    effect = DragDropEffects.Move;

                DragDropPreviewForm.Teardown();
                base.InvokeGameEntityDragStopEvt();
                return effect;
            }

            if (dragData.IsFamily)
            {
                if (hover.Tag is not IGameFamily)
                {
                    InvokeEditRequestEvt(
                        new DatGrouperEditRequestDTO(EDIT_ACTION_ENUM.FamilyAdd)
                        {
                            SourceFamily = dragData.Family
                        });

                    effect = DragDropEffects.Move;

                    DragDropPreviewForm.Teardown();
                    InvokeGameEntityDragStopEvt();
                    return effect;
                }

                if (hover.Tag is IGameFamily targetFamilyClone)
                {
                    var after = GetInsertMode(hover, pt.Y) == DRAG_INSERT_MODE.After;

                    InvokeEditRequestEvt(
                        new DatGrouperEditRequestDTO(after ? EDIT_ACTION_ENUM.FamilyMergeAsSub : EDIT_ACTION_ENUM.FamilyMergeAsMain)
                        {
                            SourceFamily = dragData.Family,
                            TargetFamily = targetFamilyClone
                        });

                    effect = DragDropEffects.Move;

                    DragDropPreviewForm.Teardown();
                    InvokeGameEntityDragStopEvt();
                    return effect;
                }

                DragDropPreviewForm.Teardown();
                base.InvokeGameEntityDragStopEvt();
                return effect;
            }

            if (dragData.IsGame)
            {
                TreeNode? anchor = ResolveGameAnchor(hover);

                if (anchor?.Tag is IGame targetGame)
                {
                    var after = GetInsertMode(anchor, pt.Y) == DRAG_INSERT_MODE.After;
                    var action = after ? EDIT_ACTION_ENUM.GameMoveAfter : EDIT_ACTION_ENUM.GameMoveBefore;

                    base.InvokeEditRequestEvt(
                        new DatGrouperEditRequestDTO(action)
                        {
                            SourceGame = dragData.Game,
                            TargetGame = targetGame
                        });

                    effect = DragDropEffects.Move;
                }

                DragDropPreviewForm.Teardown();
                InvokeGameEntityDragStopEvt();
                return effect;
            }

            DragDropPreviewForm.Teardown();
            InvokeGameEntityDragStopEvt();
            return effect;
        }

        protected override DragDropEffects HandleTreeViewDragEnter(DatGrouperDragData dragData)
        {
            if (mediaModeEnum != DatinateEnums.DAT_GROUPER_MEDIA_MODE.NOT_SET)
                return DragDropEffects.None;

            SetIsDraggedOver(true);
            ClearDragFeedback();

            return DragDropEffects.Move;
        }

        protected override DragDropEffects HandleTreeViewDragOver(DatGrouperDragData dragData, Point point)
        {
            if (mediaModeEnum != DatinateEnums.DAT_GROUPER_MEDIA_MODE.NOT_SET)
                return DragDropEffects.None;

            SetIsDraggedOver(true);

            var pt = treeView.PointToClient(point);
            var hitNode = treeView.GetNodeAt(pt);
            var hitIsSpacer = hitNode?.Tag is DatGrouperTreeView.SpacerNodeTag;
            var hitIsHidden = hitNode != null && treeView.HiddenNodes.Contains(hitNode);

            if (hitIsHidden)
            {
                ClearDragFeedback();
                return DragDropEffects.None;
            }

            var thisNode = (hitNode == null || hitIsSpacer)
                ? treeView.RootSpacerNode
                : hitNode;

            if (thisNode == null)
            {
                ClearDragFeedback();
                return DragDropEffects.None;
            }

            var rawHotNode = (hitNode == null || hitIsSpacer) ? null : hitNode;

            if (dragData.SourceEnum == DAT_GROUPER_UI_ENUM.Curated)
            {
                if (!CanAcceptCuratedDrop(dragData, thisNode))
                {
                    ClearDragFeedback();
                    return DragDropEffects.None;
                }

                if (treeView.IsNodeDisabled(thisNode))
                {
                    ClearDragFeedback();
                    return DragDropEffects.None;
                }

                if (dragData.IsFamily)
                {
                    var mode = GetInsertMode(thisNode, pt.Y);
                    var after = mode == DRAG_INSERT_MODE.After;

                    ApplyDragFeedback(
                        thisNode,
                        thisNode,
                        mode,
                        after ? "Append Games to Curated Family" : "Merge Curated Family",
                        after ? "Release to Append this Family’s Games" : "Release to Replace (existing Games kept and appended)");

                    return DragDropEffects.Move;
                }

                if (dragData.IsGame)
                {
                    var mode = GetInsertMode(thisNode, pt.Y);
                    var after = mode == DRAG_INSERT_MODE.After;

                    ApplyDragFeedback(
                        thisNode,
                        thisNode,
                        mode,
                        "Move Game " + (after ? "After" : "Before"),
                        after ? "Release to insert after this Game" : "Release to insert before this Game");

                    return DragDropEffects.Move;
                }

                if (dragData.IsPart)
                {
                    if (thisNode.Tag is IGamePart)
                    {
                        var mode = GetInsertMode(thisNode, pt.Y);
                        var after = mode == DRAG_INSERT_MODE.After;

                        ApplyDragFeedback(
                            thisNode,
                            thisNode,
                            mode,
                            "Move Part " + (after ? "After" : "Before"),
                            after ? "Release to insert after this Part" : "Release to insert before this Part");
                    }
                    else
                    {
                        ApplyDragFeedback(rawHotNode, null, DRAG_INSERT_MODE.None, null);
                    }

                    return DragDropEffects.Move;
                }

                ApplyDragFeedback(rawHotNode, null, DRAG_INSERT_MODE.None, null);
                return DragDropEffects.Move;
            }

            if (dragData.IsFamily && thisNode.Tag is not IGameFamily)
            {
                ApplyDragFeedback(
                    rawHotNode,
                    null,
                    DRAG_INSERT_MODE.None,
                    "Create a Curated Family",
                    "Release to add this Family as a new Curated Family");

                return DragDropEffects.Move;
            }

            if (thisNode.Tag is IGameEntityProxy)
            {
                if (!dragData.IsFamily)
                {
                    ClearDragFeedback();
                    return DragDropEffects.None;
                }
            }

            if (thisNode.Tag is not IGameEntity && thisNode.Tag is not IGameEntityProxy)
            {
                ClearDragFeedback();
                return DragDropEffects.None;
            }

            if (thisNode.Tag is not IGameEntityProxy && treeView.IsNodeDisabled(thisNode))
            {
                ClearDragFeedback();
                return DragDropEffects.None;
            }

            if (dragData.IsFamily)
            {
                if (thisNode.Tag is IGameFamily)
                {
                    var mode = GetInsertMode(thisNode, pt.Y);
                    var after = mode == DRAG_INSERT_MODE.After;

                    ApplyDragFeedback(
                        thisNode,
                        thisNode,
                        mode,
                        after ? "Append Games to Curated Family" : "Merge Curated Family",
                        after ? "Release to Append this Family’s Games" : "Release to Replace (existing Games kept and appended)");

                    return DragDropEffects.Move;
                }

                ApplyDragFeedback(
                    rawHotNode,
                    null,
                    DRAG_INSERT_MODE.None,
                    "Create a Curated Family",
                    "Release to add this Family as a new Curated Family");

                return DragDropEffects.Move;
            }

            if (dragData.IsGame)
            {
                var anchor = ResolveGameAnchor(thisNode);

                if (anchor?.Tag is not IGame)
                {
                    ClearDragFeedback();
                    return DragDropEffects.None;
                }

                var mode = GetInsertMode(anchor, pt.Y);
                var after = mode == DRAG_INSERT_MODE.After;

                ApplyDragFeedback(
                    anchor,
                    anchor,
                    mode,
                    "Add game to Curated Family",
                    after ? "Release to insert after this Game" : "Release to insert before this Game");

                return DragDropEffects.Move;
            }

            if (dragData.IsPart)
            {
                if (thisNode.Tag is IGamePart)
                {
                    var mode = GetInsertMode(thisNode, pt.Y);
                    var after = mode == DRAG_INSERT_MODE.After;

                    ApplyDragFeedback(
                        thisNode,
                        thisNode,
                        mode,
                        "Add Part to Curated Game",
                        after ? "Release to insert after this Part" : "Release to insert before this Part");

                    return DragDropEffects.Move;
                }

                ApplyDragFeedback(
                    rawHotNode,
                    null,
                    DRAG_INSERT_MODE.None,
                    "Add Part to Curated Game",
                    "Drop onto a Part to choose an insertion point");

                return DragDropEffects.Move;
            }

            ApplyDragFeedback(rawHotNode, null, DRAG_INSERT_MODE.None, "Add to Curated");
            return DragDropEffects.Move;
        }

        private static TreeNode? ResolveGameAnchor(TreeNode? node)
        {
            if (node == null)
                return null;

            if (node.Tag is IGamePart)
                return node.Parent;

            return node;
        }

        private bool TryAcceptCuratedFamilyDrop(DatGrouperDragData dragData, TreeNode hover, Point clientPt)
        {
            if (mediaModeEnum != DatinateEnums.DAT_GROUPER_MEDIA_MODE.NOT_SET)
                return false;

            if (hover.Tag is IGameFamily family)
            {
                var after = GetInsertMode(hover, clientPt.Y) == DRAG_INSERT_MODE.After;
                var action = after ? EDIT_ACTION_ENUM.FamilyMergeAsSub : EDIT_ACTION_ENUM.FamilyMergeAsMain;

                base.InvokeEditRequestEvt(
                    new DatGrouperEditRequestDTO(action)
                    {
                        SourceFamily = dragData.Family,
                        TargetFamily = family
                    });
                return true;
            }
            else return false;
        }

        private bool TryAcceptCuratedGameDrop(DatGrouperDragData dragData, TreeNode hover, Point clientPt)
        {
            if (mediaModeEnum != DatinateEnums.DAT_GROUPER_MEDIA_MODE.NOT_SET)
                return false;

            if (hover.Tag is IGame game)
            {
                var after = GetInsertMode(hover, clientPt.Y) == DRAG_INSERT_MODE.After;
                var action = after ? EDIT_ACTION_ENUM.GameMoveAfter : EDIT_ACTION_ENUM.GameMoveBefore;
                
                base.InvokeEditRequestEvt(
                    new DatGrouperEditRequestDTO(action)
                    {
                        SourceGame = dragData.Game,
                        TargetGame = game
                    });

                return true;
            }
            else return false;
        }

        private bool TryAcceptCuratedPartDrop(DatGrouperDragData dragData, TreeNode hover, Point clientPt)
        {
            if (mediaModeEnum != DatinateEnums.DAT_GROUPER_MEDIA_MODE.NOT_SET)
                return false;

            if (hover.Tag is IGamePart part)
            {
                var after = GetInsertMode(hover, clientPt.Y) == DRAG_INSERT_MODE.After;
                var action = after ? EDIT_ACTION_ENUM.PartMoveAfter : EDIT_ACTION_ENUM.PartMoveBefore;

                base.InvokeEditRequestEvt(
                    new DatGrouperEditRequestDTO(action)
                    {
                        SourcePart = dragData.Part,
                        TargetPart = part
                    });

                return true;
            }
            else return false;
        }

        private bool CanAcceptCuratedDrop(DatGrouperDragData dragData, TreeNode hover)
        {
            if (mediaModeEnum != DatinateEnums.DAT_GROUPER_MEDIA_MODE.NOT_SET)
                return false;

            if (hover.Tag is not IGameEntity entity)
                return false;

            if (dragData.Family is not null && object.ReferenceEquals(entity, dragData.Family))
                return false;
            if (dragData.Game is not null && object.ReferenceEquals(entity, dragData.Game))
                return false;
            if (dragData.Part is not null && object.ReferenceEquals(entity, dragData.Part))
                return false;

            if (hover.Tag is SpacerNodeTag)
                return false;

            if (treeView.IsNodeDisabled(hover))
                return false;

            return true;
        }

        private bool TryAcceptPartDrop(DatGrouperDragData entityData, TreeNode hover, Point clientPt)
        {
            if (mediaModeEnum != DatinateEnums.DAT_GROUPER_MEDIA_MODE.NOT_SET)
                return false;

            if (hover.Tag is not IGamePart anchorPartClone)
                return false;

            var after = GetInsertMode(hover, clientPt.Y) == DRAG_INSERT_MODE.After;
            EDIT_ACTION_ENUM action = after ? EDIT_ACTION_ENUM.PartMoveAfter : EDIT_ACTION_ENUM.PartMoveBefore;

            InvokeEditRequestEvt(
                new DatGrouperEditRequestDTO(action)
                {
                    SourcePart = entityData.Part,
                    TargetPart = anchorPartClone
                });

            return true;
        }
    }
}
