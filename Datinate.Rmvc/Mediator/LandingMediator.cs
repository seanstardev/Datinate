
using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    public class LandingMediator : RMediator<ILandingView>
    {
        public void LoadDatPaths(DatRootDTO[] paths, string mameHashPath)   
            => actor?.SetDatRootPaths(paths, mameHashPath);
        
        public void SetDatGrouperProjects(DatGrouperProjectDTO[] projectVOs)
            => actor?.SetDatGrouperProjects(projectVOs);
        
        public void ActivateView()
            => actor?.ActivateView();
        
        private void OnLoadDatGrouperView(string projectName)
            => base.Execute(new LoadDatGrouperProjectsCmd(projectName, true, true));
        
        private void OnLoadDatManager(DatRootDTO[] datRootVOs, string mameHashPath) 
            => base.Execute(new LoadDatManagerCmd(datRootVOs, mameHashPath));    
        
        private void OnSaveDatRootPaths(DatRootDTO[] datRootVOs, string mameHashPath) 
            => base.Execute(new SaveDatRootPathsCmd(datRootVOs, mameHashPath));


        private void OnRootDatPathRemoved() 
        {
        
        }

        private void OnDatRootPathAdded() 
        {
        
        }

        protected override void Initialised()
        {
            if (actor != null)
            {
                actor.LoadDatManagerEvt += OnLoadDatManager;
                actor.DatRootPathAddedEvt += OnDatRootPathAdded;
                actor.RootDatPathRemovedEvt += OnRootDatPathRemoved;
                actor.SaveDatRootPathsEvt += OnSaveDatRootPaths;
                actor.LoadDatGrouperProjectEvt += OnLoadDatGrouperView;
            }
        }
        protected override void Disposing()
        {
            if (actor != null)
            {
                actor.LoadDatManagerEvt -= OnLoadDatManager;
                actor.DatRootPathAddedEvt -= OnDatRootPathAdded;
                actor.RootDatPathRemovedEvt -= OnRootDatPathRemoved;
                actor.SaveDatRootPathsEvt -= OnSaveDatRootPaths;
                actor.LoadDatGrouperProjectEvt -= OnLoadDatGrouperView;
            }
        }
    }
}
