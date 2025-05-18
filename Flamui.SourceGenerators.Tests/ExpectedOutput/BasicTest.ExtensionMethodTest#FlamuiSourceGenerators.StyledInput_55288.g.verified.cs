//HintName: FlamuiSourceGenerators.StyledInput_55288.g.cs

using System;
using System.Diagnostics;

#nullable enable
namespace System.Runtime.CompilerServices
{
    // this type is needed by the compiler to implement interceptors,
    // it doesn't need to come from the runtime itself

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
        [System.Runtime.CompilerServices.InterceptsLocation(1, "8jt97AV577ddBFXYzYsTa5oAAAA=")]
        public static FlexContainer StyledInput_0b94d(this global::Flamui.Ui receiverType, ref string text, bool multiline)
        {
            receiverType.ScopeHashStack.Push(392633724);
            var res = global::Sample.ComponentGallery.Test.StyledInput(ref text, multiline);
            receiverType.ScopeHashStack.Pop();
            return res;
        }
    }
}
