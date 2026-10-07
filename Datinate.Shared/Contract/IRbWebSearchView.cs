using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Shared
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
