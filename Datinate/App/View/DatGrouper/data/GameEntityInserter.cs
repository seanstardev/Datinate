using RadioLibCore.RadioDat;

namespace datinate.app
{
    public sealed class GameEntityInserter : IGameEntityProxy
    {
        public static readonly GameEntityInserter Instance = new();
        private GameEntityInserter()
        {

        }
    }

    public interface IGameEntityProxy : IGameEntity;
    public interface IGameEntityDataPacket : IGameEntity;
}
