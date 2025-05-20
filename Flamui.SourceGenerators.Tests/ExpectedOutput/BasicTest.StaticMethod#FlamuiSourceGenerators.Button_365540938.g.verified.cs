//HintName: FlamuiSourceGenerators.Button_365540938.g.cs

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

        //(10,14)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "LFgZ0eqoB3mDEOPLPSaaOo8AAAA=")]
        public static void Button_365540938(Flamui.Ui ui)
        {
            ui.PushScope(365540938);
            global::Sample.ComponentGallery.Test.Button(ui);
            ui.PopScope();
        }
    }
}
