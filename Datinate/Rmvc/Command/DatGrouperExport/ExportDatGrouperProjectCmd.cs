using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;
using LookupKey = (string Name, string DatSourceId, string Tag, string MameName, string Fingerprint);

namespace com.RADIO.Datinate.RMVC
{
    public class ExportDatGrouperProjectCmd : RCommandAsync
    {
        private readonly IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaPriorities;
        private readonly ExportSoftwareOptionsDTO softwareOptions;
        private readonly DAT_GROUPER_EXPORT_ENUM exportEnum;

        public ExportDatGrouperProjectCmd(IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaPriorities,
            ExportSoftwareOptionsDTO softwareOptions,
            DAT_GROUPER_EXPORT_ENUM exportEnum)
        {
            this.mediaPriorities = mediaPriorities;
            this.softwareOptions = softwareOptions;
            this.exportEnum = exportEnum;
        }

        protected override async Task RunAsync()
        {
            if (exportEnum == DAT_GROUPER_EXPORT_ENUM.NOT_SET)
                return;

            if (Facade.Instance?.RadioDatModel is not { } radioModel ||
                radioModel.ActiveProject is not { } activeProject ||
                Facade.Instance?.DatGrouperModel is not { } grouperModel ||
                Facade.Instance?.ExportDatGrouperProjectProxy is not { } exportProxy ||
                Facade.Instance?.Shell is not { } shell)
                return;

            IReadOnlyList<IGameFamily> curatedFamilies = grouperModel.CuratedFamilies;

            if (exportEnum == DAT_GROUPER_EXPORT_ENUM.Software)
            {
                if (exportProxy.GetSoftwareProjectPathExists(activeProject))
                {
                    bool allowDelete = await shell.ShowMessageBox(
                        "Attention",
                        $"The existing Software Export content for Project '{activeProject.ProjectName}' will be deleted if you continue. Do you wish to proceed?",
                        true);

                    if (allowDelete == false)
                        return;
                }

                IReadOnlyList<GamePartVO> sourceParts = radioModel.GetSourceGameParts();

                Dictionary<IGamePart, DatGameVO> payloadDictionary =
                    BuildSoftwarePayloadDictionary(
                        sourceParts,
                        curatedFamilies);

                IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> familyMediaDictionary =
                    softwareOptions.SkipScoringExemptFamilies
                        ? radioModel.CreateExportCollection(curatedFamilies)
                        : new Dictionary<IGameFamily, IMediaCollectionImportExport?>();

                var flagFilterSet =
                    Facade.Instance?.AppDataProxy?.FlagFilterSetByGroup ??
                    new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>();

                Dictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary =
                    new Dictionary<string, DAT_GROUP_ENUM>();

                foreach (var sourceIdContent in radioModel.SourceIdContentDictionary)
                    if (sourceIdContent.Value.CollectionSetEnum == COLLECTION_SET_ENUM.Software)
                        softwareIdDatGroupEnumDictionary[sourceIdContent.Key] = sourceIdContent.Value.DatGroupEnum;

                exportProxy.ExportSoftware(
                    curatedFamilies,
                    payloadDictionary,
                    activeProject,
                    softwareOptions,
                    familyMediaDictionary,
                    flagFilterSet,
                    softwareIdDatGroupEnumDictionary);

                OpenFolderInExplorer(exportProxy.GetSoftwareProjectPath(activeProject));

                await shell.ShowMessageBox("Attention", "Export completed.");
                return;
            }

            if (exportEnum == DAT_GROUPER_EXPORT_ENUM.Media)
            {
                if (exportProxy.GetMediaProjectPathExists(activeProject))
                {
                    bool allowDelete = await shell.ShowMessageBox(
                        "Attention",
                        $"The existing Media Export content for Project '{activeProject.ProjectName}' will be deleted if you continue. Do you wish to proceed?",
                        true);

                    if (allowDelete == false)
                        return;
                }

                var familyMediaDictionary = radioModel.CreateExportCollection(curatedFamilies);
                var resourceDetailsDictionary = radioModel.GetResourceDetailsDictionary(curatedFamilies);

                exportProxy.ExportMedia(
                    curatedFamilies,
                    activeProject,
                    softwareOptions,
                    mediaPriorities,
                    familyMediaDictionary,
                    resourceDetailsDictionary,
                    radioModel.CreateSourceDatExportSnapshot(),
                    radioModel.SourceIdContentDictionary);

                OpenFolderInExplorer(exportProxy.GetMediaProjectPath(activeProject));

                await shell.ShowMessageBox("Attention", "Export completed.");
                return;
            }
        }
        private static Dictionary<IGamePart, DatGameVO> BuildSoftwarePayloadDictionary(
            IReadOnlyList<GamePartVO> sourceParts,
            IReadOnlyList<IGameFamily> curatedFamilies)
        {
            Dictionary<LookupKey, GamePartVO> sourcePartLookup =
                new Dictionary<LookupKey, GamePartVO>(sourceParts.Count);

            LookupKey? keyA = null;
            LookupKey? keyB = null;

            foreach (var sourcePart in sourceParts)
            {
                var lookupKey = CreateLookupKey(sourcePart);

                sourcePartLookup[lookupKey] = sourcePart;
            }

            Dictionary<IGamePart, DatGameVO> payloadDictionary =
                new Dictionary<IGamePart, DatGameVO>(sourceParts.Count);

            int hitCount = 0;
            int missCount = 0;

            foreach (var family in curatedFamilies)
            {
                string familyName = family.GetFamilyDisplayName();

                foreach (var game in family.GetAllGames())
                {
                    string gameName = game.GetNameWithoutExt();

                    foreach (var part in game.GetGameParts(false))
                    {
                        var lookupKey = CreateLookupKey(part);

                        if (sourcePartLookup.TryGetValue(lookupKey, out var sourcePart))
                        {
                            payloadDictionary[part] = sourcePart.DatGameSource;
                            hitCount++;
                        }
                        else
                        {
                            missCount++;
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine(">>> Export Payload Creation Complete.");
            System.Diagnostics.Debug.WriteLine(">>> Hits: " + hitCount);
            System.Diagnostics.Debug.WriteLine(">>> Misses: " + missCount);
            System.Diagnostics.Debug.WriteLine(">>> Payload Size: " + payloadDictionary.Count);

            System.Diagnostics.Debug.WriteLine("");

            if (keyA != null && keyB != null)
                DebugCompareLookupKeys((LookupKey)keyA, (LookupKey)keyB);

            return payloadDictionary;
        }
        private static void OpenFolderInExplorer(string? folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            if (Directory.Exists(folderPath) == false)
                return;

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Failed to open export folder: " + folderPath);
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private static LookupKey CreateLookupKey(GamePartVO sourcePart)
        {
            string nameKey = sourcePart.Name ?? string.Empty;
            string datSourceIdKey = sourcePart.DatSourceID?.ToString() ?? string.Empty;
            string tagKey = sourcePart.Tag?.ToString() ?? string.Empty;

            // NOTE: Do not use MameName here:
            string mameNameKey = sourcePart.LaunchName ?? string.Empty;
            string fingerprintKey = sourcePart.Fingerprint?.ToString() ?? string.Empty;

            return (nameKey, datSourceIdKey, tagKey, mameNameKey, fingerprintKey);
        }

        private static LookupKey CreateLookupKey(IGamePart part)
        {
            string nameKey = part.GetName() ?? string.Empty;
            string datSourceIdKey = part.GetDirectoryId()?.ToString() ?? string.Empty;
            string tagKey = part.Tag?.ToString() ?? string.Empty;
            string mameNameKey = part.LaunchName ?? string.Empty;
            string fingerprintKey = part.Fingerprint?.ToString() ?? string.Empty;

            return (nameKey, datSourceIdKey, tagKey, mameNameKey, fingerprintKey);
        }

        private static void DebugWriteLookupKey(
            LookupKey key,
            string indent = "")
        {
            System.Diagnostics.Debug.WriteLine(indent + "Part Name: " + key.Name);
            System.Diagnostics.Debug.WriteLine(indent + "DatSourceID: " + key.DatSourceId);
            System.Diagnostics.Debug.WriteLine(indent + "Tag: " + key.Tag);
            System.Diagnostics.Debug.WriteLine(indent + "LaunchName: " + key.MameName);
            System.Diagnostics.Debug.WriteLine(indent + "Fingerprint: " + key.Fingerprint);
        }

        private static void DebugCompareLookupKeys(
            LookupKey left,
            LookupKey right,
            string leftHeading = "*** LEFT",
            string rightHeading = "*** RIGHT")
        {
            System.Diagnostics.Debug.WriteLine(leftHeading);
            DebugWriteLookupKey(left, "  ");
            System.Diagnostics.Debug.WriteLine(rightHeading);
            DebugWriteLookupKey(right, "  ");

            DebugWriteFieldComparison("Part Name", left.Name, right.Name);
            DebugWriteFieldComparison("DatSourceID", left.DatSourceId, right.DatSourceId);
            DebugWriteFieldComparison("Tag", left.Tag, right.Tag);
            DebugWriteFieldComparison("LaunchName", left.MameName, right.MameName);
            DebugWriteFieldComparison("Fingerprint", left.Fingerprint, right.Fingerprint);

            bool exactMatch =
                left.Name == right.Name &&
                left.DatSourceId == right.DatSourceId &&
                left.Tag == right.Tag &&
                left.MameName == right.MameName &&
                left.Fingerprint == right.Fingerprint;

            System.Diagnostics.Debug.WriteLine(exactMatch ? ">>> LOOKUPS MATCH" : ">>> LOOKUPS DO NOT MATCH");
            System.Diagnostics.Debug.WriteLine(string.Empty);
        }

        private static void DebugWriteFieldComparison(
            string label,
            string leftValue,
            string rightValue)
        {
            bool match = leftValue == rightValue;

            System.Diagnostics.Debug.WriteLine(match
                ? "  [MATCH] " + label
                : "  [MISS] " + label);

            if (!match)
            {
                System.Diagnostics.Debug.WriteLine("    LEFT : " + leftValue);
                System.Diagnostics.Debug.WriteLine("    RIGHT: " + rightValue);
            }
        }
    }
}
