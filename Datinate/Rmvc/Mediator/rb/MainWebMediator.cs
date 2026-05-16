using datinate.app;
using datinate.shared;
using Datinate.Shared;
using RadioLibCore.RadioDat;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class MainWebMediator : RMediator
    {
        private IMainWebView? view => (IMainWebView?)base.viewBase;
        public MainWebMediator(Type view) : base(view)
        {

        }
        public void SetMediaIsAvailable() 
            => view?.SetMediaIsAvailable();
        

        public void LoadPageContent(string html)
        {
            view?.LoadPageContent(html);
        }

        public void LoadUrl(string url)
        {
            view?.LoadUrl(url);  
        }

        public void ClearView(bool performFullReset)
        {
            view?.ClearView(performFullReset);
        }

        public string? RenderGamePartGroupingReports(
            IGamePart part,
            ManagedListItemReport? managedReport,
            string customiseReport)
        {
            return view?.RenderGamePartGroupingReports(part, managedReport, customiseReport) ?? null;
        }
        public string? RenderCuratedImportErrorsReport(Dictionary<string, IGamePart?> importErrorReport)
        {
            return view?.RenderCuratedImportErrorsReport(importErrorReport);
        }
        public void UnloadPageContent(bool doNotUnloadRemoteContent)
        {
            view?.UnloadPageContent(doNotUnloadRemoteContent);
        }

        protected override void Initialsed()
        {
            if (view == null) return;

            view.ShowGamePartReportsEvt += OnShowGamePartReports;
            view.AssignMediaEvt += OnAssignMedia;
            view.PreviewMediaEvt += OnPreviewMedia;
            view.SearchEntityNameEvt += OnSearchEntityName;
        }

        private void OnPreviewMedia(DatGrouperEntryDTO dto)
        {
            base.ExecuteCommand(new EnterMediaModeCmd(true, dto.IsFromAuto));
            base.ExecuteCommand(new ShowMediaCollectionCmd(dto));
        }

        private void OnAssignMedia(DatGrouperEntryDTO dto)
        {
            base.ExecuteCommand(new EnterMediaModeCmd(false, dto.IsFromAuto));
            base.ExecuteCommand(new ShowMediaCollectionCmd(dto));
        }

        private void OnShowGamePartReports(IGamePart part)
        {
            base.ExecuteCommand(new ShowRbPartGroupingReportsCmd(part));
        }

        protected override void Disposing()
        {
            if (view == null) return;
            view.ShowGamePartReportsEvt -= OnShowGamePartReports;
            view.AssignMediaEvt -= OnAssignMedia;
            view.PreviewMediaEvt -= OnPreviewMedia;
            view.SearchEntityNameEvt -= OnSearchEntityName;
        }

        private void OnSearchEntityName(string searchName)
        {
            base.ExecuteCommand(new SetWebSearchTermsCmd(searchName, null, true));
        }

        internal void StartReceiveGameEntityDrop(DatGrouperEntryDTO datGrouperEntryDTO)
        {
            view?.StartReceiveGameEntityDrop(datGrouperEntryDTO);
        }
        internal void StopReceiveGameEntityDrop()
        {
            view?.StopReceiveGameEntityDrop();
        }

        public void SetCurationModeActive() =>
            view?.SetCurationModeIsActive();
    }
}
