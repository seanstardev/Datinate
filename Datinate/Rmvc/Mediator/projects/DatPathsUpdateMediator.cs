using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    internal class DatPathsUpdateMediator : RMediator
    {
        private IDatPathsUpdateView? view => (IDatPathsUpdateView?)base.viewBase;

        public DatPathsUpdateMediator(Type actor) : base(actor)
        {
        }

        void OnSave(DatGrouperProjectDTO projectVO) 
        {
            
            view?.Hide();
            
            // TODO: Untested with showing dialog?:
            base.ExecuteCommand(new SaveProjectCmd(projectVO, false));
        }

        public void SetView(DatGrouperProjectDTO projectVO) 
        {
            view?.SetView(projectVO);
        }

        protected override void Initialsed()
        {
            if (view != null)
            {
                view.SaveClickEvt += OnSave;
            }
        }

        protected override void Disposing()
        {
            if (view != null)
            {
                view.SaveClickEvt -= OnSave;
            }
        }
    }
}
