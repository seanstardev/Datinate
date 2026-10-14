using Datinate.Rmvc.Mediator;
using Datinate.Shared;
using Datinate.Shared.Dat;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class LazyLoadDatGrouperFilesCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            if (base.DatGrouperSessionModel == null ||
                !base.DatGrouperSessionModel.ContentPathsResolved ||
                base.DatGrouperSessionModel.MediaInitialisd)
            {
                return await Complete();
            }

            if (base.ProjectProxy == null ||
                base.RbAuxResourceProxy == null ||
                base.RbAuxMediaProxy == null ||
                base.DatGrouperMediator == null ||
                base.MediaMediator == null ||
                base.RadioDatModel == null ||
                base.RadioDatModel.ProjectName == null ||
                string.IsNullOrWhiteSpace(RadioDatModel.ProjectName))
            {
                return await Complete();
            }

            var projectName = base.RadioDatModel.ProjectName;
            var project = base.ProjectProxy.LoadProject(projectName);

            if (project == null) 
                return await Complete();

            using var progress = CreateAutoDisposingProgressHandler(
                (percent, message) =>
                    ProgressSink(percent, message, base.DatGrouperMediator));

            progress.Handler(0, "Loading Software metadata...");

            _ = base.RadioDatModel.CreateSourceSet(
                project.SoftwareEntries,
                COLLECTION_SET_ENUM.Software);

            progress.Handler(0, "Loading Resource metadata...");

            var resourceSources = base.RadioDatModel.CreateSourceSet(
                project.ResourceEntries,
                COLLECTION_SET_ENUM.Resource);

            var resourceSets = base.RbAuxResourceProxy.LoadLookups(
                resourceSources,
                progress.Handler,
                FetchDat,
                out var sourceIdResourceDatDic);

            base.RadioDatModel.AddDats(sourceIdResourceDatDic);

            progress.Handler(0, "Loading Media metadata...");

            var mediaSources = base.RadioDatModel.CreateSourceSet(
                project.MediaEntries, 
                COLLECTION_SET_ENUM.Media);

            var mediaSets = base.RbAuxMediaProxy.LoadMediaAsync(
                mediaSources,
                progress.Handler,
                base.GlobalSettingsProxy?.MameSlHashPath,
                FetchDat,
                out var sourceIdMediaDatDic);

            base.RadioDatModel.AddDats(sourceIdMediaDatDic);


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

            base.RadioDatModel.SetLookupSets(lookupSets);

            await base.Execute(new InitialiseDatGrouperMediaCmd(
                lookupSets));

            base.DatGrouperMediator?.ClearLocalProgress();

            return await Complete();
        }

        private DatVO? FetchDat(string datFullPath)
        {
            if (base.DatDetailsProxy is not { })
                return null;

            return base.DatDetailsProxy.GetDat(datFullPath);
        }

        private static void ProgressSink(int percent, string message, DatGrouperMediator? datGrouperMediator)
        {
            var uiMessage = string.IsNullOrWhiteSpace(message)
                ? "Loading Media DATs"
                : message;

            datGrouperMediator?.SetLocalProgress(percent, 100, uiMessage);
            System.Diagnostics.Debug.WriteLine(nameof(LazyLoadDatGrouperFilesCmd) + ": " + percent + "% " + uiMessage);
        }

        private static AutoDisposingProgressHandler CreateAutoDisposingProgressHandler(Action<int, string>? sink)
            => new AutoDisposingProgressHandler(sink);
        

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
