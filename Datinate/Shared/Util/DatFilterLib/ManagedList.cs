using com.RADIO.Datinate.RMVC.Shared;
using System.Text.RegularExpressions;
using static datinate.shared.DatFilterHelper;

namespace datinate.shared
{
    public sealed class ManagedList
    {
        private readonly DatGameVO[] allEntries;
        private readonly Dictionary<string, DatGameVO> entryByName;

        private readonly List<DatFilter> expressions;
        private readonly bool[] expressionsAreCategory;

        private readonly SortedDictionary<string, EXPRESSION_ACTION_ENUM> excludeList;
        private readonly SortedDictionary<string, EXPRESSION_ACTION_ENUM> includeList;
        private readonly SortedDictionary<string, EXPRESSION_ACTION_ENUM> unresolvedList;
        private readonly SortedDictionary<string, EXPRESSION_ACTION_ENUM> includeAndUnresolvedList;

        private readonly SortedDictionary<string, List<DatFilter>> conditionals;

        private readonly Dictionary<string, int> bestStripsScoreByCleanedKey;
        private readonly Dictionary<string, List<(string name, int score)>> bestStripsEntriesByCleanedKey;

        private static readonly Regex MultiSpace = new Regex(@"\s+", RegexOptions.Compiled);

        public ManagedList(DatGameVO[] allEntries, IEnumerable<DatFilter> expressions)
        {
            this.allEntries = allEntries ?? Array.Empty<DatGameVO>();

            entryByName = new Dictionary<string, DatGameVO>(StringComparer.Ordinal);
            for (int i = 0; i < this.allEntries.Length; i++)
            {
                var e = this.allEntries[i];
                if (!entryByName.ContainsKey(e.Name))
                    entryByName.Add(e.Name, e);
            }

            this.expressions = expressions?.ToList() ?? new List<DatFilter>();

            expressionsAreCategory = new bool[this.expressions.Count];
            for (int i = 0; i < this.expressions.Count; i++)
                expressionsAreCategory[i] = IsCategoryLabel(this.expressions[i].GetUserFriendlyExpression());

            excludeList = new SortedDictionary<string, EXPRESSION_ACTION_ENUM>(ListKeyComparer);
            includeList = new SortedDictionary<string, EXPRESSION_ACTION_ENUM>(ListKeyComparer);
            unresolvedList = new SortedDictionary<string, EXPRESSION_ACTION_ENUM>(ListKeyComparer);
            includeAndUnresolvedList = new SortedDictionary<string, EXPRESSION_ACTION_ENUM>(ListKeyComparer);

            conditionals = new SortedDictionary<string, List<DatFilter>>(ListKeyComparer);


            bestStripsScoreByCleanedKey = new Dictionary<string, int>(StringComparer.Ordinal);
            bestStripsEntriesByCleanedKey = new Dictionary<string, List<(string name, int score)>>(StringComparer.Ordinal);

            BuildLists();
        }

        public Dictionary<string, ManagedListItemReport> GetAllDecisionReports(bool verbose = false, int maxDigestTrace = 15)
        {
            var dic = new Dictionary<string, ManagedListItemReport>(
                includeAndUnresolvedList.Count + excludeList.Count,
                StringComparer.Ordinal);

            foreach (var kv in includeAndUnresolvedList)
            {
                var report = BuildDecisionReport(kv.Key, verbose, maxDigestTrace);
                if (report != null)
                    dic[kv.Key] = report;
            }

            foreach (var kv in excludeList)
            {
                if (dic.ContainsKey(kv.Key))
                    continue;

                var report = BuildDecisionReport(kv.Key, verbose, maxDigestTrace);
                if (report != null)
                    dic[kv.Key] = report;
            }

            return dic;
        }


        public IReadOnlyDictionary<string, EXPRESSION_ACTION_ENUM> Included => includeList;
        public IReadOnlyDictionary<string, EXPRESSION_ACTION_ENUM> Excluded => excludeList;
        public IReadOnlyDictionary<string, EXPRESSION_ACTION_ENUM> IncludedAndUnresolved => includeAndUnresolvedList;
        public IReadOnlyDictionary<string, EXPRESSION_ACTION_ENUM> Unresolved => unresolvedList;

