using System.Reflection;
using Flamui.Windowing;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Flamui;

public struct FlamuiWindowOptions
{
    public int Width = 900;
    public int Height = 500;
    public SizeConstraint MinSize = new(400, 300);
    public SizeConstraint MaxSize = new(9000, 9000);
    public PhysicalWindow? ParentWindow;

    public FlamuiWindowOptions()
    {

    }
}

public record struct SizeConstraint(int Width, int Height);

public sealed class FlamuiWindowHost
{
    private List<PhysicalWindow> _windows;

    public FlamuiWindowHost()
    {
        _windows = [];
    }

    public PhysicalWindow CreateWindow(string title, Action<Ui> buildFunc, FlamuiWindowOptions? options = null)
    {
        var o = options ?? new FlamuiWindowOptions(); //needs to be done like this so the field initializers run

        WindowOptions silkWindowOptions = WindowOptions.Default with
        {
            Size = new Vector2D<int>(o.Width, o.Height),
            Title = title,
            // VSync = false,
            VSync = true, //for some reason this doesn't work on my laptop, so we just sleep ourselves
            ShouldSwapAutomatically = false,
        };

        var window = Window.Create(silkWindowOptions);

        var glfwApi = Glfw.GetApi();

        var host = new NativeUiTreeHost(window, glfwApi);

        var physicalWindow = PhysicalWindow.Create(host, new UiTree(host, buildFunc), o);

        _windows.Add(physicalWindow);

        //hack to get paint during resize
        window.GetType().GetField("_onFrame", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(window, new Action(() => UpdateWindow(window)));

        window.Initialize();

        return physicalWindow;
    }

    public void Run()
    {
        while (true)
        {
            for (var i = 0; i < _windows.Count; i++)
            {
                var window = _windows[i].GlfwWindow;

                if (window.IsClosing)
                {
                    window.DoEvents();
                    window.Reset();

                    _windows.RemoveAt(i);
                    i--;
                    continue;
                }

                UpdateWindow(window);
            }

            if(_windows.Count == 0)
                break;
        }
    }

    private void UpdateWindow(IWindow window)
    {
        window.DoEvents();

        if (!window.IsClosing)
        {
            window.DoUpdate();
        }

        if (!window.IsClosing)
        {
            window.DoRender();
        }
    }
}
