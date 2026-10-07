using static datinate.shared.DatFilterHelper;

namespace datinate.shared
{
    public sealed class ManagedListItemReport
    {
        public readonly struct TraceItem
        {
            public int Index1Based { get; }
            public string ExpressionActionName { get; }
            public string UserFriendlyExpression { get; }
            public bool IsCategory { get; }

            public TraceItem(int index1Based, string expressionActionName, string userFriendlyExpression, bool isCategory)
            {
                Index1Based = index1Based;
                ExpressionActionName = expressionActionName;
                UserFriendlyExpression = userFriendlyExpression;
                IsCategory = isCategory;
            }
        }

        public sealed class ConditionalMatch
        {
            public string Expression { get; }
            public string EffectName { get; }
            public bool IsCategory { get; }

            public ConditionalMatch(string expression, string effectName, bool isCategory)
            {
                Expression = expression;
                EffectName = effectName;
                IsCategory = isCategory;
            }
        }

        private ManagedListItemReport(
            string entryName,
            string? entryCategoryLabel,
            bool isExcluded,
            EXPRESSION_ACTION_ENUM finalAction,
            int matchedFilterCount,
            int? decisiveFilterPriorityIndex1Based,
            string? decisiveFilterEffectName,
            string? decisiveFilterExpression,
            bool decisiveFilterIsCategory,
            int conditionalStripCount,
            string? strippedName,
            string? cleanedKey,
            int? bestInGroupScore,
            IReadOnlyList<ConditionalMatch> matchedConditionals,
            IReadOnlyList<TraceItem> trace)
        {
            EntryName = entryName;
            EntryCategoryLabel = entryCategoryLabel;

            IsExcluded = isExcluded;
            FinalAction = finalAction;

            MatchedFilterCount = matchedFilterCount;

            DecisiveFilterPriorityIndex1Based = decisiveFilterPriorityIndex1Based;
            DecisiveFilterEffectName = decisiveFilterEffectName;
            DecisiveFilterExpression = decisiveFilterExpression;
            DecisiveFilterIsCategory = decisiveFilterIsCategory;

            ConditionalStripCount = conditionalStripCount;
            StrippedName = strippedName;
            CleanedKey = cleanedKey;
            BestInGroupScore = bestInGroupScore;

            MatchedConditionals = matchedConditionals;
            Trace = trace;
        }

        public string EntryName { get; }
        public string? EntryCategoryLabel { get; }

        public bool IsExcluded { get; }
        public EXPRESSION_ACTION_ENUM FinalAction { get; }

        public int MatchedFilterCount { get; }

        public int? DecisiveFilterPriorityIndex1Based { get; }
        public string? DecisiveFilterEffectName { get; }
        public string? DecisiveFilterExpression { get; }
        public bool DecisiveFilterIsCategory { get; }

        public bool HasDecisiveFilter => DecisiveFilterPriorityIndex1Based.HasValue;

        public int ConditionalStripCount { get; }
        public string? StrippedName { get; }
        public string? CleanedKey { get; }
        public int? BestInGroupScore { get; }

        public IReadOnlyList<ConditionalMatch> MatchedConditionals { get; }
        public IReadOnlyList<TraceItem> Trace { get; }

        public static ManagedListItemReport CreateDecisive(
            string entryName,
            string? entryCategoryLabel,
            bool isExcluded,
            EXPRESSION_ACTION_ENUM finalAction,
            int matchedFilterCount,
            int decisiveFilterPriorityIndex1Based,
            string decisiveFilterEffectName,
            string decisiveFilterExpression,
            bool decisiveFilterIsCategory,
            IReadOnlyList<ConditionalMatch>? matchedConditionals,
            IReadOnlyList<TraceItem>? trace)
        {
            return new ManagedListItemReport(
                entryName,
                entryCategoryLabel,
                isExcluded,
                finalAction,
                matchedFilterCount,
                decisiveFilterPriorityIndex1Based,
                decisiveFilterEffectName,
                decisiveFilterExpression,
                decisiveFilterIsCategory,
                0,
                null,
                null,
                null,
                matchedConditionals ?? Array.Empty<ConditionalMatch>(),
                trace ?? Array.Empty<TraceItem>());
        }

        public static ManagedListItemReport CreateConditional(
            string entryName,
            string? entryCategoryLabel,
            bool isExcluded,
            EXPRESSION_ACTION_ENUM finalAction,
            int matchedFilterCount,
            int conditionalStripCount,
            string strippedName,
            string cleanedKey,
            int? bestInGroupScore,
            IReadOnlyList<ConditionalMatch>? matchedConditionals,
            IReadOnlyList<TraceItem>? trace)
        {
            return new ManagedListItemReport(
                entryName,
                entryCategoryLabel,
                isExcluded,
                finalAction,
                matchedFilterCount,
                null,
                null,
                null,
                false,
                conditionalStripCount,
                strippedName,
                cleanedKey,
                bestInGroupScore,
                matchedConditionals ?? Array.Empty<ConditionalMatch>(),
                trace ?? Array.Empty<TraceItem>());
        }
    }
}
