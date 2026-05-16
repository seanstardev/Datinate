namespace Datinate.Rmvc.Proxy.delegates
{
    public class RadioSession
    {
        public string ProjectPath { get; private set; }

        private const string sessionFileName = "session";
        private const string fallbackSessionFileFullpath = @"C:/RADIO/session";

        private string radioRootWorkingPath;

        public RadioSession(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException($"{nameof(RadioSession)} requires a valid {nameof(projectName)}.", nameof(projectName));

            var resolved = ResolveRadioRootWorkingPath();

            radioRootWorkingPath = resolved.Path;

            Directory.CreateDirectory(radioRootWorkingPath);

            ProjectPath = resolved.FromSessionFile
                ? Path.Combine(radioRootWorkingPath, projectName)
                : radioRootWorkingPath;

            Directory.CreateDirectory(ProjectPath);
        }

        private static (string Path, bool FromSessionFile) ResolveRadioRootWorkingPath()
        {
            string exeFolder = GetExeFolder();
            string exeSessionFileFullpath = Path.Combine(exeFolder, sessionFileName);

            string? configuredPath = ReadSessionPathOrNull(exeSessionFileFullpath);

            if (!string.IsNullOrWhiteSpace(configuredPath))
                return (configuredPath, true);

            configuredPath = ReadSessionPathOrNull(fallbackSessionFileFullpath);

            if (!string.IsNullOrWhiteSpace(configuredPath))
                return (configuredPath, true);

            return (exeFolder, false);
        }

        private static string GetExeFolder()
        {
            string? exeFullpath = Environment.ProcessPath;

            if (!string.IsNullOrWhiteSpace(exeFullpath))
            {
                string? exeFolder = Path.GetDirectoryName(exeFullpath);

                if (!string.IsNullOrWhiteSpace(exeFolder))
                    return exeFolder;
            }

            return AppContext.BaseDirectory;
        }

        private static string? ReadSessionPathOrNull(string sessionFileFullpath)
        {
            try
            {
                if (!File.Exists(sessionFileFullpath))
                    return null;

                string path = File.ReadAllText(sessionFileFullpath).Trim();

                if (string.IsNullOrWhiteSpace(path))
                    return null;

                return path;
            }
            catch
            {
                return null;
            }
        }
    }
}