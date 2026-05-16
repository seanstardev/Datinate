using com.RADIO.Datinate.RMVC.Shared;
using RMVC;

namespace Datinate.Shared
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
