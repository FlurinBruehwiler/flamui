using Microsoft.Extensions.DependencyInjection;

namespace Flamui;

public class WindowOptions
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

    private EventLoop _eventLoop = new();

    internal FlamuiApp(IServiceCollection services)
    {
        services.AddSingleton(this);
        services.AddSingleton(_eventLoop);
        services.AddSingleton<RegistrationManager>();

        var rootProvider = services.BuildServiceProvider();
        Services = rootProvider.CreateScope().ServiceProvider;

        var uiThread = new Thread(_eventLoop.RunUiThread);
        Dispatcher.UIThread = new Dispatcher(uiThread);
        uiThread.Start();
    }

    public void RegisterOnAfterInput(Action<UiWindow> window)
    {
        //maybe not constantly resolve the service
        Services.GetRequiredService<RegistrationManager>().OnAfterInput.Add(window);
    }

    public void CreateWindow<TRootComponent>(string title, WindowOptions? options = null) where TRootComponent : FlamuiComponent
    {
        options ??= new WindowOptions();

        _eventLoop.WindowsToCreate.Enqueue(new WindowCreationOrder
        {
            Title = title,
            Options = options,
            RootType = typeof(TRootComponent),
            ServiceProvider = Services
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
