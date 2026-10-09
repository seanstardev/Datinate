using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SaveDatGrouperExportSettingsCmd : RCommandAsync
    {
        private IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaExportEntries;
        private readonly ExportSoftwareOptionsDTO softwareExportOptions;

        public SaveDatGrouperExportSettingsCmd(
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaExportEntries, 
            ExportSoftwareOptionsDTO softwareExportOptions)
        {
            this.mediaExportEntries = mediaExportEntries;
            this.softwareExportOptions = softwareExportOptions;
        }

        protected override async Task RunAsync()
        {
            if (Facade.Instance?.RadioDatModel?.ActiveProject is not { } project ||
                Facade.Instance?.ProjectProxy is not { } projectProxy)
            {
                return;
            }
            var updatedProject = new DatGrouperProjectDTO(
                project.ProjectName,
                project.SoftwareEntries,
                project.SoftwareIgnoreEntries,
                project.AuxEntries,
                project.Comment,
                project.ExcludedDescriptorCodes,
                project.ScoringMediaTypes,
                softwareExportOptions,
                mediaExportEntries);

            await base.ExecuteCommandAsync(new SaveProjectCmd(updatedProject));
        }
    }
}
