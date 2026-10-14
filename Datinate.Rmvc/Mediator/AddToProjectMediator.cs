using Datinate.Rmvc.Command;
using Datinate.Shared;
using Datinate.Shared.Dat;
using Datinate.Shared.Rmvc;
using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Mediator
{
    public class AddToProjectMediator : RMediator<IAddToProjectView>
    {
        public void SetProjectUiDatSubsetInfo(Dictionary<string, List<string>> dic) 
            => actor?.SetProjectUiDatSubsetInfo(dic);

        public void SetView(DatVO datVO)
            => actor?.SetView(datVO);

        public void SetR2DatResources(IReadOnlyList<R2DatResourceDTO> r2DatResources) 
        {
            if (actor != null)
                actor.R2DatResources = r2DatResources;
        }

        private void OnAddToProject(
            DatVO datVO, 
            COLLECTION_SET_ENUM collectionSetEnum,
            DAT_GROUP_ENUM datGroup, 
            DAT_GROUP_TARGET_ENUM target,
            string? internalDescriptor,
            DatSubsetFilter? filter)
            => base.Execute(new AddDatToProjectCmd(
                datVO, collectionSetEnum, target, datGroup, internalDescriptor, filter));
        

        private void OnFetchDatSubsetInfo(DatVO datVO) 
            => base.Execute(new FetchDatSubsetInfoCmd(datVO));
        
        protected override void Initialised()
        {
            if (actor != null)
            {
                actor.AddDatToProjectEvt += OnAddToProject;
                actor.FetchDatSubsetInfoEvt += OnFetchDatSubsetInfo;
                actor.AddResourceBatchToProjectEvt += OnAddResourceBatchToProject;
            }
        }

        private void OnAddResourceBatchToProject(DatVO dat, DAT_GROUP_ENUM datGroupEnum)
            => base.Execute(new AddAllMediaDatSubsetsToProjectCmd(dat, datGroupEnum));  
        
        protected override void Disposing()
        {
            if (actor != null)
            {
                actor.AddDatToProjectEvt -= OnAddToProject;
                actor.FetchDatSubsetInfoEvt -= OnFetchDatSubsetInfo;
                actor.AddResourceBatchToProjectEvt -= OnAddResourceBatchToProject;
            }
        }
    }
}
