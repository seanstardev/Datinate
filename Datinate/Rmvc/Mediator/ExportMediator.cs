using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class ExportMediator : RMediator
    {
        private IExportView? view => (IExportView?)base.viewBase;
        public ExportMediator(Type view) : base(view)
        {
        }
        public void SetView(
            bool everyGameHasExactlyOnePart,
            ExportSoftwareOptionsDTO exportSoftwareOptions,
            IReadOnlyDictionary<DatinateEnums.MEDIA_TYPE_ENUM, IReadOnlyList<MediaExportPriorityItemDTO>> mediaPriorityDictionary,
            string exportSettingsMessage)
        {
            view?.SetView(
                everyGameHasExactlyOnePart,
                exportSoftwareOptions,
                mediaPriorityDictionary, 
                exportSettingsMessage);
        }

        private void OnBack()
        {
            base.ExecuteCommand(new SetProjectsViewCmd());
        }
        private void OnExportProject(
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaOptions,
            ExportSoftwareOptionsDTO softwareOptions,
            DAT_GROUPER_EXPORT_ENUM exportEnum)
        {
            base.ExecuteCommand(
                new ExportDatGrouperProjectCmd(
                    mediaOptions,
                    softwareOptions,
                    exportEnum));
        }

        protected override void Disposing()
        {
            if (view == null) return;

            view.BackEvt -= OnBack;
            view.ExportProjectEvt -= OnExportProject;
            view.SaveSettingsEvt -= OnSaveSettings;
        }

        private void OnSaveSettings(
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaExportEntries,
            ExportSoftwareOptionsDTO softwareExportOptions)
        {
            base.ExecuteCommand(new SaveDatGrouperExportSettingsCmd(
                mediaExportEntries,
                softwareExportOptions));
        }

        protected override void Initialsed()
        {
            if (view == null) return;

            view.BackEvt += OnBack;
            view.ExportProjectEvt += OnExportProject;
            view.SaveSettingsEvt += OnSaveSettings;
        }
    }
}
