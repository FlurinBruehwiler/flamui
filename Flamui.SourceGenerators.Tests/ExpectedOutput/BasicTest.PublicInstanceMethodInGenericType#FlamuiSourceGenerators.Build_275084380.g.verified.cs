//HintName: FlamuiSourceGenerators.Build_275084380.g.cs

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

        //(11,11)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "oFiGOyKC8E0rKrrTOW2utLgAAAA=")]
        public static void Build_275084380<T>(this global::Sample.ComponentGallery.GenericType<T> receiverType, Flamui.Ui ui)
        {
            ui.PushScope(275084380);
            receiverType.Build<T>(ui);
            ui.PopScope();
        }
    }
}
