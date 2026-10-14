using Datinate.Rmvc.Command;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    internal class DatPathsUpdateMediator : RMediator<IDatPathsUpdateView>
    {
        public void SetView(DatGrouperProjectDTO projectVO) 
            => actor?.SetView(projectVO);
        
        protected override void Initialised()
        {
            if (actor != null)
            {
                actor.SaveClickEvt += OnSave;
            }
        }

        protected override void Disposing()
        {
            if (actor != null)
            {
                actor.SaveClickEvt -= OnSave;
            }
        }

        private void OnSave(DatGrouperProjectDTO projectVO)
        {
            actor?.Hide();

            // TODO: Untested with showing dialog?:
            base.Execute(new SaveProjectCmd(projectVO));
        }
    }
}
