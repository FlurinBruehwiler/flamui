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

struct DataScope
{
    public int ScopeHash;
    public int IncrementalNumber;

    public int GetCompleteHash()
    {
        return HashCode.Combine(ScopeHash, IncrementalNumber);
    }
}

public sealed partial class Ui
{
    public Dictionary<int, object> LastFrameDataStore = [];
    public Dictionary<int, object> CurrentFrameDataStore = [];

    public Dictionary<int, IntPtr> UnmanagedLastFrameDataStore = [];
    public Dictionary<int, IntPtr> UnmanagedCurrentFrameDataStore = [];

    public ChunkedList<object> LastFrameRefObjects = new(100);
    public ChunkedList<object> CurrentFrameRefObjects = new(100);

    private FlamuiStack<DataScope> ScopeHashStack = new();
    // public int CurrentScopeScopeHash => ScopeHashStack.Peek().GetCompleteHash();

    private readonly Stack<UiElementContainer> OpenElementStack = new();
    private UiElementContainer OpenElement => OpenElementStack.Peek();

    public UiTree Tree = null!;
    public FontManager FontManager = new();

    private Stack<CascadingStuff> CascadingStack = [];
    public CascadingStuff CascadingValues;
    public UiElementContainer Root;

    [ThreadStatic]
    public static Arena Arena;

    public int GetHash()
    {
        return ScopeHashStack.Peek().GetCompleteHash();
    }

    public int IncreaseAndGetHash()
    {
        ref var scope = ref ScopeHashStack.PeekRef();
        scope.IncrementalNumber++;
        return scope.GetCompleteHash();
    }

    //used by source gen
    [NoScopeGeneration]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushScope(int hash)
    {
        if (ScopeHashStack.TryPeek(out var res))
        {
            ScopeHashStack.Push(new DataScope
            {
                ScopeHash = HashCode.Combine(hash, res.ScopeHash),
                IncrementalNumber = 0
            });
        }
        else
        {
            ScopeHashStack.Push(new DataScope
            {
                ScopeHash = hash,
                IncrementalNumber = 0
            });
        }
    }

    //used by source gen
    [NoScopeGeneration]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopScope()
    {
        ScopeHashStack.Pop();
    }

    [NoScopeGeneration]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IdScopeDisposable CreateIdScope(int hash)
    {
        PushScope(hash);
        return new IdScopeDisposable
        {
            ui = this
        };
    }

