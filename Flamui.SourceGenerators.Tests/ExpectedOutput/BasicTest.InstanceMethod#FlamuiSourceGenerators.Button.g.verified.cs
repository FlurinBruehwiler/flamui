//HintName: FlamuiSourceGenerators.Button.g.cs
namespace Sample.ComponentGallery;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
file sealed class InterceptsLocationAttribute(string filePath, int line, int column) : Attribute;


public static partial class InterceptionMethods
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    [InterceptsLocation("1", 2, 3)]
    public static void Button(this Sample.ComponentGallery.Test receiverType, Flamui.Ui ui)
    {
        ui.ScopeHashStack.Push(123);
        receiverType.Button(ui);
        ui.ScopeHashStack.Pop();
    }
}
