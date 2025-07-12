using System.Runtime.CompilerServices;
using Flamui.UiElements;

namespace Flamui;

public struct LayoutScopeDisposable : IDisposable
{
    public required LayoutScope LayoutScope;

    public void Dispose()
    {
        LayoutScope.Ui.PopScope();
        LayoutScope.Ui.ExitLayoutScope();
    }
}

public struct LayoutScope
{
    public required CascadingStuff CascadingStuff;
    public required UiElementContainer OpenElement;
    public required Ui Ui;

    public LayoutScopeDisposable Enter([CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        Ui.CreateIdScope(file, lineNumber);
        LayoutScopeDisposable disposable = new()
        {
            LayoutScope = this
        };

        Ui.EnterLayoutScope(this);

        return disposable;
    }
}