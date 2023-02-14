using System.Drawing;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using SkiaSharp;

public class Program
{
    public static SKCanvas Canvas;
    public static GRContext Context;
    public static SKPaint Blue;
    public static SKPaint Red;
    public static GRBackendRenderTarget RenderTarget;

    public static void Main()
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "Joa",
            PreferredStencilBufferBits = 8,
            PreferredBitDepth = new Vector4D<int>(8, 8, 8, 8)
        };
        GlfwWindowing.Use();
        var window = Window.Create(options);
        window.Initialize();

        using var grGlInterface =
            GRGlInterface.Create(name => window.GLContext!.TryGetProcAddress(name, out var addr) ? addr : 0);
        grGlInterface.Validate();
        Context = GRContext.CreateGl(grGlInterface);
        RenderTarget =
            new GRBackendRenderTarget(800, 600, 0, 8, new GRGlFramebufferInfo(0, 0x8058)); // 0x8058 = GL_RGBA8`
        using var surface = SKSurface.Create(Context, RenderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
        Canvas = surface.Canvas;

        Red = new SKPaint
        {
            Color = new SKColor(255, 0, 0, 255)
        };

        Blue = new SKPaint
        {
            Color = new SKColor(0, 204, 255, 255)
        };

        window.Render += OnWindowOnRender;

        window.Run();
    }

    static void OnWindowOnRender(double _)
    {
        Context.ResetContext();
        Canvas.Clear(SKColors.Green);

        new FlexContainer
        {
            Items = new List<Item>
            {
                new(50, 100, Red),
                new(150, 200, Blue),
                new(100, 10, Red),
                new(100, 50, Blue)
            },
            JustifyContent = JustifyContent.SpaceEvenly,
            FlexDirection = FlexDirection.ColumnReverse,
            AlignItems = AlignItems.FlexStart
        }.Render();

        Canvas.Flush();
    }
}

class FlexContainer
{
    public List<Item> Items { get; set; } = new();
    public JustifyContent JustifyContent { get; set; } = JustifyContent.FlexStart;
    public FlexDirection FlexDirection { get; set; } = FlexDirection.Row;
    public AlignItems AlignItems { get; set; } = AlignItems.FlexStart;

    public void Render()
    {
        switch (JustifyContent)
        {
            case JustifyContent.FlexStart:
                RenderFlexStart();
                break;
            case JustifyContent.FlexEnd:
                RenderFlexEnd();
                break;
            case JustifyContent.Center:
                RenderCenter();
                break;
            case JustifyContent.SpaceBetween:
                RenderSpaceBetween();
                break;
            case JustifyContent.SpaceAround:
                RenderSpaceAround();
                break;
            case JustifyContent.SpaceEvenly:
                RenderSpaceEvenly();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DrawWithMainOffset(int mainOffset, Item item)
    {
        switch (FlexDirection)
        {
            case FlexDirection.Row:
                Program.Canvas.DrawRect(mainOffset, GetCrossAxisOffset(item), item.Width, item.Height, item.Color);
                break;
            case FlexDirection.RowReverse:
                Program.Canvas.DrawRect(Program.RenderTarget.Width - mainOffset - item.Width, GetCrossAxisOffset(item), item.Width, item.Height, item.Color);
                break;
            case FlexDirection.Column:
                Program.Canvas.DrawRect(GetCrossAxisOffset(item), mainOffset, item.Width, item.Height, item.Color);
                break;
            case FlexDirection.ColumnReverse:
                Program.Canvas.DrawRect(GetCrossAxisOffset(item), Program.RenderTarget.Height - mainOffset - item.Height, item.Width, item.Height, item.Color);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private int GetCrossAxisOffset(Item item)
    {
        return AlignItems switch
        {
            AlignItems.FlexStart => 0,
            AlignItems.FlexEnd => GetCrossAxisLength() - GetItemCrossAxisLength(item),
            AlignItems.Center => GetCrossAxisLength() / 2 - GetItemCrossAxisLength(item) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetMainAxisLength()
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => Program.RenderTarget.Width,
            FlexDirection.Column or FlexDirection.ColumnReverse => Program.RenderTarget.Height,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetCrossAxisLength()
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => Program.RenderTarget.Height,
            FlexDirection.Column or FlexDirection.ColumnReverse => Program.RenderTarget.Width,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetItemMainAxisLength(Item item)
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => item.Width,
            FlexDirection.Column or FlexDirection.ColumnReverse => item.Height,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private int GetItemCrossAxisLength(Item item)
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => item.Height,
            FlexDirection.Column or FlexDirection.ColumnReverse => item.Width,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void RenderFlexStart()
    {
        var mainOffset = 0;

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item);
        }
    }

    private void RenderFlexEnd()
    {
        var mainOffset = GetMainAxisLength() - Items.Sum(GetItemMainAxisLength);

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item);
        }
    }

    private void RenderCenter()
    {
        var totalWidth = Items.Sum(GetItemMainAxisLength);
        var mainOffset = (GetMainAxisLength() - totalWidth) / 2;

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item);
        }
    }

    private void RenderSpaceBetween()
    {
        var totalWidth = Items.Sum(GetItemMainAxisLength);
        var totalRemaining = GetMainAxisLength() - totalWidth;
        var space = totalRemaining / (Items.Count - 1);

        var mainOffset = 0;

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item) + space;
        }
    }

    private void RenderSpaceAround()
    {
        var totalWidth = Items.Sum(GetItemMainAxisLength);
        var totalRemaining = GetMainAxisLength() - totalWidth;
        var space = totalRemaining / Items.Count / 2;

        var mainOffset = 0;

        foreach (var item in Items)
        {
            mainOffset += space;
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item) + space;
        }
    }

    private void RenderSpaceEvenly()
    {
        var totalWidth = Items.Sum(GetItemMainAxisLength);
        var totalRemaining = GetMainAxisLength() - totalWidth;
        var space = totalRemaining / (Items.Count + 1);

        var mainOffset = space;

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item) + space;
        }
    }
}

enum JustifyContent
{
    FlexStart,
    FlexEnd,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

enum FlexDirection
{
    Row,
    RowReverse,
    Column,
    ColumnReverse
}

enum AlignItems
{
    FlexStart,
    FlexEnd,
    Center
}

class Item
{
    public int Width { get; set; }
    public int Height { get; set; }
    public SKPaint Color { get; set; }

    public Item(int width, int height, SKPaint color)
    {
        Color = color;
        Width = width;
        Height = height;
    }
}