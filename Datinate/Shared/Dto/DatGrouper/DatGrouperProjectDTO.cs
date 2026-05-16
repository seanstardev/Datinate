using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class DatGrouperProjectDTO
    {
        public string ProjectName { get; }
        public IReadOnlyList<DatGrouperProjectEntry> SoftwareEntries { get; }
        public IReadOnlyList<DatGrouperProjectEntry> SoftwareIgnoreEntries { get; }
        public IReadOnlyList<DatGrouperProjectEntry> AuxEntries { get; }
        public string Comment { get; }
        public IReadOnlySet<string> ExcludedDescriptorCodes { get; }
        public IReadOnlySet<string> ScoringMediaTypes { get; }
        public ExportSoftwareOptionsDTO ExportSoftwareOptionsDTO { get; }
        public IReadOnlyList<DatGrouperMediaExportEntryDTO> MediaExports { get; }

        public DatGrouperProjectDTO(
            string projectNameWithoutExt,
            IReadOnlyList<DatGrouperProjectEntry> gameIncludeEntries,
            IReadOnlyList<DatGrouperProjectEntry> gameIgnoreEntries,
            IReadOnlyList<DatGrouperProjectEntry> auxIncludeEntries,
            string comment,
            IReadOnlySet<string> excludedDescriptorCodes,
            IReadOnlySet<string> scoringMediaTypes,
            ExportSoftwareOptionsDTO exportSoftwareOptionsDTO,
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaExports) 
        {
            ProjectName = projectNameWithoutExt;
            SoftwareEntries = gameIncludeEntries;
            SoftwareIgnoreEntries = gameIgnoreEntries;
            AuxEntries = auxIncludeEntries;

            Comment = comment;
            ExcludedDescriptorCodes = excludedDescriptorCodes;
            ScoringMediaTypes = scoringMediaTypes;
            ExportSoftwareOptionsDTO = exportSoftwareOptionsDTO;
            MediaExports = mediaExports;
        }

        public IReadOnlyList<DatGrouperProjectEntry> ResourceEntries => 
            AuxEntries.Where(e => e.CollectionSetEnum == COLLECTION_SET_ENUM.Resource).ToList();
        public IReadOnlyList<DatGrouperProjectEntry> MediaEntries =>
            AuxEntries.Where(e => e.CollectionSetEnum == COLLECTION_SET_ENUM.Media).ToList();

        public static DatGrouperProjectEntry[] GetAllProjectEntries(DatGrouperProjectDTO project)
            => project.SoftwareEntries
                .Concat(project.SoftwareIgnoreEntries)
                .Concat(project.AuxEntries)
                .ToArray();

        public static bool GetAllDatsExist(DatGrouperProjectDTO project)
        {
            var allEntries = GetAllProjectEntries(project);

            foreach (var entry in allEntries)
                if (string.IsNullOrWhiteSpace(entry.DatFullpath) ||
                    File.Exists(entry.DatFullpath) == false) 
                {
                    return false;
                }

            return true;
        }

        public static bool GetAllDatPathFilesExist(DatGrouperProjectDTO project)
        {
            var paths = GetAllProjectEntries(project);
            foreach(var path in paths)
            {
                if (!File.Exists(path.DatFullpath))
                {
                    return false;
                }
            }
            return true;
        }
        public bool GetAllPathsAreValidOrEmpty()
        {
            var paths = GetAllProjectEntries(this);
            foreach (var path in paths)
            {
                if (!string.IsNullOrWhiteSpace(path.ContentPath) && !Directory.Exists(path.ContentPath))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
