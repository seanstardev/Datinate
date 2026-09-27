using RMVC;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;

namespace com.RADIO.Datinate.RMVC
{
    public class AudioProxy : IRModel
    {
        private const string SeedPrefix = "Seed/VGM/";

        private string? audioEnvironmentPath = null;

        public void SetProjectRootPath(string projectRootPath)
        {
            audioEnvironmentPath = Path.Combine(projectRootPath, "VGM");
            EnsureAudioProjects();
        }

        public string AudioEnvironmentPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(audioEnvironmentPath))
                    throw new InvalidOperationException(
                        "Project root path not set. Call SetProjectRootPath() first.");

                return audioEnvironmentPath;
            }
        }

        private void EnsureAudioProjects()
        {
            Directory.CreateDirectory(AudioEnvironmentPath);

            Assembly assembly = typeof(AudioProxy).Assembly;

            string[] resources = assembly
                .GetManifestResourceNames()
                .Where(name =>
                    name.StartsWith(
                        SeedPrefix,
                        StringComparison.Ordinal))
                .ToArray();

            if (resources.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No embedded audio seed resources were found under '{SeedPrefix}'.");
            }

            int created = 0;
            int updated = 0;
            int unchanged = 0;

            Stopwatch timer = Stopwatch.StartNew();

            foreach (string resourceName in resources)
            {
                string relativePath =
                    resourceName
                        .Substring(SeedPrefix.Length)
                        .Replace('/', Path.DirectorySeparatorChar);

                if (string.IsNullOrWhiteSpace(relativePath))
                    continue;

                string destinationPath =
                    Path.GetFullPath(
                        Path.Combine(
                            AudioEnvironmentPath,
                            relativePath));

                string environmentRoot =
                    Path.GetFullPath(AudioEnvironmentPath) +
                    Path.DirectorySeparatorChar;

                if (!destinationPath.StartsWith(
                        environmentRoot,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Invalid embedded audio seed path: '{resourceName}'.");
                }

                string? destinationFolder =
                    Path.GetDirectoryName(destinationPath);

                if (!string.IsNullOrWhiteSpace(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);

                using Stream sourceStream =
                    assembly.GetManifestResourceStream(resourceName) ??
                    throw new InvalidOperationException(
                        $"Cannot open embedded audio seed resource '{resourceName}'.");

                if (!File.Exists(destinationPath))
                {
                    WriteResource(
                        sourceStream,
                        destinationPath);

                    created++;
                    continue;
                }

                var destinationInfo =
                    new FileInfo(destinationPath);

                if (destinationInfo.Length != sourceStream.Length)
                {
                    WriteResource(
                        sourceStream,
                        destinationPath);

                    updated++;
                    continue;
                }

                byte[] sourceHash =
                    SHA256.HashData(sourceStream);

                byte[] destinationHash;

                using (var destinationStream =
                    new FileStream(
                        destinationPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        64 * 1024,
                        FileOptions.SequentialScan))
                {
                    destinationHash =
                        SHA256.HashData(destinationStream);
                }

                if (sourceHash.AsSpan().SequenceEqual(destinationHash))
                {
                    unchanged++;
                    continue;
                }

                if (!sourceStream.CanSeek)
                {
                    using Stream freshSource =
                        assembly.GetManifestResourceStream(resourceName) ??
                        throw new InvalidOperationException(
                            $"Cannot reopen embedded audio seed resource '{resourceName}'.");

                    WriteResource(
                        freshSource,
                        destinationPath);
                }
                else
                {
                    sourceStream.Position = 0;

                    WriteResource(
                        sourceStream,
                        destinationPath);
                }

                updated++;
            }

            timer.Stop();

            Debug.WriteLine(
                $"[AUDIO SEED] Sync complete in {timer.ElapsedMilliseconds}ms. " +
                $"Created={created}, Updated={updated}, Unchanged={unchanged}");
        }

        private static void WriteResource(
            Stream source,
            string destinationPath)
        {
            string temporaryPath =
                destinationPath +
                ".datinate-" +
                Guid.NewGuid().ToString("N") +
                ".tmp";

            try
            {
                using (var destination =
                    new FileStream(
                        temporaryPath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        64 * 1024,
                        FileOptions.SequentialScan))
                {
                    source.CopyTo(destination);
                    destination.Flush(true);
                }

                File.Move(
                    temporaryPath,
                    destinationPath,
                    true);
            }
            finally
            {
                try
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
                catch
                {
                }
            }
        }
    }
}