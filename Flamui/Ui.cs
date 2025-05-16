using System.Runtime.CompilerServices;
using Flamui.Drawing;
using Flamui.UiElements;

namespace Flamui;

public struct CascadingStuff
{
    public Font Font;
    public ColorDefinition TextColor;
    public float TextSize;
}

//should there be a difference between Ui and UiTree? -> yes there should be,


// how should the connection between the window and the uiTree be handled? Should the uiTree know about the window? No, I don't think so.
// Should the window know about the uiTee? Yes, probably. But it would also be clean to have another separate piece that connects the whole thing, but that might over abstract the entire thing, so we really shouldn't do that.


//

/*
 * var window = Window.Create();
 * window.uiTree.OnUpdate((ui) => {
 *      using(ui.Div())
*       {
 *
*       }
 * })
 */

/*
 * We still need some kind of host, that hosts the main event loop, although.....
 *
 */


public partial class Ui
{
    public Dictionary<int, object> LastFrameDataStore = [];
    public Dictionary<int, object> CurrentFrameDataStore = [];

    public Stack<int> ScopeHashStack = new();
    public int CurrentScopeHash => ScopeHashStack.Peek();

    private readonly Stack<UiElementContainer> OpenElementStack = new();
    private UiElementContainer OpenElement => OpenElementStack.Peek();

    public UiTree Tree = null!;
    public FontManager FontManager = new();

    private Stack<CascadingStuff> CascadingStack = [];
    public CascadingStuff CascadingValues;

    [ThreadStatic]
    public static Arena Arena;

    public void PushOpenElement(UiElementContainer container)
    {
        CascadingStack.Push(CascadingValues);
        OpenElementStack.Push(container);
        ScopeHashStack.Push(container.Id.GetHashCode());
    }

    public UiElementContainer PopElement()
    {
        CascadingValues = CascadingStack.Pop();
        ScopeHashStack.Pop();
        return OpenElementStack.Pop();
    }

    public T GetData<T>(UiID id, Func<Ui, UiID, T> factoryMethod) where T : class
    {
        return GetData(id, factoryMethod, static (ui, uiId, f) => f(ui, uiId));
    }

    public T GetData<T, TContext>(UiID id, TContext context, Func<Ui, UiID, TContext, T> factoryMethod) where T : class
    {
        var globalId = HashCode.Combine(CurrentScopeHash, id.GetHashCode());
        if (LastFrameDataStore.TryGetValue(globalId, out var data))
        {
            CurrentFrameDataStore.Add(globalId, data);
            return (T)data;
        }

        var value = factoryMethod(this, id, context);
        CurrentFrameDataStore.Add(globalId, value);
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

        PushOpenElement(div);

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

        OpenElement.AddChild(svg);
        svg.ColorDefinition = colorDefinition;
        svg.Src = src;

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

    public void EnterLayoutScope(LayoutScope layoutScope)
    {
        CascadingStack.Push(layoutScope.CascadingStuff);
        OpenElementStack.Push(layoutScope.OpenElement);
    }

    public void ExitLayoutScope()
    {
        CascadingStack.Pop();
        OpenElementStack.Pop();
    }

    public LayoutScope CreateLayoutScope()
    {
        return new LayoutScope
        {
            CascadingStuff = CascadingValues,
            OpenElement = OpenElement,
            Ui = this
        };
    }
}
