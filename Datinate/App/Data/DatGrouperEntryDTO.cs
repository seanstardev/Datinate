using RadioLibCore.RadioDat;

namespace datinate.app
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
