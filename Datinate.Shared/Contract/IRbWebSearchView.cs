using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Shared.Rmvc
{
    public interface IRbWebSearchView : IRContract
    {
        event Action<string>? LoadUrlEvt;
        void SetSearchTerms(string? gameName, string? systemName);
        void ClearView();
        void SearchCurrent();
        void SetSearchEngine(WEB_SOURCE_ENUM engine, bool invokeSearchNow);
    }
}
