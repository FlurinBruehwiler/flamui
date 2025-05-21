//HintName: FlamuiSourceGenerators.Build_366951719.g.cs

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
        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "Build")]
        private static extern void Invoke_366951719(global::Sample.ComponentGallery.GenericType<T> target, Flamui.Ui ui);

        //(19,9)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "D7yeHaoyFFqNeawGafEo5R8BAAA=")]
        public static void Build_366951719<T>(this global::Sample.ComponentGallery.GenericType<T> receiverType, Flamui.Ui ui)
        {
            ui.PushScope(366951719);
            Invoke_366951719<T>(ui);
            ui.PopScope();
        }
    }
}
