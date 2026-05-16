using RadioLibCore.RadioDat;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public class DatGrouperEditRequestDTO
    {
        public DatGrouperEditRequestDTO(EDIT_ACTION_ENUM editActionEnum)
        {
            EditActionEnum = editActionEnum;
        }
        public enum EDIT_ACTION_ENUM
        {
            NOT_SET,
            
            FamilyReset,
            FamilyAdd,
            FamilyMergeAsMain,
            FamilyMergeAsSub,
            
            GameReset,
            GameMoveBefore,
            GameMoveAfter,
            GameMoveToBottom,
            GameMoveToTop,

            PartReset,
            PartMoveBefore,
            PartMoveAfter,

            PartSetAsInclude,
            PartSetAsExclude,
        }
        public EDIT_ACTION_ENUM EditActionEnum { get; }

        public IGameFamily? SourceFamily = null;
        public IGameFamily? TargetFamily = null;

        public IGame? SourceGame = null;
        public IGame? TargetGame = null;
        
        public IGamePart? SourcePart = null;
        public IGamePart? TargetPart = null;
    }
}
