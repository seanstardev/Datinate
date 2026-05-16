using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Rmvc.Command;
using RMVC;
using System.Diagnostics;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal class StartDatGrouperCmd : RCommandAsync 
    {
        private readonly DatGrouperProjectDTO projectVO;

        public StartDatGrouperCmd(DatGrouperProjectDTO projectVO) 
        {
            this.projectVO = projectVO;
        }

        protected override async Task RunAsync()
        {
            base.ExecuteCommand(new ClearDatGrouperSessionCmd());

            Debug.WriteLine($"[TIMING] Start: AutoGrouper.");

            var stopwatch = Stopwatch.StartNew();

            var autoGroupModel = Facade.Instance?.AutoGrouperModel;
            var appDataProxy = Facade.Instance?.AppDataProxy;
            
            if (autoGroupModel == null || appDataProxy == null) 
                return;

            await base.ExecuteCommandAsync(new ShowProgressCmd("Loading Project DATs.", 1, 4));

            var cmd = new BuildSmartSoftwareDatsCmd(projectVO);
            await base.ExecuteCommandAsync(cmd);

            var datAdvancedSoftwareCollection = cmd.DatAdvancedCollection;

            if (datAdvancedSoftwareCollection == null) throw new Exception();

            var keys = new HashSet<string>(StringComparer.Ordinal);

            await base.ExecuteCommandAsync(new ShowProgressCmd("Running DAT Grouper.", 2, 4));

            var families = autoGroupModel.Build(
                new AutoGrouperOptions(),
                appDataProxy.FlagFilterSetByGroup,
                datAdvancedSoftwareCollection,
                []);

            if (Facade.Instance?.DatGrouperSessionModel is { } sessionModel)
            {
                sessionModel.PartsTotal = DatinateHelper.GetTotalParts(families);

                if (Facade.Instance?.DatGrouperControlsMediator is { } controlsMediator)
                    controlsMediator.SetCompletionStats(0, sessionModel.PartsTotal);
            }
        
            await base.ExecuteCommandAsync(new ShowProgressCmd("Rendering Results.", 3, 4));

            base.ExecuteCommand(
                new InitialiseRadioDatModelCmd(
                    projectVO, 
                    autoGroupModel.AutoGroupTraceStore,
                    families));

            stopwatch.Stop();
            Debug.WriteLine($"[TIMING] End: AutoGrouper. Time: {stopwatch.Elapsed}");

            // NOTE: RadioDatModel's SourceIdContentDictionary is not available at this stage, but DatGrouperModel
            // needs to know Software source ID to build filters for Curated imports later on. This is a small downside of media lazy loading.
            Dictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary = new Dictionary<string, DAT_GROUP_ENUM>();
            foreach (var dat in datAdvancedSoftwareCollection)
                softwareIdDatGroupEnumDictionary[dat.Key] = dat.DatGroupEnum;

            await base.ExecuteCommandAsync(new LoadAutomationEnvironmentCmd(projectVO.ProjectName, families, softwareIdDatGroupEnumDictionary));

            base.ExecuteCommand(
                new SetWebSearchTermsCmd(null, projectVO.ProjectName));

            Facade.Instance?.Shell?.ShowDatGrouperWindowView();
            Facade.Instance?.Shell?.SetProjectsFormTitle(projectVO.ProjectName);
        }
    }
}
