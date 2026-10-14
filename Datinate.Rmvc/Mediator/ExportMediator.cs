using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Rmvc;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Mediator
{
    public class ExportMediator : RMediator<IExportView>
    {
        public void ClearView() => 
            actor?.ClearView();
        
        public void SetView(
            bool everyGameHasExactlyOnePart,
            ExportSoftwareOptionsDTO exportSoftwareOptions,
            IReadOnlyDictionary<
                DatinateEnums.MEDIA_TYPE_ENUM, 
                IReadOnlyList<MediaExportPriorityItemDTO>> mediaPriorityDictionary,
            string exportSettingsMessage)
            => actor?.SetView(
                everyGameHasExactlyOnePart,
                exportSoftwareOptions,
                mediaPriorityDictionary, 
                exportSettingsMessage);
        
        private void OnBack()
            => base.Execute(new SetDatGrouperFormActiveCmd());
        
        private void OnExportProject(
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaOptions,
            ExportSoftwareOptionsDTO softwareOptions,
            DAT_GROUPER_EXPORT_ENUM exportEnum)
            => base.Execute(new ExportDatGrouperProjectCmd(
                mediaOptions,
                softwareOptions,
                exportEnum));
        
        private void OnSaveSettings(
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaExportEntries,
            ExportSoftwareOptionsDTO softwareExportOptions)
            => base.Execute(new SaveDatGrouperExportSettingsCmd(
                mediaExportEntries,
                softwareExportOptions));
        
        protected override void Initialised()
        {
            if (actor == null) return;
            base.WithActor((actor) => { });
            actor.BackEvt += OnBack;
            actor.ExportProjectEvt += OnExportProject;
            actor.SaveSettingsEvt += OnSaveSettings;
        }

        protected override void Disposing()
        {
            if (actor == null) return;

            actor.BackEvt -= OnBack;
            actor.ExportProjectEvt -= OnExportProject;
            actor.SaveSettingsEvt -= OnSaveSettings;
        }
    }
}
