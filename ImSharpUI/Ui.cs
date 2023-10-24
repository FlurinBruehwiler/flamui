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
    public static Stack<ValueTuple<UiComponent, bool>> OpenComponents = new();
    public static Stack<UiElementContainer> OpenElementStack = new();
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

    public static UiContainer DivStart(
        out UiContainer uiContainer,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        return uiContainer = DivStart(key, path, line);
    }

    public static UiContainer DivStart(
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

    public static T GetComponent<T>(string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiElementId(key, path, line);
        return (T)GetComponentInternal(typeof(T), id, out _);
    }

    public static object GetComponent(Type type, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiElementId(key, path, line);
        return GetComponentInternal(type, id, out _);
    }

    private static object GetComponentInternal(Type type, UiElementId id, out bool wasNewlyCreated)
    {
        wasNewlyCreated = false;
        LastKey = id.Key;
        var parentContainer = OpenElementStack.Peek();
        if (parentContainer.OldDataById.TryGetValue(id, out var data))
        {
            parentContainer.Data.Add(new Data(data, id));
            return data;
        }

        wasNewlyCreated = true;
        var newData = Activator.CreateInstance(type);
        if (newData is null)
            throw new Exception();
        parentContainer.Data.Add(new Data(newData, id));
        return newData;
    }

    public static T StartComponent<T>(out T component, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1) where T : UiComponent
    {
        component = (T)GetComponentInternal(typeof(T), new UiElementId(key, path, line), out var isNew);
        OpenComponents.Push((component, isNew));
        return component;
    }

    public static T EndComponent<T>() where T : UiComponent
    {
        var (component, isNew) = OpenComponents.Pop();
        if (isNew)
        {
            component.OnInitialized();
        }

        component.Build();
        return (T)component;
    }

    public static T Get<T>(string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1) where T : UiElement, new()
    {
        return OpenElementStack.Peek().AddChild<T>(new UiElementId(key, path, line));
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

    public static UiSvg SvgImage(string src, ColorDefinition? colorDefinition = null,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var svg = OpenElementStack.Peek().AddChild<UiSvg>(new UiElementId(key, path, line));
        svg.ColorDefinition = colorDefinition;
        svg.Src = src;
        return svg;
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

    // public static void SetFocus(UiContainer uiContainer)
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
