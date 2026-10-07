using RMVC;

namespace Datinate.Shared
{
    public interface IDatHierarchyView : IRContract
    {
        event Action? LoadDatsSelectedEvt;
        event Action<string>? SelectedDatChangeEvt;
        event Action? ToggleViewEvt;
        event Action? HomeClickEvt;
        void SetDatPathsView(string[] paths);
        void ClearView();
    }
}
