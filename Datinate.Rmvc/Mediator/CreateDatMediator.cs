using Datinate.Rmvc.Command;
using Datinate.Shared.Dat;
using Datinate.Shared.Rmvc;
using RMVC;

namespace Datinate.Rmvc.Mediator
{
    internal class CreateDatMediator : RMediator 
    {
        private ICreateDatView? view => (ICreateDatView?)base.viewBase;

        public CreateDatMediator(Type actor) : base(actor)
        {
        }
        protected override void Initialsed()
        {
            if (view != null)
            {
                view.CreateDatEvt += OnCreateDatClick;
            }
        }

        protected override void Disposing()
        {
            if (view != null)
            {
                view.CreateDatEvt -= OnCreateDatClick;
            }
        }

        public void SetView(DatVO vo) 
        {
            view?.SetView(vo);
        }

        private void OnCreateDatClick(DatVO datVO, string fileFullpath, bool useMachineTags) 
            => base.ExecuteCommand(new CreateNewDatCmd(
                datVO, 
                fileFullpath, 
                useMachineTags));
    }
}
