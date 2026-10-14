using Datinate.Rmvc.Command;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Rmvc;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Mediator
{
    public class ProjectLoaderMediator : RMediator<IProjectLoaderView>
    {
        public bool IsProjectLoaded => actor?.IsProjectLoaded ?? false;

        public void SetView(
            DatGrouperProjectDTO[] projectVOs, string? projectNameToLoad = null) 
            => WithActor(view => view.SetView(projectVOs, projectNameToLoad));
        
        public void ClearView()
            => WithActor(view => view.ClearView());
        
        public void AddDat(
            DatGrouperProjectEntry datHeadlineVO, 
            DAT_GROUP_TARGET_ENUM datGroupTargetEnum) 
            => WithActor(view => view.AddDat(datHeadlineVO, datGroupTargetEnum, true));
        
        public void SetExpressionsFile(string expressionsXmlFullpath) 
            => WithActor(view => view.SetExpressionsFileForLastSelected(expressionsXmlFullpath));

        public DatGrouperProjectEntry[] GetAllDats() 
            => (actor == null) ? new DatGrouperProjectEntry[] { } : actor.GetAllDatHeadlines();
        
        private void OnLoadExpressionsFile() 
            => base.Execute(
                new SelectExpressionsFileCmd(EXPRESSIONS_FILE_TARGET_ENUM.PROJECT_LOADER));
        
        private void OnDatFullpathsChanged(DatGrouperProjectDTO project) 
            => base.Execute(
                new ProjectDatFullpathsChangedCmd(project));
        
        private void OnBuildProject(DatGrouperProjectDTO project) 
            => base.Execute(new StartDatGrouperCmd(project));

        private void OnSaveProject(DatGrouperProjectDTO project) 
            => base.Execute(new SaveProjectCmd(project));
       
        private void OnHighlight(DatGrouperProjectDTO project)
        {
            //           
        }

        private void OnEditExpressions(DatGrouperProjectEntry headline) 
            => base.Execute(new SetCustomiseViewCmd(headline));
        
        private void OnShowTreeView(string projectName)
            => base.Execute(new SetDatGrouperFormActiveCmd(projectName));
        
        private void OnLoadingProject() 
            => base.Execute(new ClearDatGrouperViewCmd());

        private void OnViewInitialised()
        {

        }

        protected override void Initialised()
        {
            if (actor == null) return;
            
            actor.ViewInitialisedEvt += OnViewInitialised;
            actor.BuildProjectEvt += OnBuildProject;
            actor.LoadExpressionsFileEvt += OnLoadExpressionsFile;
            actor.SaveProjectEvt += OnSaveProject;
            actor.EditExpressionsFileEvt += OnEditExpressions;
            actor.HighlightEvt += OnHighlight;
            actor.DatFullpathsChangedEvt += OnDatFullpathsChanged;
            actor.ShowTreeViewEvt += OnShowTreeView;
            actor.LoadingProjectEvt += OnLoadingProject;
        }

        protected override void Disposing()
        {
            if (actor == null) return;
            
            actor.ViewInitialisedEvt -= OnViewInitialised;
            actor.BuildProjectEvt -= OnBuildProject;
            actor.LoadExpressionsFileEvt -= OnLoadExpressionsFile;
            actor.SaveProjectEvt -= OnSaveProject;
            actor.EditExpressionsFileEvt -= OnEditExpressions;
            actor.HighlightEvt -= OnHighlight;
            actor.DatFullpathsChangedEvt -= OnDatFullpathsChanged;
            actor.ShowTreeViewEvt -= OnShowTreeView;
            actor.LoadingProjectEvt -= OnLoadingProject;
        }
    }
}
