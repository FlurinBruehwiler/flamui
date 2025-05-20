//HintName: FlamuiSourceGenerators.Div_1374120882.g.cs

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

        //(10,19)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "u5KqjaoanfiSsDXpHM0k2Y0AAAA=")]
        public static global::Flamui.UiElements.FlexContainer Div_1374120882(this global::Flamui.Ui receiverType)
        {
            receiverType.PushScope(1374120882);
            var res = receiverType.Div();
            receiverType.PopScope();
            return res;
        }
    }
}
