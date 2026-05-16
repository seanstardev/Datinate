using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using System.Text;
using System.Xml;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class ExportSoftwareDelegate
    {
        public void Export(
            IReadOnlyList<IGameFamily> curatedFamilies,
            IReadOnlyDictionary<IGameFamily, string> familyUniqueNameDictionary,
            Dictionary<IGamePart, DatGameVO> partDatEntryDictionary,
            DatGrouperProjectDTO project,
            ExportSoftwareOptionsDTO softwareOptions,
            IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> familyMediaDictionary,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary,
            string softwareProjectPath,
            string m3uExportFolder)
        {
            string projectName = project.ProjectName;
            IReadOnlySet<string> excludedDescriptors = project.ExcludedDescriptorCodes;

            bool exportAs1G1R = softwareOptions.ExportAs1G1R;
            bool skipScoringExempt = softwareOptions.SkipScoringExemptFamilies;
            bool skipExcludedGames = softwareOptions.SkipExcludedGames;
            bool exportM3Us = softwareOptions.ExportM3Us;

            string softwareDatName = exportAs1G1R
                ? projectName + " [1G1R]"
                : projectName;

            if (exportM3Us == false && TryGetM3uDisabledViolation(curatedFamilies, out string? m3uDisabledViolation))
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Software export cannot disable M3U output because " + m3uDisabledViolation);
                throw new InvalidOperationException("Software export cannot disable M3U output because " + m3uDisabledViolation);
            }

            IReadOnlyList<IGameFamily> exportFamilies = FilterFamilies(
                curatedFamilies,
                exportAs1G1R,
                skipScoringExempt,
                skipExcludedGames,
                familyMediaDictionary,
                excludedDescriptors);

            Dictionary<IGame, string> exportGamesDictionary = BuildExportGameNames(
                exportFamilies,
                flagFilterSetByGroup,
                softwareIdDatGroupEnumDictionary,
                skipExcludedGames,
                exportAs1G1R);

            XmlDocument softwareDoc = new XmlDocument();
            XmlElement softwareMachinesRootEl = ExportDatGrouperHelper.CreateDatafileDocument(
                softwareDoc,
                softwareDatName,
                softwareDatName,
                ExportDatGrouperHelper.HeaderDate,
                "Datinate",
                "1",
                false);

            string playlistProjectName = softwareDatName + " [Playlist]";

            XmlDocument? playlistDoc = null;
            XmlElement? playlistMachinesRootEl = null;
            XmlElement? playlistMachineEl = null;

            if (exportM3Us)
            {
                playlistDoc = new XmlDocument();
                playlistMachinesRootEl = ExportDatGrouperHelper.CreateDatafileDocument(
                    playlistDoc,
                    playlistProjectName,
                    playlistProjectName,
                    ExportDatGrouperHelper.HeaderDate,
                    "Datinate",
                    "1",
                    true);

                playlistMachineEl = CreatePlaylistMachineEntry(
                    playlistDoc,
                    m3uExportFolder);
            }

            Dictionary<string, string> m3uFilenameContentDictionary =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            HashSet<string> allocatedM3uFilenames =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            HashSet<string> allocatedSoftwareMachineNames =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            HashSet<string> allocatedPlaylistRomNames =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var family in exportFamilies)
            {
                if (familyUniqueNameDictionary.TryGetValue(family, out string? familyName) == false)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Software export could not resolve export family name for: " + family.GetFamilyDisplayName());
                    throw new InvalidOperationException("Software export could not resolve export family name for: " + family.GetFamilyDisplayName());
                }

                string familyPath = familyName;

                foreach (var game in GetExportGames(family, skipExcludedGames, exportAs1G1R))
                {
                    if (exportGamesDictionary.TryGetValue(game, out string? gameName) == false)
                        continue;

                    var m3uResult = AddMachineEntryToDatXml(
                        game,
                        gameName,
                        familyPath,
                        partDatEntryDictionary,
                        softwareMachinesRootEl,
                        softwareDoc,
                        allocatedSoftwareMachineNames);

                    if (exportM3Us == false)
                        continue;

                    string m3uContent = m3uResult.m3uContent.Trim();

                    if (string.IsNullOrWhiteSpace(m3uContent))
                        continue;

                    string uniqueM3uFilename = GetUniqueM3uFilename(
                        m3uResult.m3FilenameWithExt,
                        familyName,
                        allocatedM3uFilenames);

                    if (!string.Equals(uniqueM3uFilename, m3uResult.m3FilenameWithExt, StringComparison.OrdinalIgnoreCase))
                        System.Diagnostics.Debug.WriteLine("WARNING: Duplicate M3U filename during export: " + m3uResult.m3FilenameWithExt + " -> " + uniqueM3uFilename);

                    m3uFilenameContentDictionary[uniqueM3uFilename] = m3uContent;
                    _ = allocatedM3uFilenames.Add(uniqueM3uFilename);

                    AddPlaylistRomToDatXml(
                        uniqueM3uFilename,
                        m3uContent,
                        playlistMachineEl!,
                        playlistDoc!,
                        allocatedPlaylistRomNames);
                }
            }

            ExportDatGrouperHelper.ExportDat(softwareDoc, softwareProjectPath, softwareDatName);

            if (exportM3Us)
            {
                if (playlistMachineEl != null && (playlistMachineEl.SelectNodes("rom")?.Count ?? 0) > 0)
                {
                    ExportDatGrouperHelper.AppendMachineEntryToDatXml(
                        playlistMachinesRootEl!,
                        playlistMachineEl,
                        new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                        "playlist");
                }

                ExportDatGrouperHelper.ExportDat(playlistDoc!, softwareProjectPath, playlistProjectName);
                ExportM3us(m3uFilenameContentDictionary, softwareProjectPath, m3uExportFolder);
            }
        }
        private XmlElement CreatePlaylistMachineEntry(
            XmlDocument doc,
            string m3uExportFolder)
        {
            string machineName = string.IsNullOrWhiteSpace(m3uExportFolder)
                ? "Playlist"
                : m3uExportFolder.Trim();

            XmlElement machineEl = doc.CreateElement("machine");
            machineEl.SetAttribute("name", machineName);

            XmlElement descriptionEl = doc.CreateElement("description");
            _ = machineEl.AppendChild(descriptionEl);
            descriptionEl.InnerText = machineName;

            return machineEl;
        }

        private void AddPlaylistRomToDatXml(
            string m3uFilenameWithExt,
            string m3uContent,
            XmlElement playlistMachineEl,
            XmlDocument doc,
            HashSet<string> allocatedPlaylistRomNames)
        {
            if (string.IsNullOrWhiteSpace(m3uFilenameWithExt))
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Attempted to create playlist DAT rom from invalid M3U filename.");
                throw new InvalidOperationException("Attempted to create playlist DAT rom from invalid M3U filename.");
            }

            if (!allocatedPlaylistRomNames.Add(m3uFilenameWithExt))
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Duplicate playlist DAT rom skipped: " + m3uFilenameWithExt);
                return;
            }

            var romInfo = ExportDatGrouperHelper.BuildTextRomInfo(m3uContent);

            XmlElement romEl = doc.CreateElement("rom");
            _ = playlistMachineEl.AppendChild(romEl);

            romEl.SetAttribute("name", m3uFilenameWithExt);
            romEl.SetAttribute("size", romInfo.size.ToString());

            if (!string.IsNullOrWhiteSpace(romInfo.crc))
                romEl.SetAttribute("crc", romInfo.crc);

            if (!string.IsNullOrWhiteSpace(romInfo.md5))
                romEl.SetAttribute("md5", romInfo.md5);

            if (!string.IsNullOrWhiteSpace(romInfo.sha1))
                romEl.SetAttribute("sha1", romInfo.sha1);
        }
        private bool TryGetM3uDisabledViolation(
            IReadOnlyList<IGameFamily> curatedFamilies,
            out string? violation)
        {
            foreach (var family in curatedFamilies)
            {
                string familyName = family.GetFamilyDisplayName();

                foreach (var game in family.GetAllGames())
                {
                    IGamePart[] parts = game.GetGameParts(false);

                    if (parts.Length != 1)
                    {
                        violation =
                            "'" +
                            familyName +
                            " / " +
                            game.GetNameWithoutExt() +
                            "' has " +
                            parts.Length +
                            " parts. M3U output can only be disabled when every game has exactly one part.";

                        return true;
                    }
                }
            }

            violation = null;
            return false;
        }

        private IReadOnlyList<IGameFamily> FilterFamilies(
            IReadOnlyList<IGameFamily> curatedFamilies,
            bool exportAs1G1R,
            bool skipScoringExempt,
            bool skipExcludedGames,
            IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> familyMediaDictionary,
            IReadOnlySet<string> excludedDescriptors)
        {
            if (skipScoringExempt == false && skipExcludedGames == false)
                return curatedFamilies;

            List<IGameFamily> filteredFamilies = new List<IGameFamily>();

            foreach (var family in curatedFamilies)
            {
                if (skipScoringExempt && familyMediaDictionary.TryGetValue(family, out var media) && media is { })
                {
                    var lookup = new HashSet<string>(media.CheckedDescriptorCodes, StringComparer.OrdinalIgnoreCase);
                    bool hasSharedEntry = excludedDescriptors.Any(lookup.Contains);

                    if (hasSharedEntry)
                        continue;
                }

                if (skipExcludedGames)
                {
                    bool hasAtLeastOneValidExportGame =
                        GetExportGames(family, true, exportAs1G1R).Length > 0;

                    if (hasAtLeastOneValidExportGame == false)
                        continue;
                }

                filteredFamilies.Add(family);
            }

            return filteredFamilies;
        }

        private Dictionary<IGame, string> BuildExportGameNames(
            IReadOnlyList<IGameFamily> exportFamilies,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary,
            bool skipExcludedGames,
            bool exportAs1G1R)
        {
            Dictionary<IGame, string> exportGamesDictionary = new Dictionary<IGame, string>();

            foreach (var family in exportFamilies)
            {
                IGame[] exportGames = GetExportGames(
                    family,
                    skipExcludedGames,
                    exportAs1G1R);

                if (exportGames.Length == 0)
                    continue;

                var familyGamesDictionary = GameEntityNameBuilder.BuildUpdatedGameNames(
                    exportGames,
                    flagFilterSetByGroup,
                    softwareIdDatGroupEnumDictionary);

                foreach (var kvp in familyGamesDictionary)
                    exportGamesDictionary[kvp.Key] = kvp.Value;
            }

            return exportGamesDictionary;
        }

        private IGame[] GetExportGames(
            IGameFamily family,
            bool skipExcludedGames,
            bool exportAs1G1R)
        {
            if (exportAs1G1R)
            {
                IGame parentGame = family.GetParentGame();

                if (ExcludeGame(parentGame, skipExcludedGames))
                    return Array.Empty<IGame>();

                return new[] { parentGame };
            }

            IGame[] allGames = family.GetAllGames();

            if (skipExcludedGames == false)
                return allGames;

            List<IGame> filteredGames = new List<IGame>();

            foreach (var game in allGames)
                if (ExcludeGame(game, skipExcludedGames) == false)
                    filteredGames.Add(game);

            return filteredGames.ToArray();
        }

        private bool ExcludeGame(IGame game, bool skipExcludedGames)
        {
            if (skipExcludedGames == false)
                return false;

            foreach (var part in game.GetGameParts(false))
                if (part.Exclude == false)
                    return false;

            return true;
        }

        private string GetUniqueM3uFilename(
            string m3uFilenameWithExt,
            string familyName,
            IReadOnlySet<string> allocatedFilenames)
        {
            if (allocatedFilenames.Contains(m3uFilenameWithExt) == false)
                return m3uFilenameWithExt;

            string extension = Path.GetExtension(m3uFilenameWithExt);
            string baseName = Path.GetFileNameWithoutExtension(m3uFilenameWithExt);

            string familyQualifiedName = $"{baseName} [{familyName}]{extension}";
            if (allocatedFilenames.Contains(familyQualifiedName) == false)
                return familyQualifiedName;

            int index = 1;

            while (true)
            {
                string candidate = $"{baseName} [{familyName}] [{index}]{extension}";

                if (allocatedFilenames.Contains(candidate) == false)
                    return candidate;

                index++;
            }
        }

        private (string m3FilenameWithExt, string m3uContent) AddMachineEntryToDatXml(
            IGame game,
            string gameNameOverride,
            string familyPath,
            Dictionary<IGamePart, DatGameVO> partDatEntryDictionary,
            XmlElement machinesRootEl,
            XmlDocument doc,
            HashSet<string> allocatedSoftwareMachineNames)
        {
            IGamePart[] gameParts = game.GetGameParts(false);

            Dictionary<IGamePart, DatGameVO> datGameByPart = new();
            Dictionary<IGamePart, string> exportedPartExtensionByPart = new();

            foreach (var part in gameParts)
            {
                if (!partDatEntryDictionary.TryGetValue(part, out DatGameVO? datGame))
                    continue;

                string exportedPartExtension = GetExportedPartExtension(datGame, part);

                if (string.IsNullOrWhiteSpace(exportedPartExtension))
                    continue;

                datGameByPart[part] = datGame;
                exportedPartExtensionByPart[part] = exportedPartExtension;
            }

            // Accessing part info:
            // IGamePart.Tag will be 'flop1', 'flop2', etc for MAME_SL.
            //
            // This should be enough for us to create unique ZIP entries when playlist lines would otherwise duplicate, eg:
            //   2400 A.D/2400ad/2400 A.D. (4am crack) (flop1).zip
            //   2400 A.D/2400ad/2400 A.D. (4am crack) (flop2).zip
            //
            // We do not need to check if a part is MAME or MAME_SL, as we want to employ the same universal tactic whenever a
            // playlist m3u would otherwise include duplicate ZIP lines.
            //
            // If - for some reason - the .Tag is null or whitespace or not distinguishable from other playlist lines,
            // we fall back to:
            //   ../2400 A.D/2400ad/2400 A.D. (4am crack) (Part 1).zip
            //   ../2400 A.D/2400ad/2400 A.D. (4am crack) (Part 2).zip
            Dictionary<IGamePart, string> zipPartDisplayNameOverrides =
                BuildZipPartDisplayNameOverrides(
                    gameParts,
                    exportedPartExtensionByPart,
                    familyPath);

            Dictionary<string, XmlElement> diskMachineElementByPath =
                new Dictionary<string, XmlElement>(StringComparer.OrdinalIgnoreCase);

            List<XmlElement> diskMachineElementsInOrder = new List<XmlElement>();

            StringBuilder m3uContent = new StringBuilder();

            foreach (var part in gameParts)
            {
                if (!datGameByPart.TryGetValue(part, out DatGameVO? datGame))
                    continue;

                if (!exportedPartExtensionByPart.TryGetValue(part, out string? exportedPartExtension))
                    continue;

                bool hasLaunchName = string.IsNullOrWhiteSpace(part.LaunchName) == false;

                if (string.Equals(exportedPartExtension, ".zip", StringComparison.OrdinalIgnoreCase))
                {
                    string partDisplayName =
                        zipPartDisplayNameOverrides.TryGetValue(part, out string? overriddenPartDisplayName)
                            ? overriddenPartDisplayName
                            : part.GetName();

                    string partMachinePath = hasLaunchName
                        ? $"{familyPath}/{part.LaunchName}/{partDisplayName}"
                        : $"{familyPath}/{partDisplayName}";

                    XmlElement machineEl = doc.CreateElement("machine");
                    machineEl.SetAttribute("name", partMachinePath);

                    XmlElement descriptionEl = doc.CreateElement("description");
                    _ = machineEl.AppendChild(descriptionEl);
                    descriptionEl.InnerText = partDisplayName;

                    bool partAdded = false;

                    foreach (var rom in datGame.Roms)
                    {
                        bool isDisk = rom.IsDisk;

                        XmlElement romEl = isDisk ? doc.CreateElement("disk") : doc.CreateElement("rom");
                        _ = machineEl.AppendChild(romEl);

                        string romName = rom.Name;
                        romEl.SetAttribute("name", romName);

                        if (isDisk == false)
                            romEl.SetAttribute("size", rom.Size.ToString());

                        if (isDisk == false && !string.IsNullOrWhiteSpace(rom.Crc))
                            romEl.SetAttribute("crc", rom.Crc);

                        if (isDisk == false && !string.IsNullOrWhiteSpace(rom.Md5))
                            romEl.SetAttribute("md5", rom.Md5);

                        if (!string.IsNullOrWhiteSpace(rom.Sha1))
                            romEl.SetAttribute("sha1", rom.Sha1);

                        partAdded = true;
                    }

                    if (partAdded)
                    {
                        ExportDatGrouperHelper.AppendMachineEntryToDatXml(
                            machinesRootEl,
                            machineEl,
                            allocatedSoftwareMachineNames,
                            "software");

                        _ = m3uContent.AppendLine(
                            BuildM3uPath($"{partMachinePath}{exportedPartExtension}"));
                    }

                    continue;
                }

                string diskMachinePath = hasLaunchName
                    ? $"{familyPath}/{part.LaunchName}"
                    : $"{familyPath}/{gameNameOverride}";

                if (!diskMachineElementByPath.TryGetValue(diskMachinePath, out XmlElement? diskMachineEl))
                {
                    diskMachineEl = doc.CreateElement("machine");
                    diskMachineEl.SetAttribute("name", diskMachinePath);

                    XmlElement descriptionEl = doc.CreateElement("description");
                    _ = diskMachineEl.AppendChild(descriptionEl);
                    descriptionEl.InnerText = gameNameOverride;

                    diskMachineElementByPath.Add(diskMachinePath, diskMachineEl);
                    diskMachineElementsInOrder.Add(diskMachineEl);
                }

                List<string> payloadNames = new List<string>();

                foreach (var rom in datGame.Roms)
                {
                    bool isDisk = rom.IsDisk;

                    XmlElement romEl = isDisk ? doc.CreateElement("disk") : doc.CreateElement("rom");
                    _ = diskMachineEl.AppendChild(romEl);

                    string romName = $"{diskMachinePath}/{rom.Name}";
                    romEl.SetAttribute("name", romName);

                    if (isDisk == false)
                        romEl.SetAttribute("size", rom.Size.ToString());

                    if (isDisk == false && !string.IsNullOrWhiteSpace(rom.Crc))
                        romEl.SetAttribute("crc", rom.Crc);

                    if (isDisk == false && !string.IsNullOrWhiteSpace(rom.Md5))
                        romEl.SetAttribute("md5", rom.Md5);

                    if (!string.IsNullOrWhiteSpace(rom.Sha1))
                        romEl.SetAttribute("sha1", rom.Sha1);

                    payloadNames.Add(rom.Name);
                }

                if (payloadNames.Count == 0)
                    continue;

                foreach (var payloadName in payloadNames)
                    _ = m3uContent.AppendLine(
                        BuildM3uPath($"{diskMachinePath}/{payloadName}{exportedPartExtension}"));
            }

            foreach (var diskMachineEl in diskMachineElementsInOrder)
            {
                ExportDatGrouperHelper.AppendMachineEntryToDatXml(
                    machinesRootEl,
                    diskMachineEl,
                    allocatedSoftwareMachineNames,
                    "software");
            }

            return (gameNameOverride + ".m3u", m3uContent.ToString().Trim());
        }

        private string BuildM3uPath(string exportPath)
        {
            return exportPath;
        }

        private Dictionary<IGamePart, string> BuildZipPartDisplayNameOverrides(
            IGamePart[] gameParts,
            IReadOnlyDictionary<IGamePart, string> exportedPartExtensionByPart,
            string familyPath)
        {
            Dictionary<string, List<IGamePart>> conflictingZipPartsByPath =
                new(StringComparer.OrdinalIgnoreCase);

            foreach (var part in gameParts)
            {
                if (!exportedPartExtensionByPart.TryGetValue(part, out string? exportedPartExtension) ||
                    string.Equals(exportedPartExtension, ".zip", StringComparison.OrdinalIgnoreCase) == false)
                    continue;

                string partDisplayName = part.GetName();

                string partMachinePath = string.IsNullOrWhiteSpace(part.LaunchName)
                    ? $"{familyPath}/{partDisplayName}"
                    : $"{familyPath}/{part.LaunchName}/{partDisplayName}";

                if (!conflictingZipPartsByPath.TryGetValue(partMachinePath, out var conflictingParts))
                {
                    conflictingParts = new List<IGamePart>();
                    conflictingZipPartsByPath.Add(partMachinePath, conflictingParts);
                }

                conflictingParts.Add(part);
            }

            Dictionary<IGamePart, string> zipPartDisplayNameOverrides = new();

            foreach (var kvp in conflictingZipPartsByPath)
            {
                var conflictingParts = kvp.Value;

                if (conflictingParts.Count < 2)
                    continue;

                bool canUseTag =
                    conflictingParts.All(part => string.IsNullOrWhiteSpace(part.Tag) == false) &&
                    conflictingParts
                        .Select(part => part.Tag!.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Count() == conflictingParts.Count;

                if (canUseTag)
                {
                    System.Diagnostics.Debug.WriteLine("INFO: Disambiguating duplicate ZIP playlist lines using IGamePart.Tag for: " + conflictingParts[0].GetName());

                    foreach (var part in conflictingParts)
                        zipPartDisplayNameOverrides[part] = $"{part.GetName()} ({part.Tag!.Trim()})";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Duplicate ZIP playlist lines could not be distinguished by IGamePart.Tag. Falling back to Part 1 / Part 2 ordering for: " + conflictingParts[0].GetName());

                    for (int i = 0; i < conflictingParts.Count; i++)
                    {
                        var part = conflictingParts[i];
                        zipPartDisplayNameOverrides[part] = $"{part.GetName()} (Part {i + 1})";
                    }
                }
            }

            return zipPartDisplayNameOverrides;
        }

        private void ExportM3us(
            Dictionary<string, string> m3uNameContentDictionary,
            string projectPath,
            string m3uExportFolder)
        {
            var m3uExportPath = Path.Combine(projectPath, m3uExportFolder);
            Directory.CreateDirectory(m3uExportPath);

            foreach (var kvp in m3uNameContentDictionary)
            {
                var m3uFilenameWithExt = kvp.Key;
                var content = kvp.Value.Trim();
                var fullpath = Path.Combine(m3uExportPath, m3uFilenameWithExt);
                File.WriteAllText(fullpath, content);
            }
        }

        private string GetExportedPartExtension(DatGameVO datGame, IGamePart part)
        {
            bool hasDisk = false;
            bool hasRom = false;

            foreach (var rom in datGame.Roms)
            {
                if (rom.IsDisk)
                    hasDisk = true;
                else
                    hasRom = true;

                if (hasDisk && hasRom)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Mixed disk/rom payload detected for exported part: " + part.GetName());
                    throw new InvalidOperationException("Mixed disk/rom payload detected for exported part: " + part.GetName());
                }
            }

            if (hasDisk == false && hasRom == false)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: No payloads found for part during export: " + part.GetName());
                return string.Empty;
            }

            return hasDisk ? ".chd" : ".zip";
        }
    }
}
