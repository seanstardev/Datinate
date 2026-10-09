using com.RADIO.Datinate.RMVC;
using Datinate.Rmvc.Dto;
using Datinate.Rmvc.Model;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Radio;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class InitialiseRadioDatModelCmd : RCommand 
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

        protected override void Run() 
        {
            RadioDatModel? radioDatModel = Facade.Instance?.RadioDatModel;
            radioDatModel?.SetRadioDat(projectVO, autoGroupTraceStore, autoGrouperSourceCollection);

            base.ExecuteCommand(new SetDescriptorDefinitionsCmd());
        }
    }
}
