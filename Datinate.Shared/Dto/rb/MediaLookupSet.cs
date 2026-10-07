namespace Datinate.Shared.Rb
{
    public class MediaLookupSet : ILookupSet
    {
        public RadioSourceDTO RadioSource { get; }
        public string Source { get; }

        private string[] pipedNames;
        private string id;

        private readonly IReadOnlyDictionary<string, string> entryNameLookupNameDic;
        private readonly IReadOnlyDictionary<string, string> lookupNameEntryNameDic;

        public MediaLookupSet(
            string id,
            Dictionary<string, string> nameDatEntryNameDic,
            string source,
            RadioSourceDTO radioSource)
        {
            this.id = id;

            Source = source;
            
            var dic = new Dictionary<string, string>();
            foreach (var kvp in nameDatEntryNameDic)
                dic.Add(kvp.Value, kvp.Key);

            this.entryNameLookupNameDic = nameDatEntryNameDic;


            lookupNameEntryNameDic = dic;


            pipedNames = nameDatEntryNameDic.Keys.ToArray();

            Array.Sort(pipedNames);
            RadioSource = radioSource;
        }

        public string? GetEntry(string lookupName)
        {
            return lookupNameEntryNameDic.TryGetValue(lookupName, out var entryName)
                ? entryName
                : null;
        }

        public string? GetLookup(string entryName)
        {
            return entryNameLookupNameDic.TryGetValue(entryName, out var lookupName)
                ? lookupName
                : null;
        }

        public IReadOnlyCollection<string> EntryNames => pipedNames;
        
        public string Id => id;

        public bool IsMedia => true;
        public bool IsRadioResource => false;
    }
}