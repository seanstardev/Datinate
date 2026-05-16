using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Rb;
using RadioLibCore.RadioDat;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class ExportDatGrouperProjectProxy : RModel
    {
        private const string M3uExportFolder = "Playlist";

        private string? projectsPath = null;

        public void SetProjectRootPath(string projectRootPath)
        {
            projectsPath = Path.Combine(projectRootPath, "Export");
            Directory.CreateDirectory(projectsPath);
        }

        public bool GetSoftwareProjectPathExists(DatGrouperProjectDTO activeProject)
        {
            string? projectPath = GetSoftwareProjectPath(activeProject);

            if (string.IsNullOrWhiteSpace(projectPath))
                return false;

            return Directory.Exists(projectPath);
        }

        public bool GetMediaProjectPathExists(DatGrouperProjectDTO activeProject)
        {
            string? projectPath = GetMediaProjectPath(activeProject);

            if (string.IsNullOrWhiteSpace(projectPath))
                return false;

            return Directory.Exists(projectPath);
        }

        public string? GetSoftwareProjectPath(DatGrouperProjectDTO activeProject)
        {
            if (activeProject == null || string.IsNullOrWhiteSpace(activeProject.ProjectName))
                return null;

            return GetSoftwareProjectPath(activeProject.ProjectName);
        }

        public string? GetMediaProjectPath(DatGrouperProjectDTO activeProject)
        {
            if (activeProject == null || string.IsNullOrWhiteSpace(activeProject.ProjectName))
                return null;

            return GetMediaProjectPath(activeProject.ProjectName);
        }

        /*
         * NOTE: Requirement: family.GetFamilyDisplayName() must be guaranteed path-safe at this entry point.
         */
        public void ExportSoftware(
            IReadOnlyList<IGameFamily> curatedFamilies,
            Dictionary<IGamePart, DatGameVO> partDatEntryDictionary,
            DatGrouperProjectDTO project,
            ExportSoftwareOptionsDTO softwareOptions,
            IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> familyMediaDictionary,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary)
        {
            Dictionary<IGameFamily, string> familyUniqueNameDictionary =
                BuildUniqueFamilyNames(curatedFamilies);

            string projectName = project.ProjectName;

            string? softwareProjectPath = GetSoftwareProjectPath(projectName);

            if (string.IsNullOrWhiteSpace(softwareProjectPath))
                return;

            DeleteExportSoftwareProject(projectName);

            ExportSoftwareDelegate softwareDelegate = new ExportSoftwareDelegate();

            softwareDelegate.Export(
                curatedFamilies,
                familyUniqueNameDictionary,
                partDatEntryDictionary,
                project,
                softwareOptions,
                familyMediaDictionary,
                flagFilterSetByGroup,
                softwareIdDatGroupEnumDictionary,
                softwareProjectPath,
                M3uExportFolder);
        }

        /*
         * NOTE: Requirement: family.GetFamilyDisplayName() must be guaranteed path-safe at this entry point.
         */
        public void ExportMedia(
            IReadOnlyList<IGameFamily> curatedFamilies,
            DatGrouperProjectDTO project,
            ExportSoftwareOptionsDTO softwareOptions,
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaPriorities,
            IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> familyMediaDictionary,
            IReadOnlyDictionary<IGameFamily, IReadOnlyDictionary<string, ResourceDetailsDTO>> resourceDetailsDictionary,
            IReadOnlyDictionary<string, DatVO> sourceIdDatDictionary,
            IReadOnlyDictionary<string, ISourceDefinition> sourceIdContentDictionary)
        {
            Dictionary<IGameFamily, string> familyUniqueNameDictionary =
                BuildUniqueFamilyNames(curatedFamilies);

            string projectName = project.ProjectName;

            string? mediaProjectPath = GetMediaProjectPath(projectName);

            if (string.IsNullOrWhiteSpace(mediaProjectPath))
                return;

            DeleteExportMediaProject(projectName);

            var exportInfoDelegate = new ExportInfoDelegate();

            IReadOnlyDictionary<IGameFamily, InfoSpec> infoSpecDictionary =
                exportInfoDelegate.Export(
                    curatedFamilies,
                    familyUniqueNameDictionary,
                    project,
                    mediaPriorities,
                    familyMediaDictionary,
                    resourceDetailsDictionary,
                    Path.Combine(mediaProjectPath, "Info"));

            string mediaDatName = CreateMediaDatName(projectName, softwareOptions);

            var exportMediaDatDelegate = new ExportMediaDatDelegate();

            exportMediaDatDelegate.Export(
                curatedFamilies,
                familyUniqueNameDictionary,
                project,
                mediaPriorities,
                familyMediaDictionary,
                resourceDetailsDictionary,
                infoSpecDictionary,
                sourceIdDatDictionary,
                sourceIdContentDictionary,
                mediaDatName,
                mediaProjectPath);
        }

        private static string CreateMediaDatName(
            string projectName,
            ExportSoftwareOptionsDTO softwareOptions)
        {
            return softwareOptions.ExportAs1G1R
                ? projectName + " [1G1R][Media]"
                : projectName + " [Media]";
        }
        private Dictionary<IGameFamily, string> BuildUniqueFamilyNames(IReadOnlyList<IGameFamily> curatedFamilies)
        {
            Dictionary<IGameFamily, string> familyUniqueNameDictionary = new();
            HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);

            foreach (var family in curatedFamilies)
            {
                string baseName = family.GetFamilyDisplayName();
                string uniqueName = baseName;

                if (usedNames.Contains(uniqueName))
                {
                    int index = 1;

                    do
                    {
                        uniqueName = $"{baseName} [{index}]";
                        index++;
                    }
                    while (usedNames.Contains(uniqueName));
                }

                usedNames.Add(uniqueName);
                familyUniqueNameDictionary.Add(family, uniqueName);
            }

            return familyUniqueNameDictionary;
        }

        private string? GetSoftwareProjectPath(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectsPath) || string.IsNullOrWhiteSpace(projectName))
                return null;

            return Path.Combine(ProjectsPath, projectName, "Software");
        }

        private string? GetMediaProjectPath(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectsPath) || string.IsNullOrWhiteSpace(projectName))
                return null;

            return Path.Combine(ProjectsPath, projectName, "Media");
        }

        private void DeleteExportSoftwareProject(string projectName)
        {
            var projectPath = GetSoftwareProjectPath(projectName);
            if (projectPath == null) return;

            if (Directory.Exists(projectPath))
                Directory.Delete(projectPath, true);
        }

        private void DeleteExportMediaProject(string projectName)
        {
            var projectPath = GetMediaProjectPath(projectName);
            if (projectPath == null) return;

            if (Directory.Exists(projectPath))
                Directory.Delete(projectPath, true);
        }

        private string ProjectsPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(projectsPath))
                    throw new InvalidOperationException("Project root path not set. Call SetProjectRootPath() first.");

                return projectsPath;
            }
        }

        protected override void Initialise()
        {

        }
    }
}
