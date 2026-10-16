namespace Datinate.Shared.Dat
{

    /**
     * Shared by a few dtos to centralise unit rendering in tables / listViews:
     */
    public interface IByteReporter {

        ulong GetTotalSize();
    }
}
