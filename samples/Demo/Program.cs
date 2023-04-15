using System.Diagnostics;
using Demo;
using Demo.Test;
using Modern.WindowKit;
using Modern.WindowKit.Controls.Platform.Surfaces;
using Modern.WindowKit.Input.Raw;
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

        s_window.Input = Input;
        
        // s_window.PositionChanged = _ => Invalidate();
        
        s_window.Paint = DoPaint;

        s_window.Show(true, false);
        
        Renderer.Build(RootComponent);

        Dispatcher.UIThread.MainLoop(mainLoopCancellationTokenSource.Token);
    }

    public static Div? ActiveDiv;

    private static void Input(RawInputEventArgs args)
    {
        var callbackWasCalled = false;
        
        if (args is RawKeyEventArgs { Type: RawKeyEventType.KeyDown } keyEventArgs)
        {
            if (ActiveDiv?.POnKeyDown is not null)
            {
                ActiveDiv.POnKeyDown(keyEventArgs.Key);
                callbackWasCalled = true;
            }       
        }
        if (args is RawTextInputEventArgs rawInputEventArgs)
        {
            if (ActiveDiv?.POnTextInput is not null)
            {
                ActiveDiv.POnTextInput(rawInputEventArgs.Text);
                callbackWasCalled = true;
            }    
        }
        if (args is RawPointerEventArgs pointer)
        {
            var x = Scale((int)pointer.Position.X);
            var y = Scale((int)pointer.Position.Y);

            if (pointer.Type == RawPointerEventType.LeftButtonDown)
            {
                var div = HitTest(Renderer._newRoot, x, y);

                if (div is null)
                    return;
            
                if (ActiveDiv?.POnInactive is not null)
                {
                    ActiveDiv.POnInactive();
                    callbackWasCalled = true;
                }

                ActiveDiv = div;

                if (div.POnActive is not null)
                {
                    div.POnActive();
                    callbackWasCalled = true;
                }
            
                if (div.POnClick is not null)
                {
                    div.POnClick();
                    callbackWasCalled = true;
                }

                if (div.POnClickAsync is not null)
                {
                    div.POnClickAsync();
                    callbackWasCalled = true;
                }
            }
        }
        
        if (callbackWasCalled)
        {
            Renderer.Build(RootComponent);
            DoPaint(new Rect());
        }
    }

    private static Div? HitTest(Div div, int x, int y)
    {
        if (div.PComputedX <= x && div.PComputedX + div.PComputedWidth >= x && div.PComputedY <= y && div.PComputedY + div.PComputedHeight >= y)
        {
            if (div.Children is null)
            {
                return div;
            }
            
            foreach (var child in div.Children)
            {
                if(child is not Div divChild)
                    continue;
                
                var childHit = HitTest(divChild, x, y);
                if (childHit is not null)
                    return childHit;
            }

            return div;
        }

        return null;
    }

    public static Point ClickPos = new(-1, -1); 
    
    private static int Scale(int value) => (int)(value * s_window.RenderScaling);

    private static long s_lastRender = Stopwatch.GetTimestamp();

    
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
    public static TestComponent RootComponent = new TestComponent();

    
    public static void DoPaint(Rect bounds)
    {
        // if (Stopwatch.GetElapsedTime(s_lastRender).TotalMilliseconds < 16.666)
        // {
        //     skipedRenders++;
        //     return;
        // }
        
        s_lastRender = Stopwatch.GetTimestamp();
        
        var skiaFramebuffer = s_window.Surfaces.OfType<IFramebufferPlatformSurface>().First();

        using var framebuffer = skiaFramebuffer.Lock();

        var framebufferImageInfo = new SKImageInfo(framebuffer.Size.Width, framebuffer.Size.Height,
            framebuffer.Format.ToSkColorType(),
            framebuffer.Format == PixelFormat.Rgb565 ? SKAlphaType.Opaque : SKAlphaType.Premul);

        using var surface = SKSurface.Create(framebufferImageInfo, framebuffer.Address, framebuffer.RowBytes);

        surface.Canvas.DrawSurface(GetCanvas(), SKPoint.Empty);
        Canvas = surface.Canvas;

        Renderer.LayoutPaintComposite();

        Canvas.DrawRect(0,0, 50, 110, Renderer.GetColor(new ColorDefinition(0,0,0,255)));
        Canvas.DrawText(compute.ToString(), new SKPoint(10, 20), Renderer.GetColor(new ColorDefinition(141, 10, 0, 255)));
        Canvas.DrawText(draw.ToString(), new SKPoint(10, 40), Renderer.GetColor(new ColorDefinition(141, 10, 0, 255)));
        Canvas.DrawText(counter.ToString(), new SKPoint(10, 60), Renderer.GetColor(new ColorDefinition(141, 10, 0, 255)));
        Canvas.DrawText(skipedRenders.ToString(), new SKPoint(10, 80), Renderer.GetColor(new ColorDefinition(141, 10, 0, 255)));
        Canvas.DrawText(textInput, new SKPoint(10, 100), Renderer.GetColor(new ColorDefinition(141, 10, 0, 255)));
        counter++;
    }

    public static long compute = 0;
    public static long draw = 0;
    public static long counter = 0;
    public static long skipedRenders;
    public static string textInput = "";

    private static void Invalidate() => s_window.Invalidate(new Rect(Modern.WindowKit.Point.Empty, s_window.ClientSize));
}

public static class HotReloadManager
{
    public static void ClearCache(Type[]? updatedTypes)
    {
        Console.WriteLine("HotReloadManager.ClearCache");
    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        Program.Renderer.Build(Program.RootComponent);
        Program.DoPaint(new Rect());
        Console.WriteLine("HotReloadManager.UpdateApplication");
    }
}

public record struct Point(int X, int Y);