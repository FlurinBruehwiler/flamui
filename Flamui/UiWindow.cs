using System.Collections.Concurrent;
using Flamui.UiElements;
using SkiaSharp;

namespace Flamui;

public partial class UiWindow : IDisposable
{
    private readonly IntPtr _windowHandle;
    private readonly FlamuiComponent _rootComponent;
    public readonly IServiceProvider ServiceProvider;
    private readonly IntPtr _openGlContextHandle;
    private readonly GRContext _grContext;
    public uint Id;

    // private UiContainer? _hoveredContainer;
    private UiContainer? _activeContainer;

    public readonly UiContainer RootContainer;
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

    private readonly Input _input = new();
    private readonly HitTester _hitTester;
    private readonly TabIndexManager _tabIndexManager = new();
    private readonly Ui _ui = new();

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

    public List<UiElement> HoveredElements { get; set; } = new();
    public List<UiElement> OldHoveredElements { get; set; } = new();

    public UiWindow(IntPtr windowHandle, FlamuiComponent rootComponent, IServiceProvider serviceProvider)
    {
        _hitTester = new HitTester(this);
        _windowHandle = windowHandle;
        _rootComponent = rootComponent;
        ServiceProvider = serviceProvider;
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

        _ui.Window = this;

        RootContainer = new()
        {
            Id = new UiElementId(),
            Window = this
        };

        DebugPaint = Helpers.GetNewAntialiasedPaint();
        DebugPaint.Color = C.Blue.ToSkColor();

    }

    private SKPaint DebugPaint;

    public RenderContext LastRenderContext = new();
    public RenderContext RenderContext = new();

    public void Update()
    {
        var success = SDL_GL_MakeCurrent(_windowHandle, _openGlContextHandle);
        if (success != 0)
        {
            throw new Exception();
        }

        ProcessInputs();
        HitDetection();
        BuildUi();
        Layout();


        Render();



        //ToDo cleanup
        _input.OnAfterFrame();

        OldHoveredElements.Clear();
        foreach (var uiContainer in HoveredElements)
        {
            OldHoveredElements.Add(uiContainer);
        }
        HoveredElements.Clear();

    }

    private void HitDetection()
    {
        _hitTester.HandleHitTest();
    }

    private void Render()
    {
        CreateRenderInstructions();
        RenderToCanvas();
    }

    private void CreateRenderInstructions()
    {
        RootContainer.Render(RenderContext);
    }



    private void RenderToCanvas()
    {
        SDL_GetWindowSize(_windowHandle, out var width, out var height);

        using var renderTarget = new GRBackendRenderTarget(width, height, 0, 8, new GRGlFramebufferInfo(0, 0x8058));
        using var surface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

        DrawDebugOverlay(RenderContext);

        var requiresRerender = RenderContext.RequiresRerender(LastRenderContext);

        //todo wtf is happening grrrr it makes 0 sense
        if (requiresRerender)
        {
            // var start = Stopwatch.GetTimestamp();

            surface.Canvas.Clear();

            RenderContext.Rerender(surface.Canvas);

            surface.Canvas.Flush();

            // Console.WriteLine(Stopwatch.GetElapsedTime(start).TotalMilliseconds);

        }

        LastRenderContext.Reset();
        //swap Render Contexts
        (LastRenderContext, RenderContext) = (RenderContext, LastRenderContext);

        _renderHappened = requiresRerender;
    }

    private void DrawDebugOverlay(RenderContext renderContext)
    {
        if (DebugSelectedUiElement is not null && DebugSelectedUiElement.Window == this)
        {
            renderContext.Add(new Save());

            var rect = DebugSelectedUiElement.ComputedBounds;

            renderContext.Add(new RectClip
            {
                Bounds = rect,
                ClipOperation = SKClipOperation.Difference,
                Radius = 0
            });

            renderContext.Add(new Rect
            {
                Bounds = rect.Inflate(2, 2),
                Radius = 0,
                RenderPaint = new PlaintPaint
                {
                    SkColor = C.Blue.ToSkColor()
                },
                UiElement = null
            });

        }
    }

    private void ProcessInputs()
    {
        _input.HandleEvents(Events);
        _tabIndexManager.HandleTab(this);
    }

    private void BuildUi()
    {
        SDL_GetWindowSize(_windowHandle, out var width, out var height);

        _ui.OpenElementStack.Clear();
        _ui.OpenElementStack.Push(RootContainer);
        _ui.Root = RootContainer;
        RootContainer.ComputedBounds.W = width;
        RootContainer.ComputedBounds.H = height;

        RootContainer.OpenElement();

        _rootComponent.Build(_ui);
    }

    private void Layout()
    {
        RootContainer.Layout();
    }

    private bool _renderHappened;

    public void SwapWindow()
    {
        var success = SDL_GL_MakeCurrent(_windowHandle, _openGlContextHandle);
        if (success != 0)
        {
            throw new Exception();
        }

        if(_renderHappened)
            SDL_GL_SwapWindow(_windowHandle);

        //ToDo
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
