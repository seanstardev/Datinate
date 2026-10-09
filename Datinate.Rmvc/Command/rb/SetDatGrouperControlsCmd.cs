using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Rmvc.Command
{
    public class SetDatGrouperControlsCmd : RCommand
    {
        private readonly IGameEntity? entity;
        private readonly bool isFromAuto;

        public SetDatGrouperControlsCmd(IGameEntity? entity, bool isFromAuto)
        {
            this.entity = entity;
            this.isFromAuto = isFromAuto;
        }

        protected override void Run()
        {
            if (Facade.Instance?.DatGrouperControlsMediator is { } controls) 
            {
                controls.SetViewForEntity(entity, isFromAuto);
            } 
        }
    }
}
