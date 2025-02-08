using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.Windowing;


var mat = Matrix4X4.CreateTranslation(100f, 100f, 100f);
var vec = new Vector4D<float>(0, 0, 0, 1);
var res = Vector4D.Multiply(vec, mat);
return;

var window1 = Window.Create(WindowOptions.Default);
var window2 = Window.Create(WindowOptions.Default);

window1.Load += () => { Console.WriteLine("Loading Window1"); };
window2.Load += () => { Console.WriteLine("Loading Window1"); };

window1.Initialize();
window2.Initialize();

List<IWindow> windows = [window1, window2];

while (true)
{
    for (var i = 0; i < windows.Count; i++)
    {
        var window = windows[i];

        if (window.IsClosing)
        {
            window.DoEvents();
            window.Reset();

            windows.Remove(window);
            i--;
            continue;
        }

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

    if(windows.Count == 0)
        break;
}

