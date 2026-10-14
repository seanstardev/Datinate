using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Radio;
using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    public class MediaAssignmentMediator : RMediator<IMediaAssignmentView>
    {
        public void SetDescriptorDefinitions(
            IReadOnlySet20<DescriptorDefinitionDTO> descriptorDefinitions)
            => actor?.SetDescriptorDefinitions(descriptorDefinitions);
        
        public void SetScoring(DatGrouperScoring scoring)
            => actor?.SetScoring(scoring);
        
        public void SetView(IMediaCollection mediaCollection)
            => actor?.SetView(mediaCollection);
        
        public void SetViewMediaItem(
            string entryName,
            RadioSourceDTO radioSource)
            => actor?.SetViewMediaItem(entryName, radioSource);
        
        public void SetReadOnlyModeActive(bool readOnlyMode)
            => actor?.SetReadOnlyModeActive(readOnlyMode);
        
        public void ClearView()
            => actor?.ClearView();
        
        public void SetMediaCardDragStart()
            => actor?.SetMediaCardDragStart();

        public void SetMediaCardDragStop()
            => actor?.SetMediaCardDragStop();
        
        public void SetScoringActive(bool activate)
            => actor?.SetScoringActive(activate);
        
        private void OnLoadMediaInBrowser()
            => base.Execute(new ShowWebMediaInBrowserCmd());
        
        private void OnDescriptorsUpdate(
            IGameFamily family, 
            HashSet<string> checkedDescriptors, 
            string familyNotesText,
            bool descriptorsChanged)
            => base.Execute(new UpdateMediaCollectionMetaCmd(
                family, checkedDescriptors, familyNotesText, descriptorsChanged));
        
        protected override void Initialised()
        {
            if (actor == null) return;

            actor.MediaMetaUpdateEvt += OnDescriptorsUpdate;
            actor.LoadMediaInBrowserEvt += OnLoadMediaInBrowser;
        }

        protected override void Disposing()
        {
            if (actor == null) return;

            actor.MediaMetaUpdateEvt -= OnDescriptorsUpdate;
            actor.LoadMediaInBrowserEvt -= OnLoadMediaInBrowser;
        }
    }
}