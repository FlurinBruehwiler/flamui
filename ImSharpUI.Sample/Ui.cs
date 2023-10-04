using System.Runtime.CompilerServices;
using ImSharpUISample.UiElements;

namespace ImSharpUISample;

public static class Ui
{
    public static readonly Stack<UiContainer> OpenElementStack = new();
    public static List<UiContainer> AbsoluteDivs = new();

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
        var div = OpenElementStack.Pop();
        if (div.PAbsolute)
        {
            AbsoluteDivs.Add(div);
        }
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

    public static UiSvg SvgImage(string src,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var text = OpenElementStack.Peek().AddChild<UiSvg>(new UiElementId(key, path, line));
        text.Src = src;
        return text;
    }

    public static void InvokeAsync(Func<Task> fun)
    {
    }
}
