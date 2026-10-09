using Datinate.Rmvc.Dto;
using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Rmvc.Command
{
    public class LoadCurationEnvironmentCmd : RCommandAsync
    {
        private readonly string projectName;

        public LoadCurationEnvironmentCmd(string projectName)
        {
            this.projectName = projectName;
        }

        protected override async Task RunAsync()
        {
            var curatedProxy = Facade.Instance?.CuratedDatProxy;
            var radioDatModel = Facade.Instance?.RadioDatModel;
            var grouperMediator = Facade.Instance?.DatGrouperMediator;
            var datGrouperModel = Facade.Instance?.DatGrouperModel;
            var appDataProxy = Facade.Instance?.ModelDataProxy;

            Facade.Instance?.MainWebMediator?.ClearView(false);

            if (curatedProxy == null ||
                grouperMediator == null ||
                radioDatModel == null ||
                datGrouperModel == null ||
                appDataProxy == null)
            {
                return;
            }

            await base.ExecuteCommandAsync(new LazyLoadDatGrouperFilesCmd());

            if (Facade.Instance?.DatGrouperSessionModel != null)
            {
                var layout = Facade.Instance.DatGrouperSessionModel.ActivateCurationMode();
                base.ExecuteCommand(new ExitMediaModeCmd());
                grouperMediator?.SetScreenLayout(layout);
            }
            
            Dictionary<string, IGamePart?>? importErrorReport = null;

            if (curatedProxy.GetDatExists(projectName))
            {
                var collection = curatedProxy.LoadCurated(
                    projectName,
                    radioDatModel.DescriptorDefinitions,
                    appDataProxy.FlagFilterSetByGroup,
                    radioDatModel.SourceIdContentDictionary);

                if (collection is { })
                {
                    base.ExecuteCommand(new ShowProgressCmd("Importing Curated Set.", 2, 4));

                    try
                    {
                        DatGrouperEditDelta? delta = datGrouperModel.ImportCurated(
                            collection.Keys, 
                            out var errorReport);

                        importErrorReport = errorReport;

                        if (delta != null)
                        {
                            var cache = radioDatModel.ImportMediaUpdates(collection, delta.ReplacementReferences);

                            base.ExecuteCommand(new ShowProgressCmd("Rendering Curated Set.", 3, 4));

                            base.ExecuteCommand(new ApplyDatGrouperEditCmd(delta, false));

                            Facade.Instance?.DatGrouperMediator?.SetMediaCache(cache);
                        }
                    }
                    catch (Exception e)
                    {
                        //
                    }

                    base.ExecuteCommand(new ClearProgressCmd());
                }
            }

            Facade.Instance?.MainWebMediator?.SetCurationModeActive();

            if (importErrorReport != null && importErrorReport.Any())
            {
                string? errorReportHtml = Facade.Instance?.MainWebMediator?.RenderCuratedImportErrorsReport(importErrorReport);

                if (errorReportHtml != null && string.IsNullOrWhiteSpace(errorReportHtml) == false)
                    Facade.Instance?.MainWebMediator?.LoadPageContent(errorReportHtml);
            }

            return;
        }
    }
}
