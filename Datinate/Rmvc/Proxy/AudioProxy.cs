using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class AudioProxy : IRModel
    {
        private string? projectsPath = null;
        public void SetProjectRootPath(string projectRootPath)
        {
            projectsPath = Path.Combine(projectRootPath, "VGM");
            Directory.CreateDirectory(projectsPath);
        }
        public string AudioEnvironmentPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(projectsPath))
                    throw new InvalidOperationException("Project root path not set. Call SetProjectRootPath() first.");

                return projectsPath;
            }
        }
    }
}
