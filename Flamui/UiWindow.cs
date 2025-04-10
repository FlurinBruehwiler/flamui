﻿using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Flamui.Drawing;
using Flamui.Layouting;
using Flamui.PerfTrace;
using Flamui.UiElements;
using Microsoft.Extensions.DependencyInjection;
using Silk.NET.GLFW;
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

    public double DeltaTime;

    private void OnRender(double obj)
    {
        DeltaTime = obj;

        if (!isInitialized)
            return;

        //is this correct?

        var start = Stopwatch.GetTimestamp();

        using var _ = Systrace.BeginEvent(nameof(OnRender));

        Render();

        //ToDo cleanup


        OldHoveredElements.Clear();
        foreach (var uiContainer in HoveredElements)
        {
            OldHoveredElements.Add(uiContainer);
        }
        HoveredElements.Clear();

        var end = Stopwatch.GetElapsedTime(start);
        if (end.TotalMilliseconds < 16) //todo we should probably also detect the refresh rate of monitor, to know how long to sleep for (or we can try to get vsync working)s
        {
            // Console.WriteLine($"Sleeping for {end.TotalMilliseconds}");
            // Thread.Sleep(TimeSpan.FromMilliseconds(16 - end.TotalMilliseconds));
        }
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

        CreateRenderInstructions();

        Input.OnAfterFrame();
    }

    private void HandleZoomAndStuff()
    {
        if (IsKeyPressed(Key.Escape))
        {
            Close();
        }

        if (IsMouseButtonDown(MouseButton.Right))
        {
            var mouseDelta = MouseDelta;
            ZoomTarget += mouseDelta * -1 / Zoom;
        }
        
        if (IsKeyDown(Key.ControlLeft) && ScrollDeltaY != 0)
        {
            var factor = (float)Math.Pow(1.1, ScrollDeltaY);
            UserScaling *= new Vector2(factor, factor);
            UserScaling = new Vector2(Math.Clamp(UserScaling.X, 0.1f, 10f), Math.Clamp(UserScaling.Y, 0.1f, 10f));
        }

        if (IsKeyDown(Key.AltLeft) && ScrollDeltaY != 0)
        {
            var factor = (float)Math.Pow(1.1, ScrollDeltaY);
            var mouseWorldPos = MousePosition;
            ZoomOffset = MouseScreenPosition;
            ZoomTarget = mouseWorldPos;
            Zoom *= new Vector2(factor, factor);
            Zoom = new Vector2(Math.Clamp(Zoom.X, 0.01f, 100f), Math.Clamp(Zoom.Y, 0.01f, 100f));
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

    // private ZoomType zoomType = ZoomType.ScaleContent;

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
        int darkMode = 1;
        if (OperatingSystem.IsWindows())
        {
            WindowNative.DwmSetWindowAttribute(Window.Native.Win32.Value.Hwnd, 20, ref darkMode, sizeof(int));
        }

        Console.WriteLine("Loading");
        _hitTester = new HitTester(this);
        _registrationManager = ServiceProvider.GetRequiredService<RegistrationManager>();
        Input = Input.ConstructInputFromWindow(Window);

        if (OperatingSystem.IsWindows())
        {
            WindowNative.glfwSetWindowContentScaleCallback(Window.Handle, (window, xScale, yScale) => DpiScaling = new Vector2(xScale, yScale));
        }
        else if(OperatingSystem.IsLinux())
        {
            LinuxNative.glfwSetWindowContentScaleCallback(Window.Handle, (window, xScale, yScale) => DpiScaling = new Vector2(xScale, yScale));
        }

        float xScale = 1;
        float yScale = 1;

        if (OperatingSystem.IsWindows())
        {
            WindowNative.glfwGetWindowContentScale(Window.Handle, &xScale, &yScale);
        }
        else if (OperatingSystem.IsLinux())
        {
            LinuxNative.glfwGetWindowContentScale(Window.Handle, &xScale, &yScale);
        }

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
        RenderToCanvas();
    }

    private void CreateRenderInstructions()
    {
        using var _ = Systrace.BeginEvent(nameof(CreateRenderInstructions));

        RenderContext.PushMatrix(GetWorldToScreenMatrix());
        RootContainer.Render(RenderContext, new Point());
    }

    private void RenderToCanvas()
    {
        // using var _ = Systrace.BeginEvent("RenderToCanvas");

        if (!RenderContextAreSame(RenderContext, LastRenderContext))
        {
            // RenderContext.PrintCommands();

            _renderer.Gl.Viewport(Window.Size);
            RenderContext.Rerender(_renderer);
            Window.GLContext.SwapBuffers();
        }

        LastRenderContext.Reset();
        //swap Render Contexts
        (LastRenderContext, RenderContext) = (RenderContext, LastRenderContext);

        _renderHappened = true;
        // _renderHappened = requiresRerender;
    }

    private bool RenderContextAreSame(RenderContext cA, RenderContext cB)
    {
        using var _ = Systrace.BeginEvent(nameof(RenderContextAreSame));

        // using var _ = ConsoleTimer.Time("Render Equality");

        if (cA.CommandBuffers.Count != cB.CommandBuffers.Count)
        {
            return false;
        }

        foreach (var (key, commandsA) in cA.CommandBuffers)
        {
            if (!cB.CommandBuffers.TryGetValue(key, out var commandsB))
                return false;

            if (! ArenaChunkedList<Command>.CompareGrowableArenaBuffers(commandsA, commandsB))
                return false;
        }

        return true;
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

        // Console.WriteLine(Window.Size.X);

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
