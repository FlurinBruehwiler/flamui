using static SDL2.SDL;

namespace ImSharpUISample;

public class EventLoop
{
    public List<Window> Windows { get; set; } = new();

    public EventLoop()
    {
        if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS) < 0)
        {
            // Handle initialization error
            Console.WriteLine($"SDL_Init Error: {SDL_GetError()}");
            throw new Exception();
        }
    }

    public void RunMainThread()
    {
        var quit = false;
        while (!quit)
        {
            while (SDL_PollEvent(out var e) != 0)
            {
                if (e.type == SDL_EventType.SDL_QUIT)
                {
                    quit = true;
                }
                else if (e.type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
                {
                    GetWindow(e.motion.windowID).Events.Enqueue(e);
                }
                else if (e.type == SDL_EventType.SDL_MOUSEMOTION)
                {
                    GetWindow(e.motion.windowID).Events.Enqueue(e);
                }
            }
        }

        SDL_Quit();
    }

    private Window GetWindow(uint windowId)
    {
        foreach (var window in Windows)
        {
            if (window.Id == windowId)
                return window;
        }

        throw new Exception();
    }

    public void RunRenderThread()
    {
        while (true)
        {
            foreach (var window in Windows)
            {
                window.Update();
            }
            Thread.Sleep(16);
        }
    }
}
