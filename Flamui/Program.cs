using Flamui;

var eventLoop = new EventLoop();
var windowHandle = SDL_CreateWindow("SDL2 C# OpenGL",
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
// SDL_SetWindowHitTest(windowHandle, (win, a, data) =>
// {
//     unsafe
//     {
//         var area = (SDL_Point*)a;
//         if (area->y < 60)
//         {
//             return SDL_HitTestResult.SDL_HITTEST_DRAGGABLE;
//         }
//
//         return SDL_HitTestResult.SDL_HITTEST_NORMAL;
//     }
// }, 0);



var added = false;
Task.Run(() =>
{

    SDL_SetHint(SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");

    try
    {
        eventLoop.Windows.Add(new UiWindow(windowHandle));
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

