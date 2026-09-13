using RadioLibCore.RadioDat;

namespace app.datinate
{
    public partial class CurationOverlay
    {
        public IReadOnlySet<IGameFamily> GetResetFamilyMediaAssociationRisks(
            IGameFamily family)
        {
            var result =
                new HashSet<IGameFamily>(
                    ReferenceEqualityComparer.Instance);

            if (!TryResolveCuratedFamilyPlanId(
                    family,
                    out var familyPlanId))
            {
                return result;
            }

            if (!TryGetFamilyPlan(
                    familyPlanId,
                    out var familyPlan))
            {
                return result;
            }

            // A complete original auto family has somewhere unambiguous
            // to send the media association when reset.
            if (familyPlan.OriginAutoFamilyId.HasValue)
                return result;

            if (curatedFamilyCloneByPlanId.TryGetValue(
                    familyPlanId,
                    out var familyClone))
            {
                result.Add(familyClone);
            }

            return result;
        }

        public IReadOnlySet<IGameFamily> GetResetGameMediaAssociationRisks(
            IGame game)
        {
            var result =
                new HashSet<IGameFamily>(
                    ReferenceEqualityComparer.Instance);

            if (!TryResolveCuratedGameLoc(
                    game,
                    out var loc))
            {
                return result;
            }

            if (!TryGetFamilyPlan(
                    loc.FamilyPlanId,
                    out var familyPlan))
            {
                return result;
            }

            var gamePlan =
                familyPlan.Games.FirstOrDefault(
                    g => g.Id == loc.GamePlanId);

            if (gamePlan is null)
                return result;

            // The family survives this reset.
            if (familyPlan.Games.Count > 1)
                return result;

            // The family disappears, but it has an unambiguous auto-family
            // replacement.
            if (familyPlan.OriginAutoFamilyId.HasValue)
                return result;

            if (curatedFamilyCloneByPlanId.TryGetValue(
                    loc.FamilyPlanId,
                    out var familyClone))
            {
                result.Add(familyClone);
            }

            return result;
        }

        public IReadOnlySet<IGameFamily> GetResetPartMediaAssociationRisks(
            IGamePart part)
        {
            var result =
                new HashSet<IGameFamily>(
                    ReferenceEqualityComparer.Instance);

            if (!TryResolveCuratedPartLoc(
                    part,
                    out _,
                    out var loc))
            {
                return result;
            }

            if (!TryGetFamilyPlan(
                    loc.FamilyPlanId,
                    out var familyPlan))
            {
                return result;
            }

            var gamePlan =
                familyPlan.Games.FirstOrDefault(
                    g => g.Id == loc.GamePlanId);

            if (gamePlan is null)
                return result;

            // The family survives this reset.
            if (familyPlan.Games.Count > 1 ||
                gamePlan.Parts.Count > 1)
            {
                return result;
            }

            if (familyPlan.OriginAutoFamilyId.HasValue)
                return result;

            if (curatedFamilyCloneByPlanId.TryGetValue(
                    loc.FamilyPlanId,
                    out var familyClone))
            {
                result.Add(familyClone);
            }

            return result;
        }

        public IReadOnlySet<IGameFamily> GetUndoMediaAssociationRisks()
        {
            if (undos.Count == 0)
            {
                return new HashSet<IGameFamily>(
                    ReferenceEqualityComparer.Instance);
            }

            return GetHistoryMediaAssociationRisks(
                undos[^1],
                forward: false);
        }

        public IReadOnlySet<IGameFamily> GetRedoMediaAssociationRisks()
        {
            if (redos.Count == 0)
            {
                return new HashSet<IGameFamily>(
                    ReferenceEqualityComparer.Instance);
            }

            return GetHistoryMediaAssociationRisks(
                redos[^1],
                forward: true);
        }

        public bool WouldResetFamilyLoseMediaAssociations(
            IGameFamily family)
        {
            return GetResetFamilyMediaAssociationRisks(
                family).Count > 0;
        }

        public bool WouldResetGameLoseMediaAssociations(
            IGame game)
        {
            return GetResetGameMediaAssociationRisks(
                game).Count > 0;
        }

        public bool WouldResetPartLoseMediaAssociations(
            IGamePart part)
        {
            return GetResetPartMediaAssociationRisks(
                part).Count > 0;
        }

