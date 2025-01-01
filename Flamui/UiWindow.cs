using Flamui.Drawing;
using Flamui.Layouting;
using Flamui.UiElements;
using Microsoft.Extensions.DependencyInjection;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace Flamui;

public partial class UiWindow : IDisposable
{
    public IWindow Window;
    private FlamuiComponent _rootComponent;
    public IServiceProvider ServiceProvider;
    public bool IsDebugWindow;

    // private UiContainer? _hoveredContainer;
    private UiElement? _activeContainer;

    public UiElementContainer RootContainer;

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

    private Input _input;
    private HitTester _hitTester;
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
    private RegistrationManager _registrationManager;

    public UiWindow(IWindow window, FlamuiComponent rootComponent, IServiceProvider serviceProvider)
    {
        Window = window;
        _rootComponent = rootComponent;
        ServiceProvider = serviceProvider;

        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
    }

    private void OnRender(double obj)
    {
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

    private void OnUpdate(double obj)
    {
        ProcessInputs();

        //ToDo cleanup
        foreach (var action in _registrationManager.OnAfterInput)
        {
            action(this);
        }

        HitDetection();

        BuildUi();
    }

    private Renderer _renderer = new();

    private void OnLoad()
    {
        _hitTester = new HitTester(this);
        _registrationManager = ServiceProvider.GetRequiredService<RegistrationManager>();
        _input = new Input(Window);

        Ui.Window = this;

        RootContainer = new FlexContainer
        {
            Id = new UiID("RootElement", "", 0, 0),
            Window = this
        };

        _renderer.Initialize(Window);
    }

    public RenderContext LastRenderContext = new();
    public RenderContext RenderContext = new();

    public void Close()
    {

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

    private unsafe void CreateRenderInstructions()
    {
        RootContainer.Render(RenderContext, new Point());
    }

    private void RenderToCanvas()
    {
        DrawDebugOverlay(RenderContext);

        // var requiresRerender = RenderContext.RequiresRerender(LastRenderContext);

        //todo wtf is happening grrrr it makes 0 sense
        if (true)
        {
            // Console.WriteLine("rerender");

            // var start = Stopwatch.GetTimestamp();


            RenderContext.Rerender(_renderer);

            // Console.WriteLine(Stopwatch.GetElapsedTime(start).TotalMilliseconds);

        }

        LastRenderContext.Reset();
        //swap Render Contexts
        (LastRenderContext, RenderContext) = (RenderContext, LastRenderContext);

        _renderHappened = true;
        // _renderHappened = requiresRerender;
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
                if (IsMouseButtonPressed(MouseButton.Left))
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
        // renderContext.Add(new Save());
        //
        // renderContext.Add(new RectClip
        // {
        //     Bounds = rect,
        //     ClipOperation = SKClipOperation.Difference,
        //     Radius = 0
        // });
        //
        // renderContext.Add(new Rect
        // {
        //     Bounds = rect.Inflate(2, 2),
        //     Radius = 0,
        //     RenderPaint = new PlaintPaint
        //     {
        //         SkColor = C.Blue8.ToSkColor()
        //     },
        //     UiElement = null
        // });
    }

    private void ProcessInputs()
    {
        // _input.HandleEvents(Events);
        _tabIndexManager.HandleTab(this);
    }

    private void BuildUi()
    {
        // SDL_GetWindowSize(Window, out var width, out var height);

        Ui.OpenElementStack.Clear();
        Ui.OpenElementStack.Push(RootContainer);
        Ui.Root = RootContainer;

        // RootContainer.ComputedBounds.W = width;
        // RootContainer.ComputedBounds.H = height;

        RootContainer.OpenElement();

        _rootComponent.Build(Ui);

        RootContainer.PrepareLayout(Dir.Vertical);
        RootContainer.Layout(new BoxConstraint(0, Window.Size.X, 0, Window.Size.Y));
    }

    private bool _renderHappened;

    public void SwapWindow()
    {
        // var success = SDL_GL_MakeCurrent(Window, _openGlContextHandle);
        // if (success != 0)
        // {
        //     throw new Exception();
        // }
        //
        // if(_renderHappened)
        //     SDL_GL_SwapWindow(Window);
        //
        // //ToDo
    }

    public void Dispose()
    {
        // SDL_GL_DeleteContext(_openGlContextHandle);
        // SDL_DestroyWindow(Window);
    }
}
