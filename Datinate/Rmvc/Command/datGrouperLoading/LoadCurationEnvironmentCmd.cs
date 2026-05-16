using app.datinate;
using RadioLibCore.RadioDat;
using RMVC;
using System.Diagnostics;

namespace com.RADIO.Datinate.RMVC
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
            var appDataProxy = Facade.Instance?.AppDataProxy;

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
                    try
                    {
                        DatGrouperEditDelta? delta = datGrouperModel.ImportCurated(
                            collection.Keys, 
                            out var errorReport);

                        importErrorReport = errorReport;

                        if (delta != null)
                        {
                            var cache = radioDatModel.ImportMediaUpdates(collection, delta.ReplacementReferences);
                            base.ExecuteCommand(new ApplyDatGrouperEditCmd(delta, false));

                            Facade.Instance?.DatGrouperMediator?.SetMediaCache(cache);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(">> MOVE: " + e);
                    }
                }
            }

            Facade.Instance?.MainWebMediator?.SetCurationModeActive();

            if (importErrorReport != null && importErrorReport.Any())
            {
                string? errorReportHtml = Facade.Instance?.MainWebMediator?.RenderCuratedImportErrorsReport(importErrorReport);

                if (string.IsNullOrWhiteSpace(errorReportHtml) == false)
                    Facade.Instance?.MainWebMediator?.LoadPageContent(errorReportHtml);
                
            }

            return;
        }
    }
}
