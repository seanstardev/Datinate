using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class SaveDatGrouperExportSettingsCmd : RCommand
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

        protected override void Run()
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

            base.ExecuteCommand(new SaveProjectCmd(updatedProject, false));
        }
    }
}
