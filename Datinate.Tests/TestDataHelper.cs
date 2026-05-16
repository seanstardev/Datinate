namespace Datinate.Tests
{
    internal static class TestDataHelper
    {
        public static string GetMameSLDat(string filenameWithoutExt)
        {
            return Path.Combine(GetTestDataPath("DATs/MAME-SL/" + filenameWithoutExt + ".xml"));
        }
        public static string GetTestDataPath(string fileName)
        {
            var baseDir = AppContext.BaseDirectory;
            return Path.Combine(baseDir, "TestData", fileName);
        }
    }
}
