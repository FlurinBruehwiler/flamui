using static SDL2.SDL;
using ImSharpUISample;

var eventLoop = new EventLoop();
var windowHandle = SDL_CreateWindow("SDL2 C# OpenGL",
    SDL_WINDOWPOS_CENTERED,
    SDL_WINDOWPOS_CENTERED,
    800,
    600,
    SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

if (windowHandle == IntPtr.Zero)
{
    // Handle window creation error
    Console.WriteLine($"SDL_CreateWindow Error: {SDL_GetError()}");
    throw new Exception();
}

var added = false;
Task.Run(() =>
{
    try
    {
        eventLoop.Windows.Add(new Window(windowHandle));
        added = true;
        eventLoop.RunRenderThread();

    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
});
while (!added)
{

}
eventLoop.RunMainThread();

