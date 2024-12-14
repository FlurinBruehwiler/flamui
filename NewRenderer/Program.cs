﻿using System.Diagnostics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using System.Drawing;
using System.Numerics;
using System.Text.Json;

namespace NewRenderer;

public struct MeshBuilder
{
    private List<Vertex> _vertices;

    public List<uint> Indices;

    public MeshBuilder()
    {
        Indices = [];
        _vertices = [];
    }

    public uint AddVertex(Vector2 position, Vector2 uv, Color color, float bezierFillType = 0)
    {
        var pos = _vertices.Count;

        _vertices.Add(new Vertex(position, uv, color)
        {
            BezierFillType = bezierFillType
        });

        return (uint)pos;
    }

    public void AddTriangle(uint v1, uint v2, uint v3)
    {
        Indices.Add(v1);
        Indices.Add(v2);
        Indices.Add(v3);
    }

    public float[] BuildFloatArray()
    {
        const int stride = 3 + 2 + 1 + 4;
        float[] vertexFloats = new float[_vertices.Count * stride];
        for (var i = 0; i < _vertices.Count; i++)
        {
            vertexFloats[i * stride] = _vertices[i].Position.X;
            vertexFloats[i * stride + 1] = _vertices[i].Position.Y;
            vertexFloats[i * stride + 2] = 0;
            vertexFloats[i * stride + 3] = _vertices[i].UV.X;
            vertexFloats[i * stride + 4] = _vertices[i].UV.Y;
            vertexFloats[i * stride + 5] = _vertices[i].BezierFillType;
            vertexFloats[i * stride + 6] = (float)_vertices[i].Color.R / 255;
            vertexFloats[i * stride + 7] = (float)_vertices[i].Color.G / 255;
            vertexFloats[i * stride + 8] = (float)_vertices[i].Color.B / 255;
            vertexFloats[i * stride + 9] = (float)_vertices[i].Color.A / 255;
        }
//b, g,r, a
        return vertexFloats;
    }
}

public struct Vertex
{
    public Vector2 Position;
    public Vector2 UV;
    public float BezierFillType;
    public Color Color;

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
    private static GL _gl;
    private static uint _vao; //vertex array object
    private static uint _vbo; //vertex buffer object
    private static uint _ebo; //element  buffer object
    private static uint _program;
    private static int _transformLoc;

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
        // Console.WriteLine(_gl.GetError());

        var canvas = new GlCanvas();

        canvas.Color = Color.FromArgb(75, 76, 79);
        canvas.DrawRect(100, 100, 100, 100);
        canvas.DrawRoundedRect(300, 300, 100, 100, 10);

        //-----

        float[] vertexFloats = canvas.MeshBuilder.BuildFloatArray();
        uint[] indices = canvas.MeshBuilder.Indices.ToArray();
        var eboCount = (uint)indices.Length;

         _gl.BindVertexArray(_vao);
         _gl.UseProgram(_program);

         // Console.WriteLine(JsonSerializer.Serialize(vertexFloats));
         // Console.WriteLine(JsonSerializer.Serialize(indices));
         //
         // Thread.Sleep(int.MaxValue);

         //create / bind vbo
         _vbo = _gl.GenBuffer();
         _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
         _gl.BufferData(BufferTargetARB.ArrayBuffer, new ReadOnlySpan<float>(vertexFloats), BufferUsageARB.StaticDraw);

         //create / bind ebo
         _ebo = _gl.GenBuffer();
         _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
         _gl.BufferData(BufferTargetARB.ElementArrayBuffer, new ReadOnlySpan<uint>(indices), BufferUsageARB.StaticDraw);

         const int stride = 3 + 2 + 1 + 4;; //10 because of 3 vertices + 2 UVs + 1 filltype + 4 color

         const uint positionLoc = 0; //aPosition in shader
         _gl.EnableVertexAttribArray(positionLoc);
         _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)0);

         const uint texCoordLoc = 1;
         _gl.EnableVertexAttribArray(texCoordLoc);
         _gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(3 * sizeof(float)));

         const uint bezierTypeLoc = 2;
         _gl.EnableVertexAttribArray(bezierTypeLoc);
         _gl.VertexAttribPointer(bezierTypeLoc, 1, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(5 * sizeof(float)));

         const uint colorLoc = 3;
         _gl.EnableVertexAttribArray(colorLoc);
         _gl.VertexAttribPointer(colorLoc, 4, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(6 * sizeof(float)));

         _gl.BindVertexArray(0);
         _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
         _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        //-----

        _gl.Viewport(_window.Size);

        _gl.Clear(ClearBufferMask.ColorBufferBit);

        var matrix =
            Matrix4X4.CreateScale(1f / _window.Size.X, 1f / _window.Size.Y, 1) *
            Matrix4X4.CreateScale(2f, 2f, 1) *
            Matrix4X4.CreateTranslation(-1f, -1f, 0) *
            Matrix4X4.CreateScale(1f, -1f, 1f);

        _gl.ProgramUniformMatrix4(_program, _transformLoc, false, new ReadOnlySpan<float>(GetAsFloatArray(matrix)));

        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_program);

        var error = _gl.GetError();
        // Console.WriteLine(error);

        _gl.DrawElements(PrimitiveType.Triangles, eboCount, DrawElementsType.UnsignedInt,  (void*) 0);

        //--
        _gl.DeleteBuffer(_ebo);
        _gl.DeleteBuffer(_vbo);
        // _gl.DeleteBuffer(_vao);
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
        _gl = _window.CreateOpenGL();

        _gl.ClearColor(Color.FromArgb(43, 45, 48));

        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _gl.Enable(EnableCap.Blend);

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);


        const string vertexCode =
"""
#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTextureCoord;
layout (location = 2) in float aFillBezierType; //0 = disabled fill, >0 = fill inside, <0 = fill outside 
layout (location = 3) in vec4 aColor;

uniform mat4 transform;

out vec2 frag_texCoords;
out float fill_bezier_type;
out vec4 frag_color;

void main()
{
  gl_Position = transform * vec4(aPosition, 1.0);
  frag_texCoords = aTextureCoord;
  fill_bezier_type = aFillBezierType;
  frag_color = aColor;
}
""";

        const string fragmentCode =
"""
#version 330 core

in vec2 frag_texCoords;
in float fill_bezier_type;
in vec4 frag_color;

out vec4 out_color;

void main()
{
    if(fill_bezier_type == 0){
        out_color = frag_color;
    }else{
        float x = frag_texCoords.x;
        float y = frag_texCoords.y;
        
        //anti aliasing: some magic stuff i don't get from this video: https://dl.acm.org/doi/10.1145/1073204.1073303
        float f = x*x-y;
        float dx = dFdx(f);
        float dy = dFdy(f);
        float sd = f/sqrt(dx*dx+dy*dy);
        
        float opacity = 0;
        
        if(sd < -1)
            opacity = 0;
        else if (sd > 1)
            opacity = 1;
        else
            opacity = (1 + sd) / 2;
        
        if(fill_bezier_type > 0){
            opacity = 1 - opacity;
        }

        out_color = vec4(frag_color.r, frag_color.g, frag_color.b, opacity);    
    }
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

        _transformLoc = _gl.GetUniformLocation(_program, "transform");

        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        //pass attributes to vertex shader

        // Console.WriteLine(_gl.GetError());


        _gl.BindVertexArray(0);

        // Console.WriteLine(_gl.GetError());
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