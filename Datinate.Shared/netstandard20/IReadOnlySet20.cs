namespace Datinate.Shared
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IReadOnlySet20<T> : IReadOnlyCollection<T>
    {
        bool Contains(T item);
    }

    public static class ReadOnlySet20
    {
        public static IReadOnlySet20<T> From<T>(HashSet<T> source)
            => new ReadOnlySet20<T>(source);

        public static IReadOnlySet20<T> From<T>(IReadOnlySet20<T> source)
            => source;

        public static IReadOnlySet20<T> Empty<T>()
            => EmptyCache<T>.Instance;

        private static class EmptyCache<T>
        {
            internal static readonly IReadOnlySet20<T> Instance =
                new ReadOnlySet20<T>(new HashSet<T>());
        }
    }

    public sealed class ReadOnlySet20<T> : IReadOnlySet20<T>
    {
        private readonly HashSet<T> _source;

        public ReadOnlySet20(HashSet<T> source)
        {
            _source = source;
        }

        public int Count => _source.Count;

        public bool Contains(T item) => _source.Contains(item);

        public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}