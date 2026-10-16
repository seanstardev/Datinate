using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    public class Media2Mediator : RMediator<IMedia2View>
    {
        public void InitialiseView(IReadOnlyCollection<ILookupSet> lookupSets)
            => actor?.InitialiseView(lookupSets);
        
        public void SetAudioEnvironmentPath(string? audioEnvironmentPath) 
            => actor?.SetAudioEnvironmentPath(audioEnvironmentPath);

        public void SetAssignedEntriesCache(
            IReadOnlyDictionary<string, HashSet<string>> assignedEntriesCache)
            => actor?.SetAssignedEntriesCache(assignedEntriesCache);
        
        public void TeardownView()
            => actor?.TeardownView();

        public void EmptyView() 
            => actor?.EmptyView();
        
        public void ShowViewPreview(IReadOnlySet20<string> searchNames)
            => actor?.ShowViewPreview(searchNames);
        
        public void ShowViewAssign(
            IReadOnlySet20<string> searchNames, 
            IMediaCollection mediaCollection, 
            IReadOnlyList<string> prompts)
            => actor?.ShowViewAssign(searchNames, mediaCollection, prompts);
        
        public void SetMediaCardContent(
            ILookupSet lookupSet, 
            string urlOrHtml, 
            bool isHtmlRawText, 
            bool entryWasSelectedByUser, 
            string entryName)
            => actor?.SetMediaCardContent(lookupSet, urlOrHtml, isHtmlRawText, entryWasSelectedByUser, entryName);
        
        public void SetMediaCardContentNotAvailable(
            ILookupSet lookupSet, 
            string entryName, 
            bool entryWasSelectedByUser)
            => actor?.SetMediaCardContentNotAvailable(lookupSet, entryName, entryWasSelectedByUser);
        
        internal void StopReceiveGameEntityDrop()
            => actor?.StopReceiveMediaDrop();
        
        public void StartReceiveGameEntityDrop()
            => actor?.StartReceiveMediaDrop();
        
        private void OnMediaCardDragStart()
            => base.Execute(new SetMediaItemDragStartCmd());
        
        private void OnMediaCardDragEnd()
            => base.Execute(new SetMediaItemDragStopCmd());
        
        private void OnRequestMediaContent(
            ILookupSet lookupSet,
            string lookupName,
            bool entryWasSelectedByUser,
            string entryName)
            => base.Execute(
                new FetchMediaCardContentCmd(lookupSet, lookupName, entryWasSelectedByUser, entryName));

        private void OnMediaAssignmentChange(MediaItemAssignmentUpdate assignment)
            => base.Execute(new UpdateMediaCollectionItemCmd(assignment));

        private void OnShowMediaCard(ILookupSet lookupSet, string entryName, bool isAutoLoaded)
            => base.Execute(new ShowMediaCardCmd(lookupSet, entryName, isAutoLoaded));

        private void OnCloseMedia()
            => base.Execute(new ExitMediaModeCmd());

        protected override void Initialised()
        {
            if (actor == null) return;

            actor.ShowMediaCardEvt += OnShowMediaCard;
            actor.MediaAssignmentChangeEvt += OnMediaAssignmentChange;
            actor.MediaCardDragStartEvt += OnMediaCardDragStart;
            actor.MediaCardDragEndEvt += OnMediaCardDragEnd;
            actor.CloseMediaEvt += OnCloseMedia;
            actor.RequestMediaContentEvt += OnRequestMediaContent;
        }

        protected override void Disposing()
        {
            if (actor == null) return;

            actor.MediaAssignmentChangeEvt -= OnMediaAssignmentChange;
            actor.MediaCardDragStartEvt -= OnMediaCardDragStart;
            actor.MediaCardDragEndEvt -= OnMediaCardDragEnd;
            actor.CloseMediaEvt -= OnCloseMedia;
            actor.ShowMediaCardEvt -= OnShowMediaCard;
            actor.RequestMediaContentEvt -= OnRequestMediaContent;
        }
    }
}
