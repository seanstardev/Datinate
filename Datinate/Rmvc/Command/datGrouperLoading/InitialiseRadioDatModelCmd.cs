using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace com.RADIO.Datinate.RMVC
{
    public class InitialiseRadioDatModelCmd : RCommand 
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
