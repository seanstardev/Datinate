using System.Collections.ObjectModel;

namespace com.RADIO.Datinate.RMVC.Shared
{
    public sealed class R2DatResourceDTO
    {
        public string Name { get; }
        public string Template { get; }
        public IReadOnlyDictionary<string, R2DatResourceTokenDTO> Tokens { get; }

        public R2DatResourceDTO(
            string name,
            string template,
            IReadOnlyDictionary<string, R2DatResourceTokenDTO> tokens)
        {
            Name = name;
            Template = template;
            Tokens = new ReadOnlyDictionary<string, R2DatResourceTokenDTO>(
                new Dictionary<string, R2DatResourceTokenDTO>(tokens, StringComparer.Ordinal));
        }

        public string? GetUrl(string? lookup, string? system, string? name)
        {
            var url = Template;

            foreach (var kvp in Tokens)
            {
                var tokenName = kvp.Key;
                var token = kvp.Value;
                var rawValue = GetSourceValue(token.Source, name, lookup, system);

                if (string.IsNullOrWhiteSpace(rawValue))
                {
                    if (token.Required)
                        return null;

                    rawValue = string.Empty;
                }

                url = url.Replace("{" + tokenName + "}", ApplyEncoding(rawValue, token.Encoding), StringComparison.Ordinal);
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;

            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                return null;

            return uri.ToString();
        }

        private static string? GetSourceValue(
            string source,
            string? name,
            string? lookup,
            string? system)
        {
            return source.ToLowerInvariant() switch
            {
                "name" => name,
                "lookup" => lookup,
                "system" => system,
                _ => null
            };
        }

        private static string ApplyEncoding(
            string value,
            string encoding)
        {
            return encoding.ToLowerInvariant() switch
            {
                "none" => value,
                "escapedatastring" => Uri.EscapeDataString(value),
                _ => value
            };
        }
    }

    public sealed class R2DatResourceTokenDTO
    {
        public string Source { get; }
        public string Encoding { get; }
        public bool Required { get; }

        public R2DatResourceTokenDTO(string source, string encoding, bool required)
        {
            Source = source;
            Encoding = encoding;
            Required = required;
        }
    }
}
