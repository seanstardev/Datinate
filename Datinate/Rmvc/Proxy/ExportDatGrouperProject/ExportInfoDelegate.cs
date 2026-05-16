using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Rb;
using RadioLibCore.RadioDat;
using RadioLibCore.RadioResource;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class ExportInfoDelegate
    {
        public IReadOnlyDictionary<IGameFamily, InfoSpec> Export(
            IReadOnlyList<IGameFamily> curatedFamilies,
            Dictionary<IGameFamily, string> familyUniqueNameDictionary,
            DatGrouperProjectDTO project,
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaPriorities,
            IReadOnlyDictionary<IGameFamily, IMediaCollectionImportExport?> familyMediaDictionary,
            IReadOnlyDictionary<IGameFamily, IReadOnlyDictionary<string, ResourceDetailsDTO>> familyResourceDetailsDictionary,
            string mediaProjectPath)
        {
            var infoSpecDictionary = new Dictionary<IGameFamily, InfoSpec>();

            foreach (var family in curatedFamilies)
            {
                if (!familyMediaDictionary.TryGetValue(family, out var mediaCollection) ||
                    mediaCollection == null ||
                    mediaCollection.IsEmptyForExport)
                {
                    continue;
                }

                if (!familyResourceDetailsDictionary.TryGetValue(family, out var resourceDetailsDictionary) ||
                    resourceDetailsDictionary.Count == 0)
                {
                    continue;
                }

                if (!familyUniqueNameDictionary.TryGetValue(family, out var familyName) ||
                    string.IsNullOrWhiteSpace(familyName))
                {
                    familyName = family.GetFamilyDisplayName();
                }

                var info = CreateInfoShell(familyName, project.ProjectName);

                var infoAboutCandidates = CreateInfoCandidates(
                    resourceDetailsDictionary,
                    mediaPriorities,
                    MEDIA_TYPE_ENUM.Info_About);

                bool hasAbout = SetInfoAbout(info, infoAboutCandidates);

                var infoCreditsCandidates = CreateInfoCandidates(
                    resourceDetailsDictionary,
                    mediaPriorities,
                    MEDIA_TYPE_ENUM.Info_Credits);

                bool hasCredits = SetInfoCredits(info, infoCreditsCandidates);

                var infoReleasesCandidates = CreateInfoCandidates(
                    resourceDetailsDictionary,
                    mediaPriorities,
                    MEDIA_TYPE_ENUM.Info_Releases);

                bool hasReleases = SetInfoReleases(
                    info,
                    infoReleasesCandidates,
                    out string? releasesSourceId,
                    out IReadOnlySet<string> assetKeys);

                if (!hasAbout && !hasCredits && !hasReleases)
                    continue;

                string familyMediaPath = Path.Combine(mediaProjectPath, familyName);
                string infoXmlPath = Path.Combine(familyMediaPath, "Info.xml");

                Directory.CreateDirectory(familyMediaPath);

                InfoHelper.SaveInfoVO(
                    info,
                    infoXmlPath,
                    info.ResourceEnum);

                if (!File.Exists(infoXmlPath))
                    throw new InvalidOperationException("Info.xml export failed for family: " + familyName);

                infoSpecDictionary[family] = InfoSpec.FromFile(
                    infoXmlPath,
                    releasesSourceId,
                    assetKeys);
            }

            return infoSpecDictionary;
        }

        private bool SetInfoReleases(
            InfoVO mergedInfo,
            IReadOnlyList<InfoCandidate> candidates,
            out string? releasesSourceId,
            out IReadOnlySet<string> assetKeys)
        {
            foreach (var candidate in candidates)
            {
                if (candidate.Info.ReleaseVOs != null && candidate.Info.ReleaseVOs.Any())
                {
                    mergedInfo.ReleaseVOs = candidate.Info.ReleaseVOs;
                    releasesSourceId = candidate.SourceId;

                    var hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var release in mergedInfo.ReleaseVOs)
                    {
                        var assetKey = CreateCleansedAssetKey(release.AssetKey);
                        if (string.IsNullOrWhiteSpace(assetKey) == false)
                        {
                            release.AssetKey = assetKey;
                            _ = hashSet.Add(assetKey);
                        }
                    }

                    assetKeys = hashSet;
                    return true;
                }
            }

            releasesSourceId = null;
            assetKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return false;
        }

        private string? CreateCleansedAssetKey(string? assetKey)
        {
            if (string.IsNullOrWhiteSpace(assetKey))
                return assetKey;

            string cleansed = assetKey.Trim();

            string thumbReleasePrefix = "[" + R2DatWebFlag.Media.THUMB_RELEASE + "]";

            if (cleansed.StartsWith(thumbReleasePrefix, StringComparison.OrdinalIgnoreCase))
                cleansed = cleansed.Substring(thumbReleasePrefix.Length).TrimStart();

            string extension = Path.GetExtension(cleansed);

            if (IsKnownImageExtension(extension))
                cleansed = cleansed.Substring(0, cleansed.Length - extension.Length).TrimEnd();

            return string.IsNullOrWhiteSpace(cleansed) ? null : cleansed;
        }

        private bool IsKnownImageExtension(string? extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            extension = extension.Trim().ToLowerInvariant();

            return extension == ".png" ||
                   extension == ".jpg" ||
                   extension == ".jpeg" ||
                   extension == ".bmp" ||
                   extension == ".gif" ||
                   extension == ".webp" ||
                   extension == ".tif" ||
                   extension == ".tiff";
        }

        private bool SetInfoCredits(
            InfoVO mergedInfo,
            IReadOnlyList<InfoCandidate> candidates)
        {
            foreach (var candidate in candidates)
            {
                if (candidate.Info.CreditVOs != null && candidate.Info.CreditVOs.Any())
                {
                    mergedInfo.CreditVOs = candidate.Info.CreditVOs;
                    return true;
                }
            }

            return false;
        }

        private bool SetInfoAbout(
            InfoVO mergedInfo,
            IReadOnlyList<InfoCandidate> candidates)
        {
            bool hasAnything = false;

            bool hasDescription = false;
            bool hasMiscellaneous = false;
            bool hasCompilations = false;
            bool hasAlsoKnownAs = false;
            bool hasAlsoOn = false;
            bool hasEmulation = false;

            foreach (var candidate in candidates)
            {
                InfoVO info = candidate.Info;

                if (string.IsNullOrWhiteSpace(mergedInfo.Developer) &&
                    !string.IsNullOrWhiteSpace(info.Developer))
                {
                    mergedInfo.Developer = info.Developer;
                    hasAnything = true;
                }

                if (!hasDescription && GetInfoAboutDescriptionIsValid(info))
                {
                    mergedInfo.DescriptionVO = info.DescriptionVO;
                    hasDescription = true;
                    hasAnything = true;
                }

                if (!hasMiscellaneous && info.MiscPropertyVOs != null && info.MiscPropertyVOs.Any())
                {
                    mergedInfo.MiscPropertyVOs = info.MiscPropertyVOs;
                    hasMiscellaneous = true;
                    hasAnything = true;
                }

                if (!hasCompilations && info.CompilationVOs != null && info.CompilationVOs.Any())
                {
                    mergedInfo.CompilationVOs = info.CompilationVOs;
                    hasCompilations = true;
                    hasAnything = true;
                }

                if (!hasAlsoKnownAs && info.AlsoKnownAs != null && info.AlsoKnownAs.Any())
                {
                    mergedInfo.AlsoKnownAs = info.AlsoKnownAs;
                    hasAlsoKnownAs = true;
                    hasAnything = true;
                }

                if (!hasAlsoOn && info.AlsoOn != null && info.AlsoOn.Any())
                {
                    mergedInfo.AlsoOn = info.AlsoOn;
                    hasAlsoOn = true;
                    hasAnything = true;
                }

                if (string.IsNullOrWhiteSpace(mergedInfo.StartupText) &&
                    !string.IsNullOrWhiteSpace(info.StartupText))
                {
                    mergedInfo.StartupText = info.StartupText;
                    hasAnything = true;
                }

                if (!hasEmulation &&
                    info.EmulationVO != null &&
                    !string.IsNullOrWhiteSpace(info.EmulationVO.History))
                {
                    mergedInfo.EmulationVO = info.EmulationVO;
                    hasEmulation = true;
                    hasAnything = true;
                }

                if (string.IsNullOrWhiteSpace(mergedInfo.Genre) &&
                    !string.IsNullOrWhiteSpace(info.Genre))
                {
                    mergedInfo.Genre = info.Genre;
                    hasAnything = true;
                }

                if (string.IsNullOrWhiteSpace(mergedInfo.Players) &&
                    !string.IsNullOrWhiteSpace(info.Players))
                {
                    mergedInfo.Players = info.Players;
                    hasAnything = true;
                }
            }

            return hasAnything;
        }

        private InfoVO CreateInfoShell(string familyName, string projectName)
        {
            return new InfoVO(familyName)
            {
                Name = familyName,
                SystemLookup = projectName,
                ResourceEnum = "R2Dat_Merged",
                InfoXmlVersion = "1.2"
            };
        }

        private bool GetInfoAboutDescriptionIsValid(InfoVO info)
        {
            if (info.DescriptionVO == null ||
                string.IsNullOrWhiteSpace(info.DescriptionVO.Description) ||
                info.DescriptionVO.IsBoilerplate)
            {
                return false;
            }

            return true;
        }

        private IReadOnlyList<InfoCandidate> CreateInfoCandidates(
            IReadOnlyDictionary<string, ResourceDetailsDTO> resourceDetailsDictionary,
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaOptions,
            MEDIA_TYPE_ENUM mediaTypeEnum)
        {
            var list = new List<InfoCandidate>();

            foreach (var sourceId in CreateIncludedSourceIdList(mediaOptions, mediaTypeEnum))
            {
                if (!resourceDetailsDictionary.TryGetValue(sourceId, out var resourceDetails))
                    continue;

                if (resourceDetails.Info == null)
                    continue;

                list.Add(new InfoCandidate(sourceId, resourceDetails.Info));
            }

            return list;
        }

        private IReadOnlyList<string> CreateIncludedSourceIdList(
            IReadOnlyList<DatGrouperMediaExportEntryDTO> mediaOptions,
            MEDIA_TYPE_ENUM mediaTypeEnum)
        {
            var list = new List<string>();
            var added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var option in mediaOptions)
            {
                if (!IsMatchingInfoExportOption(option.MediaTypeEnum, mediaTypeEnum))
                    continue;

                foreach (var source in option.Sources)
                {
                    if (!source.Include)
                        continue;

                    if (string.IsNullOrWhiteSpace(source.SourceId))
                        continue;

                    if (!added.Add(source.SourceId))
                        continue;

                    list.Add(source.SourceId);
                }
            }

            return list;
        }

        private bool IsMatchingInfoExportOption(
            MEDIA_TYPE_ENUM exportMediaTypeEnum,
            MEDIA_TYPE_ENUM requestedMediaTypeEnum)
        {
            if (exportMediaTypeEnum == requestedMediaTypeEnum)
                return true;

            if (exportMediaTypeEnum == MEDIA_TYPE_ENUM.Info)
            {
                return requestedMediaTypeEnum == MEDIA_TYPE_ENUM.Info_About ||
                       requestedMediaTypeEnum == MEDIA_TYPE_ENUM.Info_Credits ||
                       requestedMediaTypeEnum == MEDIA_TYPE_ENUM.Info_Releases;
            }

            return false;
        }

        private sealed class InfoCandidate
        {
            public string SourceId { get; }
            public InfoVO Info { get; }

            public InfoCandidate(string sourceId, InfoVO info)
            {
                SourceId = sourceId;
                Info = info;
            }
        }
    }

    public class InfoSpec
    {
        public ulong Size { get; }
        public string Crc { get; }
        public string Md5 { get; }
        public string Sha1 { get; }
        public string? ReleasesSourceId { get; }
        public IReadOnlySet<string> AssetKeys { get; }

        public InfoSpec(
            ulong size,
            string crc,
            string md5,
            string sha1,
            string? releasesSourceId,
            IReadOnlySet<string> assetKeys)
        {
            Size = size;
            Crc = crc;
            Md5 = md5;
            Sha1 = sha1;
            ReleasesSourceId = releasesSourceId;
            AssetKeys = assetKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public static InfoSpec FromFile(string filePath)
        {
            return FromFile(
                filePath,
                null,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        public static InfoSpec FromFile(
            string filePath,
            string? releasesSourceId,
            IReadOnlySet<string> assetKeys)
        {
            byte[] bytes = File.ReadAllBytes(filePath);

            return new InfoSpec(
                (ulong)bytes.LongLength,
                ExportDatGrouperHelper.ComputeCrc32Hex(bytes),
                Convert.ToHexString(System.Security.Cryptography.MD5.HashData(bytes)).ToLowerInvariant(),
                Convert.ToHexString(System.Security.Cryptography.SHA1.HashData(bytes)).ToLowerInvariant(),
                releasesSourceId,
                assetKeys);
        }
    }
}