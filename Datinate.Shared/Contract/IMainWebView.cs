using Datinate.Shared.DatGrouper;
using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Shared.Rmvc
{
    public interface IMainWebView : IRContract
    {
        event Action<IGamePart>? ShowGamePartReportsEvt;
        event Action<DatGrouperEntryDTO>? PreviewMediaEvt;
        event Action<DatGrouperEntryDTO>? AssignMediaEvt;
        event Action<string>? SearchEntityNameEvt;
        void LoadUrl(string url);
        void LoadPageContent(string html);
        void ClearView(bool performFullReset);
        void UnloadPageContent(bool doNotUnloadRemoteContent);
        string? RenderGamePartGroupingReports(
            IGamePart part,
            ManagedListItemReport? managedReport, 
            string customiseReport);
        void StartReceiveGameEntityDrop(DatGrouperEntryDTO datGrouperEntryDTO);
        void StopReceiveGameEntityDrop();
        void SetCurationModeIsActive();
        void SetMediaIsAvailable();
        string? RenderCuratedImportErrorsReport(Dictionary<string, IGamePart?> importErrorReport);
    }
}
