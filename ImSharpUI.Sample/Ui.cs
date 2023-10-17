using System.Collections.ObjectModel;
using System.Numerics;
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
    public static Stack<UiElementContainer> OpenElementStack = new();
    public static List<UiContainer> AbsoluteDivs = new();
    public static Window? Window = null;
    public static List<UiContainer> DeferedRenderedContainers = new();
    public static UiContainer Root = null!;
    // public static SubStack StartSubStack(UiContainer temporaryContainer)
    // {
    //     var substack = new SubStack //ToDo resuse to avoid memory allocation
    //     {
    //         PreviousSubStack = OpenElementStack,
    //         CurrentStack = new Stack<UiContainer>()
    //     };
    //     OpenElementStack = substack.CurrentStack;
    //     OpenElementStack.Push(temporaryContainer);
    //     return substack;
    // }

    // public static List<UiElement> EndSubStack(SubStack subStack)
    // {
    //     OpenElementStack = subStack.PreviousSubStack;
    //     return subStack.CurrentStack.Pop().Children;
    // }

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
        return Start<UiContainer>(key, path, line);
    }

    public static void DivEnd()
    {
        End<UiContainer>();
    }

    public static T Start<T>(string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1) where T : UiElementContainer, new()
    {
        var el = OpenElementStack.Peek().AddChild<T>(new UiElementId(key, path, line));
        OpenElementStack.Push(el);
        el.OpenElement();
        return el;
    }

    public static void End<T>() where T : UiElementContainer, new()
    {
        OpenElementStack.Pop().CloseElement();
    }

    public static T Get<T>(string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var parentContainer = OpenElementStack.Peek();
        var id = new UiElementId(key, path, line);
        if (parentContainer.OldDataById.TryGetValue(id, out var data))
        {
            parentContainer.Data.Add(new Data(data, id));
            return (T)data;
        }

        var newData = Activator.CreateInstance<T>();
        if (newData is null)
            throw new Exception();
        parentContainer.Data.Add(new Data(newData, id));
        return newData;
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

    public static int GetScrollDelta()
    {
        return Window!.ScrollDelta;
    }

    public static bool TryGetMouseClickPosition(out Vector2 pos)
    {
        pos = new Vector2();

        if (Window is null)
            throw new Exception();

        if (Window.ClickPos is not { } p)
            return false;

        pos = p;
        return true;
    }

    /// <summary>
    /// Check if a mouse button has been pressed once
    /// </summary>
    /// <returns></returns>
    public static bool IsMouseButtonPressed()
    {
        if (Window is null)
            throw new Exception();

        return Window.IsMouseButtonNewlyPressed;
    }

    /// <summary>
    /// Check if a mouse button is being pressed
    /// </summary>
    /// <returns></returns>
    public static bool IsMouseButtonDown()
    {
        return Window!.IsMouseButtonDown;
    }

    /// <summary>
    /// Check if a mouse button has been released once
    /// </summary>
    /// <returns></returns>
    public static bool IsMouseButtonReleased()
    {
        return Window!.MouseButtonUp;
    }

    public static Vector2 GetMousePosition()
    {
        if (Window is null)
            throw new Exception();

        return Window.MousePosition;
    }

    public static Vector2 GetMouseDelta()
    {
        if (Window is null)
            throw new Exception();

        return Window.MousePosition - Window.LastMousePosition;
    }

    public static void InvokeAsync(Func<Task> fun)
    {
    }
}
