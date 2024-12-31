using System.Diagnostics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
using System.Numerics;
using System.Text.Json;

namespace NewRenderer;

public struct Vertex
{
    public Vector2 Position;
    public Vector2 UV;
    public float BezierFillType;
    public Color Color;
    public TextureType TextureType;

    public Vertex(Vector2 position, Vector2 uv, Color color)
    {
        Position = position;
        UV = uv;
        Color = color;
    }
}

public struct Rect
{
    public Rect(float x, float y, float width, float height)
    {
        Pos = new Vector2(x, y);
        Size = new Vector2(width, height);
    }

    public Vector2 Pos;
    public Vector2 Size;
}

public struct BezierCurve
{
    public BezierCurve(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;
    }

    public Vector2 P1;
    public Vector2 P2;
    public Vector2 P3;
}

public class Program
{
    private static IWindow _window;
    // private static GL _gl;
    // private static uint _vao; //vertex array object
    // private static uint _vbo; //vertex buffer object
    // private static uint _ebo; //element  buffer object
    // private static uint _program;
    // private static int _transformLoc;
    private static readonly Renderer _renderer = new();


    public static Font DefaultFont;

    public static void Main()
    {
        // while (!Debugger.IsAttached)
        // {
        //     Thread.Sleep(1000);
        // }

        DefaultFont = FontLoader.LoadFont("JetBrainsMono-Regular.ttf");

        WindowOptions options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "Flamui next :)",
            Samples = 4
        };

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;

        _window.Run();
    }

    private static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
            _window.Close();
    }

    private static unsafe void OnRender(double deltaTime)
    {
        // Console.WriteLine(_gl.GetError());

        var canvas = new GlCanvas(_renderer);

        canvas.Start();

        canvas.Color = Color.FromArgb(30, 31, 34);
        canvas.DrawRect(0, 0, 1000, 1000);

        canvas.Color = Color.FromArgb(187, 189, 190);
        canvas.DrawText("The quick brown fox jumps over the lazy dog", 50, 50);

        // canvas.DrawRect(0, 0, Program.DefaultFont.AtlasWidth, Program.DefaultFont.AtlasHeight);

        canvas.Color = Color.Red;
        canvas.DrawTriangle(new Vector2(100, 100), new Vector2(150, 200), new Vector2(50, 200));

        canvas.Flush();

        //-----

         // Console.WriteLine(JsonSerializer.Serialize(vertexFloats));
         // Console.WriteLine(JsonSerializer.Serialize(indices));
         //
         // Thread.Sleep(int.MaxValue);

         //--start
         //

         //
         // //----- end
         //
         // gl.BindVertexArray(0);
         // gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
         // gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        //-----
    }

    private static void OnUpdate(double deltaTime)
    {

    }

    //delaunay triangulation
    //illegal algorithm: https://www.microsoft.com/en-us/research/wp-content/uploads/2005/01/p1000-loop.pdf
    //yt: https://www.youtube.com/watch?v=SO83KQuuZvg
    private static void OnLoad()
    {
        IInputContext input = _window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
            input.Keyboards[i].KeyDown += KeyDown;

        //opengl setup
        _renderer.Initialize(_window);

        // Console.WriteLine(_gl.GetError());
    }
}