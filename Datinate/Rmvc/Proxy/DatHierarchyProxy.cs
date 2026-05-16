using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class DatHierarchyProxy : RModel
    {
        public uint LastTotalDirectoryCount { get; private set; }

        public enum DatFetchStatusEnum
        {
            OK,
            Error_NoDatsFound,
            Error_TooManyLevels,
            Error_TooManyDirs,
            Error_Access
        };

        public Task<DatFetchResult> FetchDatsAsync(DatRootDTO datRootVO)
            => FetchDatsAsync(datRootVO, null);

        public async Task<DatFetchResult> FetchDatsAsync(
            DatRootDTO datRootVO, 
            Action<string, int, int>? progress)
        {
            LastTotalDirectoryCount = 0;

            var status = DatFetchStatusEnum.OK;

            List<DatGrouperProjectEntry> headlines = new List<DatGrouperProjectEntry>();
            List<string> tempPaths = new List<string>();

            var state = new ScanProgressState(datRootVO.RootPath, progress);

            state.Report("Starting scan: " + ShortPath(datRootVO.RootPath), 0);

            status = await FetchDatsRecursivelyAsync(datRootVO, tempPaths, 0, state).ConfigureAwait(false);

            if (status != DatFetchStatusEnum.OK)
            {
                return new DatFetchResult()
                {
                    Status = status,
                    DatHeadlines = headlines.ToArray()
                };
            }

            state.Report("Sorting results (" + tempPaths.Count + ")", 95);

            tempPaths.Sort();

            foreach (var path in tempPaths)
            {
                headlines.Add(
                    new DatGrouperProjectEntry(
                        COLLECTION_SET_ENUM.NOT_SET,
                        path,
                        string.Empty,
                        string.Empty,
                        DAT_GROUP_ENUM.NOT_SET,
                        path,
                        null,
                        null,
                        null,
                        null,
                        false
                    )
                );
            }

            status = headlines.Count == 0 ? DatFetchStatusEnum.Error_NoDatsFound : DatFetchStatusEnum.OK;

            state.Report("Scan complete (" + headlines.Count + ")", 100);

            return new DatFetchResult()
            {
                Status = status,
                DatHeadlines = headlines.ToArray()
            };
        }

        private async Task<DatFetchStatusEnum> FetchDatsRecursivelyAsync(
            DatRootDTO datRootVO,
            List<string> tempPaths,
            int level,
            ScanProgressState state)
        {
            LastTotalDirectoryCount++;

            if (LastTotalDirectoryCount > Constants.MAX_DIRS_TO_PARSE)
                return DatFetchStatusEnum.Error_TooManyDirs;

            if (level > Constants.MAX_DIRS_LEVELS_TO_PARSE)
                return DatFetchStatusEnum.Error_TooManyLevels;

            state.MaybeReport(datRootVO.RootPath, level, tempPaths.Count, LastTotalDirectoryCount);

            string[] files;

            try
            {
                files = await Task.Run(() => Directory.GetFiles(datRootVO.RootPath)).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException)
            {
                return DatFetchStatusEnum.Error_Access;
            }

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var lowerFile = file.ToLowerInvariant();

                if (lowerFile.EndsWith(".dat") || lowerFile.EndsWith(".xml"))
                    tempPaths.Add(file);
            }

            string[] dirs;
            try
            {
                dirs = await Task.Run(() => Directory.GetDirectories(datRootVO.RootPath)).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException)
            {
                return DatFetchStatusEnum.Error_Access;
            }

            if (dirs.Length > 0)
                level++;

            for (int i = 0; i < dirs.Length; i++)
            {
                var dir = dirs[i];

                state.MaybeReport(dir, level, tempPaths.Count, LastTotalDirectoryCount);

                var childVO = new DatRootDTO(dir, string.Empty);

                var status = await FetchDatsRecursivelyAsync(childVO, tempPaths, level, state).ConfigureAwait(false);
                if (status != DatFetchStatusEnum.OK)
                    return status;
            }

            return DatFetchStatusEnum.OK;
        }

        private static string ShortPath(string p)
        {
            if (string.IsNullOrWhiteSpace(p))
                return string.Empty;

            var name = Path.GetFileName(p.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            return string.IsNullOrWhiteSpace(name) ? p : name;
        }

        private sealed class ScanProgressState
        {
            private readonly Action<string, int, int>? progress;
            private readonly string root;
            private long lastTick;
            private uint lastDirReported;
            private int lastPct;

            public ScanProgressState(string root, Action<string, int, int>? progress)
            {
                this.root = root;
                this.progress = progress;
            }

            public void Report(string msg, int pct)
            {
                if (progress == null)
                    return;

                if (pct < 0) pct = 0;
                if (pct > 100) pct = 100;

                lastPct = pct;
                progress(msg, pct, 100);
            }

            public void MaybeReport(string currentDir, int level, int foundCount, uint dirCount)
            {
                if (progress == null)
                    return;

                var now = Environment.TickCount64;

                if (dirCount - lastDirReported < 15 && now - lastTick < 150)
                    return;

                lastTick = now;
                lastDirReported = dirCount;

                var pct = ApproxPct(dirCount);
                if (pct < lastPct) pct = lastPct;
                if (pct < lastPct) pct = lastPct;
                if (pct > 99) pct = 99;

                lastPct = pct;

                var msg =
                    "Scanning " + ShortPath(currentDir) +
                    " (folders: " + dirCount +
                    ", dats: " + foundCount +
                    ", depth: " + level + ")";

                progress(msg, pct, 100);
            }

            private static int ApproxPct(uint dirCount)
            {
                var x = dirCount / 700.0;
                var pct = (int)Math.Round(100.0 * (1.0 - Math.Exp(-x)), MidpointRounding.AwayFromZero);
                if (pct < 0) return 0;
                if (pct > 100) return 100;
                return pct;
            }
        }

        protected override void Initialise() { }

        public sealed class DatFetchResult
        {
            public DatGrouperProjectEntry[] DatHeadlines { get; init; } = Array.Empty<DatGrouperProjectEntry>();
            public DatFetchStatusEnum Status { get; init; }
        }
    }
}
