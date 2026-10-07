using RMVC;

namespace Datinate.Shared.Rmvc
{
    public interface IProblemListView : IRContract
    {
        void SetUnreadableView(string[] problemItems);
        void SetDuplicatesView(string[] problemItems);
        void ClearView();
    }
}
