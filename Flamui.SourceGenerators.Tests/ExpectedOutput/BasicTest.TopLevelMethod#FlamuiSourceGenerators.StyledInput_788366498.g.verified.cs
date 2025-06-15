//HintName: FlamuiSourceGenerators.StyledInput_788366498.g.cs

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
        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "StyledInput")]
        private static extern void Invoke_788366498(global::Program target, Flamui.Ui ui);

        //(5,1)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "qoogqyK4pU/0fP/cRujNWB0AAAA=")][System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]

        public static void ジ_788366498(this global::Program receiverType, Flamui.Ui ui)
        {
            ui.PushScope(788366498);
            Invoke_788366498(ui);
            ui.PopScope();
        }
    }
}
