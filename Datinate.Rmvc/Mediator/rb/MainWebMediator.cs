using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Radio;
using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    public class MainWebMediator : RMediator<IMainWebView>
    {
        public void SetMediaIsAvailable()
            => actor?.SetMediaIsAvailable();

        public void LoadPageContent(string html)
            => actor?.LoadPageContent(html);

        public void LoadUrl(string url)
            => actor?.LoadUrl(url);

        public void ClearView(bool performFullReset)
            => actor?.ClearView(performFullReset);

        public string? RenderGamePartGroupingReports(
            IGamePart part,
            DatCustomisationDecisionReport? managedReport,
            AutomatedGroupingReport? automatedReport)
            => actor?.RenderGamePartGroupingReports(
                part, managedReport, automatedReport) ?? null;

        public string? RenderCuratedImportErrorsReport(Dictionary<string, IGamePart?> importErrorReport)
            => actor?.RenderCuratedImportErrorsReport(importErrorReport);

        public void UnloadPageContent(bool doNotUnloadRemoteContent)
            => actor?.UnloadPageContent(doNotUnloadRemoteContent);

        private void OnPreviewMedia(DatGrouperEntryDTO dto)
            => base.Execute(new OpenMediaCollectionCmd(true, dto));

        private void OnAssignMedia(DatGrouperEntryDTO dto)
            => base.Execute(new OpenMediaCollectionCmd(false, dto));

        private void OnShowGamePartReports(IGamePart part)
            => base.Execute(new ShowRbPartGroupingReportsCmd(part));

        private void OnSearchEntityName(string searchName)
        {
            base.Execute(new SetWebSearchTermsCmd(searchName, null, false));
            base.Execute(new SetWebSearchEngineCmd(DatinateEnums.WEB_SOURCE_ENUM.Google, true));
        }

        internal void StartReceiveGameEntityDrop(DatGrouperEntryDTO datGrouperEntryDTO)
            => actor?.StartReceiveGameEntityDrop(datGrouperEntryDTO);

        internal void StopReceiveGameEntityDrop()
            => actor?.StopReceiveGameEntityDrop();

        public void SetCurationModeActive()
            => actor?.SetCurationModeIsActive();

        protected override void Initialised()
        {
            if (actor == null) return;

            actor.ShowGamePartReportsEvt += OnShowGamePartReports;
            actor.AssignMediaEvt += OnAssignMedia;
            actor.PreviewMediaEvt += OnPreviewMedia;
            actor.SearchEntityNameEvt += OnSearchEntityName;
        }

        protected override void Disposing()
        {
            if (actor == null) return;
            actor.ShowGamePartReportsEvt -= OnShowGamePartReports;
            actor.AssignMediaEvt -= OnAssignMedia;
            actor.PreviewMediaEvt -= OnPreviewMedia;
            actor.SearchEntityNameEvt -= OnSearchEntityName;
        }
    }
}