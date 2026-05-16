namespace Datinate.Shared.Rb
{
    public class ResourceLookupSet : ILookupSet
    {
        public RadioSourceDTO RadioSource { get; }
        
        private readonly string sourceId;
        private readonly IReadOnlyDictionary<string, string> entryNameLookupNameDic;
        private readonly IReadOnlyDictionary<string, string> lookupNameEntryNameDic;
        private readonly string[] pipedNames;

        public ResourceLookupSet(
            string sourceId, 
            Dictionary<string, string> entryNamesDic,
            RadioSourceDTO radioSource)
        {
            this.sourceId = sourceId;
            
            entryNameLookupNameDic = entryNamesDic;

            var dic = new Dictionary<string, string>();
            foreach (var kvp in entryNameLookupNameDic)
                _ = dic.TryAdd(kvp.Value, kvp.Key); 

            lookupNameEntryNameDic = dic;

            pipedNames = entryNamesDic.Keys.ToArray();
            RadioSource = radioSource;
        }
        public IReadOnlyCollection<string> EntryNames =>
            pipedNames;
        public string? GetLookup(string entryName)
        {
            return entryNameLookupNameDic.TryGetValue(entryName, out var lookupName)
                ? lookupName
                : null;
        }
        public string? GetEntry(string lookupName)
        {
            return lookupNameEntryNameDic.TryGetValue(lookupName, out var entryName)
                ? entryName
                : null;
        }
        
        public string Id => sourceId;
        
        public bool IsMedia => false;

        public bool IsRadioResource => true;
    }
}
