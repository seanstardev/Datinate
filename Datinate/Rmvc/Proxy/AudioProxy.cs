using RMVC;
using System.Diagnostics;
using System.IO.Compression;
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

        public bool IsVgmEngineInstalled
        {
            get
            {
                if (string.IsNullOrWhiteSpace(audioEnvironmentPath))
                    return false;

                string extensionPath =
                    Path.Combine(
                        audioEnvironmentPath,
                        "vgmplay",
                        "extension");

                if (!Directory.Exists(extensionPath))
                    return false;

                return Directory
                    .EnumerateFiles(
                        extensionPath,
                        "*",
                        SearchOption.AllDirectories)
                    .Any();
            }
        }

        public async Task<bool> InstallOrUpdateVgmEngine(
            string vgmEngineUrl,
            Action<int, int, string>? callback = null)
        {
            if (string.IsNullOrWhiteSpace(vgmEngineUrl))
                return false;

            string tempRoot =
                Path.Combine(
                    Path.GetTempPath(),
                    "Datinate",
                    "VGM-" + Guid.NewGuid().ToString("N"));

            string zipPath =
                Path.Combine(tempRoot, "extension.zip");

            try
            {
                // ------------------------------------------------------------
                // 1. Download
                // ------------------------------------------------------------

                callback?.Invoke(0, 4, "Downloading VGMPlay-JS-2...");

                Directory.CreateDirectory(tempRoot);

                using (var httpClient = new HttpClient())
                using (HttpResponseMessage response =
                    await httpClient.GetAsync(
                        vgmEngineUrl,
                        HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    await using Stream source =
                        await response.Content.ReadAsStreamAsync();

                    await using var destination =
                        new FileStream(
                            zipPath,
                            FileMode.Create,
                            FileAccess.Write,
                            FileShare.None,
                            64 * 1024,
                            true);

                    await source.CopyToAsync(destination);
                }

                // ------------------------------------------------------------
                // 2. Verify ZIP
                // ------------------------------------------------------------

                callback?.Invoke(1, 4, "Verifying downloaded package...");

                if (!File.Exists(zipPath))
                    return false;

                var zipInfo = new FileInfo(zipPath);

                if (zipInfo.Length == 0)
                    return false;

                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    if (archive.Entries.Count == 0)
                        return false;

                    // The VGMPlay package must contain the extension directory
                    // and at least one file within it.
                    bool hasExtensionContent =
                        archive.Entries.Any(entry =>
                            !string.IsNullOrEmpty(entry.Name) &&
                            entry.FullName.StartsWith(
                                "extension/",
                                StringComparison.OrdinalIgnoreCase));

                    if (!hasExtensionContent)
                        return false;

                    // Validate every archive path before touching the current
                    // VGMPlay installation.
                    string validationRoot =
                        Path.GetFullPath(
                            Path.Combine(
                                tempRoot,
                                "validation")) +
                        Path.DirectorySeparatorChar;

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destinationPath =
                            Path.GetFullPath(
                                Path.Combine(
                                    validationRoot,
                                    entry.FullName));

                        if (!destinationPath.StartsWith(
                                validationRoot,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                }

                // ------------------------------------------------------------
                // 3. Remove existing VGMPlay installation
                // ------------------------------------------------------------

                callback?.Invoke(2, 4, "Preparing VGMPlay-JS-2 installation...");

                string vgmplayPath =
                    Path.Combine(
                        AudioEnvironmentPath,
                        "vgmplay");

                // Only VGMPlay is replaced. Everything else in the VGM
                // environment belongs to Datinate and must remain untouched.
                if (Directory.Exists(vgmplayPath))
                    Directory.Delete(vgmplayPath, true);

                Directory.CreateDirectory(vgmplayPath);

                // ------------------------------------------------------------
                // 4. Extract directly into VGM/vgmplay
                // ------------------------------------------------------------

                callback?.Invoke(3, 4, "Installing VGMPlay-JS-2...");

                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    string destinationRoot =
                        Path.GetFullPath(vgmplayPath) +
                        Path.DirectorySeparatorChar;

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destinationPath =
                            Path.GetFullPath(
                                Path.Combine(
                                    vgmplayPath,
                                    entry.FullName));

                        // Defence in depth. We already checked this before
                        // deleting the existing installation.
                        if (!destinationPath.StartsWith(
                                destinationRoot,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidDataException(
                                $"Invalid archive path: '{entry.FullName}'.");
                        }

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(destinationPath);
                            continue;
                        }

                        string? destinationDirectory =
                            Path.GetDirectoryName(destinationPath);

                        if (!string.IsNullOrWhiteSpace(destinationDirectory))
                            Directory.CreateDirectory(destinationDirectory);

                        entry.ExtractToFile(
                            destinationPath,
                            true);
                    }
                }

                // ------------------------------------------------------------
                // 5. Final sanity check
                // ------------------------------------------------------------

                string installedExtensionPath =
                    Path.Combine(
                        vgmplayPath,
                        "extension");

                if (!Directory.Exists(installedExtensionPath))
                    return false;

                if (!Directory
                        .EnumerateFiles(
                            installedExtensionPath,
                            "*",
                            SearchOption.AllDirectories)
                        .Any())
                {
                    return false;
                }

                callback?.Invoke(
                    4,
                    4,
                    "VGMPlay-JS-2 installation complete.");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[VGM INSTALL] Installation failed: {ex}");

                return false;
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempRoot))
                        Directory.Delete(tempRoot, true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"[VGM INSTALL] Unable to clean temporary files: {ex.Message}");
                }
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