using Microsoft.Extensions.DependencyInjection;

namespace Flamui;

public class FlamuiApp
{
    public IServiceProvider Services { get; private set; }

    private EventLoop _eventLoop = new();

    internal FlamuiApp(IServiceCollection services)
    {
        services.AddSingleton(this);

        Services = services.BuildServiceProvider();
    }

    public void CreateWindow<TRootComponent>(string title) where TRootComponent : FlamuiComponent
    {
        var windowHandle = SDL_CreateWindow(title,
            SDL_WINDOWPOS_CENTERED,
            SDL_WINDOWPOS_CENTERED,
            1800,
            900,
            SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

        if (windowHandle == IntPtr.Zero)
        {
            // Handle window creation error
            Console.WriteLine($"SDL_CreateWindow Error: {SDL_GetError()}");
            throw new Exception();
        }

        _eventLoop.Windows.Add(new UiWindow(windowHandle));
    }

    public void Run()
    {
        _ = new Thread(_eventLoop.RunRenderThread);

        _eventLoop.RunMainThread();
    }

    public static FlamuiBuilder CreateBuilder()
    {
        return new FlamuiBuilder();
    }
}
