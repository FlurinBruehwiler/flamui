﻿using ImSharpUISample.UiElements;
using SkiaSharp;
using static SDL2.SDL;

namespace ImSharpUISample;

public class Window : IDisposable
{
    private readonly IntPtr _windowHandle;
    private readonly SKCanvas _canvas;
    private readonly IntPtr _openGlContextHandle;

    public Window(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;

        _openGlContextHandle = SDL_GL_CreateContext(_windowHandle);
        if (_openGlContextHandle == IntPtr.Zero)
        {
            // Handle context creation error
            Console.WriteLine($"SDL_GL_CreateContext Error: {SDL_GetError()}");
            SDL_DestroyWindow(_windowHandle);
            throw new Exception();
        }

        var success = SDL_GL_MakeCurrent(_windowHandle, _openGlContextHandle);
        if (success != 0)
        {
            throw new Exception();
        }

        var glInterface = GRGlInterface.CreateOpenGl(SDL_GL_GetProcAddress);

        var context = GRContext.CreateGl(glInterface, new GRContextOptions
        {
            AvoidStencilBuffers = true
        });

        var target = new GRBackendRenderTarget(800, 600,0, 8, new GRGlFramebufferInfo(0, 0x8058));

        var surface = SKSurface.Create(context, target, GRSurfaceOrigin.TopLeft, SKColorType.Rgba8888);

        _canvas = surface.Canvas;
    }

    private readonly Sample _sample = new();

    public void Update()
    {
        _canvas.Clear();

        SDL_GetWindowSize(_windowHandle, out var width, out var height);

        Ui.OpenElementStack.Clear();
        Ui.OpenElementStack.Push(new UiContainer());
        _sample.Build();

        var root = Ui.OpenElementStack.Pop();
        root.PComputedWidth = width;
        root.PComputedHeight = height;
        root.Layout();
        root.Render(_canvas);

        _canvas.Flush();

        SDL_GL_SwapWindow(_windowHandle);
    }

    public void Dispose()
    {
        _canvas.Dispose();
        SDL_GL_DeleteContext(_openGlContextHandle);
        SDL_DestroyWindow(_windowHandle);
    }
}
