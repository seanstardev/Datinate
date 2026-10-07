using RMVC;

namespace Datinate.Shared
{
    public interface IProblemListView : IRContract
    {
        void SetUnreadableView(string[] problemItems);
        void SetDuplicatesView(string[] problemItems);
        void ClearView();
    }
}
