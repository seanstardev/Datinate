using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Rb;
using RadioLibCore.RadioDat;
using System.Xml;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class ExportMediaDatDelegate
    {
        private const string ReleaseFolderName = "release";

        public void Export(
            IReadOnlyList<IGameFamily> curatedFamilies,
            Dictionary<IGameFamily, string> familyUniqueNameDictionary,
            DatGrouperProjectDTO project,
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaOptions,
            IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> familyMediaDictionary,
            IReadOnlyDictionary<IGameFamily, IReadOnlyDictionary<string, ResourceDetailsDTO>> familyResourceDetailsDictionary,
            IReadOnlyDictionary<IGameFamily, InfoSpec> infoSpecDictionary,
            IReadOnlyDictionary<string, DatVO> sourceIdDatDictionary,
            IReadOnlyDictionary<string, ISourceDefinition> sourceIdContentDictionary,
            string mediaDatName,
            string mediaProjectPath)
        {
            XmlDocument mediaDoc = new XmlDocument();

            XmlElement mediaRootEl = ExportDatGrouperHelper.CreateDatafileDocument(
                mediaDoc,
                mediaDatName,
                mediaDatName,
                ExportDatGrouperHelper.HeaderDate,
                "Datinate",
                "1",
                true);

            HashSet<string> allocatedMachineNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var family in curatedFamilies)
            {
                if (!familyUniqueNameDictionary.TryGetValue(family, out string? familyName) ||
                    string.IsNullOrWhiteSpace(familyName))
                {
                    familyName = family.GetFamilyDisplayName();
                }

                XmlElement machineEl = mediaDoc.CreateElement("machine");
                machineEl.SetAttribute("name", familyName);

                XmlElement descriptionEl = mediaDoc.CreateElement("description");
                _ = machineEl.AppendChild(descriptionEl);
                descriptionEl.InnerText = familyName;

                bool hasContent = false;
                HashSet<string> allocatedTargetRomNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                HashSet<MEDIA_TYPE_ENUM> exportedMediaTypes = new HashSet<MEDIA_TYPE_ENUM>();

                _ = infoSpecDictionary.TryGetValue(family, out InfoSpec? infoSpec);

                if (infoSpec != null)
                {
                    string infoTargetRomName = CombineDatPath(familyName, R2DatWebFlag.Media.INFO);

                    AppendInfoRom(
                        mediaDoc,
                        machineEl,
                        infoTargetRomName,
                        infoSpec);

                    _ = allocatedTargetRomNames.Add(infoTargetRomName);
                    hasContent = true;
                }

                if (familyMediaDictionary.TryGetValue(family, out var mediaCollection) &&
                    mediaCollection != null)
                {
                    if (!familyResourceDetailsDictionary.TryGetValue(family, out var resourceDetailsDictionary))
                    {
                        resourceDetailsDictionary =
                            new Dictionary<string, ResourceDetailsDTO>(StringComparer.OrdinalIgnoreCase);
                    }

                    foreach (var option in mediaOptions)
                    {
                        MEDIA_TYPE_ENUM mediaType = option.MediaTypeEnum;

                        if (!IsExportableMediaType(mediaType))
                            continue;

                        if (exportedMediaTypes.Contains(mediaType))
                            continue;

                        MediaRomExportCandidate? candidate = TryCreateMediaRomCandidate(
                            familyName,
                            mediaType,
                            option,
                            mediaCollection,
                            resourceDetailsDictionary,
                            sourceIdDatDictionary,
                            sourceIdContentDictionary);

                        if (candidate == null)
                            continue;

                        bool appendedCandidateContent = AppendCandidateFiles(
                            mediaDoc,
                            machineEl,
                            candidate,
                            allocatedTargetRomNames);

                        if (!appendedCandidateContent)
                            continue;

                        _ = exportedMediaTypes.Add(mediaType);
                        hasContent = true;
                    }

                    if (infoSpec != null)
                    {
                        bool appendedReleaseContent = AppendReleaseMediaRoms(
                            mediaDoc,
                            machineEl,
                            familyName,
                            infoSpec,
                            mediaCollection,
                            resourceDetailsDictionary,
                            sourceIdDatDictionary,
                            sourceIdContentDictionary,
                            allocatedTargetRomNames);

                        if (appendedReleaseContent)
                            hasContent = true;
                    }
                }

                if (!hasContent)
                    continue;

                ExportDatGrouperHelper.AppendMachineEntryToDatXml(
                    mediaRootEl,
                    machineEl,
                    allocatedMachineNames,
                    "media");
            }

            ExportDatGrouperHelper.ExportDat(mediaDoc, mediaProjectPath, mediaDatName);
        }

        private bool AppendCandidateFiles(
            XmlDocument doc,
            XmlElement machineEl,
            MediaRomExportCandidate candidate,
            HashSet<string> allocatedTargetRomNames)
        {
            bool appended = false;

            foreach (var file in candidate.Files)
            {
                if (!allocatedTargetRomNames.Add(file.TargetRomName))
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Duplicate media DAT target skipped: " + file.TargetRomName);
                    continue;
                }

                AppendSourceRom(
                    doc,
                    machineEl,
                    file.TargetRomName,
                    file.Rom);

                appended = true;
            }

            return appended;
        }

        private bool AppendReleaseMediaRoms(
            XmlDocument doc,
            XmlElement machineEl,
            string familyName,
            InfoSpec infoSpec,
            IMediaCollectionImportExport mediaCollection,
            IReadOnlyDictionary<string, ResourceDetailsDTO> resourceDetailsDictionary,
            IReadOnlyDictionary<string, DatVO> sourceIdDatDictionary,
            IReadOnlyDictionary<string, ISourceDefinition> sourceIdContentDictionary,
            HashSet<string> allocatedTargetRomNames)
        {
            if (string.IsNullOrWhiteSpace(infoSpec.ReleasesSourceId))
                return false;

            if (infoSpec.AssetKeys.Count == 0)
                return false;

            string sourceId = infoSpec.ReleasesSourceId;

            if (!mediaCollection.SourceIdAssignedItemDictionary.TryGetValue(sourceId, out var assignedItem))
                return false;

            if (!sourceIdDatDictionary.TryGetValue(sourceId, out var sourceDat))
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Release media export could not find source DAT for source id: " + sourceId);
                return false;
            }

            if (!sourceIdContentDictionary.TryGetValue(sourceId, out var sourceDefinition))
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Release media export could not find source definition for source id: " + sourceId);
                return false;
            }

            _ = resourceDetailsDictionary.TryGetValue(sourceId, out var resourceDetails);

            IReadOnlyList<DatRomVO> sourceRoms = ResolveSourceRoms(
                sourceDat,
                sourceDefinition,
                assignedItem,
                resourceDetails);

            if (sourceRoms.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine(
                    "WARNING: Release media export could not resolve source roms for source id '" +
                    sourceId +
                    "', entry '" +
                    assignedItem.EntryName +
                    "', lookup '" +
                    assignedItem.LookupName +
                    "'.");

                return false;
            }

            bool appended = false;

            foreach (var sourceRom in sourceRoms)
            {
                MediaRomExportFile? releaseFile = TryCreateReleaseMediaFile(
                    familyName,
                    sourceRom,
                    infoSpec.AssetKeys);

                if (releaseFile == null)
                    continue;

                if (!allocatedTargetRomNames.Add(releaseFile.TargetRomName))
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Duplicate release media DAT target skipped: " + releaseFile.TargetRomName);
                    continue;
                }

                AppendSourceRom(
                    doc,
                    machineEl,
                    releaseFile.TargetRomName,
                    releaseFile.Rom);

                appended = true;
            }

            return appended;
        }

        private MediaRomExportFile? TryCreateReleaseMediaFile(
            string familyName,
            DatRomVO sourceRom,
            IReadOnlySet<string> releaseAssetKeys)
        {
            string sourceRomName = NormalizeDatPath(sourceRom.Name);

            if (string.IsNullOrWhiteSpace(sourceRomName))
                return null;

            string filename = GetDatFileName(sourceRomName);

            if (string.IsNullOrWhiteSpace(filename))
                return null;

            MEDIA_TYPE_ENUM mediaType = R2DatWebFlag.Media.GetTypeEnum(filename);

            if (!IsExportableMediaType(mediaType))
                return null;

            if (!TryGetReleaseMediaInputFlag(mediaType, out string? inputFlag))
                return null;

            if (!TryGetReleaseMediaOutputFlag(mediaType, out string? outputFlag))
                return null;

            string? releaseAssetKey = CreateReleaseAssetKeyFromFilename(
                filename,
                inputFlag);

            if (string.IsNullOrWhiteSpace(releaseAssetKey))
                return null;

            if (!releaseAssetKeys.Contains(releaseAssetKey))
                return null;

            string extension = Path.GetExtension(filename);
            string targetFilename = "[" + outputFlag + "]" + extension;

            string targetRomName = CombineDatPath(
                CombineDatPath(
                    CombineDatPath(familyName, ReleaseFolderName),
                    releaseAssetKey),
                targetFilename);

            return new MediaRomExportFile(sourceRom, targetRomName);
        }

        private string? CreateReleaseAssetKeyFromFilename(
            string filename,
            string inputFlag)
        {
            string cleansed = filename.Trim();

            string bracketPrefix = "[" + inputFlag + "]";

            if (!cleansed.StartsWith(bracketPrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            cleansed = cleansed.Substring(bracketPrefix.Length).TrimStart();

            string extension = Path.GetExtension(cleansed);

            if (!string.IsNullOrWhiteSpace(extension))
                cleansed = cleansed.Substring(0, cleansed.Length - extension.Length).TrimEnd();

            return string.IsNullOrWhiteSpace(cleansed) ? null : cleansed;
        }

        private MediaRomExportCandidate? TryCreateMediaRomCandidate(
            string familyName,
            MEDIA_TYPE_ENUM mediaType,
            DatGrouperMediaExportEntryDTO option,
            IMediaCollectionImportExport mediaCollection,
            IReadOnlyDictionary<string, ResourceDetailsDTO> resourceDetailsDictionary,
            IReadOnlyDictionary<string, DatVO> sourceIdDatDictionary,
            IReadOnlyDictionary<string, ISourceDefinition> sourceIdContentDictionary)
        {
            if (!TryGetMediaOutputFlag(mediaType, out string? mediaFlag))
                return null;

            var assignedItems = mediaCollection.SourceIdAssignedItemDictionary;

            foreach (var source in option.Sources)
            {
                if (!source.Include)
                    continue;

                if (string.IsNullOrWhiteSpace(source.SourceId))
                    continue;

                if (!assignedItems.TryGetValue(source.SourceId, out var assignedItem))
                    continue;

                if (!sourceIdDatDictionary.TryGetValue(source.SourceId, out var sourceDat))
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Media DAT export could not find source DAT for source id: " + source.SourceId);
                    continue;
                }

                if (!sourceIdContentDictionary.TryGetValue(source.SourceId, out var sourceDefinition))
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Media DAT export could not find source definition for source id: " + source.SourceId);
                    continue;
                }

                _ = resourceDetailsDictionary.TryGetValue(source.SourceId, out var resourceDetails);

                IReadOnlyList<DatRomVO> sourceRoms = ResolveSourceRoms(
                    sourceDat,
                    sourceDefinition,
                    assignedItem,
                    resourceDetails);

                if (sourceRoms.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "WARNING: Media DAT export could not resolve source roms for source id '" +
                        source.SourceId +
                        "', entry '" +
                        assignedItem.EntryName +
                        "', lookup '" +
                        assignedItem.LookupName +
                        "'.");

                    continue;
                }

                IReadOnlyList<DatRomVO> resourceRoms = GetResourceRomsInSourceDatOrder(
                    mediaType,
                    sourceRoms,
                    resourceDetails);

                if (resourceRoms.Count > 0)
                {
                    if (ShouldExportResourceScreensAsFolder(mediaType, sourceDefinition, resourceRoms))
                    {
                        return CreateResourceScreenCandidate(
                            familyName,
                            mediaFlag,
                            resourceRoms);
                    }

                    return CreateCandidateFromSelectedRom(
                        familyName,
                        mediaFlag,
                        resourceRoms[0]);
                }

                if (!SourceRepresentsMediaType(sourceDefinition, mediaType))
                    continue;

                if (ShouldExportSourceRomsAsFolder(mediaType, sourceRoms))
                    return CreateFolderCandidate(familyName, mediaFlag, sourceRoms);

                DatRomVO? selectedRom = sourceRoms.FirstOrDefault();

                if (selectedRom == null)
                    continue;

                return CreateCandidateFromSelectedRom(
                    familyName,
                    mediaFlag,
                    selectedRom);
            }

            return null;
        }

        private MediaRomExportCandidate CreateCandidateFromSelectedRom(
            string familyName,
            string mediaFlag,
            DatRomVO selectedRom)
        {
            if (IsArchiveRom(selectedRom))
            {
                return CreateFolderCandidate(
                    familyName,
                    mediaFlag,
                    new[] { selectedRom });
            }

            string extension = Path.GetExtension(NormalizeDatPath(selectedRom.Name));
            string targetFilename = "[" + mediaFlag + "]" + extension;
            string targetRomName = CombineDatPath(familyName, targetFilename);

            return new MediaRomExportCandidate(
                new[]
                {
                    new MediaRomExportFile(selectedRom, targetRomName)
                });
        }

        private MediaRomExportCandidate CreateResourceScreenCandidate(
            string familyName,
            string mediaFlag,
            IReadOnlyList<DatRomVO> screenRoms)
        {
            var files = new List<MediaRomExportFile>();

            DatRomVO rootRom = screenRoms[0];
            string rootExtension = Path.GetExtension(NormalizeDatPath(rootRom.Name));
            string rootTargetFilename = "[" + mediaFlag + "]" + rootExtension;
            string rootTargetRomName = CombineDatPath(familyName, rootTargetFilename);

            files.Add(new MediaRomExportFile(rootRom, rootTargetRomName));

            for (int i = 0; i < screenRoms.Count; i++)
            {
                DatRomVO screenRom = screenRoms[i];
                string extension = Path.GetExtension(NormalizeDatPath(screenRom.Name));
                string targetFilename = "[" + mediaFlag + "] " + i + extension;

                string targetRomName = CombineDatPath(
                    CombineDatPath(familyName, mediaFlag),
                    targetFilename);

                files.Add(new MediaRomExportFile(screenRom, targetRomName));
            }

            return new MediaRomExportCandidate(files);
        }

        private MediaRomExportCandidate CreateFolderCandidate(
            string familyName,
            string mediaFlag,
            IReadOnlyList<DatRomVO> sourceRoms)
        {
            var files = new List<MediaRomExportFile>();

            foreach (var sourceRom in sourceRoms)
            {
                string sourceRomName = NormalizeDatPath(sourceRom.Name);

                if (string.IsNullOrWhiteSpace(sourceRomName))
                    continue;

                string targetRomName = CombineDatPath(
                    CombineDatPath(familyName, mediaFlag),
                    sourceRomName);

                files.Add(new MediaRomExportFile(sourceRom, targetRomName));
            }

            return new MediaRomExportCandidate(files);
        }

        private IReadOnlyList<DatRomVO> GetResourceRomsInSourceDatOrder(
            MEDIA_TYPE_ENUM mediaType,
            IReadOnlyList<DatRomVO> sourceRoms,
            ResourceDetailsDTO? resourceDetails)
        {
            if (resourceDetails == null)
                return Array.Empty<DatRomVO>();

            var resourceRomNames = resourceDetails
                .GetRomNamesOfType(mediaType)
                .Select(NormalizeDatPath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (resourceRomNames.Count == 0)
                return Array.Empty<DatRomVO>();

            var list = new List<DatRomVO>();

            foreach (var rom in sourceRoms)
            {
                if (resourceRomNames.Contains(NormalizeDatPath(rom.Name)))
                    list.Add(rom);
            }

            return list;
        }

        private IReadOnlyList<DatRomVO> ResolveSourceRoms(
            DatVO sourceDat,
            ISourceDefinition sourceDefinition,
            IAssignedMediaItem assignedItem,
            ResourceDetailsDTO? resourceDetails)
        {
            string entryName = NormalizeDatPath(assignedItem.EntryName);
            string lookupName = NormalizeDatPath(assignedItem.LookupName);

            if (sourceDefinition.DatSubset is { } subset &&
                !string.IsNullOrWhiteSpace(subset.Entry))
            {
                string subsetEntryName = NormalizeDatPath(subset.Entry);
                string subsetRomName = BuildSubsetRomName(subset, lookupName);

                foreach (var entry in sourceDat.Entries)
                {
                    if (!string.Equals(NormalizeDatPath(entry.Name), subsetEntryName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var list = new List<DatRomVO>();

                    foreach (var rom in entry.Roms)
                    {
                        if (string.Equals(NormalizeDatPath(rom.Name), subsetRomName, StringComparison.OrdinalIgnoreCase))
                            list.Add(rom);
                    }

                    return list;
                }

                return Array.Empty<DatRomVO>();
            }

            DatGameVO? datEntry = FindDatEntry(
                sourceDat,
                entryName,
                lookupName,
                resourceDetails?.LookupName);

            if (datEntry == null)
                return Array.Empty<DatRomVO>();

            return datEntry.Roms;
        }

        private DatGameVO? FindDatEntry(
            DatVO sourceDat,
            params string?[] candidateNames)
        {
            HashSet<string> normalizedCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var candidateName in candidateNames)
            {
                string normalized = NormalizeDatPath(candidateName);

                if (!string.IsNullOrWhiteSpace(normalized))
                    _ = normalizedCandidates.Add(normalized);
            }

            if (normalizedCandidates.Count == 0)
                return null;

            foreach (var entry in sourceDat.Entries)
            {
                if (normalizedCandidates.Contains(NormalizeDatPath(entry.Name)))
                    return entry;
            }

            return null;
        }

        private string BuildSubsetRomName(
            DatSubsetFilter subset,
            string lookupName)
        {
            string build = string.Empty;

            if (!string.IsNullOrWhiteSpace(subset.Path))
                build += NormalizeDatPath(subset.Path) + "/";

            build += NormalizeDatPath(lookupName);
            return build;
        }

        private bool SourceRepresentsMediaType(
            ISourceDefinition sourceDefinition,
            MEDIA_TYPE_ENUM mediaType)
        {
            if (string.IsNullOrWhiteSpace(sourceDefinition.Source))
                return false;

            string source = sourceDefinition.Source.Trim();

            if (Enum.TryParse(source, true, out MEDIA_TYPE_ENUM sourceMediaType) &&
                sourceMediaType == mediaType)
            {
                return true;
            }

            string enumStyleSource = source.Replace("-", "_");

            if (Enum.TryParse(enumStyleSource, true, out sourceMediaType) &&
                sourceMediaType == mediaType)
            {
                return true;
            }

            return R2DatWebFlag.Media.GetTypeEnum(source) == mediaType;
        }

        private bool IsExportableMediaType(MEDIA_TYPE_ENUM mediaType)
        {
            return mediaType != MEDIA_TYPE_ENUM.NOT_SET &&
                   mediaType != MEDIA_TYPE_ENUM.Unspecified &&
                   mediaType != MEDIA_TYPE_ENUM.Info &&
                   mediaType != MEDIA_TYPE_ENUM.Info_About &&
                   mediaType != MEDIA_TYPE_ENUM.Info_Credits &&
                   mediaType != MEDIA_TYPE_ENUM.Info_Releases;
        }

        private bool ShouldExportResourceScreensAsFolder(
            MEDIA_TYPE_ENUM mediaType,
            ISourceDefinition sourceDefinition,
            IReadOnlyList<DatRomVO> resourceRoms)
        {
            return mediaType == MEDIA_TYPE_ENUM.Snap &&
                   sourceDefinition.CollectionSetEnum == COLLECTION_SET_ENUM.Resource &&
                   resourceRoms.Count > 1;
        }

        private bool ShouldExportSourceRomsAsFolder(
            MEDIA_TYPE_ENUM mediaType,
            IReadOnlyList<DatRomVO> sourceRoms)
        {
            if (mediaType == MEDIA_TYPE_ENUM.Soundtrack)
                return true;

            if (sourceRoms.Count > 1)
                return true;

            if (sourceRoms.Count == 1 && IsArchiveRom(sourceRoms[0]))
                return true;

            return false;
        }

        private bool IsArchiveRom(DatRomVO rom)
        {
            string extension = Path.GetExtension(NormalizeDatPath(rom.Name));

            return IsArchiveExtension(extension);
        }

        private bool IsArchiveExtension(string? extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            extension = extension.Trim().ToLowerInvariant();

            return extension == ".zip" ||
                   extension == ".7z" ||
                   extension == ".rar" ||
                   extension == ".tar" ||
                   extension == ".gz" ||
                   extension == ".tgz" ||
                   extension == ".bz2" ||
                   extension == ".tbz" ||
                   extension == ".tbz2" ||
                   extension == ".xz" ||
                   extension == ".txz" ||
                   extension == ".lz" ||
                   extension == ".lzma" ||
                   extension == ".zst" ||
                   extension == ".cab" ||
                   extension == ".lha" ||
                   extension == ".lzh" ||
                   extension == ".arc" ||
                   extension == ".arj";
        }

        private bool TryGetReleaseMediaInputFlag(
            MEDIA_TYPE_ENUM mediaType,
            out string? flag)
        {
            if (mediaType == MEDIA_TYPE_ENUM.Thumb_Release)
            {
                flag = R2DatWebFlag.Media.THUMB_RELEASE;
                return true;
            }

            return TryGetMediaOutputFlag(mediaType, out flag);
        }

        private bool TryGetReleaseMediaOutputFlag(
            MEDIA_TYPE_ENUM mediaType,
            out string? flag)
        {
            if (mediaType == MEDIA_TYPE_ENUM.Thumb_Release)
            {
                flag = R2DatWebFlag.Media.THUMB;
                return true;
            }

            return TryGetMediaOutputFlag(mediaType, out flag);
        }

        private bool TryGetMediaOutputFlag(
            MEDIA_TYPE_ENUM mediaType,
            out string? flag)
        {
            // TODO: This should be centralised:
            flag = mediaType switch
            {
                MEDIA_TYPE_ENUM.Advert => R2DatWebFlag.Media.OTHER_ADVERT,
                MEDIA_TYPE_ENUM.Box => R2DatWebFlag.Media.BOX,
                MEDIA_TYPE_ENUM.Box_Back => R2DatWebFlag.Media.BOX_BACK,
                MEDIA_TYPE_ENUM.Box_Inlay => R2DatWebFlag.Media.BOX_INLAY,
                MEDIA_TYPE_ENUM.Box_Side => R2DatWebFlag.Media.BOX_SIDE,
                MEDIA_TYPE_ENUM.Box_Top => R2DatWebFlag.Media.BOX_TOP,
                MEDIA_TYPE_ENUM.Box_Bottom => R2DatWebFlag.Media.BOX_BOTTOM,

                MEDIA_TYPE_ENUM.Manual => R2DatWebFlag.Media.MANUAL,
                MEDIA_TYPE_ENUM.Manual_Back => R2DatWebFlag.Media.MANUAL_BACK,
                MEDIA_TYPE_ENUM.Manual_Front => R2DatWebFlag.Media.MANUAL_FRONT,

                MEDIA_TYPE_ENUM.Media => R2DatWebFlag.Media.MEDIA,
                MEDIA_TYPE_ENUM.Media_Back => R2DatWebFlag.Media.MEDIA_BACK,
                MEDIA_TYPE_ENUM.Media_Label => "media-label",
                MEDIA_TYPE_ENUM.Media_Top => "media-top",

                MEDIA_TYPE_ENUM.Other => R2DatWebFlag.Media.OTHER,
                MEDIA_TYPE_ENUM.Other_Map => R2DatWebFlag.Media.OTHER_MAP,
                MEDIA_TYPE_ENUM.Other_Overlay => "other-overlay",
                MEDIA_TYPE_ENUM.Other_Reference_Card => R2DatWebFlag.Media.OTHER_REFERENCE_CARD,
                MEDIA_TYPE_ENUM.Other_Hardware => R2DatWebFlag.Media.OTHER_HARDWARE,

                MEDIA_TYPE_ENUM.Snap => R2DatWebFlag.Media.SNAP,
                MEDIA_TYPE_ENUM.Title => R2DatWebFlag.Media.TITLE,
                MEDIA_TYPE_ENUM.Video => R2DatWebFlag.Media.VIDEO,
                MEDIA_TYPE_ENUM.Thumb => R2DatWebFlag.Media.THUMB,
                MEDIA_TYPE_ENUM.Thumb_Release => R2DatWebFlag.Media.THUMB_RELEASE,
                MEDIA_TYPE_ENUM.Thumb_Screen => R2DatWebFlag.Media.THUMB_SCREEN,
                MEDIA_TYPE_ENUM.Soundtrack => "soundtrack",

                _ => null
            };

            return !string.IsNullOrWhiteSpace(flag);
        }

        private void AppendInfoRom(
            XmlDocument doc,
            XmlElement machineEl,
            string targetRomName,
            InfoSpec infoSpec)
        {
            XmlElement romEl = doc.CreateElement("rom");
            _ = machineEl.AppendChild(romEl);

            romEl.SetAttribute("name", targetRomName);
            romEl.SetAttribute("size", infoSpec.Size.ToString());

            if (!string.IsNullOrWhiteSpace(infoSpec.Crc))
                romEl.SetAttribute("crc", infoSpec.Crc);

            if (!string.IsNullOrWhiteSpace(infoSpec.Md5))
                romEl.SetAttribute("md5", infoSpec.Md5);

            if (!string.IsNullOrWhiteSpace(infoSpec.Sha1))
                romEl.SetAttribute("sha1", infoSpec.Sha1);
        }

        private void AppendSourceRom(
            XmlDocument doc,
            XmlElement machineEl,
            string targetRomName,
            DatRomVO sourceRom)
        {
            XmlElement romEl = doc.CreateElement("rom");
            _ = machineEl.AppendChild(romEl);

            romEl.SetAttribute("name", targetRomName);
            romEl.SetAttribute("size", sourceRom.Size.ToString());

            if (!string.IsNullOrWhiteSpace(sourceRom.Crc))
                romEl.SetAttribute("crc", sourceRom.Crc);

            if (!string.IsNullOrWhiteSpace(sourceRom.Md5))
                romEl.SetAttribute("md5", sourceRom.Md5);

            if (!string.IsNullOrWhiteSpace(sourceRom.Sha1))
                romEl.SetAttribute("sha1", sourceRom.Sha1);
        }

        private string CombineDatPath(
            string left,
            string right)
        {
            return NormalizeDatPath(left).TrimEnd('/') + "/" + NormalizeDatPath(right).TrimStart('/');
        }

        private static string NormalizeDatPath(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim().Replace("\\", "/");
        }

        private static string GetDatFileName(string value)
        {
            value = NormalizeDatPath(value);

            int slashIndex = value.LastIndexOf('/');

            if (slashIndex < 0)
                return value;

            if (slashIndex == value.Length - 1)
                return string.Empty;

            return value.Substring(slashIndex + 1);
        }

        private sealed class MediaRomExportCandidate
        {
            public IReadOnlyList<MediaRomExportFile> Files { get; }

            public MediaRomExportCandidate(IReadOnlyList<MediaRomExportFile> files)
            {
                Files = files;
            }
        }

        private sealed class MediaRomExportFile
        {
            public DatRomVO Rom { get; }
            public string TargetRomName { get; }

            public MediaRomExportFile(
                DatRomVO rom,
                string targetRomName)
            {
                Rom = rom;
                TargetRomName = targetRomName;
            }
        }
    }
}