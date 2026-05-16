using RadioLibCore.RadioDat;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Shared
{
    public interface IDatGrouperControlsView : IRContract
    {
        event Action<bool, IGameEntity>? AssignMediaEvt;
        event Action<bool>? EnterMediaModeEvt;
        event Action? ExitMediaModeEvt;
        event Action<bool, string>? SearchEvt;
        event Action<bool, IGamePart>? ShowPartGroupingReportsEvt;

        event Action<bool>? HideAliasesEvt;
        event Action<bool>? ShowExcludedFamiliesEvt;
        
        void ClearView();
        
        void SetDatGrouperScreenLayout(DAT_GROUPER_LAYOUT_ENUM layout);

        void SetView(IGameEntity? entity, bool isFromAuto);

        void SetCompletionStats(long curatedParts, long totalParts);
    }
}
