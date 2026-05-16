using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Util;
using RMVC;
using System.Text;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class SetDatGrouperExportViewCmd : RCommand
    {
        protected override void Run()
        {
            Facade.Instance?.Shell?.ShowExportView();

            if (Facade.Instance?.DatGrouperModel is { } datGrouperModel &&
                Facade.Instance?.RadioDatModel is { } radioDatModel &&
                Facade.Instance?.RadioDatModel?.ActiveProject is { } activeProject)
            {
                var families = datGrouperModel.CuratedFamilies;

                bool everyGameHasExactlyOnePart =
                    DatinateFamilyHelper.GetAllGamesContainExactlyOnePart(families);

                var mediaTypes = Enum.GetValues<MEDIA_TYPE_ENUM>()
                    .Where(mediaType =>
                        mediaType != MEDIA_TYPE_ENUM.NOT_SET &&
                        mediaType != MEDIA_TYPE_ENUM.Thumb_Release &&
                        mediaType != MEDIA_TYPE_ENUM.Unspecified)
                    .ToArray();

                var mediaPriorityDictionary = radioDatModel.CreateMediaExportPriorityDictionary(
                    families,
                    mediaTypes);

                var reconciled = ReconcileMediaExportPriorities(
                    mediaPriorityDictionary,
                    activeProject.MediaExports,
                    out var mediaExportSettingsMessage);

                Facade.Instance?.ExportMediator?.SetView(
                    everyGameHasExactlyOnePart,
                    activeProject.ExportSoftwareOptionsDTO,
                    reconciled,
                    mediaExportSettingsMessage ?? string.Empty);
            }
        }

        private static IReadOnlyDictionary<MEDIA_TYPE_ENUM, IReadOnlyList<MediaExportPriorityItemDTO>> ReconcileMediaExportPriorities(
            IReadOnlyDictionary<MEDIA_TYPE_ENUM, IReadOnlyList<MediaExportPriorityItemDTO>> currentDictionary,
            IReadOnlyList<DatGrouperMediaExportEntryDTO> savedEntries,
            out string? message)
        {
            message = null;

            if (savedEntries.Count == 0)
                return currentDictionary;

            var savedByMediaType = savedEntries
                .GroupBy(x => x.MediaTypeEnum)
                .ToDictionary(
                    group => group.Key,
                    group => group.First());

            var result = new Dictionary<MEDIA_TYPE_ENUM, IReadOnlyList<MediaExportPriorityItemDTO>>();
            var addedMessages = new List<string>();
            var removedMessages = new List<string>();

            foreach (var kvp in currentDictionary)
            {
                var mediaTypeEnum = kvp.Key;
                var currentItems = kvp.Value;

                if (currentItems.Count == 0)
                {
                    result[mediaTypeEnum] = currentItems;
                    continue;
                }

                var currentBySourceId = currentItems
                    .GroupBy(x => x.SourceId, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group => group.First(),
                        StringComparer.OrdinalIgnoreCase);

                if (!savedByMediaType.TryGetValue(mediaTypeEnum, out var savedEntry))
                {
                    var unsavedMediaTypeItems = currentItems
                        .Select(x => x.WithInclude(false))
                        .ToArray();

                    result[mediaTypeEnum] = unsavedMediaTypeItems;

                    foreach (var item in unsavedMediaTypeItems)
                        addedMessages.Add(CreateSourceMessage(mediaTypeEnum, item));

                    continue;
                }

                var savedIncludedItems = new List<MediaExportPriorityItemDTO>();
                var savedExcludedItems = new List<MediaExportPriorityItemDTO>();
                var savedSourceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var savedSource in savedEntry.Sources)
                {
                    if (string.IsNullOrWhiteSpace(savedSource.SourceId))
                        continue;

                    if (!savedSourceIds.Add(savedSource.SourceId))
                        continue;

                    if (!currentBySourceId.TryGetValue(savedSource.SourceId, out var currentItem))
                    {
                        removedMessages.Add(mediaTypeEnum + ": " + savedSource.SourceId);
                        continue;
                    }

                    var item = currentItem.WithInclude(savedSource.Include);

                    if (savedSource.Include)
                        savedIncludedItems.Add(item);
                    else
                        savedExcludedItems.Add(item);
                }

                var newlyDiscoveredItems = currentItems
                    .Where(x => !savedSourceIds.Contains(x.SourceId))
                    .Select(x => x.WithInclude(false))
                    .ToArray();

                foreach (var item in newlyDiscoveredItems)
                    addedMessages.Add(CreateSourceMessage(mediaTypeEnum, item));

                var finalItems = savedIncludedItems
                    .Concat(newlyDiscoveredItems)
                    .Concat(savedExcludedItems)
                    .ToArray();

                result[mediaTypeEnum] = finalItems;
            }

            message = BuildMediaExportSettingsMessage(addedMessages, removedMessages);

            return result;
        }

        private static string? BuildMediaExportSettingsMessage(
            IReadOnlyList<string> addedMessages,
            IReadOnlyList<string> removedMessages)
        {
            if (addedMessages.Count == 0 && removedMessages.Count == 0)
                return null;

            var sb = new StringBuilder();

            sb.AppendLine("Media export sources have changed since these export settings were last saved.");
            sb.AppendLine();

            if (addedMessages.Count > 0)
            {
                sb.AppendLine("Added sources were placed at the top of their excluded lists:");

                foreach (var message in addedMessages)
                    sb.AppendLine("- " + message);

                sb.AppendLine();
            }

            if (removedMessages.Count > 0)
            {
                sb.AppendLine("Saved sources that no longer exist were removed:");

                foreach (var message in removedMessages)
                    sb.AppendLine("- " + message);
            }

            return sb.ToString().Trim();
        }

        private static string CreateSourceMessage(
            MEDIA_TYPE_ENUM mediaTypeEnum,
            MediaExportPriorityItemDTO item)
        {
            var sourceName = string.IsNullOrWhiteSpace(item.ResourceSourceName)
                ? item.SourceId
                : item.ResourceSourceName;

            return mediaTypeEnum + ": " + sourceName;
        }
    }
}