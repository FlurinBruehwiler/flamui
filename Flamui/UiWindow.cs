using System.Collections.Concurrent;
using Flamui.Layouting;
using Flamui.UiElements;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;

namespace Flamui;

public partial class UiWindow : IDisposable
{
    public readonly IntPtr _windowHandle;
    private readonly FlamuiComponent _rootComponent;
    public readonly IServiceProvider ServiceProvider;
    private readonly IntPtr _openGlContextHandle;
    private readonly GRContext _grContext;
    public uint Id;
    public bool IsDebugWindow;

    // private UiContainer? _hoveredContainer;
    private UiElement? _activeContainer;

    public readonly UiElementContainer RootContainer;
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

    private readonly Input _input;
    private readonly HitTester _hitTester;
    private readonly TabIndexManager _tabIndexManager = new();
    public readonly Ui Ui = new();

    public UiElement? ActiveDiv
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
    private readonly RegistrationManager _registrationManager;

    public UiWindow(IntPtr windowHandle, FlamuiComponent rootComponent, IServiceProvider serviceProvider)
    {
        _hitTester = new HitTester(this);
        _windowHandle = windowHandle;
        _rootComponent = rootComponent;
        ServiceProvider = serviceProvider;
        _registrationManager = ServiceProvider.GetRequiredService<RegistrationManager>();
        Id = SDL_GetWindowID(_windowHandle);
        _input = new Input(this);

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

        Ui.Window = this;

        RootContainer = new FlexContainer
        {
            Id = new UiID("RootElement", "", 0, 0),
            Window = this
        };

        DebugPaint = Helpers.GetNewAntialiasedPaint();
        DebugPaint.Color = C.Blue8.ToSkColor();

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

        //ToDo cleanup
        foreach (var action in _registrationManager.OnAfterInput)
        {
            action(this);
        }

        HitDetection();

        BuildUi();


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

    public void Close()
    {
        SDL_DestroyWindow(_windowHandle);
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
        RootContainer.Render(RenderContext, new Point());
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
            // Console.WriteLine("rerender");

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
        if (IsDebugWindow)
            return;

        if (DebugSelectionModelEnabled)
        {
            var hoveredElement = HoveredElements.FirstOrDefault(x => x != null);
            if (hoveredElement != null)
            {
                if (IsMouseButtonPressed(MouseButtonKind.Left))
                {
                    DebugSelectedUiElement = hoveredElement;
                    DebugSelectionModelEnabled = false;
                }
                else
                {
                    // DebugOutline(renderContext, hoveredElement.ComputedBounds);
                }
            }
        }

        if (DebugSelectedUiElement is not null && DebugSelectedUiElement.Window == this)
        {
            // DebugOutline(renderContext, DebugSelectedUiElement.ComputedBounds);
        }
    }

    private void DebugOutline(RenderContext renderContext, Bounds rect)
    {
        renderContext.Add(new Save());

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
                SkColor = C.Blue8.ToSkColor()
            },
            UiElement = null
        });
    }

    private void ProcessInputs()
    {
        _input.HandleEvents(Events);
        _tabIndexManager.HandleTab(this);
    }

    private void BuildUi()
    {
        SDL_GetWindowSize(_windowHandle, out var width, out var height);

        Ui.OpenElementStack.Clear();
        Ui.OpenElementStack.Push(RootContainer);
        Ui.Root = RootContainer;

        // RootContainer.ComputedBounds.W = width;
        // RootContainer.ComputedBounds.H = height;

        RootContainer.OpenElement();

        _rootComponent.Build(Ui);

        RootContainer.PrepareLayout(Dir.Vertical);
        RootContainer.Layout(new BoxConstraint(0, width, 0, height));
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