        public SortedDictionary<string, EXPRESSION_ACTION_ENUM> GetAll()
        {
            var dic = new SortedDictionary<string, EXPRESSION_ACTION_ENUM>(ListKeyComparer);

            foreach (var item in includeAndUnresolvedList)
                dic[item.Key] = item.Value;

            foreach (var item in excludeList)
            {
                if (!dic.ContainsKey(item.Key))
                    dic[item.Key] = item.Value;
            }

            return dic;
        }


        public SortedDictionary<string, EXPRESSION_ACTION_ENUM> GetIncludedAndImplict() => includeAndUnresolvedList;
        public SortedDictionary<string, EXPRESSION_ACTION_ENUM> GetIncluded() => includeList;
        public SortedDictionary<string, EXPRESSION_ACTION_ENUM> GetUnresolved() => unresolvedList;
        public SortedDictionary<string, EXPRESSION_ACTION_ENUM> GetExcluded() => excludeList;

        public bool TryGetEntry(string entryName, out DatGameVO entry) => entryByName.TryGetValue(entryName, out entry);

        public bool TryGetFinalAction(string entryName, out EXPRESSION_ACTION_ENUM action)
        {
            if (excludeList.TryGetValue(entryName, out action))
                return true;

            if (includeAndUnresolvedList.TryGetValue(entryName, out action))
                return true;

            action = default;
            return false;
        }

        public ManagedListItemReport? BuildDecisionReport(string entryName, bool verbose, int maxDigestTrace = 15)
        {
            if (!TryGetFinalAction(entryName, out var finalAction))
                return null;

            var isExcluded = excludeList.ContainsKey(entryName);

            entryByName.TryGetValue(entryName, out var entry);
            var category = entry?.Category;

            int matchedFilters = 0;

            DatFilter? decisive = null;
            int? decisiveIndex = null;
            bool decisiveIsCategory = false;

            int conditionalStrips = 0;
            string stripped = entryName;

            for (int i = 0; i < expressions.Count; i++)
            {
                var exp = expressions[i];
                var expIsCategory = expressionsAreCategory[i];

                if (!Matches(exp, expIsCategory, entryName, category))
                    continue;

                matchedFilters++;

                var treatCategoryConditionalAsDecisiveExclude = expIsCategory && exp.GetExcludeConditional();

                if (exp.ExcludeAlways() || exp.IncludeAlways() || treatCategoryConditionalAsDecisiveExclude)
                {
                    decisive = exp;
                    decisiveIndex = i + 1;
                    decisiveIsCategory = expIsCategory;
                    break;
                }

                if (expIsCategory)
                    continue;

                conditionalStrips++;
                stripped = exp.ReplaceMatches(stripped, string.Empty);
            }

            stripped = MultiSpace.Replace(stripped, " ").Trim();
            var cleanedKey = stripped;

            int? bestInGroup = null;
            if (bestStripsScoreByCleanedKey.TryGetValue(cleanedKey, out var score))
                bestInGroup = score;

            var trace = BuildTrace(entryName, category, verbose ? int.MaxValue : maxDigestTrace);
            var matchedConditionals = BuildMatchedConditionals(entryName, category);

            if (decisive != null && decisiveIndex.HasValue)
            {
                return ManagedListItemReport.CreateDecisive(
                    entryName,
                    category,
                    isExcluded,
                    finalAction,
                    matchedFilters,
                    decisiveIndex.Value,
                    decisive.GetExpressionAction().ToString(),
                    decisive.GetUserFriendlyExpression(),
                    decisiveIsCategory,
                    matchedConditionals,
                    trace);
            }

            return ManagedListItemReport.CreateConditional(
                entryName,
                category,
                isExcluded,
                finalAction,
                matchedFilters,
                conditionalStrips,
                stripped,
                cleanedKey,
                bestInGroup,
                matchedConditionals,
                trace);
        }

