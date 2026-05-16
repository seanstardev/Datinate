using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using Datinate.Shared.Rb;
using RadioLibCore.RadioDat;
using RMVC;

namespace Datinate.Shared
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
        void SetDescriptorDefinitions(IReadOnlySet<DescriptorDefinitionDTO> descriptorDefinitions);
        void SetMediaCardDragStart();
        void SetMediaCardDragStop();
    }
}
