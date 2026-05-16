using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.DatHierarchyProxy;

namespace com.RADIO.Datinate.RMVC
{
    internal class LoadDatManagerCmd : RCommandAsync
    {
        private readonly DatRootDTO[] datRootPaths;
        private readonly string mameHashPath;

        public LoadDatManagerCmd(DatRootDTO[] datRootPaths, string mameHashPath)
        {
            this.datRootPaths = datRootPaths;
            this.mameHashPath = mameHashPath;
        }
        protected override async Task RunAsync()
        {
            if (Facade.Instance?.DatHierarchyProxy == null || Facade.Instance?.ActiveDatsModel == null)
                return;

            if (Facade.Instance?.GlobalSettingsProxy is { } globalProxy)
                globalProxy.MameSlHashPath = mameHashPath;

            Facade.Instance?.DatDetailsProxy?.ClearDatCache();
            Facade.Instance?.DatDetailsMediator?.ResetView();

            var ctx = SynchronizationContext.Current;

            await base.ExecuteCommandAsync(new ShowProgressCmd("Loading Dats...", 0, 100));

            Facade.Instance?.Shell?.SetProgressFormVisible(true);

            int postScheduled = 0;
            string? pendingText = null;
            int pendingPct = 0;

            void PostProgress(string text, int pct)
            {
                if (ctx == null)
                {
                    Facade.Instance?.ProgressMediator?.UpdateProgress(text, pct, 100);
                    return;
                }

                Volatile.Write(ref pendingText, text);
                Volatile.Write(ref pendingPct, pct);

                if (Interlocked.Exchange(ref postScheduled, 1) != 0)
                    return;

                ctx.Post(_ =>
                {
                    var t = Volatile.Read(ref pendingText) ?? string.Empty;
                    var p = Volatile.Read(ref pendingPct);

                    Facade.Instance?.ProgressMediator?.UpdateProgress(t, p, 100);

                    Volatile.Write(ref postScheduled, 0);
                }, null);
            }
            
            List<DatGrouperProjectEntry> datHeadlineList = new List<DatGrouperProjectEntry>();

            for (int i = 0; i < datRootPaths.Length; i++)
            {
                var rootIndex = i;
                var rootCount = datRootPaths.Length;
                var root = datRootPaths[rootIndex];

                Action<string, int, int> progress = (msg, part, total) =>
                {
                    var pctLocal = total <= 0 ? 0 : (int)Math.Round((part * 100.0) / total, MidpointRounding.AwayFromZero);
                    if (pctLocal < 0) pctLocal = 0;
                    if (pctLocal > 100) pctLocal = 100;

                    var globalPct = (int)Math.Round(((rootIndex + (pctLocal / 100.0)) * 100.0) / rootCount, MidpointRounding.AwayFromZero);
                    if (globalPct < 0) globalPct = 0;
                    if (globalPct > 100) globalPct = 100;

                    var text = "Scanning (" + (rootIndex + 1) + " / " + rootCount + "): " + msg;

                    PostProgress(text, globalPct);
                };

                var result = await Task.Run(() =>
                    Facade.Instance?.DatHierarchyProxy.FetchDatsAsync(root, progress));

                var status = result.Status;
                var runningList = result.DatHeadlines;

                switch (status)
                {
                    case DatFetchStatusEnum.Error_NoDatsFound:
                        base.ExecuteCommand(new ShowMessageCmd(
                            "A DAT Root directory contained no DAT files. Please remove the following Path:\n" + root.RootPath
                        ));
                        base.ExecuteCommand(new ClearProgressCmd());
                        return;

                    case DatFetchStatusEnum.Error_Access:
                        base.ExecuteCommand(new ShowMessageCmd(
                            "You are trying to scan folders that " + Constants.APP_NAME + " does not have access to (System, Recycle Bin, etc.).\n" +
                            "Did you select the wrong directory? Check the following Path:\n" + root.RootPath
                        ));
                        base.ExecuteCommand(new ClearProgressCmd());
                        return;

                    case DatFetchStatusEnum.Error_TooManyLevels:
                        base.ExecuteCommand(new ShowMessageCmd(
                            "The path you have chosen to scan contains too many subfolders (over " + Constants.MAX_DIRS_LEVELS_TO_PARSE + ").\n" +
                            "Did you select the wrong directory? The problem Path:\n" + root.RootPath
                        ));
                        base.ExecuteCommand(new ClearProgressCmd());
                        return;

                    case DatFetchStatusEnum.Error_TooManyDirs:
                        base.ExecuteCommand(new ShowMessageCmd(
                            "The path you have chosen to scan contains over " + Constants.MAX_DIRS_TO_PARSE.ToString() + " folders.\n" +
                            "Did you select the wrong directory? The problem Path:\n" + root.RootPath
                        ));
                        base.ExecuteCommand(new ClearProgressCmd());
                        return;

                    case DatFetchStatusEnum.OK:
                        datHeadlineList.AddRange(runningList);
                        break;
                }
            }

            DatGrouperProjectEntry[] datHeadlines = datHeadlineList.ToArray();
            
            if (Facade.Instance?.ActiveDatsModel is { })
            {
                Facade.Instance.ActiveDatsModel.HierarchyDats = datHeadlines;
                
                await base.ExecuteCommandAsync(new LoadDatSummariesCmd(false));
                
                await base.ExecuteCommandAsync(new ShowDatHierarchyCmd(datHeadlines.Select(d => d.DatFullpath).ToArray()));
            }
            else
            {
                base.ExecuteCommand(new ClearProgressCmd());
            }

            Facade.Instance?.Shell?.SetMainFormsSizeBarBackColor(SystemColors.Control);

            Facade.Instance?.MainControlsMediator?.ActivateView();
        }
    }
}
