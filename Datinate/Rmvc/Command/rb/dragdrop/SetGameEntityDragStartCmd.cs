using datinate.app;
using RMVC;

namespace com.RADIO.Datinate.RMVC
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
