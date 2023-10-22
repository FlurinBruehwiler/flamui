using System.Collections.Concurrent;
using System.Diagnostics;
using ImSharpUISample.UiElements;
using SkiaSharp;
using static SDL2.SDL;

namespace ImSharpUISample;

public partial class UiWindow : IDisposable
{
    private readonly IntPtr _windowHandle;
    private readonly IntPtr _openGlContextHandle;
    private readonly GRContext _grContext;
    public uint Id;

    // private UiContainer? _hoveredContainer;
    private UiContainer? _activeContainer;
    public readonly UiContainer RootContainer = new();
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

    public List<UiContainer> HoveredDivs { get; set; } = new();

    public UiWindow(IntPtr windowHandle)
    {
        _hitTester = new HitTester(this);
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

        var startEventHandling = Stopwatch.GetTimestamp();
        _input.HandleEvents(Events);
        _hitTester.HandleHitTest();
        // Console.WriteLine($"EventHandling: {Stopwatch.GetElapsedTime(startEventHandling).TotalMilliseconds}");

        var setup = Stopwatch.GetTimestamp();
        SDL_GetWindowSize(_windowHandle, out var width, out var height);

        using var renderTarget = new GRBackendRenderTarget(width, height, 0, 8, new GRGlFramebufferInfo(0, 0x8058));
        using var surface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

        surface.Canvas.Clear();

        // Console.WriteLine($"Setup: {Stopwatch.GetElapsedTime(setup).TotalMilliseconds}");

        Ui.OpenElementStack.Clear();
        Ui.OpenElementStack.Push(RootContainer);
        Ui.Root = RootContainer;
        RootContainer.ComputedWidth = width;
        RootContainer.ComputedHeight = height;

        RootContainer.OpenElement();

        var startBuild = Stopwatch.GetTimestamp();
        _graphSample.Build();
        // Console.WriteLine($"Building: {Stopwatch.GetElapsedTime(startBuild).TotalMilliseconds}");

        var startLayout = Stopwatch.GetTimestamp();
        RootContainer.Layout(this);
        // Console.WriteLine($"Layouting: {Stopwatch.GetElapsedTime(startLayout).TotalMilliseconds}");

        var startRendering = Stopwatch.GetTimestamp();
        RootContainer.Render(surface.Canvas);

        foreach (var deferedRenderedContainer in Ui.DeferedRenderedContainers)
        {
            deferedRenderedContainer.Render(surface.Canvas);
        }

        Ui.DeferedRenderedContainers.Clear();

        Ui.Root = null!;

        surface.Canvas.Flush();

        // Console.WriteLine($"Rendering: {Stopwatch.GetElapsedTime(startRendering).TotalMilliseconds}");

        Ui.Window = null!;

        var finilizingStart = Stopwatch.GetTimestamp();

        _input.OnAfterFrame();
        HoveredDivs.Clear();

        SDL_GL_SwapWindow(_windowHandle);

        // Console.WriteLine($"Finilizing: {Stopwatch.GetElapsedTime(finilizingStart).TotalMilliseconds}");

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
