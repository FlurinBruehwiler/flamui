using Flamui.PerfTrace;
using Microsoft.Extensions.DependencyInjection;
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
    private List<UiWindow> _windows;

    internal FlamuiApp(IServiceCollection services)
    {
        services.AddSingleton(this);
        services.AddSingleton<RegistrationManager>();

        var rootProvider = services.BuildServiceProvider();
        Services = rootProvider.CreateScope().ServiceProvider;
        _windows = [];
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

    public void CreateWindow<TRootComponent>(string title, FlamuiWindowOptions? options = null) where TRootComponent : FlamuiComponent
    {
        options ??= new FlamuiWindowOptions();

        WindowOptions o = WindowOptions.Default with
        {
            Size = new Vector2D<int>(options.Width, options.Height),
            Title = title,
            Samples = 4
        };

        var window = Window.Create(o);

        var rootComponent = ActivatorUtilities.CreateInstance<TRootComponent>(Services);

        _windows.Add(new UiWindow(window, rootComponent, Services));

        window.Initialize();
    }

    public void Run()
    {
        while (true)
        {
            for (var i = 0; i < _windows.Count; i++)
            {
                var window = _windows[i].Window;

                if (window.IsClosing)
                {
                    window.DoEvents();
                    window.Reset();

                    _windows.RemoveAt(i);
                    i--;
                    continue;
                }

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

            if(_windows.Count == 0)
                break;
        }
    }

    public static FlamuiBuilder CreateBuilder()
    {
        Systrace.Initialize("trace.trace");
        return new FlamuiBuilder();
    }
}
