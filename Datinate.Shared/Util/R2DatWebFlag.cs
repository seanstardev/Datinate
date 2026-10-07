using System.Reflection;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public static class R2DatWebFlag
    {
        public static class Media
        {
            public const string THUMB = "thumb";
            public const string THUMB_SCREEN = "thumb-screen";
            public const string LOGO = "logo";
            public const string THUMB_RELEASE = "thumb-release";

            public const string SNAP = "screen";
            public const string TITLE = "screen-title";
            public const string INFO = "info.xml";

            public const string UNSPECIFIED = "unspecified";
            public const string UNSPECIFIED_ = "unspecified - ";

            public const string BOX = "box-front";
            public const string BOX_ = "box-front - ";
            public const string BOX_BACK = "box-back";
            public const string BOX_BACK_ = "box-back - ";
            public const string BOX_INLAY = "box-inside";
            public const string BOX_INLAY_ = "box-inside - ";
            public const string BOX_TOP = "box-top";
            public const string BOX_BOTTOM = "box-bottom";
            public const string BOX_SIDE = "box-side";
            public const string BOX_SIDE_ = "box-side - ";

            public const string MANUAL_FRONT = "manual-front";
            public const string MANUAL_FRONT_ = "manual-front - ";
            public const string MANUAL_BACK = "manual-back";
            public const string MANUAL = "manual";
            public const string MANUAL_ = "manual - ";

            public const string MEDIA = "media";
            public const string MEDIA_ = "media - ";    // ??
            public const string MEDIA_BACK = "media-back";

            public const string OTHER = "other";
            public const string OTHER_ = "other - ";
            public const string OTHER_MAP = "other-map";
            public const string OTHER_MAP_ = "other-map - ";
            public const string OTHER_ADVERT = "other-advertisement";
            public const string OTHER_ADVERT_ = "other-advertisement - ";
            public const string OTHER_REFERENCE_CARD = "other-reference_card";
            public const string OTHER_REFERENCE_CARD_ = "other-reference_card - ";

            public const string OTHER_HARDWARE = "other-hardware";
            public const string OTHER_HARDWARE_ = "other-hardware - ";
            public const string VIDEO = "video";
            public const string VIDEO_ = "video - ";

            private static string[]? names = null;
            public static string[] ToArray()
            {
                if (names != null)
                    return names;

                List<string> list = new List<string>();

                FieldInfo[] fieldInfos = typeof(Media).GetFields(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                foreach (FieldInfo fi in fieldInfos)
                {
                    if (!fi.IsLiteral || fi.IsInitOnly)
                        continue;

                    if (fi.FieldType != typeof(string))
                        continue;

                    if (fi.GetRawConstantValue() is string value)
                        list.Add(value);
                }

                list.Sort((s1, s2) => s2.Length.CompareTo(s1.Length));

                names = list.ToArray();
                return names;
            }
            public static MEDIA_TYPE_ENUM GetTypeEnum(string filename)
            {
                filename = Path.GetFileName(filename);

                if (string.IsNullOrWhiteSpace(filename))
                    return MEDIA_TYPE_ENUM.Unspecified;

                return GetTypeEnumInternal(filename);
            }

            public static string? GetMatch(string flag)
            {
                if (flag == null) return null;

                string[] arr = ToArray();

                for (int i = 0; i < arr.Length; i++)
                {
                    if (flag.StartsWith(arr[i]))
                        return arr[i];
                }

                return null;
            }
            public static string RemoveLookupFlagFromResourceItemName(string name)
            {
                var s = name.Trim();
                if (s.Length == 0)
                    return s;

                int close = s.Length - 1;
                if (s[close] != ']')
                    return s;

                int open = s.LastIndexOf('[', close);
                if (open < 0)
                    return s;

                s = s.Substring(0, open).TrimEnd();
                return s;
            }

            private static MEDIA_TYPE_ENUM GetTypeEnumInternal(string filenamePart)
            {
                var maps = GetConstDrivenMapsByFlagLenDesc();

                if (filenamePart.Length > 0 && filenamePart[0] == '[')
                {
                    for (int i = 0; i < maps.Length; i++)
                    {
                        if (filenamePart.StartsWith(maps[i].BracketPrefix, StringComparison.OrdinalIgnoreCase))
                            return maps[i].Enum;
                    }
                }
                else
                {
                    for (int i = 0; i < maps.Length; i++)
                    {
                        if (filenamePart.StartsWith(maps[i].Flag, StringComparison.OrdinalIgnoreCase))
                            return maps[i].Enum;
                    }
                }

                var enums = (MEDIA_TYPE_ENUM[])Enum.GetValues(typeof(MEDIA_TYPE_ENUM));
                Array.Sort(enums, (a, b) => b.ToString().Length.CompareTo(a.ToString().Length));

                for (int i = 0; i < enums.Length; i++)
                {
                    var en = enums[i];
                    var enumStr = en.ToString().ToLowerInvariant();

                    if (enumStr.EndsWith("_", StringComparison.Ordinal))
                        enumStr = enumStr.Remove(enumStr.Length - 1, 1) + " - ";

                    enumStr = enumStr.Replace("_", "-");

                    if (en == MEDIA_TYPE_ENUM.Info && filenamePart.Equals(INFO, StringComparison.OrdinalIgnoreCase))
                        return en;

                    if (filenamePart.StartsWith("[" + enumStr, StringComparison.OrdinalIgnoreCase))
                        return en;
                }

                System.Diagnostics.Debug.WriteLine(typeof(R2DatWebFlag) + ": WARNING: Unrecognised Type: '" + filenamePart + "'.");
                return MEDIA_TYPE_ENUM.Unspecified;
            }
        }
        private struct FlagEnumMap
        {
            public readonly string Flag;
            public readonly string BracketPrefix;
            public readonly MEDIA_TYPE_ENUM Enum;

            public FlagEnumMap(string flag, MEDIA_TYPE_ENUM en)
            {
                Flag = flag;
                BracketPrefix = "[" + flag;
                Enum = en;
            }
        }

        private static FlagEnumMap[]? constDrivenMapsByFlagLenDesc = null;

        private static FlagEnumMap[] GetConstDrivenMapsByFlagLenDesc()
        {
            if (constDrivenMapsByFlagLenDesc != null)
                return constDrivenMapsByFlagLenDesc;

            var list = new List<FlagEnumMap>();

            var fieldInfos = typeof(Media).GetFields(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (var fi in fieldInfos)
            {
                if (!fi.IsLiteral || fi.IsInitOnly)
                    continue;

                if (fi.FieldType != typeof(string))
                    continue;

                object? v = fi.GetRawConstantValue();
                string? flag = v as string;

                if (string.IsNullOrWhiteSpace(flag))
                    continue;

                var name = fi.Name;

                while (name.Length > 0 && name.EndsWith("_", StringComparison.Ordinal))
                    name = name.Substring(0, name.Length - 1);

                if (!Enum.TryParse(name, ignoreCase: true, out MEDIA_TYPE_ENUM en))
                    continue;

                list.Add(new FlagEnumMap(flag, en));
            }

            list.Sort((a, b) => b.Flag.Length.CompareTo(a.Flag.Length));

            constDrivenMapsByFlagLenDesc = list.ToArray();
            return constDrivenMapsByFlagLenDesc;
        }
    }
}