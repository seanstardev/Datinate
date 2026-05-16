using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace com.RADIO.Datinate.RMVC
{
    public static class FlagStripper
    {

        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(50);

        private static readonly RegexOptions RemoveOptions =
            RegexOptions.CultureInvariant | RegexOptions.Compiled;

        private static readonly RegexOptions MatchOptions =
            RegexOptions.CultureInvariant | RegexOptions.Compiled;

        private static readonly ConcurrentDictionary<IReadOnlyCollection<string>, RuleSet> Cache =
            new ConcurrentDictionary<IReadOnlyCollection<string>, RuleSet>(new RefEq());

        public static string NormaliseName(string name, IReadOnlyCollection<string>? flags)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            if (flags != null && flags.Count > 0)
            {
                var rules = GetRuleSet(flags);
                if (rules.RemoveRegex != null)
                    name = rules.RemoveRegex.Replace(name, string.Empty);
            }

            return CollapseWhitespace(name);
        }

        public static bool MatchesAnyValue(string value, IReadOnlyCollection<string>? flags)
        {
            if (string.IsNullOrEmpty(value) || flags == null || flags.Count == 0)
                return false;

            var rules = GetRuleSet(flags);

            if (rules.LiteralSet.Count > 0 && rules.LiteralSet.Contains(value))
                return true;

            var matchRegexes = rules.MatchRegexes;
            for (var i = 0; i < matchRegexes.Length; i++)
            {
                if (matchRegexes[i].IsMatch(value))
                    return true;
            }

            return false;
        }

        private static RuleSet GetRuleSet(IReadOnlyCollection<string> flags)
        {
            return Cache.GetOrAdd(flags, BuildRuleSet);
        }

        private static RuleSet BuildRuleSet(IReadOnlyCollection<string> flags)
        {
            var literalSet = new HashSet<string>(StringComparer.Ordinal);
            var removeParts = new List<string>(flags.Count);
            var matchRegexes = new List<Regex>();

            foreach (var rawFlag in flags)
            {
                if (string.IsNullOrEmpty(rawFlag))
                    continue;

                if (TryBuildWildcard(rawFlag, out var removePart, out var matchRegex))
                {
                    removeParts.Add(removePart);
                    if (matchRegex != null)
                        matchRegexes.Add(matchRegex);
                }
                else
                {
                    removeParts.Add(Regex.Escape(rawFlag));
                    literalSet.Add(rawFlag);
                }
            }

            Regex? removeRegex = null;

            if (removeParts.Count > 0)
            {
                var combined = "(?:" + string.Join("|", removeParts) + ")";
                removeRegex = new Regex(combined, RemoveOptions, RegexTimeout);
            }

            return new RuleSet(
                removeRegex,
                literalSet,
                matchRegexes.Count == 0 ? Array.Empty<Regex>() : matchRegexes.ToArray());
        }

        private static bool TryBuildWildcard(string rawFlag, out string removeRegexPart, out Regex? matchRegex)
        {
            removeRegexPart = string.Empty;
            matchRegex = null;

            if (!rawFlag.Contains(@"\*", StringComparison.Ordinal))
                return false;

            if (rawFlag.StartsWith(@"\*", StringComparison.Ordinal))
                return false;

            var endsWithWildcard = rawFlag.EndsWith(@"\*", StringComparison.Ordinal);

            var parts = rawFlag.Split(new[] { @"\*" }, StringSplitOptions.None);
            if (parts.Length == 0 || string.IsNullOrEmpty(parts[0]))
                return false;

            var removeBody = BuildWildcardBody(parts, lazy: true);
            if (string.IsNullOrEmpty(removeBody))
                return false;

            removeRegexPart = removeBody;

            var matchBody = BuildWildcardBody(parts, lazy: false);
            if (string.IsNullOrEmpty(matchBody))
                return true;

            var anchored = "^" + matchBody + (endsWithWildcard ? "" : "$");
            matchRegex = new Regex(anchored, MatchOptions, RegexTimeout);

            return true;
        }

        private static string BuildWildcardBody(string[] parts, bool lazy)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                    sb.Append(lazy ? ".*?" : ".*");

                if (!string.IsNullOrEmpty(parts[i]))
                    sb.Append(Regex.Escape(parts[i]));
            }

            return sb.ToString();
        }

        private static string CollapseWhitespace(string name)
        {
            var sb = new StringBuilder(name.Length);
            var pendingSpace = false;

            for (var i = 0; i < name.Length; i++)
            {
                var ch = name[i];

                if (char.IsWhiteSpace(ch))
                {
                    pendingSpace = sb.Length > 0;
                    continue;
                }

                if (pendingSpace)
                {
                    sb.Append(' ');
                    pendingSpace = false;
                }

                sb.Append(ch);
            }

            return sb.ToString();
        }

        private sealed class RuleSet
        {
            public Regex? RemoveRegex { get; }
            public HashSet<string> LiteralSet { get; }
            public Regex[] MatchRegexes { get; }

            public RuleSet(Regex? removeRegex, HashSet<string> literalSet, Regex[] matchRegexes)
            {
                RemoveRegex = removeRegex;
                LiteralSet = literalSet;
                MatchRegexes = matchRegexes;
            }
        }

        private sealed class RefEq : IEqualityComparer<IReadOnlyCollection<string>>
        {
            public bool Equals(IReadOnlyCollection<string>? x, IReadOnlyCollection<string>? y) => ReferenceEquals(x, y);
            public int GetHashCode(IReadOnlyCollection<string> obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
