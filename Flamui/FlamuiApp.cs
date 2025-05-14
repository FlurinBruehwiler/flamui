using System.Reflection;
using Silk.NET.Maths;
using  Silk.NET.Windowing;

namespace Flamui;

public class FlamuiWindowOptions
{
    public int Width { get; set; } = 900;
    public int Height { get; set; } = 500;
    public SizeConstraint? MinSize { get; set; }
    public SizeConstraint? MaxSize { get; set; }
}

public record SizeConstraint(int Width, int Height);

public class FlamuiApp
{
    private List<PhysicalWindow> _windows;

    public FlamuiApp()
    {
        _windows = [];
    }

    public void CreateWindow(string title, Action<Ui> buildFunc, FlamuiWindowOptions? options = null)
    {
        options ??= new FlamuiWindowOptions();

        WindowOptions o = WindowOptions.Default with
        {
            Size = new Vector2D<int>(options.Width, options.Height),
            Title = title,
            VSync = true, //for some reason this doesn't work on my laptop, so we just sleep ourselves
            ShouldSwapAutomatically = false
        };

        var window = Window.Create(o);


        _windows.Add(PhysicalWindow.Create(window, new UiTree(buildFunc)));

        //hack to get paint during resize
        window.GetType().GetField("_onFrame", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(window, new Action(() => UpdateWindow(window)));

        window.Initialize();
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
