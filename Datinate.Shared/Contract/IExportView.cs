using Datinate.Shared.DatGrouper;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Shared.Rmvc
{
    public interface IExportView : IRContract
    {
        event Action? BackEvt;
        
        event Action<IReadOnlyList<DatGrouperMediaExportEntryDTO>, ExportSoftwareOptionsDTO, DAT_GROUPER_EXPORT_ENUM>? ExportProjectEvt;
        
        event Action<IReadOnlyList<DatGrouperMediaExportEntryDTO>, ExportSoftwareOptionsDTO>? SaveSettingsEvt;

        void ClearView();
        void SetView(
            bool everyGameHasExactlyOnePart,
            ExportSoftwareOptionsDTO exportSoftwareOptions,
            IReadOnlyDictionary<DatinateEnums.MEDIA_TYPE_ENUM, IReadOnlyList<MediaExportPriorityItemDTO>> exportMediaDictionary,
            string exportSettingsMessage);

    }
}
