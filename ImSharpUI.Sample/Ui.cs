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
    public static UiWindow Window = null!;
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

    public static string LastKey;

    public static T Get<T>(string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        LastKey = key;
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

    // public static void SetFocus(IUiContainerBuilder uiContainer)
    // {
    //     if (UiWindow is null)
    //         throw new Exception();
    //
    //     if (!((UiContainer)uiContainer).PFocusable)
    //         throw new Exception();
    //
    //     UiWindow.ActiveDiv = (UiContainer)uiContainer;
    // }

    public static void InvokeAsync(Func<Task> fun)
    {
    }
}
