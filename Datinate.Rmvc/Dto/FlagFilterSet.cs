using Datinate.Shared;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Dto
{
    internal class FlagFilterSet
    {
        public IReadOnlyCollection<DAT_GROUP_ENUM> DatGroupEnums { get; }
        public IReadOnlySet20<string> PartFlags { get; }
        public IReadOnlySet20<string> RegionFlags { get; }
        public IReadOnlySet20<string> LanguageFlags { get; }
        public IReadOnlySet20<string> FamilyFlags { get; }
        public IReadOnlySet20<string> AllFlags { get; }

        public FlagFilterSet(
            DAT_GROUP_ENUM datGroupEnum,
            IReadOnlySet20<string> partFlags,
            IReadOnlySet20<string> regionFlags,
            IReadOnlySet20<string> languageFlags,
            IReadOnlySet20<string> familyFlags) : this(
                new[] { datGroupEnum }, 
                partFlags, 
                regionFlags,
                languageFlags,
                familyFlags) 
        { }

        public FlagFilterSet(
            IReadOnlyCollection<DAT_GROUP_ENUM> datGroupEnums, 
            IReadOnlySet20<string> partFlags, 
            IReadOnlySet20<string> regionFlags,
            IReadOnlySet20<string> languageFlags,
            IReadOnlySet20<string> familyFlags)
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

            AllFlags = ReadOnlySet20.From(seen);
        }
    }
}
