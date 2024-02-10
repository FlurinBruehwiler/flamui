using System.Runtime.CompilerServices;
using Flamui.UiElements;
using Microsoft.Extensions.DependencyInjection;

namespace Flamui;

public class SubStack
{
    public required Stack<UiContainer> PreviousSubStack { get; set; }
    public required Stack<UiContainer> CurrentStack { get; set; }
}

public partial class Ui
{
    public Stack<ValueTuple<FlamuiComponent, bool>> OpenComponents = new();
    public Stack<UiElementContainer> OpenElementStack = new();
    public UiWindow Window = null!;
    public UiContainer Root = null!;
    // public SubStack StartSubStack(UiContainer temporaryContainer)
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

    // public List<UiElement> EndSubStack(SubStack subStack)
    // {
    //     OpenElementStack = subStack.PreviousSubStack;
    //     return subStack.CurrentStack.Pop().Children;
    // }

    public UiContainer DivStart(
        out UiContainer uiContainer,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        return uiContainer = DivStart(key, path, line);
    }

    public UiContainer DivStart(
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        return null;
        // return Start<UiContainer>(key, path, line);
    }

    public void DivEnd()
    {
        // End<UiContainer>();
    }

    public T Start<T>(string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1) where T : UiElementContainer, new()
    {
        var el = OpenElementStack.Peek().AddChild<T>(new UiElementId(key, path, line));
        OpenElementStack.Push(el);
        el.OpenElement();
        return el;
    }

    public void End<T>() where T : UiElementContainer, new()
    {
        OpenElementStack.Pop().CloseElement();
    }

    public string LastKey;

    public T GetComponent<T>(string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1) where T : FlamuiComponent
    {
        var id = new UiElementId(key, path, line);
        return (T)GetComponentInternal(typeof(T), id, out _);
    }

    public object GetComponent(Type type, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiElementId(key, path, line);
        return GetComponentInternal(type, id, out _);
    }

    private object GetComponentInternal(Type type, UiElementId id, out bool wasNewlyCreated)
    {
        wasNewlyCreated = false;
        LastKey = id.Key;
        var parentContainer = OpenElementStack.Peek();
        if (parentContainer.OldDataById.TryGetValue(id, out var data))
        {
            parentContainer.Data.Add(id, data);
            return data;
        }

        wasNewlyCreated = true;
        var newData = ActivatorUtilities.CreateInstance(Window.ServiceProvider, type);
        if (newData is null)
            throw new Exception();
        parentContainer.Data.Add(id, newData);

        if (newData is FlamuiComponent flamuiComponent)
        {
            flamuiComponent.OnInitialized();//todo make betta :)
        }

        return newData;
    }

    public T GetData<T>(T initialValue, out UiElementId id, string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1) where T : notnull
    {
        id = new UiElementId(key, path, line);
        var parentContainer = OpenElementStack.Peek();
        if (parentContainer.OldDataById.TryGetValue(id, out var data))
        {
            parentContainer.Data.Add(id, data);
            return (T)data;
        }
        parentContainer.Data.Add(id, initialValue);
        return initialValue;
    }

    public void SetData<T>(string value, UiElementId id) where T : notnull
    {
        var parentContainer = OpenElementStack.Peek();
        parentContainer.Data[id] = value;
    }

    public T Get<T>(string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1) where T : UiElement, new()
    {
        return OpenElementStack.Peek().AddChild<T>(new UiElementId(key, path, line));
    }

    public UiText Text(string content,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var text = OpenElementStack.Peek().AddChild<UiText>(new UiElementId(key, path, line));
        text.Content = content;
        return text;
    }

    public UiSvg SvgImage(string src, ColorDefinition? colorDefinition = null,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var svg = OpenElementStack.Peek().AddChild<UiSvg>(new UiElementId(key, path, line));
        svg.ColorDefinition = colorDefinition;
        svg.Src = src;
        return svg;
    }

    public UiImage Image(string src,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var text = OpenElementStack.Peek().AddChild<UiImage>(new UiElementId(key, path, line));
        text.Src = src;
        return text;
    }

    // public void SetFocus(UiContainer uiContainer)
    // {
    //     if (UiWindow is null)
    //         throw new Exception();
    //
    //     if (!((UiContainer)uiContainer).PFocusable)
    //         throw new Exception();
    //
    //     UiWindow.ActiveDiv = (UiContainer)uiContainer;
    // }

    public void InvokeAsync(Func<Task> fun)
    {
    }
}
