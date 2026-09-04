using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using System.Data.SQLite;
using System.Diagnostics;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class DatDbProxy : RModel
    {
        private string? dbFullpath;

        public void SetProjectRootPath(string projectRoot)
        {
            dbFullpath = Path.Combine(projectRoot, "DatinateDats.sqlite");
        }
        public bool GetDbExists() => File.Exists(dbFullpath);

        public void EnsureDbExists(Action<string, int, int>? progress)
        {
            if (string.IsNullOrWhiteSpace(dbFullpath))
            {
                System.Diagnostics.Debug.WriteLine("Error: dbFullpath not set. Call SetProjectRootPath(...) before EnsureDbExists().");
                return;
            }

            progress?.Invoke("Checking database", 0, 100);

            Directory.CreateDirectory(Path.GetDirectoryName(dbFullpath)!);

            if (File.Exists(dbFullpath))
            {
                progress?.Invoke("Database exists. Checking for removed DATs", 5, 100);
                PurgeRemovedDats(progress, 5, 100);
                progress?.Invoke("Database ready", 100, 100);
                return;
            }

            progress?.Invoke("Creating database", 10, 100);

            string connectionString =
                $"Data Source={dbFullpath};Version=3;Pooling=False;Default Timeout=5;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                progress?.Invoke("Creating tables", 35, 100);

                string createTableQuery = @"
CREATE TABLE IF NOT EXISTS Dat (
    fullpath TEXT NOT NULL PRIMARY KEY, 
    name TEXT NOT NULL, 
    folder TEXT NOT NULL, 
    date_created TEXT NOT NULL, 
    date_modified TEXT NOT NULL, 
    ""size"" INTEGER NOT NULL, 
    games INTEGER NOT NULL, 
    roms INTEGER NOT NULL, 
    sha1 TEXT NOT NULL, 
    version TEXT NOT NULL, 
    dat_format_enum TEXT NOT NULL,
    description TEXT NOT NULL, 
    category TEXT NOT NULL, 
    author TEXT NOT NULL, 
    comment TEXT NOT NULL
);
";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                    command.ExecuteNonQuery();
            }

            progress?.Invoke("Database ready", 100, 100);
        }

        public void CreateDatEntry(DatSummaryVO vo, string? sha1)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={dbFullpath};Version=3;"))
                {
                    connection.Open();

                    string insertQuery = @"
INSERT INTO Dat (
    fullpath, name, folder, date_created, date_modified, ""size"", games, roms, sha1, version, dat_format_enum, description, category, author, comment
) VALUES (
    @fullpath, @name, @folder, @date_created, @date_modified, @size, @games, @roms, @sha1, @version, @dat_format_enum, @description, @category, @author, @comment
);";

                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@fullpath", vo.DatFullpath);
                        command.Parameters.AddWithValue("@name", vo.DatHeader.Name ?? string.Empty);
                        command.Parameters.AddWithValue("@folder",
                            Path.GetDirectoryName(vo.DatFullpath) != null
                            ? new DirectoryInfo(Path.GetDirectoryName(vo.DatFullpath)!).Name
                            : string.Empty);

                        command.Parameters.AddWithValue("@date_created", File.GetCreationTimeUtc(vo.DatFullpath).ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@date_modified", File.GetLastWriteTimeUtc(vo.DatFullpath).ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@size", vo.SizeTotal);
                        command.Parameters.AddWithValue("@games", vo.GamesTotal);
                        command.Parameters.AddWithValue("@roms", vo.RomsTotal);
                        command.Parameters.AddWithValue("@sha1", sha1 ?? string.Empty);
                        command.Parameters.AddWithValue("@version", vo.DatHeader.Version ?? string.Empty);
                        command.Parameters.AddWithValue("@dat_format_enum", vo.DatHeader.DatTypeEnum.ToString());
                        command.Parameters.AddWithValue("@description", vo.DatHeader.Description ?? string.Empty);
                        command.Parameters.AddWithValue("@category", vo.DatHeader.Category ?? string.Empty);
                        command.Parameters.AddWithValue("@author", vo.DatHeader.Author ?? string.Empty);
                        command.Parameters.AddWithValue("@comment", vo.DatHeader.Comment ?? string.Empty);

                        // Execute the insertion
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                            Debug.WriteLine($"Error: No rows affected when inserting entry for fullpath: {vo.DatFullpath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: Failed to create Dat entry for fullpath {vo.DatFullpath} - {ex.Message}");
            }
        }

        public DatSummaryVO? GetDatSummary(string datFullpath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(datFullpath) || !File.Exists(datFullpath))
                {
                    Debug.WriteLine("Fail: Invalid file path or file does not exist.");
                    return null;
                }

                FileInfo fileInfo = new FileInfo(datFullpath);
                long fileSize = fileInfo.Length;
                string dateCreated = fileInfo.CreationTimeUtc.ToString("yyyy-MM-dd HH:mm:ss");
                string dateModified = fileInfo.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss");

                using (var connection = new SQLiteConnection($"Data Source={dbFullpath};Version=3;"))
                {
                    connection.Open();

                    // Query the database for the `Dat` entry with the given fullpath
                    string query = @"
SELECT fullpath, name, folder, date_created, date_modified, ""size"", games, roms, sha1, version, dat_format_enum, description, category, author, comment
FROM Dat
WHERE fullpath = @fullpath;
";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@fullpath", datFullpath);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Read the stored attributes
                                string dbDateCreated = reader["date_created"].ToString()!;
                                string dbDateModified = reader["date_modified"].ToString()!;

                                // Debugging logs for mismatched attributes
                                if (dbDateCreated != dateCreated || dbDateModified != dateModified)
                                {
                                    // Normalize dates for better comparison
                                    if (DateTime.Parse(dbDateCreated) != fileInfo.CreationTimeUtc ||
                                        DateTime.Parse(dbDateModified) != fileInfo.LastWriteTimeUtc)
                                    {
                                        DeleteDatEntry(connection, datFullpath);
                                        return null;
                                    }
                                }

                                DatHeaderVO header = new DatHeaderVO(
                                    Enum.TryParse(typeof(DAT_FORMAT_ENUM), reader["dat_format_enum"].ToString()!, true, out var formatEnum)
                                        ? (DAT_FORMAT_ENUM)formatEnum
                                        : DAT_FORMAT_ENUM.DatinateNative, // Default if parsing fails
                                    reader["name"].ToString()!,
                                    reader["description"].ToString()!,
                                    reader["category"].ToString()!,
                                    reader["version"].ToString()!,
                                    reader["author"].ToString()!,
                                    reader["comment"].ToString()!
                                );

                                DatSummaryVO summary = new DatSummaryVO(
                                    datFullpath,
                                    Convert.ToInt32(reader["games"]),
                                    Convert.ToInt32(reader["roms"]),
                                    Convert.ToUInt64(reader["size"]),
                                    header,
                                    reader["folder"].ToString()
                                );

                                return summary;
                            }
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fail: Exception occurred - {ex.Message}");
                return null;
            }
        }

        private void DeleteDatEntry(SQLiteConnection connection, string fullpath)
        {
            try
            {
                string deleteQuery = @"
DELETE FROM Dat
WHERE fullpath = @fullpath;
";

                using (var command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@fullpath", fullpath);
                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: Failed to delete entry for fullpath {fullpath} - {ex.Message}");
            }
        }

        private void PurgeRemovedDats(Action<string, int, int>? progress, int startPercent, int endPercent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dbFullpath))
                    return;

                var span = Math.Max(0, endPercent - startPercent);

                int Pct(double t)
                    => startPercent + (int)Math.Round(span * Math.Max(0.0, Math.Min(1.0, t)), MidpointRounding.AwayFromZero);

                int lastReportedPct = -1;
                long lastTick = 0;

                void Report(string msg, double t)
                {
                    if (progress == null) return;

                    var pct = Pct(t);
                    var now = Environment.TickCount64;

                    if (pct != lastReportedPct && (now - lastTick >= 120 || pct == 100))
                    {
                        lastReportedPct = pct;
                        lastTick = now;
                        progress(msg, pct, 100);
                    }
                }

                Report("Loading DAT entries from database", 0.05);

                List<string> allPaths = new List<string>();

                using (var connection = new SQLiteConnection($"Data Source={dbFullpath};Version=3;"))
                {
                    connection.Open();

                    string selectQuery = "SELECT fullpath FROM Dat;";
                    using (var command = new SQLiteCommand(selectQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            allPaths.Add(reader["fullpath"].ToString()!);
                    }
                }

                var count = allPaths.Count;
                if (count == 0)
                {
                    Report("No DAT entries in Database", 1.0);
                    return;
                }

                List<string> missingPaths = new List<string>();

                Report("Scanning filesystem for altered DATs (0/" + count + ")", 0.20);

                long scanTick = 0;
                for (int i = 0; i < count; i++)
                {
                    var path = allPaths[i];

                    if (!File.Exists(path))
                        missingPaths.Add(path);

                    var now = Environment.TickCount64;
                    if (progress != null && (i == count - 1 || now - scanTick >= 150))
                    {
                        scanTick = now;

                        var scanT = (i + 1) / (double)count;
                        var overall = 0.20 + (0.65 * scanT);

                        var pct = Pct(overall);
                        progress(
                            "Scanning filesystem for altered DATs (" +
                            DatinateHelper.GetReadableNumber(i + 1) +
                            " / " +
                            DatinateHelper.GetReadableNumber(count) + ")", pct, 100);
                    }
                }

                if (missingPaths.Count == 0)
                {
                    Report("No removed DATs to purge", 0.90);
                    Report("Purge complete", 1.0);
                    return;
                }

                using (var connection = new SQLiteConnection($"Data Source={dbFullpath};Version=3;"))
                {
                    connection.Open();

                    string deleteQuery = "DELETE FROM Dat WHERE fullpath = @fullpath;";
                    using (var command = new SQLiteCommand(deleteQuery, connection))
                    {
                        var delCount = missingPaths.Count;

                        for (int i = 0; i < delCount; i++)
                        {
                            var fullpath = missingPaths[i];
                            var name = Path.GetFileName(fullpath);

                            var delT = (i + 1) / (double)delCount;
                            var overall = 0.85 + (0.15 * delT);

                            var pct = Pct(overall);
                            progress?.Invoke("Purging entry '" + name + "' (" + (i + 1) + " / " + delCount + ")", pct, 100);

                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@fullpath", fullpath);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                Report("Purge complete", 1.0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: Failed to purge removed Dats - {ex.Message}");
            }
        }
    }
}