using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using RMVC;

namespace Datinate.Shared
{
    public interface IDatGrouperProjectSettingsView : IRContract
    {
        event Action<DatGrouperProjectDTO>? SaveEvt;
        event Action? BackEvt;
        
        event Action<int>? LoadingPercentEvt;
        event Action? ClearLoadingEvt;

        void ClearView();
        void SetView(DatGrouperProjectDTO project,
            IReadOnlySet<DescriptorDefinitionDTO> descriptors);
    }
}
