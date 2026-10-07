using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Shared.Rmvc
{
    public interface ILandingView : IRContract
    {
        event Action<DatRootDTO[], string>? LoadDatManagerEvt;
        event Action<DatRootDTO[], string>? SaveDatRootPathsEvt;
        event Action? RootDatPathRemovedEvt;
        event Action? DatRootPathAddedEvt;
        event Action<string>? LoadDatGrouperProjectEvt;

        void ActivateView();
        void SetDatRootPaths(DatRootDTO[] paths, string mameHashPath);
        void SetDatGrouperProjects(DatGrouperProjectDTO[] projectVOs);
    }
}
