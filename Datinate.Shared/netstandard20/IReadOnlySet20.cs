namespace Datinate.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IReadOnlySet20<T> : IReadOnlyCollection<T>
    {
        bool Contains(T item);
    }

    public sealed class ReadOnlySet20<T> : IReadOnlySet20<T>
    {
        private readonly HashSet<T> _source;

        public ReadOnlySet20(HashSet<T> source)
        {
            _source = source
                ?? throw new ArgumentNullException(nameof(source));
        }

        public int Count => _source.Count;

        public bool Contains(T item) => _source.Contains(item);

        public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}