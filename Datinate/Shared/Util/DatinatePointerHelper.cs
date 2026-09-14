using static com.RADIO.Datinate.RMVC.Shared.DatinateEnums;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public static class DatinatePointerHelper
    {
        public static string? GetPointerSubset(string pointerId)
        {
            if (string.IsNullOrWhiteSpace(pointerId))
                return null;

            var s = pointerId.Trim();

            if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
                s = s.Substring(1, s.Length - 2).Trim();

            s = s.TrimEnd();

            if (s.Length == 0 || s[^1] != ')')
                return null;

            int depth = 0;

            for (int i = s.Length - 1; i >= 0; i--)
            {
                char c = s[i];

                if (c == ')')
                {
                    depth++;
                    continue;
                }

                if (c == '(')
                {
                    depth--;
                    if (depth == 0)
                    {
                        var inner = s.Substring(i + 1, s.Length - i - 2).Trim();
                        return inner.Length == 0 ? null : inner;
                    }
                }
            }

            return null;
        }
        public static string BuildPointerId(
            COLLECTION_SET_ENUM collectionSetEnum
            , DAT_GROUP_ENUM datGroupEnum
            , string? source
            , DatSubsetFilter? datSubsetFilter
            , string? friendlyName)
        {
            return BuildPointerIdInternal(
                collectionSetEnum
                , datGroupEnum
                , friendlyName
                , source
                , datSubsetFilter
            );
        }

        private static string BuildPointerIdInternal(
            COLLECTION_SET_ENUM? collectionSetEnum
            , DAT_GROUP_ENUM? datGroupEnum
            , string? datQuickRef
            , string? typeOrSourceStr
            , DatSubsetFilter? datSubsetFilter)
        {
            string? collectionStr = null;

            if (collectionSetEnum != null)
                collectionStr = collectionSetEnum == COLLECTION_SET_ENUM.NOT_SET
                    ? string.Empty
                    : collectionSetEnum.ToString();

            string? datGroupStr = null;
            if (datGroupEnum != null)
                datGroupStr = datGroupEnum == DAT_GROUP_ENUM.NOT_SET
                    ? string.Empty
                    : datGroupEnum.ToString();

            if (!string.IsNullOrWhiteSpace(datQuickRef))
            {
                if (!string.IsNullOrWhiteSpace(datGroupStr))
                    datGroupStr += " - " + datQuickRef;
                else
                    datGroupStr = datQuickRef;
            }

            typeOrSourceStr = string.IsNullOrWhiteSpace(typeOrSourceStr) || typeOrSourceStr == MEDIA_TYPE_ENUM.NOT_SET.ToString()
                ? string.Empty
                : typeOrSourceStr;

            string subset = string.Empty;

            if (datSubsetFilter != null)
            {
                subset = "(";
                subset += datSubsetFilter.Entry;

                if (!string.IsNullOrWhiteSpace(datSubsetFilter.Path))
                    subset += ": " + datSubsetFilter.Path;

                subset += ")";
            }

            return BuildPointerId(
                collectionStr
                , datGroupStr
                , typeOrSourceStr
                , subset
            );
        }

        private static string BuildPointerId(
            string? collection,
            string? datGroup,
            string? sourceOrType,
            string? subset)
        {
            string running = string.Empty;

            if (!string.IsNullOrWhiteSpace(collection))
                running += collection;

            if (!string.IsNullOrWhiteSpace(datGroup))
                running += running == string.Empty ? datGroup : ": " + datGroup;

            if (!string.IsNullOrWhiteSpace(sourceOrType))
                running += running == string.Empty ? sourceOrType : ": " + sourceOrType;

            if (!string.IsNullOrWhiteSpace(subset))
                running += running == string.Empty ? subset : ": " + subset;

            return running;
        }
        public static string? GetDatFriendlyName(string? pointerId)
        {
            if (string.IsNullOrWhiteSpace(pointerId))
                return null;

            var input = pointerId.Trim();

            string? label = null;

            int dash = input.IndexOf(" - ", StringComparison.Ordinal);
            if (dash >= 0)
                label = input.Substring(dash + 3).Trim();

            if (string.IsNullOrWhiteSpace(label))
                return null;

            var s = label.Trim();

            int paren = s.IndexOf('(');
            if (paren >= 0)
                s = s.Substring(0, paren).TrimEnd();

            int firstColon = s.IndexOf(':');
            if (firstColon >= 0)
            {
                var prefix = s.Substring(0, firstColon).Trim();

                if (IsAlphaWord(prefix))
                {
                    s = prefix;
                }
                else
                {
                    s = TakeAfterLastColon(s);
                }
            }

            s = s.Replace('-', ' ')
                 .Replace('_', ' ')
                 .Trim();

            if (s.Length == 0)
                return null;

            if (!IsDatFriendlyNameAcceptable(s))
                return null;

            return s;

            static string TakeAfterLastColon(string t)
            {
                t = t.TrimEnd(':').Trim();

                int lastColon = t.LastIndexOf(':');

                return (lastColon >= 0 && lastColon < t.Length - 1)
                    ? t.Substring(lastColon + 1).Trim()
                    : t;
            }

            static bool IsAlphaWord(string t)
            {
                if (t.Length == 0)
                    return false;

                for (int i = 0; i < t.Length; i++)
                {
                    char c = t[i];

                    if (!((c >= 'A' && c <= 'Z') ||
                          (c >= 'a' && c <= 'z')))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public static string GetDatGroupAndFriendlyName(string? pointerId)
        {
            var friendly = GetDatFriendlyName(pointerId);
            var datGroup = GetDatGroup(pointerId).ToString().Replace("_", " ");

            if (string.IsNullOrWhiteSpace(friendly) == false)
                return datGroup + " - " + friendly;

            return datGroup;
        }

        public static DAT_GROUP_ENUM GetDatGroup(string? pointerId)
        {
            if (string.IsNullOrWhiteSpace(pointerId))
                return DAT_GROUP_ENUM.NOT_SET;

            var s = pointerId.Trim();

            // Find the very first ": "
            int first = s.IndexOf(": ", StringComparison.Ordinal);
            if (first < 0)
                return DAT_GROUP_ENUM.NOT_SET;

            int start = first + 2;
            if (start >= s.Length)
                return DAT_GROUP_ENUM.NOT_SET;

            // Take until next ':' or space (whichever comes first), or end of string.
            int colon = s.IndexOf(':', start);
            int space = s.IndexOf(' ', start);

            int end;
            if (colon < 0 && space < 0) end = s.Length;
            else if (colon < 0) end = space;
            else if (space < 0) end = colon;
            else end = Math.Min(colon, space);

            if (end <= start)
                return DAT_GROUP_ENUM.NOT_SET;

            var token = s.Substring(start, end - start).Trim();

            var result = (token.Length == 0)
                ? DAT_GROUP_ENUM.NOT_SET
                : DatinateHelper.GetEnumFromString<DAT_GROUP_ENUM>(token, DAT_GROUP_ENUM.NOT_SET);

            return result;
        }

        public static bool IsDatFriendlyNameAcceptable(string friendlyName)
        {
            const int MaxLength = 16;

            if (friendlyName.Length > MaxLength)
                return false;

            for (int i = 0; i < friendlyName.Length; i++)
            {
                char c = friendlyName[i];

                bool isLetter =
                    (c >= 'A' && c <= 'Z') ||
                    (c >= 'a' && c <= 'z');

                bool isDigit = c >= '0' && c <= '9';

                if (!isLetter && !isDigit && c != '_' && c != ' ')
                    return false;
            }

            return true;
        }

    }
}
