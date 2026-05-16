using com.RADIO.Datinate.RMVC.Shared;
using Datinate.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class AddToProjectMediator : RMediator 
    {
        private IAddToProjectView? view => (IAddToProjectView?)base.viewBase;

        public AddToProjectMediator(Type actor) : base(actor)
        {
        }

        public void SetProjectUiDatSubsetInfo(Dictionary<string, List<string>> dic) 
        {
            view?.SetProjectUiDatSubsetInfo(dic);
        }
        public void SetR2DatResources(IReadOnlyList<R2DatResourceDTO> r2DatResources) 
        {
            if (view != null)
                view.R2DatResources = r2DatResources;
        }
        public void SetView(DatVO datVO) 
        {
            view?.SetView(datVO);
        }
        
        private void OnAddToProject(
            DatVO datVO, 
            COLLECTION_SET_ENUM collectionSetEnum,
            DAT_GROUP_ENUM datGroup, 
            DAT_GROUP_TARGET_ENUM target,
            string? internalDescriptor,
            DatSubsetFilter? filter)
        {
            base.ExecuteCommand(
                new AddDatToProjectCmd(datVO, collectionSetEnum, target, datGroup, internalDescriptor, filter));
        }

        private void OnFetchDatSubsetInfo(DatVO datVO) 
            => base.ExecuteCommand(new FetchDatSubsetInfoCmd(datVO));
        

        protected override void Initialsed()
        {
            if (view != null)
            {
                view.AddDatToProjectEvt += OnAddToProject;
                view.FetchDatSubsetInfoEvt += OnFetchDatSubsetInfo;
                view.AddResourceBatchToProjectEvt += OnAddResourceBatchToProject;
            }
        }

        private void OnAddResourceBatchToProject(DatVO dat, DAT_GROUP_ENUM datGroupEnum)
        {
            base.ExecuteCommand(new AddAllMediaDatSubsetsToProjectCmd(dat, datGroupEnum));  
        }

        protected override void Disposing()
        {
            if (view != null)
            {
                view.AddDatToProjectEvt -= OnAddToProject;
                view.FetchDatSubsetInfoEvt -= OnFetchDatSubsetInfo;
                view.AddResourceBatchToProjectEvt -= OnAddResourceBatchToProject;
            }
        }
    }
}
