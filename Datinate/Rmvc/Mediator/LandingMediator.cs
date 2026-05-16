using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class LandingMediator : RMediator 
    {
        private ILandingView? view => (ILandingView?)base.viewBase;

        public LandingMediator(Type actor) : base(actor)
        {
        }

        public void LoadDatPaths(DatRootDTO[] paths, string mameHashPath)
        {
            view?.SetDatRootPaths(paths, mameHashPath);
        }
        public void SetDatGrouperProjects(DatGrouperProjectDTO[] projectVOs)
        {
            view?.SetDatGrouperProjects(projectVOs);
        }

        public void ActivateView()
        {
            view?.ActivateView();
        }

        private void OnLoadDatGrouperView(string? projectName)
        {
            base.ExecuteCommand(new SetDatGrouperFormVisibleCmd());
            base.ExecuteCommand(new SetDatGrouperLoaderViewCmd(projectName));
        }
        private void OnLoadDatManager(DatRootDTO[] datRootVOs, string mameHashPath) =>
            base.ExecuteCommand(new LoadDatManagerCmd(datRootVOs, mameHashPath));    
        
        private void OnSaveDatRootPaths(DatRootDTO[] datRootVOs, string mameHashPath) =>
            base.ExecuteCommand(new SaveDatRootPathsCmd(datRootVOs, mameHashPath));


        private void OnRootDatPathRemoved() 
        {
        
        }

        private void OnDatRootPathAdded() 
        {
        
        }

        protected override void Initialsed()
        {
            if (view != null)
            {
                view.LoadDatManagerEvt += OnLoadDatManager;
                view.DatRootPathAddedEvt += OnDatRootPathAdded;
                view.RootDatPathRemovedEvt += OnRootDatPathRemoved;
                view.SaveDatRootPathsEvt += OnSaveDatRootPaths;
                view.LoadDatGrouperViewEvt += OnLoadDatGrouperView;
            }
        }
        protected override void Disposing()
        {
            if (view != null)
            {
                view.LoadDatManagerEvt -= OnLoadDatManager;
                view.DatRootPathAddedEvt -= OnDatRootPathAdded;
                view.RootDatPathRemovedEvt -= OnRootDatPathRemoved;
                view.SaveDatRootPathsEvt -= OnSaveDatRootPaths;
                view.LoadDatGrouperViewEvt -= OnLoadDatGrouperView;
            }
        }
    }
}
