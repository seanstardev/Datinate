using com.RADIO.Datinate.RMVC.Shared;
using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace Datinate.Shared
{
    public interface IAddToProjectView : IRContract
    {
        event Action<
            DatVO, 
            COLLECTION_SET_ENUM,
            DAT_GROUP_ENUM, 
            DAT_GROUP_TARGET_ENUM,
            string?,
            DatSubsetFilter?>? AddDatToProjectEvt;

        public event Action<
            DatVO,
            DAT_GROUP_ENUM>? AddResourceBatchToProjectEvt;

        event Action<DatVO>? FetchDatSubsetInfoEvt;

        void SetView(DatVO datVO);
        void SetProjectUiDatSubsetInfo(Dictionary<string, List<string>> dic);

        IReadOnlyList<R2DatResourceDTO> R2DatResources { set; }
    }
}
