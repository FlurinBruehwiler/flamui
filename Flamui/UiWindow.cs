using System.Numerics;
using System.Runtime.InteropServices;
using Flamui.Drawing;
using Flamui.Layouting;
using Flamui.PerfTrace;
using Flamui.UiElements;
using Microsoft.Extensions.DependencyInjection;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Flamui;

public unsafe partial class UiWindow : IDisposable
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

    //for testing
    public UiWindow(Ui ui)
    {
        Ui = ui;
    }

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
        if (!isInitialized)
            return;

        using var _ = Systrace.BeginEvent(nameof(OnRender));

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
        if (!isInitialized)
            return;

        Console.WriteLine("Updating");
        Ui.Arena = RenderContext.Arena;

        ProcessInputs();

        //ToDo cleanup
        foreach (var action in _registrationManager.OnAfterInput)
        {
            action(this);
        }

        HitDetection();

        BuildUi();
    }

    public Vector2 DpiScaling;

    private Renderer _renderer = new();

    private bool isInitialized;

    private void OnLoad()
    {
        Console.WriteLine("Loading");
        _hitTester = new HitTester(this);
        _registrationManager = ServiceProvider.GetRequiredService<RegistrationManager>();
        _input = new Input(Window);

        glfwSetWindowContentScaleCallback(Window.Handle, (window, xScale, yScale) => DpiScaling = new Vector2(xScale, yScale));

        float xScale = 1;
        float yScale = 1;
        glfwGetWindowContentScale(Window.Handle, &xScale, &yScale);
        DpiScaling = new Vector2(xScale, yScale);
        // Console.WriteLine($"Initial scale: {xScale}, {yScale}");

        Ui.Window = this;
        Ui.FontManager = new FontManager();

        RootContainer = new FlexContainer
        {
            Id = new UiID("RootElement", "", 0, 0),
            Window = this
        };

        _renderer.Initialize(Window);

        isInitialized = true;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CallbackDelegate(IntPtr window, float xScale, float yScale);

    [DllImport("glfw3.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glfwSetWindowContentScaleCallback(IntPtr window, CallbackDelegate callback);

    [DllImport("glfw3.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glfwGetWindowContentScale(IntPtr window, float* xScale, float* yScale);

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

    private void CreateRenderInstructions()
    {
        RenderContext.AddMatrix(Matrix4X4.CreateScale(DpiScaling.X));
        RootContainer.Render(RenderContext, new Point());
    }

    private void RenderToCanvas()
    {
        using var _ = Systrace.BeginEvent("RenderToCanvas");

        DrawDebugOverlay(RenderContext);


        //todo check if something has actually changed
        RenderContext.Rerender(_renderer);



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

        Ui.ResetStuff();
        Ui.OpenElementStack.Push(RootContainer);

        // RootContainer.ComputedBounds.W = width;
        // RootContainer.ComputedBounds.H = height;

        RootContainer.OpenElement();

        _rootComponent.Build(Ui);

        RootContainer.PrepareLayout(Dir.Vertical);
        RootContainer.Layout(new BoxConstraint(0, Window.Size.X / DpiScaling.X, 0, Window.Size.Y / DpiScaling.Y));
    }

    private bool _renderHappened;

    public void Dispose()
    {
        // SDL_GL_DeleteContext(_openGlContextHandle);
        // SDL_DestroyWindow(Window);
    }
}
