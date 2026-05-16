using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared.Rb;
using RMVC;

namespace Datinate.Shared
{
    public interface IMedia2View : IRContract
    {
        event Action<RbMediaItemAssignmentUpdate>? MediaAssignmentChangeEvt;
        event Action<ILookupSet, string, bool>? ShowMediaCardEvt;
        event Action? MediaCardDragStartEvt;
        event Action? MediaCardDragEndEvt;
        event Action? CloseMediaEvt;
        event Action<ILookupSet, string, bool, string>? RequestMediaContentEvt;
        
        void StartReceiveMediaDrop();
        void StopReceiveMediaDrop();
        
        void TeardownView();
        void ShowViewAssign(string searchName, IMediaCollection mediaCollection);
        void ShowViewPreview(string searchName);
        void InitialiseView(IReadOnlyCollection<ILookupSet> lookupSets);
        void EmptyView();
        void SetAssignedEntriesCache(IReadOnlyDictionary<string, HashSet<string>> assignedEntriesCache);
        void SetMediaCardContent(ILookupSet lookupSet, string urlOrHtml, bool isHtmlRawText, bool entryWasSelectedByUser, string entryName);
        void SetMediaCardContentNotAvailable(ILookupSet lookupSet, string entryName, bool entryWasSelectedByUser);
    }
}
