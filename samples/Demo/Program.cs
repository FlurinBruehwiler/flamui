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
        IsAntialias = false
    };
    
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

        SKPaint GetRandomColor(int seed)
        {
            var rand = new Random(seed);
            return new SKPaint
            {
                Color = new SKColor((byte)rand.Next(250), (byte)rand.Next(250), (byte)rand.Next(250)),
                IsAntialias = true
            };
        }

        new FlexContainer(new Size(100, SizeKind.Percentage), new Size(100, SizeKind.Percentage), GetRandomColor(1))
        {
            ComputedWidth = ImageInfo.Width,
            ComputedHeight = ImageInfo.Height,
            ComputedX = 0,
            ComputedY = 0,
            Items = new List<FlexContainer>
            {
                new(new Size(250, SizeKind.Pixels), new Size(100, SizeKind.Percentage), GetRandomColor(2))
                {
                    Items = new List<FlexContainer>
                    {
                        new(new Size(100, SizeKind.Percentage), new Size(70, SizeKind.Pixels),
                            GetRandomColor(3)),
                        new(new Size(100, SizeKind.Percentage), new Size(100, SizeKind.Percentage),
                            GetRandomColor(4))
                        {
                            HasBorder = true
                        },
                        new(new Size(100, SizeKind.Percentage), new Size(70, SizeKind.Pixels),
                            GetRandomColor(6)),
                    },
                    JustifyContent = JustifyContent.FlexStart,
                    FlexDirection = FlexDirection.Column,
                    AlignItems = AlignItems.FlexStart
                },
                new(new Size(100, SizeKind.Percentage), new Size(100, SizeKind.Percentage), GetRandomColor(5))
                {
                    Items = new List<FlexContainer>
                    {
                        new(new Size(100, SizeKind.Percentage), new Size(150, SizeKind.Pixels), GetRandomColor(7)),
                        new(new Size(100, SizeKind.Percentage), new Size(100, SizeKind.Percentage), GetRandomColor(8))
                        {
                            Items = Enumerable.Range(0, 5).Select(x => new FlexContainer(
                                new Size(100, SizeKind.Percentage),
                                new Size(100, SizeKind.Pixels), GetRandomColor(10))
                            {
                                Radius = 100,
                                HasBorder = true
                            }).ToList(),
                            JustifyContent = JustifyContent.FlexStart,
                            FlexDirection = FlexDirection.Column,
                            AlignItems = AlignItems.FlexStart,
                            Padding = 20,
                            Gap = 10,
                            HasBorder = true
                        }
                    },
                    JustifyContent = JustifyContent.FlexStart,
                    FlexDirection = FlexDirection.Column,
                    AlignItems = AlignItems.FlexStart
                }
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
