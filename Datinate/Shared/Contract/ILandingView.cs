using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace Datinate.Shared
{
    public interface ILandingView : IRContract
    {
        event Action<DatRootDTO[], string>? LoadDatManagerEvt;
        event Action<DatRootDTO[], string>? SaveDatRootPathsEvt;
        event Action? RootDatPathRemovedEvt;
        event Action? DatRootPathAddedEvt;
        event Action<string?>? LoadDatGrouperViewEvt;

        void ActivateView();
        void SetDatRootPaths(DatRootDTO[] paths, string mameHashPath);
        void SetDatGrouperProjects(DatGrouperProjectDTO[] projectVOs);
    }
}
