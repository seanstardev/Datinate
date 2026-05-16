using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Rmvc.Proxy.delegates;
using Datinate.Shared.Rb;
using RadioLibCore.RadioDat;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class CuratedDatProxy : RModel
    {
        private string? projectsPath = null;
        private string ProjectsPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(projectsPath))
                    throw new InvalidOperationException("Project root path not set. Call SetProjectRootPath() first.");

                return projectsPath;
            }
        }

        public bool GetDatExists(string projectName)
        {
            return File.Exists(GetDatFullpath(projectName));
        }


        public IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> LoadCurated(
            string projectName,
            IReadOnlySet<string> descriptorDefinitions,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            IReadOnlyDictionary<string, ISourceDefinition> sourceIdContentDictionary)
        {
            if (!GetDatExists(projectName))
                return new Dictionary<IGameFamily, IMediaCollectionImportExport?>();

            RadioDatImporterDelegate importer = new RadioDatImporterDelegate(
                GetDatFullpath(projectName),
                descriptorDefinitions,
                flagFilterSetByGroup,
                sourceIdContentDictionary);

            return importer.Import();
        }

        public void SetProjectRootPath(string projectRootPath)
        {
            projectsPath = Path.Combine(projectRootPath, "Curate");
            Directory.CreateDirectory(projectsPath);
        }
        public void SaveCurated(
            string radioDatNameWithoutExt,
            IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> collection,
            IReadOnlyDictionary<string, ISourceDefinition> sourceIdContentDictionary,
            IReadOnlyDictionary<string, DatVO> sourceIdDatDictionary,
            RadioDatMeta datMeta)
        {
            SaveFileDialog f = new SaveFileDialog();
            f.InitialDirectory = projectsPath;
            f.FileName = radioDatNameWithoutExt + ".xml";

            if (f.ShowDialog() == DialogResult.OK)
            {
                radioDatNameWithoutExt = Path.GetFileNameWithoutExtension(f.FileName);

                var exportDelegate = new RadioDatExporterDelegate(
                    radioDatNameWithoutExt,
                    collection,
                    sourceIdContentDictionary,
                    sourceIdDatDictionary,
                    datMeta,
                    Path.GetDirectoryName(f.FileName)!);

                exportDelegate.Export();


                MessageBox.Show(
                    "The RADIO DAT '" + radioDatNameWithoutExt + ".xml" + "' has been Saved."
                    , "OK"
                    , MessageBoxButtons.OK
                    , MessageBoxIcon.Information
                );
            }
        }
        private string GetDatFullpath(string projectName)
        {
            return Path.Combine(ProjectsPath, projectName + ".xml");
        }
        protected override void Initialise()
        {

        }
    }
}
