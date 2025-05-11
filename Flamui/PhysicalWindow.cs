using System.Diagnostics;
using System.Numerics;
using Flamui.Drawing;
using Flamui.PerfTrace;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using MouseButton = Silk.NET.GLFW.MouseButton;

namespace Flamui;

//this represents the actual physical window....

public class PhysicalWindow
{
    private PhysicalWindow() { }

    public IWindow GlfWindow;

    public Glfw GlfwApi;

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

    public UiTree UiTree;

    public CommandBuffer LastCommandBuffer;
    private readonly Renderer _renderer = new();

    public static PhysicalWindow Create(IWindow window, UiTree uiTree)
    {
        var w = new PhysicalWindow();

        w.UiTree = uiTree;
        w.GlfWindow = window;
        w.GlfwApi = Glfw.GetApi();

        uiTree.UiTreeHost = new NativeUiTreeHost(window, w.GlfwApi);
        window.Load += w.OnLoad;
        window.Update += w.OnUpdate;
        window.Render += w.OnRender;

        return w;
    }

    private void OnRender(double obj)
    {
        var start = Stopwatch.GetTimestamp();

        using var _ = Systrace.BeginEvent(nameof(OnRender));

        var commands = StaticFunctions.Render(UiTree, GetWorldToScreenMatrix());
        // if (!commands.IsEqualTo(LastCommandBuffer)) //todo, re-add change detection
        {
            LastCommandBuffer = commands;

            _renderer.Gl.Viewport(GlfWindow.Size);
            StaticFunctions.ExecuteRenderInstructions(commands, _renderer, Ui.Arena);
            GlfWindow.GLContext.SwapBuffers();
        }

        // OldHoveredElements.Clear();
        // foreach (var uiContainer in HoveredElements)
        // {
        //     OldHoveredElements.Add(uiContainer);
        // }
        // HoveredElements.Clear();

        var end = Stopwatch.GetElapsedTime(start);
        if (end.TotalMilliseconds < 16) //todo we should probably also detect the refresh rate of monitor, to know how long to sleep for (or we can try to get vsync working)s
        {
            // Console.WriteLine($"Sleeping for {end.TotalMilliseconds}ms");
            // Thread.Sleep(TimeSpan.FromMilliseconds(16 - end.TotalMilliseconds));
        }
    }

    private Vector2 lastScreenMousePosition;

    private unsafe void OnUpdate(double obj)
    {
        GlfwApi.GetCursorPos((WindowHandle*)GlfWindow.Handle, out var x, out var y);
        var screenMousePos = new Vector2((float)x, (float)y);

        HandleZoomAndStuff(screenMousePos - lastScreenMousePosition);

        UiTree.MousePosition = screenMousePos;//ScreenToWorld();

        Console.WriteLine(UiTree.MousePosition);

        UiTree.Update(GlfWindow.Size.X / GetCompleteScaling().X, GlfWindow.Size.Y / GetCompleteScaling().Y);

        lastScreenMousePosition = screenMousePos;
    }

    private unsafe void OnLoad()
    {
        int darkMode = 1;
        if (OperatingSystem.IsWindows())
        {
            WindowNative.DwmSetWindowAttribute(GlfWindow.Native.Win32.Value.Hwnd, 20, ref darkMode, sizeof(int));
        }

        Console.WriteLine("Loading");
        // _hitTester = new HitTester(this);
        // _registrationManager = ServiceProvider.GetRequiredService<RegistrationManager>();
        Input.SetupInputCallbacks(this);

        if (OperatingSystem.IsWindows())
        {
            WindowNative.glfwSetWindowContentScaleCallback(GlfWindow.Handle, (window, xScale, yScale) => DpiScaling = new Vector2(xScale, yScale));
        }
        else if(OperatingSystem.IsLinux())
        {
            LinuxNative.glfwSetWindowContentScaleCallback(GlfWindow.Handle, (window, xScale, yScale) => DpiScaling = new Vector2(xScale, yScale));
        }

        float xScale = 1;
        float yScale = 1;

        if (OperatingSystem.IsWindows())
        {
            WindowNative.glfwGetWindowContentScale(GlfWindow.Handle, &xScale, &yScale);
        }
        else if (OperatingSystem.IsLinux())
        {
            LinuxNative.glfwGetWindowContentScale(GlfWindow.Handle, &xScale, &yScale);
        }

        DpiScaling = new Vector2(xScale, yScale);

        _renderer.Initialize(GlfWindow);
    }

    public Vector2 Zoom = new(1, 1);
    public Vector2 ZoomOffset = new(0, 0);
    public Vector2 ZoomTarget;

    private unsafe void HandleZoomAndStuff(Vector2 mouseDelta)
    {
        if (UiTree.IsKeyPressed(Key.Escape))
        {
            //todo(refactor)
        }

        if (UiTree.IsMouseButtonDown(Silk.NET.Input.MouseButton.Right))
        {
            Console.WriteLine(mouseDelta.ToString());
            ZoomTarget += mouseDelta * -1 / Zoom;
        }

        if (UiTree.IsKeyDown(Key.ControlLeft) && UiTree.ScrollDelta.Y != 0)
        {
            var factor = (float)Math.Pow(1.1, UiTree.ScrollDelta.Y);
            UserScaling *= new Vector2(factor, factor);
            UserScaling = new Vector2(Math.Clamp(UserScaling.X, 0.1f, 10f), Math.Clamp(UserScaling.Y, 0.1f, 10f));
        }

        if (UiTree.IsKeyDown(Key.AltLeft) && UiTree.ScrollDelta.Y != 0)
        {
            GlfwApi.GetCursorPos((WindowHandle*)GlfWindow.Handle, out var x, out var y);

            var factor = (float)Math.Pow(1.1, UiTree.ScrollDelta.Y);
            var mouseWorldPos = UiTree.MousePosition;
            ZoomOffset = new Vector2((float)x, (float)y);
            ZoomTarget = mouseWorldPos;
            Zoom *= new Vector2(factor, factor);
            Zoom = new Vector2(Math.Clamp(Zoom.X, 0.01f, 100f), Math.Clamp(Zoom.Y, 0.01f, 100f));
        }

        if (UiTree.IsKeyPressed(Key.R))
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

    public Matrix4X4<float> GetWorldToScreenMatrix()
    {
        var origin = Matrix4X4.CreateTranslation(-ZoomTarget.X, -ZoomTarget.Y, 0);
        var scale = Matrix4X4.CreateScale(GetCompleteScaling().X * Zoom.X);

        var translate = Matrix4X4.CreateTranslation(ZoomOffset.X, ZoomOffset.Y, 0);

        return  origin * scale * translate;
    }
}