    [NoScopeGeneration]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IdScopeDisposable CreateIdScope([CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        return CreateIdScope(HashCode.Combine(file.GetHashCode(), lineNumber.GetHashCode()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushOpenElement(UiElementContainer container)
    {
        CascadingStack.Push(CascadingValues);
        OpenElementStack.Push(container);
        ScopeHashStack.Push(new DataScope
        {
            ScopeHash = container.Id.GetHashCode(),
            IncrementalNumber = 0
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UiElementContainer PopElement()
    {
        CascadingValues = CascadingStack.Pop();
        ScopeHashStack.Pop();
        PopScope();
        return OpenElementStack.Pop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ref T Get<T>(T initialValue, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0) where T : unmanaged
    {
        using var _ = CreateIdScope(file, lineNumber);
        var hash = IncreaseAndGetHash();
        if (UnmanagedLastFrameDataStore.TryGetValue(hash, out var lastPtr))
        {
            var lastValue = *(T*)lastPtr;
            var ptr = Tree.Arena.Allocate(lastValue);
            UnmanagedCurrentFrameDataStore.Add(hash, (nint)ptr);
            return ref Unsafe.AsRef<T>(ptr);
        }

        var ptr2 = Tree.Arena.Allocate(initialValue);
        if (!UnmanagedCurrentFrameDataStore.TryAdd(hash, (nint)ptr2))
        {
            throw new Exception("aahhhhh");
        }
        return ref Unsafe.AsRef<T>(ptr2);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetObj<T>() where T : class, new()
    {
        var hash = IncreaseAndGetHash();
        if (LastFrameDataStore.TryGetValue(hash, out var lastValue))
        {
            CurrentFrameDataStore.Add(hash, lastValue);
            return (T)lastValue;
        }

        var initialValue = new T();
        CurrentFrameDataStore.Add(hash, initialValue);
        return initialValue;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetObjRef<T>(T initialValue) where T : class
    {
        var idx = CurrentFrameRefObjects.Count;

        var hash = IncreaseAndGetHash();
        if (UnmanagedLastFrameDataStore.TryGetValue(hash, out var lastIdx))
        {
            var item = LastFrameRefObjects[(int)lastIdx];
            CurrentFrameRefObjects.Add(item);
        }

        CurrentFrameRefObjects.Add(initialValue);
        if (!UnmanagedCurrentFrameDataStore.TryAdd(hash, idx))
        {
            throw new Exception("aahhhhh");
        }
        ref object y = ref CurrentFrameRefObjects[idx];
        _ = (T)y; //to make sure that x has the correct type
        return ref Unsafe.As<object, T>(ref y);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref string GetString(string initialValue, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        using var _ = CreateIdScope(file, lineNumber);
        return ref GetObjRef(initialValue);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetData<T>(Func<Ui, T> factoryMethod) where T : class
    {
        return GetData(factoryMethod, static (ui, f) => f(ui));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetData<T, TContext>(TContext context, Func<Ui, TContext, T> factoryMethod) where T : class
    {
        var globalId = IncreaseAndGetHash();
        if (LastFrameDataStore.TryGetValue(globalId, out var data))
        {
            CurrentFrameDataStore.Add(globalId, data);
            return (T)data;
        }

        var value = factoryMethod(this, context);
        CurrentFrameDataStore.Add(globalId, value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FlexContainer Rect([CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        CreateIdScope(file, lineNumber);

        var div = GetData(static (ui) => new FlexContainer
        {
            Id = ui.GetHash(),
            Tree = ui.Tree
        });

        OpenElement.AddChild(div);

        PushOpenElement(div);

        div.OpenElement();

        return div;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UiText Text(ArenaString content, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        using var _ = CreateIdScope(file, lineNumber);
        var text = GetData(static ui => new UiText
        {
            Id = ui.GetHash(),
            Tree = ui.Tree,
        });

        OpenElement.AddChild(text);

        text.UiTextInfo.Content = content;
        text.UiTextInfo.Font = CascadingValues.Font;
        text.UiTextInfo.Color = CascadingValues.TextColor;
        text.UiTextInfo.Size = CascadingValues.TextSize;

        return text;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UiSvg SvgImage(ArenaString src, ColorDefinition? colorDefinition = null, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        using var _ = CreateIdScope(file, lineNumber);
        var svg = GetData(static (ui) => new UiSvg
        {
            Id = ui.GetHash(),
            Tree = ui.Tree
        });

        OpenElement.AddChild(svg);
        svg.ColorDefinition = colorDefinition;
        svg.Src = src;

        return svg;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UiImage Image(string src)
    {
        var image = GetData(static (ui) => new UiImage
        {
            Id = ui.GetHash(),
            Tree = ui.Tree,
        });

        image.Src = src;
        OpenElement.AddChild(image);

        return image;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetStuff()
    {
        CascadingStack.Clear();
        CascadingValues = new CascadingStuff();
        CascadingValues.Font = FontManager.GetFont("segoeui.ttf");
        CascadingValues.TextColor = C.Black;
        CascadingValues.TextSize = 15;

        OpenElementStack.Clear();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnterLayoutScope(LayoutScope layoutScope)
    {
        CascadingStack.Push(layoutScope.CascadingStuff);
        OpenElementStack.Push(layoutScope.OpenElement);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExitLayoutScope()
    {
        CascadingStack.Pop();
        OpenElementStack.Pop();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