        private void BuildLists()
        {
            excludeList.Clear();
            includeList.Clear();
            unresolvedList.Clear();
            includeAndUnresolvedList.Clear();
            conditionals.Clear();

            bestStripsScoreByCleanedKey.Clear();
            bestStripsEntriesByCleanedKey.Clear();

            for (int j = 0; j < allEntries.Length; j++)
            {
                var entry = allEntries[j];
                var entryName = entry.Name;
                var category = entry.Category;

                var expressionMatch = false;

                for (int i = 0; i < expressions.Count; i++)
                {
                    var exp = expressions[i];
                    var expIsCategory = expressionsAreCategory[i];

                    if (!Matches(exp, expIsCategory, entryName, category))
                        continue;

                    if (expIsCategory && exp.GetExcludeConditional())
                    {
                        excludeList[entryName] = EXPRESSION_ACTION_ENUM.EXCLUDE;
                        expressionMatch = true;
                        break;
                    }

                    if (exp.ExcludeAlways())
                    {
                        excludeList[entryName] = EXPRESSION_ACTION_ENUM.EXCLUDE;
                        expressionMatch = true;
                        break;
                    }

                    if (exp.IncludeAlways())
                    {
                        includeList[entryName] = EXPRESSION_ACTION_ENUM.INCLUDE;
                        includeAndUnresolvedList[entryName] = EXPRESSION_ACTION_ENUM.INCLUDE;
                        expressionMatch = true;
                        break;
                    }

                    if (!conditionals.TryGetValue(entryName, out var list))
                    {
                        list = new List<DatFilter>();
                        conditionals.Add(entryName, list);
                    }

                    list.Add(exp);
                    expressionMatch = true;
                }

                if (!expressionMatch)
                {
                    unresolvedList[entryName] = EXPRESSION_ACTION_ENUM.INCLUDE_IMPLICIT;
                    includeAndUnresolvedList[entryName] = EXPRESSION_ACTION_ENUM.INCLUDE_IMPLICIT;
                }
            }

            AssignConditionals();
        }

        private void AssignConditionals()
        {
            if (conditionals.Count == 0)
                return;

            var conditionalFilters = expressions
                .Where((f, idx) => !expressionsAreCategory[idx] && f.GetExcludeConditional())
                .ToArray();

            string CondenseName(string entryName, string? category)
            {
                var s = entryName;

                for (int i = 0; i < conditionalFilters.Length; i++)
                {
                    var f = conditionalFilters[i];
                    if (Matches(f, expIsCategory: false, entryName: s, category: category))
                        s = f.ReplaceMatches(s, string.Empty);
                }

                return MultiSpace.Replace(s, " ").Trim();
            }

            var condensedIncluded = new HashSet<string>(StringComparer.Ordinal);
            foreach (var kv in includeAndUnresolvedList)
            {
                entryByName.TryGetValue(kv.Key, out var e);
                condensedIncluded.Add(CondenseName(kv.Key, e?.Category));
            }

            var tmp = new List<(string name, string stripped, int score)>(conditionals.Count);

            foreach (var kv in conditionals)
            {
                var name = kv.Key;

                entryByName.TryGetValue(name, out var entry);
                var category = entry?.Category;

                var stripped = name;
                var score = 0;

                var exprs = kv.Value;
                for (int i = 0; i < exprs.Count; i++)
                {
                    var exp = exprs[i];
                    var expIsCategory = IsCategoryLabel(exp.GetUserFriendlyExpression());

                    if (expIsCategory)
                        continue;

                    score++;
                    stripped = exp.ReplaceMatches(stripped, string.Empty);
                }

                stripped = MultiSpace.Replace(stripped, " ").Trim();

                tmp.Add((name, stripped, score));

                if (!bestStripsScoreByCleanedKey.TryGetValue(stripped, out var best) || score < best)
                {
                    bestStripsScoreByCleanedKey[stripped] = score;
                    bestStripsEntriesByCleanedKey[stripped] = new List<(string name, int score)> { (name, score) };
                }
                else if (score == best)
                {
                    if (!bestStripsEntriesByCleanedKey.TryGetValue(stripped, out var list))
                    {
                        list = new List<(string name, int score)>();
                        bestStripsEntriesByCleanedKey[stripped] = list;
                    }

                    list.Add((name, score));
                }
            }

            foreach (var kv in bestStripsEntriesByCleanedKey)
            {
                var stripped = kv.Key;
                var list = kv.Value;

                list.Sort((a, b) => StringComparer.Ordinal.Compare(a.name, b.name));

                for (int i = 0; i < list.Count; i++)
                {
                    var name = list[i].name;

                    if (condensedIncluded.Contains(stripped))
                    {
                        excludeList[name] = EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL;
                        continue;
                    }

                    includeAndUnresolvedList[name] = EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL;
                    includeList[name] = EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL;

                    condensedIncluded.Add(stripped);
                    break;
                }
            }

            foreach (var kv in conditionals)
            {
                var entryName = kv.Key;

                if (includeAndUnresolvedList.ContainsKey(entryName))
                    continue;

                excludeList[entryName] = EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL;
            }
        }

