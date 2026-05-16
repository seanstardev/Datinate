namespace RadioLibCore.RadioDat 
{
    public interface IResourceCollection 
    {    
        IResourceItem[] GetResourceItems();
        IResourceItem[] GetMediaItems();
        IResourceItem[] GetSupportItems();

        Dictionary<string, IResourceItem[]> GetResourceItemsLookupSorted(string path);

        Dictionary<string, IResourceItem[]> GetMediaItemsPathSorted();
        Dictionary<string, IResourceItem[]> GetResourceItemsPathSorted();
        Dictionary<string, IResourceItem[]> GetSupportItemsPathSorted();

        void SetSupportItems(IResourceItem[] supportItems);
        void SetResourceItems(IResourceItem[] resourceItems);
        void SetMediaItems(IResourceItem[] mediaItems);

        void EmptyMediaItems();
        void EmptyResourceItems();
        void EmptySupportItems();

        void AddResourceItem(IResourceItem item);
        void AddMediaItem(IResourceItem item);
        void AddSupportItem(IResourceItem item);

        bool RemoveResourceItem(string path, string filenameWithExt, string lookup);
        bool RemoveSupportItem(string path, string filenameWithExt);
        bool RemoveMediaItem(string path, string filenameWithExt);

        bool IsEmpty();
    }

    public interface IResourceItem
    {
        string GetFilenameWithExt();
        string GetPath();
        string? GetLookup();
        string? GetSource();
        string? GetType_();
        bool IsRadioResource();
        string GetFullpath();
        void SetType(string type);
        string GetFingerprint();
        bool HasFingerprint();
        string GetDirectoryId();
        void SetDirectoryId(string directoryId);
        void SetPath(string path);
    }

    public interface IDescriptor
    {
        string Name { get; }
        bool IsChecked { get; }
    }
}
