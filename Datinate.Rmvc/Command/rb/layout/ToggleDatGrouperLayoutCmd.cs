using RMVC;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Command
{
    internal class ToggleDatGrouperLayoutCmd : CommandBase<RVoid>
    {
        protected override async Task<RVoid> Run()
        {
            if (base.DatGrouperSessionModel is not null)
            {
                DAT_GROUPER_LAYOUT_ENUM layout = base.DatGrouperSessionModel.ToggleCuratedLayout();
                base.DatGrouperMediator?.SetScreenLayout(layout);
            }

            return await Complete();
        }
    }
}
