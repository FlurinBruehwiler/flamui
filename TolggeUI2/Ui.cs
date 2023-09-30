using System.Runtime.CompilerServices;
using TollgeUI2.UiElements;

namespace TollgeUI2;

public static class Ui
{
    public static readonly Stack<UiContainer> OpenElementStack = new();

    public static IUiContainerBuilder DivStart(
        out IUiContainerBuilder uiContainer,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        return uiContainer = DivStart(key, path, line);
    }

    public static IUiContainerBuilder DivStart(
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var el = OpenElementStack.Peek().AddChild<UiContainer>(new UiElementId(key, path, line));
        OpenElementStack.Push(el);
        el.OpenElement();
        return el;
    }

    public static void DivEnd()
    {
        OpenElementStack.Pop();
    }

    public static UiText Text(string content,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var text = OpenElementStack.Peek().AddChild<UiText>(new UiElementId(key, path, line));
        text.Content = content;
        return text;
    }

    public static void InvokeAsync(Func<Task> fun)
    {
    }
}
