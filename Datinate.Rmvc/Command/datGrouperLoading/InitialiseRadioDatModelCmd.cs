using Datinate.Rmvc.Dto;
using Datinate.Rmvc.Model;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class InitialiseRadioDatModelCmd : CommandBase<RVoid>
    {
        private readonly DatGrouperProjectDTO projectVO;
        private readonly AutoGrouperTraceStore? autoGroupTraceStore;
        private readonly IEnumerable<GameFamilyVO> autoGrouperSourceCollection;

        public InitialiseRadioDatModelCmd(
            DatGrouperProjectDTO projectVO,
            AutoGrouperTraceStore? autoGroupTraceStore,
            IEnumerable<GameFamilyVO> autoGrouperSourceCollection) 
        {
            this.projectVO = projectVO;
            this.autoGroupTraceStore = autoGroupTraceStore;
            this.autoGrouperSourceCollection = autoGrouperSourceCollection;
        }

        protected override async Task<RVoid> Run() 
        {
            base.RadioDatModel?.SetRadioDat(projectVO, autoGroupTraceStore, autoGrouperSourceCollection);

            await base.Execute(new SetDescriptorDefinitionsCmd());

            return await Complete();
        }
    }
}
