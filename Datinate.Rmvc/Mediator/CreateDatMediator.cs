using Datinate.Rmvc.Command;
using Datinate.Shared.Dat;
using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    internal class CreateDatMediator : RMediator<ICreateDatView>
    {
        public void SetView(DatVO vo)
            => actor?.SetView(vo);
        
        protected override void Initialised()
        {
            if (actor != null)
            {
                actor.CreateDatEvt += OnCreateDatClick;
            }
        }

        protected override void Disposing()
        {
            if (actor != null)
            {
                actor.CreateDatEvt -= OnCreateDatClick;
            }
        }
        
        private void OnCreateDatClick(
            DatVO datVO, string fileFullpath, bool useMachineTags) 
            => base.Execute(new CreateNewDatCmd(
                datVO, 
                fileFullpath, 
                useMachineTags));
    }
}
