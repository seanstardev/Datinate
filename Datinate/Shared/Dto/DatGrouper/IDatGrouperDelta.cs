using RadioLibCore.RadioDat;
using static app.datinate.DatGrouperEditDelta;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public interface IDatGrouperDelta
    {
        DELTA_NATURE_ENUM DeltaNatureEnum { get; }
        IReadOnlyDictionary<IGameFamily, IGameFamily> ReplacementReferences { get; }
        IReadOnlyList<IGameFamily> CuratedFamiliesToAdd { get; }
        IReadOnlyList<IGameFamily> CuratedFamiliesToRemove { get; }
        IReadOnlyList<IGameFamily> AutoFamiliesToAdd { get; }
        IReadOnlyList<IGameFamily> AutoFamiliesToRemove { get; }
        IReadOnlySet<IGamePart> AllCuratedAutoParts { get; }
        IReadOnlySet<IGameEntity> AutoAffectedEntities { get; }
        IReadOnlySet<IGameEntity> CuratedAffectedEntities { get; }
        int AvailableUndos { get; }
        int AvailableRedos { get; }
    }
}
