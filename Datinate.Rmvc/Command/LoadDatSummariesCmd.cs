using Datinate.Shared.Dat;
using Datinate.Shared.DatGrouper;
using RMVC;
using System.Diagnostics;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class LoadDatSummariesCmd : CommandBase<RVoid>
    {
        private readonly bool scanForDuplicateDats;

        public LoadDatSummariesCmd(bool scanForDuplicateDats)
        {
            this.scanForDuplicateDats = scanForDuplicateDats;
        }

        protected override async Task<RVoid> Run()
        {

            if (base.ActiveDatsModel == null ||
                base.DatDbProxy == null ||
                base.DatDetailsProxy == null ||
                base.UnitDisplayModel == null)
            {
                return await Complete();
            }

            DatGrouperProjectEntry[]? datPaths =
                base.ActiveDatsModel.HierarchyDats;

            if (datPaths == null)
                return await Complete();

            List<string> unrecognisedDATs = new List<string>();

            // Note: We have to send something to force Progress view update:
            await base.Execute(
                new ShowProgressCmd("Fetching DAT Summary Information", 0, datPaths.Length));

            List<DatSummaryVO> datsSummaries = new List<DatSummaryVO>();

            for (int i = 0; i < datPaths.Length; i++)
            {
                var datPath = datPaths[i];

                DatSummaryVO? dbDatEntry =
                    base.DatDbProxy.GetDatSummary(datPath.DatFullpath);

                DatSummaryVO? summary = null;

                string? sha1 = null;

                if (dbDatEntry != null)
                {
                    summary = dbDatEntry;
                }
                else
                {
                    var dat = DatDetailsProxy.GetDat(datPath.DatFullpath, out var rawFile);

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
                    await base.Execute(new ShowProgressCmd(
                        "Fetching DAT Summary Information (" + Path.GetDirectoryName(summary.DatFullpath) + " > " + Path.GetFileNameWithoutExtension(summary.DatFullpath) + ")", i + 1, datPaths.Length));

                    datsSummaries.Add(summary);

                    if (dbDatEntry == null)
                    {
                        base.DatDbProxy.CreateDatEntry(summary, sha1);
                    }
                }
            }

            await base.Execute(new ClearProgressCmd());

            bool unhandledIssue = false;  
            if (unrecognisedDATs.Count > 0) 
            {
                unhandledIssue = true;

                await base.Execute(
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
                await base.Execute(
                    new ShowMessageCmd(
                    "Please review the Problem List tabs carefully. Multiple issues may need attending."));
            }

            DatSummaryVO[] dats = datsSummaries.ToArray();

            // NOTE: Only move on to next Tab if there is something to show in summaries' view.
            if (dats.Length > 0) 
            {
                base.ActiveDatsModel.AllStoredDats = dats;

                base.DatSummaryMediator?.PopulateTable(
                    dats
                    , base.UnitDisplayModel.GetUnit()
                    , base.UnitDisplayModel.GetShowUnitInCells());

                await base.Execute(new SwitchDatViewCmd(DAT_SCREEN_ENUM.DatManager));
            }
            else {
                await base.Execute(
                    new ShowMessageCmd(
                        "None of the discovered DAT files are valid. Please look at the Problem List and change your DAT Root directories."
                    ));
            }

            return await Complete();
        }
    }
}
