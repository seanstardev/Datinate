using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetDatGrouperControlsCmd : CommandBase<RVoid>
    {
        private readonly IGameEntity? entity;
        private readonly bool isFromAuto;

        public SetDatGrouperControlsCmd(IGameEntity? entity, bool isFromAuto)
        {
            this.entity = entity;
            this.isFromAuto = isFromAuto;
        }

        protected override async Task<RVoid> Run()
        {
            if (base.DatGrouperControlsMediator is { })
                base.DatGrouperControlsMediator.SetViewForEntity(entity, isFromAuto);
            
            return await Complete();
        }
    }
}
