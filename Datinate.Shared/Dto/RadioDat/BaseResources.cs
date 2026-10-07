namespace RadioLibCore.RadioDat 
{
    public class BaseResourceCollection : IResourceCollection 
    {
        public static BaseResourceCollection Empty 
        {
            get 
            {
                return new BaseResourceCollection(
                    new IResourceItem[] { }
                    , new IResourceItem[] { }
                    , new IResourceItem[] { });
            }
        }

        protected IResourceItem[] resourceItems;
        protected IResourceItem[] mediaItems;
        protected IResourceItem[] supportItems;

        public BaseResourceCollection(
            IResourceItem[] resourceItems,
            IResourceItem[] mediaItems,
            IResourceItem[] supportItems) 
        {
            this.resourceItems = resourceItems;
            this.mediaItems = mediaItems;
            this.supportItems = supportItems;
        }

        public Dictionary<string, IResourceItem[]> GetMediaItemsPathSorted() 
            => CreateDictionary(mediaItems);
        
        public Dictionary<string, IResourceItem[]> GetResourceItemsPathSorted()
            => CreateDictionary(resourceItems);
        
        public Dictionary<string, IResourceItem[]> GetSupportItemsPathSorted()
            => CreateDictionary(supportItems);
        
        public IResourceItem[] GetMediaItems() 
            => mediaItems;
        
        public IResourceItem[] GetResourceItems()
            => resourceItems;
        
        public IResourceItem[] GetSupportItems()
            => supportItems;
        

        public void SetMediaItems(IResourceItem[] mediaItems)
            => this.mediaItems = mediaItems;
        
        public void SetResourceItems(IResourceItem[] resourceItems) 
            => this.resourceItems = resourceItems;
        

        public void SetSupportItems(IResourceItem[] supportItems)
            => this.supportItems = supportItems;
        

        public void EmptyResourceItems()
            => this.resourceItems = new IResourceItem[] { };
        
        public void EmptyMediaItems()
            => this.mediaItems = new IResourceItem[] { };
       
        public void EmptySupportItems()
            => this.supportItems = new IResourceItem[] { };
        
        public void AddResourceItem(IResourceItem resourceItem) 
        {
            List<IResourceItem> list = resourceItems.ToList();
            list.Add(resourceItem);
            resourceItems = list.ToArray();
        }
        public void AddMediaItem(IResourceItem mediaItem) 
        {
            List<IResourceItem> list = mediaItems.ToList();
            list.Add(mediaItem);
            mediaItems = list.ToArray();
        }
        public void AddSupportItem(IResourceItem supportItem) 
        {
            List<IResourceItem> list = supportItems.ToList();
            list.Add(supportItem);
            supportItems = list.ToArray();
        }
        
        public Dictionary<string, IResourceItem[]> GetResourceItemsLookupSorted(string path) 
        {
            IResourceItem[] items = CreateDictionary(resourceItems)[path];
            Dictionary<string, List<IResourceItem>> working = new Dictionary<string, List<IResourceItem>>();

            foreach (var item in items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.GetLookup()))
                    continue;

                if (!working.ContainsKey(item.GetLookup()!))
                    working.Add(item.GetLookup()!, new List<IResourceItem>());

                working[item.GetLookup()!].Add(item);
            }

            Dictionary<string, IResourceItem[]> final = new Dictionary<string, IResourceItem[]>();
            
            foreach (KeyValuePair<string, List<IResourceItem>> pair in working)
                final.Add(pair.Key, pair.Value.ToArray());

            return final;
        }

        public bool RemoveResourceItem(string path, string filenameWithExt, string lookup) 
        {
            int startSize = resourceItems.Length;
            resourceItems = RemoveItem(resourceItems, path, filenameWithExt, lookup);
            return resourceItems.Length < startSize;
        }

        public bool RemoveSupportItem(string path, string filenameWithExt) 
        {
            int startSize = supportItems.Length;
            supportItems = RemoveItem(supportItems, path, filenameWithExt, null);
            return supportItems.Length < startSize;
        }
        public bool RemoveMediaItem(string path, string filenameWithExt) 
        {
            int startSize = mediaItems.Length;
            mediaItems = RemoveItem(mediaItems, path, filenameWithExt, null);
            return mediaItems.Length < startSize;
        }

        IResourceItem[] RemoveItem(IResourceItem[] items, string path, string filenameWithExt, string? lookup) 
        {
            for (int i = 0; i < items.Length; i++) 
            {
                var item = items[i];

                if (item.GetPath() == path && item.GetFilenameWithExt() == filenameWithExt && item.GetLookup() == lookup) 
                {
                    List<IResourceItem> list = items.ToList();
                    list.Remove(item);
                    
                    return list.ToArray();
                }
            }
            return items;
        }
        Dictionary<string, IResourceItem[]> CreateDictionary(IResourceItem[] items) 
        {
            Dictionary<string, List<IResourceItem>> working = new Dictionary<string, List<IResourceItem>>();
            
            for (int i = 0; i < items.Length; i++) 
            {
                var item = items[i];

                // TODO: Can happen if a resource does not have an info.xml. Happened with Supplemental test.
                if (item == null || string.IsNullOrWhiteSpace(item.GetPath())) 
                    continue;
                
                if (!working.ContainsKey(item.GetPath()))
                    working.Add(item.GetPath(), new List<IResourceItem>());

                working[item.GetPath()].Add(item);
            }

            Dictionary<string, IResourceItem[]> final = new Dictionary<string, IResourceItem[]>();

            foreach (KeyValuePair<string, List<IResourceItem>> pair in working)
                final.Add(pair.Key, pair.Value.ToArray());

            return final;
        }

        public bool IsEmpty() 
        {
            if (resourceItems.Length == 0 && supportItems.Length == 0 && mediaItems.Length == 0) 
                return true;
            else 
                return false;        
        }
    }
    public class BaseResourceItem : IResourceItem
    {
        private readonly string filenameWithExt;
        private string path;
        private readonly string? source;
        private string? type;
        private readonly string? lookup = null;
        private readonly string fingerprint;
        private string directoryId;

        public BaseResourceItem(
            string filenameWithExt,
            string path,
            string? source,
            string? type,
            string? lookup,
            string fingerprint,
            string directoryId)
        {
            this.filenameWithExt = filenameWithExt;
            this.path = path;
            this.source = source;
            this.type = type;
            this.lookup = lookup;
            this.fingerprint = fingerprint;
            this.directoryId = directoryId;
        }

        public string GetFilenameWithExt()
            => filenameWithExt;

        public string GetDirectoryId()
            => directoryId;


        public string GetFullpath()
        {
            if (IsRadioResource())
            {
                return Path.Combine(path, lookup, filenameWithExt);
            }
            else
                return Path.Combine(path, filenameWithExt);
        }

        public string? GetLookup()
            => lookup;

        public void SetPath(string path)
            => this.path = path;

        public string GetPath()
            => path;

        public string? GetSource()
            => source;

        public string? GetType_()
            => type;

        public string GetFingerprint()
            => fingerprint;

        public bool HasFingerprint()
            => !string.IsNullOrWhiteSpace(fingerprint);

        public bool IsRadioResource()
            => !string.IsNullOrWhiteSpace(lookup);

        public void SetType(string type)
            => this.type = type;

        public void SetDirectoryId(string directoryId)
            => this.directoryId = directoryId;
    }
    public class BaseDescriptor : IDescriptor
    {
        public string Name { get; }
        public bool IsChecked { get; }

        public BaseDescriptor(
            string name,
            bool isChecked)
        {
            Name = name;
            IsChecked = isChecked;
        }
    }
}

