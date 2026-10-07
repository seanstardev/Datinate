using com.RADIO.Datinate.RMVC.Shared;

namespace RadioLibCore.RadioDat 
{
    public class BaseGameFamily : IGameFamily 
    {
        protected string displayName;
        protected IGame[] allGames;
        protected string comment;
        IResourceCollection resourceCollection;
        IDescriptor[] descriptors;
        protected bool isIgnored;

        public BaseGameFamily(
            string displayName
            , IGame[] allGames
            , IDescriptor[] descriptors
            , IResourceCollection resourceCollection
            , bool isIgnored
            , string comment
        ) {
            this.comment            = comment;
            this.descriptors        = descriptors;
            this.displayName        = displayName;
            this.allGames           = allGames;
            this.resourceCollection = resourceCollection;
            this.isIgnored          = isIgnored;
        }

        public IGamePart[] GetAllGameParts(bool includeAliasPartsInFlatList) 
        {
            List<IGamePart> list = new List<IGamePart>();
            
            for (int i = 0; i < allGames.Length; i++) 
            {
                list.AddRange(allGames[i].GetGameParts(includeAliasPartsInFlatList));
            }
            return list.ToArray();
        }

        public IGame[] GetAllGames() 
        {
            return allGames;
        }

        public IGame[] GetChildGamesOnly() 
        {
            List<IGame> list = new List<IGame>();
            
            // Skip parent:
            for (int i = 1; i < allGames.Length; i++) {
                list.Add(allGames[i]);
            }
            return list.ToArray();
        }

        public string GetComment() 
        {
            return comment;
        }
        public void SetComment(string comment) 
        {
            this.comment = comment;
        }
        public IDescriptor[] GetDescriptors() 
        {
            return descriptors;
        }        
        public void SetDescriptors(IDescriptor[] descriptors) 
        {
            this.descriptors = descriptors;
        }

        public string GetFamilyDisplayName() 
        {
            return displayName;
        }
        public void SetFamilyDisplayName(string displayName) 
        {
            this.displayName = displayName;
        }

        public bool GetIgnored() 
        {
            return isIgnored;
        }

        public IGame GetParentGame() 
        {
            return allGames[0];
        }

        public IResourceCollection GetResourceCollection() 
        {
            return resourceCollection;
        }
        public void SetResourceCollection(IResourceCollection resourceCollection) 
        {
            this.resourceCollection = resourceCollection;
        }
    }
    public class BaseGame : IGame
    {
        protected IGamePart[] gameParts;

        protected string? launchName;
        protected string nameWithoutExt;

        public BaseGame(
            string nameWithoutExt
            , IGamePart[] gameParts
            , string? launchName = null)
        {
            this.gameParts = gameParts;
            this.nameWithoutExt = nameWithoutExt;
            this.launchName = launchName;
        }
        public int GetTotalRomsCount()
        {
            int total = 0;
            foreach (var part in gameParts)
            {
                total += part.GetChecksums().Length;
            }
            return total;
        }
        public int GetTotalAliasesCount()
        {
            int total = 0;
            foreach (var part in gameParts)
            {
                total += part.GetSoftwareAliases().Length;
            }
            return total;
        }

        public IGamePart[] GetGameParts(bool includeAliasParts)
        {
            if (!includeAliasParts)
            {
                return gameParts;
            }
            else
            {
                return CreateFlatPartListWithAliases(gameParts);
            }
        }

        public string? GetLaunchName()
        {
            return launchName;
        }

        public string GetNameWithoutExt()
        {
            return nameWithoutExt;
        }

        public bool HasLaunchName()
        {
            return !string.IsNullOrWhiteSpace(launchName);
        }
        private IGamePart[] CreateFlatPartListWithAliases(IGamePart[] parts)
        {
            List<IGamePart> list = new List<IGamePart>();

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                list.Add(part);
                list.AddRange(part.GetSoftwareAliases());
            }
            return list.ToArray();
        }
    }
    public class BaseGamePart : IGamePart
    {
        public bool Exclude { get; set; } = false;

        public string? LaunchName { get; }

        public string? Tag { get; }

        public string Fingerprint { get; }

        protected string fullpath;
        protected string nameWithExt;
        protected string path;
        IGamePart[] aliases;
        protected string[] romChecksums;
        protected string directoryId;
        protected string displayName;

        public BaseGamePart(
            string name
            , IGamePart[] aliases
            , string[] romChecksums
            , string directoryId
            , string displayName
            , string? launchName
            , string? tag)
        {
            this.fullpath = name;
            this.nameWithExt = name;

            LaunchName = launchName;
            Tag = tag;

            if (romChecksums == null) romChecksums = new string[] { };

            Fingerprint = FingerprintHelper.GetFingerprint(romChecksums);

            path = string.Empty;

            try
            {
                if (!string.IsNullOrWhiteSpace(name))
                    path = Path.GetDirectoryName(name);
            }
            catch (PathTooLongException)
            {
                path = name.Replace(nameWithExt, "");
            }

            this.aliases = aliases;
            this.romChecksums = romChecksums;
            this.directoryId = directoryId;
            this.displayName = displayName;
        }

        public string GetFullpath()
        {
            return fullpath;
        }
        public string GetName()
        {
            return nameWithExt;
        }
        public string GetPath()
        {
            return path;
        }
        public void SetPath(string path)
        {
            this.path = path;
            this.fullpath = Path.Combine(path, nameWithExt);
        }
        public void SetSoftwareAliases(IGamePart[] gameParts)
        {
            aliases = gameParts;
        }
        public IGamePart[] GetSoftwareAliases()
        {
            return aliases;
        }

        public string GetDirectoryId()
        {
            return directoryId;
        }
        public bool HasDirectoryId()
        {
            return (!string.IsNullOrWhiteSpace(directoryId));
        }

        public string[] GetChecksums()
        {
            return (string[])romChecksums.Clone();
        }

        public string GetChecksumsString()
        {
            return FingerprintHelper.SortFingerprintElementsToString(romChecksums);
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        public bool HasDisplayName()
        {
            return !string.IsNullOrWhiteSpace(displayName);
        }

        public void SetDirectoryId(string directoryId)
        {
            this.directoryId = directoryId;
        }
    }
}
