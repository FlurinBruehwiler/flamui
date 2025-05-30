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

public struct IdScopeDisposable : IDisposable
{
    public Ui ui;

    public void Dispose()
    {
        ui.PopScope();
    }
}

public partial class Ui
{
    public Dictionary<int, object> LastFrameDataStore = [];
    public Dictionary<int, object> CurrentFrameDataStore = [];

    public Dictionary<int, IntPtr> UnmanagedLastFrameDataStore = [];
    public Dictionary<int, IntPtr> UnmanagedCurrentFrameDataStore = [];

    public ChunkedList<object> LastFrameRefObjects = new(100);
    public ChunkedList<object> CurrentFrameRefObjects = new(100);

    private Stack<int> ScopeHashStack = new();
    public int CurrentScopeHash => ScopeHashStack.Peek();

    private readonly Stack<UiElementContainer> OpenElementStack = new();
    private UiElementContainer OpenElement => OpenElementStack.Peek();

    public UiTree Tree = null!;
    public FontManager FontManager = new();

    private Stack<CascadingStuff> CascadingStack = [];
    public CascadingStuff CascadingValues;
    public UiElementContainer Root;

    [ThreadStatic]
    public static Arena Arena;

    //used by source gen
    [NoScopeGeneration]
    public void PushScope(int hash)
    {
        if (ScopeHashStack.TryPeek(out var res))
        {
            ScopeHashStack.Push(HashCode.Combine(hash, res));
        }
        else
        {
            ScopeHashStack.Push(hash);
        }
    }

    //used by source gen
    [NoScopeGeneration]
    public void PopScope()
    {
        ScopeHashStack.Pop();
    }

    [NoScopeGeneration]
    public IdScopeDisposable CreateIdScope(int hash)
    {
        PushScope(hash);
        return new IdScopeDisposable
        {
            ui = this
        };
    }

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

    public unsafe ref T Get<T>(T initialValue) where T : unmanaged
    {
        if (UnmanagedLastFrameDataStore.TryGetValue(CurrentScopeHash, out var lastPtr))
        {
            var lastValue = *(T*)lastPtr;
            var ptr = Tree.Arena.Allocate(lastValue);
            UnmanagedCurrentFrameDataStore.Add(CurrentScopeHash, (nint)ptr);
            return ref Unsafe.AsRef<T>(ptr);
        }

        var ptr2 = Tree.Arena.Allocate(initialValue);
        if (!UnmanagedCurrentFrameDataStore.TryAdd(CurrentScopeHash, (nint)ptr2))
        {

        }
        return ref Unsafe.AsRef<T>(ptr2);
    }

    public T GetObj<T>() where T : class, new()
    {
        if (LastFrameDataStore.TryGetValue(CurrentScopeHash, out var lastValue))
        {
            CurrentFrameDataStore.Add(CurrentScopeHash, lastValue);
            return (T)lastValue;
        }

        var initialValue = new T();
        CurrentFrameDataStore.Add(CurrentScopeHash, initialValue);
        return initialValue;
    }

    public ref T GetObjRef<T>(T initialValue) where T : class
    {
        var idx = CurrentFrameRefObjects.Count;

        if (UnmanagedLastFrameDataStore.TryGetValue(CurrentScopeHash, out var lastIdx))
        {
            var item = LastFrameRefObjects[(int)lastIdx];
            CurrentFrameRefObjects.Add(item);
        }

        CurrentFrameRefObjects.Add(initialValue);
        if (!UnmanagedCurrentFrameDataStore.TryAdd(CurrentScopeHash, idx))
        {

        }
        ref object y = ref CurrentFrameRefObjects[idx];
        _ = (T)y; //to make sure that x has the correct type
        return ref Unsafe.As<object, T>(ref y);
    }

    public ref string GetString(string initialValue)
    {
        return ref GetObjRef(initialValue);
    }

    public T GetData<T>(Func<Ui, T> factoryMethod) where T : class
    {
        return GetData(factoryMethod, static (ui, f) => f(ui));
    }

    public T GetData<T, TContext>(TContext context, Func<Ui, TContext, T> factoryMethod) where T : class
    {
        var globalId = CurrentScopeHash;
        if (LastFrameDataStore.TryGetValue(globalId, out var data))
        {
            CurrentFrameDataStore.Add(globalId, data);
            return (T)data;
        }

        var value = factoryMethod(this, context);
        CurrentFrameDataStore.Add(globalId, value);
        return value;
    }

    public FlexContainer Rect()
    {
        var div = GetData(static (ui) => new FlexContainer
        {
            Id = ui.CurrentScopeHash,
            Tree = ui.Tree
        });

        OpenElement.AddChild(div);

        PushOpenElement(div);

        div.OpenElement();

        return div;
    }

    public UiText Text(ArenaString content)
    {
        var text = GetData(static ui => new UiText
        {
            Id = ui.CurrentScopeHash,
            Tree = ui.Tree,
        });

        OpenElement.AddChild(text);

        text.UiTextInfo.Content = content;
        text.UiTextInfo.Font = CascadingValues.Font;
        text.UiTextInfo.Color = CascadingValues.TextColor;
        text.UiTextInfo.Size = CascadingValues.TextSize;

        return text;
    }

    public UiSvg SvgImage(ArenaString src, ColorDefinition? colorDefinition = null)
    {
        var svg = GetData(static (ui) => new UiSvg
        {
            Id = ui.CurrentScopeHash,
            Tree = ui.Tree
        });

        OpenElement.AddChild(svg);
        svg.ColorDefinition = colorDefinition;
        svg.Src = src;

        return svg;
    }

    public UiImage Image(string src)
    {
        var image = GetData(static (ui) => new UiImage
        {
            Id = ui.CurrentScopeHash,
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
