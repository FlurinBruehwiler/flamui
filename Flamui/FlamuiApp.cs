using System.Runtime.InteropServices;
using Flamui.PerfTrace;
using Microsoft.Extensions.DependencyInjection;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using  Silk.NET.Windowing;

namespace Flamui;

public class FlamuiWindowOptions
{
    public int Width { get; set; } = 900;
    public int Height { get; set; } = 500;
    public SizeConstraint? MinSize { get; set; }
    public SizeConstraint? MaxSize { get; set; }
}

public record SizeConstraint(int Width, int Height);

public class FlamuiApp
{
    public IServiceProvider Services { get; private set; }
    private UiWindow _window;

    internal FlamuiApp(IServiceCollection services)
    {
        services.AddSingleton(this);
        services.AddSingleton<RegistrationManager>();

        var rootProvider = services.BuildServiceProvider();
        Services = rootProvider.CreateScope().ServiceProvider;
        //
        // var uiThread = new Thread(_eventLoop.RunUiThread);
        // Dispatcher.UIThread = new Dispatcher(uiThread);
        // uiThread.Start();
    }

    public void RegisterOnAfterInput(Action<UiWindow> window)
    {
        //maybe not constantly resolve the service
        Services.GetRequiredService<RegistrationManager>().OnAfterInput.Add(window);
    }

    public unsafe void CreateWindow<TRootComponent>(string title, FlamuiWindowOptions? options = null) where TRootComponent : FlamuiComponent
    {
        options ??= new FlamuiWindowOptions();

        WindowOptions o = WindowOptions.Default with
        {
            Size = new Vector2D<int>(options.Width, options.Height),
            Title = title,
            Samples = 4
        };

        var window = Window.Create(o);

        // GlfwProvider.GLFW.Value.GetFramebufferSize((WindowHandle*)window.Handle, out var frameBufferWidth, out var frameBufferHeight);
        // Console.WriteLine($"FrameBufferSize: W:{frameBufferWidth} H:{frameBufferHeight}");
        //
        // GlfwProvider.GLFW.Value.GetWindowSize((WindowHandle*)window.Handle, out var windowWidth, out var windowHeight);
        // Console.WriteLine($"FrameBufferSize: W:{windowWidth} H:{windowHeight}");

        var rootComponent = ActivatorUtilities.CreateInstance<TRootComponent>(Services);

        _window = new UiWindow(window, rootComponent, Services);
    }

    public void Run()
    {
        _window.Window.Run();
    }

    public static FlamuiBuilder CreateBuilder()
    {
        Systrace.Initialize("trace.trace");
        return new FlamuiBuilder();
    }
}
