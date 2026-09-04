using com.RADIO.Datinate.RMVC.Shared;
using datinate.shared;
using RMVC;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public class AutoGrouperModel : RModel
    {
        private static readonly Regex RoundBracketValue = new(@"\(\s*([^)]*?)\s*\)", RegexOptions.Compiled);

        private AutoGrouperOptions autoGrouperOptions = new AutoGrouperOptions();

        private IReadOnlyDictionary<DAT_GROUP_ENUM, IReadOnlyCollection<string>> partOwnerFlagsByGroup =
            new Dictionary<DAT_GROUP_ENUM, IReadOnlyCollection<string>>();

        private IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup = 
            new Dictionary<DAT_GROUP_ENUM, FlagFilterSet>();

        private AutoGrouperTraceStore? autoGroupTraceStore = null;
        public AutoGrouperTraceStore? AutoGroupTraceStore => autoGroupTraceStore;

        public IEnumerable<GameFamilyVO> Build(
            AutoGrouperOptions autoGroupOptions,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            IReadOnlyCollection<DatAdvanced> includeAdvs,
            IReadOnlyCollection<DatAdvanced> excludeAdvs)
        {
            var sw = Stopwatch.StartNew();

            this.autoGrouperOptions = autoGroupOptions;
            this.flagFilterSetByGroup = flagFilterSetByGroup;

            autoGroupTraceStore = new AutoGrouperTraceStore(flagFilterSetByGroup);

            partOwnerFlagsByGroup = NormaliseNamesDelegate.BuildPartOwnerFlagsByGroup(flagFilterSetByGroup);

            ApplyFilters(includeAdvs);

            ApplyPublisherAndRegion(includeAdvs);
            ApplyPublisherAndRegion(excludeAdvs);

            ApplyExcludes(includeAdvs, excludeAdvs);

            NormaliseNamesDelegate.ApplyNormalisedNames(
                includeAdvs,
                flagFilterSetByGroup,
                autoGroupOptions.UsePathSafeNormalisedNames);

            NormaliseNamesDelegate.ApplyNormalisedNames(
                excludeAdvs,
                flagFilterSetByGroup,
                autoGroupOptions.UsePathSafeNormalisedNames);

            IReadOnlyCollection<DatEntriesCollection> collections =
                CreateDatEntriesCollections(includeAdvs);

            DatEntriesCollection? collection = MergeCollections(collections);

            if (collection == null || collection.TotalEntryGroupCount == 0)
                return Array.Empty<GameFamilyVO>();

            var includeByKey = includeAdvs.ToDictionary(d => d.Key, d => d, StringComparer.Ordinal);
            var gameFamilies = ConvertCollection(collection, includeByKey);

            gameFamilies = GroupGameParts(gameFamilies, true).ToArray();

            gameFamilies = SetShallowCopyLinks(gameFamilies);

            foreach (var family in gameFamilies)
                family.FormaliseMembershipStatuses();

            gameFamilies = OrderGamesWithinFamilies(gameFamilies);

            gameFamilies = GameEntityNameBuilder.UpdateNames(gameFamilies, flagFilterSetByGroup);

            sw.Stop();
            Debug.WriteLine($"[TIMING] AutoGroup time: {sw.Elapsed}");

            gameFamilies =
                gameFamilies.OrderBy(f => f.GetFamilyDisplayName(), StringComparer.OrdinalIgnoreCase).ToArray();

            return gameFamilies;
        }

        private GameFamilyVO[] OrderGamesWithinFamilies(GameFamilyVO[] families)
        {
            if (!autoGrouperOptions.OrderGamesByInclusionState || families.Length == 0)
                return families;

            foreach (var family in families)
            {
                var games = family.Games.ToList();
                if (games.Count < 2)
                    continue;

                var changed = false;
                var runStart = 0;

                while (runStart < games.Count)
                {
                    var runGroup = games[runStart].PrimaryPart.PartOwnerInfo.DatGroupEnum;
                    var runEnd = runStart + 1;

                    while (runEnd < games.Count &&
                           games[runEnd].PrimaryPart.PartOwnerInfo.DatGroupEnum == runGroup)
                    {
                        runEnd++;
                    }

                    var runLength = runEnd - runStart;
                    if (runLength > 1)
                    {
                        var orderedRun = new List<(GameVO Game, int MembershipOrder, int OriginalIndex)>(runLength);

                        for (var i = runStart; i < runEnd; i++)
                        {
                            orderedRun.Add((games[i], GetGameMembershipOrder(games[i]), i));
                        }

                        orderedRun.Sort(static (a, b) =>
                        {
                            var compare = a.MembershipOrder.CompareTo(b.MembershipOrder);
                            if (compare != 0)
                                return compare;

                            return a.OriginalIndex.CompareTo(b.OriginalIndex);
                        });

                        for (var i = 0; i < orderedRun.Count; i++)
                        {
                            var targetIndex = runStart + i;
                            if (!ReferenceEquals(games[targetIndex], orderedRun[i].Game))
                            {
                                games[targetIndex] = orderedRun[i].Game;
                                changed = true;
                            }
                        }
                    }

                    runStart = runEnd;
                }

                if (changed)
                    family.ReplaceGames(games);
            }

            return families;
        }

        private static int GetGameMembershipOrder(GameVO game)
        {
            var worstOrder = -1;

            foreach (var rawPart in game.GetGameParts(false))
            {
                if (rawPart is not GamePartVO part)
                    continue;

                if (part.IsShallowPartReference)
                    continue;

                var partOrder = GetMembershipOrder(part.MembershipStatusEnum);
                if (partOrder > worstOrder)
                {
                    worstOrder = partOrder;

                    if (worstOrder == 5)
                        break;
                }
            }

            if (worstOrder >= 0)
                return worstOrder;

            if (game.PrimaryPart is GamePartVO primaryPart)
                return GetMembershipOrder(primaryPart.MembershipStatusEnum);

            return 5;
        }

        private static int GetMembershipOrder(DAT_GROUPER_MEMBERSHIP_ENUM membershipStatusEnum)
        {
            return membershipStatusEnum switch
            {
                DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_EXPLICIT => 0,
                DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_IMPLICIT => 1,
                DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_RELATIVE => 2,
                DAT_GROUPER_MEMBERSHIP_ENUM.EXCLUDE_RELATIVE => 3,
                DAT_GROUPER_MEMBERSHIP_ENUM.EXCLUDE_EXPLICIT => 4,
                _ => 5
            };
        }
        private GameFamilyVO[] SetShallowCopyLinks(IEnumerable<GameFamilyVO> families)
        {
            var list = families.ToList();

            foreach (var family in list)
            {
                var games = family.Games.ToList();
                if (games.Count < 2)
                    continue;

                var primaryPartsByGame = new GamePartVO[games.Count][];
                var partsWithAliasesByGame = new GamePartVO[games.Count][];
                var fingerprintsByGame = new HashSet<string>[games.Count];

                for (var i = 0; i < games.Count; i++)
                {
                    var g = games[i];

                    primaryPartsByGame[i] = g.GetGameParts(false).Cast<GamePartVO>().ToArray();
                    partsWithAliasesByGame[i] = g.GetGameParts(true).Cast<GamePartVO>().ToArray();

                    fingerprintsByGame[i] = new HashSet<string>(
                        g.Parts.Select(p => p.RomsFingerprint),
                        StringComparer.Ordinal);
                }

                for (var ai = 0; ai < games.Count; ai++)
                {
                    var gameA = games[ai];
                    var partsA = primaryPartsByGame[ai];

                    for (var bi = 0; bi < games.Count; bi++)
                    {
                        if (ai == bi)
                            continue;

                        var partsB = partsWithAliasesByGame[bi];
                        var fpA = fingerprintsByGame[ai];

                        for (var pa = 0; pa < partsA.Length; pa++)
                        {
                            var partA = partsA[pa];

                            for (var pb = 0; pb < partsB.Length; pb++)
                            {
                                var partB = partsB[pb];

                                if (fpA.Contains(partB.RomsFingerprint))
                                    continue;

                                if (!partA.IsPartOwnerMatch(partB, false))
                                    continue;

                                if (gameA.TryAddShallowPart(partB))
                                    fpA.Add(partB.RomsFingerprint);
                            }
                        }
                    }
                }
                family.ReplaceGames(games);
            }
            return list.ToArray();
        }
        private GameFamilyVO[] ConvertCollection(
            DatEntriesCollection collection,
            IReadOnlyDictionary<string, DatAdvanced> includeByKey)
        {
            var families = new List<GameFamilyVO>();

            DatAdvanced GetAdv(DatGameVO g)
            {
                if (!collection.EntryDatKeyDic.TryGetValue(g, out var datKey))
                    throw new KeyNotFoundException($"EntryDatKeyDic missing key for DatGameVO instance. Name='{g.Name}', Fingerprint='{g.Fingerprint}'.");

                if (!includeByKey.TryGetValue(datKey, out var adv))
                    throw new KeyNotFoundException($"includeByKey missing key for DatAdvanced '{datKey}'.");

                return adv;
            }

            foreach (var entryGroup in collection.EntryGroups)
            {
                var datGamesList = entryGroup.FingerprintDatGamesDic.Values
                    .OrderBy(list => collection.GetDatPriority(list[0]))
                    .ToArray();

                if (datGamesList.Length == 0)
                    continue;

                var firstPriorityGame = datGamesList[0][0];

                string familyName = BuildFamilyName(firstPriorityGame);

                var family = new GameFamilyVO(familyName);

                for (var i = 0; i < datGamesList.Length; i++)
                {
                    var priorityGameThenAliases = datGamesList[i];
                    var priorityGame = priorityGameThenAliases[0];

                    var aliases = priorityGameThenAliases.Count > 1
                        ? priorityGameThenAliases.Skip(1).ToArray()
                        : Array.Empty<DatGameVO>();

                    family.AddGame(ConvertToGame(priorityGame, aliases, GetAdv));
                }

                // TODO: Where are we with this? Working on empty chd rom entries:
                if (family.GetTotalRomsCount() > 0)
                    families.Add(family);
            }

            return families.ToArray();
        }
        private static string BuildFamilyName(DatGameVO datGame)
        {
            if (!string.IsNullOrWhiteSpace(datGame.NamePublisherKey))
                return datGame.NamePublisherKey;

            return datGame.FlaglessName;
        }

        private static string BuildGameName(DatGameVO datGame)
        {
            if (!string.IsNullOrWhiteSpace(datGame.NameRegionKey))
                return datGame.NameRegionKey;

            if (!string.IsNullOrWhiteSpace(datGame.NamePublisherKey))
                return datGame.NamePublisherKey;

            return datGame.FlaglessName;
        }

        private GameVO ConvertToGame(
            DatGameVO priorityDatGame,
            DatGameVO[] aliasDatGames,
            Func<DatGameVO, DatAdvanced> getAdv)
        {
            var priorityAdv = getAdv(priorityDatGame);

            string priorityGameName = BuildGameName(priorityDatGame);

            var gamePart = ConvertToGamePart(
                priorityDatGame,
                priorityAdv.Key,
                priorityAdv.DatGroupEnum,
                priorityAdv.DatFriendlyReference);

            var gameSource = new GameVO(gamePart, priorityGameName);

            for (var i = 0; i < aliasDatGames.Length; i++)
            {
                var aliasGame = aliasDatGames[i];
                var aliasAdv = getAdv(aliasGame);

                gamePart = ConvertToGamePart(
                    aliasGame,
                    aliasAdv.Key,
                    aliasAdv.DatGroupEnum,
                    aliasAdv.DatFriendlyReference);

                _ = gameSource.AddGamePartAlias(gamePart);
            }

            return gameSource;
        }

        private IEnumerable<GameFamilyVO> GroupGameParts(IEnumerable<GameFamilyVO> families, bool allowSoftPartsMatch)
        {
            foreach (var family in families)
            {
                var games = family.Games.ToList();
                if (games.Count < 2)
                    continue;

                var byOwnerKey = new Dictionary<string, GameVO>(StringComparer.Ordinal);
                var remove = new HashSet<GameVO>();

                for (var i = 0; i < games.Count; i++)
                {
                    var g = games[i];
                    if (remove.Contains(g))
                        continue;

                    var key = GetPrimaryOwnerKey(g);
                    if (key == null)
                        continue;

                    if (!byOwnerKey.TryGetValue(key, out var winner))
                    {
                        byOwnerKey.Add(key, g);
                        continue;
                    }

                    if (object.ReferenceEquals(winner, g))
                        continue;

                    if (ShouldBlockAbsorbOnPcloneIdentity(winner, g))
                        continue;

                    winner.AbsorbGameAsParts(g);
                    remove.Add(g);
                }

                if (remove.Count > 0)
                    games.RemoveAll(g => remove.Contains(g));

                family.ReplaceGames(games);
            }

            return families;

            static string? GetPrimaryOwnerKey(GameVO game)
            {
                var info = game.PrimaryPart.PartOwnerInfo;

                var datRef = info.DatFriendlyReference ?? string.Empty;
                var owner = info.IsMameOrMameSl ? info.MameName : info.PartOwnerName;
                if (string.IsNullOrWhiteSpace(owner))
                    return null;

                return ((int)info.DatGroupEnum).ToString() + "|" + datRef.ToLowerInvariant() + "|" + owner.ToLowerInvariant();

            }
        }


        // check here and CANCEL match if would-be absorber has a primary
        // game PartOwnerInfo with matching dat group and ref AND 
        // low.PrimaryPart.PartOwnerInfo.IsMameOrMameSl is true
        // AND respective PartOwnerInfo.MameName values differ
        /// <summary>
        /// Prevents false “absorb” merges for DAT formats that define a formal parent/clone identity (MAME, MAME SL, No-Intro PClone). 
        /// If high and low both contain primary parts from the same DAT file (DatGroupEnum + DatFriendlyReference) but with 
        /// different authoritative MameName values, they are distinct titles and must remain separate; only shallow/alias linking should apply.
        /// Aliases are intentionally ignored here so that legitimate cross-references do not block aggregation.
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        private static bool ShouldBlockAbsorbOnPcloneIdentity(GameVO high, GameVO low)
        {
            var highOwnersByDat = new Dictionary<string, string>(StringComparer.Ordinal);

            for (var i = 0; i < high.Parts.Length; i++)
            {
                var info = high.Parts[i].PartOwnerInfo;
                if (!info.IsMameOrMameSl) continue;

                string datRef = info.DatFriendlyReference ?? string.Empty;
                var mameName = info.MameName;

                if (string.IsNullOrWhiteSpace(mameName)) continue;

                var key = BuildDatKey(info.DatGroupEnum, datRef);
                var owner = mameName.ToLowerInvariant();

                if (!highOwnersByDat.TryGetValue(key, out var existing))
                {
                    highOwnersByDat.Add(key, owner);
                    continue;
                }

                if (existing != owner) return true;
            }

            for (var i = 0; i < low.Parts.Length; i++)
            {
                var info = low.Parts[i].PartOwnerInfo;
                if (!info.IsMameOrMameSl) continue;

                string datRef = info.DatFriendlyReference ?? string.Empty;
                var mameName = info.MameName;

                if (string.IsNullOrWhiteSpace(mameName)) continue;

                var key = BuildDatKey(info.DatGroupEnum, datRef);
                var owner = mameName.ToLowerInvariant();

                if (highOwnersByDat.TryGetValue(key, out var highOwner) && highOwner != owner)
                    return true;
            }
            
            string BuildDatKey(DAT_GROUP_ENUM datGroupEnum, string datFriendlyReference)
            {
                return ((int)datGroupEnum).ToString() + "|" + datFriendlyReference.ToLowerInvariant();
            }
            return false;
        }
        private string? GetPartOwner(DatGameVO game, DAT_GROUP_ENUM datGroupEnum)
        {
            var flags = partOwnerFlagsByGroup.TryGetValue(datGroupEnum, out var f)
                ? f
                : Array.Empty<string>();

            var baseName = FlagStripper.NormaliseName(game.Name, flags);

            return string.Equals(game.Name, baseName, StringComparison.Ordinal) ? null : baseName;
        }

        private GamePartVO ConvertToGamePart(
            DatGameVO datGame,
            string datSourceID,
            DAT_GROUP_ENUM datGroupEnum,
            string datReference)
        {
            var partOwner = GetPartOwner(datGame, datGroupEnum);

            var part = new GamePartVO(
                datGame.Name,
                datSourceID,
                datGame,
                datGame.ExpressionActionEnum,
                datGame.Fingerprint,
                datGroupEnum,
                datReference,
                partOwner,
                datGame.MameLaunchName,
                false,
                GetMameSlPartName(datGame, datGroupEnum),
                datGame.Category);

            autoGroupTraceStore?.AttachPartReport(part, datGame, datGroupEnum, datSourceID, datReference, partOwner);

            return part;
        }

        private string? GetMameSlPartName(DatGameVO datGame, DAT_GROUP_ENUM datGroupEnum)
        {
            return datGroupEnum == DAT_GROUP_ENUM.MAME_SL ? datGame.PartOwnerName : null;
        }

        private DatEntriesCollection? MergeCollections(IReadOnlyCollection<DatEntriesCollection> collections)
        {
            if (collections.Count == 0)
                return null;

            var mainCollection = collections.FirstOrDefault(c => c.TotalEntryGroupCount > 0);
            if (mainCollection == null) return null;

            foreach (var collection in collections)
            {
                if (!object.ReferenceEquals(collection, mainCollection) &&
                    collection.TotalEntryGroupCount > 0)
                {
                    mainCollection.AbsorbEntries(collection);
                }
            }

            mainCollection.Finalise();

            return mainCollection;
        }

        private IReadOnlyCollection<DatEntriesCollection> CreateDatEntriesCollections(IReadOnlyCollection<DatAdvanced> includeAdvs)
        {
            var includeList = includeAdvs as IList<DatAdvanced> ?? includeAdvs.ToList();

            var datPriorityByDatKey = new Dictionary<string, int>(includeList.Count, StringComparer.Ordinal);
            for (var i = 0; i < includeList.Count; i++)
                datPriorityByDatKey[includeList[i].Key] = i;

            var collections = new List<DatEntriesCollection>(includeList.Count);

            for (var i = 0; i < includeList.Count; i++)
            {
                var datAdv = includeList[i];

                var collection = new DatEntriesCollection(
                    datAdv,
                    autoGrouperOptions,
                    datPriorityByDatKey,
                    autoGroupTraceStore);

                if (collection.TotalEntryGroupCount > 0)
                    collections.Add(collection);
            }

            return collections;
        }

        /**
         * NOTE: Only excluding individual files: not groups. Too risky.
         */
        private void ApplyExcludes(IReadOnlyCollection<DatAdvanced> includeAdvs, IReadOnlyCollection<DatAdvanced> excludeAdvs)
        {
            if (excludeAdvs.Count == 0)
                return;

            var excludeFingerprints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var excludeNamePublisherKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var excludeNameWithFlagsHash = new HashSet<string>(StringComparer.Ordinal);

            foreach (var datAdv in excludeAdvs)
            {
                foreach (var excludeEntry in datAdv.Entries)
                {
                    excludeFingerprints.Add(excludeEntry.Fingerprint);
                    excludeNameWithFlagsHash.Add(excludeEntry.Name);

                    if (!string.IsNullOrWhiteSpace(excludeEntry.NamePublisherKey))
                        excludeNamePublisherKeys.Add(excludeEntry.NamePublisherKey);
                }
            }

            foreach (var datAdv in includeAdvs)
            {
                var entriesToRemove = new List<string>();

                foreach (var includeEntry in datAdv.Entries)
                {
                    if (excludeFingerprints.Contains(includeEntry.Fingerprint))
                    {
                        entriesToRemove.Add(includeEntry.Name);
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(includeEntry.NamePublisherKey) &&
                        excludeNamePublisherKeys.Contains(includeEntry.NamePublisherKey))
                    {
                        entriesToRemove.Add(includeEntry.Name);
                        continue;
                    }

                    if (excludeNameWithFlagsHash.Contains(includeEntry.Name))
                        entriesToRemove.Add(includeEntry.Name);
                }

                if (entriesToRemove.Count > 0)
                {
                    Debug.WriteLine($"Excluding from: '{datAdv.Key}'.");
                    Debug.WriteLine("   ... Removing: " + string.Join(", ", entriesToRemove));
                }

                datAdv.RemoveEntriesByGameName(entriesToRemove.ToArray());
            }
        }

        /**
         * This is viable - a dat's format does not dictate origin, 
         */
        private void ApplyPublisherAndRegion(IReadOnlyCollection<DatAdvanced> dats)
        {
            foreach (var datAdv in dats)
            {
                switch (datAdv.DatGroupEnum)
                {
                    case DAT_GROUP_ENUM.TOSEC:
                    case DAT_GROUP_ENUM.TOSEC_ISO:
                    case DAT_GROUP_ENUM.TOSEC_PIX:
                        foreach (var entry in datAdv.Entries)
                            entry.SetPublisher(DatFilterHelper.GetPublisherTosec(entry.Name));
                        break;

                    case DAT_GROUP_ENUM.NO_INTRO:
                    case DAT_GROUP_ENUM.REDUMP:
                        var regions = GetRegionFlagsOrEmpty(datAdv.DatGroupEnum);

                        foreach (var entry in datAdv.Entries)
                        {
                            var candidate = RoundBracketValue.Matches(entry.Name)
                                .Select(m => m.Value.Trim())
                                .FirstOrDefault(v => FlagStripper.MatchesAnyValue(v, regions));

                            if (candidate != null)
                                entry.SetRegion(candidate.Trim().TrimStart('(').TrimEnd(')'));
                        }
                        break;
                }
            }
        }

        private void ApplyFilters(IReadOnlyCollection<DatAdvanced> datAdvs)
        {
            foreach (var datAdv in datAdvs)
            {
                if (!datAdv.HasFilters)
                {
                    foreach (var entry in datAdv.Dat.Entries)
                    {
                        entry.ExpressionActionEnum =
                            DatFilterHelper.EXPRESSION_ACTION_ENUM.NO_FILTER;
                    }
                    continue;
                }

                var gameNames = datAdv.Dat.GetEntryNames();
                var managedList = new ManagedList(datAdv.Dat.Entries.ToArray(), datAdv.Filters);

                SortedDictionary<string, DatFilterHelper.EXPRESSION_ACTION_ENUM> dic =
                    managedList.GetAll();

                autoGroupTraceStore?.AddManagedListReport(
                    datAdv.Key,
                    managedList.GetAllDecisionReports(true));

                foreach (var entry in datAdv.Dat.Entries)
                { 
                    entry.ExpressionActionEnum = dic[entry.Name];
                }
            }
        }
        
        private IReadOnlyCollection<string> GetRegionFlagsOrEmpty(DAT_GROUP_ENUM datGroupEnum)
        {
            return flagFilterSetByGroup.TryGetValue(datGroupEnum, out var set)
                ? set.RegionFlags
                : Array.Empty<string>();
        }

        // TODO: there's some promising tech here: 
        private GameFamilyVO[] PostProcess(GameFamilyVO[] gameFamilyVOs)
        {
            if (gameFamilyVOs.Length == 0)
                return gameFamilyVOs;

            //foreach (var family in gameFamilyVOs)
            //{
            //    var singlePartSourcesByChecksum = new Dictionary<string, GameVO>();
            //    var ownerGameCandidates = new List<GameVO>();

            //    foreach (var source in family.GameSourceVOs)
            //    {
            //        var parts = source.GamePartVOs;
            //        if (parts.Length == 0)
            //            continue;

            //        bool canBeCandidate = true;
            //        if (parts.Length == 1 && parts[0].IsKnownToBeOneOfManyTosecParts && parts[0].GetChecksums().Length == 1)
            //        {
            //            canBeCandidate = false;
            //            var checksums = parts[0].GetChecksums();
            //            if (checksums.Length == 1)
            //            {
            //                var checksum = checksums[0];
            //                singlePartSourcesByChecksum.Add(checksum, source);
            //            }
            //        }
            //        if (canBeCandidate)
            //        {
            //            ownerGameCandidates.Add(source);
            //        }
            //    }

            //    if (singlePartSourcesByChecksum.Count == 0 || ownerGameCandidates.Count == 0)
            //        continue;


            //    var sourcesToRemove = new List<GameVO>();

            //    foreach (var pair in singlePartSourcesByChecksum)
            //    {
            //        var checksum = pair.Key;
            //        var singleSource = pair.Value;

            //        foreach (var candidate in ownerGameCandidates)
            //        {
            //            if (!candidate.AnyMemberPartContainsRomChecksum(checksum))
            //                continue;

            //            sourcesToRemove.Add(singleSource);

            //            var orphanParts = singleSource.GetGameParts(false, false);
            //            if (orphanParts.Length > 0)
            //                family.OrphanedGameRoms.Add(orphanParts[0]);

            //            break;
            //        }
            //    }

            //    if (sourcesToRemove.Count > 0)
            //    {
            //        family.RemoveGameSources(sourcesToRemove);
            //    }
            //}

            return gameFamilyVOs;
        }
    }
}
