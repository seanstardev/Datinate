using RMVC;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class ToggleDatGrouperLayoutCmd : RCommand
    {
        protected override void Run()
        {
            if (Facade.Instance?.DatGrouperSessionModel is not null)
            {
                DAT_GROUPER_LAYOUT_ENUM layout = Facade.Instance.DatGrouperSessionModel.ToggleCuratedLayout();
                Facade.Instance?.DatGrouperMediator?.SetScreenLayout(layout);
            }
        }
    }
}
