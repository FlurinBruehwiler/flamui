using Demo;
using Demo.Test;
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
    public static string Svg;

    private static void Main()
    {
        Svg = File.ReadAllText("./battery.svg");

        s_window = AvaloniaGlobals.GetRequiredService<IWindowingPlatform>().CreateWindow();
        s_window.Resize(new Modern.WindowKit.Size(1024, 768));
        s_window.SetTitle("Modern.WindowKit Demo");
        s_window.SetIcon(SKBitmap.Decode("icon.png"));

        var mainLoopCancellationTokenSource = new CancellationTokenSource();
        s_window.Closed = () => mainLoopCancellationTokenSource.Cancel();

        s_window.Resized = (_, _) =>
        {
            s_canvas?.Dispose();
            s_canvas = null;
        };

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

    public static SKPaint Black = new()
    {
        Color = new SKColor(0, 0, 0),
        IsAntialias = true
    };

    public static SKPaint Transparent = new()
    {
        Color = new SKColor(0, 0, 0, 0),
        IsAntialias = true
    };

    public static Renderer Renderer = new Renderer();
    private static TestComponent RootComponent = new TestComponent();

    public static void DoPaint(Rect bounds)
    {
        var skiaFramebuffer = s_window.Surfaces.OfType<IFramebufferPlatformSurface>().First();

        using var framebuffer = skiaFramebuffer.Lock();

        var framebufferImageInfo = new SKImageInfo(framebuffer.Size.Width, framebuffer.Size.Height,
            framebuffer.Format.ToSkColorType(),
            framebuffer.Format == PixelFormat.Rgb565 ? SKAlphaType.Opaque : SKAlphaType.Premul);

        using var surface = SKSurface.Create(framebufferImageInfo, framebuffer.Address, framebuffer.RowBytes);

        surface.Canvas.DrawSurface(GetCanvas(), SKPoint.Empty);
        Canvas = surface.Canvas;

        Renderer.DoSomething(RootComponent.Render());

        repaint++;
        Canvas.DrawText(rerender.ToString(), new SKPoint(500, 500), Renderer.GetColor(new ColorDefinition(141, 10, 0, 255)));
        Canvas.DrawText(repaint.ToString(), new SKPoint(300, 500), Renderer.GetColor(new ColorDefinition(141, 10, 0, 255)));
    }

    public static int rerender = 0;
    public static int repaint = 0;
    
    public static SKPaint GetRandomColor(int seed)
    {
        var rand = new Random(seed);
        return new SKPaint
        {
            Color = new SKColor((byte)rand.Next(250), (byte)rand.Next(250), (byte)rand.Next(250)),
            IsAntialias = true
        };
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