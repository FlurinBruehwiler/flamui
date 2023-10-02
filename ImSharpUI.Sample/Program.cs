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
Task.Run(() =>
{
    eventLoop.Windows.Add(new Window(windowHandle));
    eventLoop.RunRenderThread();
});
eventLoop.RunMainThread();

