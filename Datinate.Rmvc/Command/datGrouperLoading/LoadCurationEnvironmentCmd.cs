using Datinate.Rmvc.Dto;
using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class LoadCurationEnvironmentCmd : CommandBase<RVoid>
    {
        private readonly string projectName;

        public LoadCurationEnvironmentCmd(string projectName)
        {
            this.projectName = projectName;
        }

        protected override async Task<RVoid> Run()
        {
            base.MainWebMediator?.ClearView(false);

            if (base.CuratedDatProxy == null ||
                base.DatGrouperMediator == null ||
                base.RadioDatModel == null ||
                base.DatGrouperModel == null ||
                base.ModelDataProxy == null)
            {
                return await Complete();
            }

            await base.Execute(new LazyLoadDatGrouperFilesCmd());

            if (base.DatGrouperSessionModel != null)
            {
                var layout = base.DatGrouperSessionModel.ActivateCurationMode();
                await base.Execute(new ExitMediaModeCmd());
                base.DatGrouperMediator?.SetScreenLayout(layout);
            }
            
            Dictionary<string, IGamePart?>? importErrorReport = null;

            if (base.CuratedDatProxy.GetDatExists(projectName))
            {
                var collection = CuratedDatProxy.LoadCurated(
                    projectName,
                    base.RadioDatModel.DescriptorDefinitions,
                    base.ModelDataProxy.FlagFilterSetByGroup,
                    base.RadioDatModel.SourceIdContentDictionary);

                if (collection is { })
                {
                    await base.Execute(new ShowProgressCmd("Importing Curated Set.", 2, 4));

                    try
                    {
                        DatGrouperEditDelta? delta = base.DatGrouperModel.ImportCurated(
                            collection.Keys, 
                            out var errorReport);

                        importErrorReport = errorReport;

                        if (delta != null)
                        {
                            var cache = base.RadioDatModel.ImportMediaUpdates(collection, delta.ReplacementReferences);

                            await base.Execute(new ShowProgressCmd("Rendering Curated Set.", 3, 4));

                            await base.Execute(new ApplyDatGrouperEditCmd(delta, false));

                            base.DatGrouperMediator?.SetMediaCache(cache);
                        }
                    }
                    catch (Exception e)
                    {
                        //
                    }

                    await base.Execute(new ClearProgressCmd());
                }
            }

            base.MainWebMediator?.SetCurationModeActive();

            if (importErrorReport != null && importErrorReport.Any())
            {
                string? errorReportHtml = 
                    base.MainWebMediator?.RenderCuratedImportErrorsReport(importErrorReport);

                if (errorReportHtml != null && string.IsNullOrWhiteSpace(errorReportHtml) == false)
                    base.MainWebMediator?.LoadPageContent(errorReportHtml);
            }

            return await Complete();
        }
    }
}
