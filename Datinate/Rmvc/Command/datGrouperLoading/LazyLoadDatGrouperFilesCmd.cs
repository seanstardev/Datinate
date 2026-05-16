using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Rb;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class LazyLoadDatGrouperFilesCmd : RCommandAsync
    {
        protected override async Task RunAsync()
        {
            var sessionModel = Facade.Instance?.DatGrouperSessionModel;
            
            if (sessionModel == null ||
                !sessionModel.ContentPathsResolved ||
                sessionModel.MediaInitialisd)
            {
                return;
            }

            var resourceProxy = Facade.Instance?.RbAuxResourceProxy;
            var mediaProxy = Facade.Instance?.RbAuxMediaProxy;
            var radioDatModel = Facade.Instance?.RadioDatModel;
            var projectProxy = Facade.Instance?.ProjectProxy;
            var media2Mediator = Facade.Instance?.MediaMediator;
            var datGrouperMediator = Facade.Instance?.DatGrouperMediator;

            if (projectProxy == null ||
                resourceProxy == null ||
                mediaProxy == null ||
                datGrouperMediator == null ||
                media2Mediator == null ||
                string.IsNullOrWhiteSpace(radioDatModel?.ProjectName))
            {
                return;
            }

            var projectName = radioDatModel.ProjectName;
            var project = projectProxy.LoadProject(projectName);

            if (project == null) return;

            using var progress = CreateAutoDisposingProgressHandler(ProgressSink);


            progress.Handler(0, "Loading Software metadata...");

            _ = radioDatModel.CreateSourceSet(
                project.SoftwareEntries,
                COLLECTION_SET_ENUM.Software);

            progress.Handler(0, "Loading Resource metadata...");

            var resourceSources = radioDatModel.CreateSourceSet(
                project.ResourceEntries,
                COLLECTION_SET_ENUM.Resource);

            var resourceSets = resourceProxy.LoadLookups(
                resourceSources,
                progress.Handler,
                FetchDat,
                out var sourceIdResourceDatDic);

            radioDatModel.AddDats(sourceIdResourceDatDic);

            progress.Handler(0, "Loading Media metadata...");

            var mediaSources = radioDatModel.CreateSourceSet(
                project.MediaEntries, 
                COLLECTION_SET_ENUM.Media);

            var mediaSets = mediaProxy.LoadMediaAsync(
                mediaSources,
                progress.Handler,
                Facade.Instance?.GlobalSettingsProxy?.MameSlHashPath,
                FetchDat,
                out var sourceIdMediaDatDic);

            radioDatModel.AddDats(sourceIdMediaDatDic);


            List<ILookupSet> lookupSets = new List<ILookupSet>();
            lookupSets.AddRange(resourceSets);
            lookupSets.AddRange(mediaSets);

            var orderById = project.AuxEntries
                .Select((p, i) => (p.ID, i))
                .GroupBy(x => x.ID)
                .ToDictionary(g => g.Key, g => g.Min(x => x.i));

            lookupSets = lookupSets
                .Select((s, i) => (Set: s, OriginalIndex: i))
                .OrderBy(x => orderById.TryGetValue(x.Set.Id, out var idx) ? idx : int.MaxValue)
                .ThenBy(x => x.OriginalIndex)
                .Select(x => x.Set)
                .ToList();

            radioDatModel.SetLookupSets(lookupSets);

            base.ExecuteCommand(new InitialiseDatGrouperMediaCmd(lookupSets));

            Facade.Instance?.DatGrouperMediator?.ClearLocalProgress();
        }

        private DatVO? FetchDat(string datFullPath)
        {
            if (Facade.Instance?.DatDetailsProxy is not { } proxy)
                return null;

            return proxy.GetDat(datFullPath);
        }

        private static void ProgressSink(int percent, string message)
        {
            var uiMessage = string.IsNullOrWhiteSpace(message)
                ? "Loading Media DATs"
                : message;

            Facade.Instance?.DatGrouperMediator?.SetLocalProgress(percent, 100, uiMessage);
            System.Diagnostics.Debug.WriteLine(nameof(LazyLoadDatGrouperFilesCmd) + ": " + percent + "% " + uiMessage);
        }

        private static AutoDisposingProgressHandler CreateAutoDisposingProgressHandler(Action<int, string>? sink)
        {
            return new AutoDisposingProgressHandler(sink);
        }

        private sealed class AutoDisposingProgressHandler : IDisposable
        {
            private readonly Action<int, string>? sink;
            private int disposed;

            public Action<int, string> Handler { get; }

            public AutoDisposingProgressHandler(Action<int, string>? sink)
            {
                this.sink = sink;
                Handler = Invoke;
            }

            private void Invoke(int percent, string message)
            {
                if (Volatile.Read(ref disposed) != 0)
                    return;

                sink?.Invoke(percent, message);
            }

            public void Dispose()
            {
                Interlocked.Exchange(ref disposed, 1);
            }
        }
    }
}
