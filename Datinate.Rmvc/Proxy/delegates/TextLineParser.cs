namespace com.RADIO.Datinate.RMVC
{
    public static class TextLineParser
    {
        public static IReadOnlyCollection<string> Parse(string content)
        {
            if (string.IsNullOrEmpty(content))
                return Array.Empty<string>();

            var lines = new List<string>();

            var span = content.AsSpan();
            var i = 0;

            while (i < span.Length)
            {
                var lineStart = i;

                while (i < span.Length && span[i] != '\n' && span[i] != '\r')
                    i++;

                var line = span.Slice(lineStart, i - lineStart).Trim();

                if (!line.IsEmpty)
                    lines.Add(line.ToString());

                if (i < span.Length && span[i] == '\r')
                    i++;

                if (i < span.Length && span[i] == '\n')
                    i++;
            }

            return lines;
        }
    }
}
