//HintName: FlamuiSourceGenerators.Button_629932351.g.cs

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
        [System.Runtime.CompilerServices.InterceptsLocation(1, "h4g79Zmf0foE//ZH3eJmIaIAAAA=")]
        public static void Button_629932351(this global::Sample.ComponentGallery.Test receiverType, Flamui.Ui ui)
        {
            ui.PushScope(629932351);
            receiverType.Button(ui);
            ui.PopScope();
        }
    }
}
