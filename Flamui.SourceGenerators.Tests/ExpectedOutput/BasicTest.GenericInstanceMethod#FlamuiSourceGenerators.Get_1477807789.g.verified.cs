//HintName: FlamuiSourceGenerators.Get_1477807789.g.cs

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

        //(10,30)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "j9GwCBaktug50WhOdZUpR5YAAAA=")][System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]

        public static ref T ジ_1477807789<T>(this global::Flamui.Ui receiverType, T initialValue) where T : struct
        {
            receiverType.PushScope(1477807789);
            ref var res = ref receiverType.Get<T>(initialValue);
            receiverType.PopScope();
            return ref res;
        }
    }
}
