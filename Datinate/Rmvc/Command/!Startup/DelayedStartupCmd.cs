using com.RADIO.Datinate;
using com.RADIO.Datinate.RMVC;
using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class DelayedStartupCmd : RCommandAsync
    {

        protected async override Task RunAsync()
        {
            base.ExecuteCommand(new UpdateUnitDisplayCmd(UnitFormatHelper.Unit.GB, true));

            var datDbProxy = Facade.Instance?.DatDbProxy;

            if (datDbProxy is not null && datDbProxy.GetDbExists() == false)
            {
                Facade.Instance?.Shell?.ShowMessageBox(
                    "Attention",
                    "Please note that Datinate will perform an initial scan of DATs once directories are set. This is a one-time process to create the Database that may take several minutes.");
            }

            await base.ExecuteCommandAsync(new ShowProgressCmd("Loading DAT Grouper Project Summaries", 1, 4));
            
            base.ExecuteCommand(new SetDatGrouperLoaderViewCmd(null, true));

            await base.ExecuteCommandAsync(new ShowProgressCmd("Initialising Database.", 2, 4));

            var ctx = SynchronizationContext.Current;

            Task SendProgressAsync(string text, int part, int total)
                => base.ExecuteCommandAsync(new ShowProgressCmd(text, part, total));

            var report = ProgressReportThrottle.CreateReporter(
                send: SendProgressAsync,
                ctx: ctx,
                prefix: "Checking Database. ",
                outputTotal: 100,
                minPercentDelta: 1,
                minIntervalMs: 120,
                reportOnMessageChange: true);

            await Task.Run(() => Facade.Instance?.DatDbProxy?.EnsureDbExists(report));

            Facade.Instance?.LandingMediator?.ActivateView();

            base.ExecuteCommand(new LoadDatRootPathsCmd());

            base.ExecuteCommand(new ClearProgressCmd());

            Facade.Instance?.Shell?.StartResizeMonitor();
        }
        private static class ProgressReportThrottle
        {
            public static Action<string, int, int> CreateReporter(
                Func<string, int, int, Task> send,
                SynchronizationContext? ctx,
                string prefix,
                int outputTotal = 100,
                int minPercentDelta = 1,
                int minIntervalMs = 125,
                bool reportOnMessageChange = true)
            {
                int lastPercent = -1;
                long lastTick = 0;
                string? lastMsg = null;

                return (msg, part, total) =>
                {
                    var pct = ToPercent(part, total);

                    var now = Environment.TickCount64;
                    var fullMsg = prefix + msg;
                    var msgChanged = reportOnMessageChange && !string.Equals(lastMsg, fullMsg, StringComparison.Ordinal);

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

                    if (ctx != null)
                        ctx.Post(_ => _ = send(fullMsg, pct, outputTotal), null);
                    else
                        _ = send(fullMsg, pct, outputTotal);
                };
            }

            private static int ToPercent(int part, int total)
            {
                if (total <= 0) return 0;

                if (part < 0) part = 0;
                if (part > total) part = total;

                var pct = (int)Math.Round((part * 100.0) / total, MidpointRounding.AwayFromZero);
                if (pct < 0) return 0;
                if (pct > 100) return 100;
                return pct;
            }
        }
    }
}
