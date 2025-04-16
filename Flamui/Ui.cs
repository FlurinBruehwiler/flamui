using System.Runtime.CompilerServices;
using Flamui.Drawing;
using Flamui.UiElements;
using Microsoft.Extensions.DependencyInjection;

namespace Flamui;

public struct CascadingStuff
{
    public Font Font;
    public ColorDefinition TextColor;
    public float TextSize;
}

public partial class Ui
{
    public readonly Stack<IStackItem> OpenElementStack = new();
    private IStackItem OpenElement => OpenElementStack.Peek();
    public UiTree Tree = null!;
    public FontManager FontManager;

    public Stack<CascadingStuff> CascadingStack = [];
    public CascadingStuff CascadingValues;

    [ThreadStatic]
    public static Arena Arena;

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
            var comp = (FlamuiComponent)ActivatorUtilities.CreateInstance(ui.Tree.ServiceProvider, type);
            comp.OnInitialized();
            return comp;
        });
    }

    public T GetData<T>(UiID id, Func<Ui, UiID, T> factoryMethod) where T : notnull
    {
        return GetData(id, factoryMethod, static (ui, uiId, f) => f(ui, uiId));
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

    public FlexContainer Div(
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiID(key, path, line, typeof(FlexContainer).GetHashCode());
        var div = GetData(id, static (ui, id) => new FlexContainer
        {
            Id = id,
            Tree = ui.Tree
        });

        OpenElement.AddChild(div);

        OpenElementStack.Push(div);
        CascadingStack.Push(CascadingValues);

        div.OpenElement();

        return div;
    }

    public UiText Text(ArenaString content,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiID(key, path, line, typeof(UiText).GetHashCode());
        var text = GetData(id, static (ui, id) => new UiText
        {
            Id = id,
            Tree = ui.Tree,
        });

        OpenElement.AddChild(text);

        text.UiTextInfo.Content = content;
        text.UiTextInfo.Font = CascadingValues.Font;
        text.UiTextInfo.Color = CascadingValues.TextColor;
        text.UiTextInfo.Size = CascadingValues.TextSize;

        return text;
    }

    public UiSvg SvgImage(ArenaString src, ColorDefinition? colorDefinition = null,
        string key = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = -1)
    {
        var id = new UiID(key, path, line, typeof(UiSvg).GetHashCode());
        var svg = GetData(id, static (ui, id) => new UiSvg
        {
            Id = id,
            Tree = ui.Tree
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
            Tree = ui.Tree,
        });

        image.Src = src;
        OpenElement.AddChild(image);

        return image;
    }

    public void ResetStuff()
    {
        CascadingStack.Clear();
        CascadingValues = new CascadingStuff();
        CascadingValues.Font = FontManager.GetFont("segoeui.ttf");
        CascadingValues.TextColor = C.Black;
        CascadingValues.TextSize = 15;

        OpenElementStack.Clear();
    }
}
