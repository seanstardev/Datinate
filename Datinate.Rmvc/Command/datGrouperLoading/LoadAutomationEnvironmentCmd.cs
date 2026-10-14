using Datinate.Rmvc.Dto;
using Datinate.Shared;
using Datinate.Shared.Radio;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class LoadAutomationEnvironmentCmd : CommandBase<RVoid>
    {
        private readonly string projectName;
        private readonly IEnumerable<GameFamilyVO> families;
        private readonly IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary;

        public LoadAutomationEnvironmentCmd(
            string projectName, 
            IEnumerable<GameFamilyVO> families,
            IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary)
        {
            this.projectName = projectName;
            this.families = families;
            this.softwareIdDatGroupEnumDictionary = softwareIdDatGroupEnumDictionary;
        }

        protected override async Task<RVoid> Run()
        {
            var autoLoadSuccessful = await base.Execute(
                new LoadDatGrouperContentPathsCmd(false));

            if (autoLoadSuccessful && base.DatGrouperSessionModel != null)
                base.DatGrouperSessionModel.ContentPathsResolved = true;

            await base.Execute(new ShowProgressCmd("Rendering Automated Set", 4, 5));

            Dictionary<string, CurationPartReport> partReportsDictionary; 

            // NOTE: We need to do this because setting Exclude on GamePartVO is fragile:
            var baseFamilies = DatinateFamilyConverter.Convert(families, out partReportsDictionary);

            var flagFilterSet = base.ModelDataProxy?.FlagFilterSetByGroup 
                ?? new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>();

            base.DatGrouperModel?.CreateSession(
                baseFamilies.ToList(), flagFilterSet, softwareIdDatGroupEnumDictionary);

            // NOTE: We MUST pass in this dictionary or we will see no shallow copy icons or include / exclude variants:
            base.DatGrouperMediator?.SetAutoView(baseFamilies.ToArray(), projectName, partReportsDictionary);

            base.DatGrouperMediator?.SetCuratedView(Array.Empty<IGameFamily>(), projectName);

            return await Complete();
        }
    }
}
