using System.Reflection;
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
    private List<PhysicalWindow> _windows;

    internal FlamuiApp(IServiceCollection services)
    {
        services.AddSingleton(this);
        // services.AddSingleton<RegistrationManager>();

        var rootProvider = services.BuildServiceProvider();
        Services = rootProvider.CreateScope().ServiceProvider;
        _windows = [];
        //
        // var uiThread = new Thread(_eventLoop.RunUiThread);
        // Dispatcher.UIThread = new Dispatcher(uiThread);
        // uiThread.Start();
    }

    public void RegisterOnAfterInput(Action<UiTree> window)
    {
        //maybe not constantly resolve the service
        // Services.GetRequiredService<RegistrationManager>().OnAfterInput.Add(window);
    }

    public void CreateWindow<TRootComponent>(string title, FlamuiWindowOptions? options = null) where TRootComponent : FlamuiComponent
    {
        options ??= new FlamuiWindowOptions();

        WindowOptions o = WindowOptions.Default with
        {
            Size = new Vector2D<int>(options.Width, options.Height),
            Title = title,
            VSync = true, //for some reason this doesn't work on my laptop, so we just sleep ourselves
            ShouldSwapAutomatically = false
        };

        var window = Window.Create(o);

        var rootComponent = ActivatorUtilities.CreateInstance<TRootComponent>(Services);

        _windows.Add(PhysicalWindow.Create(window, new UiTree(rootComponent)));

        //hack to get paint during resize
        window.GetType().GetField("_onFrame", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(window, new Action(() => UpdateWindow(window)));

        window.Initialize();

        Run();
    }

    public void Run()
    {
        while (true)
        {
            for (var i = 0; i < _windows.Count; i++)
            {
                var window = _windows[i].GlfWindow;

                if (window.IsClosing)
                {
                    window.DoEvents();
                    window.Reset();

                    _windows.RemoveAt(i);
                    i--;
                    continue;
                }

                UpdateWindow(window);
            }

            if(_windows.Count == 0)
                break;
        }
    }

    private void UpdateWindow(IWindow window)
    {
        window.DoEvents();

        if (!window.IsClosing)
        {
            window.DoUpdate();
        }

        if (!window.IsClosing)
        {
            window.DoRender();
        }
    }

    public static FlamuiBuilder CreateBuilder()
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsWindows())
        {
            return new FlamuiBuilder();
        }

        throw new Exception("Flamui is currently only supported on windows and linux");
    }
}
