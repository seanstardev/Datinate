namespace RadioLibCore.RadioDat 
{
    public interface IGameEntity { }
 

    public interface IGameFamily : IGameEntity
    {
        string GetFamilyDisplayName();
        void SetFamilyDisplayName(string displayName);
        
        IGame GetParentGame();
        IGame[] GetChildGamesOnly();

        // First game in array is always parent:
        IGame[] GetAllGames();

        IGamePart[] GetAllGameParts(bool includeAliasPartsInFlatList);

        IResourceCollection GetResourceCollection();

        IDescriptor[] GetDescriptors();
        string GetComment();
        bool GetIgnored();
    }

    public interface IGame : IGameEntity
    {
        string GetNameWithoutExt();

        bool HasLaunchName();
        string? GetLaunchName();

        IGamePart[] GetGameParts(bool includeAliasParts);
    }

    public interface IGamePart : IGameEntity
    {
        bool Exclude { get; set; }
        string? LaunchName { get; }
        string Fingerprint { get; }
        string? Tag { get; }

        string GetName();

        string GetFullpath();

        string GetPath();

        string[] GetChecksums();

        IGamePart[] GetSoftwareAliases();
        void SetSoftwareAliases(IGamePart[] gameParts);
        bool HasDirectoryId();
        string? GetDirectoryId();
        void SetDirectoryId(string directoryId);

        string GetDisplayName();
        bool HasDisplayName();
        void SetPath(string path);

        // TODO: GetName() ? e.g. Mame description name instead of just mame launch name?
    }
}
