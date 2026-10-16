#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    // NOTE: netstandard2.0 lacks the compiler marker needed by record/init properties.
    internal static class IsExternalInit { }
}
#endif
