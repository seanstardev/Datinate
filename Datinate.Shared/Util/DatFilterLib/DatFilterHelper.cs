using com.RADIO.Datinate.RMVC.Shared;
using System.Text.RegularExpressions;
using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace datinate.shared
{
    public static class DatFilterHelper 
    {
        public enum EXPRESSION_ACTION_ENUM 
        {
            INVALID
            , INCLUDE
            , EXCLUDE
            , EXCLUDE_CONDITIONAL
            , INCLUDE_IMPLICIT
            , NO_FILTER
        }

        /**
         * Returns null if publisher not found in dat entry. i.e. malformed / legacy TOSEC entry:
         */
        public static string? GetPublisherTosec(string datEntryName) 
        {
            string exp = @"\)\(([^\)]*)\)";
            Match m = Regex.Match(datEntryName, exp);
            
            if (!m.Success)
                return null;
            else 
                return m.Groups[1].Value;
        }

        public static int GetExpressionColourArgb(
            EXPRESSION_ACTION_ENUM expressionActionEnum)
        {
            switch (expressionActionEnum)
            {
                case EXPRESSION_ACTION_ENUM.EXCLUDE:
                    return unchecked((int)0xFFFF0000); // Color.Red

                case EXPRESSION_ACTION_ENUM.INCLUDE:
                    return unchecked((int)0xFF008000); // Color.Green

                case EXPRESSION_ACTION_ENUM.EXCLUDE_CONDITIONAL:
                    return unchecked((int)0xFFFF4500); // Color.OrangeRed

                case EXPRESSION_ACTION_ENUM.INCLUDE_IMPLICIT:
                    return unchecked((int)0xFF000000); // Color.Black

                default:
                    return unchecked((int)0xFF000000); // Color.Black
            }
        }

        public static int GetExpressionColourArgb(
            DAT_GROUPER_MEMBERSHIP_ENUM expressionActionEnum)
        {
            switch (expressionActionEnum)
            {
                case DAT_GROUPER_MEMBERSHIP_ENUM.EXCLUDE_EXPLICIT:
                case DAT_GROUPER_MEMBERSHIP_ENUM.EXCLUDE_RELATIVE:
                    return unchecked((int)0xFFFF0000); // Color.Red

                case DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_EXPLICIT:
                    return unchecked((int)0xFF008000); // Color.Green

                case DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_RELATIVE:
                    return unchecked((int)0xFFFF4500); // Color.OrangeRed

                case DAT_GROUPER_MEMBERSHIP_ENUM.INCLUDE_IMPLICIT:
                    return unchecked((int)0xFF000000); // Color.Black

                default:
                    return unchecked((int)0xFF000000); // Color.Black
            }
        }

        static readonly string individualFlagsRegex = @"(\[[^\[]+\])|(\([^\(]+\))";

        public static readonly string REGEX_ZERO_OR_MORE_OF_ANYTHING = "*";
        public static readonly string REGEX_ONE_OR_MORE_NUMBERS      = ":";
        public static readonly string REGEX_ONE_OR_MORE_LETTERS      = "\"";

        public static bool IsValidSearch(string text, out string? problemReport) 
        {

            problemReport = null;

            int letters     = text.Length - text.Replace(DatFilterHelper.REGEX_ONE_OR_MORE_LETTERS, "").Length;
            int numbers     = text.Length - text.Replace(DatFilterHelper.REGEX_ONE_OR_MORE_NUMBERS, "").Length;
            int anything    = text.Length - text.Replace(DatFilterHelper.REGEX_ZERO_OR_MORE_OF_ANYTHING, "").Length;

            // TODO: 
            // Don't allow more than one CONSECUTIVE special character of same type. e.g. **
            // Ensure * is always last special character if used (in relation to other special characters, that is)?
            //
            if (/*letters > 1 || numbers > 1 ||*/  anything > 1) 
            {

                problemReport =
                    "Do not use more than one '"
                    + DatFilterHelper.REGEX_ZERO_OR_MORE_OF_ANYTHING
                    + "' special character per Search.";

                return false;
            }
            return true;
        }
        public const string NO_FLAGS = "{_NO_FLAGS_}";

        public static Flag[] GetCategorySets(DatVO dat)
        {
            var dic = new SortedDictionary<string, List<DatGameVO>>(StringComparer.Ordinal);

            foreach (var g in dat.Entries)
            {
                var cat = g.Category?.Trim();
                if (string.IsNullOrWhiteSpace(cat))
                    continue;

                cat = "<" + cat + ">";

                if (!dic.TryGetValue(cat, out var list))
                {
                    list = new List<DatGameVO>();
                    dic.Add(cat, list);
                }

                list.Add(g);
            }

            var flagList = new List<Flag>(dic.Count);
            foreach (var pair in dic)
                flagList.Add(new Flag(pair.Key, pair.Value));

            return flagList.ToArray();
        }


        public static Flag[] GetFlagSets(DatGameVO[] allEntries)
        {

            SortedDictionary<string, List<DatGameVO>> dic =
                new SortedDictionary<string, List<DatGameVO>>();

            for (int i = 0; i < allEntries.Length; i++)
            {
                var gameName = allEntries[i].Name;
                var fs = GetIndividualFlags(gameName);

                if (fs.Length == 0)
                {
                    if (!dic.ContainsKey(NO_FLAGS))
                        dic.Add(NO_FLAGS, new List<DatGameVO>());
                    dic[NO_FLAGS].Add(allEntries[i]);
                    continue;
                }

                for (int j = 0; j < fs.Length; j++)
                {
                    var f = fs[j];
                    if (!dic.ContainsKey(f))
                        dic.Add(f, new List<DatGameVO>());
                    dic[f].Add(allEntries[i]);
                }
            }

            List<Flag> flagList = new List<Flag>();

            foreach (KeyValuePair<string, List<DatGameVO>> pair in dic)
                flagList.Add(new Flag(pair.Key, pair.Value.ToArray()));

            return flagList.ToArray();
        }

        private static readonly Regex IndividualFlagsRx = new Regex(individualFlagsRegex, RegexOptions.Compiled);

        public static string[] GetIndividualFlags(string gameName)
        {
            var mc = IndividualFlagsRx.Matches(gameName);

            if (mc.Count == 0)
                return Array.Empty<string>();

            var arr = new string[mc.Count];
            for (int i = 0; i < mc.Count; i++)
                arr[i] = mc[i].Value;

            return arr;
        }
    }
}
