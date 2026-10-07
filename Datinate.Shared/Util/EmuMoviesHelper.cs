using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public static class EmuMoviesHelper
    {
        private static readonly Dictionary<string, MEDIA_TYPE_ENUM> dic = new()
        {
            { "Adverts", MEDIA_TYPE_ENUM.Advert },
            { "Boxes", MEDIA_TYPE_ENUM.Box },
            { "Boxes-Back", MEDIA_TYPE_ENUM.Box_Back },
            { "Boxes-Cover", MEDIA_TYPE_ENUM.Box },        // TODO: This should probably be 'Boxes (Cover)' in source.
            { "Boxes-Inlay", MEDIA_TYPE_ENUM.Box_Inlay },
            { "Boxes-Spine", MEDIA_TYPE_ENUM.Box_Side },
            { "Manuals", MEDIA_TYPE_ENUM.Manual },
            { "Media", MEDIA_TYPE_ENUM.Media },
            { "Media-Label", MEDIA_TYPE_ENUM.Media_Label },
            { "Media-Top", MEDIA_TYPE_ENUM.Media_Top },
            { "Other-Map", MEDIA_TYPE_ENUM.Other_Map },
            { "Other-Overlay", MEDIA_TYPE_ENUM.Other_Overlay },
            { "Snaps", MEDIA_TYPE_ENUM.Snap },
            { "Snaps-Title", MEDIA_TYPE_ENUM.Title },
            { "Soundtracks", MEDIA_TYPE_ENUM.Soundtrack },
            { "Videos", MEDIA_TYPE_ENUM.Video },
        };

        private static readonly KeyValuePair<string, MEDIA_TYPE_ENUM>[] orderedMatches = dic
            .OrderByDescending(x => x.Key.Length)
            .ThenBy(x => x.Key, StringComparer.Ordinal)
            .ToArray();

        public static MEDIA_TYPE_ENUM? GetMediaTypeFromEntryName(string name)
        {
            foreach (var kvp in orderedMatches)
            {
                if (name.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }

            return null;
        }
    }
}