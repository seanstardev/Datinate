using com.RADIO.Datinate.App.View.compare;
using com.RADIO.Datinate.App.View.createDat;
using com.RADIO.Datinate.App.View.customList;
using com.RADIO.Datinate.App.View.datDetails.addToProject;
using com.RADIO.Datinate.App.View.problemList;
using com.RADIO.Datinate.App.View.projects.datPathsUpdate;
using datinate.app;
using RMVC;

namespace com.RADIO.Datinate 
{
    public interface IShell: IRAppShell 
    {
        // Views:
        CompareView CompareView { get; }
        CreateDatView CreateDatView { get; }
        ProjectLoaderView ProjectLoaderView { get; }
        DatGrouperView GameFamilyView { get; }
        CustomListView CustomListView { get; }
        ProblemListView ProblemListView { get; }
        DatPathsUpdateView DatPathsUpdateView { get; }
        AddToProjectView AddToProjectView { get;  }
        ProgressView ProgressView { get;  }
        ExportView ExportView { get; }

        // Methods:
        void SetCompareFormVisible(bool doShow);
        void SetCustomFormVisible(bool doShow);
        void SetProblemListFormVisible(bool doShow);
        void SetDatPathsUpdateFormVisible(bool doShow);
        void SetAddToProjectFormVisible(bool doShow);
        void SetProjectsFormVisible(bool doShow);
        void SetCreateDatFormVisible(bool doShow);
        void SetProgressFormVisible(bool doShow);

        void ShowDatGrouperWindowView();
        void ShowContentPathsView();
        void ShowExportView();
        void ShowRbProjectsView();
        void HandleCompareFormClose();
        void HandleProjectsFormClose();
        void SetProjectsFormTitle(string title);
        void StartResizeMonitor();
        void SetMainFormsSizeBarBackColor(Color color);
        bool CurrentProjectsPageIsProjectLoaderPage { get; }
    }
}
