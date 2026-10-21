namespace Datinate.App.Shared.Media
{
    // A value snapshot, not a native row or a heap allocation for each lookup.
    public readonly record struct MediaEntryInfo(
        int BackingIndex,
        string Name,
        int ScorePct,
        bool IsAlreadyAssigned);
}
