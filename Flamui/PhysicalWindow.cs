using System.Diagnostics;
using System.Numerics;
using Flamui.Drawing;
using Flamui.PerfTrace;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;


namespace Flamui;

//this represents the actual physical window....

public sealed class PhysicalWindow
{
    private PhysicalWindow() { }

    public IWindow GlfwWindow;

    public Glfw GlfwApi;

    /// <summary>
    /// The DPI scaling from the OS
    /// </summary>
    public Vector2 DpiScaling;

    /// <summary>
    /// The User can zoom in, to make stuff bigger
    /// </summary>
    public Vector2 UserScaling = new(1f, 1f);

    /// <summary>
    /// the complete scaling
    /// </summary>
    public Vector2 GetCompleteScaling() => DpiScaling * UserScaling;

    public UiTree UiTree;

    public CommandBuffer LastCommandBuffer;
    private readonly Renderer _renderer = new();
    private Vector2 lastScreenMousePosition;
    private int framecount;

    public static PhysicalWindow Create(IWindow window, UiTree uiTree)
    {
        var w = new PhysicalWindow();

        w.UiTree = uiTree;
        w.GlfwWindow = window;
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

            _renderer.Gl.Viewport(GlfwWindow.Size);
            StaticFunctions.ExecuteRenderInstructions(commands, _renderer, Ui.Arena);
            GlfwWindow.GLContext.SwapBuffers();
        }

        // OldHoveredElements.Clear();
        // foreach (var uiContainer in HoveredElements)
        // {
        //     OldHoveredElements.Add(uiContainer);
        // }
        // HoveredElements.Clear();

        var end = Stopwatch.GetElapsedTime(start);
        //this is explicitly 10 here and not 16, don't change without knowing what your doing
        if (end.TotalMilliseconds < 10) //todo we should probably also detect the refresh rate of monitor, to know how long to sleep for (or we can try to get vsync working)s
        {
            // Console.WriteLine($"Sleeping for {16 - end.TotalMilliseconds}ms");
            // Thread.Sleep(TimeSpan.FromMilliseconds(16 - end.TotalMilliseconds));
        }
    }

    private unsafe void OnUpdate(double obj)
    {
        framecount++;

        //a few frames after startup we do this to reduce memory usage drastically
        //https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-setprocessworkingsetsize
        if (framecount == 10)
        {
            if (OperatingSystem.IsWindows())
            {
                using var process = Process.GetCurrentProcess();

                WindowsNative.SetProcessWorkingSetSize(process.Handle, (UIntPtr)sizeof(UIntPtr) - 1, (UIntPtr)sizeof(UIntPtr) - 1);
            }
        }

        GlfwApi.GetCursorPos((WindowHandle*)GlfwWindow.Handle, out var x, out var y);
        var screenMousePos = new Vector2((float)x, (float)y);

        HandleZoomAndStuff(screenMousePos - lastScreenMousePosition, screenMousePos); //still not sure if this should be on the window, or if we can put it onto the UiTree

        UiTree.MousePosition = ScreenToWorld(screenMousePos); // todo, we don't do ScreenToWorld here, because we would do it twice because of the matrix mult in the HitTest

        UiTree.Update(GlfwWindow.Size.X / GetCompleteScaling().X, GlfwWindow.Size.Y / GetCompleteScaling().Y);

        lastScreenMousePosition = screenMousePos;
    }

    private unsafe void OnLoad()
    {
        int darkMode = 1;
        if (OperatingSystem.IsWindows())
        {
            WindowsNative.DwmSetWindowAttribute(GlfwWindow.Native.Win32.Value.Hwnd, 20, ref darkMode, sizeof(int));
        }

        Console.WriteLine("Loading");
        // _hitTester = new HitTester(this);
        // _registrationManager = ServiceProvider.GetRequiredService<RegistrationManager>();
        Input.SetupInputCallbacks(this);

        if (OperatingSystem.IsWindows())
        {
            WindowsNative.glfwSetWindowContentScaleCallback(GlfwWindow.Handle, (window, xScale, yScale) => DpiScaling = new Vector2(xScale, yScale));
        }
        else if(OperatingSystem.IsLinux())
        {
            LinuxNative.glfwSetWindowContentScaleCallback(GlfwWindow.Handle, (window, xScale, yScale) => DpiScaling = new Vector2(xScale, yScale));
        }

        float xScale = 1;
        float yScale = 1;

        if (OperatingSystem.IsWindows())
        {
            WindowsNative.glfwGetWindowContentScale(GlfwWindow.Handle, &xScale, &yScale);
        }
        else if (OperatingSystem.IsLinux())
        {
            LinuxNative.glfwGetWindowContentScale(GlfwWindow.Handle, &xScale, &yScale);
        }

        DpiScaling = new Vector2(xScale, yScale);

        _renderer.Initialize(GlfwWindow);
    }

    public float Zoom = 1f;
    public Vector2 ZoomOffset = new(0, 0);
    public Vector2 ZoomTarget;

    //not yet sure if this is the correct place for this....
    private unsafe void HandleZoomAndStuff(Vector2 mouseDelta, Vector2 mouseScreenPos)
    {
        if (UiTree.IsKeyPressed(Key.Escape))
        {
            GlfwWindow.Close();
        }

        if (UiTree.IsMouseButtonDown(Silk.NET.Input.MouseButton.Right))
        {
            ZoomTarget += mouseDelta * (-1f / Zoom);
        }

        if (UiTree.IsKeyDown(Key.ControlLeft) && UiTree.ScrollDelta.Y != 0)
        {
            var factor = (float)Math.Pow(1.1, UiTree.ScrollDelta.Y);
            UserScaling *= new Vector2(factor, factor);
            UserScaling = new Vector2(Math.Clamp(UserScaling.X, 0.1f, 10f), Math.Clamp(UserScaling.Y, 0.1f, 10f));
        }

        var mouseWorldPos = ScreenToWorld(mouseScreenPos);

        // Console.WriteLine($"ScreenPos: {mouseScreenPos}, WorldPos: {mouseWorldPos}");

        if (UiTree.IsKeyDown(Key.AltLeft) && UiTree.ScrollDelta.Y != 0)
        {
            ZoomOffset = mouseScreenPos;
            ZoomTarget = mouseWorldPos;

            var scale = 0.2f * UiTree.ScrollDelta.Y;
            Zoom = Math.Clamp(MathF.Exp(MathF.Log(Zoom) + scale), 0.125f, 64f);
        }

        if (UiTree.IsKeyPressed(Key.R))
        {
            UserScaling = new Vector2(1f, 1f);
            Zoom = 1;
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
        var scale = Matrix4X4.CreateScale(GetCompleteScaling().X * Zoom);

        var translate = Matrix4X4.CreateTranslation(ZoomOffset.X, ZoomOffset.Y, 0);

        return  origin * scale * translate;
    }
}