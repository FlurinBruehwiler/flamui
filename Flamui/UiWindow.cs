using System.Collections.Concurrent;
using System.Diagnostics;
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

    public readonly UiContainer RootContainer = new()
    {
        Id = new UiElementId(),
    };
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
    }

    public RenderContext LastRenderContext = new();
    public RenderContext RenderContext = new();

    public void Update()
    {
        Window = this;


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

    private void ProcessInputs()
    {
        _input.HandleEvents(Events);
        _tabIndexManager.HandleTab();
    }

    private void BuildUi()
    {
        SDL_GetWindowSize(_windowHandle, out var width, out var height);

        OpenElementStack.Clear();
        OpenElementStack.Push(RootContainer);
        Root = RootContainer;
        RootContainer.ComputedBounds.W = width;
        RootContainer.ComputedBounds.H = height;

        RootContainer.OpenElement();

        _rootComponent.Build();
    }

    private void Layout()
    {
        RootContainer.Layout(this);
    }

    private bool _renderHappened;

    public void SwapWindow()
    {
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
