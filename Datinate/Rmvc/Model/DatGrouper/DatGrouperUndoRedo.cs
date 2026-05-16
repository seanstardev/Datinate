using RadioLibCore.RadioDat;

namespace app.datinate
{
    public partial class CurationOverlay
    {
        internal readonly record struct AutoVisEdit(AutoFamilyId Id, bool BeforeVisible, bool AfterVisible);
        internal readonly record struct PlanEdit(Guid PlanId, PlanState Before, PlanState After);
        internal readonly record struct PartExcludeEdit(AutoPartId PartId, bool BeforeExclude, bool AfterExclude);

        internal readonly record struct PlanState(
            bool Exists,
            int Index,
            Guid PlanId,
            CuratedFamilyPlanSnap Snap);

        internal sealed record CuratedFamilyPlanSnap(
            Guid Id,
            string Name,
            AutoFamilyId? OriginAutoFamilyId,
            CuratedGamePlanSnap[] Games);

        internal sealed record CuratedGamePlanSnap(
            Guid Id,
            string Name,
            AutoGameId? AutoGameId,
            AutoPartId[] Parts);


        private readonly List<DatGrouperEditDelta> undos = [];
        private readonly List<DatGrouperEditDelta> redos = [];
        private int undosCount => undos.Count;
        private int redosCount => redos.Count;

        public DatGrouperEditDelta? ApplyUndo()
        {
            if (undos.Count == 0)
                return null;

            var action = undos[^1];
            undos.RemoveAt(undos.Count - 1);
            redos.Add(action);

            var applied = ApplyHistoryInternal(action, forward: false, availableUndos: undos.Count, availableRedos: redos.Count);
            if (applied is null)
            {
                redos.RemoveAt(redos.Count - 1);
                undos.Add(action);
                return null;
            }

            return applied;
        }

        public DatGrouperEditDelta? ApplyRedo()
        {
            if (redos.Count == 0)
                return null;

            var action = redos[^1];
            redos.RemoveAt(redos.Count - 1);
            undos.Add(action);

            var applied = ApplyHistoryInternal(action, forward: true, availableUndos: undos.Count, availableRedos: redos.Count);
            if (applied is null)
            {
                undos.RemoveAt(undos.Count - 1);
                redos.Add(action);
                return null;
            }

            return applied;
        }

        private static CuratedGamePlanSnap Snap(CuratedGamePlan gp)
        {
            return new CuratedGamePlanSnap(
                gp.Id,
                gp.Name,
                gp.AutoGameId,
                gp.Parts.Count == 0 ? Array.Empty<AutoPartId>() : gp.Parts.ToArray());
        }

        private static CuratedFamilyPlanSnap Snap(CuratedFamilyPlan fp)
        {
            if (fp.Games.Count == 0)
                return new CuratedFamilyPlanSnap(fp.Id, fp.Name, fp.OriginAutoFamilyId, Array.Empty<CuratedGamePlanSnap>());

            var games = new CuratedGamePlanSnap[fp.Games.Count];
            for (int i = 0; i < fp.Games.Count; i++)
                games[i] = Snap(fp.Games[i]);

            return new CuratedFamilyPlanSnap(fp.Id, fp.Name, fp.OriginAutoFamilyId, games);
        }

        private static CuratedFamilyPlanSnap EmptySnap(Guid id)
            => new CuratedFamilyPlanSnap(id, string.Empty, null, Array.Empty<CuratedGamePlanSnap>());

        private PlanState CapturePlanState(Guid planId)
        {
            var idx = CuratedFamiliesInternal.FindIndex(f => f.Id == planId);
            if (idx < 0)
                return new PlanState(false, -1, planId, EmptySnap(planId));

            var plan = CuratedFamiliesInternal[idx];
            return new PlanState(true, idx, plan.Id, Snap(plan));
        }

        private static void CollectParts(CuratedFamilyPlanSnap snap, HashSet<AutoPartId> into)
        {
            var games = snap.Games;
            for (int gi = 0; gi < games.Length; gi++)
            {
                var parts = games[gi].Parts;
                for (int pi = 0; pi < parts.Length; pi++)
                    _ = into.Add(parts[pi]);
            }
        }

        private static CuratedFamilyPlan BuildPlanFromSnap(CuratedFamilyPlanSnap snap)
        {
            var plan = new CuratedFamilyPlan(snap.Id, snap.Name, snap.OriginAutoFamilyId);

            var games = snap.Games;
            for (int gi = 0; gi < games.Length; gi++)
            {
                var gs = games[gi];
                if (!gs.AutoGameId.HasValue)
                    throw new InvalidOperationException($"Cannot restore game plan '{gs.Id}' because AutoGameId is null.");

                var gp = new CuratedGamePlan(gs.Id, gs.Name, gs.AutoGameId.Value);

                if (gs.Parts.Length > 0)
                    gp.Parts.AddRange(gs.Parts);

                plan.Games.Add(gp);
            }

            return plan;
        }

