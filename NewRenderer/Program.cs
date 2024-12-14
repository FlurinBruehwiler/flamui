using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
using System.Numerics;

namespace NewRenderer;

public struct MeshBuilder
{
    private Vertex[] _vertices;
    private uint _verticesPos;

    public uint[] Indices;
    private int _indicesPos;

    public MeshBuilder(int vertexCount, int trigCount)
    {
        _vertices = new Vertex[vertexCount];
        Indices = new uint[trigCount * 3];
    }

    public uint AddVertex(Vector2 position, Vector2 uv)
    {
        var pos = _verticesPos;

        _verticesPos++;
        _vertices[pos] = new Vertex(position, uv);

        return pos;
    }

    public void AddTriangle(uint v1, uint v2, uint v3)
    {
        Indices[_indicesPos] = v1;
        _indicesPos++;

        Indices[_indicesPos] = v2;
        _indicesPos++;

        Indices[_indicesPos] = v3;
        _indicesPos++;
    }

    public float[] BuildFloatArray()
    {
        const int stride = 5;
        float[] vertexFloats = new float[_vertices.Length * stride];
        for (var i = 0; i < _vertices.Length; i++)
        {
            vertexFloats[i * stride] = _vertices[i].Position.X;
            vertexFloats[i * stride + 1] = _vertices[i].Position.Y;
            vertexFloats[i * stride + 2] = 0;
            vertexFloats[i * stride + 3] = _vertices[i].UV.X;
            vertexFloats[i * stride + 4] = _vertices[i].UV.Y;
        }

        return vertexFloats;
    }
}

public struct Vertex
{
    public Vector2 Position;
    public Vector2 UV;

    public Vertex(Vector2 position, Vector2 uv)
    {
        Position = position;
        UV = uv;
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
    private static GL _gl;
    private static uint _vao; //vertex array object
    private static uint _vbo; //vertex buffer object
    private static uint _ebo; //element  buffer object
    private static uint _program;
    private static uint _eboCount;

    public static void Main()
    {
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "Flamui next :)"
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
        _gl.Viewport(_window.Size);

        _gl.Clear(ClearBufferMask.ColorBufferBit);

        var matrix =
            Matrix4X4.CreateScale(1f / _window.Size.X, 1f / _window.Size.Y, 1) *
            Matrix4X4.CreateScale(2f, 2f, 1) *
            Matrix4X4.CreateTranslation(-1f, -1f, 0) *
            Matrix4X4.CreateScale(1f, -1f, 1f);

        int transformLoc = _gl.GetUniformLocation(_program, "transform");
        _gl.ProgramUniformMatrix4(_program, transformLoc, false, new ReadOnlySpan<float>(GetAsFloatArray(matrix)));

        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_program);
        _gl.DrawElements(PrimitiveType.Triangles, _eboCount, DrawElementsType.UnsignedInt,  (void*) 0);
    }

    private static void OnUpdate(double deltaTime)
    {

    }
    //delaunay triangulation
    //illegal algorithm: https://www.microsoft.com/en-us/research/wp-content/uploads/2005/01/p1000-loop.pdf
    private static unsafe void OnLoad()
    {
        IInputContext input = _window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
            input.Keyboards[i].KeyDown += KeyDown;

        Rect[] rects =
        [
            new(0, 0, 10, 10),
            new(500, 0, 50, 50),
            new(500, 500, 100, 100),
            new(0, 500, 200, 200),
        ];

        BezierCurve[] beziers =
        [
            new(new Vector2(250, 250), new Vector2(300, 250), new Vector2(350, 100))
        ];

        //opengl setup
        _gl = _window.CreateOpenGL();

        _gl.ClearColor(Color.CornflowerBlue);

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        var meshBuilder = new MeshBuilder(rects.Length * 4 + beziers.Length * 3, rects.Length * 6 + beziers.Length * 3);

        for (uint i = 0; i < rects.Length; i++)
        {
            var rect = rects[i];

            uint topLeft = meshBuilder.AddVertex(new Vector2(rect.Pos.X, rect.Pos.Y), new Vector2(0, 0));
            uint topRight = meshBuilder.AddVertex(new Vector2(rect.Pos.X  + rect.Size.X, rect.Pos.Y), new Vector2(1, 0));
            uint bottomRight = meshBuilder.AddVertex(new Vector2(rect.Pos.X + rect.Size.X, rect.Pos.Y + rect.Size.Y), new Vector2(1, 1));
            uint bottomLeft = meshBuilder.AddVertex(new Vector2(rect.Pos.X, rect.Pos.Y + rect.Size.Y), new Vector2(0, 1));

            meshBuilder.AddTriangle(topLeft, topRight, bottomRight);
            meshBuilder.AddTriangle(bottomRight, bottomLeft, topLeft);
        }

        float[] vertexFloats = meshBuilder.BuildFloatArray();
        uint[] indices = meshBuilder.Indices;
        _eboCount = (uint)indices.Length;

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        _gl.BufferData(BufferTargetARB.ArrayBuffer, new ReadOnlySpan<float>(vertexFloats), BufferUsageARB.StaticDraw);

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        _gl.BufferData(BufferTargetARB.ElementArrayBuffer, new ReadOnlySpan<uint>(indices), BufferUsageARB.StaticDraw);

        const string vertexCode =
"""
#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTextureCoord;

uniform mat4 transform;

out vec2 frag_texCoords;

void main()
{
  gl_Position = transform * vec4(aPosition, 1.0);
  frag_texCoords = aTextureCoord;
}
""";

        const string fragmentCode =
"""
#version 330 core

in vec2 frag_texCoords;

out vec4 out_color;

void main()
{
    out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0.0, 1.0);;
}
""";

        //vertex shader compile stuff
        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexCode);

        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));

        //the same for the fragment shader
        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentCode);

        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));

        _program = _gl.CreateProgram();

        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);

        _gl.LinkProgram(_program);

        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));

        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        //pass attributes to vertex shader
        const uint positionLoc = 0; //aPosition in shader
        _gl.EnableVertexAttribArray(positionLoc);                                                      //5 because of 3 vertices + 2 UVs
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);

        const uint texCoordLoc = 1;
        _gl.EnableVertexAttribArray(texCoordLoc);
        _gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    private static float[] GetAsFloatArray(Matrix4X4<float> matrix)
    {
        return
        [
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        ];
    }
}