        private ManagedListItemReport.TraceItem[] BuildTrace(string entryName, string? category, int maxItems)
        {
            if (maxItems <= 0)
                return Array.Empty<ManagedListItemReport.TraceItem>();

            var tmp = new List<ManagedListItemReport.TraceItem>();

            for (int i = 0; i < expressions.Count; i++)
            {
                var exp = expressions[i];
                var expIsCategory = expressionsAreCategory[i];

                if (!Matches(exp, expIsCategory, entryName, category))
                    continue;

                tmp.Add(new ManagedListItemReport.TraceItem(
                    i + 1,
                    exp.GetExpressionAction().ToString(),
                    exp.GetUserFriendlyExpression(),
                    expIsCategory));

                if (tmp.Count >= maxItems)
                    break;
            }

            return tmp.Count == 0 ? Array.Empty<ManagedListItemReport.TraceItem>() : tmp.ToArray();
        }

        private ManagedListItemReport.ConditionalMatch[] BuildMatchedConditionals(string entryName, string? category)
        {
            if (!conditionals.TryGetValue(entryName, out var list) || list.Count == 0)
                return Array.Empty<ManagedListItemReport.ConditionalMatch>();

            var tmp = new List<ManagedListItemReport.ConditionalMatch>();

            for (int i = 0; i < list.Count; i++)
            {
                var exp = list[i];
                var isCategory = IsCategoryLabel(exp.GetUserFriendlyExpression());

                if (!Matches(exp, isCategory, entryName, category))
                    continue;

                tmp.Add(new ManagedListItemReport.ConditionalMatch(
                    exp.GetUserFriendlyExpression(),
                    exp.GetExpressionAction().ToString(),
                    isCategory));
            }

            return tmp.Count == 0 ? Array.Empty<ManagedListItemReport.ConditionalMatch>() : tmp.ToArray();
        }

        private static bool Matches(DatFilter exp, bool expIsCategory, string entryName, string? category)
        {
            if (!expIsCategory)
                return exp.IsMatch(entryName);

            if (string.IsNullOrWhiteSpace(category))
                return false;

            var c = category.Trim();
            if (c.Length == 0)
                return false;

            var label = (c.Length >= 2 && c[0] == '<' && c[c.Length - 1] == '>') ? c : "<" + c + ">";

            return exp.IsMatch(label);
        }

        private static bool IsCategoryLabel(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            if (s.Length < 2)
                return false;

            return s[0] == '<' && s[s.Length - 1] == '>';
        }

        private sealed class CaseInsensitiveThenOrdinalComparer : IComparer<string> { public int Compare(string? x, string? y) { if (ReferenceEquals(x, y)) return 0; if (x is null) return -1; if (y is null) return 1; var c = StringComparer.OrdinalIgnoreCase.Compare(x, y); if (c != 0) return c; return StringComparer.Ordinal.Compare(x, y); } } private static readonly IComparer<string> ListKeyComparer = new CaseInsensitiveThenOrdinalComparer();
    }
}