        public bool WouldUndoLoseMediaAssociations()
        {
            return GetUndoMediaAssociationRisks().Count > 0;
        }

        public bool WouldRedoLoseMediaAssociations()
        {
            return GetRedoMediaAssociationRisks().Count > 0;
        }

        private IReadOnlySet<IGameFamily> GetHistoryMediaAssociationRisks(
            DatGrouperEditDelta action,
            bool forward)
        {
            EnsureCuratedIndex();

            var result =
                new HashSet<IGameFamily>(
                    ReferenceEqualityComparer.Instance);

            var planEditById =
                new Dictionary<Guid, PlanEdit>();

            for (int i = 0;
                 i < action.PlanEdits.Length;
                 i++)
            {
                var edit =
                    action.PlanEdits[i];

                planEditById[edit.PlanId] =
                    edit;
            }

            var touchedPlanIds =
                new HashSet<Guid>(
                    planEditById.Keys);

            for (int i = 0;
                 i < action.PartExcludeEdits.Length;
                 i++)
            {
                var edit =
                    action.PartExcludeEdits[i];

                if (curatedLocByPartId.TryGetValue(
                        edit.PartId,
                        out var loc))
                {
                    touchedPlanIds.Add(
                        loc.FamilyPlanId);
                }
            }

            bool TryGetTargetPlanInfo(
                Guid planId,
                out bool exists,
                out AutoFamilyId? origin)
            {
                if (planEditById.TryGetValue(
                        planId,
                        out var edit))
                {
                    var target =
                        forward
                            ? edit.After
                            : edit.Before;

                    exists =
                        target.Exists;

                    origin =
                        target.Exists
                            ? target.Snap.OriginAutoFamilyId
                            : null;

                    return true;
                }

                if (TryGetFamilyPlan(
                        planId,
                        out var currentPlan))
                {
                    exists = true;
                    origin =
                        currentPlan.OriginAutoFamilyId;

                    return true;
                }

                exists = false;
                origin = null;

                return false;
            }

            var survivingTouchedPlanCount = 0;

            foreach (var planId in touchedPlanIds)
            {
                if (TryGetTargetPlanInfo(
                        planId,
                        out var exists,
                        out _) &&
                    exists)
                {
                    survivingTouchedPlanCount++;
                }
            }

            var hasSingleSurvivor =
                survivingTouchedPlanCount == 1;

            // Curated families which disappear.
            for (int i = 0;
                 i < action.PlanEdits.Length;
                 i++)
            {
                var edit =
                    action.PlanEdits[i];

                var from =
                    forward
                        ? edit.Before
                        : edit.After;

                var to =
                    forward
                        ? edit.After
                        : edit.Before;

                if (!from.Exists ||
                    to.Exists)
                {
                    continue;
                }

                if (hasSingleSurvivor)
                    continue;

                if (from.Snap.OriginAutoFamilyId.HasValue)
                    continue;

                if (curatedFamilyCloneByPlanId.TryGetValue(
                        edit.PlanId,
                        out var familyClone))
                {
                    result.Add(
                        familyClone);
                }
            }

            // Auto families which disappear from queued.
            for (int i = 0;
                 i < action.AutoVisEdits.Length;
                 i++)
            {
                var edit =
                    action.AutoVisEdits[i];

                var wasVisible =
                    forward
                        ? edit.BeforeVisible
                        : edit.AfterVisible;

                var isVisible =
                    forward
                        ? edit.AfterVisible
                        : edit.BeforeVisible;

                if (!wasVisible ||
                    isVisible)
                {
                    continue;
                }

                var hasOriginOwner = false;

                foreach (var planId in touchedPlanIds)
                {
                    if (!TryGetTargetPlanInfo(
                            planId,
                            out var exists,
                            out var origin))
                    {
                        continue;
                    }

                    if (exists &&
                        origin.HasValue &&
                        origin.Value.Equals(edit.Id))
                    {
                        hasOriginOwner = true;
                        break;
                    }
                }

                if (hasOriginOwner ||
                    hasSingleSurvivor)
                {
                    continue;
                }

                if (auto.TryResolve(
                        edit.Id,
                        out var autoFamily) &&
                    autoFamily is not null)
                {
                    result.Add(
                        autoFamily);
                }
            }

            return result;
        }
    }
}