#if NETSTANDARD2_0
using System;

namespace System.Diagnostics.CodeAnalysis
{
    // Supplies compiler-recognised nullable metadata missing from .NET Standard 2.0.
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue)
        {
            ReturnValue = returnValue;
        }

        public bool ReturnValue { get; }
    }
}
#endif
