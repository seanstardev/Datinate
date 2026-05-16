using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using RadioLibCore.RadioDat;
using RMVC;
using static app.datinate.DatGrouperEditDelta;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Shared
{
    public interface IDatGrouperView : IRContract
    {
        event Action? InitialisedEvt;

        event Action<IGameEntity?, bool>? GameEntitySelectedEvt;
        event Action<DAT_GROUPER_ACTION_ENUM, IGameEntity?>? DatGrouperActionEvt;
        event Action<DatGrouperEntryDTO>? GameEntityDragStartEvt;
        event Action? GameEntityDragStopEvt;
        
        event Action<DatGrouperEntryDTO>? ShowGameMediaEvt;
        event Action<string>? SearchGameNameEvt;

        event Action? ToggleStandardLayoutEvt;

        event Action? SaveEvt;
        event Action? BackEvt;
        event Action? CurateEvt;
        event Action? ExportEvt;
        event Action? ConfigureEvt;

        event Action? ExitMediaEvt;

        event Action<DatGrouperEditRequestDTO>? EditRequestEvt;

        void SetLocalProgress(int parts, int total, string message);
        void ClearLocalProgress();
        void SetScreenLayout(DAT_GROUPER_LAYOUT_ENUM layoutEnum);
        void ResetView();
        void SetAutoView(
            IGameFamily[] families,
            string projectName,
            Dictionary<string, CurationPartReport> partFingerprintReportDic);

        void SetCuratedView(
            IGameFamily[] families,
            string projectName);

        void SetMediaCache(IReadOnlyDictionary<IGameFamily, IMediaCollection> mediaCache);
        void SetRenderAliases(bool doRender);
        void SetExcludeFamiliesVisible(bool doExclude);

        void UpdateCuratedFamilies(
            DELTA_NATURE_ENUM deltaNatureEnum,
            IReadOnlyList<IGameFamily> curatedFamiliesToAdd, 
            IReadOnlyList<IGameFamily> curatedFamiliesToRemove,
            int undoCount,
            int redoCount,
            IReadOnlySet<IGameEntity> affectedEntities);

        void UpdateAutoFamilies(
            DELTA_NATURE_ENUM deltaNatureEnum,
            IReadOnlyList<IGameFamily> autoFamiliesToAdd, 
            IReadOnlyList<IGameFamily> autoFamiliesToRemove, 
            IReadOnlySet<IGamePart> allCuratedAutoParts,
            int undoCount,
            int redoCount,
            IReadOnlySet<IGameEntity> affectedEntities);

        void SetMediaMode(DatinateEnums.DAT_GROUPER_LAYOUT_ENUM layoutEnum);
    }
}
