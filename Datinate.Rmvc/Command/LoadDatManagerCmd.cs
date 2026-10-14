using Datinate.Rmvc.Proxy;
using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using RMVC;
using static Datinate.Rmvc.Proxy.DatHierarchyProxy;

namespace Datinate.Rmvc.Command
{
    internal class LoadDatManagerCmd : CommandBase<RVoid>
    {
        private readonly DatRootDTO[] datRootPaths;
        private readonly string mameHashPath;

        public LoadDatManagerCmd(DatRootDTO[] datRootPaths, string mameHashPath)
        {
            this.datRootPaths = datRootPaths;
            this.mameHashPath = mameHashPath;
        }
        protected override async Task<RVoid> Run()
        {
            if (base.DatHierarchyProxy == null || base.ActiveDatsModel == null)
                return await Complete();

            if (base.GlobalSettingsProxy is { })
                base.GlobalSettingsProxy.MameSlHashPath = mameHashPath;

            base.DatDetailsProxy?.ClearDatCache();
            base.DatDetailsMediator?.ResetView();

            var ctx = SynchronizationContext.Current;

            await base.Execute(new ShowProgressCmd("Loading Dats...", 0, 100));

            base.Shell?.SetProgressFormVisible(true);

            int postScheduled = 0;
            string? pendingText = null;
            int pendingPct = 0;

            void PostProgress(string text, int pct)
            {
                if (ctx == null)
                {
                    base.ProgressMediator?.UpdateProgress(text, pct, 100);
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

                    base.ProgressMediator?.UpdateProgress(t, p, 100);

                    Volatile.Write(ref postScheduled, 0);
                }, null);
            }
            
            List<DatGrouperProjectEntry> datHeadlineList = new List<DatGrouperProjectEntry>();

            for (int i = 0; i < datRootPaths.Length; i++)
            {
                var rootIndex = i;
                var rootCount = datRootPaths.Length;
                var root = datRootPaths[rootIndex];

                ProgressCallback progress = (msg, part, total) =>
                {
                    var pctLocal = total <= 0 ? 0 : (int)Math.Round((part * 100.0) / total, MidpointRounding.AwayFromZero);
                    if (pctLocal < 0) pctLocal = 0;
                    if (pctLocal > 100) pctLocal = 100;

                    var globalPct = (int)Math.Round(((rootIndex + (pctLocal / 100.0)) * 100.0) / rootCount, MidpointRounding.AwayFromZero);
                    if (globalPct < 0) globalPct = 0;
                    if (globalPct > 100) globalPct = 100;

                    var text = "Scanning (" + (rootIndex + 1) + " / " + rootCount + "): " + msg;

                    PostProgress(text, globalPct);
                    return Task.CompletedTask;
                };

                var result = await
                    base.DatHierarchyProxy!.FetchDatsAsync(root, progress);

                var status = result.Status;
                var runningList = result.DatHeadlines;

                switch (status)
                {
                    case DatFetchStatusEnum.Error_NoDatsFound:
                        await base.Execute(new ShowMessageCmd(
                            "A DAT Root directory contained no DAT files. Please remove the following Path:\n" + root.RootPath
                        ));
                        await base.Execute(new ClearProgressCmd());
                        return await Complete();

                    case DatFetchStatusEnum.Error_Access:
                        await base.Execute(new ShowMessageCmd(
                            "You are trying to scan folders that " + Constants.APP_NAME + " does not have access to (System, Recycle Bin, etc.).\n" +
                            "Did you select the wrong directory? Check the following Path:\n" + root.RootPath
                        ));
                        await base.Execute(new ClearProgressCmd());
                        return await Complete();

                    case DatFetchStatusEnum.Error_TooManyLevels:
                        await base.Execute(new ShowMessageCmd(
                            "The path you have chosen to scan contains too many subfolders (over " + Constants.MAX_DIRS_LEVELS_TO_PARSE + ").\n" +
                            "Did you select the wrong directory? The problem Path:\n" + root.RootPath
                        ));
                        await base.Execute(new ClearProgressCmd());
                        return await Complete();

                    case DatFetchStatusEnum.Error_TooManyDirs:
                        await base.Execute(new ShowMessageCmd(
                            "The path you have chosen to scan contains over " + Constants.MAX_DIRS_TO_PARSE.ToString() + " folders.\n" +
                            "Did you select the wrong directory? The problem Path:\n" + root.RootPath
                        ));
                        await base.Execute(new ClearProgressCmd());
                        return await Complete();

                    case DatFetchStatusEnum.OK:
                        datHeadlineList.AddRange(runningList);
                        break;
                }
            }

            DatGrouperProjectEntry[] datHeadlines = datHeadlineList.ToArray();
            
            if (base.ActiveDatsModel is { })
            {
                base.ActiveDatsModel.HierarchyDats = datHeadlines;
                
                await base.Execute(new LoadDatSummariesCmd(false));
                
                await base.Execute(new ShowDatHierarchyCmd(
                    datHeadlines.Select(d => d.DatFullpath).ToArray()));
            }
            else
            {
                await base.Execute(new ClearProgressCmd());
            }

            // TODO: Lazy:
            // Opaque RGB(240, 240, 240): conventional WinForms SystemColors.Control colour.
            base.Shell?.SetMainFormsSizeBarBackColorArgb(unchecked((int)0xFFF0F0F0));

            base.MainControlsMediator?.ActivateView();

            return await Complete();
        }
    }
}
