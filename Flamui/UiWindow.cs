using System.Numerics;
using System.Runtime.InteropServices;
using Flamui.Drawing;
using Flamui.Layouting;
using Flamui.PerfTrace;
using Flamui.UiElements;
using Microsoft.Extensions.DependencyInjection;
using Silk.NET.Input;
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

    public Input Input;
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
        Input.OnAfterFrame();

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

        Ui.Arena = RenderContext.Arena;

        ProcessInputs();

        //ToDo cleanup
        foreach (var action in _registrationManager.OnAfterInput)
        {
            action(this);
        }

        HitDetection();

        HandleZoomAndStuff();

        BuildUi();
    }

    public enum ZoomType
    {
        ScaleContent, //Makes the content bigger changes the layout to still fit everything
        ZoomContent, //Zooms in on part of the ui, while keeping everything sharp, allows pan
        ZoomCanvas //Zoom in on part of the image, without changing the resolution, allows pan
    }

    private void HandleZoomAndStuff()
    {
        if (IsKeyPressed(Key.Escape))
        {
            Close();
        }

        if (IsKeyPressed(Key.Number1))
        {
            zoomType = ZoomType.ScaleContent;
        }
        else if (IsKeyPressed(Key.Number2))
        {
            zoomType = ZoomType.ZoomContent;
        }
        else if (IsKeyPressed(Key.Number3))
        {
            zoomType = ZoomType.ZoomCanvas;
        }

        if (IsMouseButtonDown(MouseButton.Right) && zoomType is ZoomType.ZoomContent)
        {
            var mouseDelta = MouseDelta;
            ZoomTarget += mouseDelta * -1 / Zoom;
        }

        if (IsKeyDown(Key.ControlLeft) && ScrollDeltaY != 0)
        {
            var factor = (float)Math.Pow(1.1, ScrollDeltaY);

            switch (zoomType)
            {
                case ZoomType.ScaleContent:
                    UserScaling *= new Vector2(factor, factor);
                    UserScaling = new Vector2(Math.Clamp(UserScaling.X, 0.1f, 10f), Math.Clamp(UserScaling.Y, 0.1f, 10f));
                    break;
                case ZoomType.ZoomContent:
                    var mouseWorldPos = MousePosition;
                    ZoomOffset = MouseScreenPosition;
                    ZoomTarget = mouseWorldPos;
                    Zoom *= new Vector2(factor, factor);
                    Zoom = new Vector2(Math.Clamp(Zoom.X, 0.01f, 100f), Math.Clamp(Zoom.Y, 0.01f, 100f));
                    break;
                case ZoomType.ZoomCanvas:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (IsKeyPressed(Key.R))
        {
            UserScaling = new Vector2(1, 1);
            Zoom = new Vector2(1, 1);
            ZoomOffset = new Vector2();
            ZoomTarget = new Vector2();
        }
    }

    private Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        if (Matrix4X4.Invert(GetWorldToScreenMatrix(), out var inverted))
        {
            return screenPosition.Multiply(inverted);
        }

        throw new Exception("ahh");
    }

    private Matrix4X4<float> GetWorldToScreenMatrix()
    {
        var origin = Matrix4X4.CreateTranslation(-ZoomTarget.X, -ZoomTarget.Y, 0);
        var scale = Matrix4X4.CreateScale(GetCompleteScaling().X * Zoom.X);

        var translate = Matrix4X4.CreateTranslation(ZoomOffset.X, ZoomOffset.Y, 0);

        return  origin * scale * translate;
    }

    private ZoomType zoomType = ZoomType.ScaleContent;

    public Vector2 Zoom = new(1, 1);
    public Vector2 ZoomOffset = new(0, 0);
    public Vector2 ZoomTarget;

    /// <summary>
    /// The DPI scaling from the OS
    /// </summary>
    public Vector2 DpiScaling;

    /// <summary>
    /// The User can zoom in, to make stuff bigger
    /// </summary>
    public Vector2 UserScaling = new(1, 1);

    /// <summary>
    /// the complete scaling
    /// </summary>
    public Vector2 GetCompleteScaling() => DpiScaling * UserScaling;

    private Renderer _renderer = new();

    private bool isInitialized;

    private void OnLoad()
    {
        Console.WriteLine("Loading");
        _hitTester = new HitTester(this);
        _registrationManager = ServiceProvider.GetRequiredService<RegistrationManager>();
        Input = Input.ConstructInputFromWindow(Window);

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
        Window.Close();
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
        RenderContext.PushMatrix(GetWorldToScreenMatrix());
        RootContainer.Render(RenderContext, new Point());
    }

    private void RenderToCanvas()
    {
        using var _ = Systrace.BeginEvent("RenderToCanvas");

        // DrawDebugOverlay(RenderContext);

        //RenderContext.PrintCommands();//todo remove
        //todo check if something has actually changed
        RenderContext.Rerender(_renderer);



        LastRenderContext.Reset();
        //swap Render Contexts
        (LastRenderContext, RenderContext) = (RenderContext, LastRenderContext);

        _renderHappened = true;
        // _renderHappened = requiresRerender;
    }

    // private void DrawDebugOverlay(RenderContext renderContext)
    // {
    //     if (IsDebugWindow)
    //         return;
    //
    //     if (DebugSelectionModelEnabled)
    //     {
    //         var hoveredElement = HoveredElements.FirstOrDefault(x => x != null);
    //         if (hoveredElement != null)
    //         {
    //             if (IsMouseButtonPressed(MouseButton.Left))
    //             {
    //                 DebugSelectedUiElement = hoveredElement;
    //                 DebugSelectionModelEnabled = false;
    //             }
    //             else
    //             {
    //                 // DebugOutline(renderContext, hoveredElement.ComputedBounds);
    //             }
    //         }
    //     }
    //
    //     if (DebugSelectedUiElement is not null && DebugSelectedUiElement.Window == this)
    //     {
    //         // DebugOutline(renderContext, DebugSelectedUiElement.ComputedBounds);
    //     }
    // }

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
        RootContainer.Layout(new BoxConstraint(0, Window.Size.X / GetCompleteScaling().X, 0, Window.Size.Y / GetCompleteScaling().Y));
    }

    private bool _renderHappened;

    public void Dispose()
    {
        // SDL_GL_DeleteContext(_openGlContextHandle);
        // SDL_DestroyWindow(Window);
    }
}
