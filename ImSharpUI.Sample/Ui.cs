using System.Runtime.CompilerServices;
using ImSharpUISample.UiElements;
using SDL2;

namespace ImSharpUISample;

public class SubStack
{
    public required Stack<UiContainer> PreviousSubStack { get; set; }
    public required Stack<UiContainer> CurrentStack { get; set; }
}

public static partial class Ui
{
    public static Stack<ComponentData> OpenComponents = new();
    public static Stack<UiContainer> OpenElementStack = new();
    public static List<UiContainer> AbsoluteDivs = new();
    public static Window? Window = null;
    public static List<UiContainer> DeferedRenderedContainers = new();
    public static UiContainer Root = null!;
    public static SubStack StartSubStack(UiContainer temporaryContainer)
    {
        var substack = new SubStack //ToDo resuse to avoid memory allocation
        {
            PreviousSubStack = OpenElementStack,
            CurrentStack = new Stack<UiContainer>()
        };
        OpenElementStack = substack.CurrentStack;
        OpenElementStack.Push(temporaryContainer);
        return substack;
    }

    public static List<UiElement> EndSubStack(SubStack subStack)
    {
        OpenElementStack = subStack.PreviousSubStack;
        return subStack.CurrentStack.Pop().Children;
    }

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

    public static UiImage Image(string src,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var text = OpenElementStack.Peek().AddChild<UiImage>(new UiElementId(key, path, line));
        text.Src = src;
        return text;
    }

    public static string GetTextInput()
    {
        if (Window is null)
            throw new Exception();

        return Window.TextInput;
    }

    public static void SetFocus(IUiContainerBuilder uiContainer)
    {
        if (Window is null)
            throw new Exception();

        if (!((UiContainer)uiContainer).PFocusable)
            throw new Exception();

        Window.ActiveDiv = (UiContainer)uiContainer;
    }

    public static bool IsKeyPressed(SDL.SDL_Scancode scancode)
    {
        if (Window is null)
            throw new Exception();

        return Window.Keypressed.Contains(scancode);
    }

    public static bool IsKeyDown(SDL.SDL_Scancode scancode)
    {
        if (Window is null)
            throw new Exception();

        return Window.Keydown.Contains(scancode);
    }

    public static bool TryGetMouseClickPosition(out Vector2Int pos)
    {
        pos = new Vector2Int();

        if (Window is null)
            throw new Exception();

        if (Window.ClickPos is not { } p)
            return false;

        pos = p;
        return true;
    }

    public static void InvokeAsync(Func<Task> fun)
    {
    }
}
