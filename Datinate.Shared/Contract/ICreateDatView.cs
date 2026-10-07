using Datinate.Shared.Dat;
using RMVC;

namespace Datinate.Shared.Rmvc
{
    public interface ICreateDatView : IRContract
    {
        event Action<DatVO, string, bool>? CreateDatEvt;

        void SetView(DatVO vo);
        void Hide();
        void Show();
        void BringToFront();
    }
}
