using System.Collections.Concurrent;
using System.Runtime.InteropServices;
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

    // private UiContainer? _hoveredContainer;
    private UiContainer? _activeContainer;
    private readonly UiContainer _rootContainer = new();
    public readonly ConcurrentQueue<SDL_Event> Events = new();

    // private UiContainer? HoveredDiv
    // {
    //     get => _hoveredContainer;
    //     set
    //     {
    //         if (HoveredDiv is not null)
    //         {
    //             HoveredDiv.IsHovered = false;
    //         }
    //
    //         _hoveredContainer = value;
    //         if (value is not null)
    //         {
    //             value.IsHovered = true;
    //         }
    //     }
    // }

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

    private readonly GraphSample _graphSample = new();

    public void Update()
    {
        Ui.Window = this;
        HandleEvents();

        SDL_GetWindowSize(_windowHandle, out var width, out var height);

        var renderTarget = new GRBackendRenderTarget(width, height, 0, 8, new GRGlFramebufferInfo(0, 0x8058));

        using var surface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

        surface.Canvas.Clear();

        Ui.AbsoluteDivs.Clear();

        Ui.OpenElementStack.Clear();
        Ui.OpenElementStack.Push(_rootContainer);
        Ui.Root = _rootContainer;
        _rootContainer.PComputedWidth = width;
        _rootContainer.PComputedHeight = height;

        _rootContainer.OpenElement();

        _graphSample.Build();

        _rootContainer.Layout(this);

        ScrollDelta = 0;

        _rootContainer.Render(surface.Canvas);

        foreach (var deferedRenderedContainer in Ui.DeferedRenderedContainers)
        {
            deferedRenderedContainer.Render(surface.Canvas);
        }

        Ui.DeferedRenderedContainers.Clear();

        Ui.Root = null!;
        surface.Canvas.Flush();
        Ui.Window = null;
        TextInput = string.Empty;
        Keypressed.Clear();
        IsMouseButtonNewlyPressed = false;
        MouseButtonUp = false;
        ClickPos = null;
        LastMousePosition = MousePosition;

        SDL_GL_SwapWindow(_windowHandle);
    }

    public Vector2Int LastMousePosition { get; set; }
    public Vector2Int MousePosition { get; set; }
    public Vector2Int? ClickPos { get; set; }
    public bool IsMouseButtonDown { get; set; }
    public bool IsMouseButtonNewlyPressed { get; set; }
    public bool MouseButtonUp { get; set; }

    private void HandleEvents()
    {
        while (Events.TryDequeue(out var e))
        {
            if (e.type == SDL_EventType.SDL_MOUSEBUTTONDOWN)
            {
                ClickPos = new Vector2Int(e.motion.x, e.motion.y);
                IsMouseButtonDown = true;
                IsMouseButtonNewlyPressed = true;
            }
            else if (e.type == SDL_EventType.SDL_MOUSEBUTTONUP)
            {
                IsMouseButtonDown = false;
                MouseButtonUp = true;
            }
            else if (e.type == SDL_EventType.SDL_MOUSEMOTION)
            {
                MousePosition = new Vector2Int(e.motion.x, e.motion.y);
            }
            else if (e.type == SDL_EventType.SDL_MOUSEWHEEL)
            {
                HandleScroll(e.wheel);
            }
            else if (e.type == SDL_EventType.SDL_TEXTINPUT)
            {
                unsafe
                {
                    //ToDo https://wiki.libsdl.org/SDL2/Tutorials-TextInput
                    TextInput += Marshal.PtrToStringUTF8((IntPtr)e.text.text);
                }
            }
            else if (e.type == SDL_EventType.SDL_KEYDOWN)
            {
                Keypressed.Add(e.key.keysym.scancode);
                Keydown.Add(e.key.keysym.scancode);
            }
            else if (e.type == SDL_EventType.SDL_KEYUP)
            {
                Keydown.Remove(e.key.keysym.scancode);
            }
        }

        if (ClickPos is not null)
        {
            HandleMouseClick(ClickPos.Value);
        }

        // if (mousePos is not null)
        // {
        //     HandleMouseMove(mousePos.Value);
        // }
    }

    public HashSet<SDL_Scancode> Keypressed { get; set; } = new();
    public HashSet<SDL_Scancode> Keydown { get; set; } = new();
    public string TextInput { get; set; } = string.Empty;

    public int ScrollDelta { get; set; }

    private void HandleScroll(SDL_MouseWheelEvent wheelEvent)
    {
        ScrollDelta -= wheelEvent.y;
    }

    private void HandleMouseClick(Vector2Int clickPos)
    {
        var hitSomething = ActualHitTest(_rootContainer, clickPos.X, clickPos.Y, out var parentCanGetFocus);

        if(!hitSomething)
            ActiveDiv = null;

        if (parentCanGetFocus)
            ActiveDiv = null;
    }

    private bool ActualHitTest(UiContainer div, double x, double y, out bool parentCanGetFocus)
    {
        foreach (var absoluteDiv in Ui.AbsoluteDivs)
        {
            if(absoluteDiv.PHidden)
                continue;
            var hit = HitTest(absoluteDiv, x, y, out parentCanGetFocus);
            if (hit)
                return true;
        }

        return HitTest(div, x, y, out parentCanGetFocus);
    }

    private bool HitTest(UiContainer div, double x, double y, out bool parentCanGetFocus)
    {
        if (div.ContainsPoint(x, y))
        {
            foreach (var child in div.Children)
            {
                if (child is not UiContainer divChild)
                    continue;

                var childHit = HitTest(divChild, x, y, out var parentCanGetFocusInner);
                if (childHit)
                {
                    if (parentCanGetFocusInner)
                    {
                        if (div.PFocusable)
                        {
                            parentCanGetFocus = false;
                            return true;
                        }

                        parentCanGetFocus = true;
                        return true;
                    }

                    parentCanGetFocus = false;
                    return true;
                }
            }

            if (div.PFocusable)
            {
                ActiveDiv = div;
                parentCanGetFocus = false;
                return true;
            }

            parentCanGetFocus = true;
            return true;
        }

        parentCanGetFocus = false;
        return false;
    }

    public void Dispose()
    {
        SDL_GL_DeleteContext(_openGlContextHandle);
        SDL_DestroyWindow(_windowHandle);
    }
}

public record struct Vector2Int(int X, int Y)
{
    public static Vector2Int operator -(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.X - b.X, a.Y - b.Y);
    }
}
