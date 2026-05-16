using RadioLibCore.RadioDat;
using RMVC;

namespace com.RADIO.Datinate.RMVC
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
