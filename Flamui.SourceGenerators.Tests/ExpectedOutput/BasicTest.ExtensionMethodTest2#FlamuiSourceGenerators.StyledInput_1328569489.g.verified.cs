//HintName: FlamuiSourceGenerators.StyledInput_1328569489.g.cs

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

        //(12,9)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "EUcgOsHhGU2OCgHbR9RqM5gAAAA=")]
        public static FlexContainer StyledInput_1328569489(Flamui.Ui ui, ref string text, bool multiline)
        {
            ui.PushScope(1328569489);
            var res = global::Sample.ComponentGallery.Test.StyledInput(ui, ref text, multiline);
            ui.PopScope();
            return res;
        }
    }
}
