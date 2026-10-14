using Datinate.Rmvc.Dto;
using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using RMVC;
using System.Diagnostics;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class StartDatGrouperCmd : CommandBase<RVoid>
    {
        private readonly DatGrouperProjectDTO projectVO;

        public StartDatGrouperCmd(DatGrouperProjectDTO projectVO) 
        {
            this.projectVO = projectVO;
        }

        protected override async Task<RVoid> Run()
        {
            await base.Execute(new ClearDatGrouperSessionCmd());

            var allDatAndExpressionFilesExist = 
                await base.Execute(
                    new CheckDatGrouperProjectValidCmd(projectVO));

            // TODO: At least debug print this:
            if (allDatAndExpressionFilesExist == false)
                return await Complete();

            Debug.WriteLine($"[TIMING] Start: AutoGrouper.");

            var stopwatch = Stopwatch.StartNew();
            
            if (base.AutoGrouperModel == null || base.ModelDataProxy == null) 
                return await Complete();

            await base.Execute(new ShowProgressCmd("Loading Project DATs.", 1, 4));

            var datAdvancedSoftwareCollection = 
                await base.Execute(
                    new BuildSmartSoftwareDatsCmd(projectVO));

            if (datAdvancedSoftwareCollection == null) throw new Exception();

            var keys = new HashSet<string>(StringComparer.Ordinal);

            await base.Execute(new ShowProgressCmd("Running DAT Grouper.", 2, 4));

            var families = base.AutoGrouperModel.Build(
                new AutoGrouperOptions(),
                base.ModelDataProxy.FlagFilterSetByGroup,
                datAdvancedSoftwareCollection,
                Array.Empty<DatAdvanced>());

            if (base.DatGrouperSessionModel is { } sessionModel)
            {
                sessionModel.PartsTotal = DatinateFamilyHelper.GetTotalParts(families);

                if (base.DatGrouperControlsMediator is { } controlsMediator)
                    controlsMediator.SetCompletionStats(0, sessionModel.PartsTotal);
            }
        
            await base.Execute(new ShowProgressCmd("Rendering Results.", 3, 4));

            await base.Execute(
                new InitialiseRadioDatModelCmd(
                    projectVO,
                    base.AutoGrouperModel.AutoGroupTraceStore,
                    families));

            stopwatch.Stop();
            Debug.WriteLine($"[TIMING] End: AutoGrouper. Time: {stopwatch.Elapsed}");

            // NOTE: RadioDatModel's SourceIdContentDictionary is not available at this stage, but DatGrouperModel
            // needs to know Software source ID to build filters for Curated imports later on. This is a small downside of media lazy loading.
            Dictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary = new Dictionary<string, DAT_GROUP_ENUM>();
            
            foreach (var dat in datAdvancedSoftwareCollection)
                softwareIdDatGroupEnumDictionary[dat.Key] = dat.DatGroupEnum;

            await base.Execute(
                new LoadAutomationEnvironmentCmd(
                    projectVO.ProjectName, 
                    families, 
                    softwareIdDatGroupEnumDictionary));

            await base.Execute(
                new SetWebSearchTermsCmd(null, projectVO.ProjectName));

            base.Shell?.ShowDatGrouperWindowView();
            base.Shell?.SetProjectsFormTitle(projectVO.ProjectName);

            return await Complete();
        }
    }
}