using com.RADIO.Datinate.RMVC.Shared;
using datinate.app;
using Datinate.Shared;
using Datinate.Shared.Rb;
using RadioLibCore.RadioDat;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class MediaAssignmentMediator : RMediator
    {
        private IMediaAssignmentView? view => (IMediaAssignmentView?)base.viewBase;
        public MediaAssignmentMediator(Type view) : base(view)
        {
        }
        public void SetDescriptorDefinitions(IReadOnlySet<DescriptorDefinitionDTO> descriptorDefinitions)
        {
            view?.SetDescriptorDefinitions(descriptorDefinitions);
        }
        public void SetScoring(DatGrouperScoring scoring)
        {
            view?.SetScoring(scoring);
        }

        public void SetView(IMediaCollection mediaCollection)
        {
            view?.SetView(mediaCollection);
        }
        public void SetViewMediaItem(
            string entryName,
            RadioSourceDTO radioSource)
        {
            view?.SetViewMediaItem(entryName, radioSource);
        }
        public void SetReadOnlyModeActive(bool readOnlyMode)
        {
            view?.SetReadOnlyModeActive(readOnlyMode);
        }

        public void ClearView()
        {
            view?.ClearView();
        }

        public void SetMediaCardDragStart()
        {
            view?.SetMediaCardDragStart();
        }

        public void SetMediaCardDragStop()
        {
            view?.SetMediaCardDragStop();
        }
        protected override void Disposing()
        {
            if (view == null) return;

            view.MediaMetaUpdateEvt -= OnDescriptorsUpdate;
            view.LoadMediaInBrowserEvt -= OnLoadMediaInBrowser;
        }

        protected override void Initialsed()
        {
            if (view == null) return;

            view.MediaMetaUpdateEvt += OnDescriptorsUpdate;
            view.LoadMediaInBrowserEvt += OnLoadMediaInBrowser;
        }

        private void OnLoadMediaInBrowser()
        {
            base.ExecuteCommand(new ShowWebMediaInBrowserCmd());
        }

        private void OnDescriptorsUpdate(
            IGameFamily family, 
            HashSet<string> checkedDescriptors, 
            string familyNotesText,
            bool descriptorsChanged)
        {
            base.ExecuteCommand(new UpdateMediaCollectionMetaCmd(family, checkedDescriptors, familyNotesText, descriptorsChanged));
        }
    }
}
