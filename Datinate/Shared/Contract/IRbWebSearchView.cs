using RMVC;

namespace Datinate.Shared
{
    public interface IRbWebSearchView : IRContract
    {
        event Action<string>? LoadUrlEvt;
        void SetSearchTerms(string? gameName, string? systemName);
        void ClearView();
        void SearchCurrent();
    }
}
