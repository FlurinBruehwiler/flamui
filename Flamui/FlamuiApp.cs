using Microsoft.Extensions.DependencyInjection;

namespace Flamui;

public class WindowOptions
{
    public int Width { get; set; } = 900;
    public int Height { get; set; } = 500;
    public SizeConstraint? MinSize { get; set; }
    public SizeConstraint? MaxSize { get; set; }
}

public record class SizeConstraint(int Width, int Height);

public class FlamuiApp
{
    public IServiceProvider Services { get; private set; }

    private EventLoop _eventLoop = new();

    internal FlamuiApp(IServiceCollection services)
    {
        services.AddSingleton(this);
        services.AddSingleton(_eventLoop);

        var rootProvider = services.BuildServiceProvider();
        Services = rootProvider.CreateScope().ServiceProvider;

        var uiThread = new Thread(_eventLoop.RunUiThread);
        Dispatcher.UIThread = new Dispatcher(uiThread);
        uiThread.Start();
    }

    public void CreateWindow<TRootComponent>(string title, WindowOptions? options = null) where TRootComponent : FlamuiComponent
    {
        options ??= new WindowOptions();

        var windowHandle = SDL_CreateWindow(title,
            SDL_WINDOWPOS_CENTERED,
            SDL_WINDOWPOS_CENTERED,
            options.Width,
            options.Height,
            SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

        if (options.MinSize is not null)
        {
            SDL_SetWindowMinimumSize(windowHandle, options.MinSize.Width, options.MinSize.Height);
        }

        if (options.MaxSize is not null)
        {
            SDL_SetWindowMaximumSize(windowHandle, options.MaxSize.Width, options.MaxSize.Height);
        }

        if (windowHandle == IntPtr.Zero)
        {
            // Handle window creation error
            Console.WriteLine($"SDL_CreateWindow Error: {SDL_GetError()}");
            throw new Exception();
        }

        var rootComponent = ActivatorUtilities.CreateInstance<TRootComponent>(Services);

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _eventLoop.Windows.Add(new UiWindow(windowHandle, rootComponent, Services));
        });
    }

    public void Run()
    {
        _eventLoop.RunMainThread();
    }

    public static FlamuiBuilder CreateBuilder()
    {
        return new FlamuiBuilder();
    }
}
