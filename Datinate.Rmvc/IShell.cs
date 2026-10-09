using RMVC;

namespace Datinate.Rmvc
{
    public interface IShell: IRAppShell 
    {
        string? DatGrouperModeStartupProjectName { get; }

        // TODO: Is this needed?
        void ApplicationDoEventsHack();

        void SetMainFormVisible(bool visible);
        void SetCompareFormVisible(bool doShow);
        void SetCustomFormVisible(bool doShow);
        void SetProblemListFormVisible(bool doShow);
        void SetDatPathsUpdateFormVisible(bool doShow);
        void SetAddToProjectFormVisible(bool doShow);
        void SetProjectsFormVisible(bool doShow);
        void SetCreateDatFormVisible(bool doShow);
        void SetProgressFormVisible(bool doShow);

        void ShowDatGrouperWindowView();
        void ShowExportView();
        void ShowProjectsView();
        void HandleCompareFormClose();
        void SetProjectsFormTitle(string title);
        void StartResizeMonitor();

        void SetMainFormsSizeBarBackColorArgb(int colourArgb);
        void ExitApplication();

        bool CurrentProjectsPageIsProjectLoaderPage { get; }

        Task<string?> ShowLoadExpressionsDialog(string path);
        Task<string?> ShowSaveExpressionsDialog(string path);
    }
}
