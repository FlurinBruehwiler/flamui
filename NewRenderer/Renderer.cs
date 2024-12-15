using System.Drawing;
using System.Reflection;
using System.Text;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace NewRenderer;

public struct Mesh
{
    public float[] Floats;
    public uint[] Indices;
}

public enum Shader
{
    main_fragment,
    main_vertex,
}

public class Renderer
{
    public GL Gl;
    private uint _mainProgram;
    private int _transformLoc;
    private int _stencilEnabledLoc;
    private uint _vao;
    public IWindow Window;

    private Dictionary<Shader, string> _shaderStrings = [];

    public string GetShaderCode(Shader shader)
    {
        if (_shaderStrings.TryGetValue(shader, out var code))
            return code;

        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream($"NewRenderer.Shaders.{shader.ToString()}.glsl");
        using var reader = new StreamReader(stream!, Encoding.UTF8);
        code = reader.ReadToEnd();
        _shaderStrings[shader] = code;
        return code;
    }

    public uint CompileShader(Shader shader, ShaderType shaderType)
    {
        var identifier = Gl.CreateShader(shaderType);
        Gl.ShaderSource(identifier, GetShaderCode(shader));

        Gl.CompileShader(identifier);

        Gl.GetShader(identifier, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(identifier));

        return identifier;
    }

    public void Initialize(IWindow window)
    {
        Window = window;

        Gl = Window.CreateOpenGL();

        Gl.ClearColor(Color.FromArgb(43, 45, 48));

        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Gl.Enable(EnableCap.Blend);

        _vao = Gl.GenVertexArray();
        Gl.BindVertexArray(_vao);

        //main_program
        uint vertexShader = CompileShader(Shader.main_vertex, ShaderType.VertexShader);
        uint fragmentShader = CompileShader(Shader.main_fragment, ShaderType.FragmentShader);

        _mainProgram = CreateProgram(vertexShader, fragmentShader);
        _transformLoc = Gl.GetUniformLocation(_mainProgram, "transform");
        _stencilEnabledLoc = Gl.GetUniformLocation(_mainProgram, "stencil_enabled");

        Gl.BindVertexArray(0);
    }

    private uint CreateProgram(uint vertexShader, uint fragmentShader)
    {
        var program = Gl.CreateProgram();

        Gl.AttachShader(program, vertexShader);
        Gl.AttachShader(program, fragmentShader);

        Gl.LinkProgram(program);

        Gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + Gl.GetProgramInfoLog(program));

        Gl.DetachShader(program, vertexShader);
        Gl.DetachShader(program, fragmentShader);
        Gl.DeleteShader(vertexShader);
        Gl.DeleteShader(fragmentShader);

        return program;
    }

    public unsafe void DrawMesh(Mesh mesh, bool stencilMode = false)
    {
        Gl.BindVertexArray(_vao);
        Gl.UseProgram(_mainProgram);

        //create / bind vbo
        var vbo = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        Gl.BufferData(BufferTargetARB.ArrayBuffer, new ReadOnlySpan<float>(mesh.Floats), BufferUsageARB.StaticDraw);

        //create / bind ebo
        var ebo = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        Gl.BufferData(BufferTargetARB.ElementArrayBuffer, new ReadOnlySpan<uint>(mesh.Indices), BufferUsageARB.StaticDraw);

        const int stride = 3 + 2 + 1 + 4; //10 because of 3 vertices + 2 UVs + 1 filltype + 4 color

        const uint positionLoc = 0; //aPosition in shader
        Gl.EnableVertexAttribArray(positionLoc);
        Gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)0);

        const uint texCoordLoc = 1;
        Gl.EnableVertexAttribArray(texCoordLoc);
        Gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(3 * sizeof(float)));

        const uint bezierTypeLoc = 2;
        Gl.EnableVertexAttribArray(bezierTypeLoc);
        Gl.VertexAttribPointer(bezierTypeLoc, 1, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(5 * sizeof(float)));

        const uint colorLoc = 3;
        Gl.EnableVertexAttribArray(colorLoc);
        Gl.VertexAttribPointer(colorLoc, 4, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(6 * sizeof(float)));

        var matrix = GetWorldToScreenMatrix();

        Gl.ProgramUniformMatrix4(_mainProgram, _transformLoc, false, new ReadOnlySpan<float>(GetAsFloatArray(matrix)));

        if (stencilMode)
            Gl.ProgramUniform1(_mainProgram, _stencilEnabledLoc, 1);
        else
            Gl.ProgramUniform1(_mainProgram, _stencilEnabledLoc, 0);

        Gl.BindVertexArray(_vao);
        Gl.UseProgram(_mainProgram);

        Gl.DrawElements(PrimitiveType.Triangles, (uint)mesh.Indices.Length, DrawElementsType.UnsignedInt,  (void*) 0);

        Gl.DeleteBuffer(ebo);
        Gl.DeleteBuffer(vbo);
    }

    private Matrix4X4<float> GetWorldToScreenMatrix()
    {
        return Matrix4X4.CreateScale(1f / Window.Size.X, 1f / Window.Size.Y, 1) *
            Matrix4X4.CreateScale(2f, 2f, 1) *
            Matrix4X4.CreateTranslation(-1f, -1f, 0) *
            Matrix4X4.CreateScale(1f, -1f, 1f);
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