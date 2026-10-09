using System.Runtime.CompilerServices;

namespace Datinate.Shared
{
    public sealed class ReferenceEqualityComparer20<T> : IEqualityComparer<T>
        where T : class
    {
        public static ReferenceEqualityComparer20<T> Instance { get; } =
            new ReferenceEqualityComparer20<T>();

        private ReferenceEqualityComparer20()
        {
        }

        public bool Equals(T? x, T? y)
            => ReferenceEquals(x, y);

        public int GetHashCode(T obj)
            => RuntimeHelpers.GetHashCode(obj);
    }
}