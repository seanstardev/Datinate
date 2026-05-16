using com.RADIO.Datinate.RMVC.Shared;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Datinate.Rmvc.Proxy.delegates
{
    public static class R2DatResourceDelegate
    {
        private static readonly Regex PlaceholderRegex = new(@"\{(?<name>[A-Za-z0-9_]+)\}", RegexOptions.Compiled);

        private static readonly HashSet<string> ValidSources = new(StringComparer.OrdinalIgnoreCase)
        {
            "name",
            "lookup",
            "system"
        };

        private static readonly HashSet<string> ValidEncodings = new(StringComparer.OrdinalIgnoreCase)
        {
            "none",
            "escapeDataString"
        };

        public static IReadOnlyList<R2DatResourceDTO> LoadOrEmpty(string? xmlPath)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
                return Array.Empty<R2DatResourceDTO>();

            if (!File.Exists(xmlPath))
                return Array.Empty<R2DatResourceDTO>();

            try
            {
                return LoadInternal(xmlPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"R2DatResourceDelegate.LoadOrEmpty failed for '{xmlPath}': {ex}");
                return Array.Empty<R2DatResourceDTO>();
            }
        }

        private static IReadOnlyList<R2DatResourceDTO> LoadInternal(string xmlPath)
        {
            XDocument doc = XDocument.Load(xmlPath, LoadOptions.None);
            var root = doc.Root;

            if (root == null)
                return Array.Empty<R2DatResourceDTO>();

            var sourcesEl = root.Element("Sources");
            if (sourcesEl == null)
                return Array.Empty<R2DatResourceDTO>();

            var result = new List<R2DatResourceDTO>();
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var sourceEl in sourcesEl.Elements("Source"))
            {
                var id = ((string?)sourceEl.Attribute("id"))?.Trim();
                var displayName = ((string?)sourceEl.Attribute("displayName"))?.Trim();
                var template = ((string?)sourceEl.Attribute("template"))?.Trim();

                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(template))
                    return Array.Empty<R2DatResourceDTO>();

                if (!seenIds.Add(id))
                    return Array.Empty<R2DatResourceDTO>();

                var tokensEl = sourceEl.Element("Tokens");
                if (tokensEl == null)
                    return Array.Empty<R2DatResourceDTO>();

                var tokenMap = new Dictionary<string, R2DatResourceTokenDTO>(StringComparer.Ordinal);

                foreach (var tokenEl in tokensEl.Elements("Token"))
                {
                    var tokenName = ((string?)tokenEl.Attribute("name"))?.Trim();
                    var tokenSource = ((string?)tokenEl.Attribute("source"))?.Trim();
                    var encoding = ((string?)tokenEl.Attribute("encoding"))?.Trim();
                    var requiredRaw = ((string?)tokenEl.Attribute("required"))?.Trim();

                    if (string.IsNullOrWhiteSpace(tokenName) ||
                        string.IsNullOrWhiteSpace(tokenSource) ||
                        string.IsNullOrWhiteSpace(encoding))
                        return Array.Empty<R2DatResourceDTO>();

                    if (!ValidSources.Contains(tokenSource))
                        return Array.Empty<R2DatResourceDTO>();

                    if (!ValidEncodings.Contains(encoding))
                        return Array.Empty<R2DatResourceDTO>();

                    var required = true;
                    if (!string.IsNullOrWhiteSpace(requiredRaw) && !bool.TryParse(requiredRaw, out required))
                        return Array.Empty<R2DatResourceDTO>();

                    if (!tokenMap.TryAdd(tokenName, new R2DatResourceTokenDTO(tokenSource, encoding, required)))
                        return Array.Empty<R2DatResourceDTO>();
                }

                var placeholderNames = PlaceholderRegex.Matches(template)
                    .Select(m => m.Groups["name"].Value)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();

                if (placeholderNames.Length == 0 && tokenMap.Count != 0)
                    return Array.Empty<R2DatResourceDTO>();

                if (placeholderNames.Length != tokenMap.Count)
                    return Array.Empty<R2DatResourceDTO>();

                for (var i = 0; i < placeholderNames.Length; i++)
                {
                    if (!tokenMap.ContainsKey(placeholderNames[i]))
                        return Array.Empty<R2DatResourceDTO>();
                }

                result.Add(
                    new R2DatResourceDTO(
                        string.IsNullOrWhiteSpace(displayName) ? id : displayName,
                        template,
                        tokenMap));
            }

            return result.AsReadOnly();
        }
    }
}