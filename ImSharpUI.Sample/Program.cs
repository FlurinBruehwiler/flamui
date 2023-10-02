using Silk.NET.OpenGL;
using SkiaSharp;
using static SDL2.SDL;

SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 3);
SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 0);

SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE);

var windowFlags = SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

const int kStencilBits = 8;  // Skia needs 8 stencil bits
SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_RED_SIZE, 8);
SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_GREEN_SIZE, 8);
SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_BLUE_SIZE, 8);
SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);
SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_DEPTH_SIZE, 0);
SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_STENCIL_SIZE, kStencilBits);
SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_ACCELERATED_VISUAL, 1);

const int kMsaaSampleCount = 0;

if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS) < 0)
{
    // Handle initialization error
    Console.WriteLine($"SDL_Init Error: {SDL_GetError()}");
    return;
}

// Create an SDL window
var window = SDL_CreateWindow("SDL2 C# OpenGL", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 800, 600, windowFlags);
if (window == IntPtr.Zero)
{
    // Handle window creation error
    Console.WriteLine($"SDL_CreateWindow Error: {SDL_GetError()}");
    SDL_Quit();
    return;
}

// var screenSurface = SDL_GetWindowSurface(window);


// Create an OpenGL context
var glContext = SDL_GL_CreateContext(window);
if (glContext == IntPtr.Zero)
{
    // Handle context creation error
    Console.WriteLine($"SDL_GL_CreateContext Error: {SDL_GetError()}");
    SDL_DestroyWindow(window);
    SDL_Quit();
    return;
}

// Make the OpenGL context current
var success = SDL_GL_MakeCurrent(window, glContext);
if (success != 0)
{
    throw new Exception();
}

GL.GetApi(SDL_GL_GetProcAddress).Viewport(0, 0, 800, 600);
GL.GetApi(SDL_GL_GetProcAddress).ClearColor(1, 1, 1, 1);
GL.GetApi(SDL_GL_GetProcAddress).ClearStencil(0);
GL.GetApi(SDL_GL_GetProcAddress).Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);

var glInterface = GRGlInterface.CreateOpenGl(SDL_GL_GetProcAddress);
Console.WriteLine(glInterface.Validate());

var context = GRContext.CreateGl(glInterface, new GRContextOptions
{
    AvoidStencilBuffers = true
});

GL.GetApi(SDL_GL_GetProcAddress).GetInteger(GLEnum.FramebufferBinding, out var buffer);

var target = new GRBackendRenderTarget(800, 600,0, 8, new GRGlFramebufferInfo(0, 0x8058)
{
    FramebufferObjectId = (uint)buffer
});

var surface = SKSurface.Create(context, target, GRSurfaceOrigin.TopLeft, SKColorType.Rgba8888);

var canvas = surface.Canvas;


// Main loop
var quit = false;
while (!quit)
{
    SDL_Event e;
    while (SDL_PollEvent(out e) != 0)
    {
        if (e.type == SDL_EventType.SDL_QUIT)
        {
            quit = true;
        }
    }
    canvas.Clear();
    canvas.DrawRect(100, 100, 200, 200, new SKPaint
    {
        Color = SKColors.Red
    });
    canvas.Flush();

    // Swap the front and back buffers
    // SDL_FillRect(screenSurface, IntPtr.Zero, 300);
    SDL_GL_SwapWindow(window);
    // SDL_UpdateWindowSurface(window);
}


surface.Dispose();
// Clean up
SDL_GL_DeleteContext(glContext);
SDL_DestroyWindow(window);
SDL_Quit();

//https://chromium.googlesource.com/skia/+/chrome/m53/example/SkiaSDLExample.cpp
