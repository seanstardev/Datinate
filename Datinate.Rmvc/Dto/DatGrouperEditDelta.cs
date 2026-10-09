using Datinate.Shared;
using Datinate.Shared.DatGrouper;
using Datinate.Shared.Radio;
using static Datinate.Rmvc.Delegate.DatGrouper.CurationOverlay;
using static Datinate.Shared.DatinateEnums;

namespace Datinate.Rmvc.Dto
{
    internal class DatGrouperEditDelta : IDatGrouperDelta
    {
        public DatGrouperEditDelta(
            DELTA_NATURE_ENUM deltaNatureEnum,
            IReadOnlyDictionary<IGameFamily, IGameFamily> replacementReferences,
            IReadOnlyList<IGameFamily> curatedFamiliesToAdd,
            IReadOnlyList<IGameFamily> curatedFamiliesToRemove,
            IReadOnlyList<IGameFamily> autoFamiliesToAdd,
            IReadOnlyList<IGameFamily> autoFamiliesToRemove,
            IReadOnlySet20<IGamePart> allCuratedAutoParts,
            int availableUndos,
            int availableRedos,
            IReadOnlySet20<IGameEntity> autoAffectedEntities,
            IReadOnlySet20<IGameEntity> curatedAffectedEntities)
        {
            DeltaNatureEnum = deltaNatureEnum;
            ReplacementReferences = replacementReferences;
            CuratedFamiliesToAdd = curatedFamiliesToAdd;
            CuratedFamiliesToRemove = curatedFamiliesToRemove;
            AutoFamiliesToAdd = autoFamiliesToAdd;
            AutoFamiliesToRemove = autoFamiliesToRemove;
            AllCuratedAutoParts = allCuratedAutoParts;
            AvailableUndos = availableUndos;
            AvailableRedos = availableRedos;
            AutoAffectedEntities = autoAffectedEntities;
            CuratedAffectedEntities = curatedAffectedEntities;
        }

        public DELTA_NATURE_ENUM DeltaNatureEnum { get; }
        public IReadOnlyDictionary<IGameFamily, IGameFamily> ReplacementReferences { get; }
        public IReadOnlyList<IGameFamily> CuratedFamiliesToAdd { get; }
        public IReadOnlyList<IGameFamily> CuratedFamiliesToRemove { get; }
        public IReadOnlyList<IGameFamily> AutoFamiliesToAdd { get; }
        public IReadOnlyList<IGameFamily> AutoFamiliesToRemove { get; }
        public IReadOnlySet20<IGamePart> AllCuratedAutoParts { get; }
        public int AvailableUndos { get; }
        public int AvailableRedos { get; }
        public IReadOnlySet20<IGameEntity> AutoAffectedEntities { get; }
        public IReadOnlySet20<IGameEntity> CuratedAffectedEntities { get; }
        internal PlanEdit[] PlanEdits { get; set; } = Array.Empty<PlanEdit>();
        internal AutoVisEdit[] AutoVisEdits { get; set; } = Array.Empty<AutoVisEdit>();
        internal PartExcludeEdit[] PartExcludeEdits { get; set; } = Array.Empty<PartExcludeEdit>();
    }
}