using RadioLibCore.RadioDat;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class GameVO : IGame 
    {
        private string name;

        public GamePartVO[] Parts { get; private set; }
        public GamePartVO PrimaryPart => Parts[0];
        public DAT_GROUP_ENUM DatGroupEnum => PrimaryPart.DatGroupEnum;

        public string? DatReference => PrimaryPart.DatReference;
        public string? MameLaunchName => PrimaryPart.MameName;

        public GameVO(GamePartVO gamePartVO, string name) 
        {
            Parts = Array.Empty<GamePartVO>();
            _ = AddGamePart(gamePartVO);
            this.name = name;
        }
        public void UpdateGameName(string newName)
        {
            name = newName;
        }
        public bool AddGamePart(GamePartVO gamePart)
        {
            if (Parts.Any(p => p.RomsFingerprint == gamePart.RomsFingerprint))
                return false;
            
            Parts = Parts.Append(gamePart).ToArray();
            return true;
        }

        public bool AddGamePartAlias(GamePartVO alias)
        {
            foreach (var part in Parts)
            {
                if (part.RomsFingerprint == alias.RomsFingerprint)
                {
                    return part.AddAlias(alias);
                }
            }
            return false;
        }

        public IReadOnlyCollection<string> GetMemberPartRomChecksums()
        {
            return Parts
                .SelectMany(part => part.GetChecksums())
                .ToList();
        }
        public int GetMemberPartAliasesTotal()
        {
            return Parts.Sum(part => part.GetSoftwareAliases().Length);
        }

        public bool AbsorbGameAsParts(GameVO game)
        {
            return AddGameParts(game.Parts);
        }

        // INTERFACE
        public IGamePart[] GetGameParts(bool includeAliasPartsInFlatList)
        {
            IEnumerable<IGamePart> parts = Parts;

            if (includeAliasPartsInFlatList)
                parts = parts.Concat(Parts.SelectMany(p => p.GetSoftwareAliases()));

            return parts.ToArray();
        }
        // INTERFACE
        public string GetNameWithoutExt()
        {
            return name;
        }
        // INTERFACE
        public string GetLaunchName()
        {
            return MameLaunchName ?? string.Empty;
        }
        // INTERFACE
        public bool HasLaunchName()
        {
            return !string.IsNullOrWhiteSpace(MameLaunchName);
        }

        private bool AddGameParts(IEnumerable<GamePartVO> gameParts)
        {
            bool allAdded = true;
            foreach (var part in gameParts)
                if (!AddGamePart(part))
                    allAdded = false;

            return allAdded;
        }

        public bool TryAddShallowPart(GamePartVO part)
        {
            var shallow = new GamePartVO(
                part.Name,
                part.DatSourceID,
                part.DatGameSource,
                part.ExpressionActionEnum,
                part.RomsFingerprint,
                part.DatGroupEnum,
                part.DatReference,
                part.PartOwnerName,
                part.MameName,
                true,
                part.MameSlPartName,
                part.Tag);

            _ = shallow.AddAliases(part.GetAliases());

            return AddGamePart(shallow);
        }
    }
}
