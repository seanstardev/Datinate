using app.datinate;
using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatGrouperEditRequestDTO;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class DatGrouperModel : RModel
    {
        public IReadOnlyList<IGameFamily> CuratedFamilies 
            => overlay?.GetCuratedFamiliesAlphaSorted() ?? [];
        
        private CurationOverlay? overlay;
        public void CreateSession(
            IReadOnlyList<IGameFamily> families, 
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSet,
            IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary)
        {
            overlay = new CurationOverlay(
                families, flagFilterSet, softwareIdDatGroupEnumDictionary);
        }
        public DatGrouperEditDelta? ApplyUndo() => overlay?.ApplyUndo();
        public DatGrouperEditDelta? ApplyRedo() => overlay?.ApplyRedo();
        
        public DatGrouperEditDelta? PerformUpdate(DatGrouperEditRequestDTO dto)
        {
            if (overlay == null || dto.EditActionEnum == EDIT_ACTION_ENUM.NOT_SET)
                return null;

            DatGrouperEditDelta? delta = null;

            switch (dto.EditActionEnum)
            {
                // Reset
                case EDIT_ACTION_ENUM.FamilyReset:
                    if (dto.SourceFamily != null)
                        delta = overlay.TryResetFamily(dto.SourceFamily);
                    break;
                case EDIT_ACTION_ENUM.GameReset:
                    if (dto.SourceGame != null)
                        delta = overlay.TryResetGame(dto.SourceGame);
                    break;
                case EDIT_ACTION_ENUM.PartReset:
                    if (dto.SourcePart != null)
                        delta = overlay.TryResetPart(dto.SourcePart);
                    break;

                // Family
                case EDIT_ACTION_ENUM.FamilyAdd:
                    if (dto.SourceFamily != null)
                        delta = overlay.TryAddFamily(dto.SourceFamily);
                    break;
                case EDIT_ACTION_ENUM.FamilyMergeAsMain:
                    if (dto.SourceFamily != null && dto.TargetFamily != null)
                        delta = overlay.TryMoveAndMergeFamily(dto.SourceFamily, dto.TargetFamily, true);
                    break;
                case EDIT_ACTION_ENUM.FamilyMergeAsSub:
                    if (dto.SourceFamily != null && dto.TargetFamily != null)
                        delta = overlay.TryMoveAndMergeFamily(dto.SourceFamily, dto.TargetFamily, false);
                    break;

                // Game
                case EDIT_ACTION_ENUM.GameMoveAfter:
                    if (dto.SourceGame != null && dto.TargetGame != null)
                        delta = overlay.TryAddOrMoveGameAfter(dto.SourceGame, dto.TargetGame);
                    break;
                case EDIT_ACTION_ENUM.GameMoveBefore:
                    if (dto.SourceGame != null && dto.TargetGame != null)
                        delta = overlay.TryAddOrMoveGameBefore(dto.SourceGame, dto.TargetGame);
                    break;

                case EDIT_ACTION_ENUM.GameMoveToBottom:
                    if (dto.SourceGame != null )
                        delta = overlay.TryMoveGameToTopOrBottom(dto.SourceGame, false);
                    break;
                case EDIT_ACTION_ENUM.GameMoveToTop:
                    if (dto.SourceGame != null)
                        delta = overlay.TryMoveGameToTopOrBottom(dto.SourceGame, true);
                    break;


                // Part
                case EDIT_ACTION_ENUM.PartMoveAfter:
                    if (dto.SourcePart != null && dto.TargetPart != null)
                        delta = overlay.TryAddOrMovePartAfter(dto.SourcePart, dto.TargetPart);
                    break;
                case EDIT_ACTION_ENUM.PartMoveBefore:
                    if (dto.SourcePart != null && dto.TargetPart != null)
                        delta = overlay.TryAddOrMovePartBefore(dto.SourcePart, dto.TargetPart);
                    break;

                // Part Include / Exclude
                case EDIT_ACTION_ENUM.PartSetAsInclude:
                    if (dto.SourcePart != null)
                        delta = overlay.SetCuratedPartAsInclude(dto.SourcePart);
                    break;
                case EDIT_ACTION_ENUM.PartSetAsExclude:
                    if (dto.SourcePart != null)
                        delta = overlay.SetCuratedPartAsExclude(dto.SourcePart);
                    break;
            }

            return delta;
        }

        public DatGrouperEditDelta? ImportCurated(
            IEnumerable<IGameFamily> importedFamilies,
            out Dictionary<string, IGamePart?> errorReport)
        {
            if (overlay == null)
            {
                errorReport = new Dictionary<string, IGamePart?>();
                return null;
            }

            return overlay.ImportCurated(importedFamilies, out errorReport);
        }
        public void Teardown()
        {
            overlay = null;
        }
    }
}
