//HintName: FlamuiSourceGenerators.Build_1313965041.g.cs

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
        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "Build")]
        private static extern void Invoke_1313965041(global::Sample.ComponentGallery.GenericType<T> target, Flamui.Ui ui);

        //(19,9)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "Nl9hWOXg211UQh7iFwfJaA0BAAA=")][System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]

        public static void ジ_1313965041<T>(this global::Sample.ComponentGallery.GenericType<T> receiverType, Flamui.Ui ui)
        {
            ui.PushScope(1313965041);
            Invoke_1313965041(ui);
            ui.PopScope();
        }
    }
}
