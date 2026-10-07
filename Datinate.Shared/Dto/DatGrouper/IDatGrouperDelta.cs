using Datinate.Shared.Radio;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Shared.DatGrouper
{
    public interface IDatGrouperDelta
    {
        DELTA_NATURE_ENUM DeltaNatureEnum { get; }
        IReadOnlyDictionary<IGameFamily, IGameFamily> ReplacementReferences { get; }
        IReadOnlyList<IGameFamily> CuratedFamiliesToAdd { get; }
        IReadOnlyList<IGameFamily> CuratedFamiliesToRemove { get; }
        IReadOnlyList<IGameFamily> AutoFamiliesToAdd { get; }
        IReadOnlyList<IGameFamily> AutoFamiliesToRemove { get; }
        IReadOnlySet20<IGamePart> AllCuratedAutoParts { get; }
        IReadOnlySet20<IGameEntity> AutoAffectedEntities { get; }
        IReadOnlySet20<IGameEntity> CuratedAffectedEntities { get; }
        int AvailableUndos { get; }
        int AvailableRedos { get; }
    }
}
