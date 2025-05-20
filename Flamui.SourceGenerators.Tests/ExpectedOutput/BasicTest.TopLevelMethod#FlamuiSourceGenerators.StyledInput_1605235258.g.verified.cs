//HintName: FlamuiSourceGenerators.StyledInput_1605235258.g.cs

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
        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "StyledInput")]
        private static extern void Invoke_1605235258(global::Program target, Flamui.Ui ui);

        //(5,1)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, "JLYuF5NNI0b2Qor5HK3sQCAAAAA=")]
        public static void StyledInput_1605235258(this global::Program receiverType, Flamui.Ui ui)
        {
            ui.PushScope(1605235258);
            Invoke_1605235258(ui);
            ui.PopScope();
        }
    }
}
