using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Flamui;

public class WindowCreationOrder
{
    public required string Title { get; set; }
    public required FlamuiWindowOptions Options { get; set; }
    public required IServiceProvider ServiceProvider { get; set; }
    public required Type RootType { get; set; }
}

public class EventLoop
{
    public static List<UiWindow> Windows { get; set; } = new();
    public Queue<WindowCreationOrder> WindowsToCreate = new();

    public EventLoop()
    {
        FlamuiSynchronizationContext.Install();

        // Console.WriteLine(Environment.CurrentManagedThreadId);

        // if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS) < 0)
        // {
        //     // Handle initialization error
        //     throw new Exception();
        // }
    }

    public Silk.NET.Windowing.IWindow MainWindow;

    private void CreateWindow(WindowCreationOrder order)
    {
         WindowOptions options = WindowOptions.Default with
         {
             Size = new Vector2D<int>(order.Options.Width, order.Options.Height),
             Title = "Flamui next :)",
             Samples = 4,
         };

         MainWindow = Window.Create(options);

         var rootComponent = (FlamuiComponent)ActivatorUtilities.CreateInstance(order.ServiceProvider, order.RootType);

         Windows.Add(new UiWindow(MainWindow, rootComponent, order.ServiceProvider));

         MainWindow.Run();

        //todo handle min size
        // if (order.Options.MinSize is not null)
        // {
        //     SDL_SetWindowMinimumSize(windowHandle, order.Options.MinSize.Width, order.Options.MinSize.Height);
        // }
        //
        // if (order.Options.MaxSize is not null)
        // {
        //     SDL_SetWindowMaximumSize(windowHandle, order.Options.MaxSize.Width, order.Options.MaxSize.Height);
        // }
    }

    private UiWindow? GetWindow(uint windowId)
    {
        // foreach (var window in Windows)
        // {
        //     if (window.Id == windowId)
        //         return window;
        // }

        return null;
    }

    public void RunUiThread()
    {
        try
        {
            RunUiThreadInternal();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void RunUiThreadInternal()
    {
        // SDL_SetHint(SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");

        // while (true)
        // {
        //
        //     var startTime = Stopwatch.GetTimestamp();
        //
        //     foreach (var window in Windows)
        //     {
        //         window.Update();
        //     }
        //
        //     Dispatcher.UIThread.Queue.RunPendingTasks();
        //
        //     foreach (var uiWindow in Windows)
        //     {
        //         uiWindow.SwapWindow();
        //     }
        //
        //     var length = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
        //
        //     // Console.WriteLine(length);
        //
        //     if (length < 15)
        //     {
        //         var sleeplength = (int)(16.0f - length);
        //         // Console.WriteLine(sleeplength);
        //         Thread.Sleep(sleeplength);
        //     }
        // }
    }
}
