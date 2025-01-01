using System.Diagnostics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;

namespace NewRenderer;

public struct TextPos
{
    public int Line;
    public int Char;
}

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
    private static IMouse Mouse;


    public static Font DefaultFont;

    public static void Main()
    {
        // while (!Debugger.IsAttached)
        // {
        //     Thread.Sleep(1000);
        // }

        DefaultFont = FontLoader.LoadFont("JetBrainsMono-Regular.ttf", 20);

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
        canvas.DrawRect(0, 0, _window.Size.X, 1000);

        DrawMultilineSelectableText(canvas, 0, 0, "The quick brown fox\njumps over the lazy dog");
        // canvas.DrawRect(0, 0, Program.DefaultFont.AtlasWidth, Program.DefaultFont.AtlasHeight);

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

    private static void DrawMultilineSelectableText(GlCanvas canvas, float xCoord, float yCoord, ReadOnlySpan<char> text)
    {
        var mousePos = Mouse.Position;

        canvas.Color = Color.FromArgb(187, 189, 190);
        foreach (var line in FontShaping.SplitTextIntoLines(DefaultFont, text, _window.Size.X))
        {
            var lineSpan = text[line];
            canvas.DrawText(lineSpan, xCoord, yCoord);
            var newYCord = yCoord + DefaultFont.Ascent - DefaultFont.Descent + DefaultFont.LineGap;
            if (mousePos.Y > yCoord && mousePos.Y < newYCord)
            {
                var hitIndex = FontShaping.HitTest(DefaultFont, lineSpan, mousePos.X - xCoord);
                if (hitIndex != -1)
                {
                    var (start, end) = FontShaping.GetPositionOfChar(DefaultFont, lineSpan, hitIndex);
                    var tempColor = canvas.Color;
                    canvas.Color = Color.FromArgb(50, 184, 217, 255);
                    canvas.DrawRect(xCoord + start, yCoord, end - start, DefaultFont.Ascent - DefaultFont.Descent);
                    canvas.Color = tempColor;
                }
            }
            yCoord = newYCord;
        }
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

        Mouse = input.Mice.First();


        //opengl setup
        _renderer.Initialize(_window);

        // Console.WriteLine(_gl.GetError());
    }

}