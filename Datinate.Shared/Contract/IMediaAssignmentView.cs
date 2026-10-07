using Datinate.Shared.DatGrouper;
using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Shared.Rmvc
{
    public interface IMediaAssignmentView : IRContract
    {
        event Action<IGameFamily, HashSet<string>, string, bool>? MediaMetaUpdateEvt;
        event Action? LoadMediaInBrowserEvt;
        void ClearView();
        void SetView(IMediaCollection mediaCollection);
        void SetViewMediaItem(string entryName, RadioSourceDTO radioSource);
        void SetReadOnlyModeActive(bool readOnlyMode);
        void SetScoring(DatGrouperScoring scoring);
        void SetScoringActive(bool active);
        void SetDescriptorDefinitions(IReadOnlySet20<DescriptorDefinitionDTO> descriptorDefinitions);
        void SetMediaCardDragStart();
        void SetMediaCardDragStop();
    }
}
