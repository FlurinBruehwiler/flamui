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
        Canvas.Clear(SKColors.White);
        
        new FlexContainer
        {
            Items = new List<Item>
            {
                new(new Size(100, SizeKind.Pixels), new Size(200, SizeKind.Pixels), Blue),
                new(new Size(100, SizeKind.Percentage), new Size(200, SizeKind.Pixels), Red),
                new(new Size(100, SizeKind.Pixels), new Size(600, SizeKind.Pixels), Blue),
            },
            JustifyContent = JustifyContent.SpaceBetween,
            FlexDirection = FlexDirection.RowReverse,
            AlignItems = AlignItems.FlexEnd
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
        ComputeSize();
        
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
                Program.Canvas.DrawRect(mainOffset, GetCrossAxisOffset(item), item.ComputedWidth, item.ComputedHeight, item.Color);
                break;
            case FlexDirection.RowReverse:
                Program.Canvas.DrawRect(Program.RenderTarget.Width - mainOffset - item.ComputedWidth, GetCrossAxisOffset(item), item.ComputedWidth, item.ComputedHeight, item.Color);
                break;
            case FlexDirection.Column:
                Program.Canvas.DrawRect(GetCrossAxisOffset(item), mainOffset, item.ComputedWidth, item.ComputedHeight, item.Color);
                break;
            case FlexDirection.ColumnReverse:
                Program.Canvas.DrawRect(GetCrossAxisOffset(item), Program.RenderTarget.Height - mainOffset - item.ComputedHeight, item.ComputedWidth, item.ComputedHeight, item.Color);
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
            FlexDirection.Row or FlexDirection.RowReverse => item.ComputedWidth,
            FlexDirection.Column or FlexDirection.ColumnReverse => item.ComputedHeight,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private int GetItemMainAxisFixedLength(Item item)
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => item.Width.SizeKind == SizeKind.Percentage ? 0 : item.Width.Value,
            FlexDirection.Column or FlexDirection.ColumnReverse => item.Height.SizeKind == SizeKind.Percentage ? 0 : item.Height.Value,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private int GetItemCrossAxisLength(Item item)
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => item.ComputedHeight,
            FlexDirection.Column or FlexDirection.ColumnReverse => item.ComputedWidth,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void ComputeSize()
    {
        switch(FlexDirection)
        {
            case FlexDirection.Row or FlexDirection.RowReverse:
                ComputeRowSize();
                break;
            case FlexDirection.Column or FlexDirection.ColumnReverse:
                ComputeColumnSize();
                break;
        }
    }

    private void ComputeColumnSize()
    {
        var remainingSize = RemainingMainAxisFixedSize();
        var itemsHeightPercentage = Items.Where(x => x.Height.SizeKind == SizeKind.Percentage).ToList();
        var totalPercentage = itemsHeightPercentage.Sum(x => x.Height.Value);
        var sizePerPercent = (float)remainingSize / totalPercentage;
        foreach (var item in Items)
        {
            item.ComputedHeight = item.Height.SizeKind switch
            {
                SizeKind.Percentage => (int)(item.Height.Value * sizePerPercent),
                SizeKind.Pixels => item.Height.Value,
                _ => item.ComputedHeight
            };
            if (item.Width.SizeKind == SizeKind.Percentage)
                throw new NotImplementedException();
            item.ComputedWidth = item.Width.Value;
        }
    }
    
    private void ComputeRowSize()
    {
        var remainingSize = RemainingMainAxisFixedSize();
        var itemsWithPercentage = Items.Where(x => x.Width.SizeKind == SizeKind.Percentage).ToList();
        var totalPercentage = itemsWithPercentage.Sum(x => x.Width.Value);
        var sizePerPercent = (float)remainingSize / totalPercentage;
        foreach (var item in Items)
        {
            item.ComputedWidth = item.Width.SizeKind switch
            {
                SizeKind.Percentage => (int)(item.Width.Value * sizePerPercent),
                SizeKind.Pixels => item.Width.Value,
                _ => item.ComputedWidth
            };
            if (item.Height.SizeKind == SizeKind.Percentage)
                throw new NotImplementedException();
            item.ComputedHeight = item.Height.Value;
        }
    }

    private int RemainingMainAxisFixedSize()
    {        
        return GetMainAxisLength() - Items.Sum(GetItemMainAxisFixedLength);
    }

    private int RemainingMainAxisSize()
    {
        return GetMainAxisLength() - Items.Sum(GetItemMainAxisLength);
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
        var mainOffset = RemainingMainAxisSize();

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item);
        }
    }

    private void RenderCenter()
    {
        var mainOffset = RemainingMainAxisSize() / 2;

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item);
        }
    }

    private void RenderSpaceBetween()
    {
        var totalRemaining = RemainingMainAxisSize();
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
        var totalRemaining = RemainingMainAxisSize();
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
        var totalRemaining = RemainingMainAxisSize();
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

enum SizeKind
{
    Pixels,
    Percentage
}

record Item(Size Width, Size Height, SKPaint Color)
{
    public int ComputedWidth { get; set; }   
    public int ComputedHeight { get; set; }   
}
    
record Size(int Value, SizeKind SizeKind);
