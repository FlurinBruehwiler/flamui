using System.Collections.Concurrent;
using System.Diagnostics;
using ImSharpUISample.UiElements;
using SkiaSharp;
using static SDL2.SDL;

namespace ImSharpUISample;

public class Window : IDisposable
{
    private readonly IntPtr _windowHandle;
    private readonly IntPtr _openGlContextHandle;
    private readonly GRContext _grContext;
    public uint Id;

    private UiContainer? _hoveredContainer;
    private UiContainer? _activeContainer;
    private readonly UiContainer _rootContainer = new();
    public readonly ConcurrentQueue<SDL_Event> Events = new();

    private UiContainer? HoveredDiv
    {
        get => _hoveredContainer;
        set
        {
            if (HoveredDiv is not null)
            {
                HoveredDiv.IsHovered = false;
            }

            _hoveredContainer = value;
            if (value is not null)
            {
                value.IsHovered = true;
            }
        }
    }

    private UiContainer? ActiveDiv
    {
        get => _activeContainer;
        set
        {
            if (ActiveDiv is not null)
            {
                ActiveDiv.IsActive = false;
            }

            _activeContainer = value;
            if (value is not null)
            {
                value.IsActive = true;
            }
        }
    }

    public Window(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
        Id = SDL_GetWindowID(_windowHandle);

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

        _grContext = GRContext.CreateGl(glInterface, new GRContextOptions
        {
            AvoidStencilBuffers = true
        });
    }

    private readonly Sample _sample = new();

    public void Update()
    {
        HandleEvents();

        SDL_GetWindowSize(_windowHandle, out var width, out var height);

        var renderTarget = new GRBackendRenderTarget(width, height, 0, 8, new GRGlFramebufferInfo(0, 0x8058));

        using var surface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

        surface.Canvas.Clear();

        Ui.AbsoluteDivs.Clear();

        Ui.OpenElementStack.Clear();
        Ui.OpenElementStack.Push(_rootContainer);
        _rootContainer.OpenElement();

        _sample.Build();

        _rootContainer.PComputedWidth = width;
        _rootContainer.PComputedHeight = height;

        _rootContainer.Layout(this);

        ScrollDelta = 0;

        _rootContainer.Render(surface.Canvas);

        if (ActiveDiv is not null)
            ActiveDiv.Clicked = false;

        surface.Canvas.Flush();

        SDL_GL_SwapWindow(_windowHandle);
    }

    private void HandleEvents()
    {
        while (Events.TryDequeue(out var e))
        {
            if (e.type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
            {
                HandleMouseClick(e.motion);
            }
            else if (e.type == SDL_EventType.SDL_MOUSEMOTION)
            {
                HandleMouseMove(e.motion);
            }
            else if (e.type == SDL_EventType.SDL_MOUSEWHEEL)
            {
                HandleScroll(e.wheel);
            }
        }
    }

    public int ScrollDelta { get; set; }

    private void HandleScroll(SDL_MouseWheelEvent wheelEvent)
    {
        ScrollDelta -= wheelEvent.y;
    }

    private void HandleMouseClick(SDL_MouseMotionEvent eventMotion)
    {
        var div = ActualHitTest(_rootContainer, eventMotion.x, eventMotion.y);

        if (div is null)
            return;

        if (ActiveDiv is not null)
        {
            ActiveDiv.IsActive = false;
        }

        ActiveDiv = div;

        ActiveDiv.IsActive = true;
        ActiveDiv.Clicked = true;
    }

    private void HandleMouseMove(SDL_MouseMotionEvent eventMotion)
    {
        if (HoveredDiv is not null)
        {
            var res = ActualHitTest(HoveredDiv, eventMotion.x, eventMotion.y);

            //is in new div
            if (res is null)
            {
                HoveredDiv.IsHovered = false;
                HoveredDiv = ActualHitTest(_rootContainer, eventMotion.x, eventMotion.y);
                if (HoveredDiv is not null)
                {
                    HoveredDiv.IsHovered = true;
                }
            }
            else //is still in old div
            {
                if (res != HoveredDiv)
                {
                    HoveredDiv.IsHovered = false;
                    HoveredDiv = res;
                    HoveredDiv.IsHovered = true;
                }
            }
        }
        else
        {
            HoveredDiv = ActualHitTest(_rootContainer, eventMotion.x, eventMotion.y);
            if (HoveredDiv is not null)
            {
                HoveredDiv.IsHovered = true;
            }
        }
    }

    private UiContainer? ActualHitTest(UiContainer div, double x, double y)
    {
        foreach (var absoluteDiv in Ui.AbsoluteDivs)
        {
            var hit = HitTest(absoluteDiv, x, y);
            if (hit is not null)
                return hit;
        }

        return HitTest(div, x, y);
    }

    private static UiContainer? HitTest(UiContainer div, double x, double y)
    {
        if (DivContainsPoint(div, x, y))
        {
            foreach (var child in div.Children)
            {
                var actualChild = child;

                if (actualChild is not UiContainer divChild) continue;

                var childHit = HitTest(divChild, x, y);
                if (childHit is not null)
                    return childHit;
            }

            return div;
        }

        return null;
    }

    private static bool DivContainsPoint(UiContainer div, double x, double y)
    {
        return div.PComputedX <= x && div.PComputedX + div.PComputedWidth >= x && div.PComputedY <= y &&
               div.PComputedY + div.PComputedHeight >= y;
    }

    public void Dispose()
    {
        SDL_GL_DeleteContext(_openGlContextHandle);
        SDL_DestroyWindow(_windowHandle);
    }
}
