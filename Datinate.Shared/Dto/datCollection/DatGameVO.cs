using static datinate.shared.DatFilterHelper;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class DatGameVO : IByteReporter 
    {
        public DatRomVO[] Roms { get; }
        public string Name { get; }

        public EXPRESSION_ACTION_ENUM ExpressionActionEnum { get; set; } = EXPRESSION_ACTION_ENUM.INVALID;

        // TODO: There are probably old Dat sets that do not have SHA1.
        // i.e. they may just have CRC and / or MD5.
        public string Fingerprint { get; }

        public string? NamePublisherKey { get; private set; }
        public string? NameRegionKey { get; private set; }

        public string? Publisher { get; private set; }
        public string? Region { get; private set; }
        public string? Date { get; private set; }
        public string? Category { get; }
        public string? Description { get; }

        public string? ParentName { get; set; }
        public string? PartOwnerName { get; set; }
        public string? MameLaunchName { get; }

        public string NormalisedName { get; private set; }
        public string FlaglessName { get; private set; }


        public bool HasParent => !string.IsNullOrWhiteSpace(ParentName);

        private readonly ulong totalRomsSize;

        public DatGameVO(
            string name
            , string? description
            , string? publisher
            , string? region
            , string? date
            , string? category
            , string? mameLaunchName    // For MAME: the short name. For No-Intro PClone: the 'cloneid'  / 'cloneof' att val.
            , string? parentName        // For MAME: the short name. For No-Intro PClone: the 'cloneid'  / 'cloneof' att val.
            , DatRomVO[] roms
            , string? partOwnerName     // For MAME only: the part 'name' att value.
        ) {
            Name = name;
            Description = description;
            
            Publisher = publisher;
            Region = region;
            Date = date;
            Category = category;
            MameLaunchName = mameLaunchName;
            ParentName = parentName;
            PartOwnerName = partOwnerName;
            Roms = roms;

            totalRomsSize = Roms.Aggregate(0UL, (tally, rom) => tally + rom.Size);

            if (!string.IsNullOrWhiteSpace(description))
                this.Name = description;

            FlaglessName = DatinateHelper.GetFlaglessName(Name);

            NormalisedName = Name;

            Fingerprint = FingerprintHelper.SortFingerprintElementsToString(
                Roms.Select(r => r.Sha1).ToArray());

            UpdateNamePublisherKey();
            UpdateNameRegionKey();
        }
        public void SetNormalisedName(string? normalisedName)
        {
            NormalisedName = string.IsNullOrWhiteSpace(normalisedName) ? Name : normalisedName;

           
            FlaglessName = DatinateHelper.GetFlaglessName(NormalisedName);
            
            UpdateNamePublisherKey();
            UpdateNameRegionKey();
        }
        public void SetPublisher(string? publisher) {
            Publisher = publisher;
            UpdateNamePublisherKey();
        }
        public void SetRegion(string? region)
        {
            Region = region;
            UpdateNameRegionKey();
        }
        public void SetDate(string? date)
        {
            Date = date;
        }
        private void UpdateNamePublisherKey()
        {
            if (string.IsNullOrWhiteSpace(Publisher)) NamePublisherKey = null;
            else NamePublisherKey = DatinateHelper.CreateGameFamilyDisplayName(FlaglessName, Publisher);
        }
        private void UpdateNameRegionKey()
        {
            if (string.IsNullOrWhiteSpace(Region)) NameRegionKey = null;
            else NameRegionKey = DatinateHelper.CreateGameFamilyDisplayName(FlaglessName, Region);
        }
        // TODO:
        public ulong GetTotalSize()
        {
            return totalRomsSize;
        }

    }
}
