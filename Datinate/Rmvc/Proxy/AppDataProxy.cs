using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Rmvc.Proxy.delegates;
using RMVC;
using System.Reflection;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class AppDataProxy : RModel
    {
        public IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> FlagFilterSetByGroup { get; private set; }
        public IReadOnlyList<R2DatResourceDTO> R2DatResources { get; private set; } = Array.Empty<R2DatResourceDTO>();
        private const string embeddedPrefix = "AppData/";

        private const string R2DatResourceFilename = "R2DatResource.xml";
        private const string R2DatResourceEmbeddedFilename = @"Config/R2DatResource.xml";
        private readonly Assembly assembly;
        

        private string? projectsPath;

        public AppDataProxy()
        {
            assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            FlagFilterSetByGroup = new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>();
        }


        public void SetProjectRootPath(string projectRootPath)
        {
            this.projectsPath = projectRootPath;
            
            Directory.CreateDirectory(Path.Combine(projectRootPath, "Flag"));

            EnsureExportedMissingFiles(DatinateHelper.IsDebugBuild);
            FlagFilterSetByGroup = BuildFlagFilterSetByGroupFromAppData();
        }

        protected override void Initialise()
        {

        }

        private IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> BuildFlagFilterSetByGroupFromAppData()
        {
            var d = new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>();

            AddFlagSet(
                d,
                DAT_GROUP_ENUM.NO_INTRO,
                partFile: null,
                regionFile: @"Flag\FlagsNoIntroRegion.txt",
                languageFile: null,
                familyFile: null);

            AddFlagSet(
                d,
                new[] { DAT_GROUP_ENUM.TOSEC, DAT_GROUP_ENUM.TOSEC_ISO, DAT_GROUP_ENUM.TOSEC_PIX },
                partFile: @"Flag\FlagsTosecPart.txt",
                regionFile: null,
                languageFile: null,
                familyFile: null);

            AddFlagSet(
                d,
                DAT_GROUP_ENUM.REDUMP,
                partFile: @"Flag\FlagsRedumpPart.txt",
                regionFile: @"Flag\FlagsRedumpRegion.txt",
                languageFile: @"Flag\FlagsRedumpLanguage.txt",
                familyFile: @"Flag\FlagsRedumpFamily.txt");

            AddFlagSet(
                d,
                DAT_GROUP_ENUM.T_EN,
                partFile: @"Flag\FlagsTenPart.txt",
                regionFile: null,
                languageFile: null,
                familyFile: @"Flag\FlagsTenFamily.txt");

            return d;
        }
        private void AddFlagSet(
            Dictionary<DAT_GROUP_ENUM, FlagFilterSet> d,
            IEnumerable<DAT_GROUP_ENUM> groups,
            string? partFile,
            string? regionFile,
            string? languageFile,
            string? familyFile)
        {

            var groupArr = groups.Distinct().ToArray();

            var set = new FlagFilterSet(
                groupArr,
                LoadFlagsOrEmpty(partFile),
                LoadFlagsOrEmpty(regionFile),
                LoadFlagsOrEmpty(languageFile),
                LoadFlagsOrEmpty(familyFile));

            for (var i = 0; i < groupArr.Length; i++)
                d[groupArr[i]] = set;
        }

        private void AddFlagSet(
            Dictionary<DAT_GROUP_ENUM, FlagFilterSet> d,
            DAT_GROUP_ENUM group,
            string? partFile,
            string? regionFile,
            string? languageFile,
            string? familyFile)
        {
            AddFlagSet(d, new[] { group }, partFile, regionFile, languageFile, familyFile);
        }

        private HashSet<string> LoadFlagsOrEmpty(string? relativePath)
        {
            var result = new HashSet<string>();

            if (string.IsNullOrWhiteSpace(relativePath))
                return result;

            var fullPath = GetSafeFullPath(relativePath);

            if (!File.Exists(fullPath))
            {
                ExportSingleIfPresent(relativePath);
                if (!File.Exists(fullPath))
                    return result;
            }

            var content = File.ReadAllText(fullPath);

            return TextLineParser.Parse(content)
                .Distinct(StringComparer.Ordinal)
                .ToHashSet<string>();
        }
        private void EnsureExportedMissingFiles(bool overwrite)
        {
            if (string.IsNullOrWhiteSpace(projectsPath))
                return;

            Directory.CreateDirectory(projectsPath);

            var resources = assembly.GetManifestResourceNames()
                .Where(n => n.StartsWith(embeddedPrefix, StringComparison.Ordinal))
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToArray();

            if (resources.Length == 0)
                throw new InvalidOperationException($"No embedded resources found under prefix '{embeddedPrefix}' in assembly '{assembly.FullName}'.");

            var r2DatResourcePath = GetSafeFullPath(R2DatResourceFilename);
            var r2DatResourceEmbeddedRel = NormaliseRelativePath(R2DatResourceEmbeddedFilename);

            var r2DatResourceResName = resources.FirstOrDefault(resName =>
            {
                var relativePath = resName.Substring(embeddedPrefix.Length);
                return string.Equals(
                    NormaliseRelativePath(relativePath),
                    r2DatResourceEmbeddedRel,
                    StringComparison.OrdinalIgnoreCase);
            });

            if (!File.Exists(r2DatResourcePath) && !string.IsNullOrWhiteSpace(r2DatResourceResName))
                ExportResourceToDisk(r2DatResourceResName, r2DatResourcePath);

            R2DatResources = R2DatResourceDelegate.LoadOrEmpty(r2DatResourcePath);

            foreach (var resName in resources)
            {
                var relativePath = resName.Substring(embeddedPrefix.Length);
                if (string.IsNullOrWhiteSpace(relativePath))
                    continue;

                if (string.Equals(NormaliseRelativePath(relativePath), r2DatResourceEmbeddedRel, StringComparison.OrdinalIgnoreCase))
                    continue;

                var outPath = GetSafeFullPath(relativePath);

                if (!overwrite && File.Exists(outPath))
                    continue;

                ExportResourceToDisk(resName, outPath);
            }
        }
        private void ExportSingleIfPresent(string relativePath)
        {
            ExportSingleIfPresent(relativePath, relativePath);
        }

        private void ExportSingleIfPresent(string resourceRelativePath, string outputRelativePath)
        {
            if (string.IsNullOrWhiteSpace(projectsPath))
                return;

            Directory.CreateDirectory(projectsPath);

            var safeResourceRel = NormaliseRelativePath(resourceRelativePath);
            var resourceRel = safeResourceRel.Replace(Path.DirectorySeparatorChar, '/');
            var resName = embeddedPrefix + resourceRel;



            var outPath = GetSafeFullPath(outputRelativePath);

            if (File.Exists(outPath))
                return;

            ExportResourceToDisk(resName, outPath);
        }
        private void ExportResourceToDisk(string resourceName, string outPath)
        {
            var dir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var tmpPath = outPath + ".tmp_" + Guid.NewGuid().ToString("N");

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException($"Embedded resource stream missing: '{resourceName}'.");

                using var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                stream.CopyTo(fs);
            }

            try
            {
                File.Move(tmpPath, outPath);
            }
            catch (IOException)
            {
                if (File.Exists(tmpPath))
                    File.Delete(tmpPath);
            }
        }

        private string GetSafeFullPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(projectsPath))
                return relativePath;

            var safeRel = NormaliseRelativePath(relativePath);

            var full = Path.GetFullPath(Path.Combine(projectsPath, safeRel));
            var rootFull = Path.GetFullPath(projectsPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);

            if (!full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid relativePath (path traversal).");

            return full;
        }

        private static string NormaliseRelativePath(string relativePath)
        {
            var s = relativePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

            while (s.StartsWith(Path.DirectorySeparatorChar))
                s = s.Substring(1);

            return s;
        }
    }
}
