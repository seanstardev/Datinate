using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SaveDatGrouperExportSettingsCmd : CommandBase<RVoid>
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

        protected override async Task<RVoid> Run()
        {
            if (base.RadioDatModel?.ActiveProject is not { } project ||
                base.ProjectProxy is not { } projectProxy)
            {
                return await Complete();
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

            await base.Execute(new SaveProjectCmd(updatedProject));

            return await Complete();
        }
    }
}
