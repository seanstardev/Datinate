using System.Diagnostics.CodeAnalysis;

namespace Datinate.Shared
{
    public static class String20
    {
        public static bool IsNullOrWhiteSpace20([NotNullWhen(false)] string? value)
            => string.IsNullOrWhiteSpace(value);

        public static bool IsNullOrEmpty20([NotNullWhen(false)] string? value)
            => string.IsNullOrEmpty(value);
    }
}