        private void ApplyPlanEditsInternal(PlanEdit[] edits, bool forward)
        {
            if (edits.Length == 0)
                return;

            var touched = new HashSet<Guid>();
            for (int i = 0; i < edits.Length; i++)
                _ = touched.Add(edits[i].PlanId);

            var baseList = new List<CuratedFamilyPlan>(CuratedFamiliesInternal.Count);
            for (int i = 0; i < CuratedFamiliesInternal.Count; i++)
            {
                var p = CuratedFamiliesInternal[i];
                if (!touched.Contains(p.Id))
                    baseList.Add(p);
            }

            var toStates = new List<PlanState>(edits.Length);
            for (int i = 0; i < edits.Length; i++)
            {
                var st = forward ? edits[i].After : edits[i].Before;
                if (st.Exists)
                    toStates.Add(st);
            }

            toStates.Sort((a, b) => a.Index.CompareTo(b.Index));

            for (int i = 0; i < toStates.Count; i++)
            {
                var st = toStates[i];
                var plan = BuildPlanFromSnap(st.Snap);

                var insert = st.Index;
                if (insert < 0) insert = 0;
                if (insert > baseList.Count) insert = baseList.Count;

                baseList.Insert(insert, plan);
            }

            CuratedFamiliesInternal.Clear();
            CuratedFamiliesInternal.AddRange(baseList);

            MarkCuratedIndexDirty();
        }

        private void ApplyPartExcludeEditsInternal(PartExcludeEdit[] edits, bool forward)
        {
            for (int i = 0; i < edits.Length; i++)
            {
                var edit = edits[i];

                if (!auto.TryResolve(edit.PartId, out var part))
                    continue;

                part.Exclude = forward ? edit.AfterExclude : edit.BeforeExclude;
            }
        }

        private void AddHistoryAffectedPartEntities(
            HashSet<IGameEntity> curatedAffected,
            IReadOnlyCollection<PartExcludeEdit> partExcludeEdits)
        {
            foreach (var edit in partExcludeEdits)
            {
                if (curatedPartCloneByPartId.TryGetValue(edit.PartId, out var mappedPart) &&
                    mappedPart is IGameEntity mappedEntity)
                {
                    _ = curatedAffected.Add(mappedEntity);
                }
            }
        }

