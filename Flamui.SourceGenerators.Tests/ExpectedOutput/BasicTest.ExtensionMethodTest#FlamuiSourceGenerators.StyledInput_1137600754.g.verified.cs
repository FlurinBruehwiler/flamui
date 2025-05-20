//HintName: FlamuiSourceGenerators.StyledInput_1137600754.g.cs

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

        //(12,12)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "PI04aBcwdQ4mZuON6iDuKKUAAAA=")]
        public static FlexContainer StyledInput_1137600754(this global::Flamui.Ui receiverType, ref string text, bool multiline)
        {
            receiverType.PushScope(1137600754);
            var res = global::Sample.ComponentGallery.Test.StyledInput(receiverType, ref text, multiline);
            receiverType.PopScope();
            return res;
        }
    }
}
