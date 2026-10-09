using RMVC;

namespace com.RADIO.Datinate 
{
    public interface IShell: IRAppShell 
    {
        string? DatGrouperModeStartupProjectName { get; }

        // Methods:
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
        void SetMainFormsSizeBarBackColor(Color color);
        void ExitApplication();

        bool CurrentProjectsPageIsProjectLoaderPage { get; }
    }
}
