using System.Diagnostics;
using System.Numerics;
using Flamui.Drawing;
using Flamui.PerfTrace;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Flamui;

//this represents the actual physical window....

public class PhysicalWindow
{
    private PhysicalWindow() { }

    public IWindow Window;
    public Input Input;

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

    // public RenderContext RenderContext = new();
    public CommandBuffer LastCommandBuffer;
    private readonly Renderer _renderer = new();

    public static PhysicalWindow Create(IWindow window, UiTree uiTree)
    {
        var w = new PhysicalWindow();

        w.UiTree = uiTree;
        w.Window = window;
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
        if (!commands.IsEqualTo(LastCommandBuffer))
        {
            LastCommandBuffer = commands;

            _renderer.Gl.Viewport(Window.Size);
            StaticFunctions.ExecuteRenderInstructions(commands, _renderer, null);
            Window.GLContext.SwapBuffers();
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
            // Console.WriteLine($"Sleeping for {end.TotalMilliseconds}");
            // Thread.Sleep(TimeSpan.FromMilliseconds(16 - end.TotalMilliseconds));
        }
    }

    private void OnUpdate(double obj)
    {
        UiTree.Update(Window.Size.X / GetCompleteScaling().X, Window.Size.Y / GetCompleteScaling().Y);
    }

    private unsafe void OnLoad()
    {
        int darkMode = 1;
        if (OperatingSystem.IsWindows())
        {
            WindowNative.DwmSetWindowAttribute(Window.Native.Win32.Value.Hwnd, 20, ref darkMode, sizeof(int));
        }

        Console.WriteLine("Loading");
        // _hitTester = new HitTester(this);
        // _registrationManager = ServiceProvider.GetRequiredService<RegistrationManager>();
        Input = Input.ConstructInputFromWindow(Window, (v) => ScreenToWorld(v));

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

        // Ui.Window = this;
        // Ui.FontManager = new FontManager();
        //
        // RootContainer = new FlexContainer
        // {
        //     Id = new UiID("RootElement", "", 0, 0),
        //     Window = this
        // };
        //
        // _renderer.Initialize(Window);
    }

    public Vector2 Zoom = new(1, 1);
    public Vector2 ZoomOffset = new(0, 0);
    public Vector2 ZoomTarget;

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