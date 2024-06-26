using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Flamui;

public class WindowCreationOrder
{
    public required string Title { get; set; }
    public required WindowOptions Options { get; set; }
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

        if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS) < 0)
        {
            // Handle initialization error
            throw new Exception();
        }
    }


    public void RunMainThread()
    {
        // Console.WriteLine(Environment.CurrentManagedThreadId);

        var quit = false;
        while (!quit)
        {
            while (SDL_PollEvent(out var e) == 1)
            {
                switch (e.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        Console.WriteLine("quit");
                        quit = true;
                        break;
                    case SDL_EventType.SDL_WINDOWEVENT:
                        GetWindow(e.window.windowID)?.Events.Enqueue(e);
                        break;
                    case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    case SDL_EventType.SDL_MOUSEBUTTONUP:
                    case SDL_EventType.SDL_MOUSEMOTION:
                        GetWindow(e.motion.windowID)?.Events.Enqueue(e);
                        break;
                    case SDL_EventType.SDL_TEXTINPUT:
                        GetWindow(e.text.windowID)?.Events.Enqueue(e);
                        break;
                    case SDL_EventType.SDL_KEYDOWN or SDL_EventType.SDL_KEYUP:
                        GetWindow(e.key.windowID)?.Events.Enqueue(e);
                        break;
                    case SDL_EventType.SDL_MOUSEWHEEL:
                        GetWindow(e.wheel.windowID)?.Events.Enqueue(e);
                        break;
                }
            }

            while (WindowsToCreate.TryDequeue(out var order))
            {
                CreateWindow(order);
            }

            Thread.Sleep(1000/60);
        }

        SDL_Quit();

        Environment.Exit(0);
    }

    private void CreateWindow(WindowCreationOrder order)
    {
        var windowHandle = SDL_CreateWindow(order.Title,
            SDL_WINDOWPOS_CENTERED,
            SDL_WINDOWPOS_CENTERED,
            order.Options.Width,
            order.Options.Height,
            SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL_WindowFlags.SDL_WINDOW_POPUP_MENU);

        if (order.Options.MinSize is not null)
        {
            SDL_SetWindowMinimumSize(windowHandle, order.Options.MinSize.Width, order.Options.MinSize.Height);
        }

        if (order.Options.MaxSize is not null)
        {
            SDL_SetWindowMaximumSize(windowHandle, order.Options.MaxSize.Width, order.Options.MaxSize.Height);
        }

        if (windowHandle == IntPtr.Zero)
        {
            // Handle window creation error
            Console.WriteLine($"SDL_CreateWindow Error: {SDL_GetError()}");
            throw new Exception();
        }

        var rootComponent = (FlamuiComponent)ActivatorUtilities.CreateInstance(order.ServiceProvider, order.RootType);

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Windows.Add(new UiWindow(windowHandle, rootComponent, order.ServiceProvider));
        });
    }

    private UiWindow? GetWindow(uint windowId)
    {
        foreach (var window in Windows)
        {
            if (window.Id == windowId)
                return window;
        }

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
        SDL_SetHint(SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");

        while (true)
        {

            var startTime = Stopwatch.GetTimestamp();

            foreach (var window in Windows)
            {
                window.Update();
            }

            Dispatcher.UIThread.Queue.RunPendingTasks();

            foreach (var uiWindow in Windows)
            {
                uiWindow.SwapWindow();
            }

            var length = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;

            // Console.WriteLine(length);

            if (length < 15)
            {
                var sleeplength = (int)(16.0f - length);
                // Console.WriteLine(sleeplength);
                Thread.Sleep(sleeplength);
            }
        }
    }
}
