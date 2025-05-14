//HintName: FlamuiSourceGenerators.Button.g.cs
namespace Sample.ComponentGallery;

public static partial class UiExtensions
{
    public static void Button(this Sample.ComponentGallery.Test receiverType, Flamui.Ui ui)
    {
        ui.ScopeHashStack.Push(123);
        var res = receiverType.Button(, ui);
        ui.ScopeHashStack.Pop();
        return res;
    }
}
