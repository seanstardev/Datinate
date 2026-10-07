using Datinate.Shared.Radio;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Shared.Rmvc
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
