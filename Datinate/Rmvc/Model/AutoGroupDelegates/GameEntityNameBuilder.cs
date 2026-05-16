using com.RADIO.Datinate.RMVC.Shared;
using RadioLibCore.RadioDat;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC
{
    public static class GameEntityNameBuilder
    {
        private static readonly Regex MultiSpaceRegex = new(@"\s{2,}", RegexOptions.Compiled);
        private static string[] BuildUpdatedGameNames(
            NameBuildInput[] games,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup)
        {
            /*
             * Naming rules:
             *
             * - Games are processed strictly in family order.
             * - The first game's DAT group becomes the "top level" DAT group anchor for the family.
             * - Earlier games keep the cleaner names; later games absorb the uglier disambiguation.
             * - The family display name itself is NOT part of child-game name reservation.
             *   Parent/header display and child game naming are treated as separate display domains.
             *
             * Additional reservation rule:
             *
             * - Some non-OG sources (for example MAME_SL) do not carry formal region flags.
             * - Because of that, they can otherwise "steal" a plain flagless/root name that really belongs to an OG/top-level
             *   game which already establishes that root through a regionalized title.
             * - To avoid that, any top-level game which has both:
             *      1. a non-empty NameRegionKey
             *      2. a non-empty FlaglessName
             *   reserves that FlaglessName as an OG root for the whole family.
             * - Non-top-level games are not allowed to claim that reserved root in plain form.
             *   So if a non-top-level game's FlaglessName or stripped startName matches one of those reserved OG roots,
             *   it must skip the plain version and jump straight to the DAT-group-suffixed path.
             *
             * Additional ordered sibling root-ownership rule:
             *
             * - Independently of the top-level reservation rule above, family order also matters.
             * - Once an earlier sibling appears with a given exact FlaglessName/root, that plain root is considered consumed
             *   for all later siblings with that same exact FlaglessName spelling.
             * - This is true even if the earlier sibling did NOT end up displaying as the plain root.
             *   For example, an earlier sibling might display as:
             *      "Aconcagua [T_EN]"
             *      "Aconcagua (Japan)"
             *      "Aconcagua (Japan)(Demo)"
             * - Even then, later siblings with the same exact flagless root "Aconcagua" are no longer allowed to claim the
             *   bare plain root "Aconcagua".
             * - If a later sibling has a slightly different flagless spelling, it is treated independently and may still use
             *   its own plain root if otherwise valid.
             *
             * Special verbose-naming-exempt rule:
             *
             * - Some DAT groups intentionally do NOT participate in the normal plain-name / verbose-name flow.
             * - For those groups, we do not use:
             *      * plain NameRegionKey
             *      * plain FlaglessName
             *      * "startName with all flags except part"
             * - Instead they always go straight to the DAT-group-suffixed path:
             *      "{flagless-or-best-base} [T_EN]"
             *      "{flagless-or-best-base} [T_EN][1]"
             *      "{flagless-or-best-base} [T_EN][2]"
             * - This keeps those groups explicit and avoids them consuming a cleaner shared root invisibly.
             *
             * For normal (non-exempt) games:
             *
             * 1. Try NameRegionKey first.
             *    - If present and unique amongst all previously assigned game names, use it as-is.
             *
             * 2. If NameRegionKey is already owned by an earlier same-bucket sibling, do NOT jump straight to numbering yet.
             *    - First try the more verbose form:
             *         startName with all part flags removed, but all other flags preserved.
             *    - Example:
             *         "Some Game (Japan)(Disc 1)"        -> "Some Game (Japan)"
             *         "Some Game (Japan)(Disc 1)(Demo)"  -> "Some Game (Japan)(Demo)"
             *    - If that verbose form is unique, use it.
             *    - If it still is not unique, then number from that exact NameRegionKey as before:
             *         "Some Game (Japan) [1]"
             *         "Some Game (Japan) [2]"
             *
             * 3. Otherwise try FlaglessName.
             *    - If present and unique, use it as-is.
             *    - But for non-top-level games, this plain FlaglessName option is blocked if that root has already been
             *      reserved by a top-level regionalized game.
             *    - It is also blocked if an earlier sibling with the same exact FlaglessName has already appeared earlier
             *      in family order, even if that earlier sibling displayed in a more specific form.
             *
             * 4. Otherwise try the general disambiguation base:
             *    - startName with all part flags removed, then double spaces collapsed.
             *    - If that collapses to empty/whitespace, fall back to FlaglessName, and then to trimmed startName.
             *    - However, if that base collapses to the same exact plain FlaglessName root that has already been consumed,
             *      that plain-root form is still considered unavailable.
             *
             * 5. If that disambiguation base clashes, or if the plain root is reserved/consumed, decide the fallback style
             *    based on ownership of the collision:
             *    - if the clash/reservation belongs to a different naming bucket, prefer DAT-group suffixing first:
             *         "{base} [NO_INTRO]"
             *         "{base} [REDUMP]"
             *         "{base} [MAME_SL]"
             *    - if the clash belongs to the same naming bucket, skip DAT-group suffixing and go straight to numbering:
             *         "{base} [1]"
             *         "{base} [2]"
             *
             * 6. Final fallback is numbering on the current best base:
             *    - "{base} [1]"
             *    - "{base} [NO_INTRO][1]"
             *    - "{base} [MAME_SL][1]"
             *
             * DAT-group bucket note:
             *
             * - Bucket comparison is generic.
             * - Raw enum equality wins first.
             * - Otherwise, DAT groups that share the same FlagFilterSet instance are treated as the same logical naming bucket.
             *
             * The important intent is:
             * - do not burden the user with DAT-group text unless we have to,
             * - except for explicitly exempt DAT groups where DAT-group text is always required,
             * - use the "all flags except part" form as a last clean human-readable step before numbering on same-bucket
             *   NameRegion collisions,
             * - do not allow a later sibling to claim a plain flagless root once an earlier sibling with that exact same
             *   flagless spelling has already established ownership of it in any form,
             * - use DAT-group suffixing only when we have to distinguish across naming buckets,
             * - and when an OG/top-level regionalized game has already established a plain root, non-OG games must not steal it.
             */

            if (games.Length == 0)
                return Array.Empty<string>();

            var updatedGameNames = new string[games.Length];
            var assignedNameOwnerGroups = new Dictionary<string, DAT_GROUP_ENUM>(StringComparer.OrdinalIgnoreCase);

            DAT_GROUP_ENUM topLevelDatGroup = games[0].DatGroup;

            var reservedTopLevelFlaglessRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var game in games)
            {
                if (!IsSameNamingBucket(game.DatGroup, topLevelDatGroup, flagFilterSetByGroup))
                    continue;

                if (!string.IsNullOrWhiteSpace(game.NameRegion) &&
                    !string.IsNullOrWhiteSpace(game.NameFlagless))
                {
                    reservedTopLevelFlaglessRoots.Add(game.NameFlagless.Trim());
                }
            }

            var consumedEarlierSiblingFlaglessRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < games.Length; i++)
            {
                var game = games[i];
                DAT_GROUP_ENUM datGroup = game.DatGroup;

                string startName = game.StartName;
                string? nameRegion = game.NameRegion;
                string nameFlagless = game.NameFlagless;
                IReadOnlyCollection<string> partFlags = game.PartFlags;

                string baseName = GetDisambiguationBaseName(startName, nameFlagless, partFlags);
                bool verboseNamingExempt = GetDatGroupIsVerboseNamingExempt(datGroup);

                if (verboseNamingExempt)
                {
                    string exemptBaseName = GetVerboseNamingExemptBaseName(startName, nameFlagless, partFlags);

                    updatedGameNames[i] = GetDatGroupSuffixedOrNumberedName(
                        exemptBaseName,
                        datGroup,
                        assignedNameOwnerGroups);

                    RegisterConsumedPlainFlaglessRoot(nameFlagless, consumedEarlierSiblingFlaglessRoots);
                    continue;
                }

                if (TryAssignName(nameRegion, datGroup, assignedNameOwnerGroups))
                {
                    updatedGameNames[i] = nameRegion!.Trim();
                    RegisterConsumedPlainFlaglessRoot(nameFlagless, consumedEarlierSiblingFlaglessRoots);
                    continue;
                }

                bool exactNameRegionCollisionSameBucket =
                    ShouldNumberFromExactNameRegionCollision(
                        nameRegion,
                        datGroup,
                        assignedNameOwnerGroups,
                        flagFilterSetByGroup);

                if (exactNameRegionCollisionSameBucket)
                {
                    bool baseNameConsumesBlockedPlainRoot =
                        IsConsumedEarlierSiblingPlainRootCandidate(
                            baseName,
                            nameFlagless,
                            consumedEarlierSiblingFlaglessRoots);

                    if (!baseNameConsumesBlockedPlainRoot &&
                        TryAssignName(baseName, datGroup, assignedNameOwnerGroups))
                    {
                        updatedGameNames[i] = baseName;
                        RegisterConsumedPlainFlaglessRoot(nameFlagless, consumedEarlierSiblingFlaglessRoots);
                        continue;
                    }

                    updatedGameNames[i] = GetNextUniqueNumberedName(
                        nameRegion!.Trim(),
                        datGroup,
                        assignedNameOwnerGroups,
                        appendWithoutSpace: false);

                    RegisterConsumedPlainFlaglessRoot(nameFlagless, consumedEarlierSiblingFlaglessRoots);
                    continue;
                }

                bool plainFlaglessReservedForNonTopLevel =
                    IsPlainRootReservedForNonTopLevel(
                        datGroup,
                        topLevelDatGroup,
                        nameFlagless,
                        reservedTopLevelFlaglessRoots,
                        flagFilterSetByGroup);

                bool plainFlaglessConsumedByEarlierSibling =
                    IsConsumedEarlierSiblingPlainRoot(nameFlagless, consumedEarlierSiblingFlaglessRoots);

                bool plainFlaglessBlocked =
                    plainFlaglessReservedForNonTopLevel ||
                    plainFlaglessConsumedByEarlierSibling;

                if (!plainFlaglessBlocked &&
                    TryAssignName(nameFlagless, datGroup, assignedNameOwnerGroups))
                {
                    updatedGameNames[i] = nameFlagless.Trim();
                    RegisterConsumedPlainFlaglessRoot(nameFlagless, consumedEarlierSiblingFlaglessRoots);
                    continue;
                }

                bool plainBaseReservedForNonTopLevel =
                    IsPlainRootReservedForNonTopLevel(
                        datGroup,
                        topLevelDatGroup,
                        baseName,
                        reservedTopLevelFlaglessRoots,
                        flagFilterSetByGroup);

                bool plainBaseConsumedByEarlierSibling =
                    IsConsumedEarlierSiblingPlainRootCandidate(
                        baseName,
                        nameFlagless,
                        consumedEarlierSiblingFlaglessRoots);

                bool plainBaseBlocked =
                    plainBaseReservedForNonTopLevel ||
                    plainBaseConsumedByEarlierSibling;

                if (!plainBaseBlocked &&
                    TryAssignName(baseName, datGroup, assignedNameOwnerGroups))
                {
                    updatedGameNames[i] = baseName;
                    RegisterConsumedPlainFlaglessRoot(nameFlagless, consumedEarlierSiblingFlaglessRoots);
                    continue;
                }

                if (ShouldPreferDatGroupSuffix(
                    datGroup,
                    baseName,
                    plainBaseBlocked || plainFlaglessBlocked,
                    topLevelDatGroup,
                    reservedTopLevelFlaglessRoots,
                    assignedNameOwnerGroups,
                    flagFilterSetByGroup))
                {
                    updatedGameNames[i] = GetDatGroupSuffixedOrNumberedName(
                        baseName,
                        datGroup,
                        assignedNameOwnerGroups);
                }
                else
                {
                    updatedGameNames[i] = GetNextUniqueNumberedName(
                        baseName,
                        datGroup,
                        assignedNameOwnerGroups,
                        appendWithoutSpace: false);
                }

                RegisterConsumedPlainFlaglessRoot(nameFlagless, consumedEarlierSiblingFlaglessRoots);
            }

            return updatedGameNames;
        }

        private static string GetDisambiguationBaseName(
            string startName,
            string nameFlagless,
            IReadOnlyCollection<string> partFlags)
        {
            string baseName = RemovePartFlags(startName, partFlags);

            if (string.IsNullOrWhiteSpace(baseName))
            {
                if (!string.IsNullOrWhiteSpace(nameFlagless))
                    baseName = nameFlagless;
                else
                    baseName = startName;
            }

            return baseName.Trim();
        }

        private static string GetVerboseNamingExemptBaseName(
            string startName,
            string nameFlagless,
            IReadOnlyCollection<string> partFlags)
        {
            if (!string.IsNullOrWhiteSpace(nameFlagless))
                return nameFlagless.Trim();

            return GetDisambiguationBaseName(startName, nameFlagless, partFlags);
        }

        private static bool IsConsumedEarlierSiblingPlainRoot(
            string candidateFlaglessRoot,
            HashSet<string> consumedEarlierSiblingFlaglessRoots)
        {
            if (string.IsNullOrWhiteSpace(candidateFlaglessRoot))
                return false;

            return consumedEarlierSiblingFlaglessRoots.Contains(candidateFlaglessRoot.Trim());
        }

        private static bool IsConsumedEarlierSiblingPlainRootCandidate(
            string candidateName,
            string candidateFlaglessRoot,
            HashSet<string> consumedEarlierSiblingFlaglessRoots)
        {
            if (string.IsNullOrWhiteSpace(candidateName) ||
                string.IsNullOrWhiteSpace(candidateFlaglessRoot))
                return false;

            if (!string.Equals(
                candidateName.Trim(),
                candidateFlaglessRoot.Trim(),
                StringComparison.OrdinalIgnoreCase))
                return false;

            return consumedEarlierSiblingFlaglessRoots.Contains(candidateFlaglessRoot.Trim());
        }

        private static void RegisterConsumedPlainFlaglessRoot(
            string candidateFlaglessRoot,
            HashSet<string> consumedEarlierSiblingFlaglessRoots)
        {
            if (string.IsNullOrWhiteSpace(candidateFlaglessRoot))
                return;

            consumedEarlierSiblingFlaglessRoots.Add(candidateFlaglessRoot.Trim());
        }
        private sealed record NameBuildInput(
            DAT_GROUP_ENUM DatGroup,
            string StartName,
            string? NameRegion,
            string NameFlagless,
            IReadOnlyCollection<string> PartFlags);

        public static GameFamilyVO[] UpdateNames(
            IEnumerable<GameFamilyVO> families,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup)
        {
            foreach (var family in families)
                _ = UpdateNames(family, flagFilterSetByGroup);

            return families.ToArray();
        }

        public static BaseGameFamily UpdateNames(
            BaseGameFamily family,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary)
        {
            return UpdateNames(new BaseGameFamily[] { family }, flagFilterSetByGroup, softwareIdDatGroupEnumDictionary).First();
        }

        private static IEnumerable<BaseGameFamily> UpdateNames(
            IEnumerable<BaseGameFamily> families,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary)
        {
            var updatedFamilies = new List<BaseGameFamily>();

            foreach (var family in families)
            {
                var games = family.GetAllGames();

                if (games.Length == 0)
                    continue;

                string[] updatedGameNames = BuildUpdatedGameNamesInternal(games, flagFilterSetByGroup, softwareIdDatGroupEnumDictionary);

                var updatedGames = new List<IGame>(games.Length);

                for (int i = 0; i < games.Length; i++)
                {
                    var game = games[i];

                    updatedGames.Add(new BaseGame(
                        updatedGameNames[i],
                        game.GetGameParts(false),
                        game.GetLaunchName()));
                }

                updatedFamilies.Add(new BaseGameFamily(
                    DatinateHelper.GetFlaglessName(updatedGames.First().GetNameWithoutExt()),
                    updatedGames.ToArray(),
                    family.GetDescriptors(),
                    family.GetResourceCollection(),
                    family.GetIgnored(),
                    family.GetComment()));
            }

            return updatedFamilies;
        }

        public static GameFamilyVO UpdateNames(
            GameFamilyVO family,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup)
        {
            var games = family.Games;
            if (games.Length == 0)
                return family;

            string[] updatedGameNames = BuildUpdatedGameNames(games, flagFilterSetByGroup);

            for (int i = 0; i < games.Length; i++)
                games[i].UpdateGameName(updatedGameNames[i]);

            family.SetFamilyDisplayName(BuildAutomatedFamilyDisplayName(games.First()));

            return family;
        }

        public static string[] BuildUpdatedGameNames(
            GameVO[] games,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup)
        {
            var inputs = new NameBuildInput[games.Length];

            for (int i = 0; i < games.Length; i++)
            {
                GamePartVO part = games[i].Parts[0];
                DAT_GROUP_ENUM datGroup = part.DatGroupEnum;

                string startName = NormaliseNamesDelegate.NormaliseName((part.DatGameSource.Name ?? string.Empty).Trim());

                string? nameRegion = part.DatGameSource.NameRegionKey?.Trim();
                if (nameRegion != null)
                    nameRegion = NormaliseNamesDelegate.NormaliseName(nameRegion);

                string nameFlagless = NormaliseNamesDelegate.NormaliseName(
                    (part.DatGameSource.FlaglessName ?? string.Empty).Trim());

                IReadOnlyCollection<string> partFlags =
                    flagFilterSetByGroup.TryGetValue(datGroup, out var filterSet)
                        ? filterSet.PartFlags
                        : Array.Empty<string>();

                inputs[i] = new NameBuildInput(
                    datGroup,
                    startName,
                    nameRegion,
                    nameFlagless,
                    partFlags);
            }

            return BuildUpdatedGameNames(inputs, flagFilterSetByGroup);
        }
        public static Dictionary<IGame, string> BuildUpdatedGameNames(
            IGame[] games,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary)
        {
            Dictionary<IGame, string> dic = new Dictionary<IGame, string>();

            var refinedNames = BuildUpdatedGameNamesInternal(games, flagFilterSetByGroup, softwareIdDatGroupEnumDictionary);

            for (int i = 0; i < refinedNames.Length; i++)
            {
                var game = games[i];
                var name = refinedNames[i];
                dic[game] = name;
            }

            return dic;
        }
        private static string[] BuildUpdatedGameNamesInternal(
            IGame[] games,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup,
            IReadOnlyDictionary<string, DAT_GROUP_ENUM> softwareIdDatGroupEnumDictionary)
        {
            var inputs = new NameBuildInput[games.Length];

            for (int i = 0; i < games.Length; i++)
            {
                var part = games[i].GetGameParts(false)[0];

                string startName = NormaliseNamesDelegate.NormaliseName((part.GetName() ?? string.Empty).Trim());

                DAT_GROUP_ENUM datGroup = DAT_GROUP_ENUM.NOT_SET;

                if (softwareIdDatGroupEnumDictionary.TryGetValue(part.GetDirectoryId()!, out var definition))
                    datGroup = definition;
                else
                    Debug.WriteLine(typeof(GameEntityNameBuilder) + ": WARNING: Dat Group not found for part: " + part.GetName());

                IReadOnlyCollection<string> partFlags =
                    flagFilterSetByGroup.TryGetValue(datGroup, out var filterSet)
                        ? filterSet.PartFlags
                        : Array.Empty<string>();

                string startNameWithoutPartFlags = RemovePartFlags(startName, partFlags);

                string nameFlagless = NormaliseNamesDelegate.NormaliseName(
                    DatinateHelper.GetFlaglessName(startNameWithoutPartFlags).Trim());

                inputs[i] = new NameBuildInput(
                    datGroup,
                    startName,
                    null,
                    nameFlagless,
                    partFlags);
            }

            return BuildUpdatedGameNames(inputs, flagFilterSetByGroup);
        }

        private static bool GetDatGroupIsVerboseNamingExempt(DAT_GROUP_ENUM datGroupEnum)
        {
            return datGroupEnum == DAT_GROUP_ENUM.T_EN;
        }

        private static string BuildAutomatedFamilyDisplayName(GameVO game)
        {
            var firstPart = game.Parts[0];

            string? namePublisherKey = firstPart.DatGameSource.NamePublisherKey?.Trim();

            if (!string.IsNullOrWhiteSpace(namePublisherKey))
                return NormaliseNamesDelegate.NormaliseName(namePublisherKey);

            return NormaliseNamesDelegate.NormaliseName(
                DatinateHelper.GetFlaglessName(game.GetNameWithoutExt()).Trim());
        }

        private static string CleanDatGroupEnumName(DAT_GROUP_ENUM datGroup)
        {
            return datGroup.ToString().Replace("_", " ").Trim();
        }

        private static bool ShouldNumberFromExactNameRegionCollision(
            string? nameRegion,
            DAT_GROUP_ENUM datGroup,
            Dictionary<string, DAT_GROUP_ENUM> assignedNameOwnerGroups,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup)
        {
            if (string.IsNullOrWhiteSpace(nameRegion))
                return false;

            if (!TryGetAssignedNameOwnerGroup(nameRegion, assignedNameOwnerGroups, out var existingOwnerGroup))
                return false;

            return IsSameNamingBucket(datGroup, existingOwnerGroup, flagFilterSetByGroup);
        }

        private static bool ShouldPreferDatGroupSuffix(
            DAT_GROUP_ENUM datGroup,
            string baseName,
            bool plainRootReserved,
            DAT_GROUP_ENUM topLevelDatGroup,
            HashSet<string> reservedTopLevelFlaglessRoots,
            Dictionary<string, DAT_GROUP_ENUM> assignedNameOwnerGroups,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup)
        {
            if (plainRootReserved)
                return !IsSameNamingBucket(datGroup, topLevelDatGroup, flagFilterSetByGroup);

            if (TryGetAssignedNameOwnerGroup(baseName, assignedNameOwnerGroups, out var existingOwnerGroup))
                return !IsSameNamingBucket(datGroup, existingOwnerGroup, flagFilterSetByGroup);

            if (reservedTopLevelFlaglessRoots.Contains(baseName.Trim()))
                return !IsSameNamingBucket(datGroup, topLevelDatGroup, flagFilterSetByGroup);

            return !IsSameNamingBucket(datGroup, topLevelDatGroup, flagFilterSetByGroup);
        }

        private static bool IsPlainRootReservedForNonTopLevel(
            DAT_GROUP_ENUM datGroup,
            DAT_GROUP_ENUM topLevelDatGroup,
            string candidateName,
            HashSet<string> reservedTopLevelFlaglessRoots,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup)
        {
            if (IsSameNamingBucket(datGroup, topLevelDatGroup, flagFilterSetByGroup))
                return false;

            if (string.IsNullOrWhiteSpace(candidateName))
                return false;

            return reservedTopLevelFlaglessRoots.Contains(candidateName.Trim());
        }

        private static bool IsSameNamingBucket(
            DAT_GROUP_ENUM left,
            DAT_GROUP_ENUM right,
            IReadOnlyDictionary<DAT_GROUP_ENUM, FlagFilterSet> flagFilterSetByGroup)
        {
            if (left == right)
                return true;

            bool hasLeft = flagFilterSetByGroup.TryGetValue(left, out var leftSet);
            bool hasRight = flagFilterSetByGroup.TryGetValue(right, out var rightSet);

            if (hasLeft && hasRight && ReferenceEquals(leftSet, rightSet))
                return true;

            return false;
        }

        private static bool TryAssignName(
            string? candidateName,
            DAT_GROUP_ENUM ownerDatGroup,
            Dictionary<string, DAT_GROUP_ENUM> assignedNameOwnerGroups)
        {
            if (string.IsNullOrWhiteSpace(candidateName))
                return false;

            string trimmed = candidateName.Trim();

            if (assignedNameOwnerGroups.ContainsKey(trimmed))
                return false;

            assignedNameOwnerGroups.Add(trimmed, ownerDatGroup);
            return true;
        }

        private static bool TryGetAssignedNameOwnerGroup(
            string candidateName,
            Dictionary<string, DAT_GROUP_ENUM> assignedNameOwnerGroups,
            out DAT_GROUP_ENUM ownerDatGroup)
        {
            if (string.IsNullOrWhiteSpace(candidateName))
            {
                ownerDatGroup = default;
                return false;
            }

            return assignedNameOwnerGroups.TryGetValue(candidateName.Trim(), out ownerDatGroup);
        }

        private static string RemovePartFlags(string value, IReadOnlyCollection<string> partFlags)
        {
            var adjusted = value;

            foreach (var partFlag in partFlags)
            {
                if (string.IsNullOrWhiteSpace(partFlag))
                    continue;

                adjusted = adjusted.Replace(partFlag, string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            adjusted = MultiSpaceRegex.Replace(adjusted, " ").Trim();

            return adjusted;
        }

        private static string GetDatGroupSuffixedOrNumberedName(
            string baseName,
            DAT_GROUP_ENUM ownerDatGroup,
            Dictionary<string, DAT_GROUP_ENUM> assignedNameOwnerGroups)
        {
            string datGroupBaseName = $"{baseName} [{CleanDatGroupEnumName(ownerDatGroup)}]";

            if (TryAssignName(datGroupBaseName, ownerDatGroup, assignedNameOwnerGroups))
                return datGroupBaseName;

            return GetNextUniqueNumberedName(
                datGroupBaseName,
                ownerDatGroup,
                assignedNameOwnerGroups,
                appendWithoutSpace: true);
        }

        private static string GetNextUniqueNumberedName(
            string baseName,
            DAT_GROUP_ENUM ownerDatGroup,
            Dictionary<string, DAT_GROUP_ENUM> assignedNameOwnerGroups,
            bool appendWithoutSpace)
        {
            var index = 1;

            while (true)
            {
                var candidate = appendWithoutSpace
                    ? $"{baseName}[{index}]"
                    : $"{baseName} [{index}]";

                if (TryAssignName(candidate, ownerDatGroup, assignedNameOwnerGroups))
                    return candidate;

                index++;
            }
        }
    }
}