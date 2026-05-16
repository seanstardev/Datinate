using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{

    /**
     * Take the local left and right dats in the comparison view and compare them:
     */
    internal class CreateDatComparisonCmd : RCommand {
        protected override void Run()
        {
            Facade? instance = Facade.Instance;

            ActiveDatsModel? activeDatsModel = instance?.ActiveDatsModel;
            if (activeDatsModel == null)
                return;

            DatVO? leftDat = activeDatsModel.CompareLeftDat;
            DatVO? rightDat = activeDatsModel.CompareRightDat;

            if (leftDat == null || rightDat == null)
            {
                base.ExecuteCommand(
                    new ShowMessageCmd("Cannot proceed. You must load DATs in both the left and right views to proceed."));
                return;
            }
            else if (leftDat == rightDat)
            {
                base.ExecuteCommand(
                    new ShowMessageCmd("Cannot proceed. The DATs on the left and right sides are the same."));
                return;
            }

            var vo = CreateComparison(leftDat, rightDat);
            base.ExecuteCommand(new SetCompareViewResultCmd(vo));
        }
        private static DatComparisonVO CreateComparison(DatVO leftDat, DatVO rightDat)
        {
            HashSet<string> dupes = GetSharedFingerprints(leftDat, rightDat);

            var middleEntries = new List<DatGameVO>();
            var uniqueLeftEntries = new List<DatGameVO>();
            var uniqueRightEntries = new List<DatGameVO>();

            var sharedLeftDic = new Dictionary<string, DatGameVO>();
            var sharedRightDic = new Dictionary<string, DatGameVO>();

            var problemEntries = new List<DatGameVO>();

            // Left
            foreach (var entry in leftDat.Entries)
            {
                if (dupes.Contains(entry.Fingerprint))
                {
                    if (!sharedLeftDic.TryAdd(entry.Fingerprint, entry))
                    {
                        problemEntries.Add(entry);
                    }
                    else
                        middleEntries.Add(entry);
                }
                else
                    uniqueLeftEntries.Add(entry);
            }

            // Right
            foreach (var entry in rightDat.Entries)
            {
                if (dupes.Contains(entry.Fingerprint))
                {
                    if (!sharedRightDic.TryAdd(entry.Fingerprint, entry))
                        problemEntries.Add(entry);
                }
                else
                    uniqueRightEntries.Add(entry);
            }

            var newLeftDat = new DatVO(
                leftDat.GetDatNameWithoutExt() + " [DiffDat]",
                CreateDiffDatHeader(leftDat.DatHeaderVO),
                uniqueLeftEntries);

            var newRightDat = new DatVO(
                rightDat.GetDatNameWithoutExt() + " [DiffDat]",
                CreateDiffDatHeader(rightDat.DatHeaderVO),
                uniqueRightEntries);

            //
            // Middle
            //
            const string splitter = " ■ ";
            const string empty = "<empty>";

            var reconciledEntries = new List<DatGameVO>();

            foreach (var entry in middleEntries)
            {
                var leftEntry = sharedLeftDic[entry.Fingerprint];
                var rightEntry = sharedRightDic[entry.Fingerprint];

                var leftRoms = leftEntry.Roms;
                var rightRoms = rightEntry.Roms;

                var reconciledRoms = new List<DatRomVO>(leftRoms.Length);

                var usedRight = new bool[rightRoms.Length];
                var matchedRightIndex = new int[leftRoms.Length];
                Array.Fill(matchedRightIndex, -1);

                // First pass: match by non-empty Sha1, 1:1, no re-use
                for (int i = 0; i < leftRoms.Length; i++)
                {
                    var leftRom = leftRoms[i];
                    if (string.IsNullOrEmpty(leftRom.Sha1))
                        continue;

                    for (int j = 0; j < rightRoms.Length; j++)
                    {
                        if (!usedRight[j] && rightRoms[j].Sha1 == leftRom.Sha1)
                        {
                            usedRight[j] = true;
                            matchedRightIndex[i] = j;
                            break;
                        }
                    }
                }

                // Second pass: fill gaps with first unused right ROMs
                for (int i = 0; i < leftRoms.Length; i++)
                {
                    var leftRom = leftRoms[i];

                    DatRomVO rightRom;
                    int j = matchedRightIndex[i];

                    if (j >= 0)
                    {
                        rightRom = rightRoms[j];
                    }
                    else
                    {
                        int k = Array.FindIndex(usedRight, u => !u);
                        if (k >= 0)
                        {
                            usedRight[k] = true;
                            rightRom = rightRoms[k];
                        }
                        else
                        {
                            // Shouldn't happen if counts match, but fall back to leftRom
                            rightRom = leftRom;
                        }
                    }

                    var leftName = leftRom.Name;
                    var rightName = rightRom.Name;

                    string reconciledRomName;
                    if (string.Equals(leftName, rightName, StringComparison.Ordinal))
                    {
                        reconciledRomName = leftName;
                    }
                    else
                    {
                        var ln = string.IsNullOrEmpty(leftName) ? empty : leftName;
                        var rn = string.IsNullOrEmpty(rightName) ? empty : rightName;
                        reconciledRomName = ln + splitter + rn;
                    }

                    // Non-name fields: take from left side (Sha1 matches, so content should be equivalent)
                    reconciledRoms.Add(new DatRomVO(
                        reconciledRomName,
                        leftRom.IsDisk,
                        leftRom.Size,
                        leftRom.Crc,
                        leftRom.Md5,
                        leftRom.Sha1
                    ));
                }

                string reconciledName =
                    leftEntry.Name == rightEntry.Name ?
                    leftEntry.Name :
                    leftEntry.Name + splitter + rightEntry.Name;

                string? reconciledDescription =
                    leftEntry.Description == rightEntry.Description ?
                    leftEntry.Description :
                    leftEntry.Description ?? empty + splitter + rightEntry.Description ?? empty;

                string? reconciledPublisher =
                    leftEntry.Publisher == rightEntry.Publisher ?
                    leftEntry.Publisher :
                    leftEntry.Publisher ?? empty + splitter + rightEntry.Publisher ?? empty;

                string? reconciledRegion =
                    leftEntry.Region == rightEntry.Region ?
                    leftEntry.Region :
                    leftEntry.Region ?? empty + splitter + rightEntry.Region ?? empty;

                string? reconciledDate =
                    leftEntry.Date == rightEntry.Date ?
                    leftEntry.Date :
                    leftEntry.Date ?? empty + splitter + rightEntry.Date ?? empty;

                string? reconciledCategory =
                    leftEntry.Category == rightEntry.Category ?
                    leftEntry.Category :
                    leftEntry.Category ?? empty + splitter + rightEntry.Category ?? empty;

                string? reconciledMameName =
                    leftEntry.MameLaunchName == rightEntry.MameLaunchName ?
                    leftEntry.MameLaunchName :
                    leftEntry.MameLaunchName ?? empty + splitter + rightEntry.MameLaunchName ?? empty;

                string? reconciledParentName =
                    leftEntry.ParentName == rightEntry.ParentName ?
                    leftEntry.ParentName :
                    leftEntry.ParentName ?? empty + splitter + rightEntry.ParentName ?? empty;

                string? reconciledPartOwnerName =
                    leftEntry.PartOwnerName == rightEntry.PartOwnerName ?
                    leftEntry.PartOwnerName :
                    leftEntry.PartOwnerName ?? empty + splitter + rightEntry.PartOwnerName ?? empty;

                reconciledEntries.Add(
                    new DatGameVO(
                        reconciledName,
                        reconciledDescription, 
                        reconciledPublisher, 
                        reconciledRegion, 
                        reconciledDate,
                        reconciledCategory,
                        reconciledMameName,
                        reconciledParentName,
                        reconciledRoms.ToArray(),
                        reconciledPartOwnerName
                    ));
            }

            var middleHeader = new DatHeaderVO(
                DAT_FORMAT_ENUM.DatinateNative
                , CreateDiffDatName(leftDat.GetDatNameWithoutExt() + splitter + rightDat.GetDatNameWithoutExt())
                , "Datinate generated Diff DAT"
                , ""
                , ""
                , "Datinate"
                , "");

            var middleDat = new DatVO(
                string.Empty,
                middleHeader,
                reconciledEntries);

            // TODO: deal with the unlikely scenario where whole games
            // contain the same fingerprint. Untested code around: problemEntries.

            return new DatComparisonVO(newLeftDat, newRightDat, middleDat);
        }

        private static string CreateDiffDatName(string name)
        {
            if (name.StartsWith("[Datinate Diff DAT]"))
                return name;
            else return "[Datinate Diff DAT] " + name;
        }
        private static DatHeaderVO CreateDiffDatHeader(DatHeaderVO header)
        {
            return new DatHeaderVO(
                header.DatTypeEnum,
                CreateDiffDatName(header.Name),
                header.Description,
                header.Category,
                header.Version,
                header.Author,
                header.Comment);
        }

        private static HashSet<string> GetSharedFingerprints(DatVO leftDat, DatVO rightDat)
        {
            var dupes = new HashSet<string>(StringComparer.Ordinal);

            var rightSet = new HashSet<string>(
                rightDat.Entries
                    .Select(e => e.Fingerprint)
                    .Where(f => !string.IsNullOrWhiteSpace(f)),
                StringComparer.Ordinal);

            foreach (var entry in leftDat.Entries)
            {
                var fp = entry.Fingerprint;
                if (!string.IsNullOrWhiteSpace(fp) && rightSet.Contains(fp))
                    dupes.Add(fp);
            }

            return dupes;
        }
    }
}
