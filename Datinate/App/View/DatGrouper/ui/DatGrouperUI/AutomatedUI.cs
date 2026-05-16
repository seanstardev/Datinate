using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using static com.RADIO.Datinate.RMVC.Shared.DatGrouperEditRequestDTO;

namespace datinate.app
{
    public class AutomatedUI : DatGrouperUiBase
    {
        protected override void HandleTreeViewItemDrag(DatGrouperDragData dragData, TreeNode node)
        {
            var entity = dragData.Entity;

            var payload = new DatGrouperDragData(DAT_GROUPER_UI_ENUM.Automated, entity);

            if (payload.IsPart && IsExcludedAutoNode(payload.Part!))
                return;
            if (payload.IsGame && IsExcludedAutoNode(payload.Game!))
                return;
            if (payload.IsFamily && IsExcludedAutoNode(payload.Family!))
                return;

            HashSet<Type>? targetDisabledTagTypes = null;

            if (entity is IGameFamily)
                targetDisabledTagTypes = [typeof(IGame), typeof(IGamePart)];
            else if (entity is IGame)
                targetDisabledTagTypes = [typeof(IGameEntityProxy), typeof(IGameFamily), typeof(IGamePart)];
            else
                targetDisabledTagTypes = [typeof(IGameEntityProxy), typeof(IGameFamily), typeof(IGame)];

            SetIsBeingDragged(true);

            try
            {
                if (targetDisabledTagTypes != null)
                    SetDragTargetDisabledNodesEvt?.Invoke(targetDisabledTagTypes);

                InitialiseDragDropPreviewWithoutTreeChrome(node);

                var affected = CollectGameEntityNodesAndDescendants(node);
                var hiddenNodes_ = affected.ToHashSet();

                HashSet<Type> disabledTagTypes = [typeof(IGameEntity)];

                treeView.SetDisabledNodes(disabledTagTypes, null, hiddenNodes_);

                treeView.Refresh();

                var data = new DataObject();
                data.SetData(typeof(DatGrouperDragData), payload);

                var dto_ = new DatGrouperEntryDTO(entity, true);

                base.InvokeGameEntityDragStartEvt(dto_);

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
                    var dto__ = new DatGrouperEntryDTO(entity, true);

                    base.InvokeShowGameMediaEvt(dto__);
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
                {
                    treeView.SetDisabledNodes(
                        [typeof(IGame), typeof(IGamePart)],
                        null, null);
                }

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

            if (dragData.SourceEnum == DAT_GROUPER_UI_ENUM.Curated)
            {
                if (dragData.IsFamily)
                {
                    base.InvokeEditRequestEvt(
                        new DatGrouperEditRequestDTO(EDIT_ACTION_ENUM.FamilyReset)
                        {
                            SourceFamily = dragData.Family
                        });
                }
                else if (dragData.IsGame)
                {
                    base.InvokeEditRequestEvt(
                        new DatGrouperEditRequestDTO(EDIT_ACTION_ENUM.GameReset)
                        {
                            SourceGame = dragData.Game
                        });
                }
                else if (dragData.IsPart)
                {
                    base.InvokeEditRequestEvt(
                        new DatGrouperEditRequestDTO(EDIT_ACTION_ENUM.PartReset)
                        {
                            SourcePart = dragData.Part
                        });
                }
            }
            SetIsBeingDragged(false);
            DragDropPreviewForm.Teardown();
            base.InvokeGameEntityDragStopEvt();

            return effect;
        }

        protected override DragDropEffects HandleTreeViewDragEnter(DatGrouperDragData dragData)
        {
            if (mediaModeEnum != DatinateEnums.DAT_GROUPER_MEDIA_MODE.NOT_SET)
                return DragDropEffects.None;

            var ok = dragData.SourceEnum == DAT_GROUPER_UI_ENUM.Curated;
            SetIsDraggedOver(ok);

            var effect = ok ? DragDropEffects.Move : DragDropEffects.None;

            if (!ok)
            {
                ClearDragCaption();
                return effect;
            }

            if (dragData.IsFamily)
                UpdateDragCaption("Restore Family to Queued", "Release to move it back into Queued");
            else if (dragData.IsGame)
                UpdateDragCaption("Restore Game to Queued", "Release to move it back into Queued");
            else
                UpdateDragCaption("Restore Part to Queued", "Release to move it back into Queued");

            return effect;
        }

        protected override DragDropEffects HandleTreeViewDragOver(DatGrouperDragData dragData, Point point)
        {
            if (mediaModeEnum != DatinateEnums.DAT_GROUPER_MEDIA_MODE.NOT_SET)
                return DragDropEffects.None;

            treeView.SetHotNode(null);

            var ok = dragData.SourceEnum == DAT_GROUPER_UI_ENUM.Curated;
            SetIsDraggedOver(ok);

            var effect = DragDropEffects.None;

            if (!ok)
            {
                effect = DragDropEffects.None;
                ClearDragCaption();
                return effect;
            }

            effect = DragDropEffects.Move;

            if (dragData.IsFamily)
                UpdateDragCaption("Restore Family to Queued", "Release to move it back into Queued");
            else if (dragData.IsGame)
                UpdateDragCaption("Restore Game to Queued", "Release to move it back into Queued");
            else
                UpdateDragCaption("Restore Part to Queued", "Release to move it back into Queued");

            return effect;
        }
    }
}