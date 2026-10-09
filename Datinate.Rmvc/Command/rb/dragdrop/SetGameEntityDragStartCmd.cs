using Datinate.Shared.DatGrouper;
using RMVC;

namespace Datinate.Rmvc.Command
{
    public class SetGameEntityDragStartCmd : RCommand
    {
        private readonly DatGrouperEntryDTO datGrouperEntryDTO;

        public SetGameEntityDragStartCmd(DatGrouperEntryDTO datGrouperEntryDTO)
        {
            this.datGrouperEntryDTO = datGrouperEntryDTO;
        }

        protected override void Run()
        {
            Facade.Instance?.MediaMediator?.StartReceiveGameEntityDrop();
            Facade.Instance?.MainWebMediator?.StartReceiveGameEntityDrop(datGrouperEntryDTO);
        }
    }
}
