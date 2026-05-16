namespace Datinate.Shared.Rb
{
    public interface ILookupSet
    {
        RadioSourceDTO RadioSource { get; }
        IReadOnlyCollection<string> EntryNames { get; }
        string? GetLookup(string entryName);
        string? GetEntry(string lookupName);

        string Id { get; }
        bool IsMedia { get; }
        bool IsRadioResource { get; }
    }
}
