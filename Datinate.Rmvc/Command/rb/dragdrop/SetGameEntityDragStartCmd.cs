using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    internal class SetGameEntityDragStartCmd : CommandBase<RVoid>
    {
        private readonly DatGrouperEntryDTO datGrouperEntryDTO;

        public SetGameEntityDragStartCmd(DatGrouperEntryDTO datGrouperEntryDTO)
        {
            this.datGrouperEntryDTO = datGrouperEntryDTO;
        }

        protected override async Task<RVoid> Run()
        {
            base.MediaMediator?.StartReceiveGameEntityDrop();
            base.MainWebMediator?.StartReceiveGameEntityDrop(datGrouperEntryDTO);

            return await Complete();
        }
    }
}
