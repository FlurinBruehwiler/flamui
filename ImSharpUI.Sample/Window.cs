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
    public UiContainer? HoveredDiv
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
    public UiContainer? ActiveDiv
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
        SDL_GetWindowSize(_windowHandle, out var width, out var height);

        var renderTarget = new GRBackendRenderTarget(width, height, 0, 8, new GRGlFramebufferInfo(0, 0x8058));

        using var surface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.TopLeft, SKColorType.Rgba8888);

        surface.Canvas.Clear();

        Ui.OpenElementStack.Clear();
        Ui.OpenElementStack.Push(new UiContainer());
        _sample.Build();

        var root = Ui.OpenElementStack.Pop();
        root.PComputedWidth = width;
        root.PComputedHeight = height;
        root.Layout();
        root.Render(surface.Canvas);

        surface.Canvas.Flush();

        SDL_GL_SwapWindow(_windowHandle);
    }

    public void HandleMouseClick(SDL_MouseMotionEvent eventMotion)
    {

    }

    public void HandleMouseMove(SDL_MouseMotionEvent eventMotion)
    {
        if (HoveredDiv is not null)
        {
            var res = ActualHitTest(HoveredDiv, eventMotion.x, eventMotion.y);

            if (res is null)
            {
                HoveredDiv.IsHovered = false;
                HoveredDiv = ActualHitTest(divRoot2, pointer.Position.X, pointer.Position.Y);
                if (HoveredDiv is not null)
                {
                    HoveredDiv.IsHovered = true;
                    if (HoveredDiv.PHoverColor is not null)
                    {
                        _windowManager.DoPaint(new Rect());
                    }
                }
            }
            else
            {
                if (res != HoveredDiv)
                {
                    HoveredDiv.IsHovered = false;
                    HoveredDiv = res;
                    HoveredDiv.IsHovered = true;
                    if (HoveredDiv.PHoverColor is not null)
                    {
                        _windowManager.DoPaint(new Rect());
                    }
                }
            }
        }
        else
        {
            HoveredDiv = ActualHitTest(divRoot2, pointer.Position.X, pointer.Position.Y);
            if (HoveredDiv is not null)
            {
                HoveredDiv.IsHovered = true;
                if (HoveredDiv.PHoverColor is not null)
                {
                    _windowManager.DoPaint(new Rect());
                }
            }
        }
    }

    private UiContainer? ActualHitTest(UiContainer div, double x, double y)
    {
        //todo absolute divs

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
