using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Shared
{
    public interface IExportView : IRContract
    {
        event Action? BackEvt;
        
        event Action<IReadOnlyList<DatGrouperMediaExportEntryDTO>, ExportSoftwareOptionsDTO, DAT_GROUPER_EXPORT_ENUM>? ExportProjectEvt;
        
        event Action<IReadOnlyList<DatGrouperMediaExportEntryDTO>, ExportSoftwareOptionsDTO>? SaveSettingsEvt;

        void SetView(
            bool everyGameHasExactlyOnePart,
            ExportSoftwareOptionsDTO exportSoftwareOptions,
            IReadOnlyDictionary<DatinateEnums.MEDIA_TYPE_ENUM, IReadOnlyList<MediaExportPriorityItemDTO>> exportMediaDictionary,
            string exportSettingsMessage);

    }
}