        private DatGrouperEditDelta? ApplyHistoryInternal(DatGrouperEditDelta action, bool forward, int availableUndos, int availableRedos)
        {
            EnsureCuratedIndex();

            var planEdits = action.PlanEdits;
            var autoEdits = action.AutoVisEdits;
            var partExcludeEdits = action.PartExcludeEdits;

            var touchedPlanIds = new HashSet<Guid>();
            for (int i = 0; i < planEdits.Length; i++)
                _ = touchedPlanIds.Add(planEdits[i].PlanId);

            foreach (var edit in partExcludeEdits)
            {
                if (curatedLocByPartId.TryGetValue(edit.PartId, out var loc))
                    _ = touchedPlanIds.Add(loc.FamilyPlanId);
            }

            var oldCuratedCloneByPlanId = new Dictionary<Guid, IGameFamily>();
            var oldOriginAutoFamilyIdByPlanId = new Dictionary<Guid, AutoFamilyId?>();

            foreach (var planId in touchedPlanIds)
            {
                if (curatedFamilyCloneByPlanId.TryGetValue(planId, out var oldClone))
                    oldCuratedCloneByPlanId[planId] = oldClone;

                if (TryGetFamilyPlan(planId, out var plan))
                    oldOriginAutoFamilyIdByPlanId[planId] = plan.OriginAutoFamilyId;
            }

            var oldAutoFamilyById = new Dictionary<AutoFamilyId, IGameFamily>();
            for (int i = 0; i < autoEdits.Length; i++)
            {
                var id = autoEdits[i].Id;
                if (auto.TryResolve(id, out var fam) && fam is not null)
                    oldAutoFamilyById[id] = fam;
            }

            var fromParts = new HashSet<AutoPartId>();
            var toParts = new HashSet<AutoPartId>();

            for (int i = 0; i < planEdits.Length; i++)
            {
                var pe = planEdits[i];
                var from = forward ? pe.Before : pe.After;
                var to = forward ? pe.After : pe.Before;

                if (from.Exists)
                    CollectParts(from.Snap, fromParts);

                if (to.Exists)
                    CollectParts(to.Snap, toParts);
            }

            foreach (var pid in fromParts)
            {
                if (!toParts.Contains(pid))
                    _ = RemoveCuratedPart(pid);
            }

            foreach (var pid in toParts)
            {
                if (!fromParts.Contains(pid))
                    _ = AddCuratedPart(pid);
            }

            ApplyPlanEditsInternal(planEdits, forward);
            ApplyPartExcludeEditsInternal(partExcludeEdits, forward);

            EnsureCuratedIndex();

            var newCloneByPlanId = new Dictionary<Guid, IGameFamily>();
            foreach (var planId in touchedPlanIds)
            {
                if (!curatedLocByFamilyPlanId.ContainsKey(planId))
                    continue;

                var newClone = materialiseCuratedFamilyCloneByPlanId(planId);
                if (newClone is null)
                    return null;

                newCloneByPlanId[planId] = newClone;
            }

            var survivingTouchedPlanIds = newCloneByPlanId.Keys.ToArray();
            IGameFamily? singleSurvivorClone =
                survivingTouchedPlanIds.Length == 1 ? newCloneByPlanId[survivingTouchedPlanIds[0]] : null;

            var curatedReplace = new Dictionary<IGameFamily, IGameFamily>(ReferenceEqualityComparer.Instance);
            var curatedAdd = new List<IGameFamily>();
            var curatedRemove = new List<IGameFamily>();

            foreach (var planId in touchedPlanIds)
            {
                var existsNow = newCloneByPlanId.TryGetValue(planId, out var newClone);
                var hadOld = oldCuratedCloneByPlanId.TryGetValue(planId, out var oldClone);

                if (!existsNow)
                {
                    if (hadOld)
                    {
                        curatedRemove.Add(oldClone!);

                        if (singleSurvivorClone is not null)
                        {
                            curatedReplace[oldClone!] = singleSurvivorClone;
                        }
                        else if (oldOriginAutoFamilyIdByPlanId.TryGetValue(planId, out var origin) &&
                                    origin.HasValue &&
                                    auto.TryResolve(origin.Value, out var autoFam) &&
                                    autoFam is not null)
                        {
                            curatedReplace[oldClone!] = autoFam;
                        }
                    }

                    continue;
                }

                if (!hadOld)
                {
                    curatedAdd.Add(newClone!);
                }
                else
                {
                    curatedRemove.Add(oldClone!);
                    curatedAdd.Add(newClone!);
                    curatedReplace[oldClone!] = newClone!;
                }
            }

            var autoAdd = new List<IGameFamily>();
            var autoRemove = new List<IGameFamily>();

            for (int i = 0; i < autoEdits.Length; i++)
            {
                var e = autoEdits[i];

                var wasVisible = forward ? e.BeforeVisible : e.AfterVisible;
                var isVisible = forward ? e.AfterVisible : e.BeforeVisible;

                if (!auto.TryResolve(e.Id, out var fam) || fam is null)
                    continue;

                if (wasVisible && !isVisible)
                {
                    autoRemove.Add(fam);

                    if (oldAutoFamilyById.TryGetValue(e.Id, out var oldAutoFam))
                    {
                        IGameFamily? newRef = null;

                        if (curatedFamilyPlanIdByOriginAutoFamilyId.TryGetValue(e.Id, out var planIdNow) &&
                            newCloneByPlanId.TryGetValue(planIdNow, out var planCloneNow))
                        {
                            newRef = planCloneNow;
                        }
                        else if (singleSurvivorClone is not null)
                        {
                            newRef = singleSurvivorClone;
                        }

                        if (newRef is not null)
                            curatedReplace[oldAutoFam] = newRef;
                    }

                    continue;
                }

                if (!wasVisible && isVisible)
                {
                    autoAdd.Add(fam);
                    continue;
                }

                if (wasVisible && isVisible)
                {
                    continue;
                }
            }

            PurgeOrphanedMaterialisationCaches();
#if DEBUG
            ValidateStateOrThrow();
#endif

            var autoAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
            AddHistoryAffectedAutoEntities(autoAffected, autoEdits);

            foreach (var entity in action.AutoAffectedEntities)
                _ = autoAffected.Add(entity);

            var curatedAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
            AddHistoryAffectedCuratedEntities(
                curatedAffected,
                planEdits,
                forward,
                oldCuratedCloneByPlanId,
                newCloneByPlanId);

            AddHistoryAffectedPartEntities(curatedAffected, partExcludeEdits);

            foreach (var entity in action.CuratedAffectedEntities)
                _ = curatedAffected.Add(entity);

            return new DatGrouperEditDelta(
                DatGrouperEditDelta.DELTA_NATURE_ENUM.UndoRedo,
                curatedReplace,
                curatedAdd,
                curatedRemove,
                autoAdd,
                autoRemove,
                curatedAutoParts,
                availableUndos,
                availableRedos,
                autoAffected,
                curatedAffected)
            {
                PlanEdits = action.PlanEdits,
                AutoVisEdits = action.AutoVisEdits,
                PartExcludeEdits = action.PartExcludeEdits
            };
        }

