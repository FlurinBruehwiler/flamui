using System.Reflection;
using Silk.NET.Maths;
using  Silk.NET.Windowing;

namespace Flamui;

public struct FlamuiWindowOptions
{
    public int Width = 900;
    public int Height = 500;
    public SizeConstraint? MinSize;
    public SizeConstraint? MaxSize;
    public PhysicalWindow ParentWindow;

    public FlamuiWindowOptions()
    {

    }
}

public record SizeConstraint(int Width, int Height);

public sealed class FlamuiWindowHost
{
    private List<PhysicalWindow> _windows;

    public FlamuiWindowHost()
    {
        _windows = [];
    }

    public PhysicalWindow CreateWindow(string title, Action<Ui> buildFunc, FlamuiWindowOptions? o = null)
    {
        var options = o ?? new FlamuiWindowOptions();

        WindowOptions o2 = WindowOptions.Default with
        {
            Size = new Vector2D<int>(options.Width, options.Height),
            Title = title,
            // VSync = false,
            VSync = true, //for some reason this doesn't work on my laptop, so we just sleep ourselves
            ShouldSwapAutomatically = false,
        };

        var window = Window.Create(o2);

        var physicalWindow = PhysicalWindow.Create(window, new UiTree(buildFunc), options.ParentWindow);
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
