using System.Runtime.CompilerServices;
using Flamui.Layouting;
using Flamui.UiElements;
using Microsoft.Extensions.DependencyInjection;

namespace Flamui;

public partial class Ui
{
    public readonly Stack<IStackItem> OpenElementStack = new();

    private IStackItem OpenElement => OpenElementStack.Peek();

    public UiWindow Window = null!;
    public IUiElement Root;

    public T GetComponent<T>(string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1) where T : FlamuiComponent
    {
        var id = new UiID(key, path, line, typeof(T).GetHashCode());
        return (T)GetComponentInternal(typeof(T), id);
    }

    private object GetComponentInternal(Type type, UiID id)
    {
        return GetData(id, type, static (ui, _, type) =>
        {
            var comp = (FlamuiComponent)ActivatorUtilities.CreateInstance(ui.Window.ServiceProvider, type);
            comp.OnInitialized();
            return comp;
        });
    }

    public T GetData<T>(UiID id, Func<Ui, UiID, T> factoryMethod) where T : notnull
    {
        return GetData(id, 0, (ui, uiId, _) => factoryMethod(ui, uiId));
    }

    public T GetData<T, TContext>(UiID id, TContext context, Func<Ui, UiID, TContext, T> factoryMethod) where T : notnull
    {
        var parentContainer = OpenElementStack.Peek();
        if (parentContainer.DataStore.OldDataById.TryGetValue(id, out var data))
        {
            parentContainer.DataStore.Data.Add(id, data);
            return (T)data;
        }

        var value = factoryMethod(this, id, context);
        parentContainer.DataStore.Data.Add(id, value);
        return value;
    }

    public UiContainer Div(
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiID(key, path, line, typeof(UiContainer).GetHashCode());
        var div = GetData(id, static (ui, id) => new UiContainer
        {
            Id = id,
            Window = ui.Window
        });

        OpenElement.AddChild(div);

        OpenElementStack.Push(div);

        div.OpenElement();

        return div;
    }

    public UiText Text(string content,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiID(key, path, line, typeof(Text).GetHashCode());
        var text = GetData(id, static (ui, id) => new UiText
        {
            Id = id,
            Window = ui.Window,
        });

        OpenElement.AddChild(text);

        text.Content = content;

        return text;
    }

    public IDisposable CascadingValue<T>(T value,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiID(key, path, line, typeof(T).GetHashCode());
        var provider = GetData(id, static (_, _) => new CascadingValueProvider<T>());

        provider.Data = value;

        return provider;
    }

    public UiSvg SvgImage(string src, ColorDefinition? colorDefinition = null,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiID(key, path, line, typeof(UiSvg).GetHashCode());
        var svg = GetData(id, static (ui, id) => new UiSvg
        {
            Id = id,
            Window = ui.Window
        });

        svg.ColorDefinition = colorDefinition;
        svg.Src = src;
        OpenElement.AddChild(svg);

        return svg;
    }

    public UiImage Image(string src,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiID(key, path, line, typeof(UiImage).GetHashCode());
        var image = GetData(id, static (ui, id) => new UiImage
        {
            Id = id,
            Window = ui.Window,
        });

        image.Src = src;
        OpenElement.AddChild(image);

        return image;
    }
}
