using RMVC;
using static Datinate.Shared.DatinateEnums;

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
