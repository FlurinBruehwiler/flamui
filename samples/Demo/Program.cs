using Demo;
using Modern.WindowKit;
using Modern.WindowKit.Controls.Platform.Surfaces;
using Modern.WindowKit.Platform;
using Modern.WindowKit.Skia;
using Modern.WindowKit.Threading;
using SkiaSharp;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(HotReloadManager))]

namespace Demo;

public class Program
{
    private static IWindowImpl s_window = null!;
    public static SKSurface? s_canvas;
    public static SKImageInfo ImageInfo;
    public static SKCanvas Canvas = null;
    
    private static void Main()
    {
        s_window = AvaloniaGlobals.GetRequiredService<IWindowingPlatform>().CreateWindow();
        s_window.Resize(new Modern.WindowKit.Size(1024, 768));
        s_window.SetTitle("Modern.WindowKit Demo");
        s_window.SetIcon(SKBitmap.Decode("icon.png"));

        var mainLoopCancellationTokenSource = new CancellationTokenSource();
        s_window.Closed = () => mainLoopCancellationTokenSource.Cancel();

        s_window.Resized = (_, _) => { s_canvas?.Dispose(); s_canvas = null; };

        s_window.PositionChanged = _ => Invalidate();

        s_window.Paint = DoPaint;

        s_window.Show(true, false);

        Dispatcher.UIThread.MainLoop(mainLoopCancellationTokenSource.Token);
    }

    private static SKSurface GetCanvas()
    {
        if (s_canvas is not null)
            return s_canvas;

        var screen = s_window.ClientSize * s_window.RenderScaling;
        var info = new SKImageInfo((int)screen.Width, (int)screen.Height);

        ImageInfo = info;
        
        s_canvas = SKSurface.Create(info);
        s_canvas.Canvas.Clear(SKColors.CornflowerBlue);
        
        return s_canvas;
    }

    public static void DoPaint(Rect bounds)
    {
        var skiaFramebuffer = s_window.Surfaces.OfType<IFramebufferPlatformSurface>().First();

        using var framebuffer = skiaFramebuffer.Lock();

        var framebufferImageInfo = new SKImageInfo(framebuffer.Size.Width, framebuffer.Size.Height,
            framebuffer.Format.ToSkColorType(), framebuffer.Format == PixelFormat.Rgb565 ? SKAlphaType.Opaque : SKAlphaType.Premul);

        using var surface = SKSurface.Create(framebufferImageInfo, framebuffer.Address, framebuffer.RowBytes);

        surface.Canvas.DrawSurface(GetCanvas(), SKPoint.Empty);

        Canvas = surface.Canvas;
        
        var Red = new SKPaint
        {
            Color = new SKColor(255, 0, 0, 255)
        };

        var Blue = new SKPaint
        {
            Color = new SKColor(0, 204, 255, 255)
        };
        
        var Green = new SKPaint
        {
            Color = new SKColor(0, 204, 0, 255)
        };
        
        new FlexContainer(new Size(100, SizeKind.Percentage), new Size(100, SizeKind.Percentage), Green)
        {
            ComputedWidth = ImageInfo.Width,
            ComputedHeight = ImageInfo.Height,
            Items = new List<FlexContainer>
            {
                new(new Size(50, SizeKind.Percentage), new Size(50, SizeKind.Percentage), Blue)
                {
                    Items = new List<FlexContainer>
                    {
                        new(new Size(40, SizeKind.Percentage), new Size(50, SizeKind.Percentage), Green),
                        new(new Size(40, SizeKind.Percentage), new Size(50, SizeKind.Percentage), Green)
                    },
                    JustifyContent = JustifyContent.SpaceBetween,
                    FlexDirection = FlexDirection.Row,
                    AlignItems = AlignItems.FlexStart
                },
            },
            JustifyContent = JustifyContent.FlexStart,
            FlexDirection = FlexDirection.Row,
            AlignItems = AlignItems.FlexStart
        }.Render();
    }
    
    private static void Invalidate() => s_window.Invalidate(new Rect(Point.Empty, s_window.ClientSize));
}

public static class HotReloadManager
{
    public static void ClearCache(Type[]? updatedTypes)
    {
        Console.WriteLine("HotReloadManager.ClearCache");
    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        Program.DoPaint(new Rect());
        Console.WriteLine("HotReloadManager.UpdateApplication");
    }
}
