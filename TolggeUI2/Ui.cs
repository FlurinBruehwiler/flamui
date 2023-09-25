using System.Runtime.CompilerServices;

namespace ImmediateModeUiFrameworkTest;

public static class Ui
{
    public static readonly Stack<UiContainer> OpenElementStack = new();

    public static UiContainer DivStart(
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

public interface IUiElement
{
    public UiElementId Id { get; init; }
}

public class UiText : IUiElement
{
    public UiElementId Id { get; init; }
    public string Content { get; set; }
}

public class UiContainer : IUiElement
{
    public UiElementId Id { get; init; }
    private List<IUiElement> Children { get; set; } = new();
    public Dictionary<UiElementId, IUiElement> OldChildrenById { get; set; } = new();
    public bool FocusIn { get; set; }
    public bool FocusOut { get; set; }
    public bool Clicked { get; set; }
    public string PColor { get; set; }
    public UiContainer Color(string color)
    {
        PColor = color;
        return this;
    }

    public void OpenElement()
    {
        OldChildrenById.Clear();
        foreach (var uiElementClass in Children)
        {
            OldChildrenById.Add(uiElementClass.Id, uiElementClass);
        }

        Children.Clear();
    }

    public T AddChild<T>(UiElementId uiElementId) where T : IUiElement, new()
    {
        if (OldChildrenById.TryGetValue(uiElementId, out var child))
        {
            Children.Add(child);
            return (T)child;
        }

        var newChild = new T
        {
            Id = uiElementId
        };

        Children.Add(newChild);
        return newChild;
    }
}

public record struct UiElementId(string Key, string Path, int Line);
