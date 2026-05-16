using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class FlagFilterSet
    {
        public IReadOnlyCollection<DAT_GROUP_ENUM> DatGroupEnums { get; }
        public IReadOnlySet<string> PartFlags { get; }
        public IReadOnlySet<string> RegionFlags { get; }
        public IReadOnlySet<string> LanguageFlags { get; }
        public IReadOnlySet<string> FamilyFlags { get; }
        public IReadOnlySet<string> AllFlags { get; }

        public FlagFilterSet(
            DAT_GROUP_ENUM datGroupEnum,
            IReadOnlySet<string> partFlags,
            IReadOnlySet<string> regionFlags,
            IReadOnlySet<string> languageFlags,
            IReadOnlySet<string> familyFlags) : this(
                new[] { datGroupEnum }, 
                partFlags, 
                regionFlags,
                languageFlags,
                familyFlags) 
        { }

        public FlagFilterSet(
            IReadOnlyCollection<DAT_GROUP_ENUM> datGroupEnums, 
            IReadOnlySet<string> partFlags, 
            IReadOnlySet<string> regionFlags,
            IReadOnlySet<string> languageFlags,
            IReadOnlySet<string> familyFlags)
        {
            DatGroupEnums = datGroupEnums;
            PartFlags = partFlags;
            RegionFlags = regionFlags;
            LanguageFlags = languageFlags;
            FamilyFlags = familyFlags;

            // NOTE: below might become useful if we end up searching for for values within flags down the line.

            var all = new List<string>(partFlags.Count + regionFlags.Count);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var f in partFlags) if (seen.Add(f)) all.Add(f);
            foreach (var f in regionFlags) if (seen.Add(f)) all.Add(f);
            foreach (var f in languageFlags) if (seen.Add(f)) all.Add(f);
            foreach (var f in familyFlags) if (seen.Add(f)) all.Add(f);

            all.Sort((a, b) => { var c = b.Length.CompareTo(a.Length); return c != 0 ? c : StringComparer.Ordinal.Compare(a, b); });

            AllFlags = all.ToHashSet<string>();
        }
    }
}
