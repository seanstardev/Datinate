using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public static class NormaliseNamesDelegate
    {
        private static readonly char[] Invalid = Path.GetInvalidFileNameChars();
        public static IReadOnlyDictionary<DAT_GROUP_ENUM, IReadOnlyCollection<string>> BuildPartOwnerFlagsByGroup(
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup)
        {
            var d = new Dictionary<DAT_GROUP_ENUM, IReadOnlyCollection<string>>();

            foreach (var pair in flagFilterSetByGroup)
                d[pair.Key] = BuildDistinctSortedFlags(pair.Value.PartFlags);

            return d;
        }

        // TODO: Further developand centralise:
        public static string NormaliseName(string name)
        {
            return NormaliseName(name, null, true);
        }

        public static void ApplyNormalisedNames(
            IReadOnlyCollection<DatAdvanced> datAdvs,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            bool pathSafeNameNormalisation)
        {
            var flagsByGroup = BuildNormalisedNameFlagsByGroup(flagFilterSetByGroup);

            foreach (var datAdv in datAdvs)
            {
                var datGroupEnum = datAdv.DatGroupEnum;

                var flags = flagsByGroup.TryGetValue(datGroupEnum, out var f)
                    ? f
                    : Array.Empty<string>();

                foreach (var entry in datAdv.Entries)
                    entry.SetNormalisedName(NormaliseName(entry.Name, flags, pathSafeNameNormalisation));
            }
        }

        public static string[] BuildDistinctSortedFlags(params IReadOnlyCollection<string>[] sets)
        {
            var all = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < sets.Length; i++)
            {
                var s = sets[i];
                if (s == null) continue;

                foreach (var f in s)
                {
                    if (string.IsNullOrEmpty(f)) continue;
                    if (seen.Add(f)) all.Add(f);
                }
            }

            all.Sort((a, b) =>
            {
                var c = b.Length.CompareTo(a.Length);
                return c != 0 ? c : StringComparer.Ordinal.Compare(a, b);
            });

            return all.ToArray();
        }

        private static string StripInvalidFileNameChars(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;

            var input = s.AsSpan();
            var n = input.Length;

            Span<char> buffer = n <= 1024 ? stackalloc char[n] : new char[n];

            var j = 0;
            for (var i = 0; i < n; i++)
            {
                var ch = input[i];
                if (Array.IndexOf(Invalid, ch) >= 0)
                    continue;

                buffer[j++] = ch;
            }

            var start = 0;
            while (start < j && char.IsWhiteSpace(buffer[start]))
                start++;

            var end = j;
            while (end > start && (char.IsWhiteSpace(buffer[end - 1]) || buffer[end - 1] == '.'))
                end--;

            if (end <= start)
                return string.Empty;

            return new string(buffer.Slice(start, end - start));
        }

        private static string NormaliseName(string name, IReadOnlyCollection<string>? flags, bool pathSafeNameNormalisation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            name = name.Trim();

            if (flags != null && flags.Count > 0)
                name = FlagStripper.NormaliseName(name, flags);

            if (pathSafeNameNormalisation)
            {
                name = name.Replace(": ", " - ", StringComparison.Ordinal);
                name = name.Replace("/", "-", StringComparison.Ordinal);

                name = name.Replace("\\", "-", StringComparison.Ordinal); // TODO: Find examples
                name = name.Replace("\"", "'", StringComparison.Ordinal); // NOYE: e.g. psx: 10101 - 'Will' the Starship (Sound Technology Japan)

                name = StripInvalidFileNameChars(name);
                name = FlagStripper.NormaliseName(name, null);
            }

            return name;
        }

        private static IReadOnlyDictionary<DAT_GROUP_ENUM, IReadOnlyCollection<string>> BuildNormalisedNameFlagsByGroup(
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup)
        {
            var d = new Dictionary<DAT_GROUP_ENUM, IReadOnlyCollection<string>>();

            foreach (var pair in flagFilterSetByGroup)
            {
                var g = pair.Key;
                var set = pair.Value;

                d[g] = BuildDistinctSortedFlags(set.PartFlags, set.LanguageFlags, set.FamilyFlags);
                    
            }

            return d;
        }
    }
}
