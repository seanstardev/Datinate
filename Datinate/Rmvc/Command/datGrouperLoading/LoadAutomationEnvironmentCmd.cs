using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Util;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;
namespace com.RADIO.Datinate.RMVC
{
    public class LoadAutomationEnvironmentCmd : RCommandAsync
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

        protected override async Task RunAsync()
        {
            var cmd = new LoadDatGrouperContentPathsCmd(false, false, false);
            
            await base.ExecuteCommandAsync(cmd);

            if (cmd.AutoLoadSuccessful && Facade.Instance?.DatGrouperSessionModel != null)
                Facade.Instance.DatGrouperSessionModel.ContentPathsResolved = true;

            await base.ExecuteCommandAsync(new ShowProgressCmd("Rendering Results", 4, 5));

            Dictionary<string, CurationPartReport> partReportsDictionary; 

            // NOTE: We need to do this because setting Exclude on GamePartVO is fragile:
            var baseFamilies = DatinateFamilyConverter.Convert(families, out partReportsDictionary);

            var flagFilterSet = Facade.Instance?.AppDataProxy?.FlagFilterSetByGroup ?? new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>();

            Facade.Instance?.DatGrouperModel?.CreateSession(baseFamilies.ToList(), flagFilterSet, softwareIdDatGroupEnumDictionary);

            // NOTE: We MUST pass in this dictionary or we will see no shallow copy icons or include / exclude variants:
            Facade.Instance?.DatGrouperMediator?.SetAutoView(baseFamilies.ToArray(), projectName, partReportsDictionary);

            Facade.Instance?.DatGrouperMediator?.SetCuratedView([], projectName);
        }
    }
}
