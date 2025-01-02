using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Flamui.Drawing;

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
    private uint _texture;
    public IWindow Window;
    public static Font DefaultFont;

    private Dictionary<Shader, string> _shaderStrings = [];

    private string GetShaderCode(Shader shader)
    {
        if (_shaderStrings.TryGetValue(shader, out var code))
            return code;

        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream($"Flamui.Drawing.Shaders.{shader.ToString()}.glsl");
        using var reader = new StreamReader(stream!, Encoding.UTF8);
        code = reader.ReadToEnd();
        _shaderStrings[shader] = code;
        return code;
    }

    private uint CompileShader(Shader shader, ShaderType shaderType)
    {
        var identifier = Gl.CreateShader(shaderType);
        Gl.ShaderSource(identifier, GetShaderCode(shader));

        Gl.CompileShader(identifier);

        Gl.GetShader(identifier, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(identifier));

        return identifier;
    }

    //nvidia paper: https://developer.nvidia.com/nv-path-rendering

    public void Initialize(IWindow window)
    {
        Window = window;
        DefaultFont = FontLoader.LoadFont("JetBrainsMono-Regular.ttf", 20);

        Gl = Window.CreateOpenGL();
        Gl.Enable(EnableCap.Multisample);

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

        CheckError();

        UploadTexture(DefaultFont.AtlasBitmap, (uint)DefaultFont.AtlasWidth, (uint)DefaultFont.AtlasHeight);

        CheckError();

        Gl.BindVertexArray(0);

    }

    private void CheckError()
    {
        var err = Gl.GetError();
        Console.WriteLine(err);
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

    public unsafe void UploadTexture(byte[] data, uint width, uint height)
    {
        _texture = Gl.GenTexture();
        Gl.ActiveTexture(TextureUnit.Texture0);
        Gl.BindTexture(TextureTarget.Texture2D, _texture);

        CheckError();

        Debug.Assert(data.Length == width * height);
        fixed (byte* ptr = data)
        {
            Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, ptr);
        }

        Console.WriteLine($"Width:  {width}, Height: {height}");

        CheckError();

        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        Gl.BindTexture(TextureTarget.Texture2D, 0);

        CheckError();

        Gl.UseProgram(_mainProgram);

        int location = Gl.GetUniformLocation(_mainProgram, "uTexture");

        CheckError();

        Gl.Uniform1(location, 0);

        CheckError();
    }

    public unsafe void DrawMesh(Mesh mesh, bool stencilMode = false)
    {
        Gl.BindVertexArray(_vao);
        Gl.UseProgram(_mainProgram);

        Gl.ActiveTexture(TextureUnit.Texture0);
        Gl.BindTexture(TextureTarget.Texture2D, _texture);

        //create / bind vbo
        var vbo = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        Gl.BufferData(BufferTargetARB.ArrayBuffer, new ReadOnlySpan<float>(mesh.Floats), BufferUsageARB.StaticDraw);

        //create / bind ebo
        var ebo = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        Gl.BufferData(BufferTargetARB.ElementArrayBuffer, new ReadOnlySpan<uint>(mesh.Indices), BufferUsageARB.StaticDraw);

        const int stride = 3 + 2 + 1 + 4 + 1; //10 because of 3 vertices + 2 UVs + 1 filltype + 4 color + 1 texturetype

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

        const uint textureTypeLoc = 4;
        Gl.EnableVertexAttribArray(textureTypeLoc);
        Gl.VertexAttribPointer(textureTypeLoc, 1, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(10 * sizeof(float)));

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