using Datinate.Rmvc.Proxy;
using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class DelayedStartupCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            await base.Execute(new UpdateUnitDisplayCmd(UnitFormatHelper.Unit.GB, true));
            await base.Execute(new SetAudioConfigurationCmd());

            DatGrouperProjectDTO? datGrouperStartupProject = null;

            if (string.IsNullOrWhiteSpace(base.Shell?.DatGrouperModeStartupProjectName) == false)
            {
                datGrouperStartupProject = base.ProjectProxy?.LoadProject(
                    base.Shell?.DatGrouperModeStartupProjectName!);
            }

            if (datGrouperStartupProject == null)
            {
                if (base.DatDbProxy is not null && base.DatDbProxy.GetDbExists() == false)
                {
                    _ = base.Shell?.ShowMessageBox(
                        "Attention",
                        "Please note that Datinate will perform an initial scan of DATs once directories are set. This is a one-time process to create the Database that may take several minutes.");
                }

                await base.Execute(new ShowProgressCmd("Loading DAT Grouper Project Summaries", 1, 4));

                await base.Execute(new LoadDatGrouperProjectsCmd(null, false, false));

                await base.Execute(new ShowProgressCmd("Initialising Database.", 2, 4));

                async Task SendProgress(string text, int part, int total)
                {
                    await base.Execute(new ShowProgressCmd(text, part, total));
                }

                var report = ProgressReportThrottle.CreateReporter(
                    send: SendProgress,
                    prefix: "Checking Database. ",
                    outputTotal: 100,
                    minPercentDelta: 1,
                    minIntervalMs: 120,
                    reportOnMessageChange: true);

                if (base.DatDbProxy is { } datDbProxy)
                    await datDbProxy.EnsureDbExists(report);

                base.LandingMediator?.ActivateView();

                await base.Execute(new LoadDatRootPathsCmd());
                await base.Execute(new ClearProgressCmd());
            }
            else
            {
                base.LandingMediator?.ActivateView();
                await base.Execute(new SetDatGrouperFormVisibleCmd());

                await base.Execute(new LoadDatGrouperProjectsCmd(
                    datGrouperStartupProject!.ProjectName,
                    false,
                    false));

                base.Shell?.SetMainFormVisible(false);

                bool allDatAndExpressionFilesExist = 
                    await base.Execute(
                        new CheckDatGrouperProjectValidCmd(datGrouperStartupProject));
                
                if (allDatAndExpressionFilesExist)
                    await base.Execute(new StartDatGrouperCmd(datGrouperStartupProject!));
            }

            base.Shell?.StartResizeMonitor();

            return await Complete();
        }

        private static class ProgressReportThrottle
        {
            public static ProgressCallback CreateReporter(
                ProgressCallback send,
                string prefix,
                int outputTotal = 100,
                int minPercentDelta = 1,
                int minIntervalMs = 125,
                bool reportOnMessageChange = true)
            {
                var reportClock = System.Diagnostics.Stopwatch.StartNew();

                int lastPercent = -1;
                long lastTick = -(long)minIntervalMs; // Allow the first report immediately.
                string? lastMsg = null;

                return async (msg, part, total) =>
                {
                    var pct = ToPercent(part, total);

                    var now = reportClock.ElapsedMilliseconds;
                    var fullMsg = prefix + msg;
                    var msgChanged =
                        reportOnMessageChange &&
                        !string.Equals(lastMsg, fullMsg, StringComparison.Ordinal);

                    if (!msgChanged)
                    {
                        if (Math.Abs(pct - lastPercent) < minPercentDelta)
                        {
                            if (now - lastTick < minIntervalMs)
                                return;
                        }
                    }

                    lastPercent = pct;
                    lastTick = now;
                    lastMsg = fullMsg;

                    await send(fullMsg, pct, outputTotal);
                };
            }

            private static int ToPercent(int part, int total)
            {
                if (total <= 0)
                    return 0;

                if (part < 0)
                    part = 0;

                if (part > total)
                    part = total;

                var pct = (int)Math.Round(
                    (part * 100.0) / total,
                    MidpointRounding.AwayFromZero);

                if (pct < 0)
                    return 0;

                if (pct > 100)
                    return 100;

                return pct;
            }
        }
    }
}