        private static Dictionary<Guid, CuratedGamePlanSnap> BuildGameSnapById(CuratedFamilyPlanSnap snap)
        {
            var dict = new Dictionary<Guid, CuratedGamePlanSnap>(snap.Games.Length);
            for (int i = 0; i < snap.Games.Length; i++)
                dict[snap.Games[i].Id] = snap.Games[i];

            return dict;
        }

        private static HashSet<AutoPartId> BuildPartSet(AutoPartId[] parts)
        {
            var set = new HashSet<AutoPartId>(parts.Length);
            for (int i = 0; i < parts.Length; i++)
                _ = set.Add(parts[i]);

            return set;
        }

        private void AddHistoryAffectedCuratedEntities(
            HashSet<IGameEntity> curatedAffected,
            PlanEdit[] planEdits,
            bool forward,
            IReadOnlyDictionary<Guid, IGameFamily> oldCuratedCloneByPlanId,
            IReadOnlyDictionary<Guid, IGameFamily> newCloneByPlanId)
        {
            for (int i = 0; i < planEdits.Length; i++)
            {
                var edit = planEdits[i];
                var from = forward ? edit.Before : edit.After;
                var to = forward ? edit.After : edit.Before;

                if (from.Exists &&
                    oldCuratedCloneByPlanId.TryGetValue(edit.PlanId, out var oldFamily) &&
                    oldFamily is IGameEntity oldFamilyEntity)
                {
                    _ = curatedAffected.Add(oldFamilyEntity);
                }

                if (to.Exists &&
                    newCloneByPlanId.TryGetValue(edit.PlanId, out var newFamily) &&
                    newFamily is IGameEntity newFamilyEntity)
                {
                    _ = curatedAffected.Add(newFamilyEntity);
                }

                if (!to.Exists || !newCloneByPlanId.TryGetValue(edit.PlanId, out var newFamilyClone))
                    continue;

                var newGames = newFamilyClone.GetAllGames();
                var newGameById = new Dictionary<Guid, IGame>(newGames.Length);
                for (int gi = 0; gi < newGames.Length; gi++)
                {
                    var gameClone = newGames[gi];
                    if (curatedGamePlanByGameRef.TryGetValue(gameClone, out var info))
                        newGameById[info.GamePlanId] = gameClone;
                }

                var fromGamesById = from.Exists
                    ? BuildGameSnapById(from.Snap)
                    : new Dictionary<Guid, CuratedGamePlanSnap>();

                var toGamesById = to.Exists
                    ? BuildGameSnapById(to.Snap)
                    : new Dictionary<Guid, CuratedGamePlanSnap>();

                foreach (var kv in toGamesById)
                {
                    var gamePlanId = kv.Key;
                    var toGame = kv.Value;

                    if (!newGameById.TryGetValue(gamePlanId, out var newGameClone))
                        continue;

                    if (!fromGamesById.TryGetValue(gamePlanId, out var fromGame))
                    {
                        AddGameAndParts(curatedAffected, newGameClone);
                        continue;
                    }

                    var fromPartSet = BuildPartSet(fromGame.Parts);
                    var toPartIds = toGame.Parts;
                    var gameAdded = false;

                    for (int pi = 0; pi < toPartIds.Length; pi++)
                    {
                        var pid = toPartIds[pi];
                        if (fromPartSet.Contains(pid))
                            continue;

                        if (!gameAdded)
                        {
                            _ = curatedAffected.Add(newGameClone);
                            gameAdded = true;
                        }

                        if (curatedPartCloneByPartId.TryGetValue(pid, out var partClone) &&
                            partClone is IGameEntity partEntity)
                        {
                            _ = curatedAffected.Add(partEntity);
                        }
                    }
                }
            }
        }
        private void AddHistoryAffectedAutoEntities(
            HashSet<IGameEntity> autoAffected,
            AutoVisEdit[] autoEdits)
        {
            for (int i = 0; i < autoEdits.Length; i++)
            {
                var id = autoEdits[i].Id;

                if (auto.TryResolve(id, out var family))
                    AddFamilyAndDescendants(autoAffected, family);
            }
        }
    }
}