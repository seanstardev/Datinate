using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using Datinate.Shared.Rb;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class Media2Mediator : RMediator
    {
        private IMedia2View? view => (IMedia2View?)base.viewBase;
        public Media2Mediator(Type view) : base(view)
        {
        }
        public void InitialiseView(IReadOnlyCollection<ILookupSet> lookupSets)
        {
            view?.InitialiseView(lookupSets);
        }
        public void SetAssignedEntriesCache(IReadOnlyDictionary<string, HashSet<string>> assignedEntriesCache)
        {
            view?.SetAssignedEntriesCache(assignedEntriesCache);
        }
        public void TeardownView()
        {
            view?.TeardownView();
        }

        public void EmptyView()
        {
            view?.EmptyView();
        }
        public void ShowViewPreview(string searchName)
        {
            view?.ShowViewPreview(searchName);
        }

        public void ShowViewAssign(string searchName, IMediaCollection mediaCollection)
        {
            view?.ShowViewAssign(searchName, mediaCollection);
        }
        public void SetMediaCardContent(
            ILookupSet lookupSet, 
            string urlOrHtml, 
            bool isHtmlRawText, 
            bool entryWasSelectedByUser, 
            string entryName)
        {
            view?.SetMediaCardContent(lookupSet, urlOrHtml, isHtmlRawText, entryWasSelectedByUser, entryName);
        }
        public void SetMediaCardContentNotAvailable(ILookupSet lookupSet, string entryName, bool entryWasSelectedByUser)
        {
            view?.SetMediaCardContentNotAvailable(lookupSet, entryName, entryWasSelectedByUser);
        }
        protected override void Initialsed()
        {
            if (view == null) return;

            view.ShowMediaCardEvt += OnShowMediaCard;
            view.MediaAssignmentChangeEvt += OnMediaAssignmentChange;
            view.MediaCardDragStartEvt += OnMediaCardDragStart;
            view.MediaCardDragEndEvt += OnMediaCardDragEnd;
            view.CloseMediaEvt += OnCloseMedia;
            view.RequestMediaContentEvt += OnRequestMediaContent;
        }

        private void OnRequestMediaContent(
            ILookupSet lookupSet, 
            string lookupName, 
            bool entryWasSelectedByUser, 
            string entryName)
        {
            base.ExecuteCommand(new FetchMediaCardContentCmd(lookupSet, lookupName, entryWasSelectedByUser, entryName));
        }

        private void OnMediaAssignmentChange(RbMediaItemAssignmentUpdate assignment)
        {
            base.ExecuteCommand(new UpdateMediaCollectionItemCmd(assignment));
        }

        private void OnShowMediaCard(ILookupSet lookupSet, string entryName, bool isAutoLoaded)
        {
            base.ExecuteCommand(new ShowMediaCardCmd(lookupSet, entryName, isAutoLoaded));
        }

        private void OnCloseMedia()
        {
            base.ExecuteCommand(new ExitMediaModeCmd());
        }

        internal void StopReceiveGameEntityDrop()
        {
            view?.StopReceiveMediaDrop();
        }
        public void StartReceiveGameEntityDrop()
        {
            view?.StartReceiveMediaDrop();
        }

        private void OnMediaCardDragStart()
        {
            base.ExecuteCommand(new SetMediaItemDragStartCmd());
        }

        private void OnMediaCardDragEnd()
        {
            base.ExecuteCommand(new SetMediaItemDragStopCmd());
        }

        protected override void Disposing()
        {
            if (view == null) return;

            view.MediaAssignmentChangeEvt -= OnMediaAssignmentChange;
            view.MediaCardDragStartEvt -= OnMediaCardDragStart;
            view.MediaCardDragEndEvt -= OnMediaCardDragEnd;
            view.CloseMediaEvt -= OnCloseMedia;
            view.ShowMediaCardEvt -= OnShowMediaCard;
            view.RequestMediaContentEvt -= OnRequestMediaContent;
        }
    }
}
