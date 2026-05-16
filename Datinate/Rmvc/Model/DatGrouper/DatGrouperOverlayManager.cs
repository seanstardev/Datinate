using com.RADIO.Datinate.RMVC;
using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using System.Diagnostics;
using static app.datinate.CurationOverlay;
using static app.datinate.DatGrouperEditDelta;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace app.datinate
{
    public class DatGrouperEditDelta : IDatGrouperDelta
    {
        public enum DELTA_NATURE_ENUM
        {
            NOT_SET,
            Import,
            UndoRedo,
            PartIncludeExclude,
            Update,
        }

        public DatGrouperEditDelta(
            DELTA_NATURE_ENUM deltaNatureEnum,
            IReadOnlyDictionary<IGameFamily, IGameFamily> replacementReferences,
            IReadOnlyList<IGameFamily> curatedFamiliesToAdd,
            IReadOnlyList<IGameFamily> curatedFamiliesToRemove,
            IReadOnlyList<IGameFamily> autoFamiliesToAdd,
            IReadOnlyList<IGameFamily> autoFamiliesToRemove,
            IReadOnlySet<IGamePart> allCuratedAutoParts,
            int availableUndos,
            int availableRedos,
            IReadOnlySet<IGameEntity> autoAffectedEntities,
            IReadOnlySet<IGameEntity> curatedAffectedEntities)
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
        public IReadOnlySet<IGamePart> AllCuratedAutoParts { get; }
        public int AvailableUndos { get; }
        public int AvailableRedos { get; }
        public IReadOnlySet<IGameEntity> AutoAffectedEntities { get; }
        public IReadOnlySet<IGameEntity> CuratedAffectedEntities { get; }
        internal PlanEdit[] PlanEdits { get; init; } = Array.Empty<PlanEdit>();
        internal AutoVisEdit[] AutoVisEdits { get; init; } = Array.Empty<AutoVisEdit>();
        internal PartExcludeEdit[] PartExcludeEdits { get; init; } = Array.Empty<PartExcludeEdit>();
    }

    public partial class CurationOverlay
    {
        internal readonly record struct AutoFamilyId(int FamilyIndex);
        internal readonly record struct AutoGameId(int FamilyIndex, int GameIndex);
        internal readonly record struct AutoPartId(int FamilyIndex, int GameIndex, int PartIndex);

        private readonly AutoRegistry auto;
        private readonly Func<Guid, IGameFamily?> materialiseCuratedFamilyCloneByPlanId;

        private readonly Dictionary<Guid, IGameFamily> curatedFamilyCloneByPlanId = new();
        private readonly Dictionary<Guid, IGame> curatedGameCloneByGamePlanId = new();
        private readonly Dictionary<AutoPartId, IGamePart> curatedPartCloneByPartId = new();

        private readonly HashSet<IGamePart> curatedAutoPartsInternal = new(ReferenceEqualityComparer.Instance);
        private IReadOnlySet<IGamePart> curatedAutoParts => curatedAutoPartsInternal;

        private readonly int[] curatedPartCountByAutoFamilyIndex;
        private readonly Dictionary<AutoPartId, bool> defaultExcludeByPartId = new();

        private bool curationImportPerformed = false;

        private readonly HashSet<AutoPartId> CuratedPartsInternal = new();
        private readonly List<CuratedFamilyPlan> CuratedFamiliesInternal = new();

        private readonly Dictionary<AutoPartId, CuratedPartLoc> curatedLocByPartId = new();
        private readonly Dictionary<AutoGameId, CuratedGameLoc> curatedLocByAutoGameId = new();
        private readonly Dictionary<Guid, CuratedFamilyLoc> curatedLocByFamilyPlanId = new();
        private readonly Dictionary<Guid, CuratedGameLoc> curatedLocByGamePlanId = new();
        private readonly Dictionary<AutoFamilyId, Guid> curatedFamilyPlanIdByOriginAutoFamilyId = new();

        private bool curatedIndexDirty = true;

        private readonly Dictionary<IGameFamily, Guid> curatedFamilyPlanIdByFamilyRef = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<IGame, (Guid FamilyPlanId, Guid GamePlanId)> curatedGamePlanByGameRef = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<IGamePart, (AutoPartId PartId, Guid FamilyPlanId, Guid GamePlanId)> curatedPartByPartRef = new(ReferenceEqualityComparer.Instance);
        private readonly IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup;
        private readonly IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary;

        private readonly record struct CuratedFamilyLoc(Guid FamilyPlanId, int FamilyIndex);
        private readonly record struct CuratedGameLoc(Guid FamilyPlanId, Guid GamePlanId, int FamilyIndex, int GameIndex);
        private readonly record struct CuratedPartLoc(Guid FamilyPlanId, Guid GamePlanId, int FamilyIndex, int GameIndex, int PartIndex);

        internal CurationOverlay(
            IReadOnlyList<IGameFamily> families,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary)
        {
            auto = new AutoRegistry(families);
            
            curatedPartCountByAutoFamilyIndex = new int[auto.FamilyCount];
            materialiseCuratedFamilyCloneByPlanId = MaterialiseCuratedFamilyCloneByPlanId;

            this.flagFilterSetByGroup = flagFilterSetByGroup;
            this.softwareIdDatGroupEnumDictionary = softwareIdDatGroupEnumDictionary;


            for (int fi = 0; fi < families.Count; fi++)
            {
                var family = families[fi];
                var games = family.GetAllGames();

                for (int gi = 0; gi < games.Length; gi++)
                {
                    var game = games[gi];
                    var parts = game.GetGameParts(false);

                    for (int pi = 0; pi < parts.Length; pi++)
                    {
                        var id = new AutoPartId(fi, gi, pi);
                        defaultExcludeByPartId[id] = parts[pi].Exclude;
                    }
                }
            }
        }
        private bool GetDefaultExclude(AutoPartId partId)
        {
            if (defaultExcludeByPartId.TryGetValue(partId, out var exclude))
                return exclude;

            return auto.TryResolve(partId, out var part) && part.Exclude;
        }

        public IReadOnlyList<IGameFamily> GetCuratedFamiliesAlphaSorted()
        {
            EnsureCuratedIndex();

            var list = new List<(string Key, Guid PlanId, IGameFamily Family)>(CuratedFamiliesInternal.Count);

            for (int i = 0; i < CuratedFamiliesInternal.Count; i++)
            {
                var plan = CuratedFamiliesInternal[i];

                if (!curatedFamilyCloneByPlanId.TryGetValue(plan.Id, out var fam))
                    throw new InvalidOperationException($"Curated family plan is not materialised: {plan.Id}");

                var key = fam.GetFamilyDisplayName();
                if (string.IsNullOrWhiteSpace(key))
                    key = plan.Name;

                list.Add((key, plan.Id, fam));
            }

            list.Sort((a, b) =>
            {
                var c = FamilyNameComparer.Compare(a.Key, b.Key);
                return c != 0 ? c : a.PlanId.CompareTo(b.PlanId);
            });

            var result = new List<IGameFamily>(list.Count);
            for (int i = 0; i < list.Count; i++)
                result.Add(list[i].Family);

            return result;
        }
        public DatGrouperEditDelta? ImportCurated(
            IEnumerable<IGameFamily> importedFamilies,
            out Dictionary<string, IGamePart?> errorReport)
        {
            errorReport = new Dictionary<string, IGamePart?>();
            if (curationImportPerformed) return null;

            curationImportPerformed = true;

            if (CuratedFamiliesInternal.Count != 0 || CuratedPartsInternal.Count != 0)
                throw new InvalidOperationException("ImportCurated must only run on an empty curated overlay.");

            // --- Build a strict auto part lookup: (PartName + PartFingerprint) -> AutoPartId
            // If any key maps to more than 1 AutoPartId, import becomes ambiguous -> fail.
            var autoPartIdByKey = new Dictionary<(string Name, string Fp, string? Source, string? Tag, string? LaunchName), AutoPartId>();

            // Also keep auto family refs by id so we can create ReplacementReferences + auto removals.
            var autoFamilyRefById = new Dictionary<AutoFamilyId, IGameFamily>();

            // Populate from snapshot.
            // NOTE: we iterate the *actual* auto object graph so we can compute fingerprints consistently.
            for (int fi = 0; fi < auto.FamilyCount; fi++)
            {
                var famId = new AutoFamilyId(fi);
                if (!auto.TryResolve(famId, out var autoFamily) || autoFamily is null)
                    continue;

                autoFamilyRefById[famId] = autoFamily;

                var games = autoFamily.GetAllGames();
                for (int gi = 0; gi < games.Length; gi++)
                {
                    var game = games[gi];
                    var parts = game.GetGameParts(false);

                    for (int pi = 0; pi < parts.Length; pi++)
                    {
                        var part = parts[pi];

                        var name = part.GetName();
                        var sourceId = part.GetDirectoryId();
                        var fp = FingerprintHelper.GetFingerprint(part);
                        var key = (name, fp, sourceId, part.Tag, part.LaunchName);

                        var id = new AutoPartId(fi, gi, pi);

                        if (autoPartIdByKey.TryGetValue(key, out var existing))
                        {
                            // Ambiguous: same (name+fp) exists in multiple places in auto.
                            // We have a “perfect match” rule so this is treated as a hard failure.

                            string error = $"Ambiguous part key '{name}'." +
                                $"Already mapped to {existing}, also found {id}.";

                            Debug.WriteLine(error);
                            errorReport[error] = part;

                            continue;
                        }

                        autoPartIdByKey[key] = id;
                    }
                }
            }

            // --- We will build curated plans exactly following the imported structure.
            // While doing that, we track which auto families contribute any parts to each curated family
            // so we can later build ReplacementReferences (auto family ref -> curated clone ref).
            var contributingAutoFamiliesByPlanId = new Dictionary<Guid, HashSet<AutoFamilyId>>();

            // Strict guard: one AutoFamilyId must not be split across multiple imported curated families
            // (otherwise “where does its media go?” becomes ambiguous).
            var ownerPlanIdByAutoFamilyId = new Dictionary<AutoFamilyId, Guid>();

            // Delta payloads
            var replacements = new Dictionary<IGameFamily, IGameFamily>(ReferenceEqualityComparer.Instance);
            var curatedToAdd = new List<IGameFamily>();
            var curatedToRemove = new List<IGameFamily>(); // empty for import
            var autoToAdd = new List<IGameFamily>();       // empty for import
            var autoToRemove = new List<IGameFamily>();
                
            var importedFamiliesByPlanId = new Dictionary<Guid, List<IGameFamily>>();

            // --- Build plans
            foreach (var importedFamily in importedFamilies)
            {
                // Family display name for ordering / UI (not identity)
                var familyName = importedFamily.GetFamilyDisplayName();
                    
                var gamePlans = new List<CuratedGamePlan>();

                // Collect all resolved part ids for this imported family so we can decide OriginAutoFamilyId (optional).
                var familyAutoIds = new List<AutoFamilyId>();

                foreach (var importedGame in importedFamily.GetAllGames())
                {
                    var gameName = importedGame.GetNameWithoutExt();

                    // Resolve each imported part to an AutoPartId (strict: must match exactly).
                    var resolvedPartIdsInOrder = new List<AutoPartId>();
                    var autoGameIdCounts = new Dictionary<AutoGameId, int>();

                    var importedParts = importedGame.GetGameParts(false);
                    for (int i = 0; i < importedParts.Length; i++)
                    {
                        var p = importedParts[i];
                        
                        var name = p.GetName();
                        var sourceId = p.GetDirectoryId();

                        var fp = FingerprintHelper.GetFingerprint(p);
                        var key = (name, fp, sourceId, p.Tag, p.LaunchName);

                        if (!autoPartIdByKey.TryGetValue(key, out var autoPartId))
                        {
                            string error = $"Missing auto match for part '{name}', source='{sourceId}', Tag='{p.Tag ?? "[Null]"}' Launch={p.LaunchName ?? "[Null]"}.";
                            Debug.WriteLine(error);
                            errorReport[error] = p;
                            
                            continue;
                        }

                        resolvedPartIdsInOrder.Add(autoPartId);

                        if (auto.TryResolve(autoPartId, out var matchedAutoPart))
                            matchedAutoPart.Exclude = p.Exclude;

                        var af = new AutoFamilyId(autoPartId.FamilyIndex);
                        familyAutoIds.Add(af);

                        var ag = new AutoGameId(autoPartId.FamilyIndex, autoPartId.GameIndex);
                        autoGameIdCounts.TryGetValue(ag, out var c);
                        autoGameIdCounts[ag] = c + 1;
                    }

                    if (resolvedPartIdsInOrder.Count == 0)
                        continue;

                    // Pick an AutoGameId for the CuratedGamePlan:
                    // - This is mostly metadata used by merge logic and fast matching.
                    // - It cannot represent mixed-origin parts perfectly, so we choose the dominant origin.
                    AutoGameId chosenAutoGameId = default;
                    {
                        var bestCount = -1;
                        foreach (var kv in autoGameIdCounts)
                        {
                            if (kv.Value > bestCount)
                            {
                                bestCount = kv.Value;
                                chosenAutoGameId = kv.Key;
                            }
                        }
                    }

                    var gp = new CuratedGamePlan(gameName, chosenAutoGameId);

                    // Apply parts in imported order (this is the curated truth).
                    for (int i = 0; i < resolvedPartIdsInOrder.Count; i++)
                    {
                        var pid = resolvedPartIdsInOrder[i];

                        // AddCuratedPart maintains:
                        // - CuratedPartsInternal
                        // - CuratedAutoPartsInternal
                        // We also store pid in the plan.
                        gp.Parts.Add(pid);
                        if (!AddCuratedPart(pid))
                        {
                            string error = $"Duplicate part id encountered: {pid}.";
                            // Duplicate part id across curated import (invalid).
                            Debug.WriteLine(error);
                            errorReport[error] = null;
                            continue;
                        }
                    }

                    gamePlans.Add(gp);
                }

                if (gamePlans.Count == 0)
                    continue;

                // Determine OriginAutoFamilyId for the plan:
                // - If all parts came from the same auto family, set it.
                // - If mixed origins (merged curated family), set null.
                AutoFamilyId? origin = null;
                {
                    AutoFamilyId? first = null;
                    for (int i = 0; i < familyAutoIds.Count; i++)
                    {
                        if (!first.HasValue)
                            first = familyAutoIds[i];
                        else if (!first.Value.Equals(familyAutoIds[i]))
                        {
                            first = null;
                            break;
                        }
                    }

                    origin = first;
                }

                CuratedFamilyPlan plan =
                    origin.HasValue
                        ? new CuratedFamilyPlan(familyName, origin.Value)
                        : new CuratedFamilyPlan(familyName);

                if (!importedFamiliesByPlanId.TryGetValue(plan.Id, out var list))
                    importedFamiliesByPlanId[plan.Id] = list = new List<IGameFamily>();

                list.Add(importedFamily);

                for (int i = 0; i < gamePlans.Count; i++)
                    plan.Games.Add(gamePlans[i]);

                // Insert alpha-sorted:
                var insertIndex = GetCuratedFamilyInsertIndex(CuratedFamiliesInternal, plan.Name);
                CuratedFamiliesInternal.Insert(insertIndex, plan);
                MarkCuratedIndexDirty();

                // Track contributing auto families -> this plan (for replacement mapping)
                var contrib = new HashSet<AutoFamilyId>();
                for (int gi = 0; gi < plan.Games.Count; gi++)
                {
                    var gp = plan.Games[gi];
                    for (int pi = 0; pi < gp.Parts.Count; pi++)
                        contrib.Add(new AutoFamilyId(gp.Parts[pi].FamilyIndex));
                }

                contributingAutoFamiliesByPlanId[plan.Id] = contrib;

                foreach (var af in contrib)
                {
                    if (ownerPlanIdByAutoFamilyId.TryGetValue(af, out var existingOwner) && existingOwner != plan.Id)
                    {
                        // Ambiguous for media: one auto family was split across multiple curated families.
                        string error = $"Auto family {af} appears in multiple imported curated families. " +
                            $"Existing owner plan={existingOwner}, new owner plan={plan.Id}.";

                        Debug.WriteLine(error);
                        errorReport[error] = null;

                        continue;
                    }

                    ownerPlanIdByAutoFamilyId[af] = plan.Id;
                }
            }

            // Make all loc maps valid.
            EnsureCuratedIndex();

            // Materialise clones and build delta + replacement references.
            foreach (var plan in CuratedFamiliesInternal)
            {
                var clone = materialiseCuratedFamilyCloneByPlanId(plan.Id);
                if (clone is null)
                    throw new InvalidOperationException($"ImportCurated: failed to materialise plan {plan.Id}.");

                curatedToAdd.Add(clone);

                if (importedFamiliesByPlanId.TryGetValue(plan.Id, out var importedRefs))
                {
                    for (int i = 0; i < importedRefs.Count; i++)
                        replacements[importedRefs[i]] = clone;
                }

                if (!contributingAutoFamiliesByPlanId.TryGetValue(plan.Id, out var contrib))
                    continue;

                foreach (var af in contrib)
                {
                    if (autoFamilyRefById.TryGetValue(af, out var autoFamilyRef))
                        replacements[autoFamilyRef] = clone;

                    // If the auto family is now fully hidden, it disappears from auto UI.
                    if (!IsAutoFamilyVisible(af) && autoFamilyRefById.TryGetValue(af, out var autoRef))
                        autoToRemove.Add(autoRef);
                }
            }

            PurgeOrphanedMaterialisationCaches();
#if DEBUG
            ValidateStateOrThrow();
#endif

            return new DatGrouperEditDelta(
                DatGrouperEditDelta.DELTA_NATURE_ENUM.Import,
                replacements,
                curatedToAdd,
                curatedToRemove,
                autoToAdd,
                autoToRemove,
                curatedAutoParts,
                undosCount, 
                redosCount,
                new HashSet<IGameEntity>(),     // Can always be empty as import changes are not animated.
                new HashSet<IGameEntity>());    // Can always be empty as import changes are not animated.
        }

        private void RestoreDefaultExclude(AutoPartId partId)
        {
            if (!auto.TryResolve(partId, out var part))
                return;

            var defaultExclude = GetDefaultExclude(partId);

            if (part.Exclude != defaultExclude)
                part.Exclude = defaultExclude;
        }
        private IGameFamily? MaterialiseCuratedFamilyCloneByPlanId(Guid familyPlanId)
        {
            if (!TryGetFamilyPlan(familyPlanId, out var plan))
                return null;

            return TryMaterialiseCuratedFamily(plan, auto, this)?.FamilyClone;
        }

        private MaterialisedCuratedFamily? TryMaterialiseCuratedFamily(
            CuratedFamilyPlan familyPlan, AutoRegistry auto, CurationOverlay overlay)
        {
            var builtGames = new List<IGame>();
            var builtGamePlans = new List<CuratedGamePlan>();
            var builtGamePartIds = new List<List<AutoPartId>>();

            var gamePlans = familyPlan.Games;

            for (int gi = 0; gi < gamePlans.Count; gi++)
            {
                var gp = gamePlans[gi];

                var partRefs = new List<IGamePart>(gp.Parts.Count);
                var partIds = new List<AutoPartId>(gp.Parts.Count);

                for (int i = 0; i < gp.Parts.Count; i++)
                {
                    var id = gp.Parts[i];

                    if (!auto.TryResolve(id, out var sourcePart))
                        continue;

                    partRefs.Add(sourcePart);
                    partIds.Add(id);
                }

                if (partRefs.Count == 0)
                    continue;

                var gameClone = new BaseGame(gp.Name, partRefs.ToArray());

                builtGames.Add(gameClone);
                builtGamePlans.Add(gp);
                builtGamePartIds.Add(partIds);
            }

            if (builtGames.Count == 0)
                return null;

            var familyName = builtGames[0].GetNameWithoutExt();

            if (string.IsNullOrWhiteSpace(familyName))
                familyName = familyPlan.Name;

            var familyClone = new BaseGameFamily(
                familyName,
                builtGames.ToArray(),
                [],
                BaseResourceCollection.Empty,
                false,
                comment: string.Empty);

            familyClone = GameEntityNameBuilder.UpdateNames(
                familyClone,
                overlay.flagFilterSetByGroup,
                overlay.softwareIdDatGroupEnumDictionary);

            var materialised = new MaterialisedCuratedFamily(familyClone);
            overlay.RegisterMaterialisedFamilyClone(familyClone, familyPlan.Id);

            var materialisedGames = familyClone.GetAllGames();

            for (int gi = 0; gi < materialisedGames.Length; gi++)
            {
                var gameClone = materialisedGames[gi];
                var gp = builtGamePlans[gi];

                materialised.ClonedGames[gameClone] = gp.Id;
                overlay.RegisterMaterialisedGameClone(gameClone, familyPlan.Id, gp.Id);

                var parts = gameClone.GetGameParts(false);
                var ids = builtGamePartIds[gi];

                var mapCount = Math.Min(parts.Length, ids.Count);
                for (int pi = 0; pi < mapCount; pi++)
                {
                    materialised.ClonedParts[parts[pi]] = ids[pi];
                    overlay.RegisterMaterialisedPartClone(parts[pi], ids[pi], familyPlan.Id, gp.Id);
                }
            }
            return materialised;
        }

        public DatGrouperEditDelta? SetCuratedPartAsInclude(IGamePart curatedPart)
            => SetCuratedPartAsIncludeExclude(curatedPart, true);

        public DatGrouperEditDelta? SetCuratedPartAsExclude(IGamePart curatedPart)
            => SetCuratedPartAsIncludeExclude(curatedPart, false);
        private DatGrouperEditDelta? SetCuratedPartAsIncludeExclude(IGamePart sourcePart, bool setAsInclude)
        {
            if (!TryResolveCuratedPartLoc(sourcePart, out var partId, out var loc))
                return null;

            if (!auto.TryResolve(partId, out var canonicalPart))
                return null;

            bool targetExclude = !setAsInclude;

            if (canonicalPart.Exclude == targetExclude)
                return null;

            return ApplyMutation(
                new[] { loc.FamilyPlanId },
                Array.Empty<AutoFamilyId>(),
                () =>
                {
                    canonicalPart.Exclude = targetExclude;
                    return true;
                },
                (_, __) =>
                {
                    var autoAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
                    var curatedAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);

                    if (sourcePart is IGameEntity sourceEntity)
                        curatedAffected.Add(sourceEntity);

                    if (curatedPartCloneByPartId.TryGetValue(partId, out var mappedPart) &&
                        mappedPart is IGameEntity mappedEntity)
                    {
                        curatedAffected.Add(mappedEntity);
                    }

                    return (autoAffected, curatedAffected);
                },
                new[] { partId },
                DELTA_NATURE_ENUM.PartIncludeExclude);
        }
        public DatGrouperEditDelta? TryAddOrMovePartBefore(IGamePart partToAdd, IGamePart referencePart)
            => TryAddOrMovePartRelative(partToAdd, referencePart, after: false);

        public DatGrouperEditDelta? TryAddOrMovePartAfter(IGamePart partToAdd, IGamePart referencePart)
            => TryAddOrMovePartRelative(partToAdd, referencePart, after: true);

        public DatGrouperEditDelta? TryAddOrMoveGameBefore(IGame gameToAdd, IGame referenceGame)
            => TryAddOrMoveGameRelative(gameToAdd, referenceGame, after: false);

        public DatGrouperEditDelta? TryAddOrMoveGameAfter(IGame gameToAdd, IGame referenceGame)
            => TryAddOrMoveGameRelative(gameToAdd, referenceGame, after: true);
        public DatGrouperEditDelta? TryMoveGameToTopOrBottom(IGame game, bool moveToTop)
        {
            if (!TryResolveCuratedGameLoc(game, out var loc))
                return null;

            var familyIndex = CuratedFamiliesInternal.FindIndex(f => f.Id == loc.FamilyPlanId);
            if (familyIndex < 0)
                return null;

            var familyPlan = CuratedFamiliesInternal[familyIndex];
            if (familyPlan.Games.Count <= 1)
                return null;

            if (moveToTop)
            {
                if (loc.GameIndex == 0)
                    return null;

                var firstGamePlan = familyPlan.Games[0];
                if (!firstGamePlan.AutoGameId.HasValue)
                    return null;

                if (!auto.TryResolve(firstGamePlan.AutoGameId.Value, out var firstGame))
                    return null;

                return TryAddOrMoveGameBefore(game, firstGame);
            }

            if (loc.GameIndex == familyPlan.Games.Count - 1)
                return null;

            var lastGamePlan = familyPlan.Games[^1];
            if (!lastGamePlan.AutoGameId.HasValue)
                return null;

            if (!auto.TryResolve(lastGamePlan.AutoGameId.Value, out var lastGame))
                return null;

            return TryAddOrMoveGameAfter(game, lastGame);
        }
        public DatGrouperEditDelta? TryResetFamily(IGameFamily family)
        {
            if (!TryResolveCuratedFamilyPlanId(family, out var familyPlanId))
                return null;

            var familyIndex = CuratedFamiliesInternal.FindIndex(f => f.Id == familyPlanId);
            if (familyIndex < 0)
                return null;

            var familyPlan = CuratedFamiliesInternal[familyIndex];

            AutoFamilyId? autoFamilyToReAdd = familyPlan.OriginAutoFamilyId;
            if (!autoFamilyToReAdd.HasValue)
            {
                AutoPartId? firstPid = null;

                for (int gi = 0; gi < familyPlan.Games.Count && !firstPid.HasValue; gi++)
                {
                    var gp = familyPlan.Games[gi];
                    for (int pi = 0; pi < gp.Parts.Count; pi++)
                    {
                        firstPid = gp.Parts[pi];
                        break;
                    }
                }

                if (firstPid.HasValue)
                    autoFamilyToReAdd = new AutoFamilyId(firstPid.Value.FamilyIndex);
            }

            var autoIds = new HashSet<AutoFamilyId>();
            for (int gi = 0; gi < familyPlan.Games.Count; gi++)
            {
                var gp = familyPlan.Games[gi];
                for (int pi = 0; pi < gp.Parts.Count; pi++)
                    autoIds.Add(new AutoFamilyId(gp.Parts[pi].FamilyIndex));
            }

            var autoTouch = autoIds.Count == 0 ? Array.Empty<AutoFamilyId>() : autoIds.ToArray();

            return ApplyMutation(
                new[] { familyPlanId },
                autoTouch,
                () => TryResetFamily(familyPlanId),
                (_, __) =>
                {
                    var autoAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
                    var curatedAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);

                    if (autoFamilyToReAdd.HasValue && auto.TryResolve(autoFamilyToReAdd.Value, out var famRef))
                        AddFamilyAndDescendants(autoAffected, famRef);

                    return (autoAffected, curatedAffected);
                });
        }
        public DatGrouperEditDelta? TryResetGame(IGame game)
        {
            if (!TryResolveCuratedGameLoc(game, out var loc))
                return null;

            var familyIndex = CuratedFamiliesInternal.FindIndex(f => f.Id == loc.FamilyPlanId);
            if (familyIndex < 0)
                return null;

            var familyPlan = CuratedFamiliesInternal[familyIndex];
            var gameIndex = familyPlan.Games.FindIndex(g => g.Id == loc.GamePlanId);
            if (gameIndex < 0)
                return null;

            var gamePlan = familyPlan.Games[gameIndex];

            AutoGameId? autoGameToReAdd = gamePlan.AutoGameId;
            if (!autoGameToReAdd.HasValue && gamePlan.Parts.Count > 0)
            {
                var pid = gamePlan.Parts[0];
                autoGameToReAdd = new AutoGameId(pid.FamilyIndex, pid.GameIndex);
            }

            var autoIds = new HashSet<AutoFamilyId>();
            for (int pi = 0; pi < gamePlan.Parts.Count; pi++)
                autoIds.Add(new AutoFamilyId(gamePlan.Parts[pi].FamilyIndex));

            var autoTouch = autoIds.Count == 0 ? Array.Empty<AutoFamilyId>() : autoIds.ToArray();

            return ApplyMutation(
                new[] { loc.FamilyPlanId },
                autoTouch,
                () => TryResetGame(loc.FamilyPlanId, loc.GamePlanId),
                (_, __) =>
                {
                    var autoAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
                    var curatedAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);

                    if (autoGameToReAdd.HasValue && auto.TryResolve(autoGameToReAdd.Value, out var gameRef))
                        AddGameAndParts(autoAffected, gameRef);

                    return (autoAffected, curatedAffected);
                });
        }
        public DatGrouperEditDelta? TryResetPart(IGamePart part)
        {
            if (!TryResolveCuratedPartLoc(part, out var partId, out var loc))
                return null;

            var autoTouch = new[] { new AutoFamilyId(partId.FamilyIndex) };

            return ApplyMutation(
                new[] { loc.FamilyPlanId },
                autoTouch,
                () => TryResetPart(loc.FamilyPlanId, loc.GamePlanId, partId),
                (_, __) =>
                {
                    var autoAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
                    var curatedAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);

                    if (auto.TryResolve(partId, out var partRef) && partRef is IGameEntity entity)
                        autoAffected.Add(entity);

                    return (autoAffected, curatedAffected);
                });
        }
        public DatGrouperEditDelta? TryAddFamily(IGameFamily family)
        {
            if (!auto.TryGetFamilyId(family, out var familyId))
                return null;

            EnsureCuratedIndex();
            if (curatedFamilyPlanIdByOriginAutoFamilyId.ContainsKey(familyId))
                return null;

            var newPlanId = Guid.NewGuid();

            return ApplyMutation(
                new[] { newPlanId },
                new[] { familyId },
                () => TryAddFamily(familyId, auto, newPlanId),
                (_, __) =>
                {
                    var autoAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
                    var curatedAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);

                    if (curatedFamilyCloneByPlanId.TryGetValue(newPlanId, out var famClone))
                        AddFamilyAndDescendants(curatedAffected, famClone);

                    return (autoAffected, curatedAffected);
                });
        }
        public DatGrouperEditDelta? TryMoveAndMergeFamily(IGameFamily sourceFamily, IGameFamily targetFamily, bool mergeAsMain)
        {
            if (!TryResolveCuratedFamilyPlanId(targetFamily, out var targetFamilyPlanId))
                return null;

            (IReadOnlySet<IGameEntity> AutoAffectedEntities, IReadOnlySet<IGameEntity> CuratedAffectedEntities) BuildAffected(PlanEdit[] planEdits, AutoVisEdit[] _)
            {
                var autoAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
                var curatedAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);

                AddCuratedNewGamesAndParts(
                    targetFamilyPlanId,
                    planEdits,
                    curatedAffected,
                    includeFamilyEntity: mergeAsMain,
                    includeAddedPartsInExistingGames: true);

                return (autoAffected, curatedAffected);
            }

            if (TryResolveCuratedFamilyPlanId(sourceFamily, out var sourceFamilyPlanId))
            {
                var planTouch = sourceFamilyPlanId == targetFamilyPlanId
                    ? new[] { sourceFamilyPlanId }
                    : new[] { sourceFamilyPlanId, targetFamilyPlanId };

                return ApplyMutation(
                    planTouch,
                    Array.Empty<AutoFamilyId>(),
                    () => TryMoveAndMergeCuratedFamily(sourceFamilyPlanId, targetFamilyPlanId, mergeAsMain),
                    BuildAffected);
            }

            if (!auto.TryGetFamilyId(sourceFamily, out var sourceAutoFamilyId))
                return null;

            return ApplyMutation(
                new[] { targetFamilyPlanId },
                new[] { sourceAutoFamilyId },
                () => TryMergeAutoFamilyIntoCuratedFamily(sourceAutoFamilyId, targetFamilyPlanId, mergeAsMain),
                BuildAffected);
        }
        private bool TryMergeAutoFamilyIntoCuratedFamily(AutoFamilyId sourceAutoFamilyId, Guid targetFamilyPlanId, bool mergeAsMain)
        {
            if (!auto.TryResolve(sourceAutoFamilyId, out var sourceFamily) || sourceFamily is null)
                return false;

            var targetIndex = CuratedFamiliesInternal.FindIndex(f => f.Id == targetFamilyPlanId);
            if (targetIndex < 0)
                return false;

            var targetPlan = CuratedFamiliesInternal[targetIndex];

            var newGamePlans = new List<CuratedGamePlan>();

            var sourceGames = sourceFamily.GetAllGames();
            for (int gi = 0; gi < sourceGames.Length; gi++)
            {
                var sourceGame = sourceGames[gi];

                if (!auto.TryGetGameId(sourceGame, out var gameId))
                    continue;

                var parts = sourceGame.GetGameParts(false);
                if (parts.Length == 0)
                    continue;

                CuratedGamePlan? existing = null;
                for (int tgi = 0; tgi < targetPlan.Games.Count; tgi++)
                {
                    var gp = targetPlan.Games[tgi];
                    if (gp.AutoGameId.HasValue && gp.AutoGameId.Value.Equals(gameId))
                    {
                        existing = gp;
                        break;
                    }
                }

                var addedAny = false;

                if (existing is not null)
                {
                    for (int pi = 0; pi < parts.Length; pi++)
                    {
                        var pid = new AutoPartId(gameId.FamilyIndex, gameId.GameIndex, pi);
                        if (CuratedPartsInternal.Contains(pid))
                            continue;

                        existing.Parts.Add(pid);
                        _ = AddCuratedPart(pid);
                        addedAny = true;
                    }

                    continue;
                }

                var gpNew = new CuratedGamePlan(sourceGame.GetNameWithoutExt(), gameId);

                for (int pi = 0; pi < parts.Length; pi++)
                {
                    var pid = new AutoPartId(gameId.FamilyIndex, gameId.GameIndex, pi);
                    if (CuratedPartsInternal.Contains(pid))
                        continue;

                    gpNew.Parts.Add(pid);
                    _ = AddCuratedPart(pid);
                    addedAny = true;
                }

                if (addedAny && gpNew.Parts.Count > 0)
                    newGamePlans.Add(gpNew);
            }

            if (newGamePlans.Count == 0)
                return false;

            var insertIndex = mergeAsMain ? 0 : targetPlan.Games.Count;
            targetPlan.Games.InsertRange(insertIndex, newGamePlans);

            MarkCuratedIndexDirty();

            return true;
        }

        private DatGrouperEditDelta? TryAddOrMovePartRelative(IGamePart partToAdd, IGamePart referencePart, bool after)
        {
            if (!TryResolveCuratedPartLoc(referencePart, out _, out var refLoc))
                return null;

            var targetFamilyPlanId = refLoc.FamilyPlanId;
            var targetGamePlanId = refLoc.GamePlanId;
            var insertIndex = after ? (refLoc.PartIndex + 1) : refLoc.PartIndex;

            if (TryResolveCuratedPartLoc(partToAdd, out var partIdMoving, out var sourceLoc))
            {
                var planTouch = sourceLoc.FamilyPlanId == targetFamilyPlanId
                    ? new[] { targetFamilyPlanId }
                    : new[] { sourceLoc.FamilyPlanId, targetFamilyPlanId };

                (IReadOnlySet<IGameEntity> AutoAffectedEntities, IReadOnlySet<IGameEntity> CuratedAffectedEntities) BuildAffectedCurated(PlanEdit[] planEdits, AutoVisEdit[] _)
                {
                    var autoAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
                    var curatedAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);

                    AddCuratedNewPartsInGame(targetFamilyPlanId, targetGamePlanId, planEdits, curatedAffected);

                    if (curatedPartCloneByPartId.TryGetValue(partIdMoving, out var movedPart) &&
                        movedPart is IGameEntity movedPartEntity)
                    {
                        curatedAffected.Add(movedPartEntity);
                    }

                    return (autoAffected, curatedAffected);
                }

                return ApplyMutation(
                    planTouch,
                    Array.Empty<AutoFamilyId>(),
                    () => TryMoveCuratedPart(
                        sourceLoc.FamilyPlanId,
                        sourceLoc.GamePlanId,
                        partIdMoving,
                        targetFamilyPlanId,
                        targetGamePlanId,
                        insertIndex),
                    BuildAffectedCurated);
            }

            if (!auto.TryGetPartId(partToAdd, out var partIdToAdd))
                return null;

            (IReadOnlySet<IGameEntity> AutoAffectedEntities, IReadOnlySet<IGameEntity> CuratedAffectedEntities) BuildAffectedAuto(PlanEdit[] planEdits, AutoVisEdit[] _)
            {
                var autoAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
                var curatedAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);

                AddCuratedNewPartsInGame(targetFamilyPlanId, targetGamePlanId, planEdits, curatedAffected);

                return (autoAffected, curatedAffected);
            }

            return ApplyMutation(
                new[] { targetFamilyPlanId },
                new[] { new AutoFamilyId(partIdToAdd.FamilyIndex) },
                () => TryAddPart(partIdToAdd, targetGamePlanId, insertIndex),
                BuildAffectedAuto);
        }
        private DatGrouperEditDelta? TryAddOrMoveGameRelative(IGame gameToAdd, IGame referenceGame, bool after)
        {
            if (!TryResolveCuratedGameLoc(referenceGame, out var refLoc))
                return null;

            var targetFamilyPlanId = refLoc.FamilyPlanId;
            var insertIndex = after ? (refLoc.GameIndex + 1) : refLoc.GameIndex;

            if (TryResolveCuratedGameLoc(gameToAdd, out var sourceLoc))
            {
                var planTouch = sourceLoc.FamilyPlanId == targetFamilyPlanId
                    ? new[] { targetFamilyPlanId }
                    : new[] { sourceLoc.FamilyPlanId, targetFamilyPlanId };

                (IReadOnlySet<IGameEntity> AutoAffectedEntities, IReadOnlySet<IGameEntity> CuratedAffectedEntities) BuildAffectedCurated(PlanEdit[] planEdits, AutoVisEdit[] _)
                {
                    var autoAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
                    var curatedAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);

                    AddCuratedNewGamesAndParts(
                        targetFamilyPlanId,
                        planEdits,
                        curatedAffected,
                        includeFamilyEntity: false,
                        includeAddedPartsInExistingGames: false);

                    if (curatedGameCloneByGamePlanId.TryGetValue(sourceLoc.GamePlanId, out var movedGame))
                        AddGameAndParts(curatedAffected, movedGame);

                    return (autoAffected, curatedAffected);
                }

                return ApplyMutation(
                    planTouch,
                    Array.Empty<AutoFamilyId>(),
                    () => TryMoveCuratedGame(
                        sourceLoc.FamilyPlanId,
                        sourceLoc.GamePlanId,
                        targetFamilyPlanId,
                        insertIndex),
                    BuildAffectedCurated);
            }

            if (!auto.TryGetGameId(gameToAdd, out var gameIdToAdd))
                return null;

            (IReadOnlySet<IGameEntity> AutoAffectedEntities, IReadOnlySet<IGameEntity> CuratedAffectedEntities) BuildAffectedAuto(PlanEdit[] planEdits, AutoVisEdit[] _)
            {
                var autoAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);
                var curatedAffected = new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance);

                AddCuratedNewGamesAndParts(
                    targetFamilyPlanId,
                    planEdits,
                    curatedAffected,
                    includeFamilyEntity: false,
                    includeAddedPartsInExistingGames: false);

                return (autoAffected, curatedAffected);
            }

            return ApplyMutation(
                new[] { targetFamilyPlanId },
                new[] { new AutoFamilyId(gameIdToAdd.FamilyIndex) },
                () => TryAddGame(gameIdToAdd, targetFamilyPlanId, insertIndex, auto),
                BuildAffectedAuto);
        }
        internal bool TryGetFamilyPlan(Guid familyPlanId, out CuratedFamilyPlan plan)
        {
            var idx = CuratedFamiliesInternal.FindIndex(f => f.Id == familyPlanId);
            if (idx < 0)
            {
                plan = default!;
                return false;
            }

            plan = CuratedFamiliesInternal[idx];
            return true;
        }

        private bool IsAutoFamilyVisible(AutoFamilyId id)
        {
            var total = auto.GetTotalPartCount(id);
            if (total <= 0)
                return false;

            var curated = (uint)id.FamilyIndex < (uint)curatedPartCountByAutoFamilyIndex.Length
                ? curatedPartCountByAutoFamilyIndex[id.FamilyIndex]
                : 0;

            return curated < total;
        }
        private void PurgeOrphanedMaterialisationCaches()
        {
            foreach (var planId in curatedFamilyCloneByPlanId.Keys.ToArray())
            {
                if (!curatedLocByFamilyPlanId.ContainsKey(planId))
                    _ = curatedFamilyCloneByPlanId.Remove(planId);
            }

            foreach (var gamePlanId in curatedGameCloneByGamePlanId.Keys.ToArray())
            {
                if (!curatedLocByGamePlanId.ContainsKey(gamePlanId))
                    _ = curatedGameCloneByGamePlanId.Remove(gamePlanId);
            }

            foreach (var partId in curatedPartCloneByPartId.Keys.ToArray())
            {
                if (!CuratedPartsInternal.Contains(partId) || !curatedLocByPartId.ContainsKey(partId))
                    _ = curatedPartCloneByPartId.Remove(partId);
            }

            foreach (var kv in curatedFamilyPlanIdByFamilyRef.ToArray())
            {
                var planId = kv.Value;
                if (!curatedFamilyCloneByPlanId.TryGetValue(planId, out var current) || !ReferenceEquals(current, kv.Key))
                    _ = curatedFamilyPlanIdByFamilyRef.Remove(kv.Key);
            }

            foreach (var kv in curatedGamePlanByGameRef.ToArray())
            {
                var gamePlanId = kv.Value.GamePlanId;
                if (!curatedGameCloneByGamePlanId.TryGetValue(gamePlanId, out var current) || !ReferenceEquals(current, kv.Key))
                    _ = curatedGamePlanByGameRef.Remove(kv.Key);
            }

            foreach (var kv in curatedPartByPartRef.ToArray())
            {
                var partId = kv.Value.PartId;
                if (!curatedPartCloneByPartId.TryGetValue(partId, out var current) || !ReferenceEquals(current, kv.Key))
                    _ = curatedPartByPartRef.Remove(kv.Key);
            }
        }

        private void ValidateStateOrThrow()
        {
            var partsFromPlans = new HashSet<AutoPartId>();
            for (int fi = 0; fi < CuratedFamiliesInternal.Count; fi++)
            {
                var fam = CuratedFamiliesInternal[fi];
                for (int gi = 0; gi < fam.Games.Count; gi++)
                {
                    var gp = fam.Games[gi];
                    for (int pi = 0; pi < gp.Parts.Count; pi++)
                    {
                        var pid = gp.Parts[pi];
                        if (!partsFromPlans.Add(pid))
                            throw new InvalidOperationException("Duplicate part id referenced across curated plans.");
                    }
                }
            }

            if (partsFromPlans.Count != CuratedPartsInternal.Count || !partsFromPlans.SetEquals(CuratedPartsInternal))
                throw new InvalidOperationException("CuratedPartsInternal is out of sync with curated plans.");

            foreach (var pid in CuratedPartsInternal)
            {
                if (!auto.TryResolve(pid, out _))
                    throw new InvalidOperationException("Curated plan references part id not resolvable in auto snapshot.");
            }

            for (int i = 0; i < curatedPartCountByAutoFamilyIndex.Length; i++)
            {
                var expected = 0;
                foreach (var pid in CuratedPartsInternal)
                {
                    if (pid.FamilyIndex == i)
                        expected++;
                }

                if (curatedPartCountByAutoFamilyIndex[i] != expected)
                    throw new InvalidOperationException("curatedPartCountByAutoFamilyIndex out of sync.");
            }
        }

        private static void AddFamilyAndDescendants(HashSet<IGameEntity> into, IGameFamily family)
        {
            _ = into.Add(family);

            var games = family.GetAllGames();
            for (int gi = 0; gi < games.Length; gi++)
            {
                var game = games[gi];
                _ = into.Add(game);

                var parts = game.GetGameParts(false);
                for (int pi = 0; pi < parts.Length; pi++)
                    _ = into.Add(parts[pi]);
            }
        }

        private static void AddGameAndParts(HashSet<IGameEntity> into, IGame game)
        {
            _ = into.Add(game);

            var parts = game.GetGameParts(false);
            for (int pi = 0; pi < parts.Length; pi++)
                _ = into.Add(parts[pi]);
        }

        private static bool TryGetPlanEdit(PlanEdit[] planEdits, Guid planId, out PlanEdit edit)
        {
            for (int i = 0; i < planEdits.Length; i++)
            {
                if (planEdits[i].PlanId == planId)
                {
                    edit = planEdits[i];
                    return true;
                }
            }

            edit = default;
            return false;
        }

        private static CuratedGamePlanSnap? FindGameSnap(CuratedFamilyPlanSnap snap, Guid gamePlanId)
        {
            var games = snap.Games;
            for (int i = 0; i < games.Length; i++)
            {
                var g = games[i];
                if (g.Id == gamePlanId)
                    return g;
            }

            return null;
        }

        private void AddCuratedNewGamesAndParts(
            Guid familyPlanId,
            PlanEdit[] planEdits,
            HashSet<IGameEntity> into,
            bool includeFamilyEntity,
            bool includeAddedPartsInExistingGames)
        {
            if (!TryGetPlanEdit(planEdits, familyPlanId, out var edit))
                return;

            if (!edit.After.Exists)
                return;

            if (includeFamilyEntity && curatedFamilyCloneByPlanId.TryGetValue(familyPlanId, out var famClone))
                _ = into.Add(famClone);

            var beforeGames = edit.Before.Snap.Games;
            var beforeById = new Dictionary<Guid, CuratedGamePlanSnap>(beforeGames.Length);
            for (int i = 0; i < beforeGames.Length; i++)
                beforeById[beforeGames[i].Id] = beforeGames[i];

            var afterGames = edit.After.Snap.Games;
            for (int gi = 0; gi < afterGames.Length; gi++)
            {
                var afterGame = afterGames[gi];

                if (!beforeById.TryGetValue(afterGame.Id, out var beforeGame))
                {
                    if (curatedGameCloneByGamePlanId.TryGetValue(afterGame.Id, out var gameClone))
                        _ = into.Add(gameClone);

                    var parts = afterGame.Parts;
                    for (int pi = 0; pi < parts.Length; pi++)
                    {
                        var pid = parts[pi];
                        if (curatedPartCloneByPartId.TryGetValue(pid, out var partClone))
                            _ = into.Add(partClone);
                    }

                    continue;
                }

                if (!includeAddedPartsInExistingGames)
                    continue;

                var beforeParts = beforeGame.Parts;
                if (beforeParts.Length == 0)
                {
                    var parts = afterGame.Parts;
                    for (int pi = 0; pi < parts.Length; pi++)
                    {
                        var pid = parts[pi];
                        if (curatedPartCloneByPartId.TryGetValue(pid, out var partClone))
                            _ = into.Add(partClone);
                    }

                    continue;
                }

                var beforePartSet = new HashSet<AutoPartId>(beforeParts.Length);
                for (int pi = 0; pi < beforeParts.Length; pi++)
                    _ = beforePartSet.Add(beforeParts[pi]);

                var afterParts = afterGame.Parts;
                for (int pi = 0; pi < afterParts.Length; pi++)
                {
                    var pid = afterParts[pi];
                    if (beforePartSet.Contains(pid))
                        continue;

                    if (curatedPartCloneByPartId.TryGetValue(pid, out var partClone))
                        _ = into.Add(partClone);
                }
            }
        }

        private void AddCuratedNewPartsInGame(
            Guid familyPlanId,
            Guid gamePlanId,
            PlanEdit[] planEdits,
            HashSet<IGameEntity> into)
        {
            if (!TryGetPlanEdit(planEdits, familyPlanId, out var edit))
                return;

            var afterGame = FindGameSnap(edit.After.Snap, gamePlanId);
            if (afterGame is null)
                return;

            var beforeGame = FindGameSnap(edit.Before.Snap, gamePlanId);

            var beforeParts = beforeGame?.Parts ?? Array.Empty<AutoPartId>();
            var beforePartSet = new HashSet<AutoPartId>(beforeParts.Length);
            for (int i = 0; i < beforeParts.Length; i++)
                _ = beforePartSet.Add(beforeParts[i]);

            var afterParts = afterGame.Parts;
            for (int i = 0; i < afterParts.Length; i++)
            {
                var pid = afterParts[i];
                if (beforePartSet.Contains(pid))
                    continue;

                if (curatedPartCloneByPartId.TryGetValue(pid, out var partClone))
                    _ = into.Add(partClone);
            }
        }

        private DatGrouperEditDelta? ApplyMutation(
            IReadOnlyCollection<Guid> curatedFamilyPlanIdsToTouch,
            IReadOnlyCollection<AutoFamilyId> autoFamilyIdsToTouch,
            Func<bool> mutate,
            Func<PlanEdit[], AutoVisEdit[], (IReadOnlySet<IGameEntity> AutoAffectedEntities, IReadOnlySet<IGameEntity> CuratedAffectedEntities)>? buildAffectedEntities = null,
            IReadOnlyCollection<AutoPartId>? excludeTouchedPartIds = null,
            DatGrouperEditDelta.DELTA_NATURE_ENUM deltaNature = DatGrouperEditDelta.DELTA_NATURE_ENUM.Update)
        {
            EnsureCuratedIndex();

            var planIds = curatedFamilyPlanIdsToTouch.Count == 0
                ? Array.Empty<Guid>()
                : curatedFamilyPlanIdsToTouch.Distinct().ToArray();

            var autoIds = autoFamilyIdsToTouch.Count == 0
                ? Array.Empty<AutoFamilyId>()
                : autoFamilyIdsToTouch.Distinct().ToArray();

            var excludePartIds = excludeTouchedPartIds is null || excludeTouchedPartIds.Count == 0
                ? Array.Empty<AutoPartId>()
                : excludeTouchedPartIds.Distinct().ToArray();

            var beforePlanStates = new Dictionary<Guid, PlanState>();
            for (int i = 0; i < planIds.Length; i++)
                beforePlanStates[planIds[i]] = CapturePlanState(planIds[i]);

            var oldCuratedCloneByPlanId = new Dictionary<Guid, IGameFamily>();
            var oldOriginAutoFamilyIdByPlanId = new Dictionary<Guid, AutoFamilyId?>();

            for (int i = 0; i < planIds.Length; i++)
            {
                var planId = planIds[i];

                if (curatedFamilyCloneByPlanId.TryGetValue(planId, out var oldClone))
                    oldCuratedCloneByPlanId[planId] = oldClone;

                if (TryGetFamilyPlan(planId, out var plan))
                    oldOriginAutoFamilyIdByPlanId[planId] = plan.OriginAutoFamilyId;
            }

            var oldAutoVisibleById = new Dictionary<AutoFamilyId, bool>();
            var oldAutoFamilyById = new Dictionary<AutoFamilyId, IGameFamily>();

            for (int i = 0; i < autoIds.Length; i++)
            {
                var id = autoIds[i];
                oldAutoVisibleById[id] = IsAutoFamilyVisible(id);

                if (auto.TryResolve(id, out var fam) && fam is not null)
                    oldAutoFamilyById[id] = fam;
            }

            var beforeExcludeByPartId = new Dictionary<AutoPartId, bool>();
            for (int i = 0; i < excludePartIds.Length; i++)
            {
                var id = excludePartIds[i];
                if (auto.TryResolve(id, out var part))
                    beforeExcludeByPartId[id] = part.Exclude;
            }

            if (!mutate())
                return null;

            EnsureCuratedIndex();

            var afterPlanStates = new Dictionary<Guid, PlanState>();
            for (int i = 0; i < planIds.Length; i++)
                afterPlanStates[planIds[i]] = CapturePlanState(planIds[i]);

            var planEdits = planIds.Length == 0
                ? Array.Empty<PlanEdit>()
                : planIds.Select(pid => new PlanEdit(pid, beforePlanStates[pid], afterPlanStates[pid])).ToArray();

            var autoVisEdits = autoIds.Length == 0
                ? Array.Empty<AutoVisEdit>()
                : autoIds.Select(id => new AutoVisEdit(id, oldAutoVisibleById[id], IsAutoFamilyVisible(id))).ToArray();

            var partExcludeEdits = excludePartIds.Length == 0
                ? Array.Empty<PartExcludeEdit>()
                : excludePartIds
                    .Where(id => auto.TryResolve(id, out _))
                    .Select(id =>
                    {
                        auto.TryResolve(id, out var part);
                        return new PartExcludeEdit(id, beforeExcludeByPartId[id], part.Exclude);
                    })
                    .Where(e => e.BeforeExclude != e.AfterExclude)
                    .ToArray();

            var newCloneByPlanId = new Dictionary<Guid, IGameFamily>();
            for (int i = 0; i < planIds.Length; i++)
            {
                var planId = planIds[i];

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

            for (int i = 0; i < planIds.Length; i++)
            {
                var planId = planIds[i];

                var existsNow = newCloneByPlanId.TryGetValue(planId, out var newClone);
                var hadOld = oldCuratedCloneByPlanId.TryGetValue(planId, out var oldClone);

                if (!existsNow)
                {
                    if (hadOld)
                    {
                        curatedRemove.Add(oldClone!);
                        _ = curatedFamilyCloneByPlanId.Remove(planId);

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

            for (int i = 0; i < autoIds.Length; i++)
            {
                var id = autoIds[i];

                var wasVisible = oldAutoVisibleById.TryGetValue(id, out var v) && v;
                var isVisible = IsAutoFamilyVisible(id);

                if (!auto.TryResolve(id, out var fam) || fam is null)
                    continue;

                if (wasVisible && !isVisible)
                {
                    autoRemove.Add(fam);

                    if (oldAutoFamilyById.TryGetValue(id, out var oldAutoFam))
                    {
                        IGameFamily? newRef = null;

                        if (curatedFamilyPlanIdByOriginAutoFamilyId.TryGetValue(id, out var planIdNow) &&
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

            redos.Clear();

            var availableUndos = undos.Count + 1;
            var availableRedos = 0;

            var (autoAffectedEntities, curatedAffectedEntities) =
                buildAffectedEntities is null
                    ? (new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance), new HashSet<IGameEntity>(ReferenceEqualityComparer.Instance))
                    : buildAffectedEntities(planEdits, autoVisEdits);

            var delta = new DatGrouperEditDelta(
                deltaNature,
                curatedReplace,
                curatedAdd,
                curatedRemove,
                autoAdd,
                autoRemove,
                curatedAutoParts,
                availableUndos,
                availableRedos,
                autoAffectedEntities,
                curatedAffectedEntities)
            {
                PlanEdits = planEdits,
                AutoVisEdits = autoVisEdits,
                PartExcludeEdits = partExcludeEdits
            };

            undos.Add(delta);

            return delta;
        }
        private bool TryMoveAndMergeCuratedFamily(Guid sourceFamilyPlanId, Guid targetFamilyPlanId, bool mergeAsMain)
        {
            if (sourceFamilyPlanId == Guid.Empty || targetFamilyPlanId == Guid.Empty)
                return false;

            if (sourceFamilyPlanId == targetFamilyPlanId)
                return false;

            var sourceIndex = CuratedFamiliesInternal.FindIndex(f => f.Id == sourceFamilyPlanId);
            if (sourceIndex < 0)
                return false;

            var targetIndex = CuratedFamiliesInternal.FindIndex(f => f.Id == targetFamilyPlanId);
            if (targetIndex < 0)
                return false;

            var sourcePlan = CuratedFamiliesInternal[sourceIndex];
            var targetPlan = CuratedFamiliesInternal[targetIndex];

            var insertIndex = mergeAsMain ? 0 : targetPlan.Games.Count;

            if (sourcePlan.Games.Count > 0)
                targetPlan.Games.InsertRange(insertIndex, sourcePlan.Games);

            CuratedFamiliesInternal.RemoveAt(sourceIndex);

            MarkCuratedIndexDirty();

            return true;
        }

        private bool TryResetFamily(Guid familyPlanId)
        {
            var familyIndex = CuratedFamiliesInternal.FindIndex(f => f.Id == familyPlanId);
            if (familyIndex < 0)
                return false;

            var familyPlan = CuratedFamiliesInternal[familyIndex];

            var impactedFamilies = new HashSet<AutoFamilyId>();
            AutoPartId? firstPid = null;

            for (int gi = 0; gi < familyPlan.Games.Count; gi++)
            {
                var gp = familyPlan.Games[gi];
                for (int pi = 0; pi < gp.Parts.Count; pi++)
                {
                    var pid = gp.Parts[pi];

                    if (!firstPid.HasValue)
                        firstPid = pid;

                    RemoveCuratedPart(pid);
                    impactedFamilies.Add(new AutoFamilyId(pid.FamilyIndex));
                }
            }

            if (!firstPid.HasValue)
                return false;

            CuratedFamiliesInternal.RemoveAt(familyIndex);

            var sourceFamilyId = new AutoFamilyId(firstPid.Value.FamilyIndex);
            var sourceGameId = new AutoGameId(firstPid.Value.FamilyIndex, firstPid.Value.GameIndex);
            var sourcePartId = firstPid.Value;

            MarkCuratedIndexDirty();

            return true;
        }
        private bool TryAddFamily(AutoFamilyId familyId, AutoRegistry auto)
            => TryAddFamily(familyId, auto, Guid.NewGuid());

        private bool TryAddFamily(AutoFamilyId familyId, AutoRegistry auto, Guid newPlanId)
        {
            if (!auto.TryResolve(familyId, out var family) || family is null)
                return false;

            EnsureCuratedIndex();
            if (curatedFamilyPlanIdByOriginAutoFamilyId.ContainsKey(familyId))
                return false;

            var name = family.GetFamilyDisplayName();
            var newFamily = new CuratedFamilyPlan(newPlanId, name, familyId);

            var sourceGames = family.GetAllGames();
            var partsAdded = 0;

            for (int gi = 0; gi < sourceGames.Length; gi++)
            {
                var game = sourceGames[gi];
                if (!auto.TryGetGameId(game, out var gameId))
                    continue;

                var gp = new CuratedGamePlan(game.GetNameWithoutExt(), gameId);

                var parts = game.GetGameParts(false);
                for (int pi = 0; pi < parts.Length; pi++)
                {
                    var partId = new AutoPartId(gameId.FamilyIndex, gameId.GameIndex, pi);
                    if (CuratedPartsInternal.Contains(partId))
                        continue;

                    gp.Parts.Add(partId);
                    AddCuratedPart(partId);
                    partsAdded++;
                }

                if (gp.Parts.Count > 0)
                    newFamily.Games.Add(gp);
            }

            if (partsAdded == 0)
                return false;

            var insertIndex = GetCuratedFamilyInsertIndex(CuratedFamiliesInternal, name);
            CuratedFamiliesInternal.Insert(insertIndex, newFamily);

            MarkCuratedIndexDirty();

            return true;
        }

        private bool TryResetGame(Guid familyPlanId, Guid gamePlanId)
        {
            var fi = CuratedFamiliesInternal.FindIndex(f => f.Id == familyPlanId);
            if (fi < 0) return false;

            var familyPlan = CuratedFamiliesInternal[fi];

            var gameIndex = familyPlan.Games.FindIndex(g => g.Id == gamePlanId);
            if (gameIndex < 0) return false;

            var gamePlan = familyPlan.Games[gameIndex];

            AutoGameId? sourceAutoGameId = gamePlan.AutoGameId;
            var impactedFamilies = new HashSet<AutoFamilyId>();
            AutoPartId? firstPid = null;

            for (int pi = 0; pi < gamePlan.Parts.Count; pi++)
            {
                var pid = gamePlan.Parts[pi];

                if (!firstPid.HasValue)
                    firstPid = pid;

                RemoveCuratedPart(pid);
                impactedFamilies.Add(new AutoFamilyId(pid.FamilyIndex));
            }

            if (!sourceAutoGameId.HasValue && firstPid.HasValue)
                sourceAutoGameId = new AutoGameId(firstPid.Value.FamilyIndex, firstPid.Value.GameIndex);

            if (!sourceAutoGameId.HasValue)
                return false;

            familyPlan.Games.RemoveAt(gameIndex);

            var sourceFamilyId = new AutoFamilyId(sourceAutoGameId.Value.FamilyIndex);
            var sourceGameId = sourceAutoGameId;
            var sourcePartId = firstPid;

            if (familyPlan.Games.Count == 0)
            {
                CuratedFamiliesInternal.RemoveAt(fi);

                MarkCuratedIndexDirty();

                return true;
            }

            MarkCuratedIndexDirty();

            return true;
        }

        private bool TryResetPart(Guid familyPlanId, Guid gamePlanId, AutoPartId partId)
        {
            var fi = CuratedFamiliesInternal.FindIndex(f => f.Id == familyPlanId);
            if (fi < 0)
                return false;

            var familyPlan = CuratedFamiliesInternal[fi];

            var gi = familyPlan.Games.FindIndex(g => g.Id == gamePlanId);
            if (gi < 0)
                return false;

            var gamePlan = familyPlan.Games[gi];

            var partIndex = gamePlan.Parts.IndexOf(partId);
            if (partIndex < 0)
                return false;

            gamePlan.Parts.RemoveAt(partIndex);
            RemoveCuratedPart(partId);

            var sourceFamilyId = new AutoFamilyId(partId.FamilyIndex);
            var sourceGameId = new AutoGameId(partId.FamilyIndex, partId.GameIndex);
            var impactedFamilies = new[] { sourceFamilyId };

            if (gamePlan.Parts.Count == 0)
            {
                familyPlan.Games.RemoveAt(gi);

                if (familyPlan.Games.Count == 0)
                {
                    CuratedFamiliesInternal.RemoveAt(fi);

                    MarkCuratedIndexDirty();

                    return true;
                }

                MarkCuratedIndexDirty();

                return true;
            }

            MarkCuratedIndexDirty();

            return true;
        }

        private bool TryAddGame(AutoGameId gameId, Guid targetCuratedFamilyId, int insertIndex, AutoRegistry auto)
        {
            if (!auto.TryResolve(new AutoFamilyId(gameId.FamilyIndex), out var sourceFamily))
                return false;

            var games = sourceFamily.GetAllGames();
            if (gameId.GameIndex < 0 || gameId.GameIndex >= games.Length)
                return false;

            var sourceGame = games[gameId.GameIndex];

            var familyPlan = CuratedFamiliesInternal.FirstOrDefault(f => f.Id == targetCuratedFamilyId);
            if (familyPlan == null)
                return false;

            var gamePlan = new CuratedGamePlan(sourceGame.GetNameWithoutExt(), gameId);

            var parts = sourceGame.GetGameParts(false);
            for (int pi = 0; pi < parts.Length; pi++)
            {
                var id = new AutoPartId(gameId.FamilyIndex, gameId.GameIndex, pi);
                if (CuratedPartsInternal.Contains(id))
                    continue;

                gamePlan.Parts.Add(id);
                AddCuratedPart(id);
            }

            if (gamePlan.Parts.Count == 0)
                return false;

            insertIndex = Math.Max(0, Math.Min(insertIndex, familyPlan.Games.Count));
            familyPlan.Games.Insert(insertIndex, gamePlan);

            var impacted = new[] { new AutoFamilyId(gameId.FamilyIndex) };

            MarkCuratedIndexDirty();

            return true;
        }

        private bool TryAddPart(AutoPartId partId, Guid targetCuratedGameId, int insertIndex)
        {
            if (CuratedPartsInternal.Contains(partId))
                return false;

            for (int fi = 0; fi < CuratedFamiliesInternal.Count; fi++)
            {
                var fam = CuratedFamiliesInternal[fi];
                for (int gi = 0; gi < fam.Games.Count; gi++)
                {
                    var game = fam.Games[gi];
                    if (game.Id != targetCuratedGameId)
                        continue;

                    insertIndex = Math.Max(0, Math.Min(insertIndex, game.Parts.Count));
                    game.Parts.Insert(insertIndex, partId);
                    AddCuratedPart(partId);

                    var impacted = new[] { new AutoFamilyId(partId.FamilyIndex) };

                    MarkCuratedIndexDirty();

                    return true;
                }
            }

            return false;
        }

        private bool TryMoveCuratedGame(Guid sourceFamilyPlanId, Guid gamePlanId, Guid targetFamilyPlanId, int insertIndex)
        {
            var sourceFi = CuratedFamiliesInternal.FindIndex(f => f.Id == sourceFamilyPlanId);
            if (sourceFi < 0)
                return false;

            var targetFi = CuratedFamiliesInternal.FindIndex(f => f.Id == targetFamilyPlanId);
            if (targetFi < 0)
                return false;

            var sourceFamilyPlan = CuratedFamiliesInternal[sourceFi];

            var fromIndex = sourceFamilyPlan.Games.FindIndex(g => g.Id == gamePlanId);
            if (fromIndex < 0)
                return false;

            if (sourceFi == targetFi)
            {
                insertIndex = Math.Max(0, Math.Min(insertIndex, sourceFamilyPlan.Games.Count));

                if (insertIndex > fromIndex)
                    insertIndex--;

                if (insertIndex == fromIndex)
                    return false;

                var moving = sourceFamilyPlan.Games[fromIndex];
                sourceFamilyPlan.Games.RemoveAt(fromIndex);
                sourceFamilyPlan.Games.Insert(insertIndex, moving);

                MarkCuratedIndexDirty();

                return true;
            }

            var gamePlan = sourceFamilyPlan.Games[fromIndex];
            sourceFamilyPlan.Games.RemoveAt(fromIndex);

            if (sourceFamilyPlan.Games.Count == 0)
            {
                CuratedFamiliesInternal.RemoveAt(sourceFi);
                if (targetFi > sourceFi)
                    targetFi--;
            }

            if (targetFi < 0 || targetFi >= CuratedFamiliesInternal.Count)
                return false;

            var targetFamilyPlan = CuratedFamiliesInternal[targetFi];

            insertIndex = Math.Max(0, Math.Min(insertIndex, targetFamilyPlan.Games.Count));
            targetFamilyPlan.Games.Insert(insertIndex, gamePlan);

            MarkCuratedIndexDirty();

            return true;
        }

        private bool TryMoveCuratedPart(
            Guid sourceFamilyPlanId,
            Guid sourceGamePlanId,
            AutoPartId partId,
            Guid targetFamilyPlanId,
            Guid targetGamePlanId,
            int insertIndex)
        {
            var sourceFi = CuratedFamiliesInternal.FindIndex(f => f.Id == sourceFamilyPlanId);
            if (sourceFi < 0)
                return false;

            var sourceFamilyPlan = CuratedFamiliesInternal[sourceFi];

            var sourceGi = sourceFamilyPlan.Games.FindIndex(g => g.Id == sourceGamePlanId);
            if (sourceGi < 0)
                return false;

            var sourceGamePlan = sourceFamilyPlan.Games[sourceGi];

            var fromIndex = sourceGamePlan.Parts.IndexOf(partId);
            if (fromIndex < 0)
                return false;

            var targetFi = sourceFamilyPlanId == targetFamilyPlanId
                ? sourceFi
                : CuratedFamiliesInternal.FindIndex(f => f.Id == targetFamilyPlanId);

            if (targetFi < 0)
                return false;

            var targetFamilyPlan = CuratedFamiliesInternal[targetFi];

            var targetGi = targetFamilyPlan.Games.FindIndex(g => g.Id == targetGamePlanId);
            if (targetGi < 0)
                return false;

            var targetGamePlan = targetFamilyPlan.Games[targetGi];

            if (sourceFamilyPlanId == targetFamilyPlanId && ReferenceEquals(sourceGamePlan, targetGamePlan))
            {
                insertIndex = Math.Max(0, Math.Min(insertIndex, sourceGamePlan.Parts.Count));

                if (insertIndex > fromIndex)
                    insertIndex--;

                if (insertIndex == fromIndex)
                    return false;

                sourceGamePlan.Parts.RemoveAt(fromIndex);
                sourceGamePlan.Parts.Insert(insertIndex, partId);
            }
            else
            {
                sourceGamePlan.Parts.RemoveAt(fromIndex);

                insertIndex = Math.Max(0, Math.Min(insertIndex, targetGamePlan.Parts.Count));
                targetGamePlan.Parts.Insert(insertIndex, partId);

                if (sourceGamePlan.Parts.Count == 0)
                {
                    sourceFamilyPlan.Games.RemoveAt(sourceGi);

                    if (sourceFamilyPlan.Games.Count == 0)
                        CuratedFamiliesInternal.RemoveAt(sourceFi);
                }
            }

            MarkCuratedIndexDirty();

            return true;
        }

        internal void RegisterMaterialisedFamilyClone(IGameFamily familyClone, Guid familyPlanId)
        {
            curatedFamilyPlanIdByFamilyRef[familyClone] = familyPlanId;
            curatedFamilyCloneByPlanId[familyPlanId] = familyClone;
        }

        internal void RegisterMaterialisedGameClone(IGame gameClone, Guid familyPlanId, Guid gamePlanId)
        {
            curatedGamePlanByGameRef[gameClone] = (familyPlanId, gamePlanId);
            curatedGameCloneByGamePlanId[gamePlanId] = gameClone;
        }

        internal void RegisterMaterialisedPartClone(IGamePart partClone, AutoPartId partId, Guid familyPlanId, Guid gamePlanId)
        {
            curatedPartByPartRef[partClone] = (partId, familyPlanId, gamePlanId);
            curatedPartCloneByPartId[partId] = partClone;
        }

        private void MarkCuratedIndexDirty() => curatedIndexDirty = true;

        private void EnsureCuratedIndex()
        {
            if (!curatedIndexDirty)
                return;

            curatedLocByPartId.Clear();
            curatedLocByAutoGameId.Clear();
            curatedLocByFamilyPlanId.Clear();
            curatedLocByGamePlanId.Clear();
            curatedFamilyPlanIdByOriginAutoFamilyId.Clear();

            Array.Clear(curatedPartCountByAutoFamilyIndex, 0, curatedPartCountByAutoFamilyIndex.Length);

            for (int fi = 0; fi < CuratedFamiliesInternal.Count; fi++)
            {
                var fam = CuratedFamiliesInternal[fi];

                curatedLocByFamilyPlanId[fam.Id] = new CuratedFamilyLoc(fam.Id, fi);

                if (fam.OriginAutoFamilyId.HasValue)
                    curatedFamilyPlanIdByOriginAutoFamilyId[fam.OriginAutoFamilyId.Value] = fam.Id;

                for (int gi = 0; gi < fam.Games.Count; gi++)
                {
                    var gp = fam.Games[gi];
                    var gameLoc = new CuratedGameLoc(fam.Id, gp.Id, fi, gi);

                    curatedLocByGamePlanId[gp.Id] = gameLoc;

                    if (gp.AutoGameId.HasValue && !curatedLocByAutoGameId.ContainsKey(gp.AutoGameId.Value))
                        curatedLocByAutoGameId[gp.AutoGameId.Value] = gameLoc;

                    for (int pi = 0; pi < gp.Parts.Count; pi++)
                    {
                        var pid = gp.Parts[pi];
                        curatedLocByPartId[pid] = new CuratedPartLoc(fam.Id, gp.Id, fi, gi, pi);

                        if ((uint)pid.FamilyIndex < (uint)curatedPartCountByAutoFamilyIndex.Length)
                            curatedPartCountByAutoFamilyIndex[pid.FamilyIndex]++;
                    }
                }
            }

            curatedIndexDirty = false;
        }

        private bool TryResolveAutoPartIdFromAny(IGamePart part, out AutoPartId partId)
        {
            if (curatedPartByPartRef.TryGetValue(part, out var info))
            {
                if (curatedPartCloneByPartId.TryGetValue(info.PartId, out var current) && ReferenceEquals(current, part))
                {
                    partId = info.PartId;
                    return true;
                }
            }

            return auto.TryGetPartId(part, out partId);
        }

        private bool TryResolveCuratedFamilyPlanId(IGameFamily family, out Guid familyPlanId)
        {
            if (curatedFamilyPlanIdByFamilyRef.TryGetValue(family, out familyPlanId))
            {
                if (curatedFamilyCloneByPlanId.TryGetValue(familyPlanId, out var current) && ReferenceEquals(current, family))
                    return true;
            }

            if (auto.TryGetFamilyId(family, out var autoFamilyId))
            {
                EnsureCuratedIndex();
                return curatedFamilyPlanIdByOriginAutoFamilyId.TryGetValue(autoFamilyId, out familyPlanId);
            }

            familyPlanId = Guid.Empty;
            return false;
        }

        private bool TryResolveCuratedGameLoc(IGame game, out CuratedGameLoc loc)
        {
            EnsureCuratedIndex();

            if (curatedGamePlanByGameRef.TryGetValue(game, out var info))
            {
                if (curatedGameCloneByGamePlanId.TryGetValue(info.GamePlanId, out var current) && ReferenceEquals(current, game))
                    return curatedLocByGamePlanId.TryGetValue(info.GamePlanId, out loc);
            }

            if (auto.TryGetGameId(game, out var autoGameId))
                return curatedLocByAutoGameId.TryGetValue(autoGameId, out loc);

            loc = default;
            return false;
        }

        private bool TryResolveCuratedPartLoc(IGamePart part, out AutoPartId autoPartId, out CuratedPartLoc loc)
        {
            EnsureCuratedIndex();

            if (!TryResolveAutoPartIdFromAny(part, out autoPartId))
            {
                loc = default;
                return false;
            }

            return curatedLocByPartId.TryGetValue(autoPartId, out loc);
        }

        public void Reset()
        {
            ClearCuratedParts();
            CuratedFamiliesInternal.Clear();
            Array.Clear(curatedPartCountByAutoFamilyIndex, 0, curatedPartCountByAutoFamilyIndex.Length);
            MarkCuratedIndexDirty();
        }

        private static readonly StringComparer FamilyNameComparer = StringComparer.OrdinalIgnoreCase;

        private static int GetCuratedFamilyInsertIndex(List<CuratedFamilyPlan> list, string familyName)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var existingName = list[i].Name;
                if (FamilyNameComparer.Compare(existingName, familyName) > 0)
                    return i;
            }

            return list.Count;
        }


        private bool AddCuratedPart(AutoPartId id)
        {
            if (!CuratedPartsInternal.Add(id))
                return false;

            if (auto.TryResolve(id, out var part))
                _ = curatedAutoPartsInternal.Add(part);

            return true;
        }

        private bool RemoveCuratedPart(AutoPartId id)
        {
            if (!CuratedPartsInternal.Remove(id))
                return false;

            RestoreDefaultExclude(id);

            if (auto.TryResolve(id, out var part))
                _ = curatedAutoPartsInternal.Remove(part);

            return true;
        }
        private void ClearCuratedParts()
        {
            foreach (var id in CuratedPartsInternal)
                RestoreDefaultExclude(id);

            CuratedPartsInternal.Clear();
            curatedAutoPartsInternal.Clear();
        }
    }

    internal sealed class AutoRegistry
    {
        public AutoRegistry(IReadOnlyList<IGameFamily> families)
        {
            SetSnapshot(families);
        }

        private readonly Dictionary<AutoFamilyId, IGameFamily> familyById = new();
        private readonly Dictionary<AutoGameId, IGame> gameById = new();
        private readonly Dictionary<AutoPartId, IGamePart> partById = new();

        private readonly Dictionary<IGameFamily, AutoFamilyId> familyIdByRef = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<IGame, AutoGameId> gameIdByRef = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<IGamePart, AutoPartId> partIdByRef = new(ReferenceEqualityComparer.Instance);

        private int[] totalPartCountByFamilyIndex = Array.Empty<int>();

        public bool HasSnapshot => familyById.Count > 0;
        public int FamilyCount => totalPartCountByFamilyIndex.Length;

        public int GetTotalPartCount(AutoFamilyId id)
            => (uint)id.FamilyIndex < (uint)totalPartCountByFamilyIndex.Length
                ? totalPartCountByFamilyIndex[id.FamilyIndex]
                : 0;

        public void ClearSnapshot()
        {
            familyById.Clear();
            gameById.Clear();
            partById.Clear();

            familyIdByRef.Clear();
            gameIdByRef.Clear();
            partIdByRef.Clear();

            totalPartCountByFamilyIndex = Array.Empty<int>();
        }

        public void SetSnapshot(IReadOnlyList<IGameFamily> families)
        {
            familyById.Clear();
            gameById.Clear();
            partById.Clear();

            familyIdByRef.Clear();
            gameIdByRef.Clear();
            partIdByRef.Clear();

            totalPartCountByFamilyIndex = new int[families.Count];

            for (int fi = 0; fi < families.Count; fi++)
            {
                var fam = families[fi];
                var famId = new AutoFamilyId(fi);

                familyById[famId] = fam;
                familyIdByRef[fam] = famId;

                var games = fam.GetAllGames();
                for (int gi = 0; gi < games.Length; gi++)
                {
                    var game = games[gi];
                    var gameId = new AutoGameId(fi, gi);

                    gameById[gameId] = game;
                    gameIdByRef[game] = gameId;

                    var parts = game.GetGameParts(false);
                    for (int pi = 0; pi < parts.Length; pi++)
                    {
                        var part = parts[pi];
                        var partId = new AutoPartId(fi, gi, pi);

                        partById[partId] = part;
                        partIdByRef[part] = partId;

                        totalPartCountByFamilyIndex[fi]++;
                    }
                }
            }
        }

        public bool TryGetFamilyId(IGameFamily family, out AutoFamilyId id) => familyIdByRef.TryGetValue(family, out id);
        public bool TryGetGameId(IGame game, out AutoGameId id) => gameIdByRef.TryGetValue(game, out id);
        public bool TryGetPartId(IGamePart part, out AutoPartId id) => partIdByRef.TryGetValue(part, out id);

        public bool TryResolve(AutoFamilyId id, out IGameFamily family) => familyById.TryGetValue(id, out family!);
        public bool TryResolve(AutoGameId id, out IGame game) => gameById.TryGetValue(id, out game!);
        public bool TryResolve(AutoPartId id, out IGamePart part) => partById.TryGetValue(id, out part!);
    }

    internal sealed class CuratedFamilyPlan
    {
        public Guid Id { get; }
        public string Name { get; }
        public List<CuratedGamePlan> Games { get; } = new();

        public AutoFamilyId? OriginAutoFamilyId { get; }

        public CuratedFamilyPlan(string name) : this(Guid.NewGuid(), name, null) { }
        public CuratedFamilyPlan(string name, AutoFamilyId originAutoFamilyId) : this(Guid.NewGuid(), name, originAutoFamilyId) { }
        public CuratedFamilyPlan(Guid id, string name) : this(id, name, null) { }

        public CuratedFamilyPlan(Guid id, string name, AutoFamilyId? originAutoFamilyId)
        {
            Id = id;
            Name = name;
            OriginAutoFamilyId = originAutoFamilyId;
        }
    }

    internal sealed class CuratedGamePlan
    {
        public Guid Id { get; }
        public string Name { get; }

        public AutoGameId? AutoGameId { get; }

        public List<AutoPartId> Parts { get; } = new();

        public CuratedGamePlan(string name, AutoGameId autoGameId) : this(Guid.NewGuid(), name, autoGameId) { }

        public CuratedGamePlan(Guid id, string name, AutoGameId autoGameId)
        {
            Id = id;
            Name = name;
            AutoGameId = autoGameId;
        }
    }

    internal sealed class MaterialisedCuratedFamily
    {
        public MaterialisedCuratedFamily(IGameFamily familyClone) => FamilyClone = familyClone;
        public IGameFamily FamilyClone { get; }
        public Dictionary<IGame, Guid> ClonedGames { get; } = new();
        public Dictionary<IGamePart, AutoPartId> ClonedParts { get; } = new();
    }
}
