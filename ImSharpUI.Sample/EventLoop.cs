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
                Console.WriteLine(e);
                if (e.type == SDL_EventType.SDL_QUIT)
                {
                    quit = true;
                }
            }
        }

        SDL_Quit();
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
