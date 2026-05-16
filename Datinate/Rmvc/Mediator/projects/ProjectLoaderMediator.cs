using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class ProjectLoaderMediator : RMediator 
    {
        private IProjectLoaderView? view => (IProjectLoaderView?)base.viewBase;

        public ProjectLoaderMediator(Type actor) : base(actor)
        {

        }
        public bool IsProjectLoaded => view?.IsProjectLoaded ?? false;
        public void ReloadCurrentProject()
        {
            view?.ReloadCurrentProject();
        }

        public void SetView(DatGrouperProjectDTO[] projectVOs, string? projectToLoad = null) 
        {
            view?.SetView(projectVOs, projectToLoad);
        }

        public void ClearView()
        {
            view?.ClearView();
        }

        public void AddDat(
            DatGrouperProjectEntry datHeadlineVO, 
            DAT_GROUP_TARGET_ENUM datGroupTargetEnum) 
        {
            view?.AddDat(datHeadlineVO, datGroupTargetEnum, true);
        }

        public void SetExpressionsFile(string expressionsXmlFullpath) 
        {
            view?.SetExpressionsFileForLastSelected(expressionsXmlFullpath);
        }

        public DatGrouperProjectEntry[] GetAllDats() 
        {
            return (view == null) ? new DatGrouperProjectEntry[] { } : view.GetAllDatHeadlines();
        }

        private void OnLoadExpressionsFile() 
        {
            base.ExecuteCommand(
                new SelectExpressionsFileCmd(EXPRESSIONS_FILE_TARGET_ENUM.PROJECT_LOADER));
        }

        private void OnDatFullpathsChanged(DatGrouperProjectDTO project) 
        {
            base.ExecuteCommand(
                new ProjectDatFullpathsChangedCmd(project));
        }

        private void OnBuildProject(DatGrouperProjectDTO project) 
        {
            base.ExecuteCommand(new StartDatGrouperCmd(project));
        }

        private void OnSaveProject(DatGrouperProjectDTO project) 
        {
            base.ExecuteCommand(new SaveProjectCmd(project, false));
        }

        private void OnHighlight(DatGrouperProjectDTO project)
        {
            base.ExecuteCommand(new HighlightProjectDatsCmd(
                project.SoftwareEntries.ToArray()
                , project.SoftwareIgnoreEntries.ToArray()));            
        }

        private void OnEditExpressions(DatGrouperProjectEntry headline) 
        {
            base.ExecuteCommand(new SetCustomiseViewCmd(headline));
        }

        private void OnShowTreeView(string projectName) 
        {
            base.ExecuteCommand(new SetDatGrouperFormActiveCmd(projectName));
        }

        private void OnLoadingProject() 
        {
            base.ExecuteCommand(new ClearDatGrouperViewCmd());
        }

        protected override void Initialsed()
        {
            if (view == null) return;
            
            view.ViewInitialisedEvt += OnViewInitialised;
            view.BuildProjectEvt += OnBuildProject;
            view.LoadExpressionsFileEvt += OnLoadExpressionsFile;
            view.SaveProjectEvt += OnSaveProject;
            view.EditExpressionsFileEvt += OnEditExpressions;
            view.HighlightEvt += OnHighlight;
            view.DatFullpathsChangedEvt += OnDatFullpathsChanged;
            view.ShowTreeViewEvt += OnShowTreeView;
            view.LoadingProjectEvt += OnLoadingProject;
        }

        protected override void Disposing()
        {
            if (view == null) return;
            
            view.ViewInitialisedEvt -= OnViewInitialised;
            view.BuildProjectEvt -= OnBuildProject;
            view.LoadExpressionsFileEvt -= OnLoadExpressionsFile;
            view.SaveProjectEvt -= OnSaveProject;
            view.EditExpressionsFileEvt -= OnEditExpressions;
            view.HighlightEvt -= OnHighlight;
            view.DatFullpathsChangedEvt -= OnDatFullpathsChanged;
            view.ShowTreeViewEvt -= OnShowTreeView;
            view.LoadingProjectEvt -= OnLoadingProject;
        }

        private void OnViewInitialised()
        {

        }
    }
}
