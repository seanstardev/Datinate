using Datinate.Shared.Radio;

namespace Datinate.Shared.DatGrouper
{ 
    public class DatGrouperEntryDTO
    {
        public IGameEntity Entity { get; }
        public bool IsFromAuto { get; }
        public DatGrouperEntryDTO(IGameEntity entity, bool isFromAuto) 
        {
            Entity = entity;
            IsFromAuto = isFromAuto;
        }
    }
}
