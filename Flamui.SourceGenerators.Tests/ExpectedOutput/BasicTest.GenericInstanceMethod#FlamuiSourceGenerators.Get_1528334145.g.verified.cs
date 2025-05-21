//HintName: FlamuiSourceGenerators.Get_1528334145.g.cs

using System;
using System.Diagnostics;

#nullable enable
namespace System.Runtime.CompilerServices
{
    [Conditional("DEBUG")] // not needed post-build, so can evaporate it
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    sealed file class InterceptsLocationAttribute : Attribute
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

        //(10,30)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "YZv9kaYZhzXYJmCMM9j1W58AAAA=")]
        public static ref T Get_1528334145<T>(this global::Flamui.Ui receiverType, T initialValue) where T : struct
        {
            receiverType.PushScope(1528334145);
            ref var res = ref receiverType.Get<T>(initialValue);
            receiverType.PopScope();
            return ref res;
        }
    }
}
