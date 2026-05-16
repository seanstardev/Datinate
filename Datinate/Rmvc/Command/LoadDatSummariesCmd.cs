using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using System.Diagnostics;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    internal class LoadDatSummariesCmd : RCommandAsync
    {
        private readonly bool scanForDuplicateDats;

        public LoadDatSummariesCmd(bool scanForDuplicateDats)
        {
            this.scanForDuplicateDats = scanForDuplicateDats;
        }

        protected async override Task RunAsync()
        {

            if (Facade.Instance == null ||
                Facade.Instance?.ActiveDatsModel == null ||
                Facade.Instance?.DatDbProxy == null ||
                Facade.Instance?.DatDetailsProxy == null ||
                Facade.Instance?.UnitDisplayModel == null)
            {
                return;
            }

            var activeDatsModel = Facade.Instance.ActiveDatsModel;
            var datDetailsProxy = Facade.Instance.DatDetailsProxy;
            var datDbProxy = Facade.Instance.DatDbProxy;
            var unitDisplayModel = Facade.Instance.UnitDisplayModel;
            
            
            DatGrouperProjectEntry[]? datPaths = activeDatsModel.HierarchyDats;

            if (datPaths == null) 
                return;

            List<string> unrecognisedDATs = new List<string>();

            // Note: We have to send something to force Progress view update:
            await base.ExecuteCommandAsync(
                new ShowProgressCmd("Fetching DAT Summary Information", 0, datPaths.Length));

            List<DatSummaryVO> datsSummaries = new List<DatSummaryVO>();

            for (int i = 0; i < datPaths.Length; i++)
            {
                var datPath = datPaths[i];

                DatSummaryVO? dbDatEntry = datDbProxy.GetDatSummary(datPath.DatFullpath);

                DatSummaryVO? summary = null;

                string? sha1 = null;

                if (dbDatEntry != null)
                {
                    summary = dbDatEntry;
                }
                else
                {
                    var dat = datDetailsProxy.GetDat(datPath.DatFullpath, out var rawFile);

                    if (dat != null)
                    {
                        sha1 = rawFile.Sha1;
                        summary = dat.CreateSummary();
                    }
                }

                if (summary == null)
                {
                    Debug.WriteLine(">> LOG: Unrecognised DAT: '" + datPath.DatFullpath + "'.");
                    unrecognisedDATs.Add(datPath.DatFullpath);
                }
                else
                {
                    await base.ExecuteCommandAsync(new ShowProgressCmd(
                        "Fetching DAT Summary Information (" + Path.GetDirectoryName(summary.DatFullpath) + " > " + Path.GetFileNameWithoutExtension(summary.DatFullpath) + ")", i + 1, datPaths.Length));

                    datsSummaries.Add(summary);

                    if (dbDatEntry == null)
                    {
                        datDbProxy.CreateDatEntry(summary, sha1);
                    }
                }
            }

            base.ExecuteCommand(new ClearProgressCmd());

            bool unhandledIssue = false;  
            if (unrecognisedDATs.Count > 0) 
            {
                unhandledIssue = true;
                base.ExecuteCommand(
                    new SetProblemListCmd(
                    SetProblemListCmd.UI.Unreadable
                    , unrecognisedDATs.ToArray()
                ));
            }

            bool duplicatesIssue = false;

            if (scanForDuplicateDats) 
            {
                // TODO:
            }

            // NOTE: Multiple tabs need viewing in this scenario, justifying a prompt:
            if (duplicatesIssue && unhandledIssue) 
            {
                base.ExecuteCommand(
                    new ShowMessageCmd(
                    "Please review the Problem List tabs carefully. Multiple issues may need attending."));
            }

            DatSummaryVO[] dats = datsSummaries.ToArray();

            // NOTE: Only move on to next Tab if there is something to show in summaries' view.
            if (dats.Length > 0) 
            {
                activeDatsModel.AllStoredDats = dats;

                Facade.Instance?.DatSummaryMediator?.PopulateTable(
                    dats
                    , unitDisplayModel.GetUnit()
                    , unitDisplayModel.GetShowUnitInCells());

                base.ExecuteCommand(new SwitchDatViewCmd(DAT_SCREEN_ENUM.DatManager));
            }
            else {
                base.ExecuteCommand(
                    new ShowMessageCmd(
                        "None of the discovered DAT files are valid. Please look at the Problem List and change your DAT Root directories."
                    ));
            }
        }
    }
}
