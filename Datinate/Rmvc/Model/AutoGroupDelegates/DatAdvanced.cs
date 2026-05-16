using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class DatAdvanced
    {
        public string Key { get; }
        public DatVO Dat { get; private set; }
        public string DatFriendlyReference { get; }        
        public DAT_GROUP_ENUM DatGroupEnum { get; }
        public DatFilter[] Filters { get; }
        public DatSubsetFilter? DatSubset { get; }

        public bool HasFilters => Filters.Length > 0;
        public IReadOnlyCollection<DatGameVO> Entries => Dat.Entries;

        public DatAdvanced(
            DatVO datVO,
            string key,
            DAT_GROUP_ENUM datGroupEnum,
            string datFriendlyReference,
            DatFilter[] filters,
            DatSubsetFilter? datSubset)
        {
            Key = key;
            Dat = datVO;
            DatFriendlyReference = datFriendlyReference;
            Filters = filters;
            DatSubset = datSubset;
            DatGroupEnum = datGroupEnum;
        }

        public void RemoveEntriesByGameName(string[] strings)
        {
            if (strings.Length == 0 || Dat.Entries.Count == 0)
                return;

            var excludes = new HashSet<string>(strings, StringComparer.Ordinal);

            var list = new List<DatGameVO>();

            foreach (var e in Dat.Entries)
            {
                if (!excludes.Contains(e.Name))
                    list.Add(e);
            }

            if (list.Count == Dat.Entries.Count)
                return;

            Dat = DatVO.CreateDatFrom(Dat, list);
        }

        public static DatAdvanced CreateDatAdvancedFrom(DatAdvanced adv, IReadOnlyCollection<DatGameVO> replacementEntries)
        {
            var newDat = DatVO.CreateDatFrom(adv.Dat, replacementEntries);

            var newAdv = new DatAdvanced(
                newDat,
                adv.Key,
                adv.DatGroupEnum, 
                adv.DatFriendlyReference, 
                adv.Filters, 
                adv.DatSubset);

            return newAdv;
        }
    }
}
