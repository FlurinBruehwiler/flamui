//HintName: FlamuiSourceGenerators.Get_125634764.g.cs

using System;
using System.Diagnostics;

#nullable enable
namespace System.Runtime.CompilerServices
{
    [Conditional("DEBUG")] // not needed post-build, so can evaporate it
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    sealed file sealed class InterceptsLocationAttribute : Attribute
    {
        public InterceptsLocationAttribute(int version, string data)
        {
            _ = version;
            _ = data;
        }
    }
}

namespace InterceptorNamespace
{
    public static partial class InterceptionMethods
    {

        //(10,41)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "2pyApQnHeLSai8zVP1vcw6EAAAA=")][System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]

        public static ref T ジ_125634764<T>(this global::Flamui.Ui receiverType, T initialValue) where T : struct
        {
            receiverType.PushScope(125634764);
            ref var res = ref receiverType.Get<T>(initialValue);
            receiverType.PopScope();
            return ref res;
        }
    }
}
