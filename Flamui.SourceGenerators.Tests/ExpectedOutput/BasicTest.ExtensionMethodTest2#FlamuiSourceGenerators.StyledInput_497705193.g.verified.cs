//HintName: FlamuiSourceGenerators.StyledInput_497705193.g.cs

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

        //(12,9)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "C/o2/mwYtxy6Y4eCx2Oszo4AAAA=")][System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]

        public static FlexContainer ジ_497705193(Flamui.Ui ui, ref string text, bool multiline)
        {
            ui.PushScope(497705193);
            var res = global::Sample.ComponentGallery.Test.StyledInput(ui, ref text, multiline);
            ui.PopScope();
            return res;
        }
    }